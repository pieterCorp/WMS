param(
    [string]$RemoteUrl
)

# Simple helper to add an origin remote and push current branch
# Usage:
#   .\push-to-remote.ps1 -RemoteUrl "https://github.com/you/repo.git"
# or just run and follow prompt

# Ensure we're in a git repo
try {
    $root = git rev-parse --show-toplevel 2>$null
} catch {
    Write-Error "Not a git repository (git rev-parse failed). Run this from the repo root."
    exit 1
}

if (-not $RemoteUrl) {
    $RemoteUrl = Read-Host "Enter remote Git URL (e.g. https://github.com/username/repo.git)"
}

if (-not $RemoteUrl) {
    Write-Error "No remote URL provided. Exiting."
    exit 1
}

# Check for existing origin
$origin = $null
try {
    $origin = git remote get-url origin 2>$null
} catch { }

if ($origin) {
    Write-Host "Remote 'origin' currently set to: $origin"
    $answer = Read-Host "Replace origin with $RemoteUrl? (y/N)"
    if ($answer -ne 'y' -and $answer -ne 'Y') {
        Write-Host "Aborting. No changes made to remotes."
        exit 0
    }
    git remote set-url origin $RemoteUrl
    if ($LASTEXITCODE -ne 0) { Write-Error "Failed to set origin URL."; exit 1 }
} else {
    git remote add origin $RemoteUrl
    if ($LASTEXITCODE -ne 0) { Write-Error "Failed to add origin remote."; exit 1 }
}

# Push current branch
$branch = git rev-parse --abbrev-ref HEAD
Write-Host "Pushing branch $branch to origin..."

git push -u origin $branch
if ($LASTEXITCODE -ne 0) {
    Write-Error "git push failed. Check authentication and remote URL."
    exit 1
}

Write-Host "Push succeeded. Remote origin set to: $RemoteUrl"