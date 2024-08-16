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

# Set file or folder as read-only
function Set-ReadOnlyAttribute {
    param (
        [string]$Path
    )
    if (Test-Path $Path) {
        $item = Get-Item $Path
        $item.Attributes = 'ReadOnly'
    } else {
        Write-Host -ForegroundColor Red "Path does not exist: $Path"
    }
}

# Set permissions for a path
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

# Check if a path exists with a delay
function Test-PathExists {
    param (
        [string]$path
    )
    Start-Sleep -Milliseconds 1200  # Add delay to allow file system update
    return Test-Path $path
}

# Run the DestroyFileOrFolder command
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
        Write-Host -ForegroundColor Red "1. Should delete file: FAILED"
    } else {
        Write-Host -ForegroundColor Green "1. Should delete file: PASSED"
    }
}

function Test-2 {
    $folderPath = "$PSScriptRoot\tests\Test 2"
    Create-Items -folderPath $folderPath
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "2. Should delete empty folder: FAILED"
    } else {
        Write-Host -ForegroundColor Green "2. Should delete empty folder: PASSED"
    }
}

function Test-3 {
    $folderPath = "$PSScriptRoot\tests\Test 3"
    $filePath = Join-Path -Path $folderPath -ChildPath "Test 3.txt"
    Create-Items -folderPath $folderPath -filePath $filePath
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "3. Should delete folder with files: FAILED"
    } else {
        Write-Host -ForegroundColor Green "3. Should delete folder with files: PASSED"
    }
}

function Test-4 {
    $filePath = "$PSScriptRoot\tests\Test 4.txt"
    Create-Items -filePath $filePath
    Set-Permissions -path $filePath -principal "Administrators"
    Run-DestroyCommand -path $filePath
    if (Test-PathExists -path $filePath) {
        Write-Host -ForegroundColor Red "4. Should delete file with system rights: FAILED"
    } else {
        Write-Host -ForegroundColor Green "4. Should delete file with system rights: PASSED"
    }
}

function Test-5 {
    $folderPath = "$PSScriptRoot\tests\Test 5"
    Create-Items -folderPath $folderPath
    Set-Permissions -path $folderPath -principal "Administrators"
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "5. Should delete folder with system rights: FAILED"
    } else {
        Write-Host -ForegroundColor Green "5. Should delete folder with system rights: PASSED"
    }
}

function Test-6 {
    $folderPath = "$PSScriptRoot\tests\Test 6"
    $filePath = Join-Path -Path $folderPath -ChildPath "Test 6.txt"
    Create-Items -folderPath $folderPath -filePath $filePath
    Set-Permissions -path $folderPath -principal "Administrators"
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "6. Should delete folder with files and system rights: FAILED"
    } else {
        Write-Host -ForegroundColor Green "6. Should delete folder with files and system rights: PASSED"
    }
}

function Test-7 {
    $folderPath = "$PSScriptRoot\tests\Test 7"
    $filePath = Join-Path -Path $folderPath -ChildPath "Test 7.txt"
    Create-Items -folderPath $folderPath -filePath $filePath
    Set-Permissions -path $folderPath -principal "SYSTEM"
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "7. Should delete folder with files and SYSTEM rights: FAILED"
    } else {
        Write-Host -ForegroundColor Green "7. Should delete folder with files and SYSTEM rights: PASSED"
    }
}

function Test-8 {
    $folderPath = "$PSScriptRoot\tests\Test 8"
    Create-Items -folderPath $folderPath
    Set-Permissions -path $folderPath -principal "SYSTEM"
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "8. Should delete empty folder with SYSTEM rights: FAILED"
    } else {
        Write-Host -ForegroundColor Green "8. Should delete empty folder with SYSTEM rights: PASSED"
    }
}

function Test-9 {
    $folderPath = "$PSScriptRoot\tests\Test 9"
    $filePath = Join-Path -Path $folderPath -ChildPath "Test 9.txt"
    Create-Items -folderPath $folderPath -filePath $filePath
    Set-Permissions -path $folderPath -principal "SYSTEM"
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "9. Should delete folder with SYSTEM rights and files: FAILED"
    } else {
        Write-Host -ForegroundColor Green "9. Should delete folder with SYSTEM rights and files: PASSED"
    }
}

function Test-10 {
    $filePath = "$PSScriptRoot\tests\Test 10.txt"
    Create-Items -filePath $filePath
    Set-ReadOnlyAttribute -Path $filePath
    Run-DestroyCommand -path $filePath
    if (Test-PathExists -path $filePath) {
        Write-Host -ForegroundColor Red "10. Should delete read-only file: FAILED"
    } else {
        Write-Host -ForegroundColor Green "10. Should delete read-only file: PASSED"
    }
}

function Test-11 {
    $folderPath = "$PSScriptRoot\tests\Test 11"
    Create-Items -folderPath $folderPath
    Set-ReadOnlyAttribute -Path $folderPath
    Run-DestroyCommand -path $folderPath
    if (Test-PathExists -path $folderPath) {
        Write-Host -ForegroundColor Red "11. Should delete read-only folder: FAILED"
    } else {
        Write-Host -ForegroundColor Green "11. Should delete read-only folder: PASSED"
    }
}

# Execute tests
Test-1
Test-2
Test-3
Test-4
Test-5
Test-6
Test-7
Test-8
Test-9
Test-10
Test-11

Write-Host "All tests executed. Please check the results above."
Pause
