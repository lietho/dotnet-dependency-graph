dotnet build ..\DependencyGraph.App.IntegrationTests.csproj -c Debug

# project
..\bin\Debug\dependency-graph.exe print TestSolution\WebApplication\WebApplication.csproj -v dgml -o WebApplication.csproj.dgml --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\WebApplication\WebApplication.csproj -v dgml -o WebApplication.csproj_Exclude.dgml -e Microsoft.Extensions* --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\WebApplication\WebApplication.csproj -v dgml -o WebApplication.csproj_Include.dgml -i ClassLibrary* *Mapper* --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\WebApplication\WebApplication.csproj -v dgml -o WebApplication.csproj_MaxDepth.dgml -d 2 --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\WebApplication\WebApplication.csproj -v console --no-restore > WebApplication.csproj.txt
..\bin\Debug\dependency-graph.exe print TestSolution\WebApplication\WebApplication.csproj -v console -e Microsoft.Extensions* --no-restore > WebApplication.csproj_Exclude.txt 
..\bin\Debug\dependency-graph.exe print TestSolution\WebApplication\WebApplication.csproj -v console -i ClassLibrary* *Mapper* --no-restore > WebApplication.csproj_Include.txt
..\bin\Debug\dependency-graph.exe print TestSolution\WebApplication\WebApplication.csproj -v console -d 2 --no-restore > WebApplication.csproj_MaxDepth.txt

# solution
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.sln -v dgml -o TestSolution.sln.dgml --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.sln -v dgml -o TestSolution.sln_Exclude.dgml -e Microsoft.Extensions* --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.sln -v dgml -o TestSolution.sln_Include.dgml -i ClassLibrary* *Mapper* --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.sln -v dgml -o TestSolution.sln_MaxDepth.dgml -d 2 --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.sln -v console --no-restore > TestSolution.sln.txt
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.sln -v console -e Microsoft.Extensions* --no-restore > TestSolution.sln_Exclude.txt 
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.sln -v console -i ClassLibrary* *Mapper* --no-restore > TestSolution.sln_Include.txt
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.sln -v console -d 2 --no-restore > TestSolution.sln_MaxDepth.txt