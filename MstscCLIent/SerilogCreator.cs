using Destructurama;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Syslog;


namespace MstscCLIent
{
    public static class SerilogCreator
    {
        public static ILogger Init(Options options, string logName, string dateTime, string appSessionID)
        {

            LoggerConfiguration loggerConfiguration = new();

            loggerConfiguration
            .MinimumLevel.Verbose()
           .Destructure.UsingAttributes()
           .Enrich.WithExceptionDetails()
           .Enrich.WithEnvironmentVariable("COMPUTERNAME", "Launcher")
           .Enrich.WithProperty(name: "User", value: options.Username)
           .Enrich.WithProperty(name: "ASP", value: options.Host)
           .Enrich.WithProperty(name: @$"{logName}SessionID", value: appSessionID)
           .Enrich.FromLogContext()
           .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}");

            if (options.Debug)
            {
                string logPath = Path.GetTempPath();
                string sam = options.Username.Split('@')[0];
                Directory.CreateDirectory(@$"{logPath + "\\" + logName}");
                string appLogPath = @$"{logPath + "\\" + logName + "\\" + dateTime + "-" + sam + "-" + logName}.log";
                loggerConfiguration.WriteTo.File(restrictedToMinimumLevel: LogEventLevel.Verbose, path: appLogPath, rollOnFileSizeLimit: true);
            }

            if (!String.IsNullOrWhiteSpace(options.Syslog))
            {
                loggerConfiguration
                     .WriteTo.UdpSyslog(
                        host: options.Syslog,
                        appName: logName,
                        format: SyslogFormat.RFC5424,
                        structuredDataId: logName,
                        outputTemplate: "{Message}{NewLine}{Exception}",
                         restrictedToMinimumLevel: LogEventLevel.Information
                      );
            }
            else
            {
                loggerConfiguration.WriteTo.EventLog(logName, logName, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception} {Properties:j}", restrictedToMinimumLevel: LogEventLevel.Information);
            }

            return loggerConfiguration.CreateLogger();
        }

    }
}
