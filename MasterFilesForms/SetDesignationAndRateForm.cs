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
    public partial class SetDesignationAndRateForm : Form
    {
        string connectionString = "Data Source=CHARAKA-PC\\SQLEXPRESS;Initial Catalog=WIP_Sys;Integrated Security=True;";
        private bool userIsInteracting = false;

        public SetDesignationAndRateForm(string empIDFormatted)
        {
            InitializeComponent();
            comboBoxEmpID.Text = empIDFormatted; // Set the passed EmpIDFormatted to comboBoxEmpID
            LoadDepartments(); // Load departments when the form initializes
            LoadHourlyRates(); // Load hourly rates when the form initializes

            // Filter the data based on the empIDFormatted
            FilterDataGridByEmpID(empIDFormatted);

            // Add this line in the InitializeComponent method
            this.combDepartments.SelectedIndexChanged += new System.EventHandler(this.combDepartments_SelectedIndexChanged);

            this.dataGridViewHourlyRates.SelectionChanged += new System.EventHandler(this.dataGridViewHourlyRates_SelectionChanged);

            // Other initialization code...
            this.comboBoxEmpID.SelectedIndexChanged += new System.EventHandler(this.comboBoxEmpID_SelectedIndexChanged);

            dataGridViewHourlyRates.MouseDown += dataGridViewHourlyRates_MouseDown;


        }

        public ComboBox ComboBoxEmpID
        {
            get { return comboBoxEmpID; }
        }

        private void LoadDepartments()
        {
            try
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

                        combDepartments.DisplayMember = "DepartmentName";
                        combDepartments.ValueMember = "DepartmentID";
                        combDepartments.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while fetching departments: " + ex.Message);
            }
        }

        private void combDepartments_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combDepartments.SelectedValue != null && combDepartments.SelectedValue.ToString() != "0")
            {
                int selectedDepartmentID = Convert.ToInt32(combDepartments.SelectedValue);
                LoadDesignations(selectedDepartmentID);
            }
            else
            {
                combDesignation.DataSource = null;
            }
        }

        private void LoadDesignations(int departmentID)
        {
            try
            {
                string query = "SELECT DesignationID, DesignationName FROM Designations WHERE DepartmentID = @DepartmentID AND IsActive = 1 ORDER BY DesignationName";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentID);
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow topItem = dt.NewRow();
                        topItem[0] = 0;
                        topItem[1] = "Select Designation";
                        dt.Rows.InsertAt(topItem, 0);

                        combDesignation.DisplayMember = "DesignationName";
                        combDesignation.ValueMember = "DesignationID";
                        combDesignation.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while fetching designations: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            try
            {
                string query = "INSERT INTO HourlyRates (EmpID, DesignationID, HourlyRate, EffectiveDate) VALUES (@EmpID, @DesignationID, @HourlyRate, @EffectiveDate)";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmpID", comboBoxEmpID.Text);
                        cmd.Parameters.AddWithValue("@DesignationID", combDesignation.SelectedValue);
                        cmd.Parameters.AddWithValue("@HourlyRate", txtHourlyRate.Text);
                        cmd.Parameters.AddWithValue("@EffectiveDate", dateTimePickerEffectiveDate.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Data recorded successfully.");
                LoadHourlyRates(); // Refresh the DataGridView
                FilterDataGridByEmpID(comboBoxEmpID.Text); // Reapply the filter
                ClearFormFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while recording data: " + ex.Message);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(comboBoxEmpID.Text))
            {
                MessageBox.Show("Please select an Employee ID.");
                return false;
            }

            if (combDepartments.SelectedValue == null || combDepartments.SelectedValue.ToString() == "0")
            {
                MessageBox.Show("Please select a Department.");
                return false;
            }

            if (combDesignation.SelectedValue == null || combDesignation.SelectedValue.ToString() == "0")
            {
                MessageBox.Show("Please select a Designation.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtHourlyRate.Text) || !int.TryParse(txtHourlyRate.Text, out _))
            {
                MessageBox.Show("Please enter a valid Hourly Rate.");
                return false;
            }

            if (dateTimePickerEffectiveDate.Value == null)
            {
                MessageBox.Show("Please select an Effective Date.");
                return false;
            }

            return true;
        }



        private void LoadHourlyRates()
        {
            // Create a new DataTable to hold the data
            DataTable dataTable = new DataTable();

            // Use a using statement to ensure the connection is properly disposed of
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    conn.Open();

                    // Define your query to fetch hourly rates along with EmpIDFormatted, FName, DesignationID, and DesignationName
                    string query = @"
                SELECT 
                    hr.EmpID, 
                    e.EmpIDFormatted, 
                    e.FName, 
                    hr.DesignationID, 
                    d.DesignationName, 
                    hr.HourlyRate, 
                    hr.EffectiveDate
                FROM 
                    HourlyRates hr
                JOIN 
                    Employees e ON hr.EmpID = e.EmpID
                JOIN 
                    Designations d ON hr.DesignationID = d.DesignationID";

                    // Create a SqlDataAdapter to execute the query and fill the DataTable
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);

                    // Fill the DataTable with the results of the query
                    adapter.Fill(dataTable);

                    // Set the DataSource of the DataGridView to the DataTable
                    dataGridViewHourlyRates.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during data loading
                    MessageBox.Show("An error occurred while loading hourly rates: " + ex.Message);
                }
            }

            dataGridViewHourlyRates.CurrentCell = null; // Clear the current cell selection
            dataGridViewHourlyRates.ClearSelection();
        }

        //dataGridViewHourlyRates.CurrentCell = null; // Clear the current cell selection
        //dataGridViewHourlyRates.ClearSelection();

        private void FilterDataGridByEmpID(string empIDFormatted)
        {
            // Assuming you have a DataTable as the data source for the DataGridView
            DataTable dataTable = (DataTable)dataGridViewHourlyRates.DataSource;
            if (dataTable != null)
            {
                dataTable.DefaultView.RowFilter = $"EmpIDFormatted = '{empIDFormatted}'";
            }
        }

        private void dataGridViewHourlyRates_SelectionChanged(object sender, EventArgs e)
        {
            // Only handle the selection change if it was due to user interaction
            if (!userIsInteracting)
            {
                dataGridViewHourlyRates.ClearSelection();
            }

            if (dataGridViewHourlyRates.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridViewHourlyRates.SelectedRows[0];

                comboBoxEmpID.Text = selectedRow.Cells["EmpIDFormatted"].Value.ToString(); // Use EmpIDFormatted
                int designationID = Convert.ToInt32(selectedRow.Cells["DesignationID"].Value);
                txtHourlyRate.Text = selectedRow.Cells["HourlyRate"].Value.ToString();
                dateTimePickerEffectiveDate.Value = Convert.ToDateTime(selectedRow.Cells["EffectiveDate"].Value);

                // Fetch the DepartmentID based on the DesignationID
                int departmentID = GetDepartmentIDByDesignationID(designationID);
                combDepartments.SelectedValue = departmentID;
                combDesignation.SelectedValue = designationID;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            try
            {
                string query = "UPDATE HourlyRates SET DesignationID = @DesignationID, HourlyRate = @HourlyRate WHERE EmpID = @EmpID AND EffectiveDate = @EffectiveDate";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmpID", comboBoxEmpID.Text);
                        cmd.Parameters.AddWithValue("@DesignationID", combDesignation.SelectedValue);
                        cmd.Parameters.AddWithValue("@HourlyRate", txtHourlyRate.Text);
                        cmd.Parameters.AddWithValue("@EffectiveDate", dateTimePickerEffectiveDate.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Data updated successfully.");
                LoadHourlyRates(); // Refresh the DataGridView
                FilterDataGridByEmpID(comboBoxEmpID.Text); // Reapply the filter
                ClearFormFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while updating data: " + ex.Message);
            }
        }

        private int GetDepartmentIDByDesignationID(int designationID)
        {
            int departmentID = 0;
            try
            {
                string query = "SELECT DepartmentID FROM Designations WHERE DesignationID = @DesignationID";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DesignationID", designationID);
                        conn.Open();
                        departmentID = (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while fetching the DepartmentID: " + ex.Message);
            }

            return departmentID;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFormFields();
        }

        private void ClearFormFields()
        {
            // Clear all input fields
            combDepartments.SelectedIndex = 0; // Assuming the first item is "Select Department"
            combDesignation.DataSource = null; // Clear the designations combo box
            txtHourlyRate.Text = string.Empty;
            dateTimePickerEffectiveDate.Value = DateTime.Now; // Reset to current date

            // Clear the DataGridView selection
            dataGridViewHourlyRates.ClearSelection();
        }

        private void dataGridViewHourlyRates_MouseDown(object sender, MouseEventArgs e)
        {
            // Set the user interaction flag to true
            userIsInteracting = true;
        }

        private void comboBoxEmpID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEmpID.SelectedItem != null)
            {
                string selectedEmpIDFormatted = comboBoxEmpID.SelectedItem.ToString();
                FilterDataGridByEmpID(selectedEmpIDFormatted);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewHourlyRates.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dataGridViewHourlyRates.SelectedRows[0];

                // Extract the EmpID and EffectiveDate from the selected row
                string empID = selectedRow.Cells["EmpID"].Value.ToString();
                DateTime effectiveDate = Convert.ToDateTime(selectedRow.Cells["EffectiveDate"].Value);

                // Confirm deletion
                DialogResult result = MessageBox.Show("Are you sure you want to delete this record?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Define the DELETE query
                        string query = "DELETE FROM HourlyRates WHERE EmpID = @EmpID AND EffectiveDate = @EffectiveDate";

                        // Execute the DELETE query
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@EmpID", empID);
                                cmd.Parameters.AddWithValue("@EffectiveDate", effectiveDate);

                                conn.Open();
                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Record deleted successfully.");
                        LoadHourlyRates(); // Refresh the DataGridView
                        FilterDataGridByEmpID(comboBoxEmpID.Text); // Reapply the filter
                        ClearFormFields();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while deleting the record: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a row to delete.");
            }
        }
    }
}
