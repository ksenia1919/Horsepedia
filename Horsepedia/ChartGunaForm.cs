using Guna.UI2.WinForms;
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

namespace Horsepedia
{
    public partial class ChartGunaForm : Form
    {
        int p1, p2, p3, p4;
        public ChartGunaForm()
        {
            InitializeComponent();
        }
        public int getSlider{ get; set; }
        public void LoadFromHorses()
        {
            //выгрузка данных статистики
            var dbQeury = new DBquery();

            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Предназначение WHERE id_породы = {this.getSlider}", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            p1 = (int)reader["Выездка"];
                            p2 = (int)reader["Конкур"];
                            p3 = (int)reader["Троеборье"];
                            p4 = (int)reader["Агроэкотуризм"];
                        }
                    }
                }
            }
        }

        private DataTable dataSet()
        {
            // Создаем DataTable
            DataTable dataTable = new DataTable("MyTable");

            // Добавляем столбцы
            dataTable.Columns.Add("Характеристики", typeof(string));
            dataTable.Columns.Add("Коэффициент", typeof(double));

            // Добавляем строки
            LoadFromHorses();
            dataTable.Rows.Add("Выездка", p1);
            dataTable.Rows.Add("Конкур", p2);
            dataTable.Rows.Add("Троеборье", p3);
            dataTable.Rows.Add("Агроэкотуризм", p4);

            return dataTable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var creatorChart = new CreatorChart();
            creatorChart.ChartPie(gunaChart1, dataSet(), "Статистика породы (ChartPie)");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var creatorChart = new CreatorChart();
            creatorChart.ChartPolar(gunaChart1, dataSet(), "Статистика породы (ChartPolar)");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var creatorChart = new CreatorChart();
            creatorChart.ChartBar(gunaChart1, dataSet(), "Статистика породы (ChartBar)");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var creatorChart = new CreatorChart();
            creatorChart.ChartHorizontalBar(gunaChart1, dataSet(), "Статистика породы (ChartHorizontalBar)");
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.Owner.Show();
            this.Hide();
        }

        private void ChartGunaForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Owner.Show();
        }
    }
}
