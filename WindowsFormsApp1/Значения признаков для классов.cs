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
    public partial class Значения_признаков_для_классов : Form
    {
        SqlConnection sqlConnection;
        string classId = "";

        public Значения_признаков_для_классов()
        {
            InitializeComponent();
        }

        private async void Значения_признаков_для_классов_Load(object sender, EventArgs e)
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
                while (await sqlReader.ReadAsync())
                {
                    comboBox1.Items.Add(Convert.ToString(sqlReader["Class"]));
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

        private void Значения_признаков_для_классов_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[1];
            ifrm.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable features = new DataTable();
            features.Columns.Add("Признак");
            features.Columns.Add("Тип");
            features.Columns.Add("Значение");

            SqlDataReader sqlReader = null;
            SqlCommand command = new SqlCommand("SELECT [Id] FROM [Classes] WHERE Class=@class", sqlConnection);
            command.Parameters.AddWithValue("class", comboBox1.SelectedItem);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                await sqlReader.ReadAsync();
                classId = Convert.ToString(sqlReader["Id"]);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ввыберите класс!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }

            command = new SqlCommand("SELECT Feature.Feature, Feature.Type " +
                "FROM Feature INNER JOIN FeatureDescription ON Feature.Id=FeatureDescription.Feature " +
                "WHERE FeatureDescription.Class=@class " +
                "ORDER BY Feature.Feature", sqlConnection);
            command.Parameters.AddWithValue("class", classId);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    DataRow r = features.NewRow();
                    r["Признак"] = sqlReader["Feature"];
                    r["Тип"] = sqlReader["Type"];
                    features.Rows.Add(r);
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
            
            for (int i = 0; i < features.Rows.Count; i++)
            {
                if (Convert.ToString(features.Rows[i]["Тип"]) == "Скалярный")
                    command = new SqlCommand("SELECT * " +
                        "FROM (ClassScalarValues INNER JOIN FeatureDescription ON ClassScalarValues.Feature=FeatureDescription.Id) " +
                        "INNER JOIN Feature ON FeatureDescription.Feature = Feature.Id " +
                        "WHERE FeatureDescription.Class=@class AND Feature.Feature=@feature", sqlConnection);

                if (Convert.ToString(features.Rows[i]["Тип"]) == "Размерный")
                    command = new SqlCommand("SELECT * " +
                        "FROM (ClassDimensionValue INNER JOIN FeatureDescription ON ClassDimensionValue.Feature=FeatureDescription.Id) " +
                        "INNER JOIN Feature ON FeatureDescription.Feature = Feature.Id " +
                        "WHERE FeatureDescription.Class=@class AND Feature.Feature=@feature", sqlConnection);

                if (Convert.ToString(features.Rows[i]["Тип"]) == "Логический")
                    command = new SqlCommand("SELECT * " +
                        "FROM (ClassLogicalValues INNER JOIN FeatureDescription ON ClassLogicalValues.Feature=FeatureDescription.Id) " +
                        "INNER JOIN Feature ON FeatureDescription.Feature = Feature.Id " +
                        "WHERE FeatureDescription.Class=@class AND Feature.Feature=@feature", sqlConnection);

                command.Parameters.AddWithValue("class", classId);
                command.Parameters.AddWithValue("feature", features.Rows[i]["Признак"]);

                try
                {
                    sqlReader = await command.ExecuteReaderAsync();
                    if (await sqlReader.ReadAsync())
                        features.Rows[i]["Значение"] = "Внесено";
                    else
                        features.Rows[i]["Значение"] = "Не внесено";
                }
                catch { }
                finally
                {
                    if (sqlReader != null)
                        sqlReader.Close();
                }
            }
            
            dataGridView1.DataSource = features;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int i = dataGridView1.SelectedCells[0].RowIndex;

            if (Convert.ToString(dataGridView1.Rows[i].Cells[1].EditedFormattedValue) == "")
            {
                MessageBox.Show("Введите возможные значения для признака!","Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Convert.ToString(dataGridView1.Rows[i].Cells[1].EditedFormattedValue) == "Скалярный")
            {
                Form ifrm = new Ввод_скалярных_значений_для_классов(Convert.ToString(comboBox1.SelectedItem),
                    Convert.ToString(dataGridView1.Rows[i].Cells[0].EditedFormattedValue));
                this.Hide();
                ifrm.ShowDialog(this);
                UpdateForm();
            }

            if (Convert.ToString(dataGridView1.Rows[i].Cells[1].EditedFormattedValue) == "Размерный")
            {
                Form ifrm = new Ввод_размерных_значений_для_классов(Convert.ToString(comboBox1.SelectedItem),
                    Convert.ToString(dataGridView1.Rows[i].Cells[0].EditedFormattedValue));
                this.Hide();
                ifrm.ShowDialog(this);
                UpdateForm();
            }

            if (Convert.ToString(dataGridView1.Rows[i].Cells[1].EditedFormattedValue) == "Логический")
            {
                Form ifrm = new Ввод_логических_значений_для_классов(Convert.ToString(comboBox1.SelectedItem),
                    Convert.ToString(dataGridView1.Rows[i].Cells[0].EditedFormattedValue));
                this.Hide();
                ifrm.ShowDialog(this);
                UpdateForm();
            }
        }

        private void UpdateForm()
        {
            System.EventArgs ev = null;
            comboBox1_SelectedIndexChanged(comboBox1, ev);
        }
    }
}
