using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;

namespace UI_Visualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window{
    public MainWindow(){
        InitializeComponent();

        MapPoint mapCenterPoint = new MapPoint(-4.075, 52.4141, SpatialReferences.Wgs84);
        MainMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 100000));
    }

    private void Button_Click(object sender, RoutedEventArgs e){
        if (HelloButton.IsChecked == true){
            MessageBox.Show("Hello.");
        }
    }

}