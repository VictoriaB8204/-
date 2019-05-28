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

namespace WindowsFormsApp1
{
    public partial class Выбор_признаков : Form
    {
        SqlConnection sqlConnection;

        public Выбор_признаков()
        {
            InitializeComponent();
        }

        private async void Выбор_признаков_Load(object sender, EventArgs e)
        {
            //Центрирование формы
            if (Owner != null)
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                Owner.Location.Y + Owner.Height / 2 - Height / 2);
            
            //заполнение значениями из БД
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\учеба\Смагин\Реализация классификатора\WindowsFormsApp1\WindowsFormsApp1\Database1.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            
            SqlCommand command = new SqlCommand("SELECT [Feature] FROM [Feature] ORDER BY Feature", sqlConnection);
            SqlDataReader sqlReader = null;
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    listBox1.Items.Add(Convert.ToString(sqlReader["Feature"]));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }
        }

        private void Выбор_признаков_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[0];
            ifrm.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("Признаки перенесены!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            while (listBox1.Items.Count > 0)
            {
                listBox2.Items.Add(listBox1.Items[0]);
                listBox1.Items.RemoveAt(0);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Выберите признак!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            listBox2.Items.Add(listBox1.SelectedItem);
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem == null)
            {
                MessageBox.Show("Выберите признак!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            listBox1.Items.Add(listBox2.SelectedItem);
            listBox2.Items.RemoveAt(listBox2.SelectedIndex);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("Признаки перенесены!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            while (listBox2.Items.Count > 0)
            {
                listBox1.Items.Add(listBox2.Items[0]);
                listBox2.Items.RemoveAt(0);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("Выберите хотябы 1 признак!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Form ifrm = new Ввод_значений_выбранных_признаков(listBox2.Items);
            this.Hide(); // скрываем Form1
            ifrm.ShowDialog(this); // отображаем Form2
        }
    }
}
