﻿using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Ordos.Core.Utilities
{
    public class Logger
    {
        public static NLog.Logger Init()
        {
            var config = new LoggingConfiguration();

            config.Variables.Add("logDirectory", Paths.ExportRoot);

            var fileTarget = new FileTarget
            {
                Name = "logfile",
                FileName = @"${var:logDirectory}\Logs\Log ${date:format=yyyy-MM-dd}.log",
                Layout = new CsvLayout
                {
                    Columns =
                    {
                        new CsvColumn("Time", "${longdate}"),
                        new CsvColumn("Severity", "${uppercase:${level}}"),
                        new CsvColumn("Location",
                            "${callsite:className=True:fileName=True:includeSourcePath=False:methodName=True}"),
                        new CsvColumn("Detail", "${message}"),
                        new CsvColumn("Exception", "${exception:format=ToString}")
                    }
                }
            };
            config.AddTarget(fileTarget);

            var consoleTarget = new ConsoleTarget("logconsole")
            {
                Layout = "${longdate} ${uppercase:${level}} ${message}"
            };
            config.AddTarget(consoleTarget);

            // Step 3. Define rules
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);

            // Step 4. Activate the configuration
            LogManager.Configuration = config;

            return LogManager.GetCurrentClassLogger();
        }
    }
}