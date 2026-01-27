using DataBaseManager;
using Novacos_AIManager.Config;
using Novacos_AIManager.Service;
using Novacos_AIManager.Utils;
using Novacos_AIManager.View;
using Novacos_AIManager.ViewModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Novacos_AIManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileWatcherService fileWatcher; // 파일 실시간 탐색 후 복사
        private AppConfig _config; // 로그인/메인 뷰 설정 값

        // 🔥 MainWindow 전역 인스턴스 (싱글톤)
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            DataContext = new MainWindowViewModel();

            _config = AppConfig.Load();

            string source = _config.FileWatcherSourcePath;     // 감시할 폴더
            string target = _config.FileWatcherTargetPath;     // 이동할 폴더

            fileWatcher = new FileWatcherService(source, target);
            fileWatcher.Start();

            DatabaseConnection();//데이터베이스 접속

            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // ViewSetting == 0 → 로그인 없이 바로 메인 페이지
            if (_config.ViewSetting == 0)
            {
                LoginPanel.Visibility = Visibility.Collapsed;
                LoginPanel.IsHitTestVisible = false;

                var vm = new WebPageViewModel
                {
                    IsLoginMode = false,
                    StartUrl = _config.MainUrl
                };

                MainWindowViewModel.Instance.Navigation.SetHomePage(
                    new WebPageControl { DataContext = vm });

                MainWindowViewModel.Instance.TopBarVM.IsAnalysisEnabled = true;
                MainWindowViewModel.Instance.TopBarVM.AreNavigationVisible = true;
            }
            else
            {
                // ViewSetting == 1 → 로그인 화면 보여주기
                LoginPanel.Visibility = Visibility.Visible;

                // 웹 페이지는 비움
                MainWindowViewModel.Instance.CurrentPage = null;

                MainWindowViewModel.Instance.TopBarVM.IsAnalysisEnabled = false;
                MainWindowViewModel.Instance.TopBarVM.AreNavigationVisible = false;
            }
        }

        private async void OnLoginClicked(object sender, RoutedEventArgs e)
        {
            if (!DatabaseManager.Instance.IsConnected)
            {
                MessageBox.Show("DB 연결 실패", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool userExists = DatabaseManager.Instance.ValidateUser(txtId.Text, txtPw.Password);

            if (!userExists)
            {
                MessageBox.Show("등록되지 않은 계정입니다.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var vm = new WebPageViewModel
            {
                IsLoginMode = true,
                StartUrl = _config.LoginUrl,
                MainUrl = _config.MainUrl,
                LoginId = txtId.Text,
                LoginPw = txtPw.Password,

                IdElement = "username",
                PwElement = "password",
                ButtonElement = "btn-login"
            };

            // 페이지 교체
            MainWindowViewModel.Instance.Navigation.SetHomePage(new WebPageControl { DataContext = vm });

            // 로그인 UI 숨김
            LoginPanel.Visibility = Visibility.Collapsed;

            MainWindowViewModel.Instance.TopBarVM.IsAnalysisEnabled = true;
            MainWindowViewModel.Instance.TopBarVM.AreNavigationVisible = true;
        }

        private void OnSignUpClicked(object sender, RoutedEventArgs e)
        {
            // 기존 회원가입 흐름은 주석 처리합니다.
            //if (!DatabaseManager.Instance.IsConnected)
            //{
            //    MessageBox.Show("DB 연결 실패", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //var dialog = new UserEditDialog
            //{
            //    Owner = this
            //};

            //if (dialog.ShowDialog() != true)
            //{
            //    return;
            //}

            //if (DatabaseManager.Instance.TryAddUser(
            //        dialog.UserId,
            //        dialog.Password,
            //        dialog.UserName,
            //        dialog.Email,
            //        // dialog.num,
            //        dialog.UserType,
            //        dialog.Department,
            //        dialog.Position,
            //        // dialog.date,
            //        out var errorMessage))
            //{
            //    txtId.Text = dialog.UserId;
            //    txtPw.Password = dialog.Password;

            //    txtIdPlaceholder.Visibility = string.IsNullOrEmpty(txtId.Text)
            //        ? Visibility.Visible
            //        : Visibility.Hidden;
            //    txtPwPlaceholder.Visibility = string.IsNullOrEmpty(txtPw.Password)
            //        ? Visibility.Visible
            //        : Visibility.Hidden;

            //    MessageBox.Show("회원가입이 완료되었습니다. 로그인 정보를 확인해주세요.",
            //        "가입 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            //    return;
            //}

            //MessageBox.Show(errorMessage ?? "회원가입에 실패했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);

            var signupUrl = _config.SignupUrl;
            if (string.IsNullOrWhiteSpace(signupUrl))
            {
                MessageBox.Show("회원가입 URL이 설정되지 않았습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = signupUrl,
                UseShellExecute = true
            });
        }

        private void txtId_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtIdPlaceholder.Visibility =
                string.IsNullOrEmpty(txtId.Text) ? Visibility.Visible : Visibility.Hidden;
        }

        private void txtPw_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtPwPlaceholder.Visibility =
                string.IsNullOrEmpty(txtPw.Password) ? Visibility.Visible : Visibility.Hidden;
        }

        private void DatabaseConnection()
        {
            //이거를 view UI에서 실행할지 옮길지 고민
            // 이 줄이 실행되는 순간 DatabaseManager.Instance 생성 + DB 연결 시도 완료
            //bool dbOk = DatabaseManager.Instance.IsConnected;

            //if (!dbOk)
            //{
            //    MessageBox.Show("DB 연결 실패", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            bool ok = DatabaseManager.Instance.IsConnected;

            var top = MainWindowViewModel.Instance.TopBarVM;

            if (ok)
            {
                top.DbConnected = true;
                top.DbStatusText = "연동양호";
                top.DbStatusColor = Brushes.LimeGreen;
            }
            else
            {
                top.DbConnected = false;
                top.DbStatusText = "연동불량";
                top.DbStatusColor = Brushes.Red;
            }
        }

        private void AnalysisEngineIcon_Click(object sender, MouseButtonEventArgs e)
        {
            if (MainWindowViewModel.Instance.LeftMenuWidth == "0")
            {
                MainWindowViewModel.Instance.LeftMenuWidth = "260";     // 좌측 메뉴 열기
                MainWindowViewModel.Instance.RightPanelWidth = "0";   // 오른쪽 패널 숨기기

                OpenEngineVersionFromIcon();
            }
            else
            {
                MainWindowViewModel.Instance.LeftMenuWidth = "0";       // 닫기
            }
        }

        public void OpenEngineVersionFromIcon()
        {
            string folder = $@"{_config.EngVerPath}";

            // 폴더 데이터 로드
            var dataLoader = () => DataLoader.LoadEngineVersionDataFromFolder(folder);
            var data = dataLoader();

            // 페이지 표시
            MainWindowViewModel.Instance.Navigation.NavigateTo(
                new DataManagementPage("분석엔진버전", "Version", data, dataLoader));

        }

    }
}
