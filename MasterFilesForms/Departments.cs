using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WIP_Management_System.MasterFilesForms
{
    public partial class Departments : Form
    {
        // Assuming connectionString is your database connection string
        private string connectionString = "Data Source=localhost\\sqlexpress;Initial Catalog=WIP_Sys;Integrated Security=True";
        private bool isFormLoading = true;

        public Departments()
        {
            InitializeComponent();
            // Inside the InitializeComponent method of Departments.Designer.cs
            LoadDepartments();// Add the following line to call the LoadDepartments method
            //RefreshDepartmentComboBox();
            this.dataGridViewDepartments.SelectionChanged += new System.EventHandler(this.dataGridViewDepartments_SelectionChanged);
            this.cmbDeptId.SelectedIndexChanged += new System.EventHandler(this.cmbDeptId_SelectedIndexChanged); // Subscribe to the event

            // Clear the selection
            dataGridViewDepartments.ClearSelection();

            this.Load += new System.EventHandler(this.Departments_Load);

        }

        private void Departments_Load(object sender, EventArgs e)
        {
            // Indicate that the form is currently loading
            isFormLoading = true;

            LoadDepartments();
            dataGridViewDepartments.ClearSelection(); // Clear the initial selection

            // Indicate that the form has finished loading
            isFormLoading = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Check if the Department Name text box is not empty
            if (string.IsNullOrWhiteSpace(txtDepartmentName.Text))
            {
                MessageBox.Show("Please enter a Department Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if any of the radio buttons is checked
            if (!radioButtonActive.Checked && !radioButtonInactive.Checked)
            {
                MessageBox.Show("Please select the department status (Active or Inactive).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if department name already exists
            if (DepartmentExists(txtDepartmentName.Text))
            {
                MessageBox.Show("A department with this name already exists.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Determine the IsActive status based on the radio button selection
            bool isActive = radioButtonActive.Checked;

            // SQL statement to insert the new department and retrieve the last inserted identity value
            string query = "INSERT INTO Departments (DepartmentName, IsActive) VALUES (@DepartmentName, @IsActive); SELECT SCOPE_IDENTITY();";

            // Using block for automatic disposal of the SqlConnection
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Create the SqlCommand object
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameters to prevent SQL injection
                    cmd.Parameters.AddWithValue("@DepartmentName", txtDepartmentName.Text);
                    cmd.Parameters.AddWithValue("@IsActive", isActive);

                    // Open the connection
                    conn.Open();

                    // Execute the command and retrieve the last inserted identity value
                    object result = cmd.ExecuteScalar();

                    // Check the result (if not null, a row was inserted)
                    if (result != null)
                    {
                        // Retrieve the last inserted DepartmentID
                        int lastInsertedId = Convert.ToInt32(result);

                        // Refresh the ComboBox to include the newly added department
                        RefreshDepartmentComboBox();

                        // Optionally, select the newly added department in the ComboBox
                        cmbDeptId.SelectedValue = lastInsertedId;

                        MessageBox.Show("Department added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();

                        // Optionally, clear the form fields here or refresh the department list
                        LoadDepartments();
                    }
                    else
                    {
                        MessageBox.Show("An error occurred while adding the department.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RefreshDepartmentComboBox()
        {
            /// SQL query to select all departments
            string query = "SELECT DepartmentID, DepartmentName FROM Departments ORDER BY DepartmentName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Assuming cmbDeptId is your ComboBox
                    cmbDeptId.DataSource = dt;
                    cmbDeptId.DisplayMember = "DepartmentID"; // Changed from "DepartmentName" to "DepartmentID"
                    cmbDeptId.ValueMember = "DepartmentID";
                }
            }
        }


        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridViewDepartments.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridViewDepartments.SelectedRows[0];
                int departmentId = Convert.ToInt32(selectedRow.Cells["DepartmentID"].Value);
                string departmentName = txtDepartmentName.Text;
                bool isActive = radioButtonActive.Checked;

                // Check if department name already exists for another department
                if (DepartmentExists(departmentName, departmentId))
                {
                    MessageBox.Show("A department with this name already exists.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string query = "UPDATE Departments SET DepartmentName = @DepartmentName, IsActive = @IsActive WHERE DepartmentID = @DepartmentID";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentId);
                        cmd.Parameters.AddWithValue("@DepartmentName", departmentName);
                        cmd.Parameters.AddWithValue("@IsActive", isActive);

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Department updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadDepartments(); // Refresh the DataGridView to show the updated data
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("An error occurred while updating the department.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a department to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        



        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            // Clear the Department Name text box
            txtDepartmentName.Text = "";

            // Reset the radio buttons to unchecked
            radioButtonActive.Checked = false;
            radioButtonInactive.Checked = false;

            // If you have a ComboBox for department selection and want to reset it as well
            // Assuming cmbDeptId is your ComboBox for selecting departments
            cmbDeptId.SelectedIndex = -1; // This line resets the ComboBox selection


            // If there are any other fields or controls to reset, do so here

            // Unselect any selected rows in the dataGridViewDepartments
            dataGridViewDepartments.ClearSelection();

            // For example, if you have a label or a text box for showing selected department ID, clear it as well
            // lblDepartmentId.Text = ""; // Assuming you have a label for showing DepartmentID
        }


        private void LoadDepartments()
        {
            // SQL query to select all departments
            string query = "SELECT DepartmentID, DepartmentName, IsActive FROM Departments";

            // Using block for automatic disposal of the SqlConnection
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Create a SqlDataAdapter to execute the query and fill a DataTable
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                {
                    // Create a DataTable to hold the query results
                    DataTable dt = new DataTable();

                    // Open the connection and fill the DataTable
                    conn.Open();
                    adapter.Fill(dt);

                    // Close the connection
                    conn.Close();

                    // Set the DataGridView's DataSource to the DataTable
                    dataGridViewDepartments.DataSource = dt;

                    // Optionally, adjust column headers or formatting here
                }
            }
        }
        

        private void dataGridViewDepartments_SelectionChanged(object sender, EventArgs e)
        {
            // Check if the form is still loading, if so, do not execute the rest of the code
            if (isFormLoading) return;

            // Proceed with the rest of the event handler code only if the form has finished loading
            if (dataGridViewDepartments.SelectedRows.Count == 1)
            {
                var selectedRow = dataGridViewDepartments.SelectedRows[0];
                txtDepartmentName.Text = selectedRow.Cells["DepartmentName"].Value.ToString();
                bool isActive = (bool)selectedRow.Cells["IsActive"].Value;
                radioButtonActive.Checked = isActive;
                radioButtonInactive.Checked = !isActive;

                // If you're using cmbDeptId to show something related or need to select a specific item based on the row selection
                RefreshDepartmentComboBox();
                cmbDeptId.SelectedValue = selectedRow.Cells["DepartmentID"].Value;

                // Disable the btnAdd button if any row is selected
                btnAdd.Enabled = false;
            }
            else
            {
                // Enable the btnAdd button if no rows are selected
                btnAdd.Enabled = true;

            }
        }




        private void cmbDeptId_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*if (cmbDeptId.SelectedValue != null)
            {
                int selectedDepartmentId = Convert.ToInt32(cmbDeptId.SelectedValue);

                foreach (DataGridViewRow row in dataGridViewDepartments.Rows)
                {
                    if (Convert.ToInt32(row.Cells["DepartmentID"].Value) == selectedDepartmentId)
                    {
                        // Clear the current selection
                        dataGridViewDepartments.ClearSelection();
                        // Select the row
                        row.Selected = true;
                        // Make sure the selected row is visible
                        dataGridViewDepartments.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }*/
        }

        private bool DepartmentExists(string departmentName, int? departmentId = null)
        {
            string query = "SELECT COUNT(*) FROM Departments WHERE DepartmentName = @DepartmentName";
            if (departmentId.HasValue)
            {
                query += " AND DepartmentID <> @DepartmentID";
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DepartmentName", departmentName);
                    if (departmentId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentId.Value);
                    }

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        
    }

    


}
