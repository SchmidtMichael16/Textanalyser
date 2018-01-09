using Serilog;
using System;

namespace Log
{
    public static class SeqLog
    {
        public static void WriteNewLogMessage(string message, params object[] propertyValues)
        {
            Serilog.Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();

            Serilog.Log.Information(message, propertyValues);

            // Important to call at exit so that batched events are flushed.
            Serilog.Log.CloseAndFlush();

        }

    }
}
