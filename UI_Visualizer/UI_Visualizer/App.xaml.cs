using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace UI_Visualizer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        string apiKeyVarName = "ARCGIS_API_KEY";

        try
        {
            string apiKey = EnvironmentVariableReader.GetEnvironmentVariable(apiKeyVarName);
            // Note: it is not best practice to store API keys in source code.
            // The API key is referenced here for the convenience of this tutorial.
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = apiKey;
            Debug.WriteLine("Aquired ArcGIS API KEY");
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
}

