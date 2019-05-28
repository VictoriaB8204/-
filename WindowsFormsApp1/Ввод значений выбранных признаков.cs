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
    public partial class Ввод_значений_выбранных_признаков : Form
    {
        SqlConnection sqlConnection;
        ListBox.ObjectCollection features;

        public Ввод_значений_выбранных_признаков(ListBox.ObjectCollection _features)
        {
            InitializeComponent();
            features = _features;
        }

        private async void Ввод_значений_выбранных_признаков_Load(object sender, EventArgs e)
        {
            //Центрирование формы
            if (Owner != null)
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                Owner.Location.Y + Owner.Height / 2 - Height / 2);

            //заполнение значениями из БД
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\учеба\Смагин\Реализация классификатора\WindowsFormsApp1\WindowsFormsApp1\Database1.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            await sqlConnection.OpenAsync();
        }

        private void Выбор_признаков_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            // вызываем главную форму приложения, которая открыла текущую форму Form2, главная форма всегда = 0
            Form ifrm = Application.OpenForms[0];
            ifrm.Show();
        }

    }
}
