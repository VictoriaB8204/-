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
            
            List<List<string>> featureValues = new List<List<string>>(); //массив для хранения результата запроса возможных значений признака
            List<List<string>> classes = new List<List<string>>(); //массив для хранения результата запроса списка классов
            List<List<string>> featureDescription = new List<List<string>>(); //массив для хранения результата запроса признакового описания

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
                for(int i = 0; i < classes.Count; i++)
                {
                    command = new SqlCommand("SELECT [FeatureDescription].[Id], [Feature].[Feature], [Feature].[Type] " +
                        "FROM [FeatureDescription] INNER JOIN [Feature] ON [FeatureDescription].[Feature]=[Feature].[Id] " +
                        "WHERE [FeatureDescription].[Class]=@ClassId", sqlConnection);
                    command.Parameters.AddWithValue("ClassId", classes[i][0]);
                    try
                    {
                        //записываем результаты запроса признакового описания
                        sqlReader = await command.ExecuteReaderAsync();
                        if(!await sqlReader.ReadAsync())
                        {
                            listBox1.Items.Add("У класса " + classes[i][1] + " нет признакового описания");
                            completenessChecked = false;
                            continue;
                        }

                        do
                        {
                            featureDescription.Add(new List<string>());
                            featureDescription[featureDescription.Count - 1].Add(Convert.ToString(sqlReader["Id"]));
                            featureDescription[featureDescription.Count - 1].Add(Convert.ToString(sqlReader["Feature"]));
                            featureDescription[featureDescription.Count - 1].Add(Convert.ToString(sqlReader["Type"]));
                        } while (await sqlReader.ReadAsync());
                        sqlReader.Close();
                        
                        //проверим наличие значений признаков для класса
                        for(int j = 0; j < featureDescription.Count; j++)
                        {
                            if (featureDescription[j][2] == "Скалярный")
                                command = new SqlCommand("SELECT * FROM ClassScalarValues " +
                                    "WHERE ClassScalarValues.Id=@featureDescriptionId", sqlConnection);

                            if (featureDescription[j][2] == "Размерный")
                                command = new SqlCommand("SELECT * FROM ClassDimensionValue " +
                                    "WHERE ClassDimensionValue.Id=@featureDescriptionId", sqlConnection);

                            if (featureDescription[j][2] == "Логический")
                                command = new SqlCommand("SELECT * FROM ClassLogicalValues " +
                                    "WHERE ClassLogicalValues.Id=@featureDescriptionId", sqlConnection);

                            command.Parameters.AddWithValue("featureDescriptionId", featureDescription[j][0]);

                            try
                            {
                                sqlReader = await command.ExecuteReaderAsync();

                                //проверим возможные значения признака
                                await sqlReader.ReadAsync();
                                
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
                    "completeness=@True", sqlConnection);
                command.Parameters.AddWithValue("True", 1);
                await command.ExecuteNonQueryAsync();
            }
            else
            {
                command = new SqlCommand("UPDATE Completeness SET " +
                    "completeness=@False", sqlConnection);
                command.Parameters.AddWithValue("False", 0);
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
