using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace LogfileMetaAnalyser.Controls
{
    [Flags]
    public enum Options
    {
        InnerExceptions = 1,
        StackTrace = 2,
        ErrorNumber = 4,
        LineBreaks = 8,
        TopRelevanceOnly = 16,
        CrashReport = 32,
        Default = InnerExceptions | LineBreaks | TopRelevanceOnly,
        All = 255,
    }    
    
    public partial class ExceptionForm : Form
	{
		private readonly Exception _Exception;

        private ExceptionForm()
		{
			InitializeComponent();
		}

		private ExceptionForm(Exception exception) : this()
		{
            _Exception = exception ?? throw new ArgumentNullException(nameof(exception));

			rtbError.Rtf = ToRichText(_Exception, Options.Default);
		}

		public static DialogResult ShowDialog( Exception ex, Form parent = null)
		{
			using (ExceptionForm dlr = new(ex))
			{
				return dlr.ShowDialog(parent);
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void chbExtended_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				rtbError.Rtf = ToRichText(_Exception, chbExtended.Checked ? Options.All : Options.Default);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}
		}


        private bool IsSet(Options options, Options optionToCheck)
		{
			return (options & optionToCheck) == optionToCheck;
		}

        private string GetMessageAsRichText(Exception exception, Options options)
		{
			var message = exception.Message;

			return MaskRichText(message);
		}


        private string ToRichText(Exception ex, Options options)
		{
			var exceptions = GetExceptionsToShow(ex, options).ToList();

			var sb = new StringBuilder();
			sb.Append(RTF_Header);

			AppendRichText(options, exceptions.First(), sb, RTF_Exception_Line);

			foreach (var exception in exceptions.Skip(1))
			{
				AppendRichText(options, exception, sb, RTF_InnerException_Line);
			}

			sb.Append(RTF_Tail);

			return sb.ToString();
		}


		private void AppendCallStackAsRichText(StringBuilder sb, Exception ex)
		{
			if (ex == null || ex.StackTrace == null)
                return;


			sb.Append(RTF_CallStack_Header);

			var callStackEntries = ex.StackTrace.Split('\n');

			foreach (var line in callStackEntries.Reverse())
			{
				sb.AppendFormat(RTF_CallStack_Line, line.Trim(' ', '\r').Replace(@"\", @"\\"));
			}
		}

		private void AppendRichText(Options options, Exception exception, StringBuilder sb, string prefix)
		{
			sb.AppendFormat(prefix, GetMessageAsRichText(exception, options));

			if (IsSet(options, Options.StackTrace))
			{
				AppendCallStackAsRichText(sb, exception);
			}
		}

		private IEnumerable<Exception> GetExceptions(Exception root)
		{
			var ex = root;

			while (ex != null)
			{
				if (!(ex is AggregateException))
					yield return ex;

				if (ex is ReflectionTypeLoadException)
				{
					foreach (Exception loadEx in ((ReflectionTypeLoadException)ex).LoaderExceptions)
					{
						yield return loadEx;
					}
				}

				ex = ex.InnerException;
			}
		}

		private List<Exception> GetExceptionsToShow(Exception root, Options options)
		{
			if (!IsSet(options, Options.InnerExceptions))
			{
				return GetExceptions(root).Take(1).ToList();
			}

			var exceptions = GetExceptions(root).ToList();

			return exceptions.ToList();
		}

		private string MaskRichText(string message)
		{
			var ret = new StringBuilder(message.Length);

			foreach (var character in message)
			{
				if (character > 255)
				{
					// Unicode
					ret.AppendFormat(@"\u{0:0000}?", (int)character);
				}
				else if (character == '\\')
				{
					ret.Append(@"\\");
				}
				else if (character == '\n')
				{
					ret.Append(@"\par ");
				}
				else if (character != '\r')
				{
					ret.Append(character);
				}
			}

			return ret.ToString();
		}

        private const string RTF_CallStack_Header = @"\pard\fi-284\li568\tx568\cf1\b0\fs16";
        private const string RTF_CallStack_Line = @"\tab {0}\par";
        private const string RTF_CrashReport = @"\par\pard\tx568\cf0\b\fs18 Crash report:\b0 \par \cf1\fs16\par\pard\fi-284\li568\tx568";
        private const string RTF_Exception_Line = @"\viewkind4\tx284\tx568\uc1\pard\sa120\b\f0\fs20 {0}\par";
        private const string RTF_Header = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1031{\fonttbl{\f0\fswiss\fprq2\fcharset0 Tahoma;}{\f1\fnil\fcharset0 Tahoma;}}{\colortbl ;\red153\green153\blue153;}";
        private const string RTF_InnerException_Line = @"\pard\li284\cf0\b0\fs18 {0}\par";
        private const string RTF_Tail = @"}";
    }
}
