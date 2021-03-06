param(
    $Target="",
    [switch]$WinX64,
    [switch]$WinX86,
    [switch]$LinuxX64,
    [switch]$Osx,
    [switch]$Rpi,
    [switch]$DontRebuildStudio,
    [switch]$DontBuildStudio,
    [switch]$JustStudio,
    [switch]$JustNuget,
    [switch]$Debug,
    [switch]$DryRunVersionBump = $false,
    [switch]$Help)

$ErrorActionPreference = "Stop"

. '.\scripts\checkLastExitCode.ps1'
. '.\scripts\checkPrerequisites.ps1'
. '.\scripts\restore.ps1'
. '.\scripts\clean.ps1'
. '.\scripts\archive.ps1'
. '.\scripts\package.ps1'
. '.\scripts\buildProjects.ps1'
. '.\scripts\getScriptDirectory.ps1'
. '.\scripts\copyAssets.ps1'
. '.\scripts\version.ps1'
. '.\scripts\updateSourceWithBuildInfo.ps1'
. '.\scripts\nuget.ps1'
. '.\scripts\target.ps1'
. '.\scripts\help.ps1'
. '.\scripts\sign.ps1'

if ($Help) {
    Help
}

CheckPrerequisites

$PROJECT_DIR = Get-ScriptDirectory
$RELEASE_DIR = [io.path]::combine($PROJECT_DIR, "artifacts")
$OUT_DIR = [io.path]::combine($PROJECT_DIR, "artifacts")

$CLIENT_SRC_DIR = [io.path]::combine($PROJECT_DIR, "src", "Raven.Client")
$CLIENT_OUT_DIR = [io.path]::combine($PROJECT_DIR, "src", "Raven.Client", "bin", "Release")

$TESTDRIVER_SRC_DIR = [io.path]::combine($PROJECT_DIR, "src", "Raven.TestDriver")
$TESTDRIVER_OUT_DIR = [io.path]::combine($PROJECT_DIR, "src", "Raven.TestDriver", "bin", "Release")

$SERVER_SRC_DIR = [io.path]::combine($PROJECT_DIR, "src", "Raven.Server")

$SPARROW_SRC_DIR = [io.path]::combine($PROJECT_DIR, "src", "Sparrow")
$SPARROW_OUT_DIR = [io.path]::combine($PROJECT_DIR, "src", "Sparrow", "bin", "Release")

$TYPINGS_GENERATOR_SRC_DIR = [io.path]::combine($PROJECT_DIR, "tools", "TypingsGenerator")
$TYPINGS_GENERATOR_BIN_DIR = [io.path]::combine($TYPINGS_GENERATOR_SRC_DIR, "bin")

$STUDIO_SRC_DIR = [io.path]::combine($PROJECT_DIR, "src", "Raven.Studio")
$STUDIO_OUT_DIR = [io.path]::combine($PROJECT_DIR, "src", "Raven.Studio", "build")

$RVN_SRC_DIR = [io.path]::combine($PROJECT_DIR, "tools", "rvn")
$DRTOOL_SRC_DIR = [io.path]::combine($PROJECT_DIR, "tools", "Voron.Recovery")

if ([string]::IsNullOrEmpty($Target) -eq $false) {
    $Target = $Target.Split(",")
} else {
    $Target = $null

    if ($WinX64) {
        $Target = @( "win-x64" );
    }

    if ($WinX86) {
        $Target = @( "win-x86" );
    }

    if ($LinuxX64) {
        $Target = @( "linux-x64" );
    } 

    if ($Osx) {
        $Target = @( "osx" );
    }

    if ($Rpi) {
        $Target = @( "rpi" );
    }
}

$targets = GetBuildTargets $Target

if ($targets.Count -eq 0) {
    write-host "No targets specified."
    exit 0;
} else {
    Write-Host -ForegroundColor Magenta "Build targets: $($targets.Name)"
}

New-Item -Path $RELEASE_DIR -Type Directory -Force
CleanFiles $RELEASE_DIR
CleanBinDirs $TYPINGS_GENERATOR_SRC_DIR, $RVN_SRC_DIR, $DRTOOL_SRC_DIR, $SERVER_SRC_DIR, $CLIENT_SRC_DIR, $SPARROW_SRC_DIR, $TESTDRIVER_SRC_DIR

$versionObj = SetVersionInfo
$version = $versionObj.Version
$versionSuffix = $versionObj.VersionSuffix
$buildNumber = $versionObj.BuildNumber
$buildType = $versionObj.BuildType.ToLower()
Write-Host -ForegroundColor Green "Building $version"

ValidateClientDependencies $CLIENT_SRC_DIR $SPARROW_SRC_DIR
UpdateSourceWithBuildInfo $PROJECT_DIR $buildNumber $version

DownloadDependencies

if ($JustStudio -eq $False) {
    BuildSparrow $SPARROW_SRC_DIR
    BuildClient $CLIENT_SRC_DIR
    BuildTestDriver $TESTDRIVER_SRC_DIR

    CreateNugetPackage $CLIENT_SRC_DIR $RELEASE_DIR $versionSuffix
    CreateNugetPackage $TESTDRIVER_SRC_DIR $RELEASE_DIR $versionSuffix
}

if ($JustNuget) {
    exit 0
}

if (ShouldBuildStudio $STUDIO_OUT_DIR $DontRebuildStudio $DontBuildStudio) {
    BuildTypingsGenerator $TYPINGS_GENERATOR_SRC_DIR
    BuildStudio $STUDIO_SRC_DIR $version
    write-host "Studio built successfully."
} else {
    write-host "Not building studio..."
}

if ($JustStudio) {
    exit 0
}

Foreach ($spec in $targets) {
    $specOutDir = [io.path]::combine($OUT_DIR, $spec.Name)
    CleanDir $specOutDir

    BuildServer $SERVER_SRC_DIR $specOutDir $spec $Debug
    BuildTool rvn $RVN_SRC_DIR $specOutDir $spec $Debug
    BuildTool drtools $DRTOOL_SRC_DIR $specOutDir $spec $Debug
    
    $specOutDirs = @{
        "Main" = $specOutDir;
        "Client" = $CLIENT_OUT_DIR;
        "Server" = $([io.path]::combine($specOutDir, "Server"));
        "Rvn" = $([io.path]::combine($specOutDir, "rvn"));
        "Studio" = $STUDIO_OUT_DIR;
        "Sparrow" = $SPARROW_OUT_DIR;
        "Drtools" = $([io.path]::combine($specOutDir, "drtools"));
    }

    $buildOptions = @{
        "DontBuildStudio" = !!$DontBuildStudio;
    }
    
    if ($buildNumber -ne $DEV_BUILD_NUMBER -or $buildType -eq 'nightly') {
        if ($spec.IsUnix -eq $False) {
            $serverPath = [io.path]::combine($specOutDirs.Server, "Raven.Server.exe");
            $rvnPath = [io.path]::combine($specOutDirs.Rvn, "rvn.exe");
            $drtoolsPath = [io.path]::combine($specOutDirs.Drtools, "Voron.Recovery.exe");
            
            SignFile $PROJECT_DIR $serverPath
            SignFile $PROJECT_DIR $rvnPath
            SignFile $PROJECT_DIR $drtoolsPath
        }
    }

    CreateRavenPackage $PROJECT_DIR $RELEASE_DIR $specOutDirs $spec $version $buildOptions
}

write-host "Done creating packages."

if ($buildType -eq 'stable') {
    BumpVersion $PROJECT_DIR $versionObj.VersionPrefix $versionObj.BuildType $DryRunVersionBump
}
