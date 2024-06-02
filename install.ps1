function Get-FileDownloadUrlFromGithubReleases {
    
  [CmdletBinding()]
  param(
    [Parameter(Mandatory = $true)]
    [string]$ReleasesUrl,
    [Parameter(Mandatory = $true)]
    [string]$FileName
  )

  try {
    # Get download url from github realeses
    $source = (Invoke-RestMethod -Uri $ReleasesUrl -Method Get -ErrorAction Stop)

    return ($source[0].assets | Where-Object name -Match $FileName)[0].browser_download_url
  }
  catch {}

  return ""
}

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

    # Clean up temporary file


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
  $sourceUrl = "https://api.github.com/repos/edelvarden/RunTI/releases/latest"
  $fileName = "RunTI.exe"
  $downloadUrl = Get-FileDownloadUrlFromGithubReleases -ReleasesUrl $sourceUrl -FileName $fileName

  try {
    (New-Object System.Net.WebClient).DownloadFile($downloadUrl, (Join-Path $env:SystemRoot $fileName))
    Write-Host "Done. Successfully downloaded."

    Invoke-MargeRegFile -Url "https://raw.githubusercontent.com/edelvarden/RunTI/main/run_as_trustedinstaller.reg"
    Invoke-MargeRegFile -Url "https://raw.githubusercontent.com/edelvarden/RunTI/main/toggle_network.reg"
  }
  catch {
    Write-Warning "Failed to download the file: $_"
  }
}

Invoke-Instalation