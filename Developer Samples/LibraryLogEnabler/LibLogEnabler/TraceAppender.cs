using log4net.Appender;
using log4net.Core;
using System.Diagnostics;

namespace LibLogEnabler
{
    public class TraceAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent logEvent)
        {
            string message = logEvent.RenderedMessage;

            if (logEvent.Level <= Level.Debug)
                Trace.WriteLine(message);
            else if (logEvent.Level <= Level.Info)
                Trace.TraceInformation(message);
            else if (logEvent.Level <= Level.Warn)
                Trace.TraceWarning(message);
            else if (logEvent.Level <= Level.Error)
                Trace.TraceError(message);
        }
    }
}