using System.Windows;
using System.Windows.Media;
using Finisar.SQLite;

namespace GPX_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static SQLiteConnection sqlite_conn;
        public static SQLiteCommand sqlite_cmd;
        public static SQLiteDataReader sqlite_datareader;

        public MainWindow()
        {
            InitializeComponent();
            frame.NavigationService.Navigate(new Menu());
            mainWindow.BorderBrush = Brushes.DarkBlue;

            if (!System.IO.File.Exists(@"../../DB/DB_GPX.db"))
            {
                sqlite_conn = new SQLiteConnection("Data Source=../../DB/DB_GPX.db;Version=3;New=True;Compress=True;");
                // open the connection:
                sqlite_conn.Open();

                // create a new SQL command:
                sqlite_cmd = sqlite_conn.CreateCommand();

                // Let the SQLiteCommand object know our SQL-Query:
                sqlite_cmd.CommandText = "CREATE TABLE GPX_File (id integer primary key, Name varchar(100), Path varchar(100), Description varchar(100));";

                // Now lets execute the SQL ;D
                sqlite_cmd.ExecuteNonQuery();
            }
            else
            {
                // open the connection:
                sqlite_conn = new SQLiteConnection("Data Source=../../DB/DB_GPX.db;Version=3;New=False;Compress=True;");
                sqlite_conn.Open();
            }
        }

    }
}
