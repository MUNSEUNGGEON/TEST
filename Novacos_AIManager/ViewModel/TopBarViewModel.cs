using Novacos_AIManager.Utils;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Novacos_AIManager;

namespace Novacos_AIManager.ViewModel
{
    public class TopBarViewModel : INotifyPropertyChanged
    {
        private readonly NavigationService _navigation;

        private string _dbStatusText = "연동양호";
        public string DbStatusText
        {
            get => _dbStatusText;
            set { _dbStatusText = value; OnPropertyChanged(); }
        }

        private bool _dbConnected = true;
        public bool DbConnected
        {
            get => _dbConnected;
            set { _dbConnected = value; OnPropertyChanged(); }
        }

        private Brush _dbStatusColor = Brushes.LimeGreen;
        public Brush DbStatusColor
        {
            get => _dbStatusColor;
            set { _dbStatusColor = value; OnPropertyChanged(); }
        }

        private bool _isAnalysisEnabled;
        public bool IsAnalysisEnabled
        {
            get => _isAnalysisEnabled;
            set
            {
                _isAnalysisEnabled = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool _areNavigationVisible;
        public bool AreNavigationVisible
        {
            get => _areNavigationVisible;
            set
            {
                _areNavigationVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand ToggleRightPanelCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand GoHomeCommand { get; }
        public ICommand ExitCommand { get; }

        public TopBarViewModel(NavigationService navigation)
        {
            _navigation = navigation;

            ToggleRightPanelCommand = new RelayCommand(_ => TogglePanel(), _ => IsAnalysisEnabled);
            GoBackCommand = new RelayCommand(_ => GoBack(), _ => CanGoBack);
            GoHomeCommand = new RelayCommand(_ => _navigation.GoHome());
            ExitCommand = new RelayCommand(_ => ExitApplication());

            _navigation.NavigationStateChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(CanGoBack));
                CommandManager.InvalidateRequerySuggested();
            };
        }

        public bool CanGoBack => _navigation?.CanGoBack == true;

        // 좌측 패널 열어서 이동
        //private void TogglePanel()
        //{
        //    if (MainWindowViewModel.Instance.RightPanelWidth == "0")
        //        MainWindowViewModel.Instance.RightPanelWidth = "250";  // 패널 열기
        //    else
        //        MainWindowViewModel.Instance.RightPanelWidth = "0";    // 패널 닫기
        //}

        //좌측 패널 열지 않고 바로 이동
        private void TogglePanel()
        {
            if (MainWindowViewModel.Instance.LeftMenuWidth == "0")
            {
                MainWindowViewModel.Instance.LeftMenuWidth = "260";     // 좌측 메뉴 열기
                MainWindowViewModel.Instance.RightPanelWidth = "0";     // 우측 패널은 숨김 유지

                // 기본 화면을 바로 로드하여 추가 클릭 없이 조회 가능하도록 함
                MainWindow.Instance?.OpenEngineVersionFromIcon();
            }
        }

        private void GoBack()
        {
            _navigation.GoBack();
        }

        private void ExitApplication()
        {
            var result = MessageBox.Show(
                "정말 종료하시겠습니까?",
                "프로그램 종료",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}