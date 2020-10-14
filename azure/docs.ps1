# Install mkdocs
pip install mkdocs
pip install mkdocs-minify-plugin
pip install mkdocs-material

# Clone repo
Write-Output "Cloning repo..."
git clone -b gh-pages https://github.com/Voltstro/Pootis-Bot.git

# Remove old files
Write-Output "Removing old files..."
Get-ChildItem -Path  'Pootis-Bot\' -Recurse |
Select-Object -ExpandProperty FullName |
Where-Object {$_ -notlike 'Pootis-Bot\.git\*'} |
Sort-Object length -Descending |
Remove-Item -force

# Build docs
Write-Output "Building docs..."
mkdocs build -f docs/mkdocs.yml -d ../Pootis-Bot/

# Push the docs onto to gh-pages branch
Write-Output "Push Docs"
Set-Location -path "Pootis-Bot/"
git config --global user.name "Azure Pipelines"
git config --global user.email "azuredevops@microsoft.com"
git remote set-url origin git@github.com:voltstro/Pootis-Bot.git
git add *
git commit -m "Deploy Docs  ***NO_CI***"
git push