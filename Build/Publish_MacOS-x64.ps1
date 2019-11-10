dotnet publish ../Pootis-Bot.sln -c Release -r osx-x64 --framework netcoreapp3.0
Remove-Item -Path "..\Pootis-Bot\bin\Release\netcoreapp3.0\osx-x64\publish\Pootis-Bot.pdb"
Copy-Item "..\LICENSE.md" -Destination "..\Pootis-Bot\Bin\Release\netcoreapp3.0\osx-x64\publish\license.txt"
Compress-Archive -Path "..\Pootis-Bot\bin\Release\netcoreapp3.0\osx-x64\publish\*" -DestinationPath "..\Pootis-Bot\bin\Release\netcoreapp3.0\osx-x64.zip" -CompressionLevel Optimal -Force
pause