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
        List<List<string>> features;

        public Ввод_значений_выбранных_признаков(List<string> _features)
        {
            InitializeComponent();

            features = new List<List<string>>();
            for(int i = 0; i < _features.Count; i++)
            {
                features.Add(new List<string>());
                features[features.Count - 1].Add(_features[i]);
            }
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

            for(int i = 0; i < features.Count; i++)
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Feature WHERE Feature=@feature", sqlConnection);
                command.Parameters.AddWithValue("feature", features[i][0]);
                SqlDataReader sqlReader = null;
                try
                {
                    sqlReader = await command.ExecuteReaderAsync();
                    if(await sqlReader.ReadAsync())
                    {
                        features[i].Add(Convert.ToString(sqlReader["Id"]));
                        features[i].Add(Convert.ToString(sqlReader["Type"]));
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

                listBox1.Items.Add(features[i][0]);
            }
        }

        private void Ввод_значений_выбранных_признаков_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
            Form ifrm = Application.OpenForms[1];
            ifrm.Show();
        }

        private async void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            //скрытие элементов формы
            label3.Visible = true;
            comboBox1.Visible = false;
            comboBox1.SelectedItem = null;
            comboBox1.Items.Clear();

            radioButton1.Visible = false;
            radioButton1.Checked = false;
            radioButton2.Visible = false;
            radioButton2.Checked = false;

            label2.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            numericUpDown1.Visible = false;
            numericUpDown1.Minimum = 0;
            numericUpDown1.Maximum = 1000000;
            numericUpDown1.Value = 0;

            //Ввод значений
            SqlCommand command;
            SqlDataReader sqlReader = null;

            if (features[listBox1.SelectedIndex][2] == "Скалярный")
            {
                command = new SqlCommand("SELECT value FROM ScalarValues WHERE Feature=@featureId", sqlConnection);
                command.Parameters.AddWithValue("featureId", features[listBox1.SelectedIndex][1]);
                
                try
                {
                    sqlReader = await command.ExecuteReaderAsync();
                    while(await sqlReader.ReadAsync())
                    {
                        comboBox1.Items.Add(Convert.ToString(sqlReader["value"]));
                    }

                    if (features[listBox1.SelectedIndex].Count > 3)
                        comboBox1.SelectedItem = features[listBox1.SelectedIndex][3];

                    comboBox1.Visible = true;
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
            }
            if (features[listBox1.SelectedIndex][2] == "Логический")
            {
                command = new SqlCommand("SELECT * FROM LogicalValues WHERE Feature=@featureId", sqlConnection);
                command.Parameters.AddWithValue("featureId", features[listBox1.SelectedIndex][1]);

                try
                {
                    sqlReader = await command.ExecuteReaderAsync();
                    if (await sqlReader.ReadAsync())
                    {
                        radioButton1.Text = Convert.ToString(sqlReader["TrueValue"]);
                        radioButton2.Text = Convert.ToString(sqlReader["FalseValue"]);
                    }

                    if (features[listBox1.SelectedIndex].Count > 3)
                        if (features[listBox1.SelectedIndex][3] == "TrueValue")
                            radioButton1.Checked = true;
                        else
                            radioButton2.Checked = true;

                    radioButton1.Visible = true;
                    radioButton2.Visible = true;
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
            if (features[listBox1.SelectedIndex][2] == "Размерный")
                {
                    command = new SqlCommand("SELECT * FROM DimensionValue WHERE Feature=@featureId", sqlConnection);
                    command.Parameters.AddWithValue("featureId", features[listBox1.SelectedIndex][1]);
                    try
                    {
                        sqlReader = await command.ExecuteReaderAsync();
                        if (await sqlReader.ReadAsync())
                        {
                            label5.Text = Convert.ToString(sqlReader["unit"]);
                            
                            //установка левого значения
                            if (Convert.ToInt64(sqlReader["leftValueIncluded"]) == 1)
                            {
                                numericUpDown1.Minimum = Convert.ToDecimal(sqlReader["leftValue"]);
                                label4.Text = "[";
                            }
                            else
                            {
                                numericUpDown1.Minimum = Convert.ToDecimal(sqlReader["leftValue"]) + Convert.ToDecimal(0.001);
                                label4.Text = "(";
                            }

                            label4.Text += Convert.ToString(sqlReader["leftValue"]) + ";" + Convert.ToString(sqlReader["rightValue"]);

                            //установка правого значения
                            if (Convert.ToInt64(sqlReader["rightValueIncluded"]) == 1)
                            {
                                numericUpDown1.Maximum = Convert.ToDecimal(sqlReader["rightValue"]);
                                label4.Text += "]";
                            }
                            else
                            {
                                numericUpDown1.Maximum = Convert.ToDecimal(sqlReader["rightValue"]) - Convert.ToDecimal(0.001);
                                label4.Text += ")";
                            }
                        }

                        if (features[listBox1.SelectedIndex].Count > 3)
                            numericUpDown1.Value = Convert.ToDecimal(features[listBox1.SelectedIndex][3]);

                        label5.Visible = true;
                        label2.Visible = true;
                        label4.Visible = true;
                        numericUpDown1.Visible = true;
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
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboBox1.Visible)
                return;

            if (features[listBox1.SelectedIndex].Count > 3)
                features[listBox1.SelectedIndex][3] = Convert.ToString(comboBox1.SelectedItem);
            else
                features[listBox1.SelectedIndex].Add(Convert.ToString(comboBox1.SelectedItem));
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton1.Visible)
                return;

            if (features[listBox1.SelectedIndex].Count > 3)
                features[listBox1.SelectedIndex][3] = "TrueValue";
            else
                features[listBox1.SelectedIndex].Add("TrueValue");
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton1.Visible)
                return;

            if (features[listBox1.SelectedIndex].Count > 3)
                features[listBox1.SelectedIndex][3] = "FalseValue";
            else
                features[listBox1.SelectedIndex].Add("FalseValue");
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (!numericUpDown1.Visible)
                return;

            if (features[listBox1.SelectedIndex].Count > 3)
                features[listBox1.SelectedIndex][3] = Convert.ToString(numericUpDown1.Value);
            else
                features[listBox1.SelectedIndex].Add(Convert.ToString(numericUpDown1.Value));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool allValuesEntered = true;
            string result = "";
            for(int i = 0; i < features.Count; i++)
            {
                if(features[i].Count == 3)
                {
                    allValuesEntered = false;
                    result += "У признака '" + features[i][0] + "' не введено значение!" + System.Environment.NewLine;
                }
            }

            if (!allValuesEntered)
                MessageBox.Show(result, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                Form ifrm = new Решатель(features);
                this.Hide(); // скрываем Form1
                ifrm.ShowDialog(this); // отображаем Form2
            }
        }
    }
}
