using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Money
{
    public class Form1 : Form
    {
        // Əskinaslar (kiçikdən böyüyə). Ekranda sol sütun: 1,5,10,20 - sağ sütun: 50,100,200,500
        static readonly int[] Denominations = { 1, 5, 10, 20, 50, 100, 200, 500 };

        PictureBox[] notes = new PictureBox[8];   // əskinas şəkilləri
        Label[] counts = new Label[8];            // label1..label8 - əskinasın sayı

        TextBox textBox1;                         // Xırdalanacaq məbləğ
        Button button1;                           // Xırdala
        ErrorProvider errorProvider1;

        public Form1()
        {
            BuildUI();
        }

        // =====================================================
        //                    DİZAYN (kodla)
        // =====================================================

        private void BuildUI()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;

            Text = "Money";
            ClientSize = new Size(605, 410);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            Panel left = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(205, 410),
                BackColor = Color.FromArgb(255, 128, 0)
            };
            Panel right = new Panel
            {
                Location = new Point(205, 0),
                Size = new Size(400, 410),
                BackColor = Color.FromArgb(0, 128, 128)
            };
            Controls.Add(left);
            Controls.Add(right);

            // ---------- Sol panel ----------
            left.Controls.Add(new Label
            {
                Text = "Xırdalanacaq məbləğ",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(10, 148),
                Size = new Size(190, 22),
                ForeColor = Color.Black
            });

            textBox1 = new TextBox { Location = new Point(10, 175), Size = new Size(165, 23) };
            textBox1.TextChanged += textBox1_TextChanged;
            left.Controls.Add(textBox1);

            button1 = new Button
            {
                Text = "Xırdala",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(10, 210),
                Size = new Size(165, 45),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(225, 225, 225),
                Cursor = Cursors.Hand
            };
            button1.FlatAppearance.BorderSize = 0;
            button1.Click += button1_Click;
            left.Controls.Add(button1);
            AcceptButton = button1;   // Enter düyməsi də "Xırdala" kimi işləsin

            errorProvider1 = new ErrorProvider();
            errorProvider1.ContainerControl = this;

            // ---------- Sağ panel: 8 əskinas + 8 label ----------
            Color[] colors =
            {
                Color.FromArgb(230, 205, 140),  // 1
                Color.FromArgb(215, 175, 160),  // 5
                Color.FromArgb(170, 195, 180),  // 10
                Color.FromArgb(150, 205, 130),  // 20
                Color.FromArgb(235, 215, 120),  // 50
                Color.FromArgb(230, 170, 190),  // 100
                Color.FromArgb(120, 170, 235),  // 200
                Color.FromArgb(175, 205, 125)   // 500
            };

            for (int i = 0; i < Denominations.Length; i++)
            {
                int col = i / 4;
                int row = i % 4;
                int x = (col == 0) ? 20 : 210;
                int y = 20 + row * 97;
                int den = Denominations[i];

                PictureBox pb = new PictureBox
                {
                    Location = new Point(x, y),
                    Size = new Size(115, 75),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = colors[i],
                    BorderStyle = BorderStyle.FixedSingle,
                    Visible = false
                };

                // İstəsəniz əskinas şəkillərini Images\1.png, Images\5.png ... kimi qoyun
                pb.Image = TryLoadImage(den);

                // Şəkil yoxdursa, əskinasın nominalını yazırıq
                pb.Paint += (s, e) =>
                {
                    if (pb.Image == null)
                    {
                        using (Font f = new Font("Segoe UI", 20f, FontStyle.Bold))
                        using (StringFormat sf = new StringFormat())
                        {
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;
                            e.Graphics.DrawString(den.ToString(), f, Brushes.Black, pb.ClientRectangle, sf);
                        }
                    }
                };

                Label lbl = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(x + 122, y + 26),
                    Visible = false
                };

                notes[i] = pb;
                counts[i] = lbl;
                right.Controls.Add(pb);
                right.Controls.Add(lbl);
            }
        }

        private Image TryLoadImage(int den)
        {
            string[] extensions = { ".png", ".jpg", ".jpeg" };
            foreach (string ext in extensions)
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Images", den + ext);
                if (File.Exists(path))
                {
                    return Image.FromFile(path);
                }
            }
            return null;
        }

        // =====================================================
        //                      MƏNTİQ
        // =====================================================

        private void HideAll()
        {
            for (int i = 0; i < notes.Length; i++)
            {
                notes[i].Visible = false;
                counts[i].Visible = false;
                counts[i].Text = "";
            }
        }

        // Xana doldurulan kimi ErrorProvider işini bitirir
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length > 0)
            {
                errorProvider1.SetError(textBox1, "");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HideAll();

            string text = textBox1.Text.Trim();

            // 1) Xana boşdursa
            if (text.Length == 0)
            {
                errorProvider1.SetError(textBox1, "Məbləğ daxil edin");
                return;
            }

            // 2) Düzgün tam ədəd deyilsə (əskinaslar ən azı 1 manatdır)
            long amount;
            if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out amount))
            {
                errorProvider1.SetError(textBox1, "Məbləği tam ədədlə daxil edin");
                return;
            }

            errorProvider1.SetError(textBox1, "");

            // 3) Mənfi və ya sıfır məbləğ
            if (amount <= 0)
            {
                MessageBox.Show(
                    "Mənfi və ya sıfır məbləğ xırdalanmaz",
                    "Diqqət",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // 4) Ən böyük əskinasdan başlayaraq xırdalayırıq
            for (int i = Denominations.Length - 1; i >= 0; i--)
            {
                long count = amount / Denominations[i];

                if (count > 0)
                {
                    amount = amount % Denominations[i];

                    counts[i].Text = count + " ədəd";
                    counts[i].Visible = true;
                    notes[i].Visible = true;
                }
            }
        }
    }
}