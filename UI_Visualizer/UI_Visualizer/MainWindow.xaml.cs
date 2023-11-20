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
using Windows.Devices.Enumeration;
using System.Collections.ObjectModel;

using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace UI_Visualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static string DATA_STREAM_UUID = "19B10000-E8F2-537E-4F6C-D104768A1214".ToLower();
    static DeviceInformation device;
    private Graphic droneGraphic;
    private float pitch, yaw, roll;
    private double longitude, latitude;
    private float altitude;

    public MainWindow()
    {
        InitializeComponent();

        MapPoint mapCenterPoint = new MapPoint(-4.075, 52.4141, SpatialReferences.Wgs84);
        MainMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 100000));

        initGraphic();
        Add3DObjectToScene();

        // Populate the ComboBox with available BLE ports
        ConnectBLEDevice();

        Rotate3DObject(0, -90, 0);
        latitude = 52.4141;
        longitude = -4.075;
        altitude = 4;
        moveGraphic(52.4141, -4.075, 4);
    }

    private async void InitBLEDevice()
    {

        while (true)
        {
            if (device == null)
            {
                Thread.Sleep(200);
            }
            // Connect to device
            BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(device.Id);

            GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                Dispatcher.Invoke(() => COMTextBlock.Text += $"Found device\n");
                ProcessBLEdevice(result);
                break;
            }
        }
    }

    private async void ProcessBLEdevice(GattDeviceServicesResult result)
    {
        var services = result.Services;
        foreach (var service in services)
        {
            if (service.Uuid.ToString("D") == DATA_STREAM_UUID)
            {
                Dispatcher.Invoke(() => COMTextBlock.Text += $"Found data stream\n");
                // MessageBox.Show($"Data stream found: {service.Uuid}", "Service Found", MessageBoxButton.OK, MessageBoxImage.Information);
                ProcessService(service);
            }
            // MessageBox.Show($"Service found: {service.Uuid.ToString("D")}", "Service Found", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void ProcessService(GattDeviceService service)
    {
        GattCharacteristicsResult result = await service.GetCharacteristicsAsync();

        if (result.Status == GattCommunicationStatus.Success)
        {
            var characteristics = result.Characteristics;
            foreach (var characteristic in characteristics)
            {

                Dispatcher.Invoke(() => COMTextBlock.Text += $"Found characteristic\n");
                ProcessCharacteristic(characteristic);
                // MessageBox.Show($"Data stream found: {characteristic.GetDescriptors}", "Service Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private async void ProcessCharacteristic(GattCharacteristic characteristic)
    {
        GattCharacteristicProperties properties = characteristic.CharacteristicProperties;

        if (properties.HasFlag(GattCharacteristicProperties.Read))
        {
            // This characteristic supports reading from it.
        }
        if (properties.HasFlag(GattCharacteristicProperties.Write))
        {
            // This characteristic supports writing to it.
        }
        if (properties.HasFlag(GattCharacteristicProperties.Notify))
        {
            // This characteristic supports subscribing to notifications.
            // MessageBox.Show($"Subscribig to notifications", "Subscription", MessageBoxButton.OK, MessageBoxImage.Information);

            Dispatcher.Invoke(() => COMTextBlock.Text += $"Subscribing to characteristic.\n");
            SubscribeToCharacteristicNotif(characteristic);
        }
    }

    private async void SubscribeToCharacteristicNotif(GattCharacteristic characteristic)
    {
        GattCommunicationStatus status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify);
        if (status == GattCommunicationStatus.Success)
        {
            characteristic.ValueChanged += Characteristic_ValueChanged;
            Dispatcher.Invoke(() => COMTextBlock.Text += $"Subscribed to characteristic.\n");
            // Server has been informed of clients interest.
        }
    }

    private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
    {

        var reader = DataReader.FromBuffer(args.CharacteristicValue);
        //MessageBox.Show($"New value from UUID: {sender.Uuid.ToString("D")}", "Subscription Data", MessageBoxButton.OK, MessageBoxImage.Information);
        switch (sender.Uuid.ToString("D"))
        {
            case "19b10001-e8f2-537e-4f6c-d104768a1214": // Pitch
                pitch = readIntReverse(reader);
                Dispatcher.Invoke(() => COMTextBlock.Text += $"Pitch: {pitch}\n");
                Dispatcher.Invoke(() => pitchValue.Text = $"{pitch}");
                Rotate3DObject(yaw, pitch, roll);
                break;

            case "19b10002-e8f2-537e-4f6c-d104768a1214": // Roll
                roll = readIntReverse(reader);
                Dispatcher.Invoke(() => COMTextBlock.Text += $"Roll: {roll}\n");
                Dispatcher.Invoke(() => rollValue.Text = $"{roll}");
                break;

            case "19b10003-e8f2-537e-4f6c-d104768a1214": // Yaw
                yaw = readIntReverse(reader);
                Dispatcher.Invoke(() => COMTextBlock.Text += $"Yaw: {yaw}\n");
                Dispatcher.Invoke(() => yawValue.Text = $"{yaw}");
                break;

            case "19b10005-e8f2-537e-4f6c-d104768a1214": // Latitude
                latitude = readIntReverse(reader);
                Dispatcher.Invoke(() => COMTextBlock.Text += $"Lat: {latitude}\n");
                Dispatcher.Invoke(() => latitudeValue.Text = $"{latitude}");
                break;

            case "19b10006-e8f2-537e-4f6c-d104768a1214": // Longitude
                longitude = readIntReverse(reader);
                Dispatcher.Invoke(() => COMTextBlock.Text += $"Long: {longitude}\n");
                Dispatcher.Invoke(() => longitudeValue.Text = $"{longitude}");
                break;

            case "19b10007-e8f2-537e-4f6c-d104768a1214": // Altitude
                altitude = readIntReverse(reader);
                Dispatcher.Invoke(() => COMTextBlock.Text += $"Alt: {altitude}\n");
                Dispatcher.Invoke(() => altitudeValue.Text = $"{altitude}");
                break;

            case "19b10008-e8f2-537e-4f6c-d104768a1214": // Bat level
                int batLevel = readIntReverse(reader);
                Dispatcher.Invoke(() => COMTextBlock.Text += $"BatLevel: {batLevel}\n");
                Dispatcher.Invoke(() => batteryValue.Text = $"{batLevel}%");
                break;
            default:
                break;
        }
    }

    /**
     * Read the bytes from left to right. Does currently not handle negative numbers
     * 0        =>        0 = 0000 0000 0000 0000 0000 0000 0000
     * 1        => 16777216 = 0001 0000 0000 0000 0000 0000 0000
     * 2        => 33554432 = 0010 0000 0000 0000 0000 0000 0000
     */
    private int readIntReverse(DataReader reader)
    {
        int result = 0;
        int mult = 0;
        for (int i = 0; i < 4; i++)
        {
            byte nextByte = reader.ReadByte();

            // Shift the existing bits in int_pitch to the left by 8 and OR with the new byte
            result += nextByte*(int)Math.Pow(2, i*4 +1);
        }

        return result;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        /*
        if (HelloButton.IsChecked == true){
            moveGraphic(latitude+=0.000001, longitude-=0.000001, altitude+=100);
        }
        */
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

    private void moveGraphic(double latitude, double longitude, float altitude)
    {
        // Assuming droneGraphic is your Graphic with a MapPoint
        MapPoint objectLocation = (MapPoint)droneGraphic.Geometry;

        // Set new latitude, longitude, and altitude values
        double newLatitude = latitude;
        double newLongitude = longitude;
        double newAltitude = altitude;

        // Create a new MapPoint with the updated coordinates
        MapPoint updatedLocation = new MapPoint(newLongitude, newLatitude, newAltitude, SpatialReferences.Wgs84);

        // Update the graphic's geometry
        droneGraphic.Geometry = updatedLocation;
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void Rotate3DObject(double heading, double pitch, double roll)
    {
        // Assuming droneGraphic is your Graphic with a ModelSceneSymbol
        ModelSceneSymbol modelSymbol = (ModelSceneSymbol)droneGraphic.Symbol;
        if (modelSymbol == null)
        {
            return;
        }
        // Set the new heading value to the ModelSceneSymbol
        modelSymbol.Heading = heading;
        modelSymbol.Pitch = pitch;
        modelSymbol.Roll = roll;

        // Update the graphic symbol
        droneGraphic.Symbol = modelSymbol;
    }

    private async void ConnectBLEDevice()
    {
        // Query for extra properties you want returned
        string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

        DeviceWatcher deviceWatcher =
                    DeviceInformation.CreateWatcher(
                            BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                            requestedProperties,
                            DeviceInformationKind.AssociationEndpoint);

        // Register event handlers before starting the watcher.
        // Added, Updated and Removed are required to get all nearby devices
        deviceWatcher.Added += DeviceWatcher_Added;
        deviceWatcher.Updated += DeviceWatcher_Updated;
        deviceWatcher.Removed += DeviceWatcher_Removed;

        // EnumerationCompleted and Stopped are optional to implement.
        deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
        deviceWatcher.Stopped += DeviceWatcher_Stopped;

        // Start the watcher.
        deviceWatcher.Start();

        InitBLEDevice();
    }

    private void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
    {
        //throw new NotImplementedException();
    }

    private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
    {
        //throw new NotImplementedException();
    }

    private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
    {
        //throw new NotImplementedException();
    }

    private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
    {
        //throw new NotImplementedException();
    }

    private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
    {
        if (args.Name == "Portenta-Hopper-Drone")
        {
            device = args;
            // MessageBox.Show($"Device Name: {args.Name}", "Device Found", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        //throw new NotImplementedException();
    }

}