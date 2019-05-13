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
    public partial class Ввод_логических_значений_для_классов : Form
    {
        SqlConnection sqlConnection;
        string _class = "";
        string _feature = "";
        string _classId = "";
        string _featureId = "";

        public Ввод_логических_значений_для_классов(string Class, string feature)
        {
            _class = Class;
            _feature = feature;
            InitializeComponent();
        }

        private async void Ввод_логических_значений_для_классов_Load(object sender, EventArgs e)
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
            
            command = new SqlCommand("SELECT * " +
                "FROM LogicalValues WHERE Feature=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", _featureId);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                if (await sqlReader.ReadAsync())
                {
                    if (Convert.ToString(sqlReader["TrueValue"]) != "")
                        checkBox1.Text = Convert.ToString(sqlReader["TrueValue"]);
                    if (Convert.ToString(sqlReader["FalseValue"]) != "")
                        checkBox2.Text = Convert.ToString(sqlReader["FalseValue"]);
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

            //Заполняем список выбранных значений
            command = new SqlCommand("SELECT * " +
                "FROM ClassLogicalValues INNER JOIN FeatureDescription ON ClassLogicalValues.Feature=FeatureDescription.Id " +
                "WHERE FeatureDescription.Feature=@feature AND FeatureDescription.Class=@class", sqlConnection);
            command.Parameters.AddWithValue("feature", _featureId);
            command.Parameters.AddWithValue("class", _classId);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                if (await sqlReader.ReadAsync())
                { 
                    if(Convert.ToString(sqlReader["TrueValue"]) != "")
                        checkBox1.Checked = true;
                    if (Convert.ToString(sqlReader["FalseValue"]) != "")
                        checkBox2.Checked = true;
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

        private void Ввод_логических_значений_для_классов_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[2];
            ifrm.Show();
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            SqlDataReader sqlReader = null;
            SqlCommand command = null;
            string featureDescriptionId = "";
            command = new SqlCommand("SELECT Id FROM FeatureDescription WHERE Class=@class AND Feature=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", _featureId);
            command.Parameters.AddWithValue("class", _classId);
            sqlReader = await command.ExecuteReaderAsync();
            await sqlReader.ReadAsync();
            featureDescriptionId = Convert.ToString(sqlReader["Id"]);
            sqlReader.Close();

            command = new SqlCommand("SELECT * FROM ClassLogicalValues WHERE Feature=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", featureDescriptionId);
            sqlReader = await command.ExecuteReaderAsync();
            bool recordExists = await sqlReader.ReadAsync();
            sqlReader.Close();

            bool emptyValues = !checkBox1.Checked && !checkBox2.Checked;

            if (recordExists && !emptyValues)
                command = new SqlCommand("UPDATE ClassLogicalValues " +
                   "SET TrueValue=@TrueValue, FalseValue=@FalseValue " +
                   "WHERE Feature=@Id", sqlConnection);
            
            if (!recordExists && !emptyValues)
                command = new SqlCommand("INSERT INTO ClassLogicalValues (Feature, TrueValue, FalseValue)" +
                    "VALUES(@Id, @TrueValue, @FalseValue)", sqlConnection);

            if (recordExists && emptyValues)
                command = new SqlCommand("DELETE FROM ClassLogicalValues WHERE Feature=@Id", sqlConnection);

            if(!recordExists && emptyValues)
            {
                this.Close();
                return;
            }

            command.Parameters.AddWithValue("Id", featureDescriptionId);

            if(checkBox1.Checked)
                command.Parameters.AddWithValue("TrueValue", checkBox1.Text);
            else
                command.Parameters.AddWithValue("TrueValue", Convert.DBNull);
            if (checkBox2.Checked)
                command.Parameters.AddWithValue("FalseValue", checkBox2.Text);
            else
                command.Parameters.AddWithValue("FalseValue", Convert.DBNull);

            await command.ExecuteNonQueryAsync();

            this.Close();
        }
    }
}
