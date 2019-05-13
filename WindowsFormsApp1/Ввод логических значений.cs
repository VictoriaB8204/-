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
    public partial class Ввод_логических_значений : Form
    {
        SqlConnection sqlConnection;
        string _feature = "";

        public Ввод_логических_значений(string feature)
        {
            _feature = feature;
            InitializeComponent();
        }

        private async void Ввод_логических_значений_Load(object sender, EventArgs e)
        {
            //Центрирование формы
            if (Owner != null)
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                Owner.Location.Y + Owner.Height / 2 - Height / 2);

            label4.Text = _feature;

            //заполнение значениями из БД
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\учеба\Смагин\Реализация классификатора\WindowsFormsApp1\WindowsFormsApp1\Database1.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            SqlDataReader sqlReader = null;

            SqlCommand command = new SqlCommand("SELECT * " +
                "FROM Feature INNER JOIN LogicalValues ON Feature.Id=LogicalValues.Feature " +
                "WHERE Feature.Feature=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", _feature);

            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                if (await sqlReader.ReadAsync())
                {
                    textBox1.Text = Convert.ToString(sqlReader["TrueValue"]);
                    textBox2.Text = Convert.ToString(sqlReader["FalseValue"]);
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

        private void Ввод_логических_значений_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[2];
            ifrm.Show();
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Введите значение истина!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Введите значение ложь!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SqlCommand command = new SqlCommand("SELECT * " +
            "FROM Feature INNER JOIN LogicalValues ON Feature.Id=LogicalValues.Feature " +
            "WHERE Feature.Feature=@feature AND [TrueValue]=@TrueValue AND FalseValue=@FalseValue", sqlConnection);

            command.Parameters.AddWithValue("feature", _feature);
            command.Parameters.AddWithValue("TrueValue", textBox1.Text);
            command.Parameters.AddWithValue("FalseValue", textBox2.Text);

            bool recordExist = false;
            try
            {
                SqlDataReader sqlReader = await command.ExecuteReaderAsync();
                recordExist = await sqlReader.ReadAsync();
                sqlReader.Close();
            }
            catch (Exception ex) { }

            command = new SqlCommand("SELECT [Id] FROM [Feature] WHERE [Feature].[Feature]=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", _feature);
            SqlDataReader id = await command.ExecuteReaderAsync();
            await id.ReadAsync();

            if (recordExist)
            {
                command = new SqlCommand("UPDATE LogicalValues SET " +
                    "TrueValue=@TrueValue, FalseValue=@FalseValue " +
                    "WHERE Feature=@feature", sqlConnection);
                command.Parameters.AddWithValue("feature", id["Id"]);
                command.Parameters.AddWithValue("TrueValue", textBox1.Text);
                command.Parameters.AddWithValue("FalseValue", textBox2.Text);
                id.Close();
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                command = new SqlCommand("INSERT INTO [LogicalValues] " +
                    "(Feature, TrueValue, FalseValue)" +
                    "VALUES(@Id, @TrueValue, @FalseValue)", sqlConnection);
                command.Parameters.AddWithValue("Id", id["Id"]);
                command.Parameters.AddWithValue("TrueValue", textBox1.Text);
                command.Parameters.AddWithValue("FalseValue", textBox2.Text);
                id.Close();
                await command.ExecuteNonQueryAsync();
            }

            this.Close();
        }
    }
}
