namespace Novacos_AIManager.ViewModel.Queries
{
    /// <summary>
    /// 사용자 정보 조회에 사용되는 쿼리를 정의합니다.
    /// </summary>
    public static class AIUserInfoViewModel
    {
        public const string UserList =
            "SELECT " +
            "  tul_id AS 번호, " +
            "  TUL_USER_ID AS 아이디, " +
            "  TUL_USER_NAME AS 이름, " +
            "  TUL_USER_EMAIL AS 이메일, " +
            "  TUL_USER_TYPE AS 권한, " +
            "  TUL_USER_DEPT AS 소속, " +
            "  TUL_USER_POSITION AS 직책, " +
            "  TUL_CREATED_AT AS 등록일 " +
            "FROM tbl_user_list " +
            "ORDER BY tul_id;";
    }
}