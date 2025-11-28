using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DataBaseManager;
using Novacos_AIManager.Model;
using Novacos_AIManager.Utils;

namespace Novacos_AIManager.ViewModel
{
    public class DatabaseQueryViewModel : INotifyPropertyChanged
    {
        private readonly string _query;
        private readonly string? _emptyMessage;
        private readonly IReadOnlyList<QueryColumn>? _columnMapping;

        private DataView? _results;
        public DataView? Results
        {
            get => _results;
            private set
            {
                _results = value;
                OnPropertyChanged(nameof(Results));
            }
        }

        private ObservableCollection<UserInfoModel>? _userItems;
        public ObservableCollection<UserInfoModel>? UserItems
        {
            get => _userItems;
            private set
            {
                _userItems = value;
                OnPropertyChanged(nameof(UserItems));
            }
        }

        private UserInfoModel? _selectedUser;
        public UserInfoModel? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<string> UserTypeOptions { get; } = new();
        public ObservableCollection<string> DepartmentOptions { get; } = new();
        public ObservableCollection<string> PositionOptions { get; } = new();

        private static readonly string[] DefaultUserTypes = { "관리자", "사용자" };
        private static readonly string[] DefaultDepartments = { "연구소", "개발팀" };
        private static readonly string[] DefaultPositions = { "수석연구원", "책임연구원", "선임연구원", "연구원" };

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public string Title { get; }

        public bool IsUserInfoPage => Title == "사용자 정보";
        public bool IsNotUserInfoPage => !IsUserInfoPage;

        public RelayCommand RefreshCommand { get; }
        public RelayCommand SaveUserCommand { get; }

        public bool TryAddUser(
            string userId,
            string password,
            string userName,
            string email,
            string userType,
            string department,
            string position,
            out string? errorMessage)
        {
            if (!IsUserInfoPage)
            {
                errorMessage = "사용자 정보 페이지에서만 등록할 수 있습니다.";
                return false;
            }

            var success = DatabaseManager.Instance.TryAddUser(
                userId,
                password,
                userName,
                email,
                userType,
                department,
                position,
                out errorMessage);

            if (success)
            {
                LoadData();
                StatusMessage = "사용자가 등록되었습니다.";
            }

            return success;
        }

        public bool TryDeleteUser(int userId, out string? errorMessage)
        {
            if (!IsUserInfoPage)
            {
                errorMessage = "사용자 정보 페이지에서만 삭제할 수 있습니다.";
                return false;
            }

            var success = DatabaseManager.Instance.TryDeleteUser(userId, out errorMessage);

            if (success)
            {
                LoadData();
                StatusMessage = "사용자가 삭제되었습니다.";
            }

            return success;
        }

        public DatabaseQueryViewModel(
            string title,
            string query,
            string? emptyMessage = null,
            IReadOnlyList<QueryColumn>? columnMapping = null)
        {
            Title = title;
            _query = query;
            _emptyMessage = emptyMessage;
            _columnMapping = columnMapping;

            RefreshCommand = new RelayCommand(_ => LoadData());
            SaveUserCommand = new RelayCommand(_ => SaveEditingUser(), _ => CanSaveUser());

            InitializeDropdownOptions();

            LoadData();
        }

        private void LoadData()
        {
            if (IsUserInfoPage)
            {
                LoadUserData();
                return;
            }

            LoadGeneralData();
        }

        private void LoadGeneralData()
        {
            if (!DatabaseManager.Instance.IsConnected)
            {
                Results = null;
                StatusMessage = "데이터베이스 연결에 실패했습니다.";
                return;
            }

            if (DatabaseManager.Instance.TryGetDataTable(_query, out var table, out var errorMessage))
            {
                if (table == null || table.Rows.Count == 0)
                {
                    Results = null;
                    StatusMessage = _emptyMessage ?? "조회된 데이터가 없습니다.";
                    return;
                }

                if (_columnMapping != null)
                {
                    table = FilterColumns(table, _columnMapping);
                }

                Results = table.DefaultView;
                StatusMessage = $"총 {table.Rows.Count}건의 데이터를 조회했습니다.";
                return;
            }

            Results = null;
            StatusMessage = errorMessage ?? "알 수 없는 오류가 발생했습니다.";
        }

        private void InitializeDropdownOptions()
        {
            ResetCollection(UserTypeOptions, DefaultUserTypes);
            ResetCollection(DepartmentOptions, DefaultDepartments);
            ResetCollection(PositionOptions, DefaultPositions);
        }

        private void LoadUserData()
        {
            if (!DatabaseManager.Instance.IsConnected)
            {
                UserItems = null;
                StatusMessage = "데이터베이스 연결에 실패했습니다.";
                return;
            }

            if (DatabaseManager.Instance.TryGetDataTable(_query, out var table, out var errorMessage))
            {
                if (table == null || table.Rows.Count == 0)
                {
                    UserItems = null;
                    StatusMessage = _emptyMessage ?? "조회된 데이터가 없습니다.";
                    return;
                }

                var items = new ObservableCollection<UserInfoModel>();

                foreach (DataRow row in table.Rows)
                {
                    items.Add(new UserInfoModel
                    {
                        Id = GetInt(row, "번호"),
                        UserId = GetString(row, "아이디"),
                        UserName = GetString(row, "이름"),
                        Email = GetString(row, "이메일"),
                        UserType = GetString(row, "권한"),
                        Department = GetString(row, "소속"),
                        Position = GetString(row, "직책"),
                        CreatedAt = GetString(row, "등록일"),
                    });
                }

                UserItems = items;
                SelectedUser = null;
                UpdateUserTypeOptions(items);
                UpdateDepartmentOptions(items);
                UpdatePositionOptions(items);
                StatusMessage = $"총 {items.Count}건의 데이터를 조회했습니다.";
                return;
            }

            UserItems = null;
            StatusMessage = errorMessage ?? "알 수 없는 오류가 발생했습니다.";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void BeginEdit(UserInfoModel user)
        {
            if (UserItems != null)
            {
                foreach (var item in UserItems)
                {
                    if (item != user && item.IsEditing)
                    {
                        item.CancelEdit();
                    }
                }
            }

            SelectedUser = user;
            user.BeginEdit();
            CommandManager.InvalidateRequerySuggested();
        }

        public void CancelEdit(UserInfoModel user)
        {
            user.CancelEdit();
            CommandManager.InvalidateRequerySuggested();
        }

        private void SaveEditingUser()
        {
            if (SelectedUser == null)
            {
                StatusMessage = "편집할 사용자를 선택하세요.";
                MessageBox.Show(StatusMessage, "선택 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TryUpdateUser(SelectedUser, out var errorMessage))
            {
                StatusMessage = "사용자 정보가 수정되었습니다.";
                var selectedId = SelectedUser.Id;
                LoadUserData();
                SelectedUser = UserItems?.FirstOrDefault(u => u.Id == selectedId);
                MessageBox.Show(StatusMessage, "수정 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StatusMessage = errorMessage ?? "사용자 정보 수정에 실패했습니다.";
            MessageBox.Show(StatusMessage, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool TryUpdateUser(UserInfoModel user, out string? errorMessage)
        {
            if (!IsUserInfoPage)
            {
                errorMessage = "사용자 정보 페이지에서만 수정할 수 있습니다.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(user.EditDepartment))
            {
                errorMessage = "소속은 필수 입력 값입니다.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(user.EditPosition))
            {
                errorMessage = "직책은 필수 입력 값입니다.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(user.EditUserType))
            {
                errorMessage = "권한은 필수 입력 값입니다.";
                return false;
            }

            var success = DatabaseManager.Instance.TryUpdateUser(
                user.Id,
                user.EditUserType,
                user.EditDepartment,
                user.EditPosition,
                out errorMessage);

            if (success)
            {
                user.CommitEdit();
                return true;
            }

            user.CancelEdit();
            return false;
        }

        private bool CanSaveUser()
        {
            return IsUserInfoPage && SelectedUser?.IsEditing == true;
        }

        private static string? GetString(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) ? Convert.ToString(row[columnName]) : null;
        }

        private static int GetInt(DataRow row, string columnName)
        {
            var value = GetString(row, columnName);
            return int.TryParse(value, out var result) ? result : 0;
        }

        private void UpdateUserTypeOptions(IEnumerable<UserInfoModel> users)
        {
            ResetCollection(UserTypeOptions, DefaultUserTypes);

            foreach (var type in users)
            {
                if (!string.IsNullOrWhiteSpace(type.UserType) && !UserTypeOptions.Contains(type.UserType))
                {
                    UserTypeOptions.Add(type.UserType);
                }
            }
        }

        private void UpdateDepartmentOptions(IEnumerable<UserInfoModel> users)
        {
            ResetCollection(DepartmentOptions, DefaultDepartments);

            foreach (var department in users)
            {
                if (!string.IsNullOrWhiteSpace(department.Department) && !DepartmentOptions.Contains(department.Department))
                {
                    DepartmentOptions.Add(department.Department);
                }
            }
        }

        private void UpdatePositionOptions(IEnumerable<UserInfoModel> users)
        {
            ResetCollection(PositionOptions, DefaultPositions);

            foreach (var position in users)
            {
                if (!string.IsNullOrWhiteSpace(position.Position) && !PositionOptions.Contains(position.Position))
                {
                    PositionOptions.Add(position.Position);
                }
            }
        }

        private static void ResetCollection(ObservableCollection<string> target, IEnumerable<string> values)
        {
            target.Clear();

            foreach (var value in values)
            {
                target.Add(value);
            }
        }

        private static DataTable FilterColumns(DataTable source, IReadOnlyList<QueryColumn> columnMapping)
        {
            var table = new DataTable();

            foreach (var column in columnMapping)
            {
                var dataType = column.SourceColumn != null && source.Columns.Contains(column.SourceColumn)
                    ? source.Columns[column.SourceColumn].DataType
                    : column.DataType ?? typeof(object);

                table.Columns.Add(column.Header, dataType);
            }

            var rowIndex = 0;
            foreach (DataRow sourceRow in source.Rows)
            {
                var targetRow = table.NewRow();

                for (var i = 0; i < columnMapping.Count; i++)
                {
                    var column = columnMapping[i];
                    targetRow[i] = column.GetValue(sourceRow, rowIndex) ?? DBNull.Value;
                }

                table.Rows.Add(targetRow);
                rowIndex++;
            }

            return table;
        }
    }
}
