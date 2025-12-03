namespace Novacos_AIManager.ViewModel.Queries
{
    /// <summary>
    /// 사용자 정보 조회에 사용되는 쿼리를 정의합니다.
    /// </summary>
    public static class AIUserInfoViewModel
    {
        public const string UserList =
            "SELECT " +
            "  TUL_ID AS 번호, " +
            "  TUL_NAME AS 이름, " +
            "  TUL_USER_ID AS 아이디, " +
            "  TUL_USER_MAIL AS 이메일, " +
            "  TUL_USER_BELONG_CODE AS 소속, " +
            "  TUL_POSITION_CODE AS 직책, " +
            "  TUL_AUTH_CODE AS 권한, " +
            "  TUL_REG_TIME AS 등록일, " +
            "  TUL_DELETED " +
            "FROM tbl_user_list " +
            "WHERE TUL_DELETED = 0 " +
            "ORDER BY tul_id;";
    }
}