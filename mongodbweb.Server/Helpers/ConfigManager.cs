namespace mongodbweb.Server.Helpers
{
    public static class ConfigManager
    {
        public static IConfiguration? config;

        public static bool useAuthorization = true;
        public static string? dbHost;
        public static string? dbPort;
        public static string? dbRules;
        public static string? allowedIp;
        public static string? customString;
        public static int batchCount = 100;
        public static bool firstStart;
        public static int deleteOtpInDays = 1;

        public static void SetConfig(IConfiguration configuration)
        {
            config = configuration;

            useAuthorization = config.GetValue<bool>("UseAuthorization");
            firstStart = config.GetValue<bool>("FirstStart");
            dbHost = config.GetValue<string>("DBHost");
            dbPort = config.GetValue<string>("DBPort");
            dbRules = config.GetValue<string>("DBRule");
            allowedIp = config.GetValue<string>("AllowedIp");
            customString = config.GetValue<string>("CustomString");
            batchCount = config.GetValue<int>("BatchCount");
            deleteOtpInDays = config.GetValue<int>("DeleteOtpInDays");
        }
    }
}