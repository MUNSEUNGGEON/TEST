using System.Collections.Generic;
using Novacos_AIManager.ViewModel;

namespace Novacos_AIManager.ViewModel.Queries
{
    /// <summary>
    /// 학습데이터 조회 및 학습 모니터링(학습데이터) 화면에서 사용하는 쿼리와 컬럼 매핑을 정의합니다.
    /// </summary>
    public static class LearningDataQueries
    {
        private const string BaseSelect = "SELECT * FROM tbl_train_history";

        public static string Monitoring =>
            BaseSelect + " WHERE status NOT IN ('TRAINING', 'PENDING') ORDER BY status_reg_time DESC";

        public static IReadOnlyList<QueryColumn> Columns { get; } = new List<QueryColumn>
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
}
