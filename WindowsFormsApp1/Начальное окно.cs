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
    public partial class MainForm : Form
    {
        SqlConnection sqlConnection;

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form ifrm = new Form2();
            this.Hide(); // скрываем Form1
            ifrm.ShowDialog(this);
            UpdateForm();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!button2.IsAccessible)
                MessageBox.Show("Данная кнопка недоступна, так как не пройдена проверка полноты базы знаний.",
                    "Информация",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\учеба\Смагин\Реализация классификатора\WindowsFormsApp1\WindowsFormsApp1\Database1.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
            SqlDataReader sqlReader = null;
            
            SqlCommand command = new SqlCommand("SELECT completeness FROM Completeness", sqlConnection);
            sqlReader = await command.ExecuteReaderAsync();
            try
            {
                if(!await sqlReader.ReadAsync())
                {
                    sqlReader.Close();
                    command = new SqlCommand("INSERT INTO Completeness (completeness) VALUES (0)", sqlConnection);
                    await command.ExecuteNonQueryAsync();
                }

                if (Convert.ToInt64(sqlReader["completeness"]) == 0)
                {
                    button2.IsAccessible = false;
                    button2.Enabled = false;
                }
                else
                {
                    button2.IsAccessible = true;
                    button2.Enabled = true;
                }
            }
            catch
            {
                button2.IsAccessible = false;
                button2.Enabled = false;
            }
            sqlReader.Close();
        }

        private async void UpdateForm()
        {
            SqlCommand command = new SqlCommand("SELECT completeness FROM Completeness", sqlConnection);
            SqlDataReader sqlReader = await command.ExecuteReaderAsync();
            await sqlReader.ReadAsync();
            try
            {
                if (Convert.ToString(sqlReader["completeness"]) == "0")
                {
                    button2.IsAccessible = false;
                    button2.Enabled = false;
                }
                else
                {
                    button2.IsAccessible = true;
                    button2.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                button2.IsAccessible = false;
                button2.Enabled = false;
            }
            sqlReader.Close();
        }
    }
}
