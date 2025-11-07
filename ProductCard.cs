using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ShoesShopSystem
{
    public class ProductCard : Panel
    {
        public Product Product { get; private set; }

        public ProductCard(Product product)
        {
            Product = product;
            InitializeCard();
            ApplyStyling();
        }

        private void InitializeCard()
        {
            // Общие настройки панели
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.White;
            this.Padding = new Padding(15);
            this.Size = new Size(520, 240);
            this.AutoSize = false;
            this.AutoScroll = false;
            this.MaximumSize = new Size(520, 240);
            this.MinimumSize = new Size(520, 240);

            // ----- БЛОК КАРТИНКИ -----
            var pictureBox = new PictureBox
            {
                Size = new Size(150, 150),
                Location = new Point(345, 15),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            LoadProductImage(pictureBox);

            // ----- ТЕКСТОВАЯ ИНФОРМАЦИЯ -----
            var lblCategory = new Label
            {
                Text = $"{Product.CategoryName} | {Product.ProductName}",
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(310, 25),
                ForeColor = Color.Black
            };

            var lblDescription = new Label
            {
                Text = $"Описание: {GetShortDescription(Product.Description)}",
                Font = new Font("Times New Roman", 9, FontStyle.Italic),
                Location = new Point(10, 35),
                Size = new Size(310, 30),
                ForeColor = Color.DimGray
            };

            var lblManufacturer = new Label
            {
                Text = $"Производитель: {Product.ManufacturerName}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(10, 70),
                Size = new Size(310, 18)
            };

            var lblSupplier = new Label
            {
                Text = $"Поставщик: {Product.SupplierName}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(10, 90),
                Size = new Size(310, 18)
            };

            var lblPrice = new Label
            {
                Text = $"Цена: {Product.Price:C}",
                Font = new Font("Times New Roman", 9, Product.Discount > 0 ? FontStyle.Strikeout : FontStyle.Regular),
                ForeColor = Product.Discount > 0 ? Color.Red : Color.Black,
                Location = new Point(10, 115),
                Size = new Size(160, 20)
            };

            var lblFinalPrice = new Label
            {
                Text = Product.Discount > 0 ? $"Итоговая: {Product.FinalPrice:C}" : "",
                Font = new Font("Times New Roman", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(10, 135),
                Size = new Size(180, 22)
            };

            var lblUnit = new Label
            {
                Text = $"Единица: {Product.Unit}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(10, 165),
                Size = new Size(100, 20)
            };

            var lblStock = new Label
            {
                Text = $"На складе: {Product.StockQuantity}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(120, 165),
                Size = new Size(100, 20)
            };

            var lblDiscount = new Label
            {
                Text = Product.Discount > 0 ? $"Скидка: {Product.Discount:F2}%" : "",
                Font = new Font("Times New Roman", 10, FontStyle.Bold),
                ForeColor = Color.Maroon,
                Location = new Point(345, 170),
                Size = new Size(150, 22),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            

            // ----- ДОБАВЛЕНИЕ ЭЛЕМЕНТОВ -----
            this.Controls.AddRange(new Control[]
            {
                pictureBox, lblCategory, lblDescription, lblManufacturer,
                lblSupplier, lblPrice, lblFinalPrice, lblUnit, lblStock,
                lblDiscount
            });
        }

        private string GetShortDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return "Нет описания";
            return description.Length > 60
                ? description.Substring(0, 60) + "..."
                : description;
        }

        private void LoadProductImage(PictureBox pictureBox)
        {
            try
            {
                if (!string.IsNullOrEmpty(Product.ImagePath))
                {
                    if (File.Exists(Product.ImagePath))
                    {
                        pictureBox.Image = Image.FromFile(Product.ImagePath);
                        return;
                    }

                    string exePath = Path.GetDirectoryName(Application.ExecutablePath);
                    string imageName = Path.GetFileName(Product.ImagePath);
                    string localPath = Path.Combine(exePath, "Images", imageName);

                    if (File.Exists(localPath))
                    {
                        pictureBox.Image = Image.FromFile(localPath);
                        return;
                    }
                }

                pictureBox.Image = CreateDefaultImage();
            }
            catch
            {
                pictureBox.Image = CreateDefaultImage();
            }
        }

        private Image CreateDefaultImage()
        {
            Bitmap bmp = new Bitmap(150, 150);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.WhiteSmoke);
                using (Font f = new Font("Arial", 9))
                using (Brush b = new SolidBrush(Color.Gray))
                {
                    g.DrawString("Нет изображения", f, b, 20, 65);
                }
            }
            return bmp;
        }
        
        private void ApplyStyling()
        {
            if (Product.Discount > 15)
            {
                this.BackColor = Color.FromArgb(240, 255, 240);
            }
        }
    }
}
