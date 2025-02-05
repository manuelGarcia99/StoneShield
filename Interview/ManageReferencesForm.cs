using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace StoneShield
{
    public partial class ManageReferencesForm : Form
    {
        private string connectionString = "Server=localhost;Database=StoneShield;Uid=manel;Pwd=password2025#;";

        public ManageReferencesForm()
        {
            InitializeComponent();
            LoadReferences2();
        }

        private void LoadReferences2()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM REFS";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    refsTable.Rows.Clear();
                    while (reader.Read())
                    {
                        refsTable.Rows.Add(
                            reader["ID"],
                            reader["Nome"],
                            reader["Velocidade"],
                            reader["Cut"],
                            reader["Batch"],
                            reader["Clean"]
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading references: {ex.Message}");
            }
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            // Add Create logic
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            // Add Edit logic
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            using (Form deleteForm = new Form())
            {
                deleteForm.Text = "Delete Reference";
                deleteForm.Size = new System.Drawing.Size(400, 200);
                deleteForm.StartPosition = FormStartPosition.CenterParent;

                Label nameLabel = new Label
                {
                    Text = "Enter the Name of the Reference to Delete:",
                    Location = new System.Drawing.Point(20, 20),
                    AutoSize = true
                };

                TextBox nameBox = new TextBox
                {
                    Location = new System.Drawing.Point(20, 50),
                    Width = 340
                };

                Button deleteConfirmButton = new Button
                {
                    Text = "Delete",
                    Location = new System.Drawing.Point(20, 90),
                    Size = new System.Drawing.Size(120, 40)
                };

                deleteConfirmButton.Click += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(nameBox.Text))
                    {
                        try
                        {
                            using (MySqlConnection connection = new MySqlConnection(connectionString))
                            {
                                connection.Open();
                                string deleteQuery = "DELETE FROM REFS WHERE Nome = @Nome";
                                MySqlCommand command = new MySqlCommand(deleteQuery, connection);
                                command.Parameters.AddWithValue("@Nome", nameBox.Text);

                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Reference(s) deleted successfully!");
                                }
                                else
                                {
                                    MessageBox.Show("No reference(s) found with the provided name.");
                                }

                                LoadReferences();
                                deleteForm.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting reference: {ex.Message}");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid name.");
                    }
                };

                deleteForm.Controls.Add(nameLabel);
                deleteForm.Controls.Add(nameBox);
                deleteForm.Controls.Add(deleteConfirmButton);
                deleteForm.ShowDialog();
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
