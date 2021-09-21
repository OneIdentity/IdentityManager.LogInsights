using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;

namespace LogfileMetaAnalyser.ExceptionHandling
{
    public sealed class ExceptionHandler
    {
        private static readonly object _lockObj = new();
        private static ExceptionHandler	_instance;
        private static readonly Logger m_Logger = LogManager.GetCurrentClassLogger();

        private ExceptionHandler()
        {
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
                m_Logger.Info("Application is terminating unexpectedly.");

            if (e.ExceptionObject is Exception ex)
                m_Logger.Fatal(ex, ex.Message);
        }

        private void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            m_Logger.Fatal(e.Exception, e.Exception.Message);
        }

        private bool IsCancellationException(Exception ex)
        {
            bool _Filter(Exception _ex, Type _type)
            {
                if ( _ex is AggregateException agg )
                {
                    if ( _type.IsInstanceOfType(agg) )
                        return true;

                    if (agg.InnerExceptions.Any(_type.IsInstanceOfType)) 
                        return true;
                }
                else
                {
                    for (var e = _ex; e != null; e = e.InnerException)
                        if ( _type.IsInstanceOfType(e) )
                            return true;
                }

                return false;
            }            
            
            
            return _Filter(ex, typeof(OperationCanceledException)) ||
                   _Filter(ex, typeof(TaskCanceledException));
        }


        public void HandleException(Exception exception)
        {
            if (exception == null || IsCancellationException(exception))
                return;

            m_Logger.Error(exception, exception.Message);
        }

        public static ExceptionHandler Instance
        {
            get
            {
                if (_instance == null)
                    lock (_lockObj)
                    {
                        _instance ??= new ExceptionHandler();
                    }

                return _instance;
            }
        }
    }
}