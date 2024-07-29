using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace WIP_Management_System.MasterFilesForms
{
    public partial class WorkTypes : Form
    {
        string connectionString = "Data Source=CHARAKA-PC\\SQLEXPRESS;Initial Catalog=WIP_Sys;Integrated Security=True;";
        private bool userIsInteracting = false;

        public WorkTypes()
        {
            InitializeComponent();
            LoadDepartments();
            LoadWorkTypes();

            // Add event handlers
            dataGridViewWorkTypes.SelectionChanged += dataGridViewWorkTypes_SelectionChanged;
            dataGridViewWorkTypes.MouseDown += dataGridViewWorkTypes_MouseDown;
            //dataGridViewWorkTypes.ClearSelection();

        }

        private void LoadDepartments()
        {
            string query = "SELECT DepartmentID, DepartmentName FROM Departments WHERE IsActive = 1 ORDER BY DepartmentName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    DataRow topItem = dt.NewRow();
                    topItem[0] = 0;
                    topItem[1] = "Select Department";
                    dt.Rows.InsertAt(topItem, 0);

                    combDepartment.DisplayMember = "DepartmentName";
                    combDepartment.ValueMember = "DepartmentID";
                    combDepartment.DataSource = dt;
                }
            }
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                if (!IsDuplicateWorkType())
                {
                    InsertWorkType();
                    ClearFormFields();
                    LoadWorkTypes();
                }
                else
                {
                    MessageBox.Show("A Work Type with the same name already exists in the selected department.");
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtWorkTypeName.Text))
            {
                MessageBox.Show("Work Type Name is required.");
                return false;
            }

            if (combDepartment.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a department.");
                return false;
            }

            if (!radioButtonActive.Checked && !radioButtonInactive.Checked)
            {
                MessageBox.Show("Please select the status (Active/Inactive).");
                return false;
            }

            return true;
        }

        private bool IsDuplicateWorkType()
        {
            int departmentID = (int)combDepartment.SelectedValue;
            string workTypeName = txtWorkTypeName.Text;

            string query = "SELECT COUNT(*) FROM WorkTypes WHERE WorkTypeName = @WorkTypeName AND DepartmentID = @DepartmentID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@WorkTypeName", workTypeName);
                command.Parameters.AddWithValue("@DepartmentID", departmentID);

                try
                {
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                    return true; // Assume duplicate to prevent insertion on error
                }
            }
        }

        private void InsertWorkType()
        {
            int departmentID = (int)combDepartment.SelectedValue;
            string workTypeName = txtWorkTypeName.Text;
            bool isActive = radioButtonActive.Checked;
            bool isBillable = checkBoxBillable.Checked;
            int workTypeID = GenerateWorkTypeID(departmentID);

            string query = "INSERT INTO WorkTypes (WorkTypeID, WorkTypeName, DepartmentID, IsActive, IsBillable) VALUES (@WorkTypeID, @WorkTypeName, @DepartmentID, @IsActive, @IsBillable)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@WorkTypeID", workTypeID);
                command.Parameters.AddWithValue("@WorkTypeName", workTypeName);
                command.Parameters.AddWithValue("@DepartmentID", departmentID);
                command.Parameters.AddWithValue("@IsActive", isActive);
                command.Parameters.AddWithValue("@IsBillable", isBillable);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show($"Work Type added successfully with ID: {workTypeID}");

                    // Update comboBoxWorkTypeID
                    comboBoxWorkTypeID.Items.Add(workTypeID);
                    comboBoxWorkTypeID.SelectedItem = workTypeID;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private int GenerateWorkTypeID(int departmentID)
        {
            string query = "SELECT ISNULL(MAX(WorkTypeID), @BaseID) FROM WorkTypes WHERE DepartmentID = @DepartmentID";
            int baseID = departmentID * 100;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DepartmentID", departmentID);
                command.Parameters.AddWithValue("@BaseID", baseID);

                try
                {
                    connection.Open();
                    int maxWorkTypeID = (int)command.ExecuteScalar();
                    return maxWorkTypeID + 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                    return baseID + 1; // Default to baseID + 1 on error
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFormFields();
        }

        private void ClearFormFields()
        {
            // Clear all input fields
            txtWorkTypeName.Clear();
            combDepartment.SelectedIndex = 0; // Reset to "Select Department"
            radioButtonActive.Checked = false;
            radioButtonInactive.Checked = false;
            checkBoxBillable.Checked = false;

            //Clear or reset the comboBoxDesignationID If it's bound to a data source and you want to deselect any selection
            comboBoxWorkTypeID.Text = "";

            // Clear DataGridView selection
            dataGridViewWorkTypes.ClearSelection();
        }

        private void LoadWorkTypes()
        {
            string query = "SELECT WorkTypeID, WorkTypeName, DepartmentID, IsActive, IsBillable FROM WorkTypes";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    dataGridViewWorkTypes.DataSource = dataTable;

                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

            //unselecting the selected row
            dataGridViewWorkTypes.CurrentCell = null;
            dataGridViewWorkTypes.ClearSelection();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                if (!IsDuplicateWorkType())
                {
                    UpdateWorkType();
                    ClearFormFields();
                    LoadWorkTypes();
                }
                else
                {
                    MessageBox.Show("A Work Type with the same name already exists in the selected department.");
                }
            }
        }


        private void dataGridViewWorkTypes_SelectionChanged(object sender, EventArgs e)
        {
            // Only handle the selection change if it was due to user interaction
            if (!userIsInteracting)
            {
                dataGridViewWorkTypes.ClearSelection();
                
            }

            if (dataGridViewWorkTypes.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridViewWorkTypes.SelectedRows[0];

                // Populate the form fields with the selected row data
                comboBoxWorkTypeID.Text = selectedRow.Cells["WorkTypeID"].Value.ToString();
                txtWorkTypeName.Text = selectedRow.Cells["WorkTypeName"].Value.ToString();
                combDepartment.SelectedValue = selectedRow.Cells["DepartmentID"].Value;
                radioButtonActive.Checked = (bool)selectedRow.Cells["IsActive"].Value;
                radioButtonInactive.Checked = !(bool)selectedRow.Cells["IsActive"].Value;
                checkBoxBillable.Checked = (bool)selectedRow.Cells["IsBillable"].Value;

                // Disable the btnAdd button if any row is selected
                btnAdd.Enabled = false;
            }
            else
            {
                // Enable the btnAdd button if no row is selected
                btnAdd.Enabled = true;
            }

            

        }


        private void UpdateWorkType()
        {
            if (dataGridViewWorkTypes.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridViewWorkTypes.SelectedRows[0];
                int workTypeID = (int)selectedRow.Cells["WorkTypeID"].Value;

                int departmentID = (int)combDepartment.SelectedValue;
                string workTypeName = txtWorkTypeName.Text;
                bool isActive = radioButtonActive.Checked;
                bool isBillable = checkBoxBillable.Checked;

                string query = "UPDATE WorkTypes SET WorkTypeName = @WorkTypeName, DepartmentID = @DepartmentID, IsActive = @IsActive, IsBillable = @IsBillable WHERE WorkTypeID = @WorkTypeID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@WorkTypeName", workTypeName);
                    command.Parameters.AddWithValue("@DepartmentID", departmentID);
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    command.Parameters.AddWithValue("@IsBillable", isBillable);
                    command.Parameters.AddWithValue("@WorkTypeID", workTypeID);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                        MessageBox.Show($"Work Type updated successfully with ID: {workTypeID}");
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a Work Type to update.");
            }
        }

        private void dataGridViewWorkTypes_MouseDown(object sender, MouseEventArgs e)
        {
            // Set the user interaction flag to true
            userIsInteracting = true;
        }
    }



}
