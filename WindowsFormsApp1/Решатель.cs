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
    public partial class Решатель : Form
    {
        SqlConnection sqlConnection;
        List<List<string>> features;
        List<List<string>> classes;

        public Решатель(List<List<string>> _features)
        {
            InitializeComponent();
            features = new List<List<string>>();
            features = _features;
        }

        private async void Решатель_Load(object sender, EventArgs e)
        {
            //Центрирование формы
            if (Owner != null)
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                Owner.Location.Y + Owner.Height / 2 - Height / 2);

            //подключение БД
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\учеба\Смагин\Реализация классификатора\WindowsFormsApp1\WindowsFormsApp1\Database1.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();

            //запрашиваем список классов
            classes = new List<List<string>>();
            SqlCommand command = new SqlCommand("SELECT * FROM Classes", sqlConnection);
            SqlDataReader sqlReader = null;
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while(await sqlReader.ReadAsync())
                {
                    classes.Add(new List<string>());
                    classes[classes.Count - 1].Add(Convert.ToString(sqlReader["Id"]));
                    classes[classes.Count - 1].Add(Convert.ToString(sqlReader["Class"]));
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!sqlReader.IsClosed)
                    sqlReader.Close();
            }

            //классифицируем
            string featureDescriptionId = "";
            bool isSuitableСlass;
            for (int i = 0; i < features.Count; i++)
                for (int j = 0; j < classes.Count; j++)
                {
                    isSuitableСlass = false;
                    //нужно узнать является ли класс подходящим для признака с таким значением
                    //для этого нужно:
                    //знать идектификатор признакового описания
                    command = new SqlCommand("SELECT FeatureDescription.Id " +
                        "FROM FeatureDescription " +
                        "WHERE Feature = @featureId AND Class = @classId", sqlConnection);
                    command.Parameters.AddWithValue("featureId", features[i][1]);
                    command.Parameters.AddWithValue("classId", classes[j][0]);
                    //искать классы раньше
                    try
                    {
                        sqlReader = await command.ExecuteReaderAsync();
                        if (await sqlReader.ReadAsync())
                            featureDescriptionId = Convert.ToString(sqlReader["Id"]);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        if (!sqlReader.IsClosed)
                            sqlReader.Close();
                    }
                    
                    //для скалярных признаков нужно искать записи с равным значением
                    if(features[i][2] == "Скалярный")
                    {
                        command = new SqlCommand("SELECT * FROM ClassScalarValues WHERE Feature=@feature AND value=@value", sqlConnection);
                        command.Parameters.AddWithValue("feature", featureDescriptionId);
                        command.Parameters.AddWithValue("value", features[i][3]);
                        try
                        {
                            sqlReader = await command.ExecuteReaderAsync();
                            if (await sqlReader.ReadAsync())
                                isSuitableСlass = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            if (!sqlReader.IsClosed)
                                sqlReader.Close();
                        }
                    }

                    //для размерных искать признак с значением внутри интервала
                    if (features[i][2] == "Размерный")
                    {
                        command = new SqlCommand("SELECT * FROM ClassDimensionValue WHERE Feature=@feature", sqlConnection);
                        command.Parameters.AddWithValue("feature", featureDescriptionId);
                        try
                        {
                            sqlReader = await command.ExecuteReaderAsync();
                            if (await sqlReader.ReadAsync())
                            {
                                //если значение строго между левым и правым, то подходит
                                if (Convert.ToDecimal(features[i][3]) > Convert.ToDecimal(sqlReader["leftValue"]) &&
                                    Convert.ToDecimal(features[i][3]) < Convert.ToDecimal(sqlReader["rightValue"]))
                                    isSuitableСlass = true;

                                //если значение = левому значению И левое значение включено, то тоже подходит
                                if (Convert.ToDecimal(features[i][3]) == Convert.ToDecimal(sqlReader["leftValue"]) &&
                                    Convert.ToString(sqlReader["leftValueIncluded"]) == "1")
                                    isSuitableСlass = true;

                                //если значение = правому значению И правое значение включено, то тоже подходит
                                if (Convert.ToDecimal(features[i][3]) == Convert.ToDecimal(sqlReader["rightValue"]) &&
                                    Convert.ToString(sqlReader["rightValueIncluded"]) == "1")
                                    isSuitableСlass = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            if (!sqlReader.IsClosed)
                                sqlReader.Close();
                        }
                    }

                    //для логических искать записи, гле нужное значение не NULL
                    if (features[i][2] == "Логический")
                    {
                        if (features[i][3] == "TrueValue")
                            command = new SqlCommand("SELECT * FROM ClassLogicalValues WHERE Feature=@feature AND TrueValue IS NOT NULL", sqlConnection);
                        else
                            command = new SqlCommand("SELECT * FROM ClassLogicalValues WHERE Feature=@feature AND FalseValue IS NOT NULL", sqlConnection);

                        command.Parameters.AddWithValue("feature", featureDescriptionId);
                        try
                        {
                            sqlReader = await command.ExecuteReaderAsync();
                            if (await sqlReader.ReadAsync())
                                isSuitableСlass = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            if (!sqlReader.IsClosed)
                                sqlReader.Close();
                        }
                    }
                    
                    //если класс не подходит и у него пустая причина, то добавить причину
                    if (!isSuitableСlass && classes[j].Count == 2)
                        classes[j].Add("Признак " + features[i][0] + " имеет не подходящее значение");
                }

            //выводим результат
            DataTable unsuitableClasses = new DataTable();
            unsuitableClasses.Columns.Add("Класс");
            unsuitableClasses.Columns.Add("Причина");
            for (int i = 0; i < classes.Count; i++)
            {
                if (classes[i].Count == 2)
                    listBox1.Items.Add(classes[i][1]);
                else
                {
                    DataRow r = unsuitableClasses.NewRow();
                    r["Класс"] = classes[i][1];
                    r["Причина"] = classes[i][2];
                    unsuitableClasses.Rows.Add(r);
                }
            }

            if (listBox1.Items.Count == 0)
                listBox1.Items.Add("Подходящий класс не найден");

            dataGridView1.DataSource = unsuitableClasses;
        }

        private void Решатель_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            Form ifrm = Application.OpenForms[2];
            ifrm.Close();
            ifrm = Application.OpenForms[1];
            ifrm.Close();
            ifrm = Application.OpenForms[0];
            ifrm.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
