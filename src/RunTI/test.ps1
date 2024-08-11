# Define paths
$exePath = "$PSScriptRoot\bin\Release\RunTI.exe"

# Create test files and folders
function Create-Items {
    param (
        [string]$filePath,
        [string]$folderPath
    )
    if ($filePath) {
        New-Item -Path $filePath -ItemType File -Force | Out-Null
    }
    if ($folderPath) {
        New-Item -Path $folderPath -ItemType Directory -Force | Out-Null
    }
}

# Function to set permissions
function Set-Permissions {
    param (
        [string]$Path,
        [string]$Principal
    )

    if (-not (Test-Path $Path)) {
        Write-Host -ForegroundColor Red "Path does not exist: $Path"
        return
    }

    # Get the existing ACL
    $acl = Get-Acl $Path

    # Define the new owner
    $owner = New-Object System.Security.Principal.NTAccount($Principal)
    $acl.SetOwner($owner)

    # Deny permissions to Everyone and Users
    $denyEveryoneRule = New-Object System.Security.AccessControl.FileSystemAccessRule("Everyone", "FullControl", "ContainerInherit, ObjectInherit", "None", "Deny")
    $denyUsersRule = New-Object System.Security.AccessControl.FileSystemAccessRule("Users", "FullControl", "ContainerInherit, ObjectInherit", "None", "Deny")
    $acl.AddAccessRule($denyEveryoneRule)
    $acl.AddAccessRule($denyUsersRule)
    
    # Allow permissions to the specified principal
    $allowRule = New-Object System.Security.AccessControl.FileSystemAccessRule($Principal, "FullControl", "ContainerInherit, ObjectInherit", "None", "Allow")
    $acl.AddAccessRule($allowRule)

    # Apply the updated ACL
    Set-Acl -Path $Path -AclObject $acl
}

# Function to check if a path exists with a delay
function Test-PathExists {
    param (
        [string]$path
    )
    Start-Sleep -Milliseconds 1200  # Add delay to allow file system update
    return Test-Path $path
}

# Function to run the DestroyFileOrFolder command
function Run-DestroyCommand {
    param (
        [string]$path
    )
    & $exePath /DestroyFileOrFolder $path
}

# Define and run test cases
function Test-1 {
    $filePath = "$PSScriptRoot\tests\Test 1.txt"
    Create-Items -filePath $filePath
    Run-DestroyCommand -path $filePath
    if (Test-PathExists -path $filePath) {
        Write-Host -ForegroundColor Red "Test 1 failed: File $filePath still exists."
    } else {
        Write-Host -ForegroundColor Green "Test 1 passed: File $filePath was successfully destroyed."
    }
}

function Test-2 {
    $folderPath = "$PSScriptRoot\tests\Test 2"
    Create-Items -folderPath $folderPath
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "Test 2 failed: Folder $folderPath still exists."
    } else {
        Write-Host -ForegroundColor Green "Test 2 passed: Folder $folderPath was successfully destroyed."
    }
}

function Test-3 {
    $folderPath = "$PSScriptRoot\tests\Test 3"
    $filePath = Join-Path -Path $folderPath -ChildPath "Test 3.txt"
    Create-Items -folderPath $folderPath -filePath $filePath
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "Test 3 failed: Folder $folderPath still exists."
    } else {
        Write-Host -ForegroundColor Green "Test 3 passed: Folder $folderPath was successfully destroyed."
    }
}

function Test-4 {
    $filePath = "$PSScriptRoot\tests\Test 4.txt"
    Create-Items -filePath $filePath
    Set-Permissions -path $filePath -principal "Administrators"
    Run-DestroyCommand -path $filePath
    if (Test-PathExists -path $filePath) {
        Write-Host -ForegroundColor Red "Test 4 failed: File $filePath still exists."
    } else {
        Write-Host -ForegroundColor Green "Test 4 passed: File $filePath was successfully destroyed."
    }
}

function Test-5 {
    $folderPath = "$PSScriptRoot\tests\Test 5"
    Create-Items -folderPath $folderPath
    Set-Permissions -path $folderPath -principal "Administrators"
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "Test 5 failed: Folder $folderPath still exists."
    } else {
        Write-Host -ForegroundColor Green "Test 5 passed: Folder $folderPath was successfully destroyed."
    }
}

function Test-6 {
    $folderPath = "$PSScriptRoot\tests\Test 6"
    $filePath = Join-Path -Path $folderPath -ChildPath "Test 6.txt"
    Create-Items -folderPath $folderPath -filePath $filePath
    Set-Permissions -path $folderPath -principal "Administrators"
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "Test 6 failed: Folder $folderPath still exists."
    } else {
        Write-Host -ForegroundColor Green "Test 6 passed: Folder $folderPath was successfully destroyed."
    }
}

function Test-7 {
    $folderPath = "$PSScriptRoot\tests\Test 7"
    $filePath = Join-Path -Path $folderPath -ChildPath "Test 8.txt"
    Create-Items -filePath $filePath
    Set-Permissions -path $folderPath -principal "SYSTEM"
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "Test 7 failed: Folder $folderPath still exists."
    } else {
        Write-Host -ForegroundColor Green "Test 7 passed: Folder $folderPath was successfully destroyed."
    }
}

function Test-8 {
    $folderPath = "$PSScriptRoot\tests\Test 8"
    Create-Items -folderPath $folderPath
    Set-Permissions -path $folderPath -principal "SYSTEM"
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "Test 8 failed: Folder $folderPath still exists."
    } else {
        Write-Host -ForegroundColor Green "Test 8 passed: Folder $folderPath was successfully destroyed."
    }
}

function Test-9 {
    $folderPath = "$PSScriptRoot\tests\Test 9"
    $filePath = Join-Path -Path $folderPath -ChildPath "Test 9.txt"
    Create-Items -folderPath $folderPath -filePath $filePath
    Set-Permissions -path $folderPath -principal "SYSTEM"
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "Test 9 failed: Folder $folderPath still exists."
    } else {
        Write-Host -ForegroundColor Green "Test 9 passed: Folder $folderPath was successfully destroyed."
    }
}

# Execute all tests
Test-1
Test-2
Test-3
Test-4
Test-5
Test-6
Test-7
Test-8
Test-9

# Final result
Write-Host "All tests executed. Please check the results above."

Pause
