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
    public partial class Признаковое_описание_классов : Form
    {
        SqlConnection sqlConnection;
        string classId = "";

        public Признаковое_описание_классов()
        {
            InitializeComponent();
        }

        private async void Признаковое_описание_классов_Load(object sender, EventArgs e)
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

        private void Признаковое_описание_классов_FormClosing(object sender, FormClosingEventArgs e)
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
            SqlDataReader sqlReader = null;
            listBox1.Items.Clear();
            listBox2.Items.Clear();

            SqlCommand command = new SqlCommand("SELECT [Id] FROM [Classes] WHERE [Class]=@class", sqlConnection);
            command.Parameters.AddWithValue("class", comboBox1.SelectedItem);
            sqlReader = await command.ExecuteReaderAsync();
            await sqlReader.ReadAsync();
            classId = Convert.ToString(sqlReader["Id"]);
            sqlReader.Close();

            //Выбранные признаки
            command = new SqlCommand("SELECT Feature.Feature " +
                "FROM [FeatureDescription] INNER JOIN Feature " +
                "ON [FeatureDescription].[Feature]=Feature.Id " +
                "WHERE Class=@class ORDER BY Feature", sqlConnection);
            command.Parameters.AddWithValue("class", classId);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    listBox2.Items.Add(Convert.ToString(sqlReader["Feature"]));
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
            
            //Невыбранные признаки
            command = new SqlCommand("SELECT [Feature] FROM [Feature] ORDER BY Feature", sqlConnection);
            try
            {
                sqlReader = await command.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    if (listBox2.FindString(Convert.ToString(sqlReader["Feature"])) != -1)
                        continue;

                    listBox1.Items.Add(Convert.ToString(sqlReader["Feature"]));
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

        private async void button1_Click(object sender, EventArgs e)
        {
            if (classId == "")
            {
                MessageBox.Show("Выберите класс!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("Признаки перенесены!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SqlDataReader sqlReader = null;

            SqlCommand command = new SqlCommand("UPDATE Completeness SET " +
                "completeness=@value", sqlConnection);
            command.Parameters.AddWithValue("value", 0);
            await command.ExecuteNonQueryAsync();

            string featureId = "";
            while(listBox1.Items.Count > 0)
            {
                command = new SqlCommand("SELECT [Id] FROM [Feature] WHERE [Feature]=@feature", sqlConnection);
                command.Parameters.AddWithValue("feature", listBox1.Items[0]);
                sqlReader = await command.ExecuteReaderAsync();
                await sqlReader.ReadAsync();
                featureId = Convert.ToString(sqlReader["Id"]);
                sqlReader.Close();

                command = new SqlCommand("INSERT INTO [FeatureDescription] " +
                    "(Class, Feature)VALUES(@class, @feature)", sqlConnection);
                command.Parameters.AddWithValue("class", classId);
                command.Parameters.AddWithValue("feature", featureId);
                await command.ExecuteNonQueryAsync();

                listBox2.Items.Add(listBox1.Items[0]);
                listBox1.Items.RemoveAt(0);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (classId == "")
            {
                MessageBox.Show("Выберите класс!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Выберите признак!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SqlDataReader sqlReader = null;

            SqlCommand command = new SqlCommand("UPDATE Completeness SET " +
                "completeness=@value", sqlConnection);
            command.Parameters.AddWithValue("value", 0);
            await command.ExecuteNonQueryAsync();

            string featureId = "";

            command = new SqlCommand("SELECT [Id] FROM [Feature] WHERE [Feature]=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", listBox1.SelectedItem);
            sqlReader = await command.ExecuteReaderAsync();
            await sqlReader.ReadAsync();
            featureId = Convert.ToString(sqlReader["Id"]);
            sqlReader.Close();

            command = new SqlCommand("INSERT INTO [FeatureDescription] " +
                "(Class, Feature)VALUES(@class, @feature)", sqlConnection);
            command.Parameters.AddWithValue("class", classId);
            command.Parameters.AddWithValue("feature", featureId);
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }

            listBox2.Items.Add(listBox1.SelectedItem);
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (classId == "")
            {
                MessageBox.Show("Выберите класс!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (listBox2.SelectedItem == null)
            {
                MessageBox.Show("Выберите признак!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SqlDataReader sqlReader = null;
            
            SqlCommand command = new SqlCommand("UPDATE Completeness SET " +
                "completeness=@value", sqlConnection);
            command.Parameters.AddWithValue("value", 0);
            await command.ExecuteNonQueryAsync();

            string featureId = "";

            command = new SqlCommand("SELECT [Id] FROM [Feature] WHERE [Feature]=@feature", sqlConnection);
            command.Parameters.AddWithValue("feature", listBox2.SelectedItem);
            sqlReader = await command.ExecuteReaderAsync();
            await sqlReader.ReadAsync();
            featureId = Convert.ToString(sqlReader["Id"]);
            sqlReader.Close();

            command = new SqlCommand("DELETE FROM [FeatureDescription] " +
                "WHERE Feature=@feature AND Class=@class", sqlConnection);
            command.Parameters.AddWithValue("class", classId);
            command.Parameters.AddWithValue("feature", featureId);
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex) { }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }

            listBox1.Items.Add(listBox2.SelectedItem);
            listBox2.Items.RemoveAt(listBox2.SelectedIndex);
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (classId == "")
            {
                MessageBox.Show("Выберите класс!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("Признаки перенесены!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SqlDataReader sqlReader = null;

            SqlCommand command = new SqlCommand("UPDATE Completeness SET " +
                "completeness=@value", sqlConnection);
            command.Parameters.AddWithValue("value", 0);
            await command.ExecuteNonQueryAsync();

            string featureId = "";

            while(listBox2.Items.Count > 0)
            {
                command = new SqlCommand("SELECT [Id] FROM [Feature] WHERE [Feature]=@feature", sqlConnection);
                command.Parameters.AddWithValue("feature", listBox2.Items[0]);
                sqlReader = await command.ExecuteReaderAsync();
                await sqlReader.ReadAsync();
                featureId = Convert.ToString(sqlReader["Id"]);
                sqlReader.Close();

                command = new SqlCommand("DELETE FROM [FeatureDescription] " +
                "WHERE Feature=@feature AND Class=@class", sqlConnection);
                command.Parameters.AddWithValue("class", classId);
                command.Parameters.AddWithValue("feature", featureId);
                try
                {
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex) { }
                finally
                {
                    if (sqlReader != null)
                        sqlReader.Close();
                }

                listBox1.Items.Add(listBox2.Items[0]);
                listBox2.Items.RemoveAt(0);
            }
        }
    }
}
