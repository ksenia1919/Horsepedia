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
    public partial class Login : Form
    {
        int id_pers;
        public Login()
        {
            InitializeComponent();

            checkBox1.Checked = true;
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            //this.Owner.Show();
            Application.Exit();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                guna2TextBox2.UseSystemPasswordChar = true;
            else
                guna2TextBox2.UseSystemPasswordChar = false;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var login = guna2TextBox1.Text;
            var password = guna2TextBox2.Text;
            if (login == string.Empty || password == string.Empty)
            {
                MessageBox.Show("Заполните все поля", "Ошибка");
                return;
            }

            if (AuthorizeUser(login, password))
            {
                if (UserAdmin(login))
                {
                    //записываем посещение
                    string date = DateTime.Now.ToString("dd.MMM, HH:mm");
                    string query = $"Insert into Посещения(Пользователь, Дата) values({id_pers}, '{date}')";
                    var dbquery = new DBquery();
                    dbquery.queryExecute(query);

                    AdminForm adminForm = new AdminForm();
                    adminForm.Owner = this.Owner;
                    adminForm.Show();
                    this.Hide();
                }
                else
                {
                    //записываем посещение
                    string date = DateTime.Now.ToString("dd.MMM, HH:mm");
                    string query = $"Insert into Посещения(Пользователь, Дата) values({id_pers}, '{date}')";
                    var dbquery = new DBquery();
                    dbquery.queryExecute(query);

                    MainForm mainForm = new MainForm();
                    mainForm.Owner = this.Owner;
                    mainForm.getLoginperson = login;
                    mainForm.Show();
                    this.Hide();
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка");
            }
        }
        private bool AuthorizeUser(string login, string password)
        {
            bool isAuthorized = false;

            var dbQeury = new DBquery();
            var getHash = new Hashing();

            using (SqlConnection con = new SqlConnection(dbQeury.StringCon()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand($"SELECT * FROM Пользователи WHERE логин ='{login}' and пароль = '{getHash.Hash(password)}'", con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["пароль"].ToString() == getHash.Hash(password) && reader["логин"].ToString() == login)
                            {
                                id_pers = Convert.ToInt32(reader["Код_пользователя"]);
                                isAuthorized = true;
                                MessageBox.Show("Вход успешно выполнен!", "Успех");
                            }
                        }
                    }
                }
            }
            return isAuthorized;
        }

        private bool UserAdmin(string login)
        {
            bool isAdmin = false;

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
                            if (reader["роль"].ToString() == "admin")
                            {
                                isAdmin = true;
                            }
                        }
                    }
                }
            }
            return isAdmin;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.Owner.Show();
            this.Hide();
        }
    }
}
