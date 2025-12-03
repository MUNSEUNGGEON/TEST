using System.Collections.Generic;
using Novacos_AIManager.ViewModel;

namespace Novacos_AIManager.ViewModel.Queries
{
    /// <summary>
    /// 분석엔진 조회 및 학습 모니터링(분석엔진) 화면에서 사용하는 쿼리와 컬럼 매핑을 정의합니다.
    /// </summary>
    public static class AIEngineMonitoringViewModel
    {
        private const string BaseSelect = "SELECT * FROM tbl_train_history";

        public static string Monitoring =>
            BaseSelect + " WHERE TTH_STATUS NOT IN ('TRAINING', 'PENDING') ORDER BY TTH_STATUS_REG_TIME DESC";

        public static IReadOnlyList<QueryColumn> Columns { get; } = new List<QueryColumn>
        {
            QueryColumn.RowNumber("Num"),                      // 번호

            QueryColumn.FromSource("TTH_ID",                "Id"),             // 분석 엔진 고유 ID
            QueryColumn.FromSource("TTH_TRT_id",            "TRE_ID"),         // 학습/엔진 ID (Training / Engine ID)
            QueryColumn.FromSource("TTH_STATUS",            "STATUS"),         // 상태 (진행중, 완료 등)
            QueryColumn.FromSource("TTH_LOSS",              "LOSS"),           // 손실값 (Loss)
            QueryColumn.FromSource("TTH_ACCURACY",          "ACCURACY"),       // 정확도 (Accuracy)
            QueryColumn.FromSource("TTH_VAL_LOSS",        "VAL_LOSS"),       // 검증 손실값 (Validation Loss)
            QueryColumn.FromSource("TTH_CUR_EPOCH",     "CUR_EPOCH"),      // 현재 Epoch
            QueryColumn.FromSource("TTH_TOTAL_EPOCH",   "TOTAL_EPOCH"),    // 전체 Epoch
            QueryColumn.FromSource("TTH_PRECISION",         "PRECISION"),      // 정밀도 (Precision)
            QueryColumn.FromSource("TTH_F1SCORE",           "F1SCORE"),        // F1 점수 (F1 Score)
            QueryColumn.FromSource("TTH_MAP",               "mAP"),            // 평균 정밀도 (Mean Average Precision)
            QueryColumn.FromSource("TTH_STATUS_REG_TIME",   "RESULT_TIME"),    // 결과 기록 시간 (Result Time)
            QueryColumn.FromSource("TTH_OUTPUT_FILE_PATH",  "OUTPL"),          // 출력 경로 (Output Path)
        };
    }
}