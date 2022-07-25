$existingLicenses = Get-ChildItem -Path 'Licenses'
If ($existingLicenses.Count -gt 0)
{
	Write-Host "Backing up existing license files" -ForegroundColor Cyan
	$date = Get-Date -Format "yyyy-MM-ddTHH-mm-ss"
	$oldFolder = New-Item -Path "Old Licenses\$date" -ItemType Directory
	$existingLicenses | Move-Item -Destination $oldFolder
}

# Codehouse developers
Write-Host "Generating Codehouse license" -ForegroundColor Cyan
.\LicenseGenerator.exe `
	--Licensee "Codehouse ltd." `
	--Entitlement "Unlimited developers" `
	--Lifetime "P3M" `
	--Products "Codehouse.ModelGenerator" `
	--Key "Codehouse.ModelGenerator 2022" `
	--Output "Licenses\CH license.dat"

# AzDO license
Write-Host "Generating AzDO license" -ForegroundColor Cyan
.\LicenseGenerator.exe `
	--Licensee "Codehouse ltd." `
	--Entitlement "Build use only" `
	--Lifetime "P3M" `
	--Products "Codehouse.ModelGenerator" `
	--Key "Codehouse.ModelGenerator 2022" `
	--Output "Licenses\AzDO license.dat"

# APMT license
Write-Host "Generating APMT license" -ForegroundColor Cyan
.\LicenseGenerator.exe `
	--Licensee "APM Terminals" `
	--Entitlement "5 developers" `
	--Lifetime "P3M" `
	--Products "Codehouse.ModelGenerator" `
	--Key "Codehouse.ModelGenerator 2022" `
	--Output "Licenses\APMT license.dat"

# Deploy CH license to X:
Write-Host "Update Codehouse license on X:" -ForegroundColor Cyan
Copy-Item `
    -Path "Licenses\CH license.dat" `
	-Destination "X:\Development Tools\Codehouse Model Generator\license.dat" `
	-Force

# Update AzDO license(s) on AzDO
Write-Host "Update license on Azure DevOps" -ForegroundColor Cyan
$licenseKey = Get-Content "Licenses\APMT license.dat"

$variableName = 'modelGeneratorLicenseKey'
$variableGroups = @(
    # More variable groups can be added here
	@{Id = 334; Project = 'Apm.MainSite'}
)

$variableGroups | %{
    $id = $_.Id
	$project = $_.Project
	
	Write-Output "  Updating variable $variableName in group $id on project $project"
	& az pipelines variable-group variable update --id $id --name $variableName --project $project --secret true --value $licenseKey
	Write-Output "  Done."
}

Write-Host "APMT license must be sent out manually" -ForegroundColor Yellow
Write-Host "Finished." -ForegroundColor Cyan