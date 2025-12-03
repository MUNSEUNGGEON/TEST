using Novacos_AIManager.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Novacos_AIManager.View
{
    /// <summary>
    /// WebPageControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WebPageControl : UserControl
    {
        private bool _isInitialized = false;        // Loaded 중복 방지용
        private bool _navHandlerRegistered = false; // NavigationCompleted 중복 방지용

        private WebPageViewModel _vm; // 캐싱

        public WebPageControl()
        {
            InitializeComponent();
            Loaded += WebPageControl_Loaded;
        }

        private async void WebPageControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 이미 초기화된 경우 Loaded 재실행 방지
            if (_isInitialized)
                return;

            _isInitialized = true;

            if (_vm == null)
                _vm = this.DataContext as WebPageViewModel;

            var vm = _vm;

            System.Diagnostics.Trace.WriteLine("Loaded 시작: vm.MainUrl = " + vm?.MainUrl);
            if (vm == null)
                return;

            // WebView2 초기화
            await webView.EnsureCoreWebView2Async();
            System.Diagnostics.Trace.WriteLine("EnsureCoreWebView2Async 후: vm.MainUrl = " + vm?.MainUrl);

            webView.Visibility = Visibility.Collapsed;

            // NavigationCompleted → 자동 로그인 처리
            if (!_navHandlerRegistered)
            {
                webView.CoreWebView2.NavigationCompleted += async (s, args) =>
                {
                    // 자동 로그인 모드일 경우
                    if (vm.IsLoginMode)
                    {
                        if (vm.LoginId == "admin" && vm.LoginPw == "amig0618!")
                        {
                            // 값 입력 (+ input 이벤트 발생)
                            await webView.ExecuteScriptAsync($"let id=document.querySelector('.id'); id.value='intellivix'; id.dispatchEvent(new Event('input',{{bubbles:true}}));");
                            await webView.ExecuteScriptAsync($"let pw=document.querySelector('.password'); pw.value='pass0001'; pw.dispatchEvent(new Event('input',{{bubbles:true}}));");
                            // 로그인 버튼 클릭
                            await webView.ExecuteScriptAsync($"document.querySelector('.btn-login').click();");
                        }
                        else
                        {
                            // 값 입력 (+ input 이벤트 발생)
                            await webView.ExecuteScriptAsync($"let id=document.querySelector('.id'); id.value='{vm.LoginId}'; id.dispatchEvent(new Event('input',{{bubbles:true}}));");
                            await webView.ExecuteScriptAsync($"let pw=document.querySelector('.password'); pw.value='{vm.LoginPw}'; pw.dispatchEvent(new Event('input',{{bubbles:true}}));");
                            // 로그인 버튼 클릭
                            await webView.ExecuteScriptAsync($"document.querySelector('.btn-login').click();");
                        }
                        //위에 안되면 아래꺼 사용
                        //await webView.ExecuteScriptAsync(@$"let id = document.querySelector('.id'); id.value = '" + vm.LoginId + @"'; id.dispatchEvent(new Event('input',{bubbles: true}));");
                        //await webView.ExecuteScriptAsync(@$"let pw = document.querySelector('.password'); pw.value = '" + vm.LoginPw + @"'; pw.dispatchEvent(new Event('input',{bubbles: true}));");
                        ////await Task.Delay(1000);
                        //await webView.ExecuteScriptAsync($"document.querySelector('.btn-login').click();");

                        // 페이지 갱신될 시간 확보
                        await Task.Delay(1000);

                        //webView.Visibility = Visibility.Visible;

                        // 자동 로그인은 1회만 수행
                        vm.IsLoginMode = false;
                        //return;
                    }

                    // 로그인 완료 이후 화면 보이기
                    webView.Visibility = Visibility.Visible;
                };

                _navHandlerRegistered = true;
            }

            // URL은 여기서 설정
            if (webView.Source == null)
                webView.Source = new Uri(vm.StartUrl);
            else
                webView.Source = new Uri(vm.MainUrl);
                //webView.Source = new Uri(vm.StartUrl);
        }
    }
}
