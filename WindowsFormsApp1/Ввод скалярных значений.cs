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
    public partial class Ввод_скалярных_значений : Form
    {
        SqlConnection sqlConnection;
        string _feature = "";

        public Ввод_скалярных_значений(string feature)
        {
            _feature = feature;
            InitializeComponent();
        }

        private async void Ввод_скалярных_значений_Load(object sender, EventArgs e)
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

            SqlCommand command = new SqlCommand("SELECT ScalarValues.value " +
                "FROM Feature INNER JOIN ScalarValues ON Feature.Id=ScalarValues.Feature " +
                "WHERE Feature.Feature=@feature ORDER BY ScalarValues.value", sqlConnection);
            command.Parameters.AddWithValue("feature", _feature);

            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
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

        private void Ввод_скалярных_значений_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[2];
            ifrm.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                SqlCommand command = new SqlCommand("UPDATE Completeness SET " +
                    "completeness=@value", sqlConnection);
                command.Parameters.AddWithValue("value", 0);
                await command.ExecuteNonQueryAsync();

                command = new SqlCommand("SELECT ScalarValues.value " +
                "FROM Feature INNER JOIN ScalarValues ON Feature.Id=ScalarValues.Feature " +
                "WHERE Feature.Feature=@feature AND [value]=@value", sqlConnection);
                command.Parameters.AddWithValue("feature", _feature);
                command.Parameters.AddWithValue("value", textBox1.Text);
                bool recordExist = false;
                try
                {
                    SqlDataReader sqlReader = await command.ExecuteReaderAsync();
                    recordExist = await sqlReader.ReadAsync();
                    sqlReader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (recordExist)
                {
                    MessageBox.Show("Такое значение уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    SqlCommand command1 = new SqlCommand("SELECT [Id] FROM [Feature] WHERE [Feature].[Feature]=@feature", sqlConnection);
                    command1.Parameters.AddWithValue("feature", _feature);
                    SqlDataReader id = await command1.ExecuteReaderAsync();
                    await id.ReadAsync();

                    SqlCommand command2 = new SqlCommand("INSERT INTO [ScalarValues] (Feature, value)VALUES(@Id, @value)", sqlConnection);
                    command2.Parameters.AddWithValue("Id", id["Id"]);
                    command2.Parameters.AddWithValue("value", textBox1.Text);
                    id.Close();
                    await command2.ExecuteNonQueryAsync();

                    listBox1.Items.Add(textBox1.Text);
                    textBox1.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Введите значение!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            int chosenValue = listBox1.SelectedIndex;
            if (chosenValue != -1)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить выбранное значение?\n Восстановление удаленного значения будет невозможно.", "Предупреждение", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (result == DialogResult.OK)
                {
                    SqlCommand command = new SqlCommand("UPDATE Completeness SET " +
                        "completeness=@value", sqlConnection);
                    command.Parameters.AddWithValue("value", 0);
                    await command.ExecuteNonQueryAsync();

                    command = new SqlCommand("DELETE ScalarValues " +
                        "FROM Feature INNER JOIN ScalarValues ON Feature.Id=ScalarValues.Feature " +
                        "WHERE Feature.Feature=@feature AND value=@value", sqlConnection);

                    command.Parameters.AddWithValue("feature", _feature);
                    command.Parameters.AddWithValue("value", listBox1.Items[chosenValue]);
                    try
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex) { }

                    SqlCommand command2 = new SqlCommand("DELETE ClassScalarValues " +
                            "FROM (Feature INNER JOIN [FeatureDescription] ON Feature.Id=[FeatureDescription].Feature) " +
                            "INNER JOIN ClassScalarValues ON ClassScalarValues.Feature=[FeatureDescription].Id " +
                            "WHERE Feature.Feature=@feature AND value=@value", sqlConnection);

                    command2.Parameters.AddWithValue("feature", _feature);
                    command2.Parameters.AddWithValue("value", listBox1.Items[chosenValue]);
                    try
                    {
                        await command2.ExecuteReaderAsync();
                    }
                    catch (Exception ex) { }

                    listBox1.Items.RemoveAt(chosenValue);
                }
            }
            else
            {
                MessageBox.Show("Удаляемое значение не выбрано!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
