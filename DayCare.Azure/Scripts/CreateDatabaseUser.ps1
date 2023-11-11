param(
    [Parameter(Mandatory=$true)]
    [string] $ServerName,

    [Parameter(Mandatory=$true)]
    [string] $DatabaseName,

    [Parameter(Mandatory=$true)]
    [string] $ManagedIdentityName,

    [Parameter(Mandatory=$true)]
    [string] $Roles
)

Install-Module -Name SqlServer -Scope CurrentUser -Force -AllowClobber

# Use Managed Identity for AAD Authentication
# Ensure that the Managed Identity of this script has the necessary permissions
$AccessToken = (Get-AzAccessToken -ResourceUrl 'https://database.windows.net/').Token

# Create the database user for the managed identity
$Command = @"
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'$ManagedIdentityName')
BEGIN
    EXEC('CREATE USER [$ManagedIdentityName] FROM EXTERNAL PROVIDER;');
END
"@
Invoke-SqlCmd -AccessToken $AccessToken -ServerInstance $ServerName -Database $DatabaseName -Query $Command

# Add the managed identity to the specified roles
foreach ($Role in $Roles.Split(",")) {
    $Command = @"
IF NOT EXISTS (SELECT 1 FROM sys.database_role_members AS drm
               JOIN sys.database_principals AS dp ON drm.role_principal_id = dp.principal_id
               JOIN sys.database_principals AS dp2 ON drm.member_principal_id = dp2.principal_id
               WHERE dp.name = N'$Role' AND dp2.name = N'$ManagedIdentityName')
BEGIN
    EXEC('ALTER ROLE [$Role] ADD MEMBER [$ManagedIdentityName];');
END
"@
    Invoke-SqlCmd -AccessToken $AccessToken -ServerInstance $ServerName -Database $DatabaseName -Query $Command
}
