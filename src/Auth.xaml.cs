using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Auth
{
    public partial class MainWindow : Window
    {
        private readonly string con_str =
            "Server = Symon-PC\\SQLEXPRESS; " +
            "Database = Learning; " +
            "Integrated Security = yes";
        private string password = "";
        private int selLength = 0;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.txtLogin.GotFocus += new RoutedEventHandler(SetActive);
            this.txtPassw.GotFocus += new RoutedEventHandler(SetActive);
        }

        private void txtPassw_SelectionChanged(object sender, RoutedEventArgs e) {
            TextBox box = (TextBox) sender;

            this.selLength = box.SelectionLength;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e) {
            string login = this.txtLogin.Text;
            string loginBuf;
            string loginField = null; /* A field name, by which hash will be requested. */
            string passw = this.password;
            string passwHashEx;

            /* Check login. */
            if (string.IsNullOrEmpty(login) || login == "login") {
                MessageBox.Show("Login entered is empty.", "Auth");

                return;
            }

            foreach (string fname in ReadConfigFields()) {
                loginBuf = GetValueFromField(fname, fname + " " + login);
                if (loginBuf != null) {
                    loginField = fname;

                    break;
                }
            }

            if (loginField == null) {
                MessageBox.Show("No such user exists.", "Auth");

                return;
            }

            /* Check password. */
            if (string.IsNullOrEmpty(passw) || passw == "password") {
                MessageBox.Show("Password entered is empty.", "Auth");

                return;
            }

            passwHashEx = GetValueFromField("password_hash", loginField + " " + login);
            if (!Utils.CompHash(passw, passwHashEx)) {
                MessageBox.Show("Incorrect password.", "Auth");

                return;
            }

            MessageBox.Show("You are successfully logged in.", "Auth", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetActive(object sender, RoutedEventArgs args) {
            TextBox box = (TextBox) sender;

            if (box.Text == "password")
                /* To replace password text box character to password char, 
                 * that don't use marginal PasswordBox. */
                this.txtPassw.TextChanged += new TextChangedEventHandler(RedirectPassword);

            box.Foreground = Brushes.Black;
            box.Text = "";

            box.GotFocus -= new RoutedEventHandler(SetActive);
        }

        private void RedirectPassword(object sender, TextChangedEventArgs e) {
            TextBox box = (TextBox) sender;
            string buf = "";
            int selStart = box.SelectionStart;

            if (box.Text.All(c => c == '•')) {
                if (box.Text.Length < this.password.Length) {
                    this.password = this.password.Remove(selStart, this.selLength != 0 ? this.selLength : 1);

                    this.selLength = 0;
                }

                return;
            }

            if (box.Text.Length == 0)
                this.password = "";
            else {
                if (this.selLength > 0)
                    this.password = this.password.Remove(selStart - 1, this.selLength);

                this.password = this.password.Insert(selStart - 1, box.Text.Trim('•'));
            }

            for (int i = 0; i < box.Text.Length; i++)
                buf += '•';

            box.Text = buf;
            box.SelectionStart = selStart;
            box.SelectionLength = 0;
        }

        private string GetValueFromField(string fieldName, string predicate) {
            SqlConnection con = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            SqlDataReader r;
            string result;

            void Assert(Exception ex) {
                MessageBox.Show((ex + "").Split('\n')[0], "Auth");

                Environment.Exit(-1);
            }

            try {
                con.ConnectionString = this.con_str;
            } catch (ArgumentException ex) {
                Assert(ex);
            }

            con.Open();

            cmd.Connection = con;
            cmd.CommandText =
                "select " + fieldName +
                "\n   from Users" +
                "\n       where " + predicate.Split(' ')[0] + " = '" + predicate.Split(' ')[1] + "'";

            try {
                r = cmd.ExecuteReader();

                if (!r.Read())
                    return null;

                result = r[0] + "";

                r.Close();
                con.Close();

                return result;
            } catch (SqlException ex) {
                Assert(ex);
            }

            return null;
        }

        private IEnumerable<string> ReadConfigFields() {
            string[] lines;
            string[] buf;

            void Assert(string msg) {
                MessageBox.Show(msg);

                Environment.Exit(-1);
            }

            if (!File.Exists("config"))
                Assert("Couldn't find config file.");

            lines = File.ReadAllLines("config");
            if (lines.Length == 0)
                Assert("Config is empty.");

            buf = lines[0].Split('=');

            if (buf.Length < 2)
                Assert("Config is corrupted.");

            else {
                foreach (string item in buf[1].Split(',')) {
                    if (!string.IsNullOrWhiteSpace(item))
                        yield return item.Trim(' ');
                }
            }
        }
    }
}
