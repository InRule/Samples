using System;
using System.Web;
//LibLog References
using TraceLogger.Logging;
using TraceLogger.Logging.LogProviders;
//Log4Net References (mainly for code-based configuration)
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace LibLogEnabler
{
    public class LoggingEnabler : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            EnableLog4NetFromConfig();
            //EnableLog4NetFromCode();

            var logger = LogProvider.GetLogger("Startup");
            logger.Debug("Initialized.");
        }
        private void EnableLog4NetFromConfig()
        {
            log4net.Config.XmlConfigurator.Configure();
            LogProvider.SetCurrentLogProvider(new Log4NetLogProvider());
        }
        private void EnableLog4NetFromCode()
        {
            try
            {
                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

                hierarchy.Root.Appenders.Clear();

                PatternLayout patternLayout = new PatternLayout();
                patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
                patternLayout.ActivateOptions();

                RollingFileAppender roller = new RollingFileAppender();
                roller.AppendToFile = true;
                roller.File = @"InRule_ExecutionEngine.log";
                roller.Layout = patternLayout;
                roller.MaxSizeRollBackups = 5;
                roller.MaximumFileSize = "1MB";
                roller.RollingStyle = RollingFileAppender.RollingMode.Size;
                roller.StaticLogFileName = true;
                roller.ActivateOptions();
                hierarchy.Root.AddAppender(roller);

                TraceAppender traceAppender = new TraceAppender();
                traceAppender.Layout = patternLayout;
                traceAppender.ActivateOptions();
                hierarchy.Root.AddAppender(traceAppender);

                ConsoleAppender consoleAppender = new ConsoleAppender();
                consoleAppender.Layout = patternLayout;
                consoleAppender.ActivateOptions();
                hierarchy.Root.AddAppender(consoleAppender);

                hierarchy.Root.Level = Level.Warn;
                hierarchy.Configured = true;

                LogProvider.SetCurrentLogProvider(new Log4NetLogProvider());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error configuring log4net: " + ex.ToString());
            }
        }

        public void Dispose()
        {
        }
    }
}