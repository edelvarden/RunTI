# Define paths
$exePath = "$PSScriptRoot\bin\Release\RunTI.exe"
$testFile1 = "$PSScriptRoot\tests\test1.txt"
$testFolder2 = "$PSScriptRoot\tests\test2"
$testFolder3 = "$PSScriptRoot\tests\test3"
$testFile3 = "$testFolder3\test3.txt"
$testFolder4 = "$PSScriptRoot\tests\test4"
$testFile4 = "$testFolder4\test4.txt"
$testFolder5 = "$PSScriptRoot\tests\test5"
$testFile5 = "$testFolder5\test5.txt"

# Create test files and folders
New-Item -Path $testFile1 -ItemType File -Force | Out-Null
New-Item -Path $testFolder2 -ItemType Directory -Force | Out-Null
New-Item -Path $testFolder3 -ItemType Directory -Force | Out-Null
New-Item -Path $testFile3 -ItemType File -Force | Out-Null
New-Item -Path $testFolder4 -ItemType Directory -Force | Out-Null
New-Item -Path $testFile4 -ItemType File -Force | Out-Null
New-Item -Path $testFolder5 -ItemType Directory -Force | Out-Null
New-Item -Path $testFile5 -ItemType File -Force | Out-Null

# Set admin rights on testFolder4: only administrators can delete
$acl = Get-Acl $testFolder4
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("Everyone", "FullControl", "ContainerInherit, ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("Administrators", "FullControl", "ContainerInherit, ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl $testFolder4 $acl

cmd /c takeown /f $testFolder4 /r /d y

# Set system rights on testFolder5: only SYSTEM can delete
$acl = Get-Acl $testFolder5
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("Everyone", "FullControl", "ContainerInherit, ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("SYSTEM", "FullControl", "ContainerInherit, ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl $testFolder5 $acl

cmd /c takeown /f $testFolder5 /r /d y

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

# Test destroying each item
$allTestsPassed = $true

# Test 1: Destroy file
Run-DestroyCommand $testFile1
if (Test-PathExists $testFile1) {
    Write-Host -ForegroundColor Red "Test 1 failed: File $testFile1 still exists."
    $allTestsPassed = $false
} else {
    Write-Host -ForegroundColor Green "Test 1 passed: File $testFile1 was successfully destroyed."
}

# Test 2: Destroy empty folder
Run-DestroyCommand $testFolder2
if (Test-PathExists $testFolder2) {
    Write-Host -ForegroundColor Red "Test 2 failed: Folder $testFolder2 still exists."
    $allTestsPassed = $false
} else {
    Write-Host -ForegroundColor Green "Test 2 passed: Folder $testFolder2 was successfully destroyed."
}

# Test 3: Destroy folder with file
Run-DestroyCommand $testFolder3
if (Test-PathExists $testFolder3) {
    Write-Host -ForegroundColor Red "Test 3 failed: Folder $testFolder3 still exists."
    $allTestsPassed = $false
} else {
    Write-Host -ForegroundColor Green "Test 3 passed: Folder $testFolder3 was successfully destroyed."
}

# Test 4: Destroy folder with admin rights
Run-DestroyCommand $testFolder4
if (Test-PathExists $testFolder4) {
    Write-Host -ForegroundColor Red "Test 4 failed: Folder $testFolder4 still exists."
    $allTestsPassed = $false
} else {
    Write-Host -ForegroundColor Green "Test 4 passed: Folder $testFolder4 was successfully destroyed."
}

# Test 5: Destroy folder with system rights
Run-DestroyCommand $testFolder5
if (Test-PathExists $testFolder5) {
    Write-Host -ForegroundColor Red "Test 5 failed: Folder $testFolder5 still exists."
    $allTestsPassed = $false
} else {
    Write-Host -ForegroundColor Green "Test 5 passed: Folder $testFolder5 was successfully destroyed."
}

# Final result
if ($allTestsPassed) {
    Write-Host "All tests passed. The DestroyFileOrFolder function works as expected."
} else {
    Write-Host "Some tests failed. Please check the results above."
}

Pause
