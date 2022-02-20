# This script is designed to work on Docker running on Apple Silicon

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$ServerName = "local_sql_server"
$DatabaseName = "ConfigProviderExample"
$SAPassword = "MyPassword123"
$SchemaName = "Example"
$TableName = "ConfigInfo"

$module = Get-Module -Name "SqlServer"
if ($module -eq $null)
{
    Install-Module -Name SqlServer -Scope CurrentUser
}


& docker pull "mcr.microsoft.com/azure-sql-edge"

& docker run `
    -e "ACCEPT_EULA=1" `
    -e "MSSQL_SA_PASSWORD=$SAPasssword" `
    -e "MSSQL_PID=Developer" `
    -e "MSSQL_USER=SA" `
    -p 1433:1433 `
    -d `
    --name=$ServerName `
    mcr.microsoft.com/azure-sql-edge

Write-Information "Docker image $ServerName is now running."

Write-Information "Waiting for SQL Server Startup"
$pinfo = New-Object System.Diagnostics.ProcessStartInfo
$pinfo.FileName = "docker"
$pinfo.RedirectStandardError = $true
$pinfo.RedirectStandardOutput = $true
$pinfo.UseShellExecute = $false
$pinfo.Arguments = "logs --follow $ServerName"

Write-Output $pinfo;

$dockerLogProcess = New-Object System.Diagnostics.Process
$dockerLogProcess.StartInfo = $pinfo
$dockerLogProcess.Start() | Out-Null

$startTime = Get-Date;
while($true)
{
    $outputLine = $dockerLogProcess.StandardOutput.ReadLine();
    Write-Information $outputLine;
    if ($outputLine.Contains("SQL Server is now ready for client connections"))
    {
        $diff = ((Get-Date) - $startTime).TotalSeconds;
        Write-Information "Started SQL Server in $diff seconds.";
        break;
    }
}

Start-Sleep -Seconds 1

$databaseExistsQueryResult = Invoke-SqlCmd -ServerInstance localhost -Username SA -Password $SAPassword -Query "SET NOCOUNT ON; SELECT DB_ID('$DatabaseName') AS DbId"
$databaseExists = -not ($databaseExistsQueryResult.DbId -eq [System.DBNull]::Value)
if (-not $databaseExists)
{
    $databaseExistsQueryResult = Invoke-SqlCmd -ServerInstance localhost -Username SA -Password $SAPassword -Query "CREATE DATABASE [$DatabaseName]"
}

$schemaId = Invoke-SqlCmd -ServerInstance localhost -Database $DatabaseName -Username SA -Password $SAPassword -Query "SET NOCOUNT ON; SELECT * FROM sys.schemas WHERE name = '$SchemaName'"
if ($null -eq $schemaId)
{
    Invoke-SqlCmd -ServerInstance localhost -Database $DatabaseName -Username SA -Password $SAPassword -Query "CREATE SCHEMA $SchemaName"
}

$tableId = Invoke-SqlCmd -ServerInstance localhost -Database $DatabaseName -Username SA -Password $SAPassword -Query "SET NOCOUNT ON; SELECT object_id FROM sys.tables WHERE type_desc = 'USER_TABLE' and Name = '$TableName'"
if ($null -eq $tableId)
{
    Invoke-SqlCmd -ServerInstance localhost -Database $DatabaseName -Username SA -Password $SAPassword -Query "CREATE TABLE $SchemaName.$TableName(ConfigKey NVARCHAR(1024) PRIMARY KEY CLUSTERED, ConfigValue NVARCHAR(MAX))"
}


