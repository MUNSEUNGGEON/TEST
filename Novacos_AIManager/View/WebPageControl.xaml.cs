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
        public WebPageControl()
        {
            InitializeComponent();
            //DataContext = MainWindowViewModel.Instance;
            Loaded += WebPageControl_Loaded;
        }

        private async void WebPageControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as WebPageViewModel;
            if (vm == null)
                return;

            await webView.EnsureCoreWebView2Async();

            webView.Visibility = Visibility.Collapsed;
            // 개발자 도구 열고 싶으면
            // webView.CoreWebView2.OpenDevToolsWindow();

            webView.CoreWebView2.NavigationCompleted += async (s, args) =>
            {
                if (vm.IsLoginMode)
                {
                    // HTML 요소 자동 입력
                    //await webView.ExecuteScriptAsync($"document.querySelector('.id').value = '{vm.LoginId}';");
                    //await webView.ExecuteScriptAsync($"document.querySelector('.password').value = '{vm.LoginPw}';");
                    await webView.ExecuteScriptAsync(@$"let id = document.querySelector('.id'); id.value = '" + vm.LoginId + @"'; id.dispatchEvent(new Event('input',{bubbles: true}));");
                    await webView.ExecuteScriptAsync(@$"let pw = document.querySelector('.password'); pw.value = '" + vm.LoginPw + @"'; pw.dispatchEvent(new Event('input',{bubbles: true}));");
                    //await Task.Delay(1000);
                    await webView.ExecuteScriptAsync($"document.querySelector('.btn-login').click();");
                    //await webView.ExecuteScriptAsync($"document.getElementById('{vm.IdElement}').value = '{vm.LoginId}';");
                    //await webView.ExecuteScriptAsync($"document.getElementById('{vm.PwElement}').value = '{vm.LoginPw}';");
                    //await webView.ExecuteScriptAsync($"document.getElementById('{vm.ButtonElement}').click();");
                    //await webView.ExecuteScriptAsync($"document.querySelector('{vm.ButtonElement}').click();");
                    // 버튼 클릭 대신 login() 함수 직접 실행
                    //await webView.ExecuteScriptAsync("login();");

                    await Task.Delay(1000);

                    //webView.Source = new Uri(vm.StartUrl);
                    webView.Visibility = Visibility.Visible;

                    return;
                }
            };

            // 로그인 후 이동
            // 시작 URL
            webView.Source = new Uri(vm.MainUrl);

            return;
        }

        //        private async void WebPageControl_Loaded(object sender, RoutedEventArgs e)
        //        {
        //            var vm = this.DataContext as WebPageViewModel;
        //            if (vm == null)
        //                return;

        //            // WebView2 초기화
        //            await webView.EnsureCoreWebView2Async();

        //            // 네비게이션 완료 이벤트 등록
        //            webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;

        //            // 시작 URL로 이동
        //            if (!string.IsNullOrWhiteSpace(vm.StartUrl))
        //            {
        //                webView.Source = new Uri(vm.StartUrl);
        //            }
        //        }

        //        private async void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        //        {
        //            var vm = this.DataContext as WebPageViewModel;
        //            if (vm == null)
        //                return;

        //            // 로그인 모드가 아니거나 이미 시도했으면 패스
        //            if (!vm.IsLoginMode || _loginAttempted)
        //                return;

        //            _loginAttempted = true;

        //            // 자동 로그인 스크립트
        //            string script = $@"
        //(function() {{
        //    function findElement(selector) {{
        //        if (!selector) return null;
        //        return document.getElementById(selector)
        //            || document.querySelector(selector)
        //            || document.querySelector('[name=""' + selector + '""]');
        //    }}

        //    function tryClick(el) {{
        //        if (!el) return false;
        //        el.click();
        //        el.dispatchEvent(new MouseEvent('click', {{ bubbles: true, cancelable: true, view: window }}));
        //        return true;
        //    }}

        //    const idEl   = findElement({JsonSerializer.Serialize(vm.IdElement)});
        //    const pwEl   = findElement({JsonSerializer.Serialize(vm.PwElement)});
        //    const btnEl  = findElement({JsonSerializer.Serialize(vm.ButtonElement)});

        //    const idValue = {JsonSerializer.Serialize(vm.LoginId)};
        //    const pwValue = {JsonSerializer.Serialize(vm.LoginPw)};

        //    if (idEl) {{
        //        idEl.focus();
        //        idEl.value = idValue ?? '';
        //        idEl.dispatchEvent(new Event('input', {{ bubbles: true }}));
        //        idEl.dispatchEvent(new Event('change', {{ bubbles: true }}));
        //    }}

        //    if (pwEl) {{
        //        pwEl.focus();
        //        pwEl.value = pwValue ?? '';
        //        pwEl.dispatchEvent(new Event('input', {{ bubbles: true }}));
        //        pwEl.dispatchEvent(new Event('change', {{ bubbles: true }}));
        //    }}

        //    const buttonFallback = btnEl
        //        || (idEl && idEl.form && (idEl.form.querySelector('button[type=""submit""], input[type=""submit""], input[type=""button""], button')))
        //        || document.querySelector('button[type=""submit""], input[type=""submit""], button.login, input[name=""login""]');

        //    if (buttonFallback) {{
        //        tryClick(buttonFallback);
        //    }} else if (idEl && idEl.form) {{
        //        idEl.form.submit();
        //    }}
        //}})();
        //";

        //            try
        //            {
        //                await webView.ExecuteScriptAsync(script);

        //                // 로그인 시도 후 메인 URL로 이동 (조금 대기)
        //                if (!string.IsNullOrWhiteSpace(vm.MainUrl))
        //                {
        //                    await Task.Delay(1000);
        //                    webView.Source = new Uri(vm.MainUrl);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                // 필요하면 로그 찍기
        //                Console.WriteLine(ex);
        //            }
        //        }
    }
}
