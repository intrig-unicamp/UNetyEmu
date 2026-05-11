# Libraries
import subprocess # For launching Unity executable
import platform # For OS detection
import os # For file path handling
import urllib.request # For downloading release from GitHub
import tarfile # For extracting .tar.gz archives
import zipfile # For extracting .zip archives (Windows)

# -------------------------------------------------

# Set the name of the Unity executable
LINUX_EXECUTABLE = "Linux/LiUSmall.x86_64"
WINDOWS_EXECUTABLE = "Windows/UNetyEmu.exe"

# URLs of the release builds
LINUX_DOWNLOAD_URL = "https://github.com/intrig-unicamp/UNetyEmu/releases/download/aamas26-release/LiUSmall-Linux.tar.gz"
WINDOWS_DOWNLOAD_URL = "https://github.com/intrig-unicamp/UNetyEmu/releases/download/aamas26-release/LiUSmall-Windows.zip"

# Archive names
LINUX_ARCHIVE = "LiUSmall-Linux.tar.gz"
WINDOWS_ARCHIVE = "LiUSmall-Windows.zip"

# -------------------------------------------------
# Function to download and extract the Unity build
def download_build(url, archive_name, dest_folder):

    print("\nUnity build not found. Downloading...")

    archive_path = os.path.join(dest_folder, archive_name)

    # Download release file
    urllib.request.urlretrieve(url, archive_path)

    print("Download complete. Extracting...")

    # Extract tar.gz (Linux)
    if archive_name.endswith(".tar.gz"):
        with tarfile.open(archive_path, "r:gz") as tar:
            tar.extractall(dest_folder)

    # Extract zip (Windows)
    elif archive_name.endswith(".zip"):
        with zipfile.ZipFile(archive_path, 'r') as zip_ref:
            zip_ref.extractall(dest_folder)

    # Remove compressed file
    os.remove(archive_path)

    print("Extraction complete.")

# -------------------------------------------------
# Function to check and set execute permissions for the Unity executable
def secure_permission_to_execute(path):
    
    # Only needed on Linux
    if platform.system() == "Linux":
        if not os.access(path, os.X_OK):
            print(f"\nThe file {path} does not have execute permissions. Adding permissions...")
            subprocess.run(["chmod", "+x", path], check=True)

# -------------------------------------------------
# Launch the Unity application
def launch_unity():
    
    # Detect the operating system
    system = platform.system()
    
    # Get the directory of the current script
    script_dir = os.path.dirname(os.path.abspath(__file__))

    # Linux executable
    if system == "Linux":

        UNITY_EXE = os.path.join(script_dir, LINUX_EXECUTABLE)

        # Download build if executable does not exist
        if not os.path.exists(UNITY_EXE):
            download_build(
                LINUX_DOWNLOAD_URL,
                LINUX_ARCHIVE,
                script_dir
            )

    # Windows executable
    elif system == "Windows":

        UNITY_EXE = os.path.join(script_dir, WINDOWS_EXECUTABLE)

        # Download build if executable does not exist
        if not os.path.exists(UNITY_EXE):
            download_build(
                WINDOWS_DOWNLOAD_URL,
                WINDOWS_ARCHIVE,
                script_dir
            )

    else:
        raise Exception("Unsupported operating system") # Unsupported OS

    # Check if the file has execute permissions
    secure_permission_to_execute(UNITY_EXE)

    # Check if the file exists
    if not os.path.exists(UNITY_EXE):
        raise FileNotFoundError(f"Executable not found in: {UNITY_EXE}")

    # Launch Unity executable
    print(f"\nLaunching Unity: {UNITY_EXE}")
    return subprocess.Popen(UNITY_EXE)

# -------------------------------------------------
# Main routine to run communication with Unity
def main():
    
    # Launch Unity
    launch_unity()

# -------------------------------------------------
# Main function to run the script
if __name__ == "__main__":
    main()
