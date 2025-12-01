using System.Collections.Generic;
using Novacos_AIManager.ViewModel;

namespace Novacos_AIManager.ViewModel.Queries
{
    /// <summary>
    /// 학습데이터 조회 및 학습 모니터링(학습데이터) 화면에서 사용하는 쿼리와 컬럼 매핑을 정의합니다.
    /// </summary>
    public static class AILearningMonitoringViewModel
    {
        private const string BaseSelect = "SELECT * FROM tbl_train_history";

        public static string Monitoring =>
            BaseSelect + " WHERE TTH_STATUS NOT IN ('TRAINING', 'PENDING') ORDER BY TTH_STATUS_REG_TIME DESC";

        public static IReadOnlyList<QueryColumn> Columns { get; } = new List<QueryColumn>
        {
            QueryColumn.RowNumber("Num"),                    // 번호

            QueryColumn.FromSource("TTH_ID",               "Id"),            // 학습 데이터 고유 ID
            QueryColumn.FromSource("TTH_TRT_id",           "TRT_ID"),        // 학습 세션 ID
            QueryColumn.FromSource("TTH_LOSS",             "LOSS"),          // 학습 손실값 (Training Loss)
            QueryColumn.FromSource("TTH_ACCURACY",         "ACCURACY"),      // 학습 정확도 (Training Accuracy)
            QueryColumn.FromSource("TTH_VAL_LOSS",         "VAL_LOSS"),      // 검증 손실값 (Validation Loss)
            QueryColumn.FromSource("TTH_VAL_ACC",        "VALUE_ACC"),     // 검증 정확도 (Validation Accuracy)
            QueryColumn.FromSource("TTH_CUR_EPOCH",    "CUR_EPOCH"),     // 현재 Epoch
            QueryColumn.FromSource("TTH_TOTAL_EPOCH",  "TOTAL_EPOCH"),   // 전체 Epoch
            QueryColumn.FromSource("TTH_CUR_ITER",         "CUR_ITER"),      // 현재 Iteration
            QueryColumn.FromSource("TTH_TOTAL_ITER",       "TOTAL_ITER"),    // 전체 Iteration
            QueryColumn.FromSource("TTH_PRECISION",        "PRECISION"),     // 정밀도 (Precision)
            QueryColumn.FromSource("TTH_RECALL",           "RECALL"),        // 재현율 (Recall)
            QueryColumn.FromSource("TTH_F1SCORE",          "F1SCORE"),       // F1 점수 (F1 Score)
            QueryColumn.FromSource("TTH_MAP",              "mAP"),           // 평균 정밀도 (Mean Average Precision)
        };
    }
}
