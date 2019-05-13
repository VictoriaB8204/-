using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form ifrm = new Form3();
            ifrm.Show(this); // отображаем Form2
            this.Hide(); // скрываем Form1
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //Центрирование формы
            if (Owner != null)
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                Owner.Location.Y + Owner.Height / 2 - Height / 2);
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[0];
            ifrm.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form ifrm = new Form4();
            ifrm.Show(this); // отображаем Form2
            this.Hide(); // скрываем Form1
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form ifrm = new Установка_типа_признака();
            ifrm.Show(this); // отображаем Form2
            this.Hide(); // скрываем Form1
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form ifrm = new Признаковое_описание_классов();
            ifrm.Show(this); // отображаем Form2
            this.Hide(); // скрываем Form1
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form ifrm = new Значения_признаков_для_классов();
            ifrm.Show(this); // отображаем Form2
            this.Hide(); // скрываем Form1
        }
    }
}
