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
    public partial class Ввод_размерных_значений_для_классов : Form
    {
        SqlConnection sqlConnection;
        string _class = "";
        string _feature = "";
        string _classId = "";
        string _featureId = "";
        double leftValueIncluded = -1;
        decimal leftValue = 0;
        decimal rightValue = 0;
        double rightValueIncluded = -1;

        public Ввод_размерных_значений_для_классов(string Class, string feature)
        {
            _class = Class;
            _feature = feature;
            InitializeComponent();
        }

        private async void Ввод_размерных_значений_для_классов_Load(object sender, EventArgs e)
        {
            //Центрирование формы
            if (Owner != null)
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                Owner.Location.Y + Owner.Height / 2 - Height / 2);

            label6.Text = _class;
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

            //Достаем возможные значения
            command = new SqlCommand("SELECT * FROM DimensionValue WHERE DimensionValue.Feature=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", _featureId);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                if (await sqlReader.ReadAsync())
                {
                    leftValueIncluded = Convert.ToInt64(sqlReader["leftValueIncluded"]);
                    if (leftValueIncluded == 0)
                        comboBox1.SelectedIndex = 0;
                    else
                        comboBox1.SelectedIndex = 1;
                    leftValue = Convert.ToDecimal(sqlReader["leftValue"]);
                    numericUpDown1.Value = leftValue;
                    rightValue = Convert.ToDecimal(sqlReader["rightValue"]);
                    numericUpDown2.Value = rightValue;
                    rightValueIncluded = Convert.ToInt64(sqlReader["rightValueIncluded"]);
                    if (rightValueIncluded == 0)
                        comboBox2.SelectedIndex = 0;
                    else
                        comboBox2.SelectedIndex = 1;
                    label8.Text = Convert.ToString(sqlReader["unit"]);
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

            //Достаем уже внесенные значения
            command = new SqlCommand("SELECT * " +
                "FROM ClassDimensionValue INNER JOIN FeatureDescription ON ClassDimensionValue.Feature=FeatureDescription.Id " +
                "WHERE FeatureDescription.Feature=@feature AND FeatureDescription.Class=@class ", sqlConnection);
            command.Parameters.AddWithValue("feature", _featureId);
            command.Parameters.AddWithValue("class", _classId);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                if(await sqlReader.ReadAsync())
                {
                    numericUpDown1.Value = Convert.ToDecimal(sqlReader["leftValue"]);
                    if (Convert.ToInt64(sqlReader["leftValueIncluded"]) == 0)
                        comboBox1.SelectedIndex = 0;
                    else
                        comboBox1.SelectedIndex = 1;
                    numericUpDown2.Value = Convert.ToDecimal(sqlReader["rightValue"]);
                    if (Convert.ToInt64(sqlReader["rightValueIncluded"]) == 0)
                        comboBox2.SelectedIndex = 0;
                    else
                        comboBox2.SelectedIndex = 1;
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

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value < leftValue)
                numericUpDown1.Value = leftValue;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown2.Value > rightValue)
                numericUpDown2.Value = rightValue;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value == leftValue && leftValueIncluded == 0)
                comboBox1.SelectedIndex = 0;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (numericUpDown2.Value == rightValue && rightValueIncluded == 0)
                comboBox2.SelectedIndex = 0;
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            string featureDescriptionId = "";
            SqlCommand command = new SqlCommand("SELECT Id FROM FeatureDescription WHERE Class=@class AND Feature=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", _featureId);
            command.Parameters.AddWithValue("class", _classId);
            SqlDataReader sqlReader = null;
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                await sqlReader.ReadAsync();
                featureDescriptionId = Convert.ToString(sqlReader["Id"]);
                sqlReader.Close();

                command = new SqlCommand("SELECT * FROM ClassDimensionValue WHERE Feature=@feature", sqlConnection);
                command.Parameters.AddWithValue("feature", featureDescriptionId);
                sqlReader = await command.ExecuteReaderAsync();
                bool recordExist = await sqlReader.ReadAsync();
                sqlReader.Close();

                if (recordExist)
                    command = new SqlCommand("UPDATE ClassDimensionValue " +
                    "SET leftValueIncluded=@leftValueIncluded, leftValue=@leftValue, " +
                    "rightValue=@rightValue, rightValueIncluded=@rightValueIncluded " +
                    "WHERE Feature=@Id", sqlConnection);
                else
                    command = new SqlCommand("INSERT INTO ClassDimensionValue " +
                        "(Feature, leftValueIncluded, leftValue, rightValue, rightValueIncluded)" +
                        "VALUES(@Id, @leftValueIncluded, @leftValue, @rightValue, @rightValueIncluded)", sqlConnection);
                command.Parameters.AddWithValue("Id", featureDescriptionId);
                command.Parameters.AddWithValue("leftValueIncluded", Convert.ToDecimal(comboBox1.SelectedIndex));
                command.Parameters.AddWithValue("leftValue", numericUpDown1.Value);
                command.Parameters.AddWithValue("rightValue", numericUpDown2.Value);
                command.Parameters.AddWithValue("rightValueIncluded", Convert.ToDecimal(comboBox2.SelectedIndex));
                await command.ExecuteNonQueryAsync();
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
            this.Close();
        }
    }
}
