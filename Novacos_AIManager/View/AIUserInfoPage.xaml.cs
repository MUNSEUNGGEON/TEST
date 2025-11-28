using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Novacos_AIManager.ViewModel;

namespace Novacos_AIManager.View
{
    public partial class DatabaseQueryPage : UserControl
    {
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
            if (ViewModel == null)
            {
                return;
            }

            var dialog = new UserEditDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (ViewModel.TryAddUser(
                    dialog.UserId,
                    dialog.Password,
                    dialog.UserName,
                    dialog.Email,
                    dialog.UserType,
                    dialog.Department,
                    dialog.Position,
                    out var errorMessage))
            {
                MessageBox.Show("사용자가 등록되었습니다.", "등록 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show(errorMessage ?? "사용자 등록에 실패했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DeleteUser(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (ResultsGrid.SelectedItem is not DataRowView selectedRow)
            {
                MessageBox.Show("삭제할 사용자를 선택하세요.", "선택 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(Convert.ToString(selectedRow.Row["Id"]), out var userId))
            {
                MessageBox.Show("선택한 사용자 정보를 읽을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confirmation = MessageBox.Show("선택한 사용자를 삭제하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            if (ViewModel.TryDeleteUser(userId, out var errorMessage))
            {
                MessageBox.Show("사용자가 삭제되었습니다.", "삭제 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show(errorMessage ?? "사용자 삭제에 실패했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}

