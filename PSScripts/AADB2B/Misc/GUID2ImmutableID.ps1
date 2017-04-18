#------------------------------------------------------------------------------   
#   
# Copyright © 2012 Microsoft Corporation.  All rights reserved.   
#   
# This Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment.   
# THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED,  
# INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.   
# We grant You a nonexclusive, royalty-free right to use and modify the Sample Code and to reproduce and distribute the object code  
# form of the Sample Code, provided that You agree: (i) to not use Our name, logo, or trademarks to market Your software product in  
# which the Sample Code is embedded; (ii) to include a valid copyright notice on Your software product in which the Sample Code is  
# embedded; and (iii) to indemnify, hold harmless, and defend Us and Our suppliers from and against any claims or lawsuits,  
# including attorneys’ fees, that arise or result from the use or distribution of the Sample Code. 
#   
#------------------------------------------------------------------------------   
#   
# PowerShell Source Code   
#   
# NAME:   
#    GUID2ImmutableID.ps1   
#   
# VERSION:   
#    1.0   
# Author: Steve Halligan 
#   
#------------------------------------------------------------------------------   
function isGUID ($data) { 
    try { 
        $guid = [GUID]$data 
        return 1 
    } catch { 
        #$notguid = 1 
        return 0 
    } 
} 
function isBase64 ($data) { 
    try { 
        $decodedII = [system.convert]::frombase64string($data) 
        return 1 
    } catch { 
        return 0 
    } 
} 
function displayhelp  { 
    write-host "Please Supply the value you want converted" 
    write-host "Examples:" 
    write-host "To convert a GUID to an Immutable ID: GUID2ImmutableID.ps1 '748b2d72-706b-42f8-8b25-82fd8733860f'" 
    write-host "To convert an ImmutableID to a GUID: GUID2ImmutableID.ps1 'ci2LdGtw+EKLJYL9hzOGDw=='" 
    } 
 
function Guid2ImmutableID
{
    param([string]$valuetoconvert) 

    if ($valuetoconvert -eq $NULL) { 
        DisplayHelp 
        return $null
    } 
    if (isGUID($valuetoconvert)) 
    { 
        $guid = [GUID]$valuetoconvert 
        $bytearray = $guid.tobytearray() 
        $immutableID = [system.convert]::ToBase64String($bytearray) 
        #write-host "ImmutableID" 
        #write-host "-----------" 
        return $immutableID 
    } elseif (isBase64($valuetoconvert)){ 
        $decodedII = [system.convert]::frombase64string($valuetoconvert) 
        if (isGUID($decodedII)) { 
            $decode = [GUID]$decodedii 
            $decode 
        } else { 
            Write-Host "Value provided not in GUID or ImmutableID format." 
            DisplayHelp 
            return $null
        } 
    } else { 
        Write-Host "Value provided not in GUID or ImmutableID format." 
        DisplayHelp 
        return $null
    } 
}