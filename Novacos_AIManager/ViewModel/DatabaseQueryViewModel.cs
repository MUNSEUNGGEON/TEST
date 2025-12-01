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

        private static readonly IReadOnlyDictionary<string, string> UserTypeCodeToLabel =
            new Dictionary<string, string>
            {
                { "1", "매니저" },
                { "2", "현장관리자" },
                { "3", "작업자" },
            };

        private static readonly IReadOnlyDictionary<string, string> DepartmentCodeToLabel =
            new Dictionary<string, string>
            {
                { "100", "연구소" },
                { "101", "글로벌사업부" },
                { "102", "융합사업부" },
                { "103", "공공사업부" },
                { "104", "기타" },
            };

        private static readonly IReadOnlyDictionary<string, string> PositionCodeToLabel =
            new Dictionary<string, string>
            {
                { "105", "연구원" },
                { "106", "과장" },
                { "107", "차장" },
                { "108", "상무" },
            };

        private static readonly IReadOnlyDictionary<string, string> UserTypeLabelToCode =
            UserTypeCodeToLabel.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        private static readonly IReadOnlyDictionary<string, string> DepartmentLabelToCode =
            DepartmentCodeToLabel.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        private static readonly IReadOnlyDictionary<string, string> PositionLabelToCode =
            PositionCodeToLabel.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        private static readonly string[] DefaultUserTypes = UserTypeCodeToLabel.Values.ToArray();
        private static readonly string[] DefaultDepartments = DepartmentCodeToLabel.Values.ToArray();
        private static readonly string[] DefaultPositions = PositionCodeToLabel.Values.ToArray();
        private static readonly string[] DefaultUserTypes = { "1", "2", "3" };
        private static readonly string[] DefaultDepartments = { "100", "101", "102", "103", "104" };
        private static readonly string[] DefaultPositions = { "105", "106", "107", "108" };

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

        private void LoadUserData()
        {
            if (!DatabaseManager.Instance.IsConnected)
            {
                UserItems = null;
                ResetUserOptionCollections();
                StatusMessage = "데이터베이스 연결에 실패했습니다.";
                return;
            }

            if (DatabaseManager.Instance.TryGetDataTable(_query, out var table, out var errorMessage))
            {
                if (table == null || table.Rows.Count == 0)
                {
                    UserItems = null;
                    ResetUserOptionCollections();
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
                        UserType = MapCodeToLabel(GetString(row, "권한"), UserTypeCodeToLabel),
                        Department = MapCodeToLabel(GetString(row, "소속"), DepartmentCodeToLabel),
                        Position = MapCodeToLabel(GetString(row, "직책"), PositionCodeToLabel),
                        CreatedAt = GetString(row, "등록일"),
                    });
                }

                UserItems = items;
                SelectedUser = null;
                UpdateUserOptionCollections(items);
                StatusMessage = $"총 {items.Count}건의 데이터를 조회했습니다.";
                return;
            }

            UserItems = null;
            ResetUserOptionCollections();
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

            var userTypeCode = MapLabelToCode(user.EditUserType, UserTypeLabelToCode, UserTypeCodeToLabel);
            var departmentCode = MapLabelToCode(user.EditDepartment, DepartmentLabelToCode, DepartmentCodeToLabel);
            var positionCode = MapLabelToCode(user.EditPosition, PositionLabelToCode, PositionCodeToLabel);

            var success = DatabaseManager.Instance.TryUpdateUser(
                user.Id,
                userTypeCode,
                departmentCode,
                positionCode,
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

        private void ResetUserOptionCollections()
        {
            UserTypeOptions.Clear();
            DepartmentOptions.Clear();
            PositionOptions.Clear();

            AddOptions(UserTypeOptions, DefaultUserTypes);
            AddOptions(DepartmentOptions, DefaultDepartments);
            AddOptions(PositionOptions, DefaultPositions);
        }

        private void UpdateUserOptionCollections(IEnumerable<UserInfoModel> users)
        {
            ResetUserOptionCollections();

            foreach (var user in users)
            {
                AddDistinctOption(UserTypeOptions, user.UserType);
                AddDistinctOption(DepartmentOptions, user.Department);
                AddDistinctOption(PositionOptions, user.Position);
            }
        }

        private static void AddDistinctOption(ObservableCollection<string> target, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !target.Contains(value))
            {
                target.Add(value);
            }
        }

        private static void AddOptions(ObservableCollection<string> target, IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                AddDistinctOption(target, value);
            }
        }

        private static string MapCodeToLabel(string? value, IReadOnlyDictionary<string, string> map)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value ?? string.Empty;
            }

            return map.TryGetValue(value, out var label) ? label : value;
        }

        private static string MapLabelToCode(
            string? value,
            IReadOnlyDictionary<string, string> labelToCode,
            IReadOnlyDictionary<string, string> codeToLabel)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (labelToCode.TryGetValue(value, out var code))
            {
                return code;
            }

            return codeToLabel.ContainsKey(value) ? value : value;
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
