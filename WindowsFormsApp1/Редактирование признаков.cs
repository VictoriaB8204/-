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
    public partial class Form4 : Form
    {
        SqlConnection sqlConnection;

        public Form4()
        {
            InitializeComponent();
        }

        private async void Form4_Load(object sender, EventArgs e)
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
            SqlCommand command = new SqlCommand("SELECT [Feature] FROM [Feature] ORDER BY Feature", sqlConnection);
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

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
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
                SqlCommand command = new SqlCommand("SELECT [Id] FROM [Feature] WHERE [Feature]=@feature", sqlConnection);
                command.Parameters.AddWithValue("feature", textBox1.Text);
                SqlDataReader sqlReader = await command.ExecuteReaderAsync();
                bool recordExist = await sqlReader.ReadAsync();
                sqlReader.Close();

                if (recordExist)
                {
                    MessageBox.Show("Такой признак уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    command = new SqlCommand("INSERT INTO [Feature] (Feature)VALUES(@feature)", sqlConnection);
                    command.Parameters.AddWithValue("feature", textBox1.Text);
                    await command.ExecuteNonQueryAsync();
                    listBox1.Items.Add(textBox1.Text);
                    textBox1.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Введите название признака!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            int chosenFeature = listBox1.SelectedIndex;
            if (chosenFeature != -1)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранный признак?\nВосстановление удаленного значения будет невозможно.", "Предупреждение", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (result == DialogResult.OK)
                {
                    SqlCommand command = new SqlCommand("DELETE FROM [Feature] WHERE [Feature]=@feature", sqlConnection);
                    command.Parameters.AddWithValue("feature", listBox1.Items[chosenFeature]);
                    await command.ExecuteNonQueryAsync();
                    listBox1.Items.RemoveAt(chosenFeature);
                }
            }
            else
            {
                MessageBox.Show("Удаляемый признак не выбран!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
