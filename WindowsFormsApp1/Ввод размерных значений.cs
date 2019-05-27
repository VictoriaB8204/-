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
    public partial class Ввод_размерных_значений : Form
    {
        SqlConnection sqlConnection;
        string _feature = "";

        public Ввод_размерных_значений(string feature)
        {
            _feature = feature;
            InitializeComponent();
        }

        private async void Ввод_размерных_значений_Load(object sender, EventArgs e)
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
                "FROM Feature INNER JOIN DimensionValue ON Feature.Id=DimensionValue.Feature " +
                "WHERE Feature.Feature=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", _feature);

            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                if(await sqlReader.ReadAsync())
                {
                    textBox1.Text = Convert.ToString(sqlReader["unit"]);
                    comboBox1.SelectedIndex = Convert.ToInt16(sqlReader["leftValueIncluded"]);
                    numericUpDown1.Value = Convert.ToDecimal(sqlReader["leftValue"]);
                    numericUpDown2.Value = Convert.ToDecimal(sqlReader["rightValue"]);
                    comboBox2.SelectedIndex = Convert.ToInt16(sqlReader["rightValueIncluded"]);
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

        private void Ввод_размерных_значений_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[2];
            ifrm.Show();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > numericUpDown2.Value)
                numericUpDown2.Value = numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if(numericUpDown1.Value > numericUpDown2.Value)
                numericUpDown1.Value = numericUpDown2.Value;
        }

        private void SetParameters(SqlCommand command)
        {
            command.Parameters.AddWithValue("leftValueIncluded", comboBox1.SelectedIndex);
            command.Parameters.AddWithValue("leftValue", numericUpDown1.Value);
            command.Parameters.AddWithValue("rightValue", numericUpDown2.Value);
            command.Parameters.AddWithValue("rightValueIncluded", comboBox2.SelectedIndex);
            command.Parameters.AddWithValue("unit", textBox1.Text);
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Введите единицы измерения!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBox1.SelectedItem == null || comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Выберите скобки интервала!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Convert.ToString(comboBox1.SelectedItem) == "(" && 
                Convert.ToString(comboBox2.SelectedItem) == ")" && 
                numericUpDown1.Value == numericUpDown2.Value)
            {
                MessageBox.Show("Пустой интервал!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Convert.ToString(comboBox1.SelectedItem) != Convert.ToString(comboBox2.SelectedItem) &&
                numericUpDown1.Value == numericUpDown2.Value)
            {
                MessageBox.Show("Неверный интервал!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SqlCommand command = new SqlCommand("UPDATE Completeness SET " +
                "completeness=@value", sqlConnection);
            command.Parameters.AddWithValue("value", 0);
            await command.ExecuteNonQueryAsync();

            command = new SqlCommand("SELECT * " +
            "FROM Feature INNER JOIN DimensionValue ON Feature.Id=DimensionValue.Feature " +
            "WHERE [Feature]=@feature AND [leftValueIncluded]=@leftValueIncluded AND leftValue=@leftValue " +
            "AND rightValue=@rightValue AND [rightValueIncluded]=@rightValueIncluded AND unit=@unit", sqlConnection);

            command.Parameters.AddWithValue("feature", _feature);
            SetParameters(command);

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
                command = new SqlCommand("UPDATE DimensionValue SET " +
                    "leftValueIncluded=@leftValueIncluded, leftValue=@leftValue, " +
                    "rightValue=@rightValue, [rightValueIncluded]=@rightValueIncluded, unit=@unit" +
                    "WHERE [Feature]=@feature", sqlConnection);
                command.Parameters.AddWithValue("feature", id["Id"]);
                SetParameters(command);
                id.Close();
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                command = new SqlCommand("INSERT INTO [DimensionValue] " +
                    "(Feature, leftValueIncluded, leftValue, rightValue, rightValueIncluded, unit)" +
                    "VALUES(@Id, @leftValueIncluded, @leftValue, @rightValue, @rightValueIncluded, @unit)", sqlConnection);
                command.Parameters.AddWithValue("Id", id["Id"]);
                SetParameters(command);
                id.Close();
                await command.ExecuteNonQueryAsync();
            }

            this.Close();
        }
    }
}
