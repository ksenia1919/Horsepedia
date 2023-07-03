using Guna.Charts.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Horsepedia
{
    public partial class Registration : Form
    {
        public Registration()
        {
            InitializeComponent();
            guna2TextBox1.Text = "";
            guna2TextBox2.Text = "";
            guna2TextBox3.Text = "";
            checkBox1.Checked = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                guna2TextBox2.UseSystemPasswordChar = true;
                guna2TextBox3.UseSystemPasswordChar = true;
            }
            else
            {
                guna2TextBox2.UseSystemPasswordChar = false;
                guna2TextBox3.UseSystemPasswordChar = false;
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (guna2TextBox2.Text != guna2TextBox3.Text)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка");
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
            if (!AlreadyExistsUser(login, password)) { 
                Hashing GH = new Hashing();
                string query = $"Insert into Пользователи(роль, логин, пароль) values('users', '{login}', '{GH.Hash(password)}')";

                var dbquery = new DBquery();
                dbquery.queryExecute(query);

                guna2TextBox1.Text = "";
                guna2TextBox2.Text = "";
                guna2TextBox3.Text = "";
                Login LF = new Login();
                LF.Owner = this;
                LF.Show();
                this.Hide();
            }
            else MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка");
        }

        private bool AlreadyExistsUser(string login, string password)
        {
            bool isExists = false;

            var dbQeury = new DBquery();
            var getHash = new Hashing();

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

        private void label1_Click(object sender, EventArgs e)
        {
            guna2TextBox1.Text = "";
            guna2TextBox2.Text = "";
            guna2TextBox3.Text = "";

            Login LF = new Login();
            LF.Owner = this;
            LF.Show();
            this.Hide();
        }
    }
}
