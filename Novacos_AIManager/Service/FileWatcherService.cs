using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacos_AIManager.Service
{
    public class FileWatcherService
    {
        private FileSystemWatcher watcher;
        private readonly string sourceFolder;
        private readonly string targetFolder;

        public FileWatcherService(string source, string target)
        {
            sourceFolder = source;
            targetFolder = target;

            watcher = new FileSystemWatcher
            {
                Path = sourceFolder,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*",
                EnableRaisingEvents = false,
                IncludeSubdirectories = false
            };

            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
        }

        public void Start()
        {
            // ⭐ 처음 실행 시 1회 폴더 비교 & 누락 파일 자동 복사
            SyncFolders();

            watcher.EnableRaisingEvents = true;
            Console.WriteLine("📁 파일 감시 시작됨: " + sourceFolder);
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            Console.WriteLine("📁 파일 감시 중지됨");
        }

        // ===============================
        // ⭐ 처음 1회만 파일 비교 & 복사
        // ===============================
        private void SyncFolders()
        {
            Console.WriteLine("🔍 폴더 동기화 시작...");

            if (!Directory.Exists(sourceFolder))
                return;

            Directory.CreateDirectory(targetFolder);

            var sourceFiles = Directory.GetFiles(sourceFolder);
            var targetFiles = new HashSet<string>(Directory.GetFiles(targetFolder));

            foreach (var src in sourceFiles)
            {
                string fileName = Path.GetFileName(src);
                string targetPath = Path.Combine(targetFolder, fileName);

                // 목적지에 없으면 복사
                if (!File.Exists(targetPath))
                {
                    try
                    {
                        File.Copy(src, targetPath, true);
                        Console.WriteLine($"✔ 복사됨 (초기 동기화): {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ 복사 실패: {fileName} -> {ex.Message}");
                    }
                }
            }

            Console.WriteLine("🔍 폴더 동기화 완료");
        }


        // ===============================
        // ⭐ 새로운 파일 감지 시 자동 이동
        // ===============================

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            await HandleNewFile(e.FullPath);
        }

        private async void OnChanged(object sender, FileSystemEventArgs e)
        {
            await HandleNewFile(e.FullPath);
        }

        // ⭐ 파일 생성 → 파일이 완전히 생성/저장될 때까지 기다린 후 복사 & 이동
        private async Task HandleNewFile(string fullPath)
        {
            string fileName = Path.GetFileName(fullPath);
            string destPath = Path.Combine(targetFolder, fileName);

            // 💡 파일이 아직 쓰는 중이면 잠금 발생 → 일정 시간 동안 재시도
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    using (FileStream stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        break; // 잠금 없음 → break
                    }
                }
                catch
                {
                    await Task.Delay(500); // 0.5초 후 재시도
                }
            }

            try
            {
                File.Copy(fullPath, destPath, true);  // 복사
                File.Delete(fullPath);                // 삭제 → 이동 효과

                Console.WriteLine($"✔ 파일 이동됨: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ 파일 이동 실패: " + ex.Message);
            }
        }
    }
}
