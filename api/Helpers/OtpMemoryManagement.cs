﻿using api.Models;
using static api.Helpers.LogManager;

namespace api.Helpers
{
    public static class OtpMemoryManagement
    {
        private static readonly Dictionary<string, OtpFileObject> OtpStore = new();
        private static readonly LogManager Logger = new();
        private static DateTime LastCleanupTime = DateTime.MinValue;
        private const int CleanupFreshRateInDay = 1;

        public static void WriteOtpFile(string uuid, OtpFileObject data)
        {
            data.Expire = DateTime.Now.AddDays(ConfigManager.deleteOtpInDays);
            OtpStore[uuid] = data;
        }

        public static OtpFileObject? ReadOtp(string uuid)
        {
            CleanUpOtp();

            if (OtpStore.TryGetValue(uuid, out var otpFile))
            {
                otpFile.LastAccess = DateTime.Now;
                return otpFile;
            }
            return null;
        }

        public static void DeleteOtp(string? uuid)
        {
            if (uuid is null)
                return;

            if (OtpStore.ContainsKey(uuid))
            {
                OtpStore.Remove(uuid);
                Logger.WriteLog(LogType.Info, "Deleted OTP Logout: " + uuid);
            }
        }

        private static void CleanUpOtp()
        {
            if ((DateTime.Now - LastCleanupTime).Days < CleanupFreshRateInDay)
                return;

            var expiredKeys = OtpStore.Where(pair => pair.Value.Expire < DateTime.Now)
                                      .Select(pair => pair.Key).ToList();

            foreach (var key in expiredKeys)
            {
                OtpStore.Remove(key);
                Logger.WriteLog(LogType.Info, $"The OTP {key} was deleted because it was older than {ConfigManager.deleteOtpInDays} days");
            }

            LastCleanupTime = DateTime.Now;
        }

        public static List<OtpFileObject> GetAllOtp()
        {
            return OtpStore.Values.Where(otp => otp.Expire >= DateTime.Now).ToList();
        }
    }
}
