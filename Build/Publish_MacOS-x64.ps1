dotnet publish ../Pootis-Bot.sln -c Release -r osx-x64
Remove-Item -Path "..\Pootis-Bot\bin\Release\netcoreapp2.2\osx-x64\publish\Pootis-Bot.pdb"
Copy-Item "..\LICENSE.md" -Destination "..\Pootis-Bot\Bin\Release\netcoreapp2.2\osx-x64\publish\license.txt"
Compress-Archive -Path "..\Pootis-Bot\bin\Release\netcoreapp2.2\osx-x64\publish\*" -DestinationPath "..\Pootis-Bot\bin\Release\netcoreapp2.2\osx-x64.zip" -CompressionLevel Optimal -Force
pause