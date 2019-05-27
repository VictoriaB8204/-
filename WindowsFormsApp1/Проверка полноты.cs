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
    public partial class Проверка_полноты : Form
    {
        SqlConnection sqlConnection;

        public Проверка_полноты()
        {
            InitializeComponent();
        }

        private async void Проверка_полноты_Load(object sender, EventArgs e)
        {
            //Центрирование формы
            if (Owner != null)
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                Owner.Location.Y + Owner.Height / 2 - Height / 2);

            //выполняем проверку полноты
            bool completenessChecked = true;

            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\учеба\Смагин\Реализация классификатора\WindowsFormsApp1\WindowsFormsApp1\Database1.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            SqlDataReader sqlReader = null;

            List<string> featureValues = new List<string>(); //массив для хранения результата запроса возможных значений признака
            List<List<string>> classes = new List<List<string>>(); //массив для хранения результата запроса списка классов
            List<List<string>> featureDescription = new List<List<string>>(); //массив для хранения результата запроса признакового описания
            List<string> featureClassValues = new List<string>(); //массив для хранения результата запроса значений признака для класса

            //получаем список классов
            SqlCommand command = new SqlCommand("SELECT * FROM [Classes] ORDER BY Class", sqlConnection);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                //записываем результаты запроса
                while (await sqlReader.ReadAsync())
                {
                    classes.Add(new List<string>());
                    classes[classes.Count - 1].Add(Convert.ToString(sqlReader["Id"]));
                    classes[classes.Count - 1].Add(Convert.ToString(sqlReader["Class"]));
                }
                sqlReader.Close();

                //для каждого класса получаем признаковое описание
                for (int i = 0; i < classes.Count; i++)
                {
                    command = new SqlCommand("SELECT [FeatureDescription].[Id] As Id, [Feature].[Feature], [Feature].[Type], [Feature].[Id] As FeatureId " +
                        "FROM [FeatureDescription] INNER JOIN [Feature] ON [FeatureDescription].[Feature]=[Feature].[Id] " +
                        "WHERE [FeatureDescription].[Class]=@ClassId", sqlConnection);
                    command.Parameters.AddWithValue("ClassId", classes[i][0]);
                    try
                    {
                        //записываем результаты запроса признакового описания
                        sqlReader = await command.ExecuteReaderAsync();
                        if (!await sqlReader.ReadAsync())
                        {
                            listBox1.Items.Add("У класса " + classes[i][1] + " нет признакового описания");
                            completenessChecked = false;
                            sqlReader.Close();
                            continue;
                        }

                        do
                        {
                            featureDescription.Add(new List<string>());
                            featureDescription[featureDescription.Count - 1].Add(Convert.ToString(sqlReader["Id"]));
                            featureDescription[featureDescription.Count - 1].Add(Convert.ToString(sqlReader["Feature"]));
                            featureDescription[featureDescription.Count - 1].Add(Convert.ToString(sqlReader["Type"]));
                            featureDescription[featureDescription.Count - 1].Add(Convert.ToString(sqlReader["FeatureId"]));
                        } while (await sqlReader.ReadAsync());
                        sqlReader.Close();

                        //проверим наличие значений признаков для класса
                        for (int j = 0; j < featureDescription.Count; j++)
                        {
                            if (featureDescription[j][2] == "")
                            {
                                listBox1.Items.Add("У признака " + featureDescription[j][1]
                                    + " отсутствуют возможные значения");
                                listBox1.Items.Add("В классе " + classes[i][1]
                                    + " у признака " + featureDescription[j][1]
                                    + " отсутствуют значения");
                                completenessChecked = false;
                                continue;
                            }

                            if (featureDescription[j][2] == "Скалярный")
                                command = new SqlCommand("SELECT * FROM ClassScalarValues " +
                                    "WHERE ClassScalarValues.Feature=@featureDescriptionId", sqlConnection);

                            if (featureDescription[j][2] == "Размерный")
                                command = new SqlCommand("SELECT * FROM ClassDimensionValue " +
                                    "WHERE ClassDimensionValue.Feature=@featureDescriptionId", sqlConnection);

                            if (featureDescription[j][2] == "Логический")
                                command = new SqlCommand("SELECT * FROM ClassLogicalValues " +
                                    "WHERE ClassLogicalValues.Feature=@featureDescriptionId", sqlConnection);

                            command.Parameters.AddWithValue("featureDescriptionId", featureDescription[j][0]);

                            try
                            {
                                sqlReader = await command.ExecuteReaderAsync();

                                //запись результатов
                                if (!await sqlReader.ReadAsync())
                                {
                                    listBox1.Items.Add("У класса " + classes[i][1]
                                    + " у признака " + featureDescription[j][1] + " отсутствует значение");
                                    completenessChecked = false;
                                    sqlReader.Close();
                                    continue;
                                }

                                featureClassValues.Clear();
                                do
                                {
                                    if (featureDescription[j][2] == "Скалярный")
                                        featureClassValues.Add(Convert.ToString(sqlReader["value"]));

                                    if (featureDescription[j][2] == "Размерный")
                                    {
                                        featureClassValues.Add(Convert.ToString(sqlReader["leftValueIncluded"]));
                                        featureClassValues.Add(Convert.ToString(sqlReader["leftValue"]));
                                        featureClassValues.Add(Convert.ToString(sqlReader["rightValue"]));
                                        featureClassValues.Add(Convert.ToString(sqlReader["rightValueIncluded"]));
                                    }

                                    if (featureDescription[j][2] == "Логический")
                                    {
                                        featureClassValues.Add(Convert.ToString(sqlReader["TrueValue"]));
                                        featureClassValues.Add(Convert.ToString(sqlReader["FalseValue"]));
                                    }
                                } while (await sqlReader.ReadAsync());
                                sqlReader.Close();

                                //поиск возможных значений
                                if (featureDescription[j][2] == "Скалярный")
                                    command = new SqlCommand("SELECT * FROM ScalarValues " +
                                        "WHERE ScalarValues.Feature=@featuresId", sqlConnection);

                                if (featureDescription[j][2] == "Размерный")
                                    command = new SqlCommand("SELECT * FROM DimensionValue " +
                                        "WHERE DimensionValue.Feature=@featuresId", sqlConnection);

                                command.Parameters.AddWithValue("featuresId", featureDescription[j][3]);

                                try
                                {
                                    sqlReader = await command.ExecuteReaderAsync();

                                    //запись результатов
                                    featureValues.Clear();
                                    while (await sqlReader.ReadAsync())
                                    {
                                        if (featureDescription[j][2] == "Скалярный")
                                            featureValues.Add(Convert.ToString(sqlReader["value"]));

                                        if (featureDescription[j][2] == "Размерный")
                                        {
                                            featureValues.Add(Convert.ToString(sqlReader["leftValueIncluded"]));
                                            featureValues.Add(Convert.ToString(sqlReader["leftValue"]));
                                            featureValues.Add(Convert.ToString(sqlReader["rightValue"]));
                                            featureValues.Add(Convert.ToString(sqlReader["rightValueIncluded"]));
                                        }
                                    }
                                    sqlReader.Close();

                                    //проверка значений признаков для классов
                                    if (featureDescription[j][2] == "Скалярный")
                                        for (int k = 0; k < featureClassValues.Count; k++)
                                        {
                                            if (featureValues.Find(value => value == featureClassValues[k]) == null)
                                            {
                                                listBox1.Items.Add("В классе " + classes[i][1]
                                                    + " у признака " + featureDescription[j][1]
                                                    + " значение " + featureClassValues[k] + " отсутствует в списке возможных значений");
                                                completenessChecked = false;
                                            }
                                        }

                                    if (featureDescription[j][2] == "Размерный")
                                    {
                                        if (Convert.ToDouble(featureValues[1]) > Convert.ToDouble(featureClassValues[1]))
                                        {
                                            listBox1.Items.Add("В классе " + classes[i][1]
                                                + " у признака " + featureDescription[j][1]
                                                + " значение " + featureClassValues[1] + " выходит за границы возможных значений этого признака");
                                            completenessChecked = false;
                                        }

                                        if (Convert.ToDouble(featureValues[1]) == Convert.ToDouble(featureClassValues[1]) &&
                                            Convert.ToInt32(featureValues[0]) == 0)
                                        {
                                            listBox1.Items.Add("В классе " + classes[i][1]
                                                + " у признака " + featureDescription[j][1]
                                                + " значение " + featureClassValues[1] + " выходит за границы возможных значений этого признака");
                                            completenessChecked = false;
                                        }

                                        if (Convert.ToDouble(featureValues[2]) < Convert.ToDouble(featureClassValues[2]))
                                        {
                                            listBox1.Items.Add("В классе " + classes[i][1]
                                                + " у признака " + featureDescription[j][1]
                                                + " значение " + featureClassValues[2] + " выходит за границы возможных значений этого признака");
                                            completenessChecked = false;
                                        }

                                        if (Convert.ToDouble(featureValues[2]) == Convert.ToDouble(featureClassValues[2]) &&
                                            Convert.ToInt32(featureValues[3]) == 0)
                                        {
                                            listBox1.Items.Add("В классе " + classes[i][1]
                                                + " у признака " + featureDescription[j][1]
                                                + " значение " + featureClassValues[2] + " выходит за границы возможных значений этого признака");
                                            completenessChecked = false;
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    sqlReader.Close();
                                    completenessChecked = false;
                                    continue;
                                }
                                finally
                                {
                                    if (sqlReader != null)
                                        sqlReader.Close();
                                }

                            }
                            catch
                            {
                                listBox1.Items.Add("В классе " + classes[i][1]
                                    + " у признака " + featureDescription[j][1] + " отсутствует значение");
                                completenessChecked = false;
                            }
                            finally
                            {
                                if (sqlReader != null)
                                    sqlReader.Close();
                            }
                        }

                        featureDescription.Clear();
                    }
                    catch
                    {
                        listBox1.Items.Add("У класса " + classes[i][1] + " нет признакового описания");
                        completenessChecked = false;
                    }
                    finally
                    {
                        if (sqlReader != null)
                            sqlReader.Close();
                    }
                }
            }
            catch
            {
                listBox1.Items.Add("Список классов пуст");
            }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }
            classes.Clear();

            //запись результатов проверки полноты
            if (completenessChecked)
            {
                listBox1.Items.Add("Проверка полноты пройдена");
                command = new SqlCommand("UPDATE Completeness SET " +
                    "completeness=@value", sqlConnection);
                command.Parameters.AddWithValue("value", 1);
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                command = new SqlCommand("UPDATE Completeness SET " +
                    "completeness=@value", sqlConnection);
                command.Parameters.AddWithValue("value", 0);
                await command.ExecuteNonQueryAsync();
            }
        }

        private void Проверка_полноты_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[1];
            ifrm.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
