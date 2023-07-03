using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Image = System.Drawing.Image;

namespace Horsepedia
{
    public partial class MainForm : Form
    {
        int old_label_width;
        
        public List<string> id_horses = new List<string>();
        int slider = 0;

        bool chekChosen = false;
        bool withoutAdd = false;
        bool checkSearch = false;
        public string getLoginperson { get; set; }
        public MainForm()
        {
            InitializeComponent();

            old_label_width = label4.Width;
            guna2ImageCheckBox1.Checked = false;
            guna2Button2.Enabled = false;

            //LoadIdForListHorses();
            //LoadFromHorses();
        }
        public void LoadFromHorses()
        {
            //будет выгружаться данные из таблицы Лошади(основная)
            var dbQeury = new DBquery();
            try { 
            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Лошади WHERE id_породы = {id_horses[slider]}", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (isChecked())
                            {
                                withoutAdd = true;
                                guna2ImageCheckBox2.Checked = true;
                            }
                            else
                            {
                                withoutAdd = false;
                                guna2ImageCheckBox2.Checked = false;
                            }
                            label2.Text = reader["Наименование_лошади"].ToString();
                            label3.Text = reader["Место_происхождения"].ToString();
                            label4.Text = reader["Цена"].ToString() + ",00$";
                            guna2TextBox2.Text = reader["Описание"].ToString();
                            guna2PictureBox1.Image = Image.FromFile($"{reader["Изображение"]}");
                        }
                    }
                }
            }
            }
            catch{ MessageBox.Show("Ошибка"); }
        }
        public void LoadFromChosen()
        {
            //выгрузка Избранных
            var dbQeury = new DBquery();

            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                try { 
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Избранные JOIN Лошади on Избранные.id_породы = Лошади.id_породы WHERE логин_пользователя = '{this.getLoginperson}' AND Лошади.id_породы = {id_horses[slider]}", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["Наименование_лошади"].ToString() != "")
                            {
                                label2.Text = reader["Наименование_лошади"].ToString();
                                label3.Text = reader["Место_происхождения"].ToString();
                                label4.Text = reader["Цена"].ToString() + ",00$";
                                guna2TextBox2.Text = reader["Описание"].ToString();
                                guna2PictureBox1.Image = Image.FromFile($"{reader["Изображение"]}");
                                guna2ImageCheckBox2.Checked = true;

                                if (id_horses.Count == 1)
                                {
                                    guna2Button1.Enabled = false;
                                    guna2Button2.Enabled = false;
                                }
                            }
                        }
                    }
                }
                }
                catch { MessageBox.Show("Ошибка222"); }
            }
        }

        public void LoadIdForListHorses()
        {
            //очистить лист
            id_horses.Clear();
            var dbQeury = new DBquery();
            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT id_породы FROM Лошади", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // add the id_породы value to the list
                            id_horses.Add(reader.GetInt32(0).ToString());
                        }
                    }
                }
            }
        }
        public void LoadIdForListSelecked()
        {
            id_horses.Clear();
            var dbQeury = new DBquery();
            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT id_породы FROM Избранные WHERE логин_пользователя = '{this.getLoginperson}'", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                       while (reader.Read())
                       {
                            id_horses.Add(reader.GetInt32(0).ToString());
                       } 
                    }
                }
            }
        }

        public bool isChecked()
        {
            var dbQeury = new DBquery();

            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Избранные WHERE id_породы = {id_horses[slider]} and логин_пользователя = '{this.getLoginperson}'", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["id_породы"].ToString() != "")
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //this.Owner.Close();
            Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.Owner.Show();
            this.Hide();
        }

        private void guna2ImageCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ImageCheckBox1.Checked)
            {
                guna2Button2.Enabled = false;
                chekChosen = true;
                withoutAdd = true;
                slider = 0;
                LoadIdForListSelecked();
                //проверка на наличие данных в таблице Избранные
                if (id_horses.Count == 0)
                {
                    MessageBox.Show("У вас нет сохраненных пород");
                    guna2ImageCheckBox1.Checked = false;
                }
                else
                {
                    LoadFromChosen();
                    //+отключение работы поиска
                    guna2TextBox1.Enabled = false;
                }
            }
            else
            {
                guna2TextBox1.Enabled = true;
                guna2Button2.Enabled = false;
                guna2Button1.Enabled = true;
                chekChosen = false;
                slider = 0;
                LoadIdForListHorses();
                LoadFromHorses();
            }
        }

        private void label4_SizeChanged(object sender, EventArgs e)
        {
            label4.Left -= label4.Width - old_label_width; //привязка текста к правому краю
            old_label_width = label4.Width;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            guna2Button2.Enabled = true;
            //листалка в право
            slider++;
            if (slider == id_horses.Count-1)
            {
                guna2Button1.Enabled = false;
            }
            if(chekChosen) LoadFromChosen();
            else if (checkSearch) dataSearch();
            else LoadFromHorses();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            guna2Button1.Enabled = true;
            //листалка в лево
            slider--;
            if (slider == 0)
            {
                guna2Button2.Enabled = false;
            }
            if (chekChosen) LoadFromChosen();
            else if (checkSearch) dataSearch();
            else LoadFromHorses();
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            //новая форма с диаграмками
            ChartGunaForm gunaForm = new ChartGunaForm();
            gunaForm.Owner = this;
            gunaForm.getSlider = Convert.ToInt32(id_horses[slider]);
            gunaForm.Show();
            this.Hide();
        }

        private void guna2ImageCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ImageCheckBox2.Checked && withoutAdd == false)
            {
                //добавляем породу в избранное
                string query = $"Insert into Избранные(логин_пользователя, id_породы) values('{getLoginperson}', '{id_horses[slider]}')";

                var dbquery = new DBquery();
                dbquery.queryExecute(query);
            }
            else if(!guna2ImageCheckBox2.Checked)
            {
                //убираем породу из избранное
                string query = $"Delete from Избранные where логин_пользователя = '{getLoginperson}' and id_породы = '{id_horses[slider]}'";

                var dbquery = new DBquery();
                dbquery.queryExecute(query);
                if (chekChosen)
                {
                    if (id_horses.Count == 1)//если не осталось пород, тогда выгружаем все
                    {
                        guna2ImageCheckBox1.Checked = false;
                    }
                    else 
                    {
                        guna2Button2.Enabled = false;
                        guna2Button1.Enabled = true;

                        slider = 0;
                        LoadIdForListSelecked();
                        LoadFromChosen();
                    }
                }
            }
            if (chekChosen) withoutAdd = true;
            else withoutAdd = false;
        }

        private void guna2TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (guna2TextBox1.Text != "")
                {
                    try
                    {
                        //поиск данных
                        checkSearch = true;
                        slider = 0;
                        LoadIdForListData();
                        dataSearch();

                        guna2Button2.Enabled = false;
                        if (id_horses.Count == 1) { guna2Button1.Enabled = false; }
                        else guna2Button1.Enabled = true;
                    }
                    catch
                    {
                        MessageBox.Show("По вашему запросу ничего не найдено");                
                    }
                }
                else
                {
                    checkSearch = false;
                    slider = 0;
                    LoadIdForListHorses();
                    LoadFromHorses();

                    guna2Button2.Enabled = false;
                    if (id_horses.Count == 1) { guna2Button1.Enabled = false; }
                    else guna2Button1.Enabled = true;
                }
            }
        }

        public void dataSearch()
        {
            //Поиск по запросу
            var dbQeury = new DBquery();
            try { 
            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Лошади WHERE Наименование_лошади like '%{guna2TextBox1.Text}%' and id_породы = '{id_horses[slider]}'", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (isChecked())
                            {
                                withoutAdd = true;
                                guna2ImageCheckBox2.Checked = true;
                            }
                            else
                            {
                                withoutAdd = false;
                                guna2ImageCheckBox2.Checked = false;
                            }
                            label2.Text = reader["Наименование_лошади"].ToString();
                            label3.Text = reader["Место_происхождения"].ToString();
                            label4.Text = reader["Цена"].ToString() + ",00$";
                            guna2TextBox2.Text = reader["Описание"].ToString();
                            guna2PictureBox1.Image = Image.FromFile($"{reader["Изображение"]}");
                        }
                    }
                }
            }
            }
            catch { MessageBox.Show("По вашему запросу ничего не найдено"); }
        }

        public void LoadIdForListData()
        {
            id_horses.Clear();
            var dbQeury = new DBquery();
            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Лошади WHERE Наименование_лошади like '%{guna2TextBox1.Text}%'", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_horses.Add(reader.GetInt32(0).ToString());
                        }
                    }
                }
            }
        }
    }
}
