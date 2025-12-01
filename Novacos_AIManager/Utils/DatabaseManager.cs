using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataBaseManager
{
    public class DatabaseManager
    {
        private static readonly Lazy<DatabaseManager> _instance =
            new Lazy<DatabaseManager>(() => new DatabaseManager());

        public static DatabaseManager Instance => _instance.Value;

        private MySqlConnection? _connection;

        private class DbAccount
        {
            public string connectionIp { get; set; }
            public string connectionDB { get; set; }
            public string connectionID { get; set; }
            public string connectionPW { get; set; }

        }

        private readonly Dictionary<string, DbAccount> _accounts =
            new Dictionary<string, DbAccount>();

        public bool IsConnected =>
            _connection != null && _connection.State == ConnectionState.Open;

        private DatabaseManager()
        {
            RegisterAccounts();

            // 자동 연결
            TryAutoConnect();
        }

        private void RegisterAccounts()
        {
            // local 계정
            _accounts["local"] = new DbAccount
            {
                connectionIp = "127.0.0.1",
                connectionDB = "AMIG",
                connectionID = "root",
                connectionPW = "1234"
            };

            // 인텔리빅스 계정
            _accounts["lim"] = new DbAccount
            {
                connectionIp = "192.168.1.150",
                connectionDB = "Lim",
                connectionID = "lim",
                connectionPW = "pass0001!"
            };
        }

        private void TryAutoConnect()
        {
            //계정 변경
            //TryConnectWithKey("local");
            TryConnectWithKey("lim");
        }

        private bool TryConnectWithKey(string key)
        {
            if (!_accounts.ContainsKey(key))
                return false;

            try
            {
                _connection?.Close();

                var acc = _accounts[key];

                string connStr =
                    $"Server={acc.connectionIp};Port=3306;Database={acc.connectionDB};Uid={acc.connectionID};Pwd={acc.connectionPW};";

                _connection = new MySqlConnection(connStr);
                _connection.Open();

                return true;
            }
            catch
            {
                _connection = null;
                return false;
            }
        }

        // 외부에서 원하는 계정으로 연결 변경
        public bool SwitchConnection(string key)
        {
            return TryConnectWithKey(key);
        }

        public MySqlConnection? GetConnection()
        {
            return IsConnected ? _connection : null;
        }

        public bool ValidateUser(string userId, string password)
        {
            if (!IsConnected)
                return false;

            const string query =
                "SELECT COUNT(*) FROM tbl_user_list WHERE TUL_USER_ID = @userId AND TUL_USER_PWD = @password";

            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@password", password);

            try
            {
                var result = cmd.ExecuteScalar();
                if (result == null)
                    return false;

                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        #region --- SELECT ---
        public void ExecuteQuery(string query, Action<MySqlDataReader> readAction)
        {
            if (!IsConnected) return;

            using (var cmd = new MySqlCommand(query, _connection))
            using (var reader = cmd.ExecuteReader())
            {
                readAction(reader);
            }
        }

        public bool TryGetDataTable(string query, out DataTable? table, out string? errorMessage)
        {
            table = null;
            errorMessage = null;

            if (!IsConnected)
            {
                errorMessage = "데이터베이스에 연결되어 있지 않습니다.";
                return false;
            }

            try
            {
                using var cmd = new MySqlCommand(query, _connection);
                using var adapter = new MySqlDataAdapter(cmd);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                table = dataTable;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
        #endregion

        #region --- NON QUERY HELPERS ---
        private bool TryExecuteNonQuery(string query, IDictionary<string, object?> parameters, out string? errorMessage)
        {
            errorMessage = null;

            if (!IsConnected)
            {
                errorMessage = "데이터베이스에 연결되어 있지 않습니다.";
                return false;
            }

            try
            {
                using var cmd = new MySqlCommand(query, _connection);

                foreach (var parameter in parameters)
                {
                    cmd.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                }

                var affectedRows = cmd.ExecuteNonQuery();

                if (affectedRows > 0)
                {
                    return true;
                }

                errorMessage = "변경된 데이터가 없습니다.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

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
            const string query =
                "INSERT INTO tbl_user_list (" +
                "TUL_USER_ID, TUL_USER_PWD, TUL_NAME, TUL_USER_MAIL, TUL_USER_BELONG_CODE, TUL_AUTH_CODE, TUL_POSITION_CODE" +
                ") VALUES (" +
                "@userId, @password, @userName, @userEmail, @userType, @department, @position" +
                ")";

            return TryExecuteNonQuery(query,
                new Dictionary<string, object?>
                {
                    {"@userId", userId },
                    {"@password", password },
                    {"@userName", userName },
                    {"@userEmail", email },
                    {"@userType", userType },
                    {"@department", department },
                    {"@position", position },
                },
                out errorMessage);
        }

        public bool TryDeleteUser(int userId, out string? errorMessage)
        {
            const string query = "DELETE FROM tbl_user_list WHERE tul_id = @id";

            return TryExecuteNonQuery(query,
                new Dictionary<string, object?>
                {
                    {"@id", userId }
                },
                out errorMessage);
        }

        public bool TryUpdateUser(
            int id,
            string? userType,
            string? department,
            string? position,
            out string? errorMessage)
        {
            const string query =
                "UPDATE tbl_user_list SET " +
                "TUL_USER_BELONG_CODE = @userType, " +
                "TUL_POSITION_CODE = @department, " +
                "TUL_AUTH_CODE = @position " +
                "WHERE tul_id = @id";

            return TryExecuteNonQuery(query,
                new Dictionary<string, object?>
                {
                    {"@id", id },
                    {"@userType", userType },
                    {"@department", department },
                    {"@position", position },
                },
                out errorMessage);
        }
        #endregion

        public void Disconnect()
        {
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }
    }
}