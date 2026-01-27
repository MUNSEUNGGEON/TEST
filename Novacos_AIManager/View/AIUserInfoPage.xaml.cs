using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using Novacos_AIManager.Config;
using Novacos_AIManager.Model;
using Novacos_AIManager.ViewModel;

namespace Novacos_AIManager.View
{
    public partial class DatabaseQueryPage : UserControl
    {
        private bool _isEditRequested;
        public DatabaseQueryPage(
            string title,
            string query,
            string? emptyMessage = null,
            IReadOnlyList<QueryColumn>? columnMapping = null)
        {
            InitializeComponent();
            DataContext = new DatabaseQueryViewModel(title, query, emptyMessage, columnMapping);
        }

        private DatabaseQueryViewModel? ViewModel => DataContext as DatabaseQueryViewModel;

        private void RegisterUser(object sender, RoutedEventArgs e)
        {
            // 기존 사용자 등록 흐름은 주석 처리합니다.
            //if (ViewModel == null)
            //{
            //    return;
            //}

            //var dialog = new UserEditDialog
            //{
            //    Owner = Window.GetWindow(this)
            //};

            //if (dialog.ShowDialog() != true)
            //{
            //    return;
            //}

            //if (ViewModel.TryAddUser(
            //        dialog.UserId,
            //        dialog.Password,
            //        dialog.UserName,
            //        dialog.Email,
            //        dialog.UserType,
            //        dialog.Department,
            //        dialog.Position,
            //        out var errorMessage))
            //{
            //    MessageBox.Show("사용자가 등록되었습니다.", "등록 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            //    return;
            //}

            //MessageBox.Show(errorMessage ?? "사용자 등록에 실패했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);

            var config = AppConfig.Load();
            var signupUrl = config.SignupUrl;
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

        private void DeleteUser(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (!ViewModel.IsUserInfoPage)
            {
                return;
            }

            var selectedUser = ViewModel.SelectedUser;

            if (selectedUser == null)
            {
                MessageBox.Show("삭제할 사용자를 선택하세요.", "선택 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show("선택한 사용자를 삭제하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            if (ViewModel.TryDeleteUser(selectedUser.Id, out var errorMessage))
            {
                MessageBox.Show("사용자가 삭제되었습니다.", "삭제 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show(errorMessage ?? "사용자 삭제에 실패했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnUserGridDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel?.SelectedUser == null)
            {
                return;
            }

            ViewModel.BeginEdit(ViewModel.SelectedUser);
            _isEditRequested = true;
            UserResultsGrid.BeginEdit();
        }

        private void OnUserGridBeginningEditt(object sender, DataGridBeginningEditEventArgs e)
        {
            if (_isEditRequested)
            {
                _isEditRequested = false;
                return;
            }

            e.Cancel = true;
                }
        private void OnUserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            foreach (var removed in e.RemovedItems.OfType<UserInfoModel>())
            {
                if (removed.IsEditing)
                {
                    ViewModel.CancelEdit(removed);
                }
            }
        }
    }

}
