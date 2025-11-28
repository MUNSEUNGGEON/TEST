using Novacos_AIManager.ViewModel;
using System;
using System.Collections.Generic;

namespace Novacos_AIManager.Utils
{
    /// <summary>
    /// 간단한 내비게이션 히스토리 관리 서비스.
    /// 현재 화면을 변경할 때 스택에 이전 화면을 쌓고, 뒤로가기/홈 이동 기능을 제공합니다.
    /// </summary>
    public class NavigationService
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly Stack<object> _backStack = new();

        public event EventHandler NavigationStateChanged;

        public NavigationService(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public object HomePage { get; private set; }

        public bool CanGoBack => _backStack.Count > 0;

        /// <summary>
        /// 홈 화면을 설정하고 바로 이동합니다. 이전 히스토리는 초기화됩니다.
        /// </summary>
        public void SetHomePage(object homePage)
        {
            HomePage = homePage;
            _backStack.Clear();
            _viewModel.CurrentPage = homePage;
            OnNavigationStateChanged();
        }

        /// <summary>
        /// 새 화면으로 이동합니다. 기존 화면을 히스토리에 저장합니다.
        /// </summary>
        public void NavigateTo(object nextPage)
        {
            if (_viewModel.CurrentPage != null)
            {
                _backStack.Push(_viewModel.CurrentPage);
            }

            _viewModel.CurrentPage = nextPage;
            OnNavigationStateChanged();
        }

        /// <summary>
        /// 직전 화면으로 되돌아갑니다.
        /// </summary>
        public void GoBack()
        {
            if (!CanGoBack)
            {
                return;
            }

            _viewModel.CurrentPage = _backStack.Pop();
            OnNavigationStateChanged();
        }

        /// <summary>
        /// 홈 화면으로 이동합니다. 히스토리를 모두 비웁니다.
        /// </summary>
        public void GoHome()
        {
            if (HomePage == null)
            {
                return;
            }

            // 홈 화면으로 이동할 때 열려 있는 좌측 메뉴를 닫는다.
            _viewModel.LeftMenuWidth = "0";

            _backStack.Clear();
            _viewModel.CurrentPage = HomePage;
            OnNavigationStateChanged();
        }

        private void OnNavigationStateChanged()
        {
            NavigationStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
