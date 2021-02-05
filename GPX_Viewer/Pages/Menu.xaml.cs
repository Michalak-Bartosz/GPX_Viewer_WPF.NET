using System.Windows;
using System.Windows.Controls;
using GPX_Viewer.MapAPI;

namespace GPX_Viewer
{
    /// <summary>
    /// Interaction logic for Page_0.xaml
    /// </summary>
    public partial class Menu : Page
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void bingsMapClick(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Bing_Maps());
        }

        private void googleMapsClick(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Google_Maps());
        }

        private void opeenStreetMapsClick(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Yandex_Map());
        }
    }
}
