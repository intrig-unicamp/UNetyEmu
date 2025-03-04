using System; // Library to use DateTime class
using System.IO; // Library to use File class
using System.Text; // Library to use StringBuilder class
using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Static class to save mission events to a CSV file
public static class MissionLogger
{

    // -----------------------------------------------------------------------------------------------------
    // Private static variables of this class:

    // Create the folder path where the CSV files will be saved
    private static string folderPath = Path.Combine(Application.dataPath, "MissionsLogs");

    // This will store the file path for the current simulation CSV
    private static string currentFilePath;

    // -----------------------------------------------------------------------------------------------------
    // Call this method when the simulation starts to create a new file:

    public static void StartNewSimulationLog()
    {
        
        // Ensure the "MissionsLogs" folder exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Create a new unique file name using the current date and time
        string fileName = "MissionsLogs_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        currentFilePath = Path.Combine(folderPath, fileName);

        // Create a StringBuilder to store CSV data
        StringBuilder csvData = new StringBuilder();

        // Add the header row to the CSV file if it's the first time creating it
        csvData.AppendLine("PlayerName,CurrentTime,Latitude,Longitude,Altitude,Azimuth,BatteryLevel,CurrentState,MissionId,MissionStatus,arrivalDateTime,action,priority,packageWeight");

        // Write the header to the new CSV file
        File.WriteAllText(currentFilePath, csvData.ToString());

    }

    // -----------------------------------------------------------------------------------------------------
    // Call this method to log each mission event during the simulation:

    public static void LogMissionEvent(string playerName, string currentTime, float latitude, float longitude, float altitude, float azimuth, float batteryLevel, string currentState, string missionId, string missionStatus, string arrivalDateTime, string action, int priority, float packageWeight)
    {
        
        // Try to write the mission event data to the CSV file
        try
        {
            
            // Check if the current simulation file exists (it should if StartNewSimulationLog() has been called)
            if (string.IsNullOrEmpty(currentFilePath) || !File.Exists(currentFilePath))
            {
                return;
            }

            // Create a StringBuilder to append data for this event
            StringBuilder csvData = new StringBuilder();

            // Append the mission event data as a new row in CSV format
            csvData.AppendLine($"{playerName},{currentTime},{latitude},{longitude},{altitude},{azimuth},{batteryLevel},{currentState},{missionId},{missionStatus},{arrivalDateTime},{action},{priority},{packageWeight}");

            // Append the new row to the existing CSV file
            File.AppendAllText(currentFilePath, csvData.ToString());

        }
        catch (Exception e)
        {
            
            // Log any errors that occur while writing to the file
            Debug.LogError("Error writing to CSV file: " + e.Message);

        }

    }

}
