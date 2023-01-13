﻿# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param(
    # Resource group name of the IoT dps.
    [Parameter(Mandatory=$true)]
    [string] $resourceGroup,

    # IoT dps name used for running the sample.
    [Parameter(Mandatory=$true)]
    [string] $dpsName,

    # IoT hub name used for running the sample.
    [Parameter(Mandatory=$true)]
    [string] $iothubName,

    # Device Id created to use in sample.
    [Parameter(Mandatory=$true)]
    [string] $deviceId,
    
    [Parameter(Mandatory=$false)]
    [string] $groupEnrollmentId = "x509GroupEnrollment"
)

# Check if the resource group exists. If not, exit.
$resourceGroupExists = az group exists -n $resourceGroup
if ($resourceGroupExists -ne $true)
{
    Write-Host "Resource Group '$resourceGroup' does not exist. Exiting..."
    exit
}

# Check if the IoT hub instance exists. If not, exit.
$iothubExists = az iot hub show --name $iothubName -g $resourceGroup 2> $NULL
if ($iothubExists)
{
    # Check if the device exists. If it does, delete the device.
    $deviceExists = az iot hub device-identity show --device-id $deviceId -g $resourceGroup --hub-name $iothubName 2> $NULL
    if ($deviceExists)
    {
        Write-Host "Deleting device '$deviceId' in '$iothubName'..."
        az iot hub device-identity delete --device-id $deviceId --hub-name $iothubName -g $resourceGroup 2> $NULL
        Write-Host "Device '$deviceId' deleted in '$iothubName'."
    }
    else
    {
        Write-Host "Device '$deviceId' does not exist under '$iothubName'."
    }
}
else
{
    Write-Host "IoThub '$iothubName' does not exist under '$resourceGroup'."
}

# Check if the DPS instance exists. If it does, delete the device.
$dpsExists = az iot dps show --name $dpsName -g $resourceGroup 2> $NULL
if ($dpsExists)
{
    # Check if the enrollment group exists in dps instance.
    $groupEnrollmentExists = az iot dps enrollment-group show --dps-name $dpsName -g $resourceGroup --enrollment-id $groupEnrollmentId 2> $NULL
    if ($groupEnrollmentExists)
    {
        Write-Host "Deleting enrollment group '$groupEnrollmentId' in '$dpsName'..."
        az iot dps enrollment-group delete -g $resourceGroup --eid $groupEnrollmentId --dps-name $dpsName 2> $NULL
        Write-Host "Enrollment group '$groupEnrollmentId' is deleted in '$dpsName'."
    }
    else
    {
        Write-Host "$groupEnrollmentId enrollment group does not exist in $dpsName."
    }
}
else
{
    Write-Host "DPS '$dpsName' does not exist under '$resourceGroup'. Unabled to delete '$groupEnrollmentId'."
}