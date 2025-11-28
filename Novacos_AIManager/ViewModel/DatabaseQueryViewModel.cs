using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Controls.Primitives;
using DataBaseManager;
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

        public RelayCommand RefreshCommand { get; }

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

            LoadData();
        }

        private void LoadData()
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
