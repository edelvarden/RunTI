function Invoke-MargeRegFile {
    [CmdletBinding()]
    param(
      [Parameter(Mandatory = $true)]
      [string]$Url
    )

  try {

    $hash = [System.Guid]::NewGuid().ToString("N")
    $tempDir = $env:TEMP
    $tempFile = (Join-Path $tempDir "$hash.reg")

    (New-Object System.Net.WebClient).DownloadFile($Url, $tempFile)

    # Merge the .reg file silently
    Start-Process regedit -ArgumentList "/s", $tempFile -Wait

    Write-Host "Registry changes merged successfully."
  }
  catch {
      Write-Warning "Failed to merge registry changes: $_"
  }
  finally{
    # Clean up temporary file
    if (Test-Path $tempFile -PathType Leaf) {
        Remove-Item -Path $tempFile -Force
    }
  }
}

function Invoke-Instalation {

  $fileName = (Join-Path $env:SystemRoot "RunTI.exe")

  try{
    Invoke-MargeRegFile -Url "https://raw.githubusercontent.com/edelvarden/RunTI/main/remove_run_as_trustedinstaller.reg"

    if (Test-Path $fileName -PathType Leaf) {
      Remove-Item -Path $fileName -Force
    }
      
    Write-Host "Done. Successfully removed."
  }
  catch {
    Write-Warning "Failed to remove the file: $_"
  }
}

Invoke-Instalation