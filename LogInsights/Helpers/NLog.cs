using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

namespace LogInsights.Helpers
{
    public class NLog : IDisposable
    {
        private readonly string LogerName;
        private readonly Logger log;

        public NLog(string LogerName)
        {
            this.LogerName = LogerName;
                       
            log = LogManager.GetLogger(this.LogerName);
        }

        public void Dispose()
        {
            try
            {
                LogManager.Flush();
            }
            catch { }
        }

        public static void NTrace(string loggername, string msg, params object[] o)
        {
            using (NLog l = new NLog(loggername))
                l.Trace(msg, o);
        }

        public void Trace(string msg, params object[] o)
        {
            if (!log.IsTraceEnabled)
                return;

            if (o != null && o.Length > 0 && msg.Contains("{0}"))
                msg = string.Format(msg, o);

            log.Trace(msg);
        }

        public static void NDebug(string loggername, string msg, params object[] o)
        {
            using (NLog l = new NLog(loggername))
                l.Debug(msg, o);
        }

        public void Debug(string msg, params object[] o)
        {
            if (!log.IsDebugEnabled)
                return;

            if (o != null && o.Length > 0 && msg.Contains("{0}"))
                msg = string.Format(msg, o);

            log.Debug(msg);
        }

        public static void NInfo(string loggername, string msg, params object[] o)
        {
            using (NLog l = new NLog(loggername))
                l.Info(msg, o);
        }

        public void Info(string msg, params object[] o)
        {
            if (!log.IsInfoEnabled)
                return;

            if (o != null && o.Length > 0 && msg.Contains("{0}"))
                msg = string.Format(msg, o);

            log.Info(msg);
        }

        public static void NWarning(string loggername, string msg, params object[] o)
        {
            using (NLog l = new NLog(loggername))
                l.Warning(msg, o);
        }

        public void Warning(string msg, params object[] o)
        {
            if (!log.IsWarnEnabled)
                return;

            if (o != null && o.Length > 0 && msg.Contains("{0}"))
                msg = string.Format(msg, o);

            log.Warn(msg);
        }

        public static void NWarning(string loggername, Exception exception)
        {
            using (NLog l = new NLog(loggername))
                l.Warning(exception);
        }

        public void Warning(Exception E)
        {
            log.Warn(E, "Warning");
        }

        public static void NError(string loggername, string msg, params object[] o)
        {
            using (NLog l = new NLog(loggername))
                l.Error(msg, o);
        }

        public void Error(string msg, params object[] o)
        {
            if (o != null && o.Length > 0 && msg.Contains("{0}"))
                msg = string.Format(msg, o);

            log.Error(msg);
            LogManager.Flush();
        }

        public static void NError(string loggername, Exception exception, string msg, params object[] o)
        {
            using (NLog l = new NLog(loggername))
                l.Error(exception, msg, o);
        }

        public void Error(Exception exception, string msg, params object[] o)
        {
            if (o != null && o.Length > 0 && msg.Contains("{0}"))
                msg = string.Format(msg, o);

            log.Error(exception, msg);
            //log.Error(Helpers.ExceptionHelper.GetCompleteErrorMessage(E));

            LogManager.Flush();
        }


    }

}
