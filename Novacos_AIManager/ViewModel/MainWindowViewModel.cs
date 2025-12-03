using Novacos_AIManager.Config;
using Novacos_AIManager.Utils;
using Novacos_AIManager.View;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Novacos_AIManager.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public TopBarViewModel TopBarVM { get; }
        public FooterBarViewModel FooterBarVM { get; }
        public NavigationService Navigation { get; }

        public static MainWindowViewModel Instance { get; private set; }

        public MainWindowViewModel()
        {
            Instance = this;

            Navigation = new NavigationService(this);
            TopBarVM = new TopBarViewModel(Navigation);
            FooterBarVM = new FooterBarViewModel();

            //RightPage = new WebPageControl();

            var config = AppConfig.Load();

            // ✔ WebPageViewModel 만들기
            var webVm = new WebPageViewModel()
            {
                MainUrl = config.MainUrl,
                StartUrl = config.LoginUrl
            };

            // ✔ WebPageControl + DataContext 설정
            var homePage = new WebPageControl()
            {
                DataContext = webVm
            };

            // ✔ NavigationService 에 홈 화면 등록
            Navigation.SetHomePage(homePage);
        }

        private object _currentPage;
        public object CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); }
        }

        private string _rightPanelWidth = "0"; // 기본은 숨김
        public string RightPanelWidth
        {
            get => _rightPanelWidth;
            set
            {
                _rightPanelWidth = value;
                OnPropertyChanged();
            }
        }

        private string _leftMenuWidth = "0";
        public string LeftMenuWidth
        {
            get => _leftMenuWidth;
            set { _leftMenuWidth = value; OnPropertyChanged(); }
        }

        private UserControl _rightPage;
        public UserControl RightPage
        {
            get => _rightPage;
            set { _rightPage = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string prop = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    }
}
