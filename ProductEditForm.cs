using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ShoesShopSystem
{
    public partial class ProductEditForm : Form
    {
        private int productId = -1;
        private string currentImagePath = "";

        private TextBox txtProductName, txtDescription, txtPrice, txtStock, txtDiscount, txtUnit;
        private ComboBox cmbCategory, cmbManufacturer, cmbSupplier;
        private PictureBox pictureBox;
        private Button btnSelectImage, btnSave, btnCancel;

        public ProductEditForm(int productId = -1)
        {
            this.productId = productId;
            CreateForm();
            if (productId != -1)
                LoadProductData();
        }

        private void CreateForm()
        {
            this.Text = productId == -1 ? "Добавление товара" : "Редактирование товара";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Picture Box
            pictureBox = new PictureBox
            {
                Location = new Point(50, 20),
                Size = new Size(150, 150),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.LightGray
            };
            pictureBox.Image = CreateDefaultImage();

            // Button for image selection
            btnSelectImage = new Button
            {
                Text = "Выбрать изображение",
                Location = new Point(50, 180),
                Size = new Size(150, 25),
                Font = new Font("Times New Roman", 8)
            };

            // Create controls
            CreateControl("Наименование:", 220, out txtProductName, 200);
            CreateControl("Описание:", 250, out txtDescription, 200, true);
            CreateComboBox("Категория:", 300, out cmbCategory);
            CreateComboBox("Производитель:", 330, out cmbManufacturer);
            CreateComboBox("Поставщик:", 360, out cmbSupplier);
            CreateControl("Цена:", 390, out txtPrice, 100);
            CreateControl("Единица измерения:", 420, out txtUnit, 100);
            CreateControl("Количество:", 450, out txtStock, 100);
            CreateControl("Скидка %:", 480, out txtDiscount, 100);

            // Buttons
            btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(150, 520),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 250, 154),
                Font = new Font("Times New Roman", 10, FontStyle.Bold)
            };

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(260, 520),
                Size = new Size(100, 35),
                Font = new Font("Times New Roman", 10)
            };

            // Events
            btnSelectImage.Click += BtnSelectImage_Click;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            // Load combobox data
            LoadComboBoxData();

            // Add controls to form
            this.Controls.Add(pictureBox);
            this.Controls.Add(btnSelectImage);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void CreateControl(string labelText, int y, out TextBox textBox, int width, bool multiline = false)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(50, y),
                Size = new Size(150, 20),
                Font = new Font("Times New Roman", 9)
            };

            textBox = new TextBox
            {
                Location = new Point(200, y),
                Size = new Size(width, multiline ? 40 : 20),
                Font = new Font("Times New Roman", 9),
                Multiline = multiline
            };

            if (multiline)
                textBox.Height = 40;

            this.Controls.Add(label);
            this.Controls.Add(textBox);
        }

        private void CreateComboBox(string labelText, int y, out ComboBox comboBox)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(50, y),
                Size = new Size(150, 20),
                Font = new Font("Times New Roman", 9)
            };

            comboBox = new ComboBox
            {
                Location = new Point(200, y),
                Size = new Size(200, 20),
                Font = new Font("Times New Roman", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            this.Controls.Add(label);
            this.Controls.Add(comboBox);
        }

        private Image CreateDefaultImage()
        {
            var bmp = new Bitmap(150, 150);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                using (var font = new Font("Arial", 10))
                using (var brush = new SolidBrush(Color.DarkGray))
                {
                    g.DrawString("Нет фото", font, brush, 45, 60);
                }
            }
            return bmp;
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (var connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();

                    // Categories
                    cmbCategory.Items.Clear();
                    using (var cmd = new MySqlCommand("SELECT CategoryID, CategoryName FROM Categories", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbCategory.Items.Add(new ComboBoxItem(
                                reader.GetString("CategoryName"),
                                reader.GetInt32("CategoryID")
                            ));
                        }
                    }

                    // Manufacturers
                    cmbManufacturer.Items.Clear();
                    using (var cmd = new MySqlCommand("SELECT ManufacturerID, ManufacturerName FROM Manufacturers", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbManufacturer.Items.Add(new ComboBoxItem(
                                reader.GetString("ManufacturerName"),
                                reader.GetInt32("ManufacturerID")
                            ));
                        }
                    }

                    // Suppliers
                    cmbSupplier.Items.Clear();
                    using (var cmd = new MySqlCommand("SELECT SupplierID, SupplierName FROM Suppliers", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbSupplier.Items.Add(new ComboBoxItem(
                                reader.GetString("SupplierName"),
                                reader.GetInt32("SupplierID")
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProductData()
        {
            try
            {
                using (var connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT p.*, c.CategoryName, m.ManufacturerName, s.SupplierName 
                           FROM Products p
                           LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                           LEFT JOIN Manufacturers m ON p.ManufacturerID = m.ManufacturerID
                           LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                           WHERE p.ProductID = @ProductID";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", productId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtProductName.Text = reader["ProductName"].ToString();
                                txtDescription.Text = reader["Description"].ToString();
                                txtPrice.Text = reader["Price"].ToString();
                                txtUnit.Text = reader["Unit"].ToString();
                                txtStock.Text = reader["StockQuantity"].ToString();
                                txtDiscount.Text = reader["Discount"].ToString();
                                currentImagePath = reader["ImagePath"]?.ToString();

                                // Load image - try from file first, then from resources
                                if (!string.IsNullOrEmpty(currentImagePath) && System.IO.File.Exists(currentImagePath))
                                {
                                    pictureBox.Image = Image.FromFile(currentImagePath);
                                }
                                else
                                {
                                    // Try to load from resources by image name
                                    string imageName = System.IO.Path.GetFileName(currentImagePath);
                                    if (!string.IsNullOrEmpty(imageName))
                                    {
                                        try
                                        {
                                            // This will work if you added images to Resources
                                            var resourceManager = new System.Resources.ResourceManager("ShoesShopSystem.Properties.Resources", typeof(Program).Assembly);
                                            var image = (Image)resourceManager.GetObject(imageName.Replace(".jpg", "").Replace(".png", ""));
                                            if (image != null)
                                            {
                                                pictureBox.Image = image;
                                            }
                                            else
                                            {
                                                pictureBox.Image = CreateDefaultImage();
                                            }
                                        }
                                        catch
                                        {
                                            pictureBox.Image = CreateDefaultImage();
                                        }
                                    }
                                    else
                                    {
                                        pictureBox.Image = CreateDefaultImage();
                                    }
                                }

                                // Set combobox values
                                SetComboBoxValue(cmbCategory, reader["CategoryName"].ToString());
                                SetComboBoxValue(cmbManufacturer, reader["ManufacturerName"].ToString());
                                SetComboBoxValue(cmbSupplier, reader["SupplierName"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных товара: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetComboBoxValue(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Text == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnSelectImage_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Title = "Выберите изображение товара";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var image = Image.FromFile(openFileDialog.FileName);
                        pictureBox.Image = new Bitmap(image, pictureBox.Size);
                        currentImagePath = openFileDialog.FileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (!ValidateInput())
                return;

            try
            {
                using (var connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();

                    string imagePath = currentImagePath;

                    if (productId == -1)
                    {
                        // Add new product
                        string query = @"INSERT INTO Products (ProductName, Description, CategoryID, ManufacturerID, SupplierID, 
                                      Price, Unit, StockQuantity, Discount, ImagePath) 
                                      VALUES (@ProductName, @Description, @CategoryID, @ManufacturerID, @SupplierID, 
                                      @Price, @Unit, @StockQuantity, @Discount, @ImagePath)";

                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            SetCommandParameters(cmd, imagePath);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Товар успешно добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Update existing product
                        string query = @"UPDATE Products SET 
                                      ProductName = @ProductName,
                                      Description = @Description,
                                      CategoryID = @CategoryID,
                                      ManufacturerID = @ManufacturerID,
                                      SupplierID = @SupplierID,
                                      Price = @Price,
                                      Unit = @Unit,
                                      StockQuantity = @StockQuantity,
                                      Discount = @Discount,
                                      ImagePath = @ImagePath
                                      WHERE ProductID = @ProductID";

                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            SetCommandParameters(cmd, imagePath);
                            cmd.Parameters.AddWithValue("@ProductID", productId);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Товар успешно обновлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения товара: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetCommandParameters(MySqlCommand cmd, string imagePath)
        {
            cmd.Parameters.AddWithValue("@ProductName", txtProductName.Text);
            cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
            cmd.Parameters.AddWithValue("@CategoryID", ((ComboBoxItem)cmbCategory.SelectedItem).Value);
            cmd.Parameters.AddWithValue("@ManufacturerID", ((ComboBoxItem)cmbManufacturer.SelectedItem).Value);
            cmd.Parameters.AddWithValue("@SupplierID", ((ComboBoxItem)cmbSupplier.SelectedItem).Value);
            cmd.Parameters.AddWithValue("@Price", decimal.Parse(txtPrice.Text));
            cmd.Parameters.AddWithValue("@Unit", txtUnit.Text);
            cmd.Parameters.AddWithValue("@StockQuantity", int.Parse(txtStock.Text));
            cmd.Parameters.AddWithValue("@Discount", decimal.Parse(txtDiscount.Text));
            cmd.Parameters.AddWithValue("@ImagePath", imagePath);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(txtProductName.Text))
            {
                MessageBox.Show("Введите наименование товара", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (cmbCategory.SelectedItem == null || cmbManufacturer.SelectedItem == null || cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию, производителя и поставщика", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену (неотрицательное число)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Введите корректное количество (неотрицательное число)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!decimal.TryParse(txtDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Введите корректную скидку (от 0 до 100)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

    // Helper class for combobox items
    public class ComboBoxItem
    {
        public string Text { get; set; }
        public int Value { get; set; }

        public ComboBoxItem(string text, int value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}