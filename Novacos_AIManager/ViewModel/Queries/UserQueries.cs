namespace Novacos_AIManager.ViewModel.Queries
{
    /// <summary>
    /// 사용자 정보 조회에 사용되는 쿼리를 정의합니다.
    /// </summary>
    public static class UserQueries
    {
        public const string UserList =
            "SELECT tul_id AS 번호, TUL_USER_ID AS 사용자아이디, TUL_USER_NAME AS 사용자명, TUL_USER_EMAIL AS 이메일, " +
            "TUL_USER_TYPE AS 사용자유형, TUL_USER_DEPT AS 부서, TUL_USER_POSITION AS 직급, TUL_CREATED_AT AS 등록일 " +
            "FROM tbl_user_list ORDER BY tul_id";
    }
}
