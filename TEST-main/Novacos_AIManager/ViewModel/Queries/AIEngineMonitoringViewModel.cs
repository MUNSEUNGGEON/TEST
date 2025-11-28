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
            BaseSelect + " WHERE status IN ('TRAINING', 'PENDING') ORDER BY status_reg_time DESC";

        public static IReadOnlyList<QueryColumn> Columns { get; } = new List<QueryColumn>
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
    }
}
