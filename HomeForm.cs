using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WIP_Management_System.Properties;
using WIP_Management_System.TransForms;

namespace WIP_Management_System
{
    public partial class HomeForm : Form
    {
        //Fields
        private bool isCollapsed;
        private Form activeForm;

        public HomeForm()
        {
            InitializeComponent();
            btnCloseChildFrm.Visible = false;
            this.SetStyle(ControlStyles.ResizeRedraw, true); // Redraw on resize
            this.Region = CreateRoundedRegion(this.ClientRectangle, 20); // Rounded corners
            this.ControlBox = false;// Remove the default title bar and control box
            this.Text = String.Empty;// Remove the default title bar and control box
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea; //

        }

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isCollapsed)
            {
                btnTransactions.Image = Resources.Collapse_Arrow_Small;
                pnlTransactions.Height += 10;

                if (pnlTransactions.Size == pnlTransactions.MaximumSize)
                {
                    timer1.Stop();
                    isCollapsed = false;
                }

            }
            else
            {
                btnTransactions.Image = Resources.Expand_Arrow_small;

                pnlTransactions.Height -= 10;

                if (pnlTransactions.Size == pnlTransactions.MinimumSize)
                {
                    timer1.Stop();
                    isCollapsed = true;
                }

            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (isCollapsed)
            {
                btnReports.Image = Resources.Collapse_Arrow_Small;
                pnlReports.Height += 10;

                if (pnlReports.Size == pnlReports.MaximumSize)
                {
                    timer2.Stop();
                    isCollapsed = false;
                }

            }
            else
            {
                btnReports.Image = Resources.Expand_Arrow_small;
                pnlReports.Height -= 10;

                if (pnlReports.Size == pnlReports.MinimumSize)
                {
                    timer2.Stop();
                    isCollapsed = true;
                }

            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (isCollapsed)
            {
                btnMasterFiles.Image = Resources.Collapse_Arrow_Small;

                pnlMasterFiles.Height += 10;

                if (pnlMasterFiles.Size == pnlMasterFiles.MaximumSize)
                {
                    timer3.Stop();
                    isCollapsed = false;
                }

            }
            else
            {
                btnMasterFiles.Image = Resources.Expand_Arrow_small;

                pnlMasterFiles.Height -= 10;

                if (pnlMasterFiles.Size == pnlMasterFiles.MinimumSize)
                {
                    timer3.Stop();
                    isCollapsed = true;
                }

            }
        }


        private void btnTransactions_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            timer2.Start();
        }

        private void btnMasterFiles_Click(object sender, EventArgs e)
        {
            timer3.Start();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OpenChildForm(Form childForm, object btnSender)
        {  
            if (activeForm != null)
                activeForm.Close();
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            this.pnlFormsDisplay.Controls.Add(childForm);
            this.pnlFormsDisplay.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
            lblTitle.Text = childForm.Text;

        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new HForm.Dashboardfrm(), sender);
        }


        private void Reset()
        {
            lblTitle.Text = "Welcome!";
            btnCloseChildFrm.Visible = false;
        }


        private void pnlHeader_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void pnlHeader_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void pnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
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

        private void btnMaxAndNormal_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                this.Region = null;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.Region = CreateRoundedRegion(this.ClientRectangle, 20);
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new ProfForm.ProfileForm(), sender);
        }

        private void btnRecordTS_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new TransForms.TimeSheetForm(), sender);
        }

        private void btnRecordOtherExp_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new TransForms.OtherExpensesForm(), sender);
        }

        private void btnProcessTraveling_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new TransForms.TravelingProcessForm(), sender);
        }


        private void btnRepoWIP_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new RepoForms.WIPReportForm(), sender);
        }

        private void btnRepoWIP_P_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new RepoForms.WIPReportPartner(), sender);
        }

        private void btnRepoWIP_M_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new RepoForms.WIPReportManager(), sender);
        }

        private void btnRepoTravelingCost_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new RepoForms.TravelingCostForm(), sender);
        }

        private void btnRepoOtherExp_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new RepoForms.OtherExpensesForm(), sender);
        }

        private void btnMasterEmployees_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new MasterFilesForms.EmployeeDetailsForm(), sender);
        }

        private void btnMasterCustomers_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new MasterFilesForms.Customers(), sender);
        }

        private void btnMasterLocations_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new MasterFilesForms.Locations(), sender);
        }

        private void btnMasterDesignations_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new MasterFilesForms.Designations(), sender);
        }

        private void btnMasterDepartments_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new MasterFilesForms.Departments(), sender);
        }

        private void btnMasterWorkTypes_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new MasterFilesForms.WorkTypes(), sender);
        }

        private void btnCloseChildFrm_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = false;
            if (activeForm != null)
                activeForm.Close();
            Reset();
        }

        private void btnJobMaster_Click(object sender, EventArgs e)
        {
            btnCloseChildFrm.Visible = true;
            OpenChildForm(new TransForms.JobMasterForm(), sender);
        }
    }
}
