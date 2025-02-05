using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace StoneShield3
{
    public partial class ManageReferencesForm : Form
    {
        private string connectionString = "Server=localhost;Database=StoneShield;Uid=manel;Pwd=password2025#;";
        private System.ComponentModel.IContainer components = null;
        private DataGridView refsTable;
        private Button backButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public ManageReferencesForm()
        {
            InitializeComponent();
            LoadReferences();
        }

        private void InitializeComponent()
        {
            // Initialize refsTable
            this.refsTable = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(760, 400),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            refsTable.Columns.Add("ID", "ID");
            refsTable.Columns.Add("Nome", "Nome");
            refsTable.Columns.Add("Velocidade", "Velocidade");
            refsTable.Columns.Add("Cut", "Cut");
            refsTable.Columns.Add("Batch", "Batch");
            refsTable.Columns.Add("Clean", "Clean");

            // Initialize Back Button
            this.backButton = new Button
            {
                Text = "Back",
                Location = new Point(20, 450),
                Size = new Size(120, 40)
            };
            this.backButton.Click += new EventHandler(BackButton_Click);

            // Add controls to form
            this.Controls.Add(this.refsTable);
            this.Controls.Add(this.backButton);

            this.Text = "Manage References";
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }


        private void LoadReferences()
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
            // Open a form for creating a new reference
            using (Form createForm = new Form())
            {
                createForm.Text = "Create New Reference";
                createForm.Size = new Size(400, 450);
                createForm.StartPosition = FormStartPosition.CenterParent;

                // Input fields
                TextBox nomeBox = new TextBox { PlaceholderText = "Name", Location = new Point(20, 20), Width = 340 };
                TextBox velocidadeBox = new TextBox { PlaceholderText = "Velocidade (0.01 - 1.00)", Location = new Point(20, 60), Width = 340 };
                NumericUpDown batchBox = new NumericUpDown { Minimum = 1, Maximum = 100, Location = new Point(20, 100), Width = 340 };
                CheckBox cleanBox = new CheckBox { Text = "Clean (Yes/No)", Location = new Point(20, 140) };
                CheckBox cutBox = new CheckBox { Text = "Cut (Yes/No)", Location = new Point(20, 180) };
                TextBox imagePathBox = new TextBox { PlaceholderText = "Image Path", Location = new Point(20, 220), Width = 270, ReadOnly = true };
                Button browseButton = new Button { Text = "Browse", Location = new Point(300, 220), Width = 60 };
                Button saveButton = new Button { Text = "Save", Location = new Point(20, 270), Width = 340 };

                // Add controls to the createForm
                createForm.Controls.Add(nomeBox);
                createForm.Controls.Add(velocidadeBox);
                createForm.Controls.Add(batchBox);
                createForm.Controls.Add(cleanBox);
                createForm.Controls.Add(cutBox);
                createForm.Controls.Add(imagePathBox);
                createForm.Controls.Add(browseButton);
                createForm.Controls.Add(saveButton);

                // Browse button logic
                browseButton.Click += (s, args) =>
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            imagePathBox.Text = openFileDialog.FileName;
                        }
                    }
                };

                // Save button logic
                saveButton.Click += (s, args) =>
                {
                    // Validate inputs
                    if (string.IsNullOrEmpty(nomeBox.Text) || string.IsNullOrEmpty(velocidadeBox.Text) || string.IsNullOrEmpty(imagePathBox.Text))
                    {
                        MessageBox.Show("All fields are required, including the image path.");
                        return;
                    }

                    if (!float.TryParse(velocidadeBox.Text, out float velocidade) || velocidade < 0.01 || velocidade > 1.00)
                    {
                        MessageBox.Show("Invalid Velocidade. It must be a number between 0.01 and 1.00.");
                        return;
                    }

                    try
                    {
                        // Insert data into the database
                        using (MySqlConnection connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            string query = "INSERT INTO REFS (Nome, Velocidade, Cut, Batch, Clean, Image) " +
                                           "VALUES (@Nome, @Velocidade, @Cut, @Batch, @Clean, @Image)";
                            MySqlCommand command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@Nome", nomeBox.Text);
                            command.Parameters.AddWithValue("@Velocidade", velocidade);
                            command.Parameters.AddWithValue("@Cut", cutBox.Checked);
                            command.Parameters.AddWithValue("@Batch", (int)batchBox.Value);
                            command.Parameters.AddWithValue("@Clean", cleanBox.Checked);
                            command.Parameters.AddWithValue("@Image", imagePathBox.Text);

                            command.ExecuteNonQuery();
                        }

                        MessageBox.Show("Reference added successfully!");
                        LoadReferences(); // Refresh the table
                        createForm.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving reference: {ex.Message}");
                    }
                };

                // Show the form as a dialog
                createForm.ShowDialog();
            }
        }


        private void EditButton_Click(object sender, EventArgs e)
        {
            using (Form nameForm = new Form())
            {
                nameForm.Text = "Select Reference to Edit";
                nameForm.Size = new Size(400, 200);
                nameForm.StartPosition = FormStartPosition.CenterParent;

                Label nameLabel = new Label
                {
                    Text = "Enter the Name of the Reference to Edit:",
                    Location = new Point(20, 20),
                    AutoSize = true
                };

                TextBox nameBox = new TextBox
                {
                    Location = new Point(20, 50),
                    Width = 340
                };

                Button confirmButton = new Button
                {
                    Text = "Next",
                    Location = new Point(20, 90),
                    Size = new Size(120, 40)
                };

                confirmButton.Click += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(nameBox.Text))
                    {
                        nameForm.Close();
                        OpenEditForm(nameBox.Text);
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid name.");
                    }
                };

                nameForm.Controls.Add(nameLabel);
                nameForm.Controls.Add(nameBox);
                nameForm.Controls.Add(confirmButton);
                nameForm.ShowDialog();
            }
        }

        private void OpenEditForm(string referenceName)
        {
            using (Form editForm = new Form())
            {
                editForm.Text = $"Edit Reference: {referenceName}";
                editForm.Size = new Size(400, 500);
                editForm.StartPosition = FormStartPosition.CenterParent;

                TextBox nomeBox = new TextBox { Text = referenceName, Location = new Point(20, 20), Width = 340 };
                TextBox velocidadeBox = new TextBox { PlaceholderText = "Velocidade (0.01 - 1.00)", Location = new Point(20, 60), Width = 340 };
                NumericUpDown batchBox = new NumericUpDown { Minimum = 1, Maximum = 100, Location = new Point(20, 100), Width = 340 };
                CheckBox cleanBox = new CheckBox { Text = "Clean (Yes/No)", Location = new Point(20, 140) };
                CheckBox cutBox = new CheckBox { Text = "Cut (Yes/No)", Location = new Point(20, 180) };
                TextBox imagePathBox = new TextBox { PlaceholderText = "Image Path", Location = new Point(20, 220), Width = 270, ReadOnly = true };
                Button browseButton = new Button { Text = "Browse", Location = new Point(300, 220), Width = 60 };
                Button saveButton = new Button { Text = "Save", Location = new Point(20, 270), Width = 340 };

                // Browse Button Logic
                browseButton.Click += (s, args) =>
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            imagePathBox.Text = openFileDialog.FileName;
                        }
                    }
                };

                // Save Button Logic
                saveButton.Click += (s, args) =>
                {
                    if (!float.TryParse(velocidadeBox.Text, out float velocidade) || velocidade < 0.01 || velocidade > 1.00)
                    {
                        MessageBox.Show("Invalid Velocidade. It must be a number between 0.01 and 1.00.");
                        return;
                    }

                    try
                    {
                        using (MySqlConnection connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();
                            string query = "UPDATE REFS SET Nome = @Nome, Velocidade = @Velocidade, Cut = @Cut, Batch = @Batch, Clean = @Clean, Image = @Image WHERE Nome = @OldNome";

                            MySqlCommand command = new MySqlCommand(query, connection);
                            command.Parameters.AddWithValue("@Nome", nomeBox.Text);
                            command.Parameters.AddWithValue("@Velocidade", velocidade);
                            command.Parameters.AddWithValue("@Cut", cutBox.Checked);
                            command.Parameters.AddWithValue("@Batch", (int)batchBox.Value);
                            command.Parameters.AddWithValue("@Clean", cleanBox.Checked);
                            command.Parameters.AddWithValue("@Image", imagePathBox.Text);
                            command.Parameters.AddWithValue("@OldNome", referenceName);

                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Reference updated successfully!");
                            }
                            else
                            {
                                MessageBox.Show("No reference found with the provided name.");
                            }

                            LoadReferences(); // Refresh DataGridView
                            editForm.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating reference: {ex.Message}");
                    }
                };

                // Add Controls to the Edit Form
                editForm.Controls.Add(nomeBox);
                editForm.Controls.Add(velocidadeBox);
                editForm.Controls.Add(batchBox);
                editForm.Controls.Add(cleanBox);
                editForm.Controls.Add(cutBox);
                editForm.Controls.Add(imagePathBox);
                editForm.Controls.Add(browseButton);
                editForm.Controls.Add(saveButton);

                editForm.ShowDialog();
            }
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
            // Close the ManageReferencesForm
            this.Close();
        }
    }
}
