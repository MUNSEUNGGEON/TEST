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

    public static class TrainingHistoryQueries
    {
        // 분석 엔진 조회
        private const string AnalysisEngineSelect = "SELECT * FROM tbl_train_history";
        // 학습데이터 조회
        private const string LearningDataSelect = "SELECT * FROM tbl_train_history";

        public static string ForActiveRuns =>
            AnalysisEngineSelect + " WHERE status IN ('TRAINING', 'PENDING') ORDER BY status_reg_time DESC";

        public static string ForCompletedRuns =>
            LearningDataSelect + " WHERE status NOT IN ('TRAINING', 'PENDING') ORDER BY status_reg_time DESC";


        // ==============================
        // 분석엔진(진행/최근) 컬럼 매핑
        // ==============================

        public static IReadOnlyList<QueryColumn> AnalysisColumns { get; } = new List<QueryColumn>
        {
            QueryColumn.RowNumber("Num"),                      // 번호

            QueryColumn.FromSource("id",                "Id"),             // 분석 엔진 고유 ID
            QueryColumn.FromSource("trt_id",            "TRE_ID"),         // 학습/엔진 ID (Training / Engine ID)
            QueryColumn.FromSource("status",            "STATUS"),         // 상태 (진행중, 완료 등)
            QueryColumn.FromSource("loss",              "LOSS"),           // 손실값 (Loss)
            QueryColumn.FromSource("accuracy",          "ACCURACY"),       // 정확도 (Accuracy)
            QueryColumn.FromSource("value_loss",        "VAL_LOSS"),       // 검증 손실값 (Validation Loss)
            QueryColumn.FromSource("tth_cur_epoch",     "CUR_EPOCH"),      // 현재 Epoch
            QueryColumn.FromSource("tth_total_epoch",   "TOTAL_EPOCH"),    // 전체 Epoch
            QueryColumn.FromSource("precision",         "PRECISION"),      // 정밀도 (Precision)
            QueryColumn.FromSource("f1score",           "F1SCORE"),        // F1 점수 (F1 Score)
            QueryColumn.FromSource("map",               "mAP"),            // 평균 정밀도 (Mean Average Precision)
            QueryColumn.FromSource("status_reg_time",   "RESULT_TIME"),    // 결과 기록 시간 (Result Time)
            QueryColumn.FromSource("output_file_path",  "OUTPL"),          // 출력 경로 (Output Path)
        };


        // ==============================
        // 학습데이터(완료 이력) 컬럼 매핑
        // ==============================

        public static IReadOnlyList<QueryColumn> LearningColumns { get; } = new List<QueryColumn>
        {
            QueryColumn.RowNumber("Num"),                    // 번호

            QueryColumn.FromSource("id",               "Id"),            // 학습 데이터 고유 ID
            QueryColumn.FromSource("trt_id",           "TRT_ID"),        // 학습 세션 ID
            QueryColumn.FromSource("loss",             "LOSS"),          // 학습 손실값 (Training Loss)
            QueryColumn.FromSource("accuracy",         "ACCURACY"),      // 학습 정확도 (Training Accuracy)
            QueryColumn.FromSource("val_loss",         "VAL_LOSS"),      // 검증 손실값 (Validation Loss)
            QueryColumn.FromSource("value_acc",        "VALUE_ACC"),     // 검증 정확도 (Validation Accuracy)
            QueryColumn.FromSource("tth_cur_epoch",    "CUR_EPOCH"),     // 현재 Epoch
            QueryColumn.FromSource("tth_total_epoch",  "TOTAL_EPOCH"),   // 전체 Epoch
            QueryColumn.FromSource("cur_iter",         "CUR_ITER"),      // 현재 Iteration
            QueryColumn.FromSource("total_iter",       "TOTAL_ITER"),    // 전체 Iteration
            QueryColumn.FromSource("precision",        "PRECISION"),     // 정밀도 (Precision)
            QueryColumn.FromSource("recall",           "RECALL"),        // 재현율 (Recall)
            QueryColumn.FromSource("f1score",          "F1SCORE"),       // F1 점수 (F1 Score)
            QueryColumn.FromSource("map",              "mAP"),           // 평균 정밀도 (Mean Average Precision)
        };
    }

    public static class UserQueries
    {
        //사용자 정보 조회
        public const string UserList =
            "SELECT tul_id AS Id, TUL_USER_ID AS UserId, TUL_USER_NAME AS UserName, TUL_USER_EMAIL AS Email, " +
            "TUL_USER_TYPE AS UserType, TUL_USER_DEPT AS Department, TUL_USER_POSITION AS Position, TUL_CREATED_AT AS CreatedAt " +
            "FROM tbl_user_list ORDER BY tul_id";
    }

    public sealed class QueryColumn
    {
        public string Header { get; }
        public string? SourceColumn { get; }
        public Type? DataType { get; }

        private readonly Func<int, object?>? _valueFactory;

        private QueryColumn(string header, string? sourceColumn, Func<int, object?>? valueFactory, Type? dataType)
        {
            Header = header;
            SourceColumn = sourceColumn;
            _valueFactory = valueFactory;
            DataType = dataType;
        }

        public static QueryColumn FromSource(string sourceColumn, string? header = null)
        {
            return new QueryColumn(header ?? sourceColumn, sourceColumn, null, null);
        }

        public static QueryColumn RowNumber(string header = "No")
        {
            return new QueryColumn(header, null, index => index + 1, typeof(int));
        }

        public object? GetValue(DataRow row, int rowIndex)
        {
            if (SourceColumn != null)
            {
                return row.Table.Columns.Contains(SourceColumn) ? row[SourceColumn] : null;
            }

            return _valueFactory?.Invoke(rowIndex);
        }
    }
}
