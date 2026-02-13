using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Novacos_AIManager.View;
using Novacos_AIManager.ViewModel.Queries;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DataBaseManager
{
    public class DatabaseManager
    {
        private static readonly Lazy<DatabaseManager> _instance =
            new Lazy<DatabaseManager>(() => new DatabaseManager());

        public static DatabaseManager Instance => _instance.Value;

        private MySqlConnection? _connection;

        public string? CurrentUserId { get; private set; }
        public string? CurrentUserAuthCode { get; private set; }
        public bool CanManageUserInfo => CurrentUserAuthCode == "1";

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
                connectionDB = "wpf_ui",
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
            TryConnectWithKey("local");
            //TryConnectWithKey("lim");
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
                "SELECT TUL_AUTH_CODE FROM tbl_user_list WHERE TUL_USER_ID = @userId AND TUL_USER_PWD = @password LIMIT 1";

            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@password", password);

            try
            {
                var result = cmd.ExecuteScalar();
                if (result == null)
                {
                    CurrentUserId = null;
                    CurrentUserAuthCode = null;
                    return false;
                }

                var authCode = Convert.ToString(result);
                if (string.IsNullOrWhiteSpace(authCode))
                {
                    CurrentUserId = null;
                    CurrentUserAuthCode = null;
                    return false;
                }

                CurrentUserId = userId;
                CurrentUserAuthCode = authCode;
                return true;
            }
            catch
            {
                CurrentUserId = null;
                CurrentUserAuthCode = null;
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

        public object? ExecuteSalar(string query)
        {
            object? result = null;

            ExecuteQuery(query, reader =>
            {
                if (reader.Read())
                {
                    result = reader.IsDBNull(0) ?
                    null : reader.GetValue(0);
                }
            });
            return result;
        }

        private HttpClient CreateCvatClient(string baseUrl, string username, string password)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);

            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            return client;
        }

        public async Task<int> GetCvatUserIdAsync(string baseUrl, string adminId, string adminPw, string targetUsername)
        {
            using var client = CreateCvatClient(baseUrl, adminId, adminPw);

            HttpResponseMessage response = await client.GetAsync($"/users?search={targetUsername}");
            if (!response.IsSuccessStatusCode)
                return -1;

            string json = await response.Content.ReadAsStringAsync();

            var users = System.Text.Json.JsonSerializer.Deserialize<List<CvatUser>>(json);

            if (users != null && users.Count > 0)
            {
                return users[0].Id;

            }
            return -1;
        }

        public class CvatUser
        {
            public int Id { get; set; }
            public string Username { get; set; }

        }

        public async void CvatTest()
        {
            string baseUrl = "http://192.168.1.150:8080/api";
            string cvatAdminId = "intellivix";
            string cvatAdminPw = "pass0001";

            string usernameToFind = "novacos4";

            int cvatId = await GetCvatUserIdAsync(baseUrl, cvatAdminId, cvatAdminPw, usernameToFind);

            if (cvatId != null)
            {
                Console.WriteLine($"CVAT user  = {cvatId}");

            }
            else
            {
                Console.WriteLine($"CVAT 없");
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
            //int cvatid;
            var now = DateTime.Now;

            string UserList = "SELECT TUL_CVAT_ID FROM tbl_user_list ORDER BY TUL_CVAT_ID DESC LIMIT 1";

            object cvatid2 = ExecuteSalar(UserList);
            int cvatid = Convert.ToInt32(cvatid2) + 1;

            //CvatTest();

            const string query =
                    "INSERT INTO  tbl_user_list (TUL_USER_ID, TUL_NAME, TUL_USER_PWD, TUL_USER_PHONE, TUL_USER_MAIL, TUL_CVAT_ID, TUL_AUTH_CODE, TUL_POSITION_CODE, TUL_USER_BELONG_CODE,  TUL_REG_TIME, TUL_DELETED)" +
                    "VALUES (@userId, @userName, @password, @phone, @userEmail, @cvatid, @department, @position, @userType, @registrationTime, @deleted)";

            return TryExecuteNonQuery(query,
                new Dictionary<string, object?>
                {
                    {"@userId", userId },
                    {"@userName", userName },
                    {"@password", password },
                    {"@phone", null },
                    {"@userEmail", email },
                    {"@cvatid", cvatid},
                    {"@department", userType},
                    {"@position", position },
                    {"@userType", department },
                    {"@registrationTime", now },
                    {"@deleted", 0 }
                },
                out errorMessage);
        }


        public bool TryDeleteUser(int userId, out string? errorMessage)
        {
            //const string query = "DELETE FROM tbl_user_list WHERE tul_id = @id";
            const string query = "UPDATE tbl_user_list set TUL_DELETED = 1 WHERE TUL_ID = @id";

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
                "TUL_USER_BELONG_CODE = @department, " +
                "TUL_POSITION_CODE = @position, " +
                "TUL_AUTH_CODE = @userType " +
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
