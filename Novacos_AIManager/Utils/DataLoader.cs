using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression; // ← 지금은 사용 안 하지만, 혹시 나중에 ZIP 처리 다시 넣을 수도 있으니 일단 유지
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Novacos_AIManager.Utils
{
    /// <summary>
    /// 엔진 버전 정보 로딩 유틸리티
    /// - 폴더 내 파일명을 읽어와 DataGrid에 바인딩 가능한 컬렉션으로 변환
    /// - 파일은 "열지 않고", 파일명 문자열만 사용해서 날짜/코어이름/카메라타입을 추출
    /// - 기본 파일명 규칙: YYYYMMDD_Version_CoreName_CameraType
    ///   (예: 20251124_v1_EngineDistribution_EO)
    /// </summary>
    public static class DataLoader
    {
        /// <summary>
        /// 키워드 없이 폴더 전체 파일을 로드하는 기본 API.
        /// </summary>
        public static ObservableCollection<dynamic> LoadEngineVersionDataFromFolder(string folderPath)
        {
            // 내부 공용 함수에 keyword=null로 전달
            return LoadEngineVersionDataFromFolder(folderPath, null);
        }

        /// <summary>
        /// 폴더에서 파일 목록을 읽어와
        /// - Num (순번)
        /// - FileName (코어 이름: 날짜/버전/카메라타입 제거된 이름)
        /// - Column2 (버전: 파일명 두 번째 토큰)
        /// - CameraType (카메라 타입: 파일명 마지막 토큰)
        /// - Date (필터용 날짜: 파일명 첫 번째 토큰 또는 수정일)
        /// 으로 구성된 컬렉션을 반환합니다.
        /// 
        /// 파일을 실제로 열지 않고, 파일명과 파일 수정일 정보만 사용합니다.
        /// 파일명 기본 규칙:
        ///   YYYYMMDD_Version_CoreName_CameraType
        /// 예)
        ///   20251124_v1_EngineDistribution_EO
        ///   20251125_v2_EngineVersion_MWIR
        /// 
        /// keyword가 주어지면 파일명에 keyword가 포함된 파일만 필터링합니다(대소문자 무시).
        /// </summary>
        public static ObservableCollection<dynamic> LoadEngineVersionDataFromFolder(string folderPath, string? keyword)
        {
            var list = new ObservableCollection<dynamic>();

            // 폴더 경로가 비었거나 존재하지 않으면 빈 리스트 반환
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return list;

            int num = 1;

            // 폴더 내 모든 파일 조회 (확장자 무관)
            foreach (var file in Directory.GetFiles(folderPath))
            {
                var info = new FileInfo(file);

                // 키워드 필터 (파일명에 keyword가 없으면 스킵)
                if (IsFiltered(info.Name, keyword))
                    continue;

                // 파일명 기반 날짜/버전/카메라타입/코어 이름 추출
                string date = ExtractDateFromName(info.Name, info.LastWriteTime);
                string version = ExtractVersionFromName(info.Name);
                string cameraType = ExtractCameraTypeFromName(info.Name);
                string coreName = ExtractCoreName(info.Name);

                // DataGrid에 바인딩할 익명 객체 추가
                list.Add(new
                {
                    Num = num++,
                    FileName = coreName,    // 예: 20251124_v1_EngineDistribution_EO → EngineDistribution
                    Column2 = version,      // 예: 20251124_v1_EngineDistribution_EO → v1
                    CameraType = cameraType,// 예: 20251124_v1_EngineDistribution_EO → EO
                    Date = date             // 필터용 날짜 값
                });

                // 🔻 🔻 🔻 예전 ZIP 내부까지 읽던 로직 (지금은 사용하지 않음, 참고용으로 주석 처리) 🔻 🔻 🔻
                /*
                if (string.Equals(info.Extension, ".zip", StringComparison.OrdinalIgnoreCase))
                {
                    using var archive = ZipFile.OpenRead(file);

                    foreach (var entry in archive.Entries)
                    {
                        if (string.IsNullOrWhiteSpace(entry.Name))
                            continue;

                        if (IsFiltered(entry.FullName, keyword))
                            continue;

                        var entryWriteTime = entry.LastWriteTime == DateTimeOffset.MinValue
                            ? info.LastWriteTime
                            : entry.LastWriteTime.LocalDateTime;

                        string entryDate = ExtractDateFromName(entry.FullName, entryWriteTime);
                        string entryCameraType = ExtractCameraTypeFromName(entry.FullName);
                        string entryCoreName = ExtractCoreName(entry.Name);

                        list.Add(new
                        {
                            Num = num++,
                            FileName = entryCoreName,
                            Column2 = entryDate,
                            CameraType = entryCameraType
                        });
                    }
                }
                */
                // 🔺 🔺 🔺 ZIP 내부 처리 끝 (현재 요구사항에서는 사용 안 함) 🔺 🔺 🔺
            }

            return list;
        }

        /// <summary>
        /// 파일명에서 날짜(YYYYMMDD)를 추출.
        /// - 규칙 1: "언더바(_)로 분리했을 때 첫 번째 토큰이 8자리 숫자"면 그 값을 날짜로 사용
        ///   예) 20251124_v1_EngineDistribution_EO → 20251124
        /// - 규칙 2: 그렇지 않으면 전체 이름에서 8자리 숫자를 정규식으로 찾아서 사용
        /// - 규칙 3: 그래도 없으면 파일 수정일(LastWriteTime)을 yyyyMMdd로 사용
        /// </summary>
        private static string ExtractDateFromName(string fileName, DateTime lastWriteTime)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            // 언더바 기준으로 분리
            var parts = nameWithoutExtension
                .Split('_', StringSplitOptions.RemoveEmptyEntries);

            // 첫 토큰이 8자리 숫자(YYYYMMDD)라면 그대로 사용
            if (parts.Length > 0 && Regex.IsMatch(parts[0], @"^\d{8}$"))
                return parts[0];

            // 혹시 다른 위치에 8자리 날짜가 있으면 첫 매칭값 사용
            var dateMatch = Regex.Match(nameWithoutExtension, @"\d{8}");
            if (dateMatch.Success)
                return dateMatch.Value;

            // 파일명에 날짜가 없으면 파일 수정일 사용
            return lastWriteTime.ToString("yyyyMMdd");
        }

        /// <summary>
        /// 파일명에서 카메라 타입을 추출.
        /// - 기본 규칙: 언더바('_')로 분리했을 때 "마지막 토큰"을 카메라 타입으로 사용
        ///   예) 20251124_v1_EngineDistribution_EO → EO
        ///       20251125_v2_Engine_V1_MWIR       → MWIR
        /// - 토큰이 2개 이상일 때만 마지막 토큰을 카메라 타입으로 간주
        /// - 규칙에 맞지 않으면 "Null" 반환
        /// </summary>
        private static string ExtractCameraTypeFromName(string fileName)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            var parts = nameWithoutExtension
                .Split('_', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                // 마지막 토큰을 카메라 타입으로 사용 (예: MWIR, EO, LWIR 등)
                return parts[^1]; // C# 8.0 index-from-end 문법
            }

            // 카메라 타입을 인식할 수 없으면 기본값
            return "Null";
        }

        /// <summary>
        /// 파일명에서 버전 정보를 추출.
        /// - 규칙: 언더바('_')로 분리했을 때 두 번째 토큰을 버전으로 사용
        ///   예) 20251124_v1_EngineDistribution_EO → v1
        /// - 규칙에 맞지 않으면 "Null" 반환
        /// </summary>
        private static string ExtractVersionFromName(string fileName)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            var parts = nameWithoutExtension
                .Split('_', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
                return parts[1];

            return "Null";
        }

        /// <summary>
        /// 파일명에서 "코어 이름"을 추출.
        /// - 규칙: [0] = 날짜, [1] = 버전, [마지막] = 카메라타입 으로 보고,
        ///         그 사이에 있는 토큰들을 코어 이름으로 사용.
        ///   예) 20251124_v1_EngineDistribution_EO → EngineDistribution
        ///       20251124_v2_Engine_V1_EO         → Engine_V1
        /// - 토큰이 3개(날짜 + 버전 + 이름)인 경우: 세 번째 토큰을 코어 이름으로 사용
        /// - 그 외 경우: 전체 이름(확장자 제거) 사용
        /// </summary>
        private static string ExtractCoreName(string fileName)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            var parts = nameWithoutExtension
                .Split('_', StringSplitOptions.RemoveEmptyEntries);

            // 규칙: [0] = 날짜, [1] = 버전, [마지막] = 카메라 타입, 그 사이 = 코어 이름
            if (parts.Length >= 4)
            {
                // 중간 토큰들을 다시 이어붙여 코어 이름으로 사용
                // 예: [0]=20251124, [1]=v1, [2]=Engine, [3]=V1, [4]=EO → Engine_V1
                return string.Join('_', parts.Skip(2).Take(parts.Length - 3));
            }

            // 날짜 + 버전 + 이름 구조인 경우: [0]=날짜, [1]=버전, [2]=이름
            if (parts.Length == 3)
                return parts[2];

            // 표준 규칙에 맞지 않는 경우 기존 추출 로직 사용
            return (
                ExtractDateFromName(fileName, lastWriteTime),
                ExtractVersionFromName(fileName),
                NormalizeCoreNameFallback(fileName),
                ExtractCameraTypeFromName(fileName)
            );
        }

        /// <summary>
        /// 규칙 외 파일에서 카메라 타입이 FileName에 표시되는 것을 막기 위한 보완 로직.
        /// </summary>
        private static string NormalizeCoreNameFallback(string fileName)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var parts = nameWithoutExtension.Split('_', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 4)
            {
                return string.Join('_', parts.Skip(2).Take(parts.Length - 3));
            }

            // 토큰이 3개(날짜/버전/카메라 또는 이름)인 경우에는 카메라 타입을 코어 이름으로 오인하지 않도록 빈 값 반환
            if (parts.Length == 3)
                return "";

            return nameWithoutExtension;
        }

        /// <summary>
        /// 키워드 필터링 함수.
        /// - keyword가 비어 있지 않으면,
        ///   파일명에 keyword가 포함되어 있지 않을 때 true(=필터링 대상) 반환.
        /// - keyword가 null 또는 공백이면 항상 false 반환(필터링 없음).
        /// </summary>
        private static bool IsFiltered(string name, string? keyword)
        {
            return !string.IsNullOrWhiteSpace(keyword) &&
                   !name.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }

        // 🔻 🔻 🔻 예전 버전(참고용) – 필요 없으면 완전히 삭제해도 됨 🔻 🔻 🔻
        /*
        public static ObservableCollection<dynamic> LoadEngineVersionDataFromFolder(string folderPath)
        {
            var list = new ObservableCollection<dynamic>();

            if (!Directory.Exists(folderPath))
                return list;

            int num = 1;

            foreach (var file in Directory.GetFiles(folderPath))
            {
                var info = new FileInfo(file);

                list.Add(new
                {
                    Num = num++,
                    FileName = info.Name,
                    Column2 = info.LastWriteTime.ToString("yyyyMMdd"), // 파일 수정 날짜
                    CameraType = "Null" // 필요하면 나중에 바꿔도 됨
                });
            }

            return list;
        }

        public static ObservableCollection<dynamic> LoadEngineVersionDataFromFolder(string folderPath, string keyword)
        {
            var list = new ObservableCollection<dynamic>();

            if (!Directory.Exists(folderPath))
                return list;

            int num = 1;

            foreach (var file in Directory.GetFiles(folderPath))
            {
                var info = new FileInfo(file);

                // 파일 이름에 keyword 포함된 경우만 추가
                if (!info.Name.ToLower().Contains(keyword.ToLower()))
                    continue;

                list.Add(new
                {
                    Num = num++,
                    FileName = info.Name,
                    Column2 = info.LastWriteTime.ToString("yyyyMMdd"),
                    CameraType = "Null"
                });
            }

            return list;
        }
        */
        // 🔺 🔺 🔺 예전 버전 끝 🔺 🔺 🔺
    }
}
