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
using System.Net.Mail;
using System.Security.Cryptography;

namespace WIP_Management_System.MasterFilesForms
{
    public partial class EmployeeDetailsForm : Form
    {
        // Connection string - update with your actual database connection details
        string pibtconnect = @"Data Source=localhost\sqlexpress;Initial Catalog=WIP_Sys;Integrated Security=True";
        
        private DateTime? resignationDate;

        public EmployeeDetailsForm()
        {
            InitializeComponent();
            LoadEmployeeData(); // Load employee data into the DataGridView
            dataGridViewEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewEmployees.CellClick += dataGridViewEmployees_CellClick;
            dataGridViewEmployees.SelectionChanged += dataGridViewEmployees_SelectionChanged; // Subscribe to the SelectionChanged event

            // Insert prompt items
            InsertPromptItems();

            // Subscribe to the Shown event
            this.Shown += EmployeeDetailsForm_Shown;

            // Set initial visibility
            this.linkLabelSetDesig.Visible = false;
            this.txtDesignation.Visible = false;

        }

        private void EmployeeDetailsForm_Shown(object sender, EventArgs e)
        {
            // Clear selection when the form is fully loaded and shown
            dataGridViewEmployees.ClearSelection();
            dataGridViewEmployees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridViewEmployees.Columns["EmpIDFormatted"].Width = 100; // Manually set width for specific columns
        }

        private void InsertPromptItems()
        {
            // Assuming cmbGender and cmbAuthLv are already populated with their respective lists

            // Insert "Select Gender" prompt at the beginning for cmbGender
            cmbGender.Items.Insert(0, "Select Gender");
            cmbGender.SelectedIndex = 0;

            // Insert "Select Authority Level" prompt at the beginning for cmbAuthLv
            cmbAuthLv.Items.Insert(0, "Select Authority Level");
            cmbAuthLv.SelectedIndex = 0;
        }



        private void radioButtonInactive_CheckedChanged(object sender, EventArgs e)
        {
            // Check if the radio button is checked
            bool isChecked = radioButtonInactive.Checked;

            // Set the visibility of the dateTimePicker and label based on the radio button state
            dateTimePickerResignedDate.Visible = isChecked;
            lblResignDate.Visible = isChecked;
        }



        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Assuming you have a method to get the selected EmpIDFormatted
            string selectedEmpIDFormatted = GetSelectedEmpIDFormatted();

            // Create an instance of SetDesignationAndRateForm and pass the selected EmpIDFormatted
            SetDesignationAndRateForm setDesignationAndRateForm = new SetDesignationAndRateForm(selectedEmpIDFormatted);
            setDesignationAndRateForm.Show();
        }

        // Example method to get the selected EmpIDFormatted
        private string GetSelectedEmpIDFormatted()
        {
            // Replace this with your actual logic to get the selected EmpIDFormatted
            return txtEmpID.Text; // Assuming txtEmpID contains the formatted EmpID
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Validate input fields before proceeding
            if (!ValidateInput())
            {
                return; // If validation fails, exit the method
            }

            // Gather data from form controls
            string fName = txtFName.Text;
            string lName = txtLName.Text;
            string companyEmail = txtCompanyEmail.Text;
            string gender = cmbGender.SelectedItem.ToString();
            string nic = txtNIC.Text;
            string mobileNo = txtMobileNo.Text;
            string empAddress = txtAddress.Text;
            DateTime joinedDate = dateTimePickerJoinedDate.Value;
            DateTime? resignDate = radioButtonInactive.Checked ? dateTimePickerResignedDate.Value : (DateTime?)null;
            string authorityLevel = cmbAuthLv.SelectedItem.ToString();
            bool isActive = radioButtonActive.Checked;

            // Validate email format
            if (!IsValidEmail(companyEmail))
            {
                MessageBox.Show("The email address format is invalid. Please enter a valid email address.", "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check for duplicates before proceeding
            if (CheckForDuplicates(mobileNo, nic, companyEmail))
            {
                MessageBox.Show("An employee with the same Mobile No, NIC, or Company Email already exists.", "Duplicate Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // SQL query to insert the data
            string query = @"INSERT INTO Employees (FName, LName, CompanyEmail, Gender, NIC, MobileNo, EmpAddress, JoinedDate, ResignDate, AuthorityLevel, IsActive)
                             VALUES (@FName, @LName, @CompanyEmail, @Gender, @NIC, @MobileNo, @EmpAddress, @JoinedDate, @ResignDate, @AuthorityLevel, @IsActive)";

            try
            {
                using (SqlConnection connection = new SqlConnection(pibtconnect))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        command.Parameters.AddWithValue("@FName", fName);
                        command.Parameters.AddWithValue("@LName", lName);
                        command.Parameters.AddWithValue("@CompanyEmail", companyEmail);
                        command.Parameters.AddWithValue("@Gender", gender);
                        command.Parameters.AddWithValue("@NIC", nic);
                        command.Parameters.AddWithValue("@MobileNo", mobileNo);
                        command.Parameters.AddWithValue("@EmpAddress", empAddress);
                        command.Parameters.AddWithValue("@JoinedDate", joinedDate);
                        command.Parameters.AddWithValue("@ResignDate", resignDate.HasValue ? (object)resignDate.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@AuthorityLevel", authorityLevel);
                        command.Parameters.AddWithValue("@IsActive", isActive);

                        // Open the connection and execute the query
                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        // Check if the insert was successful
                        if (result > 0)
                        {
                            // Generate a random password
                            string randomPassword = GenerateRandomPassword(8); // Assuming a method to generate a random password

                            // Hash the password before storing it
                            string hashedPassword = HashPassword(randomPassword);
                            command.Parameters.AddWithValue("@RandomPassword", hashedPassword);

                            // Update UserName and UserPassword if they are null
                            string updateQuery = @"
                                UPDATE Employees 
                                SET UserName = COALESCE(UserName, @EmpIDFormatted), 
                                    UserPassword = COALESCE(UserPassword, @RandomPassword) 
                                WHERE CompanyEmail = @CompanyEmail AND (UserName IS NULL OR UserPassword IS NULL)";

                            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@EmpIDFormatted", fName); // Use fName as a placeholder, replace with actual logic to fetch or generate EmpIDFormatted
                                updateCommand.Parameters.AddWithValue("@RandomPassword", randomPassword);
                                updateCommand.Parameters.AddWithValue("@CompanyEmail", companyEmail);

                                // Execute the update command
                                updateCommand.ExecuteNonQuery();
                            }

                            // Send login details via email
                            SendLoginDetailsEmail(companyEmail, fName, randomPassword); // Use fName as a placeholder for UserName, replace with actual logic

                            MessageBox.Show("Employee details added and login details sent via email successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ClearForm();
                            LoadEmployeeData();
                        }
                        else
                        {
                            MessageBox.Show("Error adding employee details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to validate email format using System.Net.Mail.MailAddress
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateInput()
        {
            // Validate First Name
            if (string.IsNullOrWhiteSpace(txtFName.Text))
            {
                MessageBox.Show("First name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtFName.Focus();
                return false;
            }

            // Validate Last Name
            if (string.IsNullOrWhiteSpace(txtLName.Text))
            {
                MessageBox.Show("Last name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLName.Focus();
                return false;
            }

            // Validate Company Email
            if (string.IsNullOrWhiteSpace(txtCompanyEmail.Text) || !IsValidEmail(txtCompanyEmail.Text))
            {
                MessageBox.Show("A valid company email is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCompanyEmail.Focus();
                return false;
            }

            // Validate NIC
            if (string.IsNullOrWhiteSpace(txtNIC.Text))
            {
                MessageBox.Show("NIC is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNIC.Focus();
                return false;
            }

            // Validate Mobile No
            if (string.IsNullOrWhiteSpace(txtMobileNo.Text) || !txtMobileNo.Text.All(char.IsDigit) || txtMobileNo.Text.Length != 10)
            {
                MessageBox.Show("A valid mobile number is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtMobileNo.Focus();
                return false;
            }

            // Validate Address
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Address is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtAddress.Focus();
                return false;
            }

            // Validate Gender Selection
            if (cmbGender.SelectedIndex == -1 || cmbGender.SelectedItem.ToString() == "Select Gender")
            {
                MessageBox.Show("Please select a gender.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbGender.Focus();
                return false;
            }

            // Validate Authority Level
            if (cmbAuthLv.SelectedIndex == -1 || cmbAuthLv.SelectedItem.ToString() == "Select Authority Level")
            {
                MessageBox.Show("Please select an authority level.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbAuthLv.Focus();
                return false;
            }

            // Validate Active Status - Ensure one of the radio buttons is checked
            if (!radioButtonActive.Checked && !radioButtonInactive.Checked)
            {
                MessageBox.Show("Please select an active status for the employee.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Additional validations can be added here as needed

            return true; // All validations passed
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return; // If validation fails, do not proceed with the update
            }

            // Gather updated data from form controls
            string empID = txtEmpID.Text; // Assuming txtEmpID holds the unique EmpIDFormatted value
            string fName = txtFName.Text;
            string lName = txtLName.Text;
            string companyEmail = txtCompanyEmail.Text.Trim();
            string gender = cmbGender.SelectedItem.ToString();
            string nic = txtNIC.Text.Trim();
            string mobileNo = txtMobileNo.Text.Trim();
            string empAddress = txtAddress.Text;
            DateTime joinedDate = dateTimePickerJoinedDate.Value;
            DateTime? resignDate = radioButtonInactive.Checked ? dateTimePickerResignedDate.Value : (DateTime?)null;
            string authorityLevel = cmbAuthLv.SelectedItem.ToString();
            bool isActive = radioButtonActive.Checked;
            // Assuming 'currentEmployeeId' is the unique identifier for the currently selected employee.
            string currentEmployeeId = txtEmpID.Text.Trim();

            if (CheckForDuplicatesExcludingCurrent(mobileNo, nic, companyEmail, currentEmployeeId))
            {
                MessageBox.Show("Duplicate entries found for Mobile No, NIC, or Company Email.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            // SQL query to update the data
            string query = @"UPDATE Employees SET FName = @FName, LName = @LName, CompanyEmail = @CompanyEmail, Gender = @Gender, NIC = @NIC, MobileNo = @MobileNo, EmpAddress = @EmpAddress, JoinedDate = @JoinedDate, ResignDate = @ResignDate, AuthorityLevel = @AuthorityLevel, IsActive = @IsActive WHERE EmpIDFormatted = @EmpID";

            try
            {
                using (SqlConnection connection = new SqlConnection(pibtconnect))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        command.Parameters.AddWithValue("@EmpID", empID);
                        command.Parameters.AddWithValue("@FName", fName);
                        command.Parameters.AddWithValue("@LName", lName);
                        command.Parameters.AddWithValue("@CompanyEmail", companyEmail);
                        command.Parameters.AddWithValue("@Gender", gender);
                        command.Parameters.AddWithValue("@NIC", nic);
                        command.Parameters.AddWithValue("@MobileNo", mobileNo);
                        command.Parameters.AddWithValue("@EmpAddress", empAddress);
                        command.Parameters.AddWithValue("@JoinedDate", joinedDate);
                        command.Parameters.AddWithValue("@ResignDate", resignDate.HasValue ? (object)resignDate.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@AuthorityLevel", authorityLevel);
                        command.Parameters.AddWithValue("@IsActive", isActive);

                        // Open the connection and execute the query
                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        // Check if the update was successful
                        if (result > 0)
                        {
                            MessageBox.Show("Employee details updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Unselect any selected rows in the DataGridView
                            dataGridViewEmployees.ClearSelection();

                            // Clear the form fields
                            ClearForm();

                            // Refresh the DataGridView to show the updated data
                            LoadEmployeeData();
                        }
                        else
                        {
                            MessageBox.Show("Error updating employee details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to clear the form fields
        private void ClearForm()
        {
            // Clear text boxes
            txtEmpID.Clear();
            txtFName.Clear();
            txtLName.Clear();
            txtCompanyEmail.Clear();
            txtNIC.Clear();
            txtMobileNo.Clear();
            txtAddress.Clear();
            txtSearch.Clear();

            // Reset combo boxes to their prompt items or default selection
            cmbGender.SelectedIndex = 0; // Assuming the first item is the prompt or default
            cmbAuthLv.SelectedIndex = 0; // Assuming the first item is the prompt or default

            // Reset date pickers to today's date or another appropriate default value
            dateTimePickerJoinedDate.Value = DateTime.Now;
            dateTimePickerResignedDate.Value = DateTime.Now;

            // Reset radio buttons to a default state
            radioButtonActive.Checked = true;
            radioButtonInactive.Checked = false;

            // Unselect any selected rows in the DataGridView, if applicable
            dataGridViewEmployees.ClearSelection();

            // Optionally, set focus to the first input control
            txtFName.Focus();
        }

        private void LoadEmployeeData()
        {
            string query = @"SELECT EmpIDFormatted, FName, LName, CompanyEmail, Gender, NIC, MobileNo, EmpAddress, JoinedDate, ResignDate, AuthorityLevel, UserName, UserPassword, IsActive 
                             FROM Employees";

            DataTable dataTable = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(pibtconnect))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            // Open the connection, fill the DataTable and then close the connection.
                            connection.Open();
                            adapter.Fill(dataTable);
                            connection.Close();
                        }
                    }
                }

                // Bind the DataTable to the DataGridView
                dataGridViewEmployees.DataSource = dataTable;

                // After loading the data, clear any selection.
                dataGridViewEmployees.ClearSelection();

                // Adjust the DataGridView properties to enhance user experience
              //dataGridViewEmployees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Ensure this is set after data is loaded
                dataGridViewEmployees.AllowUserToAddRows = false; // Disable the option to add rows directly in the grid
                dataGridViewEmployees.ReadOnly = true; // Make the grid read-only to prevent direct edits
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading employee data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridViewEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Check if the clicked row index is valid
            {
                DataGridViewRow row = dataGridViewEmployees.Rows[e.RowIndex];

                // Assuming the DataGridView columns match the database columns
                txtEmpID.Text = row.Cells["EmpIDFormatted"].Value.ToString();
                txtFName.Text = row.Cells["FName"].Value.ToString();
                txtLName.Text = row.Cells["LName"].Value.ToString();
                txtCompanyEmail.Text = row.Cells["CompanyEmail"].Value.ToString();

                // Handle Gender ComboBox selection
                string genderValue = row.Cells["Gender"].Value.ToString().Trim();
                int genderIndex = cmbGender.FindStringExact(genderValue);
                cmbGender.SelectedIndex = genderIndex >= 0 ? genderIndex : 0; // Revert to prompt if not found

                txtNIC.Text = row.Cells["NIC"].Value.ToString();
                txtMobileNo.Text = row.Cells["MobileNo"].Value.ToString();
                txtAddress.Text = row.Cells["EmpAddress"].Value.ToString();
                dateTimePickerJoinedDate.Value = Convert.ToDateTime(row.Cells["JoinedDate"].Value);

                if (row.Cells["ResignDate"].Value != DBNull.Value)
                {
                    dateTimePickerResignedDate.Value = Convert.ToDateTime(row.Cells["ResignDate"].Value);
                    radioButtonInactive.Checked = true;
                }
                else
                {
                    radioButtonActive.Checked = true;
                }

                // Handle Authority Level ComboBox selection
                string authLevelValue = row.Cells["AuthorityLevel"].Value.ToString().Trim();
                int authLevelIndex = cmbAuthLv.FindStringExact(authLevelValue);
                cmbAuthLv.SelectedIndex = authLevelIndex >= 0 ? authLevelIndex : 0; // Revert to prompt if not found

                // Disable the btnAdd button when a row is selected
                btnAdd.Enabled = false;

            }
            else
            {
                // Enable the btnAdd button if no row is selected (e.g., clicking outside the rows)
                btnAdd.Enabled = true;

            }
        }

        private void dataGridViewEmployees_SelectionChanged(object sender, EventArgs e)
        {
            // Disable the btnAdd button if any row is selected
            btnAdd.Enabled = dataGridViewEmployees.SelectedRows.Count == 0;

            // Check if any row is selected
            if (dataGridViewEmployees.SelectedRows.Count > 0)
            {
                // Make the controls visible
                linkLabelSetDesig.Visible = true;
                txtDesignation.Visible = true;
            }
            else
            {
                // Optionally, you can hide the controls if no row is selected
                linkLabelSetDesig.Visible = false;
                txtDesignation.Visible = false;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();

            // Ensure this line is executed after all other logic in the ClearForm method
        }
        private string GenerateRandomPassword(int length)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_~-";
            Random random = new Random();
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }
            return new string(chars);
        }

        private void SendLoginDetailsEmail(string toEmail, string userName, string password)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("charakasharishka@gmail.com");
                mail.To.Add(toEmail);
                mail.Subject = "Your Login Details";
                mail.Body = $"Hello, \n\nYour login details are as follows:\nUsername: {userName}\nPassword: {password}\n\nPlease change your password after your first login.";

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("charakasharishka@gmail.com", "szyiaiqoqxcvrubb");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send email. {ex.Message}", "Email Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Assuming dataGridViewEmployees is bound to a DataTable
            if (dataGridViewEmployees.DataSource is DataTable dataTable)
            {
                // Create a DataView from the DataTable
                DataView dataView = dataTable.DefaultView;

                // Construct a filter expression based on all columns or specific columns. Adjust according to your needs.
                // This example constructs a filter string that checks if any of the specified columns contain the text in txtSearch.
                // Note: Replace 'Column1', 'Column2', etc., with your actual column names.
                StringBuilder filterExpression = new StringBuilder();
                string[] searchableColumns = new string[] { "FName", "LName", "CompanyEmail", "NIC", "MobileNo", "EmpAddress", "AuthorityLevel" }; // Example columns
                foreach (string colName in searchableColumns)
                {
                    if (filterExpression.Length > 0) filterExpression.Append(" OR ");
                    filterExpression.AppendFormat("{0} LIKE '%{1}%'", colName, txtSearch.Text.Replace("'", "''")); // Handling single quotes for SQL LIKE
                }

                // Apply the filter
                dataView.RowFilter = filterExpression.ToString();

                // Optionally, you can sort the filtered data. For example, to sort by 'FName' in ascending order:
                // dataView.Sort = "FName ASC";

                // The DataGridView will automatically update to reflect the DataView's filtering.
            }

        }

        private bool CheckForDuplicates(string mobileNo, string nic, string companyEmail)
        {
            using (SqlConnection conn = new SqlConnection(pibtconnect))
            {
                string query = @"SELECT COUNT(*) FROM Employees 
                                 WHERE MobileNo = @MobileNo OR NIC = @NIC OR CompanyEmail = @CompanyEmail";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MobileNo", mobileNo);
                    cmd.Parameters.AddWithValue("@NIC", nic);
                    cmd.Parameters.AddWithValue("@CompanyEmail", companyEmail);

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private bool CheckForDuplicatesExcludingCurrent(string mobileNo, string nic, string companyEmail, string currentEmployeeId)
        {
            // This method should check for duplicates in your data source, excluding the current employee.
            // The implementation will depend on how you're accessing your data (e.g., database, in-memory collection).
            // Below is a pseudo-code example for a database check.

            using (SqlConnection conn = new SqlConnection(pibtconnect))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Employees WHERE (MobileNo = @MobileNo OR NIC = @NIC OR CompanyEmail = @CompanyEmail) AND EmpIDFormatted != @CurrentEmployeeId", conn))
                {
                    cmd.Parameters.AddWithValue("@MobileNo", mobileNo);
                    cmd.Parameters.AddWithValue("@NIC", nic);
                    cmd.Parameters.AddWithValue("@CompanyEmail", companyEmail);
                    cmd.Parameters.AddWithValue("@CurrentEmployeeId", currentEmployeeId);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

    }
}
