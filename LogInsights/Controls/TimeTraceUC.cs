using LogInsights.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Linq; 
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using LogInsights.Helpers;

namespace LogInsights.Controls
{
    public partial class TimeTraceUC : UserControl
    {
        private static int minYPositionClipping = 45;
        private static int majorNumGridLinesHoricontal = 20;

        public int margin = 5; //space between UC border and its content
        public int padding = 5; //space between tracks
        public int trackheight = 15; //height of a track header block in pixel
        public Font trackFont = new Font("Courier New", 8);
        public int trackeventheight = 18; //height of a track event in pixel
        public Font trackeventFont = new Font("Courier New", 10);
        public Font headerFont = new Font("Courier New", 8);
        public float zoomrate = 0.2f; //defines the percentage (0..1) on what bigger or smaller the clip will be after changing one zoom level
        public bool showPopupOnTrackClick = true;

        private string _title;
        public string title
        {
            get { return _title; }
            set { _title = value; label_title.Text = _title; }
        }

        public event EventHandler<TimelineTrackEventClickArgs> TrackClicked;

        private DateTime timestamp_start_abs = DateTime.Now.AddDays(-1);
        private DateTime timestamp_end_abs = DateTime.Now.AddDays(+1);
        private TimeSpan timeSpan_abs;

        private DateTime timestamp_start_rel = DateTime.Now.AddDays(-1);
        private DateTime timestamp_end_rel = DateTime.Now.AddDays(+1);
        private TimeSpan timeSpan_rel;

        private Rectangle zoomRect;

        private Rectangle trackArea;
        private int trackAreaVrtHeight = 0;

        private ToolTip panelToolTip;
        private Point panelToolTipLastMouseLocation = new Point(0, 0);
        private DateTime panelToolTipLastShown = DateTime.Now;
        private int panelToolTipDelay = 1250; //333;

        private List<ToolTipAreaInformation> toolTipAreaInformation = new List<ToolTipAreaInformation>();


        private OutstandingOperation<int> isOffsetXOperation = new OutstandingOperation<int>();
        private int _offset_x = 0;
        private int offset_x
        {
            get { return _offset_x; }
            set
            {
                if (_offset_x == value)
                    return;

                isOffsetXOperation.Set(_offset_x, Math.Max(0, value)); //canot be negative

                _offset_x = isOffsetXOperation.newvalue;

                RefreshControl();
            }
        }

        private int _offset_y = 0;
        private int offset_y
        {
            get { return _offset_y; }
            set
            {
                if (_offset_y == value)
                    return;

                _offset_y = Math.Max(0, value);  //canot be negative

                RefreshControl();
            }
        }


        //zoom factor affect the x-achis ONLY
        private OutstandingOperation<float> isZoomChangeOperation = new OutstandingOperation<float>();
        private float _zoomfactor;  //1f == show the whole trace
        private float zoomfactor
        {
            get { return _zoomfactor; }
            set
            {
                if (_zoomfactor == value || value == 0 || value < 0.005d || value > 50d)
                    if (!isZoomChangeOperation.isOutstanding)  //if already there is an outstanding job, do not leave here
                        return;

                if (value >= 0.99f && value <= 1.01f)
                    value = 1f;

                isZoomChangeOperation.Set(_zoomfactor, value);

                _zoomfactor = isZoomChangeOperation.newvalue;

                RefreshControl();
            }
        }

        private double density = 0f; // means 1 pixel represents <density> seconds in the time line

        private List<vrtTimelineTrack> timelineTracks = new List<vrtTimelineTrack>();



        public TimeTraceUC()
        {
            InitializeComponent();

            //control init
            title = "Time Trace showing general events";

            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.UseTextForAccessibility
                    , true);

            //panel init
            thePanel.HorizontalScroll.Enabled = false;
            thePanel.VerticalScroll.Enabled = false;

            typeof(Panel).InvokeMember(
              "DoubleBuffered",
              BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
              null,
              thePanel,
              new object[] { true });

            thePanel.Paint += ThePanel_OnPaint;

            thePanel.Resize += new EventHandler((object o, EventArgs a) => {
                try
                {
                    RefreshControl();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });

            thePanel.MouseWheel += new MouseEventHandler((object o, MouseEventArgs args) => {
                try
                {
                    if (args.Delta != 0)
                    {
                        int mx = (-1.0f * args.Delta * SystemInformation.MouseWheelScrollLines / 120f).Int();
                        int dl = vScrollBar1.Value + (mx * vScrollBar1.SmallChange);
                        vScrollBar1.Value = Math.Max(vScrollBar1.Minimum, Math.Min(vScrollBar1.Maximum, dl));
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });

            thePanel.MouseMove += new MouseEventHandler((object o, MouseEventArgs args) => {
                try
                {
                    thePanel.Focus();

                    if (args.Button == MouseButtons.Left)
                    {
                        if (zoomRect.Location.X == 0 && zoomRect.Location.Y == 0)
                        {
                            zoomRect.Location = new Point(Math.Max(0, args.Location.X), trackArea.Top);
                            zoomRect.Size = new Size(0, trackArea.Height);
                                               
                        }
                        else
                        if (args.Location.X - zoomRect.Left > 0)
                            zoomRect.Size = new Size(args.Location.X - zoomRect.Left, trackArea.Height);
                        else
                        {
                            int oldX = zoomRect.Left;
                            zoomRect.Location = new Point(Math.Max(0, args.Location.X), trackArea.Top);
                            zoomRect.Size = new Size(oldX - args.Location.X, trackArea.Height);
                        }
                            

                        RefreshControl();
                    }

                    if (args.Location != panelToolTipLastMouseLocation && (DateTime.Now - panelToolTipLastShown).TotalMilliseconds > panelToolTipDelay)
                    {
                        string ttt = StringHelper.ShortenText(GetToolTipText(args.Location));
                        if (!string.IsNullOrEmpty(ttt))
                            panelToolTip.Show(ttt, this, args.Location.Move(16), 4000);

                        panelToolTipLastMouseLocation = args.Location;
                        panelToolTipLastShown = DateTime.Now;
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });

            thePanel.MouseUp += new MouseEventHandler((object o, MouseEventArgs args) =>
            {
                try
                {
                    if (zoomRect.X >= 0 && zoomRect.Y >= 0 && zoomRect.Width > 2)
                    {
                        timestamp_start_rel = GetClipXPosition(zoomRect.Left);
                        timestamp_end_rel = GetClipXPosition(zoomRect.Right);

                        if (timestamp_start_rel > timestamp_end_rel)
                        {
                            DateTime dmy = new DateTime(timestamp_end_rel.Ticks);
                            timestamp_end_rel = timestamp_start_rel;
                            timestamp_start_rel = dmy;
                        }

                        timeSpan_rel = timestamp_end_rel - timestamp_start_rel;

                    
                        RefreshControl();
                        zoomRect = new Rectangle(0, 0, 0, 0);
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });
            
            thePanel.MouseClick += new MouseEventHandler((object o, MouseEventArgs args) =>
            {
                try
                {
                    if (args.Y < trackArea.Y)
                        return;

                    panelToolTipLastMouseLocation = args.Location;

                    var tripelData = GetTrackDataFromLocationExt(args.Location); //<vrtTimelineTrack, vrtTimelineTrackEvent, string, TextMessage>
                    vrtTimelineTrack track = tripelData.Item1;
                    vrtTimelineTrackEvent trackEvt = tripelData.Item2;
                    string trackText = tripelData.Item3 ?? tripelData.Item4?.messageText;
                    TextMessage trackTextMsg = tripelData.Item4;

                    if (track == null)
                        return;


                    //fire click event
                    if (TrackClicked != null)
                    {
                        var evtData = new TimelineTrackEventClickArgs()
                        {
                            timelineTrackId = track.baseTrack.id,
                            timelineTrackEvent = trackEvt?.baseTrackEvent,
                            timelineTrackTextMessage = trackTextMsg,
                            timelineTrackText = trackText
                        };

                        TrackClicked(this, evtData);
                    }


                    //show popup
                    if (showPopupOnTrackClick && !string.IsNullOrEmpty(trackText))
                    {
                        TextBoxFrm tbform = new TextBoxFrm();
                        tbform.SetupLabel("Details ...");
                        tbform.SetupData(trackText);
                        tbform.ShowDialog();
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });


            //tooltip
            panelToolTip = new ToolTip();
            panelToolTip.AutomaticDelay = 3000;
            panelToolTip.InitialDelay = panelToolTipDelay;
            panelToolTip.ReshowDelay = 2500;


            //scroll bars init
            hScrollBar1.LargeChange = 25;
            hScrollBar1.SmallChange = 8;
            hScrollBar1.ValueChanged += new EventHandler((object o, EventArgs a) => {
                try
                {
                    offset_x = hScrollBar1.Value;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });

            vScrollBar1.LargeChange = 25;
            vScrollBar1.SmallChange = 8;
            vScrollBar1.ValueChanged += new EventHandler((object o, EventArgs a) => {
                try
                {
                    offset_y = vScrollBar1.Value;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });

            //buttons
            button_zoomAll.Click += new EventHandler((object o, EventArgs args) => {
                try
                {
                    isZoomChangeOperation.isOutstanding = true;
                    zoomfactor = 1f;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });
            var tt_button_zoomAll = new ToolTip();
            tt_button_zoomAll.SetToolTip(button_zoomAll, "zoom out and show all");

            button_zoomIn.Click += new EventHandler((object o, EventArgs args) => {
                zoomfactor += zoomrate;
            });
            var tt_button_zoomIn = new ToolTip();
            tt_button_zoomIn.SetToolTip(button_zoomIn, "zoom in");

            button_zoomOut.Click += new EventHandler((object o, EventArgs args) => {
                zoomfactor -= zoomrate;
            });
            var tt_button_zoomOut = new ToolTip();
            tt_button_zoomOut.SetToolTip(button_zoomOut, "zoom out");

            button_ShowAll.Click += new EventHandler((object o, EventArgs args) =>
            {
                try
                {
                    Form fm = new Form();

                    fm.Owner = Application.OpenForms[0];                
                    fm.Size = new Size(500, 300);
                    fm.Location = new Point(10, 10);
                    fm.ShowIcon = false;
                    fm.ShowInTaskbar = false;
                    fm.MinimizeBox = false;
                    fm.StartPosition = FormStartPosition.CenterScreen;
                    fm.WindowState = FormWindowState.Maximized;
                    fm.Text = "time trace full screen";

                    fm.Controls.Add(thePanel);
                    zoomfactor = 1f;
                    fm.ShowDialog();

                    this.tableLayoutPanel1.Controls.Add(this.thePanel, 0, 1);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });
            var tt_button_ShowAll = new ToolTip();
            tt_button_ShowAll.SetToolTip(button_ShowAll, "extract to own maximized window");

            button_exportGraph.Click += new EventHandler((object o, EventArgs args) =>
               {
                   try
                   {
                       ExportGraph();
                   }
                   catch (Exception e)
                   {
                       ExceptionHandler.Instance.HandleException(e);
                   }
               });
            var tt_button_Export = new ToolTip();
            tt_button_Export.SetToolTip(button_exportGraph, "Export this graph as image file");


            //key press events           
            thePanel.PreviewKeyDown += new PreviewKeyDownEventHandler( (object o, PreviewKeyDownEventArgs args) => 
            {
                try
                {
                    switch (args.KeyCode)
                    {
                        case Keys.OemMinus:
                        case Keys.Subtract:
                            zoomfactor -= zoomrate;
                            break;

                        case Keys.PageDown:
                            zoomfactor -= zoomrate * 3;
                            break;

                        case Keys.Oemplus:
                        case Keys.Add:
                            zoomfactor += zoomrate;
                            break;

                        case Keys.PageUp:
                            zoomfactor += zoomrate * 3;
                            break;

                        case Keys.Multiply:
                        case Keys.Zoom:
                            isZoomChangeOperation.isOutstanding = true;
                            zoomfactor = 1f;
                            break;
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });

 

            //init&refresh control and paint it
            zoomfactor = 1f;
            RefreshControl();
            thePanel.Focus(); 
        }

        public TimelineTrack AddBlockTrack(string id, string label, Color lineColor, Color trackbackColor1, Color trackbackColor2, Color eventBackColor1, Color eventBackColor2, Brush trackColorBrush = null, Brush trackeventColorbrush = null)
        {
            return AddTrack(id, label, TimelineTrackType.Block, lineColor, trackbackColor1, trackbackColor2, eventBackColor1, eventBackColor2, trackColorBrush, trackeventColorbrush);
        }

        public TimelineTrack AddPointTrack(string id, string label, Color lineColor)
        {
            return AddTrack(id, label, TimelineTrackType.Point, lineColor, Color.Black, Color.Black, Color.Black, Color.Black, null, null);
        }

        public TimelineTrack AddTrack(string id, string label, TimelineTrackType tracktype, Color lineColor, Color trackbackColor1, Color trackbackColor2, Color eventBackColor1, Color eventBackColor2, Brush trackColorBrush = null, Brush trackeventColorbrush = null)
        {
            TimelineTrack track = new TimelineTrack()
            {
                id = id,
                label = label,
                trackType = tracktype,
                lineColor = lineColor,
                trackColor1 = trackbackColor1,
                trackColor2 = trackbackColor2,
                trackColorbrush = trackColorBrush,
                trackeventColor1 = eventBackColor1,
                trackeventColor2 = eventBackColor2,
                trackeventColorbrush = trackeventColorbrush
            };

            var vrtTrack = vrtTimelineTrack.FromTimelineTrack(track);

            track.OnChangeTrackEventList += new EventHandler((object o, EventArgs e) =>
            {
                try
                {
                    var trackInput = (TimelineTrack)o;
                    var vrtTrackInput = timelineTracks.Where(t => t.baseTrack == trackInput).FirstOrDefault();

                    if (vrtTrackInput != null)
                    {
                        var vrtEventList = trackInput.trackEvents.Select(ev => vrtTimelineTrackEvent.FromTimelineTrackEvent(trackInput, ev));

                        vrtTrackInput.vrtTrackEvents = vrtEventList.ToList();

                        vrtTrackInput.RearrangeTrackevents();
                        vrtTrackInput.SetToolTipText(trackInput);
                    }

                    if (timelineTracks.Where(t => t.isValid).HasNoData() == false)
                    {
                        timestamp_start_abs = timelineTracks.Where(t => t.isValid).Min(tx => tx.baseTrack.trackStart);  
                        timestamp_end_abs = timelineTracks.Where(t => t.isValid).Max(tx => tx.baseTrack.trackEnd);
                        timeSpan_abs = timestamp_end_abs - timestamp_start_abs;
                    }

                    zoomfactor = 1f;
                    isZoomChangeOperation.isOutstanding = true;

                    RefreshControl();
                }
                catch (Exception exception)
                {
                    ExceptionHandler.Instance.HandleException(exception);
                }
            });


            timelineTracks.Add(vrtTrack);

            RefreshControl();

            return (track);
        }

        public TimelineTrack GetTrack(string id)
        {
            var tr = timelineTracks.FirstOrDefault(t => t.baseTrack.id == id);
            if (tr == null)
                throw new ArgumentException("no such track with id " + id);

            return (tr.baseTrack);
        }

        public void ExportGraph()
        {
            using (SaveFileDialog sg = new SaveFileDialog())
            {
                sg.DefaultExt = "png";
                sg.FileName = "Export.png";
                if (sg.ShowDialog() == DialogResult.OK)
                {
                    int h = trackheight * (timelineTracks.Count + margin * 2) + 100;
                    using (Bitmap bmp = new Bitmap(thePanel.Width, h))
                    {
                        Graphics gg = Graphics.FromImage(bmp);
                        gg.Clear(Color.White);
                        PaintImage(gg, true);

                        bmp.Save(sg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }

                    RefreshControl();
                }
            }
        }

        private void ThePanel_OnPaint(object sender, PaintEventArgs args)
        {
            try
            {
                RefreshVirtualClip();
                PanelPaint(args);
                RefreshVirtualClipPostPaint();
            }
            catch (Exception e)
            {
                ExceptionHandler.Instance.HandleException(e);
            }
        }

        private void RefreshControl()
        {
            thePanel.Invalidate(true);  //thePanel.Refresh();
        }

        private void RefreshVirtualClip()
        {
            //define the area where tracks can be printed and displayed (it's the physical area, not the virtual)
            trackArea = new Rectangle(margin,
                 minYPositionClipping,
                 thePanel.Width - (2 * margin),
                 thePanel.Height - minYPositionClipping - (2 * margin)
                 );

            if (isZoomChangeOperation.isOutstanding)
            {
                if (isZoomChangeOperation.newvalue == 1f)
                {
                    timestamp_start_rel = timestamp_start_abs;
                    timestamp_end_rel = timestamp_end_abs;
                    timeSpan_rel = timestamp_end_rel - timestamp_start_rel;

                    _offset_x = 0;

                    hScrollBar1.Enabled = false;
                }
                else //zoom into the center or out from the center of the relativ (clipped) area
                {
                    //float zoomChange = isZoomChangeOperation.oldvalue - isZoomChangeOperation.newvalue;
                    float zoomChange = isZoomChangeOperation.newvalue / isZoomChangeOperation.oldvalue;

                    double half = timeSpan_rel.TotalSeconds / 2d;
                    double half_newDelta = half - (half / zoomChange);

                    timestamp_start_rel = timestamp_start_rel.AddSeconds(half_newDelta);
                    timestamp_end_rel = timestamp_end_rel.AddSeconds(half_newDelta * -1d);

                    timeSpan_rel = timestamp_end_rel - timestamp_start_rel;

                    hScrollBar1.Enabled = zoomfactor > 1f;
                }

                isZoomChangeOperation.Done();
            }  // isZoomChangeOperation


            if (isOffsetXOperation.isOutstanding)
            {
                int deltaX = isOffsetXOperation.newvalue - isOffsetXOperation.oldvalue;

                //density == 1 px == density sec 
                double movingX_sec = deltaX * density;

                timestamp_start_rel = timestamp_start_rel.AddSeconds(movingX_sec);
                timestamp_end_rel = timestamp_end_rel.AddSeconds(movingX_sec);

                timeSpan_rel = timestamp_end_rel - timestamp_start_rel;

                isOffsetXOperation.Done();
            } //isOffsetXOperation


            //calculate density and scrollbar setup
            density = timeSpan_rel.TotalSeconds / trackArea.Width; //1px == <n> seconds 

        }

        private void RefreshVirtualClipPostPaint()
        {
            double vrtWidthAll = timeSpan_abs.TotalSeconds / density;  //this is the width in pixels the tracks need at current zoom level

            //recalc scrollbars

            if (vrtWidthAll <= trackArea.Width)  //in this case the whole track length fits the drawing area, so we can disable h-scrolling
                hScrollBar1.Enabled = false;

            if (hScrollBar1.Enabled)
            {
                hScrollBar1.Maximum = (vrtWidthAll - trackArea.Width).Int();

                _offset_x = ((timestamp_start_rel - timestamp_start_abs).TotalSeconds / density).IntDown();
                hScrollBar1.Value = MathHelper<int>.Within(_offset_x, hScrollBar1.Minimum, hScrollBar1.Maximum);
            }


            if (trackAreaVrtHeight > trackArea.Height)
            {
                vScrollBar1.Enabled = true;
                vScrollBar1.Maximum = trackAreaVrtHeight - trackArea.Height;
            }
            else
                vScrollBar1.Enabled = false;
        }

        private void PanelPaint(PaintEventArgs args)
        {
            PaintImage(args.Graphics);
        }

        private Graphics PaintImage(Graphics g, bool drawOverScreen = false)
        { 
            int w = trackArea.Width;   // thePanel.Width;// g.VisibleClipBounds.Width.Int();
            int h = trackArea.Height;  // thePanel.Height;// g.VisibleClipBounds.Height.Int();

            int le = trackArea.Left;
            int re = trackArea.Right;

            toolTipAreaInformation.Clear();

            try
            {
                StringFormat drawFormat = new StringFormat();
                SolidBrush drawBrush = new SolidBrush(Color.Black);
                Pen drawPen = new Pen(drawBrush);


                //display track background with major grid
                g.FillRectangle(new LinearGradientBrush(trackArea, Color.AntiqueWhite, Color.White, LinearGradientMode.ForwardDiagonal), trackArea);

                Dictionary<int, DateTime> majorVertGridLines = DateHelper
                                                                    .GetMeaningfulDateTimePoints(timestamp_start_rel, timestamp_end_rel, majorNumGridLinesHoricontal)  
                                                                    .ToDictionary(dt => GetClipXPosition(dt));

                g.FillRectangle(new LinearGradientBrush(trackArea, Color.AntiqueWhite, Color.White, LinearGradientMode.ForwardDiagonal), trackArea);
                Pen gridLinePen = new Pen(Color.DarkGreen);
                gridLinePen.DashStyle = DashStyle.Dash;

                foreach (int x in majorVertGridLines.Keys)
                    g.DrawLine(gridLinePen, x, trackArea.Top, x, trackArea.Bottom);


                //display tracks
                Rectangle rect;
                Brush br;

                int currentYposition = trackArea.Top - offset_y;

                foreach (var track in timelineTracks.Where(e => e.isValid && 
                                                                DateHelper.DateRangeInterferesWithRange(timestamp_start_rel, 
                                                                                                        timestamp_end_rel,
                                                                                                        e.baseTrack.trackStart, 
                                                                                                        e.baseTrack.trackEnd)))
                {
                    //draw the track group header when in block mode
                    if (track.baseTrack.trackType == TimelineTrackType.Block)
                    {
                        int posX = GetClipXPosition(track.baseTrack.trackStart);
                        rect = new Rectangle(
                                posX, currentYposition,
                                GetClipXPosition(track.baseTrack.trackEnd) - posX, trackheight);

                        track.drawRecangle = rect;
                        track.tooltipRectangle = rect.GetUnion(trackArea);

                        if (rect.IntersectsWith(trackArea) && !drawOverScreen)
                        {
                            track.isShown = true;

                            if (rect.Width > 2)
                            {
                                br = track.baseTrack.trackColorbrush ?? new LinearGradientBrush(rect, track.baseTrack.trackColor1, track.baseTrack.trackColor2, LinearGradientMode.ForwardDiagonal);
                                g.FillRectangle(br, rect);
                            }

                            g.DrawRectangle(new Pen(track.baseTrack.lineColor), rect);

                            if (rect.Width >= 5)
                                g.DrawString(track.baseTrack.label, trackFont, new SolidBrush(track.baseTrack.trackColor1.Complement()), rect, StringFormat.GenericDefault);
                        }
                        else
                            track.isShown = false;

                        currentYposition += trackheight + padding;
                    }

                    //draw the events
                    foreach (var trackevt in track.vrtTrackEvents.Where(e => DateHelper.DateRangeInterferesWithRange(timestamp_start_rel, timestamp_end_rel, e.baseTrackEvent.eventStart, e.baseTrackEvent.eventEnd)))
                    {
                        if (track.baseTrack.trackType == TimelineTrackType.Block)
                        {
                            int posX = GetClipXPosition(trackevt.baseTrackEvent.eventStart);
                            rect = new Rectangle(
                                    posX, currentYposition + trackevt.laneId * (trackeventheight + padding),
                                    GetClipXPosition(trackevt.baseTrackEvent.eventEnd) - posX, trackeventheight)
                                    .EnsureMinSize(1,1);

                            trackevt.drawRecangle = rect;
                            trackevt.tooltipRectangle = rect.GetUnion(trackArea);

                            if ((rect.IntersectsWith(trackArea) && !drawOverScreen) || drawOverScreen)
                            {
                                trackevt.isShown = true;
                                if (rect.Width > 0)
                                {
                                    br = track.baseTrack.trackeventColorbrush ?? new LinearGradientBrush(rect, track.baseTrack.trackeventColor1, track.baseTrack.trackeventColor2, LinearGradientMode.ForwardDiagonal);

                                    g.FillRectangle(br, rect);
                                }
                                g.DrawRectangle(new Pen(track.baseTrack.lineColor), rect);

                                if (rect.Width >= 5)                              
                                    g.DrawString(trackevt.baseTrackEvent.label, trackeventFont, new SolidBrush(track.baseTrack.trackeventColor1.Complement()), rect);                               
                            }
                            else
                                trackevt.isShown = false;
                        }
                        else  //event point
                        {
                            rect = new Rectangle(GetClipXPosition(trackevt.baseTrackEvent.eventStart)-1, currentYposition,
                                                  3, trackheight);

                            trackevt.drawRecangle = rect;
                            trackevt.tooltipRectangle = rect.GetUnion(trackArea);

                            if ((rect.IntersectsWith(trackArea) && !drawOverScreen) || drawOverScreen)
                            {
                                trackevt.isShown = true;
                                g.DrawRectangle(new Pen(track.baseTrack.lineColor), rect);
                            }
                            else
                                trackevt.isShown = false;

                        }

                        trackAreaVrtHeight = Math.Max(trackAreaVrtHeight, rect.Y + rect.Height);
                    }


                    currentYposition += trackeventheight + padding + (track.vrtTrackEvents.Max(t => t.laneId) * (trackeventheight + padding));
                }


                //Header
                Rectangle headerRect = new Rectangle(0, 0, thePanel.Width, trackArea.Top - 4);
                g.FillRectangle(new LinearGradientBrush(headerRect, Color.LightGray, Color.Gray, LinearGradientMode.Vertical), headerRect);

                //shadow
                var brShad = new SolidBrush(Color.FromArgb(150, Color.Black));
                List<Point> shadPoints = new List<Point>();
                shadPoints.Add(new Point(headerRect.X, headerRect.Bottom));
                shadPoints.Add(new Point(headerRect.Right, headerRect.Bottom));
                shadPoints.Add(new Point(headerRect.Right - 3, headerRect.Bottom + 3));
                shadPoints.Add(new Point(headerRect.X + 3, headerRect.Bottom + 3));
                shadPoints.Add(shadPoints[0]);
                g.FillPolygon(brShad, shadPoints.ToArray());


                //timestamps (left, center, right)
                bool showDate = true;
                string s_timestamp_start_rel = timestamp_start_rel.ToStringByReferenceDate(timestamp_end_rel, showDate);

                showDate = (timestamp_end_rel - timestamp_start_rel).TotalHours >= 24 || timestamp_start_rel.Hour > timestamp_end_rel.Hour;  //show date when diff > 24 or when midnight is passed
                string s_timestamp_end_rel = timestamp_end_rel.ToStringByReferenceDate(timestamp_start_rel, showDate);
                               
                SizeF fs = g.MeasureString(s_timestamp_end_rel, headerFont);
                int xPosLe = 0 + margin;
                int xPosRi = re - fs.Width.Int();
                int yPos = 0 + margin;  //not clipping area, because clipping area represents the drawing area below this header
                int ww = fs.Width.Int();
                int hh = fs.Height.Int();
                               
                //left
                g.DrawString(s_timestamp_start_rel, headerFont, drawBrush, xPosLe, yPos);

                //right
                g.DrawString(s_timestamp_end_rel, headerFont, drawBrush, xPosRi, yPos);

                //center
                var centerElem = majorVertGridLines.GetCenterElem();
                string s_timestamp_center = centerElem.Value.ToStringByReferenceDate(timestamp_end_rel, false);
                fs = g.MeasureString(s_timestamp_center, headerFont);
                g.DrawString(s_timestamp_center, headerFont, drawBrush, centerElem.Key - fs.Width / 2f, yPos);


                //debug
                //string debugString = string.Format("< ZoomF={0:F2}; Den={1:F2}; hScrollB: Val={2}; Max={3}; offsetX={4} >", zoomfactor, density, hScrollBar1.Value, hScrollBar1.Maximum, offset_x);
                //g.DrawString(debugString, headerFont, new SolidBrush(Color.Red), 122, yy);

                //vertical limit lines left & right
                float lineLength = yPos + fs.Height + 10;
                g.DrawLine(Pens.Black, le, yPos, le, yPos + lineLength);
                g.DrawLine(Pens.Black, re, yPos, re, yPos + lineLength);

                //arrow
                Point p_left = new Point(le, yPos + fs.Height.Int() + 10);
                Point p_rightend = new Point(re, p_left.Y);
                Point p_rightup = new Point(p_rightend.X - 12, p_rightend.Y - 5);
                Point p_rightdn = new Point(p_rightend.X - 12, p_rightend.Y + 5);

                g.DrawLine(drawPen, p_rightend, p_left);
                g.FillPolygon(drawBrush, new PointF[] { p_rightend, p_rightup, p_rightdn, p_rightend });

                //arrow grid
                foreach (var kp in majorVertGridLines)
                {
                    hh = kp.Key == centerElem.Key ? hh = (p_left.Y / 2f).Int() - 1 : 0;
                    g.DrawLine(gridLinePen, kp.Key, p_left.Y - hh, kp.Key, trackArea.Top);
                    rect = new Rectangle(kp.Key - 2, p_left.Y - 2, 5, 5);

                    g.FillEllipse(drawBrush, rect);
                    toolTipAreaInformation.Add(new ToolTipAreaInformation(rect, kp.Value.ToString("G")));
                }

                //zooming rect
                if (!drawOverScreen)
                    if (zoomRect.Location.X > 0 && zoomRect.Location.Y > 0 && zoomRect.Width > 0)
                        g.DrawRectangle(Pens.Red, zoomRect);

                //realize margin at buttom
                if (!drawOverScreen)
                    g.FillRectangle(new SolidBrush(this.BackColor), 0, trackArea.Bottom, thePanel.Width, thePanel.Height - trackArea.Bottom);

            }
            catch {}

            g.Flush();
            return g;
        }

        private int GetClipXPosition(DateTime dt)
        {
            //timestamp_start_rel -- trackArea.Left
            //timestamp_end_rel   --  trackArea.Right
            //dt -- ???

            var dtdelta = ((dt.Ticks - timestamp_start_rel.Ticks) * (trackArea.Right - trackArea.Left) / timeSpan_rel.Ticks) + trackArea.Left;

            return Convert.ToInt32(dtdelta);
        }

        private DateTime GetClipXPosition(int locX)
        {
            //timestamp_start_rel -- trackArea.Left
            //timestamp_end_rel   --  trackArea.Right
            //?? -- locX

            var ticks = ((locX - trackArea.Left) * (timeSpan_rel.Ticks) / (trackArea.Right - trackArea.Left)) + timestamp_start_rel.Ticks;

            return new DateTime(ticks);
        }

        private string GetToolTipText(Point p)
        {
            //try to find p in Control Header
            var ttt = toolTipAreaInformation.Where(tt => tt.tooltipArea.Contains(p)).FirstOrDefault();
            if (ttt != null)
                return ttt.tooltipText;

            //try to find p in track(event) objects
            var tripel = GetTrackDataFromLocation(p);            
            if (tripel.Item3 != null)
                return tripel.Item3;
            
            //bad luck
            return null;
        }

        private Tuple<vrtTimelineTrack, vrtTimelineTrackEvent, string> GetTrackDataFromLocation(Point p)
        {
            //track object
            var fndTr = timelineTracks.Where(tr => tr.isShown && tr.tooltipRectangle.Contains(p)).FirstOrDefault();
            if (fndTr != null)
                return new Tuple<vrtTimelineTrack, vrtTimelineTrackEvent, string>(fndTr, null, fndTr.toolTipText);
                        
            //track event object
            var fndTrEvt = timelineTracks.SelectMany(tr => tr.vrtTrackEvents.Where(te => te.isShown && te.tooltipRectangle.Contains(p))).FirstOrDefault();
            if (fndTrEvt != null)
            {
                var vrtTrack = timelineTracks.First(vTr => vTr.vrtTrackEvents.Any(a => a == fndTrEvt));
                return new Tuple<vrtTimelineTrack, vrtTimelineTrackEvent, string>(vrtTrack, fndTrEvt, fndTrEvt.toolTipText);
            }

            //return null tripel when input point hit no track (event) object
            return new Tuple<vrtTimelineTrack, vrtTimelineTrackEvent, string>(null, null, null);
        }

        private Tuple<vrtTimelineTrack, vrtTimelineTrackEvent, string, TextMessage> GetTrackDataFromLocationExt(Point p)  
        {
            //track object
            var fndTr = timelineTracks.Where(tr => tr.isShown && tr.tooltipRectangle.Contains(p)).FirstOrDefault();
            if (fndTr != null)
                return new Tuple<vrtTimelineTrack, vrtTimelineTrackEvent, string, TextMessage>(fndTr, null, fndTr.toolTipText, null);
    
            //track event object
            var fndTrEvt = timelineTracks.SelectMany(tr => tr.vrtTrackEvents.Where(te => te.isShown && te.tooltipRectangle.Contains(p))).FirstOrDefault();
            if (fndTrEvt != null)
            {
                var vrtTrack = timelineTracks.First(vTr => vTr.vrtTrackEvents.Any(a => a == fndTrEvt));
                return new Tuple<vrtTimelineTrack, vrtTimelineTrackEvent, string, TextMessage>(vrtTrack, fndTrEvt, fndTrEvt.toolTipText, fndTrEvt.textmsg);
            }

            //return null tripel when input point hit no track (event) object
            return new Tuple<vrtTimelineTrack, vrtTimelineTrackEvent, string, TextMessage>(null, null, null, null);
        }

       

        //========================================================================================================================================
        // internal classes
        //========================================================================================================================================
        internal class OutstandingOperation<T>
        {
            public T oldvalue;
            public T newvalue;

            public bool isOutstanding;


            public OutstandingOperation() { }

            public void Set(T oldval, T newVal)
            {
                oldvalue = oldval;
                newvalue = newVal;
                isOutstanding = true;
            }

            public void Done()
            {
                isOutstanding = false;
            }
        }

        //========================================================================================================================================
        internal class ToolTipAreaInformation
        {
            public string tooltipText;
            public Rectangle tooltipArea;

            public ToolTipAreaInformation(Rectangle areaSize, string text)
            {
                tooltipArea = areaSize;

                if (tooltipArea.Width == 0 || tooltipArea.Height == 0)
                    tooltipArea = new Rectangle(areaSize.Location, new Size(Math.Max(1, tooltipArea.Width), Math.Max(1, tooltipArea.Height)));

                tooltipText = text;
            }
        }

        //========================================================================================================================================
        internal class vrtTimelineBase
        {
            public string toolTipText = "";
            public Rectangle drawRecangle;
            public Rectangle tooltipRectangle;
            public bool isShown = false;
           
            public void SetToolTipText(TimelineTrack track)
            {
                string msg = track.label;

                toolTipText = string.Format("{0} - {1}\n{2}", track.trackStart.ToString("g"), track.trackEnd.ToString("g"), msg).Replace("\n", "\r\n");
            }

            public void SetToolTipText(TimelineTrack track, TimelineTrackEvent trackevent)
            {
                string msg = string.Format("{0}{2}{1}", trackevent.label, trackevent.additionalData, trackevent.additionalData != "" ? "\n\n" : "");

                if (track.trackType == TimelineTrackType.Block)
                    toolTipText =  string.Format("{0} - {1}\n{2}", trackevent.eventStart.ToString("g"), trackevent.eventEnd.ToString("g"), msg).Replace("\n", "\r\n");
                else
                    toolTipText = string.Format("{0}\n{1}", trackevent.eventStart.ToString("g"), msg).Replace("\n", "\r\n");
            }
        }

        //========================================================================================================================================
        internal class vrtTimelineTrack : vrtTimelineBase
        {
            public TimelineTrack baseTrack;
            public List<vrtTimelineTrackEvent> vrtTrackEvents = new List<vrtTimelineTrackEvent>();

            public bool isValid
            {
                get { return baseTrack.isValid; }
            }

            public vrtTimelineTrack() { }

            public static vrtTimelineTrack FromTimelineTrack(TimelineTrack track)
            {
                vrtTimelineTrack v = new vrtTimelineTrack();
                v.baseTrack = track;
                v.SetToolTipText(track);
                return v;
            }

            public void RearrangeTrackevents()
            {
                if (baseTrack.trackType == TimelineTrackType.Point)
                    return;

                Dictionary<int, Tuple<DateTime, DateTime>> lanes = new Dictionary<int, Tuple<DateTime, DateTime>>();
                byte curLaneId;

                foreach (var trEvt in vrtTrackEvents)
                {
                    //does the track event fit into a free place in a lane?
                    //var foundLane = lanes.Where(l => !trEvt.eventStart.InRange(l.Value.Item1, l.Value.Item2) && !trEvt.eventEnd.InRange(l.Value.Item1, l.Value.Item2));
                    var foundLane = lanes.Where(l => trEvt.baseTrackEvent.eventStart >= l.Value.Item2 || trEvt.baseTrackEvent.eventEnd <= l.Value.Item1).ToArray();  //we require: trEvt.eventEnd > trEvt.eventStart

                    if (foundLane.Length == 0)
                    {
                        if (lanes.Count == 255)  //no more than 255 lanes please!
                            curLaneId = 0; //warning: expect overlapping 
                        else
                        {
                            curLaneId = Convert.ToByte(lanes.Count);
                            lanes.Add(curLaneId, new Tuple<DateTime, DateTime>(baseTrack.trackStart, baseTrack.trackStart));
                        }
                    }
                    else
                        curLaneId = Convert.ToByte(foundLane.OrderBy(l => l.Key).First().Key);

                    //assign event to lane
                    trEvt.laneId = curLaneId;

                    //refresh lane 
                    lanes[curLaneId] = new Tuple<DateTime, DateTime>(
                        DateHelper.MinDt(lanes[curLaneId].Item1, trEvt.baseTrackEvent.eventStart),
                        DateHelper.MaxDt(lanes[curLaneId].Item2, trEvt.baseTrackEvent.eventEnd)
                        );
                }
            }
        }

        //========================================================================================================================================
        internal class vrtTimelineTrackEvent : vrtTimelineBase
        {
            public TimelineTrackEvent baseTrackEvent;
            public byte laneId = 0;
            public TextMessage textmsg;

            public vrtTimelineTrackEvent() { }

            public static vrtTimelineTrackEvent FromTimelineTrackEvent(TimelineTrack track, TimelineTrackEvent trackEvent)
            {
                vrtTimelineTrackEvent v = new vrtTimelineTrackEvent()
                {
                    baseTrackEvent = trackEvent,
                    textmsg = trackEvent.message
                };
                
                v.SetToolTipText(track, trackEvent);
                

                return v;
            }
        }

    }  //end of TimeTraceUC


    

    //========================================================================================================================================
    //data classes
    //========================================================================================================================================
    public enum TimelineTrackType
    {
        Point,
        Block
    }

    public class TimelineTrack
    {
        public string label = "";
        public string id;
        public TimelineTrackType trackType;

        public Color lineColor = Color.Black;

        public Brush trackColorbrush;
        public Color trackColor1;
        public Color trackColor2;

        public Brush trackeventColorbrush;
        public Color trackeventColor1;
        public Color trackeventColor2;

        public DateTime trackStart { get; private set; } = DateTime.MinValue;
        public DateTime trackEnd { get; private set; } = DateTime.MinValue;

        public List<TimelineTrackEvent> trackEvents = new List<TimelineTrackEvent>();

        public bool isValid
        {
            get {
                return trackEvents.Count > 0 && !trackStart.IsNull() && !trackEnd.IsNull();
                }
        }

        public event EventHandler OnChangeTrackEventList;

        public TimelineTrack()
        { }

        public void AddTrackEvents(List<TimelineTrackEvent> trackevents)
        {
            var trackeventsReal = trackevents.Where(t => t != null && !t.eventStart.IsNull() && !t.eventEnd.IsNull()).ToList();
            
            if (trackeventsReal.Count > 0)
            {
                trackEvents.AddRange(trackeventsReal);

                trackStart = trackEvents.Min(t => t.eventStart);
                trackEnd = trackEvents.Max(t => t.eventEnd);
            }

            OnChangeTrackEventList?.Invoke(this, new EventArgs());
        }

        public void AddTrackEvent(TimelineTrackEvent trackevent)
        {
            if (trackevent == null || trackevent.eventStart.IsNull() || trackevent.eventEnd.IsNull())
                return;

            var l = new List<TimelineTrackEvent>();
            l.Add(trackevent);
            AddTrackEvents(l);
        }        
    }


    //========================================================================================================================================
    public class TimelineTrackEvent
    {
        public string label;
        public string additionalData;
        public DateTime eventStart;
        public DateTime eventEnd;  //optional
        public int durationMinutes;
        public TextMessage message;
        

        public TimelineTrackEvent(string label, DateTime dtStart, DateTime dtEnd, TextMessage textmessage = null, string additionalData = "")
        {
            this.label = label;
            this.eventStart = dtStart;
            this.eventEnd = dtEnd;
            this.additionalData = additionalData;
            this.message = textmessage;
            durationMinutes = Convert.ToInt32((dtEnd - dtStart).TotalMinutes);

            if (eventStart.IsNull() || eventEnd.IsNull())
                throw new ArgumentOutOfRangeException($"Event start time ({eventStart}) or end time ({eventEnd}) out of expected range");
        }

        public TimelineTrackEvent(string label, DateTime dtStart, TextMessage textmessage = null, string additionalData = "", double durationMin = 0d)
        {
            this.label = label;
            this.eventStart = dtStart;
            this.eventEnd = dtStart.AddMinutes(durationMin);
            this.additionalData = additionalData;
            durationMinutes = Convert.ToInt32(durationMin);
            this.message = textmessage;

            if (eventStart.IsNull() || eventEnd.IsNull())
                throw new ArgumentOutOfRangeException($"Event start time ({eventStart}) or end time ({eventEnd}) out of expected range");
        }
    }


    //========================================================================================================================================
    public class TimelineTrackEventClickArgs : EventArgs
    {
        public string timelineTrackId;
        public TimelineTrackEvent timelineTrackEvent;
        public string timelineTrackText;
        public TextMessage timelineTrackTextMessage;
    }

    
}
