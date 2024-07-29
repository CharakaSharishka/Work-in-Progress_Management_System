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
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;


namespace WIP_Management_System.MasterFilesForms
{
    public partial class Designations : Form
    {
        string connectionString = "Data Source=CHARAKA-PC\\SQLEXPRESS;Initial Catalog=WIP_Sys;Integrated Security=True;";
        private bool userInteraction = false;

        public Designations()
        {
            InitializeComponent();
            LoadDepartments(); // Populate the comboBoxDepartment
            LoadDesignations(); // Load designations into dataGridViewDesignations
            dataGridViewDesignations.SelectionChanged += dataGridViewDesignations_SelectionChanged;
            dataGridViewDesignations.MouseDown += dataGridViewDesignations_MouseDown;

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

                    comboBoxDepartment.DisplayMember = "DepartmentName";
                    comboBoxDepartment.ValueMember = "DepartmentID";
                    comboBoxDepartment.DataSource = dt;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtDesignationName.Text))
            {
                MessageBox.Show("Please enter a Designation Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBoxDepartment.SelectedIndex <= 0) // Assuming the first item is "Select Department"
            {
                MessageBox.Show("Please select a Department.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!radioButtonActive.Checked && !radioButtonInactive.Checked)
            {
                MessageBox.Show("Please select the status (Active or Inactive).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            int departmentId = Convert.ToInt32(comboBoxDepartment.SelectedValue);
            int newDesignationId = CalculateNewDesignationId(departmentId);

            // Prepare data for insertion
            string designationName = txtDesignationName.Text;
            bool isActive = radioButtonActive.Checked;

            // Check if the designation already exists within the same department
            if (DesignationExists(designationName, departmentId))
            {
                MessageBox.Show("A designation with this name already exists in the selected department.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // SQL INSERT statement
            string query = "INSERT INTO Designations (DesignationID, DesignationName, DepartmentID, IsActive) VALUES (@DesignationID, @DesignationName, @DepartmentID, @IsActive)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameters to prevent SQL injection
                    cmd.Parameters.AddWithValue("@DesignationID", newDesignationId);
                    cmd.Parameters.AddWithValue("@DesignationName", designationName);
                    cmd.Parameters.AddWithValue("@DepartmentID", departmentId);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    // Open the connection and execute the command
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();

                    // Check if the insertion was successful
                    if (result > 0)
                    {
                        MessageBox.Show("Designation added successfully. Designation ID: {newDesignationId}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDesignations();
                        ClearFormFields();

                        // Optionally, clear the form fields here or refresh the designation list

                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the designation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool DesignationExists(string designationName, int departmentId)
        {
            string query = @"
        SELECT COUNT(*) 
        FROM Designations 
        WHERE DesignationName = @DesignationName 
        AND DepartmentID = @DepartmentID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DesignationName", designationName);
                    cmd.Parameters.AddWithValue("@DepartmentID", departmentId);

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private int CalculateNewDesignationId(int departmentId)
        {
            int newDesignationId = departmentId * 100; // Starting point for the department (e.g., 100 for DepartmentID 1)
            string query = "SELECT MAX(DesignationID) FROM Designations WHERE DesignationID / 100 = @DepartmentId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DepartmentId", departmentId);

                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != DBNull.Value)
                    {
                        int maxDesignationId = Convert.ToInt32(result);
                        newDesignationId = maxDesignationId + 1;
                    }
                    else
                    {
                        // If no existing designation for the department, start with departmentId * 100 + 1 (e.g., 101 for DepartmentID 1)
                        newDesignationId += 1;
                    }
                }
            }

            return newDesignationId;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFormFields();
        }

        private void ClearFormFields()
        {
            // Clear the Designation Name text box
            txtDesignationName.Text = "";

            // Reset the Department combo box to the first item (assuming it's the "Select Department" item)
            comboBoxDepartment.SelectedIndex = 0;

            // Uncheck both radio buttons
            radioButtonActive.Checked = false;
            radioButtonInactive.Checked = false;

            // Unselect any selected rows in the dataGridViewDesignations
            dataGridViewDesignations.ClearSelection();
            dataGridViewDesignations.CurrentCell = null;


            //Clear or reset the comboBoxDesignationID If it's bound to a data source and you want to deselect any selection
            combDesignationID.Text = "";
        }

        private void LoadDesignations()
        {
            // Modified query to include a JOIN with the Departments table
            string query = @"
                SELECT 
                    D.DesignationID, 
                    D.DesignationName, 
                    D.DepartmentID, 
                    Dept.DepartmentName,
                    D.IsActive
                FROM 
                    Designations D
                    INNER JOIN Departments Dept ON D.DepartmentID = Dept.DepartmentID
                ORDER BY 
                    D.DesignationID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridViewDesignations.DataSource = dt;
                }
            }

            dataGridViewDesignations.CurrentCell = null;
            dataGridViewDesignations.ClearSelection();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtDesignationName.Text))
            {
                MessageBox.Show("Please enter a Designation Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBoxDepartment.SelectedIndex <= 0) // Assuming the first item is "Select Department"
            {
                MessageBox.Show("Please select a Department.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!radioButtonActive.Checked && !radioButtonInactive.Checked)
            {
                MessageBox.Show("Please select the status (Active or Inactive).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Extracting the values from the form controls
            int designationId = Convert.ToInt32(dataGridViewDesignations.SelectedRows[0].Cells["DesignationID"].Value);
            string designationName = txtDesignationName.Text.Trim();
            int departmentId = Convert.ToInt32(comboBoxDepartment.SelectedValue);
            bool isActive = radioButtonActive.Checked;

            // Check if the designation already exists within the same department, excluding the current designation
            if (DesignationExists(designationName, departmentId, designationId))
            {
                MessageBox.Show("A designation with this name already exists in the selected department.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // SQL UPDATE statement
            string query = @"
                UPDATE Designations
                SET DesignationName = @DesignationName, DepartmentID = @DepartmentID, IsActive = @IsActive
                WHERE DesignationID = @DesignationID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameters to prevent SQL injection
                    cmd.Parameters.AddWithValue("@DesignationID", designationId);
                    cmd.Parameters.AddWithValue("@DesignationName", designationName);
                    cmd.Parameters.AddWithValue("@DepartmentID", departmentId);
                    cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0); // Assuming IsActive is a bit field in SQL

                    // Open the connection and execute the command
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();

                    // Check if the update was successful
                    if (result > 0)
                    {
                        MessageBox.Show("Designation updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDesignations(); // Refresh the list to show the updated data
                        ClearFormFields(); // Optionally clear the form fields
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while updating the designation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool DesignationExists(string designationName, int departmentId, int currentDesignationId = 0)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM Designations 
                WHERE DesignationName = @DesignationName 
                AND DepartmentID = @DepartmentID
                AND DesignationID <> @CurrentDesignationID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DesignationName", designationName);
                    cmd.Parameters.AddWithValue("@DepartmentID", departmentId);
                    cmd.Parameters.AddWithValue("@CurrentDesignationID", currentDesignationId);

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private void dataGridViewDesignations_SelectionChanged(object sender, EventArgs e)
        {
            if (!userInteraction)
            {
                dataGridViewDesignations.ClearSelection();
            }

            if (dataGridViewDesignations.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridViewDesignations.SelectedRows[0];
                txtDesignationName.Text = selectedRow.Cells["DesignationName"].Value.ToString();
                comboBoxDepartment.SelectedValue = selectedRow.Cells["DepartmentID"].Value;
                bool isActive = (bool)selectedRow.Cells["IsActive"].Value;
                radioButtonActive.Checked = isActive;
                radioButtonInactive.Checked = !isActive;

                // Set comboBoxDesignationID value
                var designationID = selectedRow.Cells["DesignationID"].Value.ToString();
                combDesignationID.Text = designationID; // If comboBoxDesignationID is used to display the ID // Or, if comboBoxDesignationID is bound to a list of IDs, ensure it's properly populated and use: // comboBoxDesignationID.SelectedValue = designationID;

                // Disable the btnAdd button if any row is selected
                btnAdd.Enabled = false;
            }
            else
            {
                // Enable the btnAdd button if no row is selected
                btnAdd.Enabled = true;
            }
        }

        private void dataGridViewDesignations_MouseDown(object sender, MouseEventArgs e)
        {
            userInteraction = true;
        }
    }
}
