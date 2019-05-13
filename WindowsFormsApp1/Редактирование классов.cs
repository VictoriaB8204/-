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
    public partial class Form3 : Form
    {
        SqlConnection sqlConnection;

        public Form3()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private async void Form3_Load(object sender, EventArgs e)
        {
            //Центрирование формы
            if (Owner != null)
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                Owner.Location.Y + Owner.Height / 2 - Height / 2);

            //заполнение значениями из БД
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\учеба\Смагин\Реализация классификатора\WindowsFormsApp1\WindowsFormsApp1\Database1.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            SqlDataReader sqlReader = null;
            SqlCommand command = new SqlCommand("SELECT [Class] FROM [Classes] ORDER BY Class", sqlConnection);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while(await sqlReader.ReadAsync())
                {
                    listBox1.Items.Add(Convert.ToString(sqlReader["Class"]));
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[1];
            ifrm.Show();
        }
        
        private async void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                SqlCommand command = new SqlCommand("SELECT [Id] FROM [Classes] WHERE [Class]=@class", sqlConnection);
                command.Parameters.AddWithValue("class", textBox1.Text);
                SqlDataReader sqlReader = await command.ExecuteReaderAsync();
                bool recordExist = await sqlReader.ReadAsync();
                sqlReader.Close();

                if(recordExist)
                {
                    MessageBox.Show("Такой класс уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    command = new SqlCommand("INSERT INTO [Classes] (Class)VALUES(@class)", sqlConnection);
                    command.Parameters.AddWithValue("class", textBox1.Text);
                    await command.ExecuteNonQueryAsync();
                    listBox1.Items.Add(textBox1.Text);
                    textBox1.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Введите название класса!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            int chosenClass = listBox1.SelectedIndex;
            if (chosenClass != -1)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранный класс?\n Восстановление удаленного значения будет невозможно.", "Предупреждение", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if(result == DialogResult.OK)
                {
                    SqlCommand command = new SqlCommand("DELETE FROM [Classes] WHERE [Class]=@class", sqlConnection);
                    command.Parameters.AddWithValue("class", listBox1.Items[chosenClass]);
                    await command.ExecuteNonQueryAsync();
                    listBox1.Items.RemoveAt(chosenClass);
                }
            }
            else
            {
                MessageBox.Show("Удаляемый класс не выбран!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
