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



        #region protected members

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

        [DllImport("user32.dll")]
        extern static IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

        [DllImport("user32.dll")]
        extern static IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);

        [DllImport("user32.dll")]
        extern static bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);

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

        const int WM_PAINT = 15;
        const int WM_USER = 0x400;
        const int WM_SETREDRAW = 0x000B;
        const int EM_GETEVENTMASK = WM_USER + 59;
        const int EM_SETEVENTMASK = WM_USER + 69;
        const int EM_GETSCROLLPOS = WM_USER + 221;
        const int EM_SETSCROLLPOS = WM_USER + 222;

        private void Scrolltest()
        {
            Point _ScrollPoint = Point.Empty;

            SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref _ScrollPoint);

            _ScrollPoint.Y = 0;

            SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref _ScrollPoint);
        }

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
        extern private static int SendMessageRefRect(IntPtr hWnd, uint msg, int wParam, ref RECT rect);

        [DllImport(@"user32.dll", EntryPoint = @"SendMessage", CharSet = CharSet.Auto)]
        extern private static int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, ref Rectangle lParam);

        private const int EmGetrect = 0xB2;
        private const int EmSetrect = 0xB3;



        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern static bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        [DllImport("user32.dll")]
        extern static int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref SCROLLINFO lpsi, bool fRedraw);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        extern static IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        struct SCROLLINFO
        {
            public uint cbSize;
            public uint fMask;
            public int nMin;
            public int nMax;
            public uint nPage;
            public int nPos;
            public int nTrackPos;
        }

        enum ScrollBarDirection
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }

        enum ScrollInfoMask
        {
            SIF_RANGE = 0x1,
            SIF_PAGE = 0x2,
            SIF_POS = 0x4,
            SIF_DISABLENOSCROLL = 0x8,
            SIF_TRACKPOS = 0x10,
            SIF_ALL = SIF_RANGE + SIF_PAGE + SIF_POS + SIF_TRACKPOS
        }

        const int WM_VSCROLL = 277;
        const int SB_LINEUP = 0;
        const int SB_LINEDOWN = 1;
        const int SB_THUMBPOSITION = 4;
        const int SB_THUMBTRACK = 5;
        const int SB_TOP = 6;
        const int SB_BOTTOM = 7;
        const int SB_ENDSCROLL = 8;

        public void ScrollToLine(int iLine)
        {
            if (iLine < 0)
                return;

            int iChar = GetFirstCharIndexFromLine(iLine);

            Point cPos = GetPositionFromCharIndex(iChar);

            IntPtr handle = Handle;

            // Get current scroller position

            SCROLLINFO si = new SCROLLINFO();
            si.cbSize = (uint)Marshal.SizeOf(si);
            si.fMask = (uint)ScrollInfoMask.SIF_ALL;
            GetScrollInfo(handle, (int)ScrollBarDirection.SB_VERT, ref si);

            // Increase position by pixles
            si.nPos += cPos.Y;

            // Reposition scroller
            SetScrollInfo(handle, (int)ScrollBarDirection.SB_VERT, ref si, true);

            // Send a WM_VSCROLL scroll message using SB_THUMBTRACK as wParam
            // SB_THUMBTRACK: low-order word of wParam, si.nPos high-order word of  wParam

            IntPtr ptrWparam = new IntPtr(SB_THUMBTRACK + 0x10000 * si.nPos);
            IntPtr ptrLparam = new IntPtr(0);
            SendMessage(handle, WM_VSCROLL, ptrWparam, ptrLparam);

            //SendMessage(rtbLog.Handle, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);

            //rtbLog.Select(iChar, 0);
        }

        public void BeginUpdate()
        {
            SendMessage(Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        }

        public void EndUpdate()
        {
            SendMessage(Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
        }


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

        public void ScrollToLineX(int jumppos)
        {
           Scrolltest();
        }
    }
}
