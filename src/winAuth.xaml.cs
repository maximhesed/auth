using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ais.src;
using Auth.src.model;

namespace Auth.src
{
    public partial class winAuth : Window
    {
        private readonly DesignEntities ctx = new DesignEntities();
        private readonly PasswordRedirector redirector;
        readonly string color = "#ffa3a3a3";
        bool remember = false;

        public winAuth() {
            InitializeComponent();

            this.redirector = new PasswordRedirector(this.txtPassw);
        }

        void Window_Loaded(object sender, RoutedEventArgs e) {
            FileStream fs;
            byte[] id = new byte[sizeof(int)];
            string id_str = "";
            int id_res;

            this.txtLogin.GotFocus += SetActive;
            this.txtPassw.GotFocus += SetActive;

            try {
                fs = new FileStream("data", FileMode.Open);
            } catch (FileNotFoundException) {
                Console.WriteLine("Couldn't find a data file.");

                return;
            }

            if (fs.Read(id, 0, sizeof(int)) == 0) {
                Console.WriteLine("The file is empty.");

                return;
            }

            for (int i = 0; i < sizeof(int); i++) {
                if (id[i] == 0xa)
                    break;

                id_str += id[i] - '0';
            }

            id_res = int.Parse(id_str);

            /* A data file has been found. Use it to pass an user's data into the fields. */
            this.txtLogin.Focus();
            this.txtLogin.Text = this.ctx.Users.First(u => u.id == id_res).login;
            this.txtPassw.Focus();
            this.redirector.Override(this.txtPassw, "12345");

            this.remember = true;

            /* Track the fields' changes. */
            this.txtLogin.TextChanged += TrackChanges;
            this.txtPassw.TextChanged += TrackChanges;

            this.txtPassw.SelectionStart = this.txtPassw.Text.Length;

            fs.Close();
        }

        void btnLogin_Click(object sender, RoutedEventArgs e) {
            Users user;

            /* A login check. */
            if (string.IsNullOrEmpty(this.txtLogin.Text) || this.txtLogin.Text == "login") {
                MessageBox.Show("Login entered is empty.", "Auth");

                return;
            }

            user = this.ctx.Users.FirstOrDefault(u => new List<string> { u.login, u.email }
                .Contains(this.txtLogin.Text));
            if (user == null) {
                MessageBox.Show("No such user exists.", "Auth");

                return;
            }

            /* A password check. */
            if (string.IsNullOrEmpty(this.txtPassw.Text) || this.txtPassw.Text == "password") {
                MessageBox.Show("Password entered is empty.", "Auth");

                return;
            }

            if (!this.remember) {
                if (!Utils.CompHash(this.redirector.Passw, user.password_hash)) {
                    MessageBox.Show("Incorrect password.", "Auth");

                    return;
                }
            }

            if (this.chkRemember.IsChecked == true) {
                FileStream fs = new FileStream("data", FileMode.Create);

                /* Store a user's id, to get its data after the next launch of the app. */
                fs.Write(Encoding.ASCII.GetBytes($"{user.id}\n"), 0, $"{user.id}\n".Length);

                fs.Close();
            }

            MessageBox.Show("You are successfully logged in.", "Auth", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        void TrackChanges(object sender, RoutedEventArgs e) {
            this.remember = false;

            (sender as TextBox).TextChanged -= TrackChanges;
        }

        void SetActive(object sender, RoutedEventArgs e) {
            TextBox box = (TextBox) sender;

            if (box.Foreground != Brushes.Black) {
                this.redirector.Unbind(box);

                box.Foreground = Brushes.Black;
                box.Text = "";

                this.redirector.Bind(box);
            }

            box.GotFocus -= SetActive;
            box.LostFocus += SetInactive;
        }

        void SetInactive(object sender, RoutedEventArgs e) {
            TextBox box = (TextBox) sender;

            if (string.IsNullOrEmpty(box.Text)) {
                box.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString(this.color));

                switch (box.Name) {
                    case "txtLogin":
                        box.Text = "login";

                        break;

                    case "txtPassw":
                        this.redirector.Unbind(this.txtPassw);

                        box.Text = "password";

                        break;
                }
            }

            box.LostFocus -= SetInactive;
            box.GotFocus += SetActive;
        }
    }
}
