using System.Drawing.Imaging;

namespace ProjectileMotion
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private class Path
        {
            public double v0 = 0d;
            public double h0 = 0d;
            public double a = 0d;
            public Color c = Color.Black;

            public Path(double v0, double a, double h0, Color c)
            {
                this.v0 = v0;
                this.h0 = h0;
                this.a = a * Math.PI / 180d;
                this.c = c;
            }
        }

        private Size displaySize;
        private List<Path> paths = new List<Path>();
        private Font f = new Font(FontFamily.GenericSerif, 12);

        private const double g = 9.80665d;
        private double scaleX = 10;                     // 10 px : 1 unit
        private double scaleY = 10;                     // 10 px : 1 unit
        private int axisPadding = 30;                   // distance from form edge to axis

        private void DrawPaths()
        {
            foreach (Path path in paths)
            {
                double x, y;
                double v0 = path.v0;
                double h0 = path.h0;
                double a = path.a;
                Color c = path.c;
                PointF lastP = new PointF(axisPadding, (float)(pb_display.Height - axisPadding - path.h0 * scaleY));
                PointF tP;
                Image temp = pb_display.Image;

                using (Graphics gr = Graphics.FromImage(temp))
                {
                    using (Pen p = new Pen(c, 2))
                    {
                        for (double i = axisPadding; i < pb_display.Width; i++)
                        {
                            x = (i - axisPadding) / scaleX;
                            y = h0 + x * Math.Tan(a) - (g * x * x) / (2 * v0 * v0 * Math.Cos(a) * Math.Cos(a));

                            if (y < 0)
                                break;

                            tP = new PointF((float)(x * scaleX + axisPadding), (float)(pb_display.Height - axisPadding - y * scaleY));
                            gr.DrawLine(p, lastP, tP);
                            lastP = tP;
                        }
                    }
                }

                pb_display.Image = temp;
                pb_display.Invalidate();
            }
        }

        private void DrawLegend()
        {
            if (chb_legend.Checked)
            {
                Image temp = pb_display.Image;
                int w = 0;

                if (this.WindowState == FormWindowState.Maximized)
                    f = new Font(FontFamily.GenericSerif, 16);

                if (chb_L_v0.Checked)
                    w += TextRenderer.MeasureText("V₀: 100.00m/s".ToString(), f).Width + 5;
                if (chb_L_angle.Checked)
                    w += TextRenderer.MeasureText("α: -90°".ToString(), f).Width + 5;
                if (chb_L_h0.Checked)
                    w += TextRenderer.MeasureText("h₀: 100.00m".ToString(), f).Width + 5;

                Point p = new Point(pb_display.Width - w, 0);
                using (Graphics g = Graphics.FromImage(temp))
                {
                    int padding = 5;
                    int y = padding;

                    foreach (Path path in paths)
                    {
                        string str = String.Empty;

                        if (chb_L_v0.Checked)
                            str += ("V₀: " + Math.Round(path.v0, 2) + "m/s").PadRight(14);
                        if (chb_L_angle.Checked)
                            str += ("α: " + (int)Math.Round(path.a * 180 / Math.PI) + "°").PadRight(10);
                        if (chb_L_h0.Checked)
                            str += "h₀: " + Math.Round(path.h0, 2) + "m";

                        g.DrawString(str, f, new SolidBrush(path.c), new Point(p.X + padding, y));
                        y += (int)f.Size + 2;
                    }
                }

                pb_display.Image = temp;
                pb_display.Invalidate();
            }
        }

        private void ClearDisplayImage(Color c)
        {
            double P0d = 4;                                                                                     // diameter of coordinate origin point
            double lh = 6;                                                                                      // tickmark line height
            Font f = new Font(FontFamily.GenericSerif, 14);                                                     // font

            if (this.WindowState == FormWindowState.Maximized)
            {
                f = new Font(FontFamily.GenericSerif, 18);
                axisPadding = 40;
                lh = 8;
                P0d = 6;
            }

            Point P0 = new Point(axisPadding, pb_display.Height - axisPadding);                                 // coordinate origin
            Point X0 = new Point(pb_display.Width, pb_display.Height - axisPadding);                            // end of X axis
            Point Y0 = new Point(axisPadding, 0);                                                               // end of Y axis

            Image temp = new Bitmap(displaySize.Width, displaySize.Height);                                     // new clean image
            using (Graphics g = Graphics.FromImage(temp))
            {
                g.Clear(c);

                using (Brush b = new SolidBrush(Color.Black))
                {
                    using (Pen p = new Pen(Color.Black))
                    {
                        #region Draw axis
                        g.DrawLine(p, P0, X0);                                                                  // X axis
                        g.DrawLine(p, P0, Y0);                                                                  // Y axis
                        g.DrawString("0", f, b, 15, pb_display.Height - 30);                                    // coordinate origin label
                        g.DrawString("X", f, b, pb_display.Width - 25, pb_display.Height - 25);                 // X axis label
                        g.DrawString("Y", f, b, 5, 5);                                                          // Y axis label
                        g.FillPie(b, (int)(P0.X - P0d / 2), (int)(P0.Y - P0d / 2), (int)P0d, (int)P0d, 0, 360); // coordinate origin point

                        for (double i = P0.X; i < X0.X; i += scaleX)                                            // X axis tickmarks
                        {
                            int t = (int)((i - P0.X) / scaleX);

                            if (t % 10 == 0 && i != P0.X)
                            {
                                if (scaleX >= 2.7)
                                    p.Width = (float)(2d * lh / 3d);
                                else
                                    p.Width = (float)(lh / 3d);
                            }
                            else if (t % 5 == 0 && scaleX >= 2.7)
                                p.Width = (float)(lh / 3d);

                            if (t != 0 && (scaleX > 20 ||
                                scaleX >= 6 && scaleX < 20 && t % 5 == 0 ||
                                scaleX >= 2.7 && scaleX < 6 && t % 10 == 0 ||
                                scaleX >= 2 && scaleX < 2.7 && t % 20 == 0 ||
                                scaleX >= 1 && scaleX < 2 && t % 50 == 0) &&
                                TextRenderer.MeasureText(t.ToString(), f).Width + (float)i - 13 < pb_display.Width - 25)
                                g.DrawString(t.ToString(), f, b, (float)i - 13, (float)(pb_display.Height - 25));

                            if (!(scaleX < 2 && p.Width > 1))
                                g.DrawLine(p, (int)i, (int)(P0.Y - lh / 2), (int)i, (int)(P0.Y + lh / 2));

                            p.Width = (float)(lh / 6d);
                        }

                        for (double i = P0.Y; i > Y0.Y; i -= scaleY)                                            // Y axis tickmarks
                        {
                            int t = (int)((i - P0.Y) / scaleY);

                            if (t % 10 == 0 && i != P0.Y)
                            {
                                if (scaleY >= 2.7)
                                    p.Width = 4;
                                else
                                    p.Width = 4;
                            }
                            else if (((i - P0.Y) / scaleY) % 5 == 0)
                                p.Width = 2;


                            if (t != 0 && (scaleY > 20 ||
                                scaleY >= 6 && scaleY < 20 && t % 5 == 0 ||
                                scaleY >= 2.7 && scaleY < 6 && t % 10 == 0 ||
                                scaleY >= 2 && scaleY < 2.7 && t % 20 == 0 ||
                                scaleY >= 1 && scaleY < 2 && t % 50 == 0) &&
                                TextRenderer.MeasureText("Y", f).Height + 5 < (float)i - 10)
                                g.DrawString(((int)((i - P0.Y) / -scaleY)).ToString(), f, b, 0, (float)i - 10);

                            g.DrawLine(p, (int)(P0.X - lh / 2), (int)i, (int)(P0.X + lh / 2), (int)i);
                            p.Width = 1;
                        }
                        #endregion
                    }
                }

                pb_display.Image = temp;
            }
        }

        private void ClearDisplayImage()
        {
            ClearDisplayImage(Color.White);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            displaySize = new Size(pb_display.Width, pb_display.Height);
            ClearDisplayImage();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            displaySize = new Size(pb_display.Width, pb_display.Height);
            ClearDisplayImage();
            DrawPaths();
            DrawLegend();
        }

        private void num_angle_ValueChanged(object sender, EventArgs e)
        {
            trackBar_angle.Value = (int)num_angle.Value;
        }

        private void trackBar_angle_Scroll(object sender, EventArgs e)
        {
            num_angle.Value = trackBar_angle.Value;
        }

        private void btn_zoomPlus_Click(object sender, EventArgs e)
        {
            if (scaleX <= 400)
            {
                scaleX *= 1.25;
                scaleY *= 1.25;
            }

            ClearDisplayImage();
            DrawPaths();
            DrawLegend();
        }

        private void btn_zoomMinus_Click(object sender, EventArgs e)
        {
            if (scaleX >= 1.5)
            {
                scaleX /= 1.25;
                scaleY /= 1.25;
            }

            ClearDisplayImage();
            DrawPaths();
            DrawLegend();
        }

        private void btn_zoomReset_Click(object sender, EventArgs e)
        {
            scaleX = 10;
            scaleY = 10;

            ClearDisplayImage();
            DrawPaths();
            DrawLegend();
        }

        private void btn_draw_Click(object sender, EventArgs e)
        {
            paths.Add(new Path(Convert.ToDouble(num_v0.Value), Convert.ToDouble(num_angle.Value), Convert.ToDouble(num_h0.Value), btn_color.BackColor));

            ClearDisplayImage();
            DrawPaths();
            DrawLegend();
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            paths.Clear();
            ClearDisplayImage();
        }

        private void btn_color_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
                if (cd.ShowDialog() == DialogResult.OK)
                    btn_color.BackColor = cd.Color;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = ".jpg";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pb_display.Image.Save(dialog.FileName, ImageFormat.Jpeg);
                MessageBox.Show("Image successfully saved!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void chb_legend_CheckedChanged(object sender, EventArgs e)
        {
            chb_L_angle.Enabled = chb_legend.Checked;
            chb_L_v0.Enabled = chb_legend.Checked;
            chb_L_h0.Enabled = chb_legend.Checked;
        }

        private void UpdateLegend(object sender, EventArgs e)
        {
            ClearDisplayImage();
            DrawPaths();
            DrawLegend();
        }
    }
}