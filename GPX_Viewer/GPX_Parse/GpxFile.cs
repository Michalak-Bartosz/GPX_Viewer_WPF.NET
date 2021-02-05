using Finisar.SQLite;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using Gpx;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GPX_Viewer.GPX_Parse
{
    class GpxFile
    {
        public bool Track { get; set; }
        public bool Route { get; set; }
        public bool Waypoint { get; set; }
        public bool Check { get; set; }
        public bool InMap { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Path { get; set; }
        public int NumberPolyline { get; set; }
        public int NumberWaypoint { get; set; }
        public SolidColorBrush color { get; set; }
        public GpxReader reader;

        private static Random random = new Random();

        public static bool ReadGPXFile(string fileNamePath, GpxFile elementFileList)
        {
            if (fileNamePath != "" )
            {
                try
                {
                    FileStream readFile = File.Open(elementFileList.Path, FileMode.Open);

                    GpxReader reader = new GpxReader(readFile);
                    bool OK = false;
                    do
                    {
                        OK = true;
                    }
                    while (reader.Read());

                    if (OK)
                    {
                        if (reader.Track != null || reader.WayPoint != null || reader.Route != null)
                        {
                            elementFileList.reader = reader;
                            if (reader.Track != null)
                            {
                                elementFileList.Track = true;
                            }
                            if (reader.WayPoint != null)
                            {
                                elementFileList.Waypoint = true;
                            }
                            if (reader.Route != null)
                            {
                                elementFileList.Route = true;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Plik GPX nie zawiera żadnych tras, które można wyświetlić na mapie");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Nie udało się odczytaj prawidłowo pliku.\nPrawdopodobnie jest on uszkodzony!");
                    }
                    readFile.Close();
                }
                catch (System.IO.FileNotFoundException)
                {
                    MessageBox.Show("WCZYTYWANIE PLIKÓW Z BAZY DANYCH\n\nPlik GPX o ścierzce dostępu:\n" + elementFileList.Path + "\nNie istnieje!");

                    MainWindow.sqlite_cmd = MainWindow.sqlite_conn.CreateCommand();

                    SQLiteParameter id = MainWindow.sqlite_cmd.Parameters.Add("@Id", SqlDbType.Int);
                    id.Value = Int32.Parse(elementFileList.Number);

                    MainWindow.sqlite_cmd.CommandText = "DELETE FROM GPX_File WHERE id = @Id;";

                    MainWindow.sqlite_cmd.ExecuteNonQuery();
                    return false;
                }
                return true;
            }
            else
            {
                MessageBox.Show("Brak plików GPX!\nWybierz plik i wczytaj do progoramu.");
                return false;
            }
        }

        public static void trackGPXfile_API_1(GpxFile file, List<MapPolyline> polylineList_1, Map mapControl_1)
        {
            if (file.reader.Track.Segments != null)
            {
                IList<GpxTrackSegment> segmentList = new List<GpxTrackSegment>();
                segmentList = file.reader.Track.Segments;

                foreach (GpxTrackSegment segment in segmentList)
                {
                    GpxPointCollection<GpxTrackPoint> trackPointsList = new GpxPointCollection<GpxTrackPoint>();
                    foreach (GpxTrackPoint point in segment.TrackPoints)
                    {
                        trackPointsList.AddPoint(point);
                    }

                    LocationCollection lokalizacjeLista = new LocationCollection();
                    foreach (GpxTrackPoint point in trackPointsList)
                    {
                        lokalizacjeLista.Add(new Location(point.Latitude, point.Longitude));
                    }
                    drawTraceAndRouteOnMap_API_1(lokalizacjeLista, polylineList_1, file, mapControl_1);
                }
            }
        }

        public static void trackGPXfile_API_2(GpxFile file, List<GMapRoute> polylineList_2, GMapControl mapControl_2)
        {
            if (file.reader.Track.Segments != null)
            {
                IList<GpxTrackSegment> segmentList = new List<GpxTrackSegment>();
                segmentList = file.reader.Track.Segments;

                foreach (GpxTrackSegment segment in segmentList)
                {
                    GpxPointCollection<GpxTrackPoint> trackPointsList = new GpxPointCollection<GpxTrackPoint>();
                    foreach (GpxTrackPoint point in segment.TrackPoints)
                    {
                        trackPointsList.AddPoint(point);
                    }

                    List<PointLatLng> lokalizacjeLista = new List<PointLatLng>();
                    foreach (GpxTrackPoint point in trackPointsList)
                    {
                        lokalizacjeLista.Add(new PointLatLng(point.Latitude, point.Longitude));
                    }
                    drawTraceAndRouteOnMap_API_2(lokalizacjeLista, polylineList_2, file, mapControl_2);
                }
            }
        }

        public static void routesGPXfile_API_1(GpxFile file, List<MapPolyline> polylineList_1, Map mapControl_1)
        {
            if (file.reader.Route.RoutePoints != null)
            {
                IList<GpxRoutePoint> routePointsList = new List<GpxRoutePoint>();
                routePointsList = file.reader.Route.RoutePoints;

                LocationCollection lokalizacjeLista = new LocationCollection();
                foreach (GpxRoutePoint point in routePointsList)
                {
                    lokalizacjeLista.Add(new Location(point.Latitude, point.Longitude));
                }
                drawTraceAndRouteOnMap_API_1(lokalizacjeLista, polylineList_1, file, mapControl_1);
            }
        }

        public static void routesGPXfile_API_2(GpxFile file, List<GMapRoute> polylineList_2, GMapControl mapControl_2)
        {
            if (file.reader.Route.RoutePoints != null)
            {
                IList<GpxRoutePoint> routePointsList = new List<GpxRoutePoint>();
                routePointsList = file.reader.Route.RoutePoints;

                List<PointLatLng> lokalizacjeLista = new List<PointLatLng>();
                foreach (GpxRoutePoint point in routePointsList)
                {
                    lokalizacjeLista.Add(new PointLatLng(point.Latitude, point.Longitude));
                }
                drawTraceAndRouteOnMap_API_2(lokalizacjeLista, polylineList_2, file, mapControl_2);
            }
        }

        private static void drawTraceAndRouteOnMap_API_1(LocationCollection lista, List<MapPolyline> polylineList_1, GpxFile file, Map mapControl_1)
        {
            if (file.NumberPolyline == -1)
            {
                polylineList_1.Add(new MapPolyline());
                file.NumberPolyline = polylineList_1.Count() - 1;

                file.color = new SolidColorBrush(Color.FromRgb((byte)random.Next(1, 255), (byte)random.Next(1, 255), (byte)random.Next(1, 233)));
                polylineList_1[file.NumberPolyline].Stroke = file.color;
                polylineList_1[file.NumberPolyline].StrokeThickness = 5;
                polylineList_1[file.NumberPolyline].Opacity = 1;
                polylineList_1[file.NumberPolyline].Locations = lista;
            }
            mapControl_1.Children.Add(polylineList_1[file.NumberPolyline]);
        }

        private static void drawTraceAndRouteOnMap_API_2(List<PointLatLng> lista, List<GMapRoute> polylineList_2, GpxFile file, GMapControl mapControl_2)
        {
            if (file.NumberPolyline == -1)
            {
                polylineList_2.Add(new GMapRoute(lista));
                file.NumberPolyline = polylineList_2.Count() - 1;

                polylineList_2[file.NumberPolyline].ZIndex = 10;
                polylineList_2[file.NumberPolyline].RegenerateShape(mapControl_2);
                file.color = new SolidColorBrush(Color.FromRgb((byte)random.Next(1, 255), (byte)random.Next(1, 255), (byte)random.Next(1, 233)));
                (polylineList_2[file.NumberPolyline].Shape as System.Windows.Shapes.Path).Stroke = file.color;
                (polylineList_2[file.NumberPolyline].Shape as System.Windows.Shapes.Path).StrokeThickness = 5;
                (polylineList_2[file.NumberPolyline].Shape as System.Windows.Shapes.Path).Effect = null;
            }
            mapControl_2.Markers.Add(polylineList_2[file.NumberPolyline]);
        }
    }
}
