using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WIP_Management_System
{
    public partial class frmLoading : Form
    {
        public frmLoading()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // Prevent flickering
            this.SetStyle(ControlStyles.ResizeRedraw, true); // Redraw on resize
        }

        int second = 0;
        bool move;
        int x, y;

        private void frmLoading_MouseUp(object sender, MouseEventArgs e)
        {
            move = false;
        }

        private void frmLoading_MouseDown(object sender, MouseEventArgs e)
        {
            move = true;
            x = e.X;
            y = e.Y;
        }

        private void frmLoading_MouseMove(object sender, MouseEventArgs e)
        {
            if (move)
            {
                this.DesktopLocation = new Point(MousePosition.X - x, MousePosition.Y - y);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            second++;
            pnl_ProgressBar.Left += 2;
            if (pnl_ProgressBar.Left + pnl_ProgressBar.Width > pnl_Center.Width + 70)
            {
                pnl_ProgressBar.Left = 0;
            }

            if (second == 300)
            {
                LoginForm loginform = new LoginForm();
                timer1.Stop();
                this.Hide();
                loginform.Show();
            }
        }

        //setDesktopLocation(MousePosition.X - x, MousePosition.Y - y);



        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            GraphicsPath path = new GraphicsPath();
            int arcRadius = 20; // Adjust this value to change the roundness
            path.AddArc(new Rectangle(0, 0, arcRadius * 2, arcRadius * 2), 180, 90);
            path.AddArc(new Rectangle(this.Width - arcRadius * 2, 0, arcRadius * 2, arcRadius * 2), 270, 90);
            path.AddArc(new Rectangle(this.Width - arcRadius * 2, this.Height - arcRadius * 2, arcRadius * 2, arcRadius * 2), 0, 90);
            path.AddArc(new Rectangle(0, this.Height - arcRadius * 2, arcRadius * 2, arcRadius * 2), 90, 90);
            path.CloseFigure();
            this.Region = new Region(path);
        }


    }
}
