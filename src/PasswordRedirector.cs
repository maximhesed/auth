using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ais.src
{
    class PasswordRedirector
    {
        string PasswProperty = "";
        string PasswRepeatProperty = "";
        int txtPasswSelLen = 0;
        int txtPasswSelRepeatLen = 0;
        readonly TextBox txtPassw;
        readonly TextBox txtPasswRepeat;
        readonly string txtPasswName = "txtPassw";
        readonly string txtPasswRepeatName = "txtPasswRepeat";

        internal string Passw {
            get => this.PasswProperty;
            private set => this.PasswProperty = value;
        }

        internal string PasswRepeat {
            get => this.PasswRepeatProperty;
            private set => this.PasswRepeatProperty = value;
        }

        public PasswordRedirector(TextBox txtPassw) {
            this.txtPassw = txtPassw;
            this.txtPasswRepeat = null;
        }

        public PasswordRedirector(TextBox txtPassw, TextBox txtPasswRepeat) {
            this.txtPassw = txtPassw;
            this.txtPasswRepeat = txtPasswRepeat;
        }

        internal void Bind(TextBox box) {
            if (box.Name == this.txtPasswName) {
                this.txtPassw.SelectionChanged += OnSelectionChanged;
                this.txtPassw.TextChanged += OnTextChanged;
                this.txtPassw.KeyUp += OnKeyUp;
                this.Passw = "";
            }
            else if (box.Name == this.txtPasswRepeatName) {
                if (this.txtPasswRepeat != null) {
                    this.txtPasswRepeat.SelectionChanged += OnSelectionChanged;
                    this.txtPasswRepeat.TextChanged += OnTextChanged;
                    this.txtPasswRepeat.KeyUp += OnKeyUp;
                    this.PasswRepeat = "";
                }
            }
        }

        internal void Unbind(TextBox box) {
            if (box.Name == this.txtPasswName) {
                this.txtPassw.SelectionChanged -= OnSelectionChanged;
                this.txtPassw.TextChanged -= OnTextChanged;
                this.txtPassw.KeyUp -= OnKeyUp;
                this.Passw = "";
            }
            else if (box.Name == this.txtPasswRepeatName) {
                if (this.txtPasswRepeat != null) {
                    this.txtPasswRepeat.SelectionChanged -= OnSelectionChanged;
                    this.txtPasswRepeat.TextChanged -= OnTextChanged;
                    this.txtPasswRepeat.KeyUp -= OnKeyUp;
                    this.PasswRepeat = "";
                }
            }
        }

        internal void Override(TextBox box, string passw) {
            if (box.Name == this.txtPasswName)
                this.Passw = passw;
            else if (box.Name == this.txtPasswRepeatName) {
                if (this.txtPasswRepeat != null)
                    this.PasswRepeat = passw;
            }

            box.Text = passw;
        }

        void OnSelectionChanged(object sender, RoutedEventArgs e) {
            TextBox box = (TextBox) sender;

            if (box.Name == this.txtPasswName)
                this.txtPasswSelLen = box.SelectionLength;
            else if (box.Name == this.txtPasswRepeatName)
                this.txtPasswSelRepeatLen = box.SelectionLength;
        }

        void OnTextChanged(object sender, TextChangedEventArgs e) {
            TextBox box = (TextBox) sender;

            void RedirectTo(ref string str, ref int selLen) {
                int selStart = box.SelectionStart;
                string buf = "";

                if (box.Text.All(c => c == '•')) {
                    if (box.Text.Length < str.Length) {
                        str = str.Remove(selStart, selLen != 0
                            ? selLen : 1);

                        selLen = 0;
                    }

                    return;
                }

                if (box.Text.Length == 0)
                    str = "";
                else {
                    if (selStart > 0) {
                        if (selLen > 0)
                            str = str.Remove(selStart - 1, selLen);

                        str = str.Insert(selStart - 1, box.Text.Trim('•'));
                    }
                }

                for (int i = 0; i < box.Text.Length; i++)
                    buf += '•';

                box.Text = buf;
                box.SelectionStart = selStart;
                box.SelectionLength = 0;
            }

            if (box.Name == this.txtPasswName)
                RedirectTo(ref this.PasswProperty, ref this.txtPasswSelLen);
            else if (box.Name == this.txtPasswRepeatName)
                RedirectTo(ref this.PasswRepeatProperty, ref this.txtPasswSelRepeatLen);
        }

        void OnKeyUp(object sender, KeyEventArgs e) {
            TextBox box = (TextBox) sender;

            if (e.Key == Key.Back && (Keyboard.IsKeyDown(Key.LeftCtrl)
                    || Keyboard.IsKeyDown(Key.RightCtrl))) {
                if (box.Name == this.txtPasswName)
                    this.Passw = "";
                else if (box.Name == this.txtPasswRepeatName)
                    this.PasswRepeat = "";
            }
        }
    }
}
