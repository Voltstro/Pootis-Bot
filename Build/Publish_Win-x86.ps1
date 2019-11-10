dotnet publish ../Pootis-Bot.sln -c Release -r win-x86 --framework netcoreapp3.0
Remove-Item -Path "..\Pootis-Bot\bin\Release\netcoreapp3.0\win-x86\publish\Pootis-Bot.pdb"
Copy-Item "..\LICENSE.md" -Destination "..\Pootis-Bot\Bin\Release\netcoreapp3.0\win-x86\publish\license.txt"
Compress-Archive -Path "..\Pootis-Bot\bin\Release\netcoreapp3.0\win-x86\publish\*" -DestinationPath "..\Pootis-Bot\bin\Release\netcoreapp3.0\win-x86.zip" -CompressionLevel Optimal -Force
pause