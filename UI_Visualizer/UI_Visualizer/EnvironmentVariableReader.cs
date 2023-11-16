using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace UI_Visualizer;

public class EnvironmentVariableReader
{
    public static string GetEnvironmentVariable(string variableName)
    {
        string variableValue = Environment.GetEnvironmentVariable(variableName);

        if (variableValue != null)
        {
            return variableValue;
        }
        else
        {
            // You can throw an exception or handle the case as appropriate for your application
            throw new InvalidOperationException($"The environment variable '{variableName}' is not set.");
        }
    }
}
