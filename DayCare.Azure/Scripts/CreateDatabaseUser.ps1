param(
    [Parameter(Mandatory=$true)]
    [string] $ServerName,

    [Parameter(Mandatory=$true)]
    [string] $DatabaseName,

    [Parameter(Mandatory=$true)]
    [string] $ManagedIdentityName
)

Install-Module -Name SqlServer -Scope CurrentUser -Force -AllowClobber


# Use Managed Identity for AAD Authentication
# Ensure that the Managed Identity of this script has the necessary permissions
$AccessToken = (Get-AzAccessToken -ResourceUrl 'https://database.windows.net/').Token

# Create the database user for the managed identity
$Command = "CREATE USER [$ManagedIdentityName] FROM EXTERNAL PROVIDER;"

Invoke-SqlCmd -AccessToken $AccessToken -ServerInstance $ServerName -Database $DatabaseName -Query $Command
