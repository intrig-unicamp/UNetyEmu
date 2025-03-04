using System; // Library to use the Environment class
using System.Net.Sockets; // Library to use the TcpClient and NetworkStream classes
using System.Collections; // Library to use coroutines
using System.Text; // Library to use the Encoding class
using UnityEngine; // Library to use the MonoBehaviour class
using System.Collections.Generic; // Library to use lists
using System.Threading.Tasks; // Library to use tasks
using System.Diagnostics;  // Library to use the ProcessStartInfo class

// Class to control the communication of the base station (Unity to Mininet-WiFi)
public class BaseStationMininetWifi : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Flag to check if the connection with Mininet is established
    [ReadOnly] public bool flagMininetWifiReady = false; 
    
    // Variables to show the coverage radius of the base station
    public Color gizmoColor = Color.green; // Color of the gizmo
    public float radius = 1.0f; // Coverage radius (used for Gizmos)

    // Enum to define message labels
    public enum Label
    {
        FirstSpecialMessage, // this label is for the first message only
        Position // used to send positions timely
    }

    // Time interval between messages in seconds
    public float messageInterval = 1f;

    // Get the current user's username dynamically
    string userName = Environment.UserName;

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    // Variables to store the IP address of the Mininet-WiFi VM
    private TcpClient tcpClient;
    private NetworkStream stream;
    private string response;
    private float lastMessageTime = 0f; // Timestamp of the last message sent
    private bool isFirstMessage = true;
    private string mn_wifi_vm_ip = ""; // IP address of the Mininet-WiFi VM
    private int dstPort = 12345; // send through this port
    private List<string> droneIPs = new List<string>(); // List to store drone IPs (10.0.0.X), where X is 101, 102, 103, and so on
    private int baseIPLastOctet = 101; // Start IP counter from 101
 
    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Create an instance of PowerShellExecutor
        GetVmIp executor = new GetVmIp();

        // Call the ExecuteScript method with the desired parameters
        string vmName = "mn-wifi";
        int adapterIndex = 1; // 0 = Adapter 1; 1 = Adapter 2; and so on...

        // Execute the PowerShell script to get the IP address of the Mininet-WiFi VM
        executor.ExecuteScript(vmName, adapterIndex, userName, this);

        // Start the coroutine when the scene starts
        StartCoroutine(WaitForDrones());

    }

    // -----------------------------------------------------------------------------------------------------
    // Coroutine to wait for the drones to be created:

    IEnumerator WaitForDrones()
    {
        
        // Wait until the drones are created
        while (GameObject.FindGameObjectsWithTag("Drone").Length == 0)
        {
            yield return new WaitForSeconds(1);  // Wait for 1 second before checking again
        }

        // Get all drones and assign IPs
        GameObject[] droneObjects = GameObject.FindGameObjectsWithTag("Drone");

        // Ensure the list is empty before adding new ones
        droneIPs.Clear(); 

        // Assign IPs to the drones
        for (int i = 0; i < droneObjects.Length; i++)
        {
            string ip = $"10.0.0.{baseIPLastOctet + i}"; // Assign sequential IPs
            droneIPs.Add(ip); // Add the IP to the list
        }

        // Log the number of drones and their IPs
        UnityEngine.Debug.Log($"Assigned {droneIPs.Count} drone IPs: {string.Join(", ", droneIPs)}");

        // Once drones are found, log the count and set up the connection
        UnityEngine.Debug.Log("Drones detected. Setting up connection...");
        SendFirstPositionMessage(mn_wifi_vm_ip, dstPort);

        // Start the coroutine for sending drone messages
        StartCoroutine(SendDroneMessagesCoroutine());

    }

    // -----------------------------------------------------------------------------------------------------
    // Coroutine to send drone messages:

    IEnumerator SendDroneMessagesCoroutine()
    {
        
        // Infinite loop to send messages
        while (true)
        {
            
            // Check if the time interval has passed
            if (Time.time - lastMessageTime >= messageInterval)
            {
                
                // Get all drones in the scene
                GameObject[] droneObjects = GameObject.FindGameObjectsWithTag("Drone");

                // Send the position of each drone
                foreach (GameObject drone in droneObjects)
                {
                    
                    // Encode the drone position and ID
                    string message = EncodeDecode.EncodeSingleDronePositionWithLabelAndID(drone, Label.Position.ToString());
                    string droneIP = droneIPs[Array.IndexOf(droneObjects,drone)];

                    // Log the encoded message
                    UnityEngine.Debug.Log($"Encoded message for drone {drone.name} ({droneIP}): {message}");

                    // Start the Python script as a task
                    Task.Run(() => CallSenderPythonScriptTask(droneIP, dstPort, message));

                    // Log the start of the Python script task
                    UnityEngine.Debug.Log("Started Python script task...");

                }

                // Update the last message time
                lastMessageTime = Time.time;

            }

            // Wait for the next frame
            yield return null; 

        }

    }
    
    // -----------------------------------------------------------------------------------------------------
    // Method to set the Mininet-WiFi VM IP address:

    public void SetMnWifiVmIp(string ip)
    {
        mn_wifi_vm_ip = ip; // Set the IP address of the Mininet-WiFi VM
        UnityEngine.Debug.Log($"Mininet-WiFi VM IP set to: {mn_wifi_vm_ip}"); // Log the IP address of the Mininet-WiFi VM
    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:
    
    void FixedUpdate()
    {
        
        // If stream is null or not writable
        if (stream == null || !stream.CanWrite)
        {
            ConnectToMininet(mn_wifi_vm_ip, dstPort); // Attempting to reconnect
            return;
        }

        // If first message is true
        if (isFirstMessage)
        {

            // Get the position of the base station       
            Vector3 baseStationPosition = transform.position;

            // Encode the position and coverage radius message
            string message = EncodeDecode.EncodePositionAndCoverageRadiusMessageOnce(baseStationPosition,
                Label.FirstSpecialMessage.ToString());

            // Send the message to Mininet
            SendMessageToMininet(message, mn_wifi_vm_ip, dstPort);

            // Reset the first message flag
            isFirstMessage = false;

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to call the Python script to send messages to the drones:

    void CallSenderPythonScriptTask(string dstIp, int dstPort, string message)
    {
        
        // Try to call the Python script
        try
        {
            
            // Escape double quotes in the message
            string escapedMessage = message.Replace("\"", "\\\"");

            // Create a new instance of ProcessStartInfo
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "python";

            // Get the path of the Python script
            string folder = Application.dataPath + "/Scripts/Network/broker_tcp_sender.py";
            
            // Set the arguments for the Python script
            psi.Arguments = $"\"{folder}\" \"{dstIp}\" \"{dstPort}\" \"{escapedMessage}\"";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            // Start the Python script
            using (Process process = Process.Start(psi))
            {
                
                // Log the start of the Python script
                UnityEngine.Debug.Log($"(Sender Task) Starting Python script with arguments: {psi.Arguments}");

                // Read the output and error of the Python script
                string result = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Wait for the Python script to exit
                process.WaitForExit();

                // Log the result of the Python script
                if (!string.IsNullOrEmpty(error))
                {
                    UnityEngine.Debug.LogError($"(Sender Task) Broker script error: {error}");
                }
                else
                {
                    UnityEngine.Debug.Log("(Sender Task) Broker response: " + result);
                }

            }

        }
        catch (Exception e)
        {
            
            // Log the error if the Python script fails
            UnityEngine.Debug.LogError($"(Sender Task) Error calling broker_tcp_sender.py: {e.Message}");

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to connect to Mininet-WiFi:

    void ConnectToMininet(string ip, int port)
    {
        
        // Try to connect to Mininet-WiFi
        try
        {
            
            // If the TCP client is not null
            if (tcpClient != null)
            {
                
                // If the TCP client is connected
                if (tcpClient.Connected)
                {
                    UnityEngine.Debug.Log($"Already connected to {ip}:{port}"); // Log the connection
                    return;
                }

                // Close the TCP client
                tcpClient.Close();

            }

            // Log the connection attempt
            UnityEngine.Debug.Log($"Attempting to connect to {ip}:{port}...");

            // Create a new TCP client
            tcpClient = new TcpClient();

            // Connect to the IP address and port
            tcpClient.Connect(ip, port);  // Explicit connection
            stream = tcpClient.GetStream();

            // Log the successful connection
            UnityEngine.Debug.Log($"Connected to {ip}:{port} successfully!");

        }
        catch (SocketException e)
        {
            
            // Log the socket exception
            UnityEngine.Debug.LogError($"SocketException: {e.Message} (Error Code: {e.SocketErrorCode})");

            // Handle the connection failure. End simulation.
            HandleConnectionFailure();

        }
        catch (Exception e)
        {
            
            // Log the general exception
            UnityEngine.Debug.LogError($"General Exception: {e.Message}");

            // Handle the connection failure. End simulation.
            HandleConnectionFailure();

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to send the first position message to Mininet-WiFi:

    void SendFirstPositionMessage(string ip, int port) 
    {
        
        // Connect to Mininet
        ConnectToMininet(ip, port);
        
        // If the stream is not null and can write
        if (stream != null && stream.CanWrite)
        {
            
            // Get the position of the base station
            Vector3 position = transform.position;

            // Encode the position and coverage radius message
            string message = EncodeDecode.EncodePositionAndCoverageRadiusMessageOnce(position, Label.FirstSpecialMessage.ToString());

            // Send the message to Mininet
            UnityEngine.Debug.Log("Encoded JSON message: " + message);
            SendMessageToMininet(message, ip, port);

            // Wait for Mininet's response asynchronously
            _ = WaitForResponseAsync();  // Fire and forget if SendBaseStationPosition isn't async

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to send messages to Mininet-WiFi:

    void SendMessageToMininet(string message, string ip, int port)
    {
        
        // If the TCP client is null, not connected, or the stream is null or cannot write
        if (tcpClient == null || !tcpClient.Connected || stream == null || !stream.CanWrite)
        {
            UnityEngine.Debug.LogError($"Not connected. Attempting to reconnect to {ip}:{port}..."); // Log the reconnection
            ConnectToMininet(ip, port);  // Reconnect to the current target IP
        }

        // If the TCP client is not null and connected, and the stream is not null and can write
        if (tcpClient != null && tcpClient.Connected && stream != null && stream.CanWrite)
        {
            
            // Encode the message to bytes
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Send the message bytes to the stream
            stream.Write(messageBytes, 0, messageBytes.Length);
            UnityEngine.Debug.Log($"Sent message bytes to {ip}:{port}: {message}");
            UnityEngine.Debug.Log($"Sent message to {ip}:{port}: {message}");

        }
        else
        {
            
            // Log the error if the message fails to send
            UnityEngine.Debug.LogError($"Failed to send message to {ip}:{port}");

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to wait for Mininet-WiFi's response asynchronously:

    async Task WaitForResponseAsync()
    {
        
        // Try to wait for the response
        try
        {
            
            // Create a buffer to read the response
            byte[] buffer = new byte[1024];

            // Read the stream asynchronously
            var readTask = stream.ReadAsync(buffer, 0, buffer.Length);

            // Wait for the response or timeout
            var timeoutTask = Task.Delay(60000);

            // If the read task finishes first
            if (await Task.WhenAny(readTask, timeoutTask) == readTask)
            {
                
                // Get the number of bytes read
                int bytesRead = readTask.Result;

                // If there are bytes read
                if (bytesRead > 0)
                {
                    
                    // Get the response from the buffer
                    response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Decode the response to a CoverageData object
                    var coverageData = JsonUtility.FromJson<EncodeDecode.CoverageData>(response);

                    // If the coverage data is not null
                    if (coverageData != null)
                    {
                        
                        // Set the coverage radius using the Base Station GameObject
                        UnityEngine.Debug.Log($"Coverage radius received: {coverageData.coverageRadius}");
                        radius = coverageData.coverageRadius;
                        
                        // Activate the flag to indicate that Mininet is ready
                        flagMininetWifiReady = true;
                        
                    }

                }
                else
                {
                    UnityEngine.Debug.LogWarning("Empty response from Mininet."); // Log the empty response
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Response timed out."); // Log the timeout
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error while waiting for response: {e.Message}"); // Log the error to wait for the response
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Special Unity method to clean the stream once it terminates:

    void OnApplicationQuit()
    {
        CleanupConnection(); // Clean up the connection
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to clean up the connection:

    void CleanupConnection()
    {
        if (stream != null) stream.Close(); // Close the stream
        if (tcpClient != null) tcpClient.Close(); // Close the TCP client
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to draw the coverage radius of the base station:

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor; // Set the color of the gizmo
        Gizmos.DrawWireSphere(transform.position, radius); // Draw the wire sphere
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to handle connection failure:

    void HandleConnectionFailure()
    {
        
        CleanupConnection(); // Clean up the connection

        // Log the connection failure
        UnityEngine.Debug.LogError("Connection failed. Stopping the simulation...");

        // Log the warning to run the Mininet-WiFi topology script
        UnityEngine.Debug.LogWarning("Make sure to run first the mininet_topo.py file in the Mininet-Wifi virtual machine.");
        
        // Stop the simulation
        UnityEditor.EditorApplication.isPlaying = false;

    }

}
