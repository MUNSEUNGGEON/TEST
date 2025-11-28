namespace Novacos_AIManager.ViewModel.Queries
{
    /// <summary>
    /// 사용자 정보 조회에 사용되는 쿼리를 정의합니다.
    /// </summary>
    public static class UserQueries
    {
        public const string UserList =
            "SELECT tul_id AS Id, TUL_USER_ID AS UserId, TUL_USER_NAME AS UserName, TUL_USER_EMAIL AS Email, " +
            "TUL_USER_TYPE AS UserType, TUL_USER_DEPT AS Department, TUL_USER_POSITION AS Position, TUL_CREATED_AT AS CreatedAt" +
            "FROM tbl_user_list ORDER BY tul_id";
    }
}
