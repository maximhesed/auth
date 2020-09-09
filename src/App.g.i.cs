namespace Auth.src
{
    partial class App : System.Windows.Application
    {
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main() {
            Auth.App app = new Auth.App();

            app.InitializeComponent();
            app.Run();
        }
    }
}
