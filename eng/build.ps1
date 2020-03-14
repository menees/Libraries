param(
    [bool] $build = $true,
    [bool] $test = $false,
    [string[]] $configurations = @('Debug', 'Release'),
	[bool] $publish = $false,
    [string] $msBuildVerbosity = 'minimal',
    [string] $nugetApiKey = $null
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptPath = [IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Definition)
$repoPath = Resolve-Path (Join-Path $scriptPath '..')

if ($build)
{
    foreach ($configuration in $configurations)
    {
        # Restore NuGet packages first
        msbuild "$repoPath\Libraries.sln" /p:Configuration=$configuration /t:Restore /v:$msBuildVerbosity
        msbuild "$repoPath\Libraries.sln" /p:Configuration=$configuration /v:$msBuildVerbosity
    }
}

if ($test)
{
    # Using "dotnet test Libraries.sln" gets errors about .NET Core not supporting ResolveComReference.
    # So we have to directly run the vstest app against the DLLs we built above.
    foreach ($configuration in $configurations)
    {
        $testDlls = @(Get-ChildItem -r "$repoPath\tests\**\*.Tests.dll" | Where-Object {$_.Directory -like "*\bin\$configuration\*"})
        foreach ($testDll in $testDlls)
        {
            write-host "`n`n***** $testDll *****"
            vstest.console.exe $testDll /Platform:X64
        }
    }
}

if ($publish)
{
    $versionFile = "$repoPath\src\Directory.Build.props"
    $version = Get-Content $versionFile |`
        Where-Object {$_ -like '*<Version>*</Version>*'} |`
        ForEach-Object {$_.Replace('<Version>', '').Replace('</Version>', '').Trim()}

    $published = $false
    if ($version)
    {
        $artifactsPath = "$repoPath\artifacts"
        if (!(Test-Path $artifactsPath))
        {
            $ignore = mkdir $artifactsPath
            if ($ignore) { } # For PSUseDeclaredVarsMoreThanAssignments
        }

        foreach ($configuration in $configurations)
        {
            if ($configuration -like '*Release*')
            {
                Write-Host "Publishing version $version $configuration packages to $artifactsPath"
                $packages = @(Get-ChildItem -r "$repoPath\src\**\*.$version.nupkg" | Where-Object {$_.Directory -like "*\bin\$configuration"})
                foreach ($package in $packages)
                {
                    Write-Host "Copying $package"
                    Copy-Item -Path $package -Destination $artifactsPath -Force

                    if ($nugetApiKey)
                    {
                        $artifactPackage = Join-Path $artifactsPath (Split-Path -Leaf $package)
                        dotnet nuget push $artifactPackage -k $nugetApiKey -s https://api.nuget.org/v3/index.json --skip-duplicate
                        $published = $true
                    }
                }
            }
        }
    }

    if ($published)
    {
        Write-Host "`n`n****** REMEMBER TO INCREMENT THE VERSION ELEMENTS IN $versionFile AFTER TAGGING! ******"
    }
}
