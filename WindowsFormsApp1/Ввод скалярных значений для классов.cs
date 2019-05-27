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
    public partial class Ввод_скалярных_значений_для_классов : Form
    {
        SqlConnection sqlConnection;
        string _class = "";
        string _feature = "";
        string _classId = "";
        string _featureId = "";

        public Ввод_скалярных_значений_для_классов(string Class, string feature)
        {
            _class = Class;
            _feature = feature;
            InitializeComponent();
        }

        private async void Ввод_скалярных_значений_для_классов_Load(object sender, EventArgs e)
        {
            //Центрирование формы
            if (Owner != null)
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                Owner.Location.Y + Owner.Height / 2 - Height / 2);

            label2.Text = _class;
            label3.Text = _feature;

            //заполнение значениями из БД
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\учеба\Смагин\Реализация классификатора\WindowsFormsApp1\WindowsFormsApp1\Database1.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            SqlDataReader sqlReader = null;

            //Ищем Id признака
            SqlCommand command = new SqlCommand("SELECT Id FROM Feature WHERE Feature=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", _feature);
            sqlReader = await command.ExecuteReaderAsync();
            await sqlReader.ReadAsync();
            _featureId = Convert.ToString(sqlReader["Id"]);
            sqlReader.Close();

            //Ищем Id класса
            command = new SqlCommand("SELECT Id FROM Classes WHERE Class=@class", sqlConnection);
            command.Parameters.AddWithValue("class", _class);
            sqlReader = await command.ExecuteReaderAsync();
            await sqlReader.ReadAsync();
            _classId = Convert.ToString(sqlReader["Id"]);
            sqlReader.Close();

            //Заполняем список выбранных значений
            command = new SqlCommand("SELECT value " +
                "FROM ClassScalarValues INNER JOIN FeatureDescription ON ClassScalarValues.Feature=FeatureDescription.Id " +
                "WHERE FeatureDescription.Feature=@feature AND FeatureDescription.Class=@class " +
                "ORDER BY value", sqlConnection);
            command.Parameters.AddWithValue("feature", _featureId);
            command.Parameters.AddWithValue("class", _classId);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    listBox2.Items.Add(Convert.ToString(sqlReader["value"]));
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

            //Заполняем список не выбранных значений
            command = new SqlCommand("SELECT value " +
                "FROM ScalarValues WHERE Feature=@feature ORDER BY value", sqlConnection);
            command.Parameters.AddWithValue("feature", _featureId);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    if(listBox2.FindString(Convert.ToString(sqlReader["value"])) == -1)
                        listBox1.Items.Add(Convert.ToString(sqlReader["value"]));
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

        private void Ввод_скалярных_значений_для_классов_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[2];
            ifrm.Show();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("Значения перенесены!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SqlDataReader sqlReader = null;
            SqlCommand command = null;
            string featureDescriptionId = "";
            
            command = new SqlCommand("UPDATE Completeness SET " +
                "completeness=@value", sqlConnection);
            command.Parameters.AddWithValue("value", 0);
            await command.ExecuteNonQueryAsync();

            while (listBox1.Items.Count > 0)
            {
                command = new SqlCommand("SELECT Id FROM FeatureDescription WHERE Class=@class AND Feature=@feature", sqlConnection);
                command.Parameters.AddWithValue("feature", _featureId);
                command.Parameters.AddWithValue("class", _classId);
                sqlReader = await command.ExecuteReaderAsync();
                await sqlReader.ReadAsync();
                featureDescriptionId = Convert.ToString(sqlReader["Id"]);
                sqlReader.Close();

                command = new SqlCommand("INSERT INTO ClassScalarValues (Feature, value)VALUES(@Id, @value)", sqlConnection);
                command.Parameters.AddWithValue("Id", featureDescriptionId);
                command.Parameters.AddWithValue("value", listBox1.Items[0]);
                await command.ExecuteNonQueryAsync();

                listBox2.Items.Add(listBox1.Items[0]);
                listBox1.Items.RemoveAt(0);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Выберите значение!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SqlCommand command = new SqlCommand("UPDATE Completeness SET " +
                "completeness=@value", sqlConnection);
            command.Parameters.AddWithValue("value", 0);
            await command.ExecuteNonQueryAsync();

            string featureDescriptionId = "";
            command = new SqlCommand("SELECT Id FROM FeatureDescription WHERE Class=@class AND Feature=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", _featureId);
            command.Parameters.AddWithValue("class", _classId);
            SqlDataReader sqlReader = await command.ExecuteReaderAsync();
            await sqlReader.ReadAsync();
            featureDescriptionId = Convert.ToString(sqlReader["Id"]);
            sqlReader.Close();

            command = new SqlCommand("INSERT INTO ClassScalarValues (Feature, value)VALUES(@Id, @value)", sqlConnection);
            command.Parameters.AddWithValue("Id", featureDescriptionId);
            command.Parameters.AddWithValue("value", listBox1.SelectedItem);
            await command.ExecuteNonQueryAsync();

            listBox2.Items.Add(listBox1.SelectedItem);
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem == null)
            {
                MessageBox.Show("Выберите значение!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SqlCommand command = new SqlCommand("UPDATE Completeness SET " +
                "completeness=@value", sqlConnection);
            command.Parameters.AddWithValue("value", 0);
            await command.ExecuteNonQueryAsync();

            string featureDescriptionId = "";
            command = new SqlCommand("SELECT Id FROM FeatureDescription WHERE Class=@class AND Feature=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", _featureId);
            command.Parameters.AddWithValue("class", _classId);
            SqlDataReader sqlReader = await command.ExecuteReaderAsync();
            await sqlReader.ReadAsync();
            featureDescriptionId = Convert.ToString(sqlReader["Id"]);
            sqlReader.Close();

            command = new SqlCommand("DELETE FROM ClassScalarValues WHERE Feature=@feature AND value=@value", sqlConnection);
            command.Parameters.AddWithValue("feature", featureDescriptionId);
            command.Parameters.AddWithValue("value", listBox2.SelectedItem);
            await command.ExecuteNonQueryAsync();

            listBox1.Items.Add(listBox2.SelectedItem);
            listBox2.Items.RemoveAt(listBox2.SelectedIndex);
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("Значения перенесены!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SqlDataReader sqlReader = null;
            SqlCommand command = null;
            string featureDescriptionId = "";

            command = new SqlCommand("UPDATE Completeness SET " +
                "completeness=@value", sqlConnection);
            command.Parameters.AddWithValue("value", 0);
            await command.ExecuteNonQueryAsync();

            while (listBox2.Items.Count > 0)
            {
                command = new SqlCommand("SELECT Id FROM FeatureDescription WHERE Class=@class AND Feature=@feature", sqlConnection);
                command.Parameters.AddWithValue("feature", _featureId);
                command.Parameters.AddWithValue("class", _classId);
                sqlReader = await command.ExecuteReaderAsync();
                await sqlReader.ReadAsync();
                featureDescriptionId = Convert.ToString(sqlReader["Id"]);
                sqlReader.Close();

                command = new SqlCommand("DELETE FROM ClassScalarValues WHERE Feature=@feature AND value=@value", sqlConnection);
                command.Parameters.AddWithValue("feature", featureDescriptionId);
                command.Parameters.AddWithValue("value", listBox2.Items[0]);
                await command.ExecuteNonQueryAsync();

                listBox1.Items.Add(listBox2.Items[0]);
                listBox2.Items.RemoveAt(0);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
