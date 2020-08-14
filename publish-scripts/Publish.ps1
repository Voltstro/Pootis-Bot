#Parameters that we need
param(
    [Parameter(Mandatory=$True)]
    [string] $rid
)

$framework = "netcoreapp3.1"

dotnet publish ../Pootis-Bot.sln -c Release -r $rid --framework $framework --self-contained false --nologo
Copy-Item "..\LICENSE.md" -Destination ("..\src\Pootis-Bot\Bin\Release\" + $framework + "\" + $rid + "\publish\license.txt")
Compress-Archive -Path ("..\src\Pootis-Bot\Bin\Release\" + $framework + "\" + $rid + "\publish\*") -DestinationPath ("..\src\Pootis-Bot\bin\Release\pootis-bot-" + $rid + ".zip") -CompressionLevel Optimal -Force