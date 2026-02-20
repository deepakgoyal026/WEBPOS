using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace EazyPOS.Common
{
    public class LoggerService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public LoggerService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public bool WriteLogs(string strMessage)
        {
            try
            {
                string isWriteLog = _configuration["AppSettings:isWriteLog"];
                string logFolderConfig = _configuration["AppSettings:LogFolderPath"];

                if (!string.IsNullOrEmpty(isWriteLog) && isWriteLog.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    string logFolderPath;
                    if (!string.IsNullOrEmpty(logFolderConfig) && logFolderConfig.StartsWith("~"))
                    {
                        var relativePath = logFolderConfig.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
                        logFolderPath = Path.Combine(_environment.ContentRootPath, relativePath);
                    }
                    else
                    {
                        logFolderPath = logFolderConfig;
                    }

                    if (!Directory.Exists(logFolderPath))
                    {
                        Directory.CreateDirectory(logFolderPath);
                    }

                    string logFilePath = Path.Combine(logFolderPath, "Tracing.txt");
                    string fileContent = "--" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "--" + Environment.NewLine + strMessage;

                    using (FileStream fs = new FileStream(logFilePath, FileMode.Append, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine(fileContent);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
