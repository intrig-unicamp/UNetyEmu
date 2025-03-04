param (
    [string]$VMName,  # VM Name (e.g., "mn-wifi")
    [int]$AdapterIndex = 0  # Adapter index (default to 0 for Adapter 1)
)

# Get the correct "Program Files" path dynamically
$ProgramFilesPath = "C:\Program Files"

# Construct the full path to VBoxManage
$VBoxManage = "$ProgramFilesPath\Oracle\VirtualBox\VBoxManage.exe"

# Construct the guest property path dynamically for the adapter
$guestPropertyPath = "/VirtualBox/GuestInfo/Net/$AdapterIndex/V4/IP"

# Get the IP address of the specified adapter
$ip = & "$VBoxManage" guestproperty get $VMName $guestPropertyPath

# Extract only the IP address value (remove the "Value:" part)
$ipValue = $ip -replace "Value:\s+", ""

# Output result
Write-Output "VM '$VMName' Adapter $AdapterIndex IP: $ipValue"
