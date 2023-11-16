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


using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;

namespace UI_Visualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window{
    public MainWindow(){
        InitializeComponent();

        MapPoint mapCenterPoint = new MapPoint(-4.075, 52.4141, SpatialReferences.Wgs84);
        MainMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 100000));

        Add3DObjectToScene();
    }

    private void Button_Click(object sender, RoutedEventArgs e){
        if (HelloButton.IsChecked == true){
            MessageBox.Show("Hello.");
        }
    }

    private void Add3DObjectToScene()
    {
        // Create a graphics overlay for 3D objects.
        GraphicsOverlay graphicsOverlay = new GraphicsOverlay();

        // Specify the path to your 3D model file (e.g., .gltf or .glb).
        var modelUri = new System.Uri("C:\\Users\\matsh\\Documents\\GitHub\\Drone\\UI_Visualizer\\UI_Visualizer\\Assets\\Drone_Coarse_GLTF.gltf");

        // Create a model symbol from the 3D model file.
        ModelSceneSymbol modelSymbol = ModelSceneSymbol.CreateAsync(modelUri, 1.0).Result;

        // Create a point where you want to place the 3D object.
        MapPoint objectLocation = new MapPoint(-4.075, 52.3141, 7360.0, SpatialReferences.Wgs84);

        // Create a graphic with the 3D symbol and location.
        Graphic droneGraphic = new Graphic(objectLocation, modelSymbol);

        // Add the graphic to the graphics overlay.
        graphicsOverlay.Graphics.Add(droneGraphic);

        // Add the graphics overlay to the scene.
        MainSceneView.GraphicsOverlays.Add(graphicsOverlay);

        // create an OrbitGeoElementCameraController, pass in the target graphic and initial camera distance
        var orbitGraphicController = new OrbitGeoElementCameraController(droneGraphic, 2);
        MainSceneView.CameraController = orbitGraphicController;
    }

}