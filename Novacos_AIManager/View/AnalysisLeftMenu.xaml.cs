using Novacos_AIManager.Config;
using Novacos_AIManager.Utils;
using Novacos_AIManager.ViewModel;
using Novacos_AIManager.ViewModel.Queries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// AnalysisLeftMenu.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AnalysisLeftMenu : UserControl
    {
        public AppConfig _config { get; set; }
        public AnalysisLeftMenu()
        {
            InitializeComponent();
            _config = AppConfig.Load();
        }

        // ----------------------- 상위 메뉴 토글 -----------------------
        private void ToggleEngineMenu(object sender, MouseButtonEventArgs e)
        {
            EngineSubMenu.Visibility =
                EngineSubMenu.Visibility == Visibility.Visible ?
                Visibility.Collapsed : Visibility.Visible;
        }

        private void ToggleDataMenu(object sender, MouseButtonEventArgs e)
        {
            DataSubMenu.Visibility =
                DataSubMenu.Visibility == Visibility.Visible ?
                Visibility.Collapsed : Visibility.Visible;
        }

        private void ToggleMonitoringMenu(object sender, MouseButtonEventArgs e)
        {
            MonitoringSubMenu.Visibility =
                MonitoringSubMenu.Visibility == Visibility.Visible ?
                Visibility.Collapsed : Visibility.Visible;
        }

        // ----------------------- 하위 메뉴 페이지 전환 -----------------------

        private void OpenEngineVersion(object sender, MouseButtonEventArgs e)
        {
            string folder = $"{_config.EngVerPath}";

            // 폴더에서 파일 목록 자동 로드
            var dataLoader = () => DataLoader.LoadEngineVersionDataFromFolder(folder);
            var data = dataLoader();

            MainWindowViewModel.Instance.Navigation.NavigateTo(
                new DataManagementPage("분석엔진버전", "Version", data, dataLoader));
        }

        private void OpenEngineDeploy(object sender, MouseButtonEventArgs e)
        {
            string folder = $"{_config.EngDeployPath}";

            // 폴더에서 파일 목록 자동 로드
            var dataLoader = () => DataLoader.LoadEngineVersionDataFromFolder(folder);
            var data = dataLoader();

            MainWindowViewModel.Instance.Navigation.NavigateTo(
                new DataManagementPage("분석엔진배포", "DistributionDay", data, dataLoader));
        }


        private void OpenDataVersion(object sender, MouseButtonEventArgs e)
        {
            string folder = $"{_config.LearningVerPath}";

            // 폴더에서 파일 목록 자동 로드
            var dataLoader = () => DataLoader.LoadEngineVersionDataFromFolder(folder);
            var data = dataLoader();

            MainWindowViewModel.Instance.Navigation.NavigateTo(
                new DataManagementPage("학습데이터버전", "Version", data, dataLoader));
        }

        private void OpenDataDeploy(object sender, MouseButtonEventArgs e)
        {
            string folder = $"{_config.LearningDeployPath}";

            // 폴더에서 파일 목록 자동 로드
            var dataLoader = () => DataLoader.LoadEngineVersionDataFromFolder(folder);
            var data = dataLoader();

            MainWindowViewModel.Instance.Navigation.NavigateTo(
                new DataManagementPage("학습데이터배포", "DistributionDay", data, dataLoader));
        }


        private void OpenMonitoringEngine(object sender, MouseButtonEventArgs e)
        {
            MainWindowViewModel.Instance.Navigation.NavigateTo(
                new DatabaseQueryPage(
                    "학습 모니터링 - 분석엔진",
                    AIEngineMonitoringViewModel.Monitoring,
                    "진행 중인 학습 정보가 없습니다.",
                    AIEngineMonitoringViewModel.Columns));
        }

        private void OpenMonitoringData(object sender, MouseButtonEventArgs e)
        {
            MainWindowViewModel.Instance.Navigation.NavigateTo(
                new DatabaseQueryPage(
                    "학습 모니터링 - 학습데이터",
                    AILearningMonitoringViewModel.Monitoring,
                    "완료된 학습 이력이 없습니다.",
                    AILearningMonitoringViewModel.Columns));
        }


        private void OpenUserInfo(object sender, MouseButtonEventArgs e)
        {
            MainWindowViewModel.Instance.Navigation.NavigateTo(
                new DatabaseQueryPage(
                    "사용자 정보",
                    AIUserInfoViewModel.UserList,
                    "등록된 사용자가 없습니다."));
        }
    }
}
