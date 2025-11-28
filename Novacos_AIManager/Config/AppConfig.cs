using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Novacos_AIManager.Config
{
    public class AppConfig
    {
        public int ViewSetting { get; set; }
        public string LoginUrl { get; set; }
        public string MainUrl { get; set; }
        public string EngVerPath { get; set; }
        public string EngDeployPath { get; set; }
        public string LearningVerPath {  get; set; }
        public string LearningDeployPath { get; set; }
        public string FileWatcherSourcePath { get; set; }
        public string FileWatcherTargetPath { get; set; }

        private static readonly string ConfigDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");

        private static readonly string ConfigFilePath =
            Path.Combine(ConfigDir, "app_config.json");

        // ======================
        // 1) config 파일 읽기
        // ======================
        public static AppConfig Load()
        {
            // config 폴더 없으면 생성
            if (!Directory.Exists(ConfigDir))
                Directory.CreateDirectory(ConfigDir);

            // config 파일 없으면 기본값 생성
            if (!File.Exists(ConfigFilePath))
            {
                var defaultConfig = new AppConfig
                {
                    ViewSetting = 1,
                    LoginUrl = "http://127.0.0.1:8080/login",
                    MainUrl = "http://127.0.0.1:8080/main",
                    EngVerPath = "C:\\EngineVersions\\AIEngineVersion",
                    EngDeployPath = "C:\\EngineVersions\\AIEngineDistribution",
                    LearningVerPath = "C:\\EngineVersions\\AILearningDataVersion",
                    LearningDeployPath = "C:\\EngineVersions\\AILearningDataDistribution",
                    FileWatcherSourcePath = "C:\\EngineVersions",
                    FileWatcherTargetPath = "C:\\EngineVersions"
                };

                Save(defaultConfig); // 자동 저장
                return defaultConfig;
            }

            // 파일이 존재하면 읽기
            string json = File.ReadAllText(ConfigFilePath);
            return JsonSerializer.Deserialize<AppConfig>(json);

            //// 파일이 존재하면 읽기
            //string json = File.ReadAllText(ConfigFilePath);
            //var config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();

            //// 기존 config에 신규 필드가 없는 경우 기본값 설정
            //config.FileWatcherSourcePath ??= "C:\\EngineVersions1";
            //config.FileWatcherTargetPath ??= "C:\\EngineVersions2";

            //return config;
        }

        // ======================
        // 2) config 저장하기
        // ======================
        public static void Save(AppConfig config)
        {
            string json = JsonSerializer.Serialize(
                config, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(ConfigFilePath, json);
        }
    }
}
