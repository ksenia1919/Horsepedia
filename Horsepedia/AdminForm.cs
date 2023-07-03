using DGVPrinterHelper;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Device.Location;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using static DGVPrinterHelper.DGVPrinter;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Image = System.Drawing.Image;

namespace Horsepedia
{
    public partial class AdminForm : Form
    {
        private GeoCoordinateWatcher watcher;
        private GMapOverlay markersOverlay = new GMapOverlay("markers");

        string pathImage;
        public AdminForm()
        {
            InitializeComponent();
            var dbquery = new DBquery();
            dbquery.DisplayData("Пользователи",dataGridView1);
            dbquery.DisplayData("Лошади", dataGridView2);
            dbquery.DisplayData("Предназначение", dataGridView3);
            dbquery.DisplayData("Посещения", dataGridView4);
            ShowCurrentLocationOnMap();
        }

        private void ShowCurrentLocationOnMap()
        {
            // Создать экземпляр GeoCoordinateWatcher для получения текущей геолокации
            watcher = new GeoCoordinateWatcher();
            if (watcher.TryStart(false, TimeSpan.FromSeconds(10)))
            {
                watcher.PositionChanged += Watcher_PositionChanged;       
            }
        }
        private void Watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            // Проверить, что служба геолокации активирована
            if (watcher.Status == GeoPositionStatus.Ready)
            {
                // Прогружаем карту
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
                gMapControl1.MinZoom = 2;
                gMapControl1.MaxZoom = 16;
                gMapControl1.Zoom = 4;
                gMapControl1.Position = new GMap.NET.PointLatLng(66.4169575018027, 94.25025752215694);
                gMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
                gMapControl1.CanDragMap = true;
                gMapControl1.DragButton = MouseButtons.Left;
                gMapControl1.ShowCenter = false;
                gMapControl1.ShowTileGridLines = false;

                // Получить текущее местоположение из события PositionChanged
                GeoCoordinate currentLocation = e.Position.Location;

                // Очистить слой с маркерами
                markersOverlay.Markers.Clear();

                if (!currentLocation.IsUnknown)
                {
                    // Поставить метку
                    GMarkerGoogle marker = new GMarkerGoogle(new PointLatLng(currentLocation.Latitude, currentLocation.Longitude), GMarkerGoogleType.green);
                    markersOverlay.Markers.Add(marker);
                    gMapControl1.Overlays.Clear();
                    gMapControl1.Overlays.Add(markersOverlay);
                    gMapControl1.Position = new PointLatLng(currentLocation.Latitude, currentLocation.Longitude);
                    gMapControl1.Zoom = 30;
                }
                else
                {
                    // Удалить все маркеры, если координаты не удалось получить
                    markersOverlay.Markers.Clear();

                    // Вывести сообщение об ошибке
                    MessageBox.Show("Не удалось определить местоположение.", "Ошибка");
                }

                // Обновить карту
                gMapControl1.Refresh();
            }
            else
            {
                MessageBox.Show("Служба геолокации не готова.");
            }
        }

        private void AdminForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var login = guna2TextBox1.Text;
            var password = guna2TextBox2.Text;
            if (login == string.Empty || password == string.Empty)
            {
                MessageBox.Show("Заполните все поля", "Ошибка");
                return;
            }
            PassValidation PassV = new PassValidation();
            if (PassV.Validation(guna2TextBox2.Text) == false)
            {
                return;
            }

            if (guna2TextBox2.Text == guna2TextBox1.Text)
            {
                MessageBox.Show("Пароль и логин не должны совпадать.", "Ошибка");
                return;
            }

            ValidationLogin LV = new ValidationLogin();
            if (LV.Validation(guna2TextBox1.Text) == false)
            {
                return;
            }
            else
            {
                Register(guna2TextBox1.Text, guna2TextBox2.Text);
            }
        }
        private void Register(string login, string password)
        {
            if (!AlreadyExistsUser(login, password))
            {
                Hashing GH = new Hashing();
                string query = $"Insert into Пользователи(роль, логин, пароль) values('admin', '{login}', '{GH.Hash(password)}')";

                var dbquery = new DBquery();
                dbquery.queryExecute(query);
                checkBox1.Checked = false;
                checkBox1.Checked = true;
            }
            else MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка");
        }
        private bool AlreadyExistsUser(string login, string password)
        {
            bool isExists = false;

            var dbQeury = new DBquery();

            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Пользователи WHERE логин ='{login}'", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["логин"].ToString() == login)
                            {
                                isExists = true;
                            }
                        }
                    }
                }
            }
            return isExists;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked) 
            {
                string query = $"Select * from Пользователи where роль = 'admin'";
                var sortAdmin = new DBquery();
                sortAdmin.queryReturnData(query, dataGridView1);
            }
            else 
            {
                var dbquery = new DBquery();
                dbquery.DisplayData("Пользователи", dataGridView1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var login = guna2TextBox1.Text;
            var password = guna2TextBox2.Text;
            if (login == string.Empty)
            {
                MessageBox.Show("Заполните поле \"Логин\"", "Ошибка");
                return;
            }
            DialogResult dialogResult = MessageBox.Show("Вы действительно хотите удалить данного администратора?", "Удаление записи", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (AdminisFind(login))
                {
                    string query = $"Delete from Пользователи where логин = '{login}'";
                    var dbquery = new DBquery();
                    dbquery.queryExecute(query);

                    dbquery.DisplayData("Пользователи", dataGridView1);
                    ///////////////////////////////////
                    checkBox1.Checked = false;
                    checkBox1.Checked = true;
                }
                else
                {
                    MessageBox.Show("Данный администратор не был обнаружен", "Ошибка");
                    return;
                }
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
            
        }
        public bool AdminisFind(string login)
        {
            bool isFind = false;

            var dbQeury = new DBquery();
            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Пользователи WHERE логин ='{login}' and роль = 'admin'", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isFind = true;
                        }
                    }
                }
            }
            return isFind;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var login = guna2TextBox1.Text;
            var password = guna2TextBox2.Text;
            if (login == string.Empty || password == string.Empty)
            {
                MessageBox.Show("Заполните все поля.", "Ошибка");
                return;
            }
            PassValidation PassV = new PassValidation();
            if (PassV.Validation(guna2TextBox2.Text) == false)
            {
                return;
            }
            if (guna2TextBox2.Text == guna2TextBox1.Text)
            {
                MessageBox.Show("Пароль и логин не должны совпадать.", "Ошибка");
                return;
            }
            if (AdminisFind(login))
            {
                var getHash = new Hashing();
                string query = $"Update Пользователи set пароль = '{getHash.Hash(password)}' where логин = '{login}'";
                var dbquery = new DBquery();
                dbquery.queryReturnData(query, dataGridView1);
                dbquery.DisplayData("Пользователи", dataGridView1);
                checkBox1.Checked = false;
                checkBox1.Checked = true;
            }
            else { MessageBox.Show("Данный логин администратора не был найден.", "Ошибка"); return;}
        }

        private void button8_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;

                try
                {
                    string text = File.ReadAllText(fileName);
                    textBox1.Text = text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Невозможно прочитать выбранный файл. Original error: " + ex.Message, "Ошибка чтения");
                }
            }
        }

        private void dataGridView2_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            try 
            {
                if (dataGridView2.SelectedRows.Count > 0)
                {
                    guna2TextBox3.Text = dataGridView2.SelectedRows[0].Cells[0].Value.ToString();
                    guna2TextBox4.Text = dataGridView2.SelectedRows[0].Cells[1].Value.ToString();
                    guna2TextBox5.Text = dataGridView2.SelectedRows[0].Cells[2].Value.ToString();
                    guna2TextBox6.Text = dataGridView2.SelectedRows[0].Cells[3].Value.ToString();
                    textBox1.Text = dataGridView2.SelectedRows[0].Cells[4].Value.ToString();
                    pictureBox1.Image = Image.FromFile(dataGridView2.SelectedRows[0].Cells[5].Value.ToString());

                    //записываем в Статистику
                    var dbQeury = new DBquery();

                    using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
                    {
                        con.Open();
                        using (SqlCommand command = new SqlCommand($"SELECT * FROM Предназначение WHERE id_породы = {Convert.ToInt32(guna2TextBox3.Text)}", con))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (reader["id_породы"].ToString() != "")
                                    {
                                        guna2TextBox7.Text = reader["Выездка"].ToString();
                                        guna2TextBox8.Text = reader["Конкур"].ToString();
                                        guna2TextBox9.Text = reader["Троеборье"].ToString();
                                        guna2TextBox10.Text = reader["Агроэкотуризм"].ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Опаньки"); }
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                    pathImage = openFileDialog.FileName;
                }
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var dgvPrinter = new DGVPrinter();
            dgvPrinter.CreateReport("Отчет по посещениям", dataGridView4);
        }

        private bool HorseIsFined(string nameHorse, string location, int cost, string image)
        {
            var dbQeury = new DBquery();

            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Лошади WHERE Наименование_лошади = '{nameHorse}' and Место_происхождения = '{location}' and Цена = {cost} and Изображение = '{image}'", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["id_породы"].ToString() != "")
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string name = guna2TextBox4.Text;
            string location = guna2TextBox5.Text;
            int cost = Convert.ToInt32(guna2TextBox6.Text);
            string depiction = textBox1.Text;
            string image = pathImage;// string image = Path.GetFileName(pathImage);

            if (name == string.Empty || location == string.Empty || guna2TextBox6.Text == string.Empty || depiction == string.Empty || guna2TextBox7.Text == string.Empty || guna2TextBox8.Text == string.Empty || guna2TextBox9.Text == string.Empty || guna2TextBox10.Text == string.Empty)
            {
                MessageBox.Show("Заполните все даные.", "Ошибка");
                return;
            }
            if(image == string.Empty) {
                MessageBox.Show("Выберите картинку повторно.", "Ошибка");
                return;
            }
            if(Convert.ToUInt32(guna2TextBox7.Text) >100 || Convert.ToUInt32(guna2TextBox8.Text) > 100 || Convert.ToUInt32(guna2TextBox9.Text) > 100 || Convert.ToUInt32(guna2TextBox10.Text) > 100)
            {
                MessageBox.Show("Данные заполнены не корректно.", "Ошибка");
                return;
            }

            if (HorseIsFined(name, location, cost, image))
            {
                try
                {
                    DialogResult dialogResult = MessageBox.Show("Такая порода уже существует, все еще хотите ее добавить?", "Добавление записи", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //добавляем породу
                        string query = $"Insert into Лошади(Наименование_лошади, Место_происхождения, Цена, Описание, Изображение) values('{name}', '{location}', {cost}, '{depiction}', '{image}')";
                        var dbquery = new DBquery();
                        dbquery.queryExecute(query);
                        dbquery.DisplayData("Лошади", dataGridView2);
                        //узнаем id последней записи (собственно, которую мы добавили)
                        int id_horse = Convert.ToInt32(dataGridView2.SelectedRows[dataGridView2.Rows.Count - 1].Cells[0].Value);
                        query = $"Insert into Предназначение values('{id_horse}', {Convert.ToInt32(guna2TextBox7.Text)}, {Convert.ToInt32(guna2TextBox8.Text)}, {Convert.ToInt32(guna2TextBox9.Text)}, {Convert.ToInt32(guna2TextBox10.Text)})";
                        dbquery.queryExecute(query);
                        dbquery.DisplayData("Предназначение", dataGridView3);
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                }
                catch (Exception ew) { MessageBox.Show("Произошла ощибка добавления породы","Опаньки"); }
            }
            else
            {
                //добавляем породу
                string query = $"Insert into Лошади(Наименование_лошади, Место_происхождения, Цена, Описание, Изображение) values('{name}', '{location}', {cost}, '{depiction}', '{image}')";
                var dbquery = new DBquery();
                dbquery.queryExecute(query);
                dbquery.DisplayData("Лошади", dataGridView2);
                //узнаем id последней записи (собственно, которую мы добавили)
                int id_horse = Convert.ToInt32(dataGridView2.Rows[dataGridView2.RowCount-1].Cells[0].Value);
                query = $"Insert into Предназначение values('{id_horse}', {Convert.ToInt32(guna2TextBox7.Text)}, {Convert.ToInt32(guna2TextBox8.Text)}, {Convert.ToInt32(guna2TextBox9.Text)}, {Convert.ToInt32(guna2TextBox10.Text)})";
                dbquery.queryExecute(query);
                dbquery.DisplayData("Предназначение", dataGridView3);
            }
        }

        private bool FindHorseinID(int id)
        {
            var dbQeury = new DBquery();

            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Лошади WHERE id_породы = {id}", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["id_породы"].ToString() != "")
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (guna2TextBox3.Text == string.Empty) {
                MessageBox.Show("Запись не определена.", "Ошибка");
                return;
            }
            //удаление
            int id = Convert.ToInt32(guna2TextBox3.Text);
            DialogResult dialogResult = MessageBox.Show($"Вы действительно хотите удалить данную запись? (id{id})", "Удаление записи", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (FindHorseinID(id))
                {
                    string query = $"Delete from Лошади where id_породы = '{id}'";
                    var dbquery = new DBquery();
                    dbquery.queryExecute(query);

                    dbquery.DisplayData("Лошади", dataGridView2);
                    dbquery.DisplayData("Предназначение", dataGridView3);
                }
                else
                {
                    MessageBox.Show("Данная порода не была обнаружена", "Ошибка");
                    return;
                }
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (guna2TextBox3.Text == string.Empty)
            {
                MessageBox.Show("Запись не определена.", "Ошибка");
                return;
            }
            if (guna2TextBox4.Text == string.Empty || guna2TextBox5.Text == string.Empty || guna2TextBox6.Text == string.Empty || textBox1.Text == string.Empty || guna2TextBox7.Text == string.Empty || guna2TextBox8.Text == string.Empty || guna2TextBox9.Text == string.Empty || guna2TextBox10.Text == string.Empty)
            {
                MessageBox.Show("Заполните все данные.", "Ошибка");
                return;
            }
            if (Convert.ToUInt32(guna2TextBox7.Text) > 100 || Convert.ToUInt32(guna2TextBox8.Text) > 100 || Convert.ToUInt32(guna2TextBox9.Text) > 100 || Convert.ToUInt32(guna2TextBox10.Text) > 100)
            {
                MessageBox.Show("Данные заполнены не корректно.", "Ошибка");
                return;
            }

            //изменение
            int id = Convert.ToInt32(guna2TextBox3.Text);
            if (FindHorseinID(id))
            {
                var dbquery = new DBquery();
                string query = $"Update Лошади set Наименование_лошади = '{guna2TextBox4.Text}'," +
                    $"Место_происхождения = '{guna2TextBox5.Text}'," +
                    $"Цена = {Convert.ToInt32(guna2TextBox6.Text)}," +
                    $"Описание = '{textBox1.Text}'," +
                    $"Изображение = '{pathImage}' " +
                    $"where id_породы = {id}";
                dbquery.queryExecute(query);

                query = $"Update Предназначение set Выездка = {guna2TextBox7.Text}," +
                    $"Конкур = {guna2TextBox8.Text}," +
                    $"Троеборье = {guna2TextBox9.Text}," +
                    $"Агроэкотуризм = {guna2TextBox10.Text} " +
                    $"where id_породы = {id}";
                dbquery.queryExecute(query);

                dbquery.DisplayData("Лошади", dataGridView2);
                dbquery.DisplayData("Предназначение", dataGridView3); 
            }
            else { MessageBox.Show("Данная породв не была найдена.", "Ошибка"); return; }
        }

        private void guna2TextBox11_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (guna2TextBox11.Text != "")
                {
                    //сортировка поиска
                    var dbquery = new DBquery();
                    dbquery.DisplayData($"Лошади WHERE Наименование_лошади like '%{guna2TextBox11.Text}%'", dataGridView2);
                }
                else
                {
                    var dbquery = new DBquery();
                    dbquery.DisplayData("Лошади", dataGridView2);
                }
            }
        }

        private void guna2TextBox12_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (guna2TextBox12.Text != "")
                {
                    //сортировка поиска
                    var dbquery = new DBquery();
                    dbquery.DisplayData($"Посещения WHERE Дата like '%{guna2TextBox12.Text}%'", dataGridView4);
                }
                else
                {
                    var dbquery = new DBquery();
                    dbquery.DisplayData("Посещения", dataGridView4);
                }
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            this.Owner.Show();
            this.Hide();
        }

        private void guna2TextBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void guna2TextBox7_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void guna2TextBox8_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void guna2TextBox9_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void guna2TextBox10_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
