using MySql.Data.MySqlClient;

using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using SkiaSharp;




namespace StoneShield3
{
    public partial class MainForm : Form
    {
        private string connectionString = "Server=localhost;Database=StoneShield;Uid=manel;Pwd=password2025#;";//mudar
        private System.Windows.Forms.Timer cycleTimer;
        private int cycleDuration;
        private bool isRunning = false;
        private string operationsFilePath = "C:/Users/manec/OneDrive/Desktop/operations.txt";


        public MainForm()
        {
            InitializeComponent();
            LoadRefsTable();
            AddEventHandlers();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private Bitmap LoadWebPImage(string imagePath)
        {
            try
            {
                // Read the WEBP image using SkiaSharp
                using (var webpStream = File.OpenRead(imagePath))
                {
                    var skBitmap = SKBitmap.Decode(webpStream);
                    if (skBitmap == null) throw new Exception("Failed to decode WEBP image.");

                    // Convert SKBitmap to a .NET Bitmap
                    using (var skImage = SKImage.FromBitmap(skBitmap))
                    using (var data = skImage.Encode(SKEncodedImageFormat.Png, 100)) // Convert to PNG format
                    {
                        using (var ms = new MemoryStream())
                        {
                            data.SaveTo(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            return new Bitmap(ms);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading WEBP image: {ex.Message}");
                return null;
            }
        }

        private Image ResizeAndCenterImage(Image originalImage, Size targetSize)
        {
            Bitmap resizedImage = new Bitmap(targetSize.Width, targetSize.Height);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.Clear(Color.White); // Background color (optional)

                // Calculate the scaling factor
                float scale = Math.Min((float)targetSize.Width / originalImage.Width, (float)targetSize.Height / originalImage.Height);

                // Calculate the scaled dimensions
                int scaledWidth = (int)(originalImage.Width * scale);
                int scaledHeight = (int)(originalImage.Height * scale);

                // Calculate position to center the image
                int offsetX = (targetSize.Width - scaledWidth) / 2;
                int offsetY = (targetSize.Height - scaledHeight) / 2;

                // Draw the image centered
                graphics.DrawImage(originalImage, offsetX, offsetY, scaledWidth, scaledHeight);
            }

            return resizedImage;
        }


        private void InitializeComponent()
        {
            this.Text = "StoneShield Application";
            this.ClientSize = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Search Bar
            TextBox searchBar = new TextBox
            {
                Location = new Point(20, 20),
                Width = 600,
                Name = "searchBar",
                PlaceholderText = "Search by Nome..."
            };

            PictureBox magnifierIcon = new PictureBox
            {
                Image = SystemIcons.Information.ToBitmap(), // Replace with actual magnifier icon
                Location = new Point(630, 20),
                Size = new Size(20, 20),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            // DataGridView for REFS Table
            DataGridView refsTable = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(760, 300),
                Name = "refsTable",
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            refsTable.Columns.Add("Nome", "Nome");
            refsTable.Columns.Add("Velocidade", "Velocidade");
            refsTable.Columns.Add("Cut", "Cut (Yes/No)");
            refsTable.Columns.Add("Batch", "Batch");
            refsTable.Columns.Add("Clean", "Clean (Yes/No)");

            // Selected Item Details Panel
            Label detailsLabel = new Label
            {
                Text = "Selected Item Details:",
                Location = new Point(20, 380),
                AutoSize = true
            };

            PictureBox imageBox = new PictureBox
            {
                Location = new Point(20, 410),
                Size = new Size(150, 150),
                Name = "imageBox",
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom // Ensures proper scaling and centering
            };


            Label detailsText = new Label
            {
                Location = new Point(200, 410),
                Size = new Size(580, 150),
                Name = "detailsText",
                BorderStyle = BorderStyle.FixedSingle
            };

            // Buttons
            Button startButton = new Button
            {
                Text = "START",
                Location = new Point(20, 580),
                Size = new Size(120, 40),
                Name = "startButton",
                Enabled = false
            };

            Button stopButton = new Button
            {
                Text = "STOP",
                Location = new Point(160, 580),
                Size = new Size(120, 40),
                Name = "stopButton",
                Enabled = false
            };

            Button cancelButton = new Button
            {
                Text = "CANCEL",
                Location = new Point(300, 580),
                Size = new Size(120, 40),
                Name = "cancelButton",
                Enabled = false
            };

            // Lamp Indicator
            Label lampLabel = new Label
            {
                Text = "INIT",
                Location = new Point(450, 580),
                Size = new Size(100, 40),
                Name = "lampLabel",
                BackColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add controls to the Form
            this.Controls.Add(searchBar);
            this.Controls.Add(magnifierIcon);
            this.Controls.Add(refsTable);
            this.Controls.Add(detailsLabel);
            this.Controls.Add(imageBox);
            this.Controls.Add(detailsText);
            this.Controls.Add(startButton);
            this.Controls.Add(stopButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(lampLabel);
        }


        private void LoadRefsTable()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Nome, Velocidade, Cut, Batch, Clean FROM REFS";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    DataGridView refsTable = (DataGridView)this.Controls["refsTable"];
                    refsTable.Rows.Clear();

                    while (reader.Read())
                    {
                        refsTable.Rows.Add(
                            reader["Nome"],
                            reader["Velocidade"],
                            Convert.ToBoolean(reader["Cut"]) ? "Yes" : "No",
                            reader["Batch"],
                            Convert.ToBoolean(reader["Clean"]) ? "Yes" : "No"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading table: " + ex.Message);
            }
        }

        private void AddEventHandlers()
        {
            TextBox searchBar = (TextBox)this.Controls["searchBar"];
            searchBar.TextChanged += (s, e) => SearchRefs(searchBar.Text);

            DataGridView refsTable = (DataGridView)this.Controls["refsTable"];
            refsTable.SelectionChanged += (s, e) => DisplaySelectedDetails();

            Button startButton = (Button)this.Controls["startButton"];
            startButton.Click += (s, e) => StartCycle();

            Button stopButton = (Button)this.Controls["stopButton"];
            stopButton.Click += (s, e) => StopCycle();

            Button cancelButton = (Button)this.Controls["cancelButton"];
            cancelButton.Click += (s, e) => ResetUI();
        }

        private void SearchRefs(string searchText)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"SELECT Nome, Velocidade, Cut, Batch, Clean, Image FROM REFS WHERE Nome LIKE '%{searchText}%';";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    DataGridView refsTable = (DataGridView)this.Controls["refsTable"];
                    refsTable.Rows.Clear();

                    while (reader.Read())
                    {
                        refsTable.Rows.Add(
                            reader["Nome"],
                            reader["Velocidade"],
                            Convert.ToBoolean(reader["Cut"]) ? "Yes" : "No",
                            reader["Batch"],
                            Convert.ToBoolean(reader["Clean"]) ? "Yes" : "No"
                        );

                        string imagePath = reader["Image"].ToString();
                        PictureBox imageBox = (PictureBox)this.Controls["imageBox"];

                        if (File.Exists(imagePath))
                        {
                            Image originalImage;

                            if (Path.GetExtension(imagePath).ToLower() == ".webp")
                            {
                                // Load WEBP image using SkiaSharp
                                originalImage = LoadWebPImage(imagePath);
                            }
                            else
                            {
                                // Load other image formats
                                originalImage = Image.FromFile(imagePath);
                            }

                            // Resize and center the image
                            if (originalImage != null)
                            {
                                imageBox.Image = ResizeAndCenterImage(originalImage, imageBox.Size);
                            }
                        }
                        else
                        {
                            imageBox.Image = null; // Clear the image if the file is not found
                            MessageBox.Show($"Image file not found: {imagePath}");
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching table: " + ex.Message);
            }
        }


        private void DisplaySelectedDetails()
        {
            DataGridView refsTable = (DataGridView)this.Controls["refsTable"];
            if (refsTable.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = refsTable.SelectedRows[0];
                string details = $"Nome: {selectedRow.Cells["Nome"].Value}\n" +
                                 $"Velocidade: {selectedRow.Cells["Velocidade"].Value}\n" +
                                 $"Cut: {selectedRow.Cells["Cut"].Value}\n" +
                                 $"Batch: {selectedRow.Cells["Batch"].Value}\n" +
                                 $"Clean: {selectedRow.Cells["Clean"].Value}";

                Label detailsText = (Label)this.Controls["detailsText"];
                detailsText.Text = details;

                Button startButton = (Button)this.Controls["startButton"];
                Button cancelButton = (Button)this.Controls["cancelButton"];
                startButton.Enabled = true;
                cancelButton.Enabled = true;
            }
        }

        private void StartCycle()
        {
            if (isRunning) return;

            DataGridView refsTable = (DataGridView)this.Controls["refsTable"];
            if (refsTable.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = refsTable.SelectedRows[0];
                float velocidade = Convert.ToSingle(selectedRow.Cells["Velocidade"].Value);
                cycleDuration = (int)(7 - (velocidade - 0.01) * (4 / 0.99)) * 1000;

                Label lampLabel = (Label)this.Controls["lampLabel"];
                lampLabel.BackColor = Color.Green;
                lampLabel.Text = "RUNNING";

                Button startButton = (Button)this.Controls["startButton"];
                Button cancelButton = (Button)this.Controls["cancelButton"];
                Button stopButton = (Button)this.Controls["stopButton"];
                startButton.Enabled = false;
                cancelButton.Enabled = false;
                stopButton.Enabled = true;

                cycleTimer = new System.Windows.Forms.Timer { Interval = cycleDuration };
                cycleTimer.Tick += (s, e) => EndCycle();
                cycleTimer.Start();

                isRunning = true;
            }
        }

        private void StopCycle()
        {
            if (!isRunning) return;

            cycleTimer?.Stop();

            Label lampLabel = (Label)this.Controls["lampLabel"];
            lampLabel.BackColor = Color.Red;
            lampLabel.Text = "STOPPED";

            var stopTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            stopTimer.Tick += (s, e) => ResetUI();
            stopTimer.Start();

            isRunning = false;
        }

        

        private void EndCycle()
        {
            cycleTimer?.Stop();

            DataGridView refsTable = (DataGridView)this.Controls["refsTable"];
            if (refsTable.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = refsTable.SelectedRows[0];
                string nome = selectedRow.Cells["Nome"].Value.ToString();
                string id = refsTable.SelectedRows[0].Index.ToString(); // Assuming ID is not displayed but corresponds to the row index

                UpdateOperationsFile(nome, id); // Update the file
            }

            Label lampLabel = (Label)this.Controls["lampLabel"];
            lampLabel.BackColor = Color.Blue;
            lampLabel.Text = "DONE";

            Button stopButton = (Button)this.Controls["stopButton"];
            stopButton.Enabled = false;

            var doneTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            doneTimer.Tick += (s, e) => ResetUI();
            doneTimer.Start();

            isRunning = false;
        }

        private void UpdateOperationsFile(string nome, string id)
        {
            try
            {
                if (!File.Exists(operationsFilePath))
                {
                    // Create file and add a header if it doesn't exist
                    using (var writer = new StreamWriter(operationsFilePath, false))
                    {
                        writer.WriteLine("Nome;ID;Count");
                        writer.WriteLine($"{nome};{id};1");
                    }
                }
                else
                {
                    var lines = File.ReadAllLines(operationsFilePath);
                    bool found = false;

                    for (int i = 1; i < lines.Length; i++) // Skip the header row
                    {
                        var columns = lines[i].Split(';');
                        if (columns[0] == nome && columns[1] == id)
                        {
                            int count = int.Parse(columns[2]) + 1;
                            lines[i] = $"{nome};{id};{count}";
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        using (var writer = new StreamWriter(operationsFilePath, true))
                        {
                            writer.WriteLine($"{nome};{id};1");
                        }
                    }
                    else
                    {
                        File.WriteAllLines(operationsFilePath, lines);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating operations file: {ex.Message}");
            }
        }

        private void ResetUI()
        {
            Label lampLabel = (Label)this.Controls["lampLabel"];
            lampLabel.BackColor = Color.Gray;
            lampLabel.Text = "INIT";

            Button startButton = (Button)this.Controls["startButton"];
            Button stopButton = (Button)this.Controls["stopButton"];
            Button cancelButton = (Button)this.Controls["cancelButton"];
            startButton.Enabled = false;
            stopButton.Enabled = false;
            cancelButton.Enabled = false;

            Label detailsText = (Label)this.Controls["detailsText"];
            detailsText.Text = string.Empty;
        }
    }
}
