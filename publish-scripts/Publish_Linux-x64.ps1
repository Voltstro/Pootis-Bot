dotnet publish ../Pootis-Bot.sln -c Release -r linux-x64 --framework netcoreapp3.1 --self-contained false
Copy-Item "..\LICENSE.md" -Destination "..\src\Pootis-Bot\Bin\Release\netcoreapp3.1\linux-x64\publish\license.txt"
Compress-Archive -Path "..\src\Pootis-Bot\bin\Release\netcoreapp3.1\linux-x64\publish\*" -DestinationPath "..\src\Pootis-Bot\bin\Release\pootis-bot-linux-x64.zip" -CompressionLevel Optimal -Force
pause