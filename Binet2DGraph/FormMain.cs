using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Security.Policy;
using System.Windows.Forms;

// https://www.youtube.com/watch?v=ghxQA3vvhsk
// (((1+sqrt(5))/2)^n - ((1-sqrt(5))/2)^n)/sqrt(5)

// https://www.geogebra.org/m/ypqcuqcs

namespace Binet2DGraph {
    public partial class FormMain : Form {
        private static readonly double s5 = Math.Sqrt(5.0);
        private static readonly double phi = (1.0 + s5) / 2.0;

        private Pen axesColor = Pens.DimGray;
        private Pen ticksColor = Pens.DarkGray;

        private static float zx = 50.0f; // Scale X axis
        private static float zy = 50.0f; // Scale Y axis
        private static float maxIter = 30.0f;
        private static float iterStep = 0.05f;
        private PointF offset = new PointF(-zx * 5, 0);

        public FormMain() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);

            this.Paint += DrawGraph;

            this.KeyDown += (object s, KeyEventArgs e) => {
                switch(e.KeyCode) {
                    case Keys.Add:
                        zx *= 1.2f;
                        zy *= 1.2f;
                        this.Invalidate();
                        break;
                    case Keys.Subtract:
                        zx /= 1.2f;
                        zx /= 1.2f;
                        this.Invalidate();
                        break;
                    case Keys.Up:
                        offset.Y += 10;
                        this.Invalidate();
                        break;
                    case Keys.Down:
                        offset.Y -= 10;
                        this.Invalidate();
                        break;
                    case Keys.Left:
                        offset.X += 10;
                        this.Invalidate();
                        break;
                    case Keys.Right:
                        offset.X -= 10;
                        this.Invalidate();
                        break;
                }
            };
        }

        private void DrawGraph(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            Rectangle r = this.DisplayRectangle;
            float zy10 = zy * 10.0f; // Constant positive imaginary values zoom
                                     //float z10 = 50.0f / z * 500.0f; // Increase vertical zoom for 
                                     // positive imaginary values while zooming out

            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.TranslateTransform(r.Width / 2 + offset.X, r.Height / 2 + offset.Y);
            g.ScaleTransform(1, -1);

            DrawAxes(g, r);

            Complex b;
            List<PointF> psp = new List<PointF>();
            List<PointF> psn = new List<PointF>();
            for(double n = 0; n < maxIter; n += iterStep) {
                b = Binet(n);
                AddPoint(psp, r, (float)b.Real * zx, (float)b.Imaginary * zy10);

                b = Binet(-n);
                AddPoint(psn, r, (float)b.Real * zx, (float)b.Imaginary * zy);
            }

            g.DrawCurve(Pens.DeepSkyBlue, psp.ToArray());
            g.DrawCurve(Pens.YellowGreen, psn.ToArray());

            DrawTicks(g, r);
        }

        private void AddPoint(List<PointF> pts, Rectangle r, float x, float y) {
            pts.Add(new PointF(Math.Min(x, r.Width - offset.X),
                               Math.Min(y, r.Height + offset.Y)));
        }

        private void DrawAxes(Graphics g, Rectangle r) {
            g.DrawLine(axesColor, -r.Width / 2 - offset.X, 0, r.Width / 2 - offset.X, 0);
            g.DrawLine(axesColor, 0, r.Height / 2 + offset.Y, 0, -r.Height / 2 + offset.Y);
        }

        private void DrawTicks(Graphics g, Rectangle r) {
            g.ScaleTransform(1, -1);

            int fh = this.Font.Height;
            float xy = 0;
            float lastXY = 0;
            int v = 1;

            xy = zx;
            while(xy - Math.Abs(offset.X) < r.Width) {
                if((xy - lastXY) > fh) {
                    g.DrawLine(ticksColor, +xy, 5, +xy, -5);
                    g.DrawLine(ticksColor, -xy, 5, -xy, -5);
                    g.DrawString(v.ToString(), this.Font, Brushes.White, xy - 4, fh - 5);
                    g.DrawString((-v).ToString(), this.Font, Brushes.White, -xy - 8, fh - 5);

                    // TODO: Improve this formula... we can do better
                    lastXY = xy + ((-v).ToString().Length * fh / 2);
                }
                xy += zx;
                v++;
            }

            lastXY = 0;
            v = 1;
            xy = zy;
            while(xy - Math.Abs(offset.Y) < r.Height) {
                if((xy - lastXY) > fh) {
                    g.DrawLine(ticksColor, -5, +xy, 5, +xy);
                    g.DrawLine(ticksColor, -5, -xy, 5, -xy);
                    g.DrawString(v.ToString(), this.Font, Brushes.White, 8, -xy - fh / 2);
                    g.DrawString((-v).ToString(), this.Font, Brushes.White, 8, xy - fh / 2);

                    // TODO: Improve this formula... we can do better
                    lastXY = xy + fh;
                }

                xy += zy;
                v++;
            }
        }

        public static Complex Binet(Complex n) {
            return ((Complex.Pow(phi, n) - Complex.Pow(-1.0 / phi, n)) / s5);
        }
    }
}