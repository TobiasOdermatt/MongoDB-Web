using mongodbweb.Server.Models;
using System.Text.Json;
using static mongodbweb.Server.Helpers.LogManager;
using Path = System.IO.Path;

namespace mongodbweb.Server.Helpers
{
    public static class OtpFileManagement
    {
        private static readonly LogManager Logger = new();
        private static readonly string OtpPath = $"{Directory.GetCurrentDirectory()}" + @"\OTP\";
        private static readonly string UserStoragePath = $"{Directory.GetCurrentDirectory()}" + @"\UserStorage\";
        private const int CleanupFreshRateInDay = 1;

        public static void WriteOtpFile(string uuid, OtpFileObject data)
        {
            data.Expire = DateTime.Now.AddDays(ConfigManager.deleteOtpInDays);
            var path = OtpPath + uuid + ".txt";
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(path, json);
        }

        public static OtpFileObject? ReadOtpFile(string uuid)
        {
            CleanUpOtpFiles();
            var path = OtpPath + uuid + ".txt";
            if (!File.Exists(path))
                return null;

            var text = File.ReadAllText(path);
            var otpFile = Newtonsoft.Json.JsonConvert.DeserializeObject<OtpFileObject>(text);
            if (otpFile != null)
                ChangeLastAccess(uuid, otpFile);

            return otpFile;
        }

        private static void ChangeLastAccess(string uuid, OtpFileObject otpFile)
        {
            otpFile.LastAccess = DateTime.Now;
            var path = OtpPath + uuid + ".txt";
            var updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(otpFile);
            File.WriteAllText(path, updatedJson);
        }

        public static void DeleteOtpFile(string? uuid)
        {
            if (uuid is null)
                return;

            var path = OtpPath + uuid + ".txt";
            var userPath = UserStoragePath + uuid;
            if (File.Exists(path))
                File.Delete(path);

            if (Directory.Exists(userPath))
                DeleteDirectory(userPath);

            Logger.WriteLog(LogType.Info, "Deleted OTP file Logout: " + uuid);
        }

        private static void CleanUpOtpFiles()
        {
            if (!CheckCleanUpNeeded())
                return;

            UpdateCleanUpLog();
            LogManager log = new(LogType.Info, "Cleaned up OTP files");
            foreach (var fileName in Directory.GetFiles(OtpPath))
            {
                var text = File.ReadAllText(fileName);
                if (Path.GetFileName(fileName) == "CleanUpFile.txt")
                    continue;

                var otpFile = Newtonsoft.Json.JsonConvert.DeserializeObject<OtpFileObject>(text);
                if (otpFile is null) continue;
                if (otpFile.Expire >= DateTime.Now) continue;
                File.Delete(fileName);
                Logger.WriteLog(LogType.Info, "The OTP file " + fileName + " was deleted because it was older than " + ConfigManager.deleteOtpInDays + " days");
                var uuid = Path.GetFileNameWithoutExtension(fileName);
                var userPath = UserStoragePath + uuid;
                if (Directory.Exists(userPath))
                    DeleteDirectory(userPath);
            }
        }

        private static bool CheckCleanUpNeeded()
        {
            if (!Directory.Exists(OtpPath))
                Directory.CreateDirectory(OtpPath);

            var cleanUpPath = OtpPath + "CleanUpFile.txt";
            if (File.Exists(cleanUpPath))
            {
                var text = File.ReadAllText(cleanUpPath);
                var cleanUpFile = Newtonsoft.Json.JsonConvert.DeserializeObject<CleanUpFileObject>(text);
                if (cleanUpFile is null) return true;
                if (cleanUpFile.LastCleanUp.AddDays(CleanupFreshRateInDay) < DateTime.Now)
                    return true;
            }
            else
                return true;

            return false;
        }

        private static void UpdateCleanUpLog()
        {
            CleanUpFileObject data = new()
            {
                LastCleanUp = DateTime.Now
            };

            var json = JsonSerializer.Serialize(data);
            if (!File.Exists(OtpPath + "CleanUpFile.txt"))
                File.Create(OtpPath + "CleanUpFile.txt").Close();

            File.WriteAllText(OtpPath + "CleanUpFile.txt", json);
        }

        public static List<OtpFileObject> GetAllOtpFiles()
        {
            var otpList = new List<OtpFileObject>();
            foreach (var fileName in Directory.GetFiles(OtpPath))
            {
                if (Path.GetFileName(fileName) == "CleanUpFile.txt")
                    continue;

                var text = File.ReadAllText(fileName);
                var otpFile = Newtonsoft.Json.JsonConvert.DeserializeObject<OtpFileObject>(text);
                if (otpFile is not null)
                    otpList.Add(otpFile);

            }
            return otpList;
        }

        private static void DeleteDirectory(string targetDir)
        {
            var files = Directory.GetFiles(targetDir);
            var dirs = Directory.GetDirectories(targetDir);

            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }
    }
}
