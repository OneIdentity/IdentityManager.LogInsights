using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LogfileMetaAnalyser.Controls
{
    /// <summary>
    /// Extended RichTextBox with line number support
    /// </summary>
    public class ExRichTextBox : RichTextBox
    {
        private PAINTSTRUCT _ps;
        private bool _bShowLineNumbers;
        private int _lineNumberOffset;
        private int _lineNumberWidth = 64;

        #region public members

        public ExRichTextBox()
        {
            //SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        [DefaultValue(false)]
        public bool ShowLineNumbers
        {
            get { return _bShowLineNumbers; }

            set
            {
                if (_bShowLineNumbers == value)
                    return;

                _bShowLineNumbers = value;

                ConfigureLineNumbers();
            }
        }

        [DefaultValue(0)]
        public int LineNumberOffset
        {
            get { return _lineNumberOffset; }

            set
            {
                _lineNumberOffset = value;

                Invalidate();
            }
        }

        [DefaultValue(0)]
        public int LineNumberWidth
        {
            get { return _lineNumberWidth; }

            set
            {
                _lineNumberWidth = value;

                ConfigureLineNumbers();
            }
        }

        #endregion

        private void ConfigureLineNumbers()
        {
            if (ShowLineNumbers)
            {
                SetInnerMargins(LineNumberWidth, 0, 0, 0);
            }
            else
            {
                SetInnerMargins(0, 0, 0, 0);
            }

            Invalidate();
        }



        #region prtected members

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!ShowLineNumbers)
                return;

            Point pScroll = GetPositionFromCharIndex(0);

            Rectangle rDraw = new Rectangle(pScroll.X - LineNumberWidth, ClientRectangle.Y, LineNumberWidth - 2, ClientRectangle.Height);

            rDraw.Intersect(e.ClipRectangle);

            if (rDraw.Width == 0 || rDraw.Height == 0)
                return;

            Color c = Color.FromArgb(5, 170, 215);

            using (Pen pLine = new Pen(c))
            {
                e.Graphics.DrawLine(pLine, pScroll.X - 2, rDraw.Top, pScroll.X - 2, rDraw.Bottom);
            }

            // get the char index at the upper left drawing region
            int cIndex = GetCharIndexFromPosition(rDraw.Location);

            // get the line of this char index
            int iLine = GetLineFromCharIndex(cIndex);

            // get the real start position of this line
            Point pC = GetPositionFromCharIndex(cIndex);

            // this is the rectangle for the first line to draw
            Rectangle rLine = new Rectangle(pScroll.X - LineNumberWidth, pC.Y - 15, LineNumberWidth - 2, 15);

            int iMax = Math.Max(Lines.Length, 1);

            using (Brush bText = new SolidBrush(c))
            using (StringFormat sf = new StringFormat(StringFormat.GenericDefault))
            {
                sf.Alignment = StringAlignment.Far;

                // draw next line till the draw region ends
                while (rLine.Y <= rDraw.Bottom)
                {
                    e.Graphics.FillRectangle(Brushes.WhiteSmoke, rLine);

                    e.Graphics.DrawString((LineNumberOffset + iLine).ToString(), Font, bText, rLine, sf);

                    rLine.Y += 15;
                    iLine++;

                    if (iLine > iMax)
                        break;
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            ConfigureLineNumbers();
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            IntPtr hdc=IntPtr.Zero;

            if (m.Msg == WM_PAINT)
            {
                hdc = BeginPaint(Handle, out _ps);

                Invalidate(_ps.rcPaint.ToRectangle());
            }

            base.WndProc(ref m);

            if (hdc != IntPtr.Zero)
            {
                using (Graphics graphic = Graphics.FromHwnd(Handle))
                {
                    OnPaint(new PaintEventArgs(graphic, _ps.rcPaint.ToRectangle()));
                }

                EndPaint(Handle, ref _ps);
            }
        }

        #endregion

        #region WinCalls

        private const int WM_PAINT = 15;

        [StructLayout(LayoutKind.Sequential)]
        struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
        }

        [DllImport("user32.dll")]
        static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);

        [DllImport("user32.dll")]
        static extern bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);

        public void SetInnerMargins( int left, int top, int right, int bottom)
        {
            var rect = GetFormattingRect();

            var newRect = new Rectangle(left, top, rect.Width - left - right, rect.Height - top - bottom);
            SetFormattingRect(newRect);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public readonly int Left;
            public readonly int Top;
            public readonly int Right;
            public readonly int Bottom;

            private RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom)
            {
            }

            public Rectangle ToRectangle()
            {
                return new Rectangle(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top);
            }
        }

        [DllImport(@"User32.dll", EntryPoint = @"SendMessage", CharSet = CharSet.Auto)]
        private static extern int SendMessageRefRect(IntPtr hWnd, uint msg, int wParam, ref RECT rect);

        [DllImport(@"user32.dll", EntryPoint = @"SendMessage", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, ref Rectangle lParam);

        private const int EmGetrect = 0xB2;
        private const int EmSetrect = 0xB3;

        private void SetFormattingRect(Rectangle rect)
        {
            var rc = new RECT(rect);
            SendMessageRefRect(Handle, EmSetrect, 0, ref rc);
        }

        private Rectangle GetFormattingRect()
        {
            var rect = new Rectangle();
            SendMessage(Handle, EmGetrect, (IntPtr)0, ref rect);
            return rect;
        }

        #endregion
    }
}
