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
    public partial class LoginForm : Form
    {

        public LoginForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // Prevent flickering
            this.SetStyle(ControlStyles.ResizeRedraw, true); // Redraw on resize
            this.Region = CreateRoundedRegion(this.ClientRectangle, 20); // Rounded corners

        }

        int second = 0;
        bool move;
        int x, y;

        private void btn_Login_MouseHover(object sender, EventArgs e)
        {
            btn_Login.ForeColor = Color.Black; // Change the color
        }

        private void btn_Login_MouseLeave(object sender, EventArgs e)
        {
            btn_Login.ForeColor = Color.White; // Change the color
        }

        private void pnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            move = true;
            x = e.X;
            y = e.Y;
        }

        private void pnlHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (move)
            {
                int newX = this.Left + (e.X - x);
                int newY = this.Top + (e.Y - y);
                this.Location = new Point(newX, newY);
            }
        }

        private void pnlHeader_MouseUp(object sender, MouseEventArgs e)
        {
            move = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btn_Login_Click(object sender, EventArgs e)
        {
            if (txt_UserName.Text == "admin" && txt_Password.Text == "admin")
            {
                this.Hide();
                HomeForm homeForm = new HomeForm();
                homeForm.Show();
            }
            else
            {
                MessageBox.Show("Invalid username or password");
            }
        }

        // Method to create a rounded region
        private Region CreateRoundedRegion(Rectangle rectangle, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rectangle.X, rectangle.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rectangle.Right - radius * 2, rectangle.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rectangle.Right - radius * 2, rectangle.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rectangle.X, rectangle.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return new Region(path);
        }


    }
}
