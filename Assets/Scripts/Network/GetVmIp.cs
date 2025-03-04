using System; // Library to use the Environment class
using System.Diagnostics; // Library to use the ProcessStartInfo and Process classes
using System.Text.RegularExpressions; // Library to use the Regex class
using UnityEngine; // Library to use the Application class

// Class to get the IP address of the Mininet-WiFi VM
public class GetVmIp
{
    
    // -----------------------------------------------------------------------------------------------------
    // Class to execute the PowerShell script to get the IP address of the Mininet-WiFi VM:

    public void ExecuteScript(string vmName, int adapterIndex, string userName, BaseStationMininetWifi baseStation)
    {
        
        // Use the Unity project's relative path instead of a hardcoded path
        string scriptPath = Application.dataPath + "/Scripts/Network/get_vm_ip.ps1";
        UnityEngine.Debug.Log($"Executing script: {scriptPath}");

        // Create the PowerShell command
        string command = $"& '{scriptPath}' -VMName '{vmName}' -AdapterIndex {adapterIndex}";

        // Create the process start info
        ProcessStartInfo processStartInfo = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -NoProfile -Command \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Try to start the PowerShell process
        try
        {
            
            // Log the start of the process
            UnityEngine.Debug.Log("Getting the VM's IP...");

            // Start the PowerShell process
            using (Process process = Process.Start(processStartInfo))
            {
                
                // Check if the process is null
                if (process == null)
                {
                    UnityEngine.Debug.LogError("Failed to start PowerShell process.");
                    return;
                }

                // Read the output and error streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                UnityEngine.Debug.Log($"Output:\n{output}");

                // Log any errors
                if (!string.IsNullOrEmpty(error))
                {
                    UnityEngine.Debug.LogError($"Error:\n{error}");
                }

                // Wait for the process to exit
                process.WaitForExit();

                // Extract the IP address from the output
                string extractedIp = ExtractIpAddress(output);

                // Pass extracted IP to BaseStation
                if (!string.IsNullOrEmpty(extractedIp))
                {
                    UnityEngine.Debug.Log($"Extracted VM IP: {extractedIp}");
                    baseStation.SetMnWifiVmIp(extractedIp);
                }
                else
                {
                    UnityEngine.Debug.LogError("Failed to extract IP address.");
                }

                // Log the end of the process
                UnityEngine.Debug.Log("PowerShell process finished.");

            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error executing PowerShell script: " + ex.Message);
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to extract the IP address from the PowerShell script output:

    private string ExtractIpAddress(string input)
    {
        
        // Remove any leading/trailing whitespace or newlines
        input = input.Trim(); 

        // Use a regular expression to extract the IP address
        Regex ipRegex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", RegexOptions.Multiline);
        MatchCollection matches = ipRegex.Matches(input);

        // Return the last detected IP address
        if (matches.Count > 0)
        {
            return matches[matches.Count - 1].Value; // Get the last detected IP (most likely the correct one)
        }

        // Return an empty string if no IP address was found
        return string.Empty;

    }

}
