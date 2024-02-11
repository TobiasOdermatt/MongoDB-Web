using api.Models;
using static System.Int32;

namespace api.Helpers
{
    public class LogManager
    {
        private static readonly string CurrentDirectory = $"{Directory.GetCurrentDirectory()}";
        public static readonly string Path = $"{CurrentDirectory}\\Logs\\{DateTime.Now:yyyy}\\{DateTime.Now:MM}\\";

        /// <summary>
        /// Write a new log
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public LogManager(LogType type, string message)
        {
            CreateDirectory();
            CreateLogFile(type);
            UpdateLogFile(type, message);
        }

        /// <summary>
        /// Create a new Log with Exception
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public LogManager(LogType type, string message, Exception exception)
        {
            CreateDirectory();
            CreateLogFile(type);
            UpdateLogFile(type, message);
            CreateExceptionLogFile(message, exception);
        }

        public LogManager()
        {
            CreateDirectory();
        }

        public void WriteLog(LogType type, string message, Exception exception)
        {
            CreateDirectory();
            CreateLogFile(type);
            UpdateLogFile(type, message);
            CreateExceptionLogFile(message, exception);
        }

        public void WriteLog(LogType type, string message)
        {
            CreateDirectory();
            CreateLogFile(type);
            UpdateLogFile(type, message);
        }

        public enum LogType
        {
            Info,
            Warning,
            Error
        }

        //Create the Exception File for the error
        private static void CreateExceptionLogFile(string message, Exception exception)
        {
            var logMessage = DateTime.Now.ToString("HH:mm:ss") + " - " + message + exception.Message + " - " + exception.StackTrace;
            using var sw = new StreamWriter(new FileStream(Path + "Exception.txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            sw.WriteLine(logMessage);
            sw.Flush();
            sw.Close();
        }

        //Create Start of Log File
        private static void CreateLogFile(LogType type)
        {
            if (File.Exists(Path + type + ".txt")) return;
            StreamWriter createFile = new(Path + type + ".txt", false);
            createFile.WriteLine("#===============================");
            createFile.WriteLine("# Web MongoDB Log " + type);
            createFile.WriteLine("#===============================");
            createFile.Flush();
            createFile.Close();
        }

        //Add a new line to the log file
        private static void UpdateLogFile(LogType type, string line)
        {
            try
            {
                using var file = new StreamWriter(new FileStream(Path + type + ".txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
                var logLine = DateTime.Now.ToString("dd") + " | " + DateTime.Now.ToString("HH:mm:ss ") + "|[" + type.ToString() + "]|" + "| " + line;
                file.WriteLine(logLine);
                Console.WriteLine(logLine);
                file.Flush();
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][UpdateLogFile] Error: {e.Message}");
            }
        }

        //Create Directory if the Dir not exists.
        private static void CreateDirectory()
        {
            try
            {
                DirectoryInfo dir = new(Path);

                if (!dir.Exists)
                    dir.Create();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][CreateDirectory] Error: {e.Message}");
            }
        }

        public static (int, int, int) CountLog(DateTime date)
        {
            var currentPath = $"{CurrentDirectory}\\Logs\\{date:yyyy}\\{date:MM}\\";
            var countOfInfo = CountLinesInLogFile(currentPath + "Info.txt");
            var countOfWarning = CountLinesInLogFile(currentPath + "Warning.txt");
            var countOfError = CountLinesInLogFile(currentPath + "Error.txt");

            return (countOfInfo, countOfWarning, countOfError);
        }

        // Counts the lines in the log file
        private static int CountLinesInLogFile(string filePath)
        {
            var count = 0;
            try
            {
                if (File.Exists(filePath))
                {
                    using StreamReader file = new(filePath);
                    while (file.ReadLine() is { } line)
                    {
                        //If line start with # then skip
                        if (line.StartsWith("#"))
                            continue;

                        count++;
                    }
                    file.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][CountLinesInLogFile] Error: {e.Message}");
            }

            return count;
        }

        public static List<DateTime> GetAvailableLogDates()
        {
            List<DateTime> dates = new();
            try
            {
                DirectoryInfo dir = new(CurrentDirectory + "\\Logs");
                foreach (var year in dir.GetDirectories())
                {
                    foreach (var month in year.GetDirectories())
                    {
                        DateTime date = new(Parse(year.Name), Parse(month.Name), 1);
                        dates.Add(date);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][GetLogDates] Error: {e.Message}");
            }

            return dates;
        }

        private static List<LogObject> ReadSingleLogFile(string path, DateTime dateTime)
        {
            List<LogObject> logs = new();
            try
            {
                if (File.Exists(path))
                {
                    using StreamReader file = new(path);
                    while (file.ReadLine() is { } line)
                    {
                        //If line is a comment then skip
                        if (line.StartsWith("#"))
                            continue;

                        var log = line.Split(" | ");
                        var date = log[0].Split(".");
                        var time = log[1].Split(" ");
                        var type = time[1].Split("]|");
                        var message = log[1].Split("||");

                        LogObject logObject = new()
                        {
                            Created = new DateTime(dateTime.Year, dateTime.Month, Parse(date[0]), Parse(time[0].Split(":")[0]), Parse(time[0].Split(":")[1]), Parse(time[0].Split(":")[2])),
                            Type = type[0].Replace("|[", ""),
                            Message = message[1]
                        };
                        logs.Add(logObject);
                    }
                    file.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][ReadSingleLogFile] Error: {e.Message}");
            }
            return logs;
        }


        //Read Log Files form Argument LogType and DateTime if type is All read all Log Files and return a List of LogObjects
        public static List<LogObject> ReadLogFiles(string type, DateTime date)
        {
            List<LogObject> logObjects = new();
            var currentPath = $"{CurrentDirectory}\\Logs\\{date:yyyy}\\{date:MM}\\";

            try
            {
                if (type == "Info")
                {
                    logObjects = ReadSingleLogFile(currentPath + "Info.txt", date);
                }
                else if (type == "Warning")
                {
                    logObjects = ReadSingleLogFile(currentPath + "Warning.txt", date);
                }
                else if (type == "Error")
                {
                    logObjects = ReadSingleLogFile(currentPath + "Error.txt", date);
                }
                else
                {
                    logObjects.AddRange(ReadSingleLogFile(currentPath + "Info.txt", date));
                    logObjects.AddRange(ReadSingleLogFile(currentPath + "Warning.txt", date));
                    logObjects.AddRange(ReadSingleLogFile(currentPath + "Error.txt", date));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LogManager][ReadLogFiles] Error: {e.Message}");
            }

            return logObjects;
        }
    }
}
