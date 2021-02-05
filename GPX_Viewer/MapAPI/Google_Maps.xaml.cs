using Finisar.SQLite;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Gpx;
using GPX_Viewer.GPX_Parse;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GPX_Viewer.MapAPI
{
    /// <summary>
    /// Interaction logic for Google_Maps.xaml
    /// </summary>
    public partial class Google_Maps : Page
    {
        List<GMapRoute> polylineList = new List<GMapRoute>();
        List<MapPolyline> waypointList = new List<MapPolyline>();
        List<GpxFile> GpxFileList = new List<GpxFile>();
        private string fileName = "";
        private string fileNamePath = "";

        Random random = new Random();

        public Google_Maps()
        {
            InitializeComponent();
            textBoxOpis.IsEnabled = false;
            readFromDataBase();
        }

        private void changeMapModeButton(object sender, RoutedEventArgs e)
        {
            switch (mapControl.MapProvider.GetType().ToString())
            {
                case "GMap.NET.MapProviders.GoogleMapProvider":
                    mapControl.MapProvider = GoogleSatelliteMapProvider.Instance;
                    break;
                case "GMap.NET.MapProviders.GoogleSatelliteMapProvider":
                    mapControl.MapProvider = GoogleMapProvider.Instance;
                    break;
                default:
                    mapControl.MapProvider = GoogleMapProvider.Instance;
                    break;
            }
        }

        private void browseButton(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".gpx";
            dlg.Filter = "Text documents (.gpx)|*.gpx";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                fileNamePath = dlg.FileName;
                fileName = System.IO.Path.GetFileName(fileNamePath);
                fileNameTextBox.Text = fileName;
            }
        }

        private void addTraceToListViewButton(object sender, RoutedEventArgs e)
        {
            if (fileNameTextBox.Text != "")
            {
                bool OK_Path = true;
                foreach (GpxFile elementFileList in GpxFileList)
                {
                    if (elementFileList.Path == fileNamePath)
                    {
                        MessageBox.Show("Plik GPX o takiej ścieżce dostępu już jest wczytany do programu!\nWybierz inny plik.");
                        OK_Path = false;
                        break;
                    }
                }
                if (OK_Path)
                {
                    GpxFileList.Add(new GpxFile() { Number = (GpxFileList.Count() + 1).ToString(), Name = fileName, Check = false, Path = fileNamePath, InMap = false, NumberPolyline = -1, Track = false, Waypoint = false, Route = false, color = new SolidColorBrush(Colors.Black) });
                    bool OK = GpxFile.ReadGPXFile(fileNamePath, GpxFileList[GpxFileList.Count() - 1]);
                    if (OK)
                    {
                        traceListView.ItemsSource = GpxFileList.Select(s => s).ToList();
                    }

                    newGpxFile(fileName, fileNamePath);
                    saveInDataBase((GpxFileList.Count()), fileName, fileNamePath, "[Opisz swoje doznania podczas trasy...]");
                }

                fileName = "";
                fileNamePath = "";
                fileNameTextBox.Text = "";
            }
            else
            {
                MessageBox.Show("Brak plików GPX!\nWybierz plik i wczytaj do progoramu.");
            }
        }

        private void newGpxFile(string Name, string Path)
        {
            FileStream readFile = File.Open(Path, FileMode.Open);
            FileStream writeFile = File.OpenWrite("../../Files/" + Name);

            GpxReader reader = new GpxReader(readFile);
            GpxWriter writer = new GpxWriter(writeFile);

            while (reader.Read())
            {
                switch (reader.ObjectType)
                {
                    case GpxObjectType.Metadata:
                        writer.WriteMetadata(reader.Metadata);
                        break;
                    case GpxObjectType.WayPoint:
                        writer.WriteWayPoint(reader.WayPoint);
                        break;
                    case GpxObjectType.Route:
                        writer.WriteRoute(reader.Route);
                        break;
                    case GpxObjectType.Track:
                        writer.WriteTrack(reader.Track);
                        break;
                }
            }
            readFile.Close();
            writeFile.Close();
        }

        private void saveInDataBase(int Id, string Name, string Path, string Description)
        {

            MainWindow.sqlite_cmd = MainWindow.sqlite_conn.CreateCommand();
            MainWindow.sqlite_cmd.CommandText = "INSERT INTO GPX_File (id, Name, Path, Description) VALUES (@Id, @Name, @Path, @Description);";
            MainWindow.sqlite_cmd.CommandType = CommandType.Text;

            SQLiteParameter id = MainWindow.sqlite_cmd.Parameters.Add("@Id", SqlDbType.Int);
            id.Value = Id;

            SQLiteParameter name = MainWindow.sqlite_cmd.Parameters.Add("@Name", SqlDbType.VarChar);
            name.Value = Name;

            SQLiteParameter path = MainWindow.sqlite_cmd.Parameters.Add("@Path", SqlDbType.VarChar);
            path.Value = Path;

            SQLiteParameter description = MainWindow.sqlite_cmd.Parameters.Add("@Description", SqlDbType.VarChar);
            description.Value = Description;

            MainWindow.sqlite_cmd.ExecuteNonQuery();

        }

        private void readFromDataBase()
        {

            MainWindow.sqlite_cmd = MainWindow.sqlite_conn.CreateCommand();
            MainWindow.sqlite_cmd.CommandText = "SELECT * FROM GPX_File";
            MainWindow.sqlite_datareader = MainWindow.sqlite_cmd.ExecuteReader();

            List<string> nameList = new List<string>();
            List<string> pathList = new List<string>();

            while (MainWindow.sqlite_datareader.Read())
            {
                nameList.Add((string)MainWindow.sqlite_datareader["Name"]);
                pathList.Add((string)MainWindow.sqlite_datareader["Path"]);
            }

            int id = 1;
            bool ERROR = false;
            foreach (string name in nameList)
            {
                GpxFileList.Add(new GpxFile() { Number = (GpxFileList.Count() + 1).ToString(), Name = name, Check = false, Path = pathList[id - 1], InMap = false, NumberPolyline = -1, Track = false, Waypoint = false, Route = false, color = new SolidColorBrush(Colors.Black) });
                bool OK = GpxFile.ReadGPXFile(pathList[id - 1], GpxFileList[GpxFileList.Count() - 1]);
                if (OK)
                {
                    traceListView.ItemsSource = GpxFileList.Select(s => s).ToList();
                }
                else
                {
                    GpxFileList.RemoveAt(GpxFileList.Count() - 1);
                    ERROR = true;
                }
                id++;
            }

            if (ERROR)
            {
                updatePrimaryKey();
            }
        }

        private void updatePrimaryKey()
        {
            MainWindow.sqlite_cmd = MainWindow.sqlite_conn.CreateCommand();
            MainWindow.sqlite_cmd.CommandText = "SELECT * FROM GPX_File";
            MainWindow.sqlite_datareader = MainWindow.sqlite_cmd.ExecuteReader();

            List<string> pathList = new List<string>();

            while (MainWindow.sqlite_datareader.Read())
            {
                pathList.Add((string)MainWindow.sqlite_datareader["Path"]);
            }

            int id = 1;
            foreach (string path in pathList)
            {
                MainWindow.sqlite_cmd = MainWindow.sqlite_conn.CreateCommand();
                SQLiteParameter id_parm = MainWindow.sqlite_cmd.Parameters.Add("@Id", SqlDbType.Int);
                id_parm.Value = id;

                SQLiteParameter path_parm = MainWindow.sqlite_cmd.Parameters.Add("@Path", SqlDbType.VarChar);
                path_parm.Value = path;

                MainWindow.sqlite_cmd.CommandText = "UPDATE GPX_File SET id = @Id WHERE Path = @Path;";

                MainWindow.sqlite_cmd.ExecuteNonQuery();

                id++;
            }
        }

        private void updateGpxFileListNumbers()
        {
            int id = 1;
            foreach (GpxFile elementFileList in GpxFileList)
            {
                elementFileList.Number = id.ToString();
                id++;
            }
        }

        private void dodjaOpis(object sender, RoutedEventArgs e)
        {
            if (traceListView.SelectedItem != null)
            {
                var elementFileList = GpxFileList.First(s => s == traceListView.SelectedItem);
                MainWindow.sqlite_cmd = MainWindow.sqlite_conn.CreateCommand();

                SQLiteParameter id = MainWindow.sqlite_cmd.Parameters.Add("@Id", SqlDbType.Int);
                id.Value = Int32.Parse(elementFileList.Number);

                SQLiteParameter description = MainWindow.sqlite_cmd.Parameters.Add("@Description", SqlDbType.VarChar);
                description.Value = textBoxOpis.Text;

                MainWindow.sqlite_cmd.CommandText = "UPDATE GPX_File SET Description = @Description WHERE id = @Id;";

                MainWindow.sqlite_cmd.ExecuteNonQuery();
            }
        }

        private void odswiezListView(object sender, RoutedEventArgs e)
        {
            foreach (GpxFile elementFileList in traceListView.ItemsSource)
            {
                if (!elementFileList.Check && !elementFileList.InMap)
                {
                    var lvitem = traceListView.ItemContainerGenerator.ContainerFromItem(elementFileList) as ListViewItem;
                    lvitem.Foreground = new SolidColorBrush(Colors.Black);
                    lvitem.BorderBrush = null;
                }
                else if (elementFileList.Check && elementFileList.InMap)
                {
                    var lvitem = traceListView.ItemContainerGenerator.ContainerFromItem(elementFileList) as ListViewItem;
                    lvitem.Foreground = elementFileList.color;
                    lvitem.BorderBrush = elementFileList.color;
                    lvitem.BorderThickness = new Thickness(5, 5, 5, 5);
                }
            }
        }

        private void traceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (traceListView.SelectedItem != null)
            {
                textBoxOpis.IsEnabled = true;
                textBoxOpis.Text = readDescriptionFromDataBase();
            }
            else
            {
                textBoxOpis.Text = "";
                textBoxOpis.IsEnabled = false;
            }
        }

        private string readDescriptionFromDataBase()
        {

            var elementFileList = GpxFileList.First(s => s == traceListView.SelectedItem);

            MainWindow.sqlite_cmd = MainWindow.sqlite_conn.CreateCommand();

            SQLiteParameter id = MainWindow.sqlite_cmd.Parameters.Add("@Id", SqlDbType.Int);
            id.Value = Int32.Parse(elementFileList.Number);

            MainWindow.sqlite_cmd.CommandText = "SELECT * FROM GPX_File WHERE id = @Id;";

            MainWindow.sqlite_datareader = MainWindow.sqlite_cmd.ExecuteReader();

            string opis = "";

            while (MainWindow.sqlite_datareader.Read())
            {
                opis += MainWindow.sqlite_datareader["Description"];
            }

            return opis;
        }

        private void clearTraceListViewButton(object sender, RoutedEventArgs e)
        {
            foreach (GpxFile elementFileList in traceListView.ItemsSource)
            {
                MainWindow.sqlite_cmd = MainWindow.sqlite_conn.CreateCommand();

                SQLiteParameter id = MainWindow.sqlite_cmd.Parameters.Add("@Id", SqlDbType.Int);
                id.Value = Int32.Parse(elementFileList.Number);

                MainWindow.sqlite_cmd.CommandText = "DELETE FROM GPX_File WHERE id = @Id;";

                MainWindow.sqlite_cmd.ExecuteNonQuery();
            }

            GpxFileList.Clear();
            polylineList.Clear();
            traceListView.ItemsSource = "";
            traceListView.Items.Refresh();
            mapControl.Markers.Clear();
        }

        private void dellCheckedTraceListViewButton(object sender, RoutedEventArgs e)
        {
            if (traceListView.SelectedItem != null)
            {
                var elementFileList = GpxFileList.First(s => s == traceListView.SelectedItem);
                if (elementFileList.InMap)
                {
                    mapControl.Markers.Remove(polylineList[elementFileList.NumberPolyline]);
                }

                MainWindow.sqlite_cmd = MainWindow.sqlite_conn.CreateCommand();

                SQLiteParameter id = MainWindow.sqlite_cmd.Parameters.Add("@Id", SqlDbType.Int);
                id.Value = Int32.Parse(elementFileList.Number);

                MainWindow.sqlite_cmd.CommandText = "DELETE FROM GPX_File WHERE id = @Id;";

                MainWindow.sqlite_cmd.ExecuteNonQuery();

                updatePrimaryKey();

                GpxFileList.Remove(elementFileList);
                traceListView.ItemsSource = GpxFileList.Select(s => s).ToList();
                updateGpxFileListNumbers();
                traceListView.Items.Refresh();
            }
        }

        private void mapUpdate(object sender, RoutedEventArgs e)
        {
            List<GpxFile> tmpGpxList = new List<GpxFile>();
            foreach (GpxFile elementFileList in traceListView.ItemsSource)
            {
                if (!elementFileList.Check && elementFileList.InMap)
                {
                    mapControl.Markers.Remove(polylineList[elementFileList.NumberPolyline]);
                    elementFileList.InMap = false;
                    var lvitem = traceListView.ItemContainerGenerator.ContainerFromItem(elementFileList) as ListViewItem;
                    lvitem.Foreground = new SolidColorBrush(Colors.Black);
                    lvitem.BorderBrush = null;
                }
                else if (elementFileList.Check && !elementFileList.InMap)
                {
                    if (elementFileList.Track)
                    {
                        GpxFile.trackGPXfile_API_2(elementFileList, polylineList, mapControl);
                        elementFileList.InMap = true;
                    }
                    if (elementFileList.Waypoint)
                    {

                    }
                    if (elementFileList.Route)
                    {
                        GpxFile.routesGPXfile_API_2(elementFileList, polylineList, mapControl);
                        elementFileList.InMap = true;
                    }
                    var lvitem = traceListView.ItemContainerGenerator.ContainerFromItem(elementFileList) as ListViewItem;
                    lvitem.Foreground = elementFileList.color;
                    lvitem.BorderBrush = elementFileList.color;
                    lvitem.BorderThickness = new Thickness(5, 5, 5, 5);
                }
            }
        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMapProvider.WebProxy = WebRequest.GetSystemWebProxy();
            GMapProvider.WebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

            mapControl.MapProvider = GoogleMapProvider.Instance;

            mapControl.Zoom = 2;
            mapControl.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            mapControl.DragButton = MouseButton.Left;
            mapControl.Position = new PointLatLng(20,20);
        }

    }
}
