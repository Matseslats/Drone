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

using System.IO.Ports;
using System.Windows.Markup;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace UI_Visualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    private Graphic droneGraphic;
    private float pitch, yaw, roll;
    private float longitude, latitude;

    public MainWindow()
    {
        InitializeComponent();

        MapPoint mapCenterPoint = new MapPoint(-4.075, 52.4141, SpatialReferences.Wgs84);
        MainMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 100000));

        initGraphic();
        Add3DObjectToScene();

        // Populate the ComboBox with available COM ports
        PopulateCOMPorts();
        Rotate3DObject(0, -90, 0);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (HelloButton.IsChecked == true){

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

    private void moveGraphic(float latitude, float longitude)
    {
        droneGraphic.Geometry = new MapPoint(longitude, latitude);
    }

    private void Rotate3DObject(double heading, double pitch, double roll)
    {
        // Assuming droneGraphic is your Graphic with a ModelSceneSymbol
        ModelSceneSymbol modelSymbol = (ModelSceneSymbol)droneGraphic.Symbol;

        // Set the new heading (rotation) for the 3D model in degrees
        double newHeadingDegrees = 45.0;

        // Set the new heading value to the ModelSceneSymbol
        modelSymbol.Heading = heading;
        modelSymbol.Pitch = pitch;
        modelSymbol.Roll = roll;

        // Update the graphic symbol
        droneGraphic.Symbol = modelSymbol;
    }
    
    private void updateDataFromString(String line)
    {
        // Check and remove semicolon at the end
        if (line.EndsWith(";"))
        {
            line = line.Substring(0, line.Length - 1);
        } else
        {
            Console.WriteLine("Incomplete line.");
            return;
        }

        // Split the line into an array of strings
        string[] parts = line.Split(',');

        // Convert the strings to floats
        float float1, float2, float3;

        if (parts.Length == 3 &&
            float.TryParse(parts[0], out float1) &&
            float.TryParse(parts[1], out float2) &&
            float.TryParse(parts[2], out float3))
        {
            // Now, float1, float2, and float3 contain the converted values
            Console.WriteLine($"Float1: {float1}, Float2: {float2}, Float3: {float3}");
            pitch = float1;
            roll = float2;
            yaw = float3;
            Rotate3DObject(yaw, pitch, roll);
        }
        else
        {
            Console.WriteLine("Invalid format or unable to convert to floats.");
        }
    }
    

    private void PopulateCOMPorts()
    {
        string[] availablePorts = SerialPort.GetPortNames();

        if (availablePorts.Length > 0)
        {
            // Add the available ports to the ComboBox
            comPortComboBox.ItemsSource = availablePorts;
            comPortComboBox.SelectedIndex = 0; // Set the default selection if needed

            // Enable the ComboBox and allow user input
            comPortComboBox.IsEnabled = true;
            int comPortCount = comPortComboBox.Items.Count;
            String sOrNot = comPortCount > 1 ? "s" : "";
            COMTextBlock.Text = $"{comPortComboBox.Items.Count} COM port{sOrNot} found.";
        }
        else
        {
            comPortComboBox.IsEnabled = false;
            // MessageBox.Show("No COM ports found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            COMTextBlock.Text = "No COM ports found.";
        }
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        string selectedPort = comPortComboBox.SelectedItem as string;

        if (!string.IsNullOrEmpty(selectedPort))
        {
            // Call your method with the selected COM port
            ReadPortData(selectedPort);
        }
        else
        {
            MessageBox.Show("Please select a COM port.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ReadPortData(string selectedPort)
    {
        SerialPort myport = new SerialPort();
        myport.BaudRate = 9600;
        myport.DataBits = 8;
        myport.StopBits = StopBits.One;
        myport.Parity = Parity.None;
        myport.PortName = selectedPort;

        if (myport.IsOpen)
        {
            MessageBox.Show("Port is already open.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            myport.Close();
        }
        

        myport.Open();

        try
        {
            await Task.Run(() =>
            {
                int dataLength = myport.ReadBufferSize;
                Dispatcher.Invoke(() => COMTextBlock.Text = $"Buffer size: {dataLength}");
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            myport.Close();
        }
    }


    // COM port re-populating
    // Define constant values for Windows messages
    private const int WM_DEVICECHANGE = 0x0219;
    private const int DBT_DEVICEARRIVAL = 0x8000;
    private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    private const int DBT_DEVTYP_PORT = 0x00000003;

    // Define a structure for DEV_BROADCAST_PORT
    [StructLayout(LayoutKind.Sequential)]
    private struct DEV_BROADCAST_PORT
    {
        public int dbcp_size;
        public int dbcp_devicetype;
        public int dbcp_reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public char[] dbcp_name;
    }

    // Override the WndProc method to handle Windows messages
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Get the window handle
        IntPtr hwnd = new WindowInteropHelper(this).Handle;

        // Register for device change notifications
        HwndSource source = HwndSource.FromHwnd(hwnd);
        source.AddHook(new HwndSourceHook(WndProc));
    }

    // WndProc method to handle Windows messages
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_DEVICECHANGE)
        {
            int eventType = wParam.ToInt32();

            if (eventType == DBT_DEVICEARRIVAL || eventType == DBT_DEVICEREMOVECOMPLETE)
            {
                DEV_BROADCAST_PORT devBroadcastPort = Marshal.PtrToStructure<DEV_BROADCAST_PORT>(lParam);

                if (devBroadcastPort.dbcp_devicetype == DBT_DEVTYP_PORT)
                {
                    // USB serial port device has been plugged or unplugged, refresh the COM ports
                    PopulateCOMPorts();
                }
            }
        }

        return IntPtr.Zero;
    }
}