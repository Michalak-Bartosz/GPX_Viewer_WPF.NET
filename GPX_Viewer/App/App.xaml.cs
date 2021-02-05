using System.Windows;

namespace GPX_Viewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        { 
            GPX_Viewer.MainWindow.sqlite_conn.Close();
        }
    }
}
