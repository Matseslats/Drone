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

    private Graphic droneGraphic;

    public MainWindow(){
        InitializeComponent();

        MapPoint mapCenterPoint = new MapPoint(-4.075, 52.4141, SpatialReferences.Wgs84);
        MainMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 100000));

        initGraphic();
        Add3DObjectToScene();
        
    }

    private void Button_Click(object sender, RoutedEventArgs e){
        if (HelloButton.IsChecked == true){
            moveGraphic();
        }
    }

    private void initGraphic()
    {

        // Specify the path to your 3D model file (e.g., .gltf or .glb).
        var modelUri = new System.Uri("C:\\Users\\matsh\\Documents\\GitHub\\Drone\\UI_Visualizer\\UI_Visualizer\\Assets\\Drone_Coarse_GLTF.gltf");

        // Create a model symbol from the 3D model file.
        ModelSceneSymbol modelSymbol = ModelSceneSymbol.CreateAsync(modelUri, 1.0).Result;

        // Create a point where you want to place the 3D object.
        MapPoint objectLocation = new MapPoint(-4.075, 52.3141, 7360.0, SpatialReferences.Wgs84);

        // Create a graphic with the 3D symbol and location.
        droneGraphic = new Graphic(objectLocation, modelSymbol);

    }

    private void Add3DObjectToScene()
    {
        // Create a graphics overlay for 3D objects.
        GraphicsOverlay graphicsOverlay = new GraphicsOverlay();

        // Add the graphic to the graphics overlay.
        graphicsOverlay.Graphics.Add(droneGraphic);

        // Add the graphics overlay to the scene.
        MainSceneView.GraphicsOverlays.Add(graphicsOverlay);

        // create an OrbitGeoElementCameraController, pass in the target graphic and initial camera distance
        var orbitGraphicController = new OrbitGeoElementCameraController(droneGraphic, 2);
        MainSceneView.CameraController = orbitGraphicController;
    }

    private void moveGraphic()
    {
        // Get the current location (point) of the graphic.
        var currentPosition = droneGraphic.Geometry as MapPoint;

        // Define new x and y coordinates by applying an offset.
        var newX = currentPosition.X + 0.001;
        var newY = currentPosition.Y + 0.001;

        // Update the point with the new coordinates (graphic will update to show new location).
        var updatedPosition = new MapPoint(newX, newY);
        droneGraphic.Geometry = updatedPosition;
    }
}