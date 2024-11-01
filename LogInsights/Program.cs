using System;
using System.Windows.Forms;
using LogInsights.ExceptionHandling;

namespace LogInsights
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            var mainForm = new MainForm();
            var exHandler = ExceptionHandler.Instance;
            exHandler.MainForm = mainForm;

            Application.Run(mainForm);
        }
    }
}
