using System;
using System.Windows.Forms;
using LogfileMetaAnalyser.ExceptionHandling;

namespace LogfileMetaAnalyser
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            var exHandler = ExceptionHandler.Instance;

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                exHandler.HandleException(ex);

                //MessageBox.Show(E.Message + "\r\n" + E.StackTrace, "gen Error");
            }
        }
    }
}
