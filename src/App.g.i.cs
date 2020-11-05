namespace Auth.src
{
    public partial class App : System.Windows.Application
    {
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main() {
            App app = new App();

            app.InitializeComponent();
            app.Run();
        }
    }
}
