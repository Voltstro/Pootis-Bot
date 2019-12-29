dotnet publish ../Pootis-Bot.sln -c Release -r win-x64 --framework netcoreapp3.0
Remove-Item -Path "..\src\Pootis-Bot\bin\Release\netcoreapp3.0\win-x64\publish\Pootis-Bot.pdb"
Copy-Item "..\LICENSE.md" -Destination "..\src\Pootis-Bot\Bin\Release\netcoreapp3.0\win-x64\publish\license.txt"
Compress-Archive -Path "..\src\Pootis-Bot\bin\Release\netcoreapp3.0\win-x64\publish\*" -DestinationPath "..\src\Pootis-Bot\bin\Release\pootis-bot-win-x64.zip" -CompressionLevel Optimal -Force
pause