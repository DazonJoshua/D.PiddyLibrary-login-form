using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;

namespace D.PiddyLibrary_login_form
{
    
    public partial class MainWindow : Window
    {
        private string connectionString = "Data Source=127.0.0.1;Initial Catalog=DPiddyLibrary;Persist Security Info=True;User ID=dbmsuser1;Password=dbmsuser1;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True";

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string userIDInput = txtUserID.Text.Trim(); // Retrieve the UserID from input

            // To validate the UserID input 
            if (string.IsNullOrWhiteSpace(userIDInput) || !int.TryParse(userIDInput, out int userID))
            {
                txtNotification.Text = "Please enter a valid UserID."; // Notify invalid input
                return; // Exit if invalid
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Open the database connection

                    // Execute stored procedure "GetUserDetails" to fetch user details
                    using (SqlCommand cmd = new SqlCommand("GetUserDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure; // Indicate stored procedure
                        cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID; // Add UserID parameter

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Check if a record is found
                            {
                                string firstName = reader["Registration_FirstName"].ToString(); // Get FirstName
                                string lastName = reader["Registration_LastName"].ToString(); // Get LastName

                                txtNotification.Text = $"Welcome {firstName} {lastName}!"; // Display welcome message

                                // Record the login action
                                RecordAction(userID, "LOG IN");
                            }
                            else
                            {
                                // Notify the user if the UserID is not found
                                txtNotification.Text = "Login failed. Please check your User ID.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    txtNotification.Text = $"Error: {ex.Message}";
                }
                
                txtUserID.Text = string.Empty;
            }
        }

        // Handles the logout process
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            string userIDInput = txtUserID.Text.Trim(); // Retrieve the UserID from input

            // Validate the UserID input
            if (string.IsNullOrWhiteSpace(userIDInput) || !int.TryParse(userIDInput, out int userID))
            {
                txtNotification.Text = "Please enter a valid UserID."; // Notify invalid input
                return; // Exit if invalid
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Open the database connection

                    // Check the most recent record for the user
                    string checkQuery = @"
                        SELECT TOP 1 Description 
                        FROM Records 
                        WHERE LibraryUser_UserID = @UserID 
                        ORDER BY Timestamp DESC";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID; // Add UserID parameter

                        object result = checkCmd.ExecuteScalar(); // Execute the query and get the result

                        if (result != null && result.ToString() == "LOG IN") // If the last action was "Login"
                        {
                            // Record the logout action
                            RecordAction(userID, "LOG OUT");
                            txtNotification.Text = "Logout successful. Goodbye!";
                        }
                        else
                        {
                            // Notify the user if they haven't logged in
                            txtNotification.Text = "You must login first before logging out.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    txtNotification.Text = $"Error: {ex.Message}"; // Display any errors
                }

                txtUserID.Text = string.Empty;
            }
        }

        // Records login/logout actions in the database
        private void RecordAction(int userID, string action)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open(); // Open the database connection

                    // Retrieve FirstName and LastName using UserID
                    string fetchQuery = @"
                    SELECT Registration_FirstName, Registration_LastName 
                    FROM LibraryUser 
                    WHERE UserID = @UserID";

                    string firstName = string.Empty;
                    string lastName = string.Empty;

                    using (SqlCommand fetchCmd = new SqlCommand(fetchQuery, conn))
                    {
                        fetchCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;

                        using (SqlDataReader reader = fetchCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                firstName = reader["Registration_FirstName"].ToString();
                                lastName = reader["Registration_LastName"].ToString();
                            }
                            else
                            {
                                // If UserID is not found, exit the method
                                txtNotification.Text = "UserID not found while recording action.";
                                return;
                            }
                        }
                    }

                    // Insert login/logout action along with FirstName and LastName
                    string insertQuery = @"
                INSERT INTO Records (LibraryUser_UserID, Registration_FirstName, Registration_LastName, Description, Timestamp) 
                VALUES (@UserID, @FirstName, @LastName, @Description, @Timestamp)";

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.Add("@UserID", SqlDbType.Int).Value = userID;
                        insertCmd.Parameters.Add("@FirstName", SqlDbType.NVarChar).Value = firstName;
                        insertCmd.Parameters.Add("@LastName", SqlDbType.NVarChar).Value = lastName;
                        insertCmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = action;
                        insertCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = DateTime.Now;

                        insertCmd.ExecuteNonQuery(); // Execute the insert query
                    }
                }
                catch (Exception ex)
                {
                    txtNotification.Text = $"Error: {ex.Message}";
                }
            }
        }
        private void CloseApp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MinimizeApp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.WindowState = WindowState.Minimized;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
           
