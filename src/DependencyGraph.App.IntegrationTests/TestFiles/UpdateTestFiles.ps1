..\bin\Debug\net8.0\dependency-graph.exe ..\..\DependencyGraph.App\DependencyGraph.App.csproj -v dgml -o DependencyGraph.App.csproj.dgml --no-restore

..\bin\Debug\net8.0\dependency-graph.exe ..\..\DependencyGraph.App\DependencyGraph.App.csproj -v dgml -o DependencyGraph.App.csproj_Exclude.dgml -e Microsoft.Extensions* --no-restore

..\bin\Debug\net8.0\dependency-graph.exe ..\..\DependencyGraph.App\DependencyGraph.App.csproj -v dgml -o DependencyGraph.App.csproj_Include.dgml -i DependencyGraph* *NuGet* --no-restore

..\bin\Debug\net8.0\dependency-graph.exe ..\..\DependencyGraph.App\DependencyGraph.App.csproj -v dgml -o DependencyGraph.App.csproj_MaxDepth.dgml -d 2 --no-restore

..\bin\Debug\net8.0\dependency-graph.exe ..\..\DependencyGraph.App\DependencyGraph.App.csproj -v console --no-restore > DependencyGraph.App.csproj.txt

..\bin\Debug\net8.0\dependency-graph.exe ..\..\DependencyGraph.App\DependencyGraph.App.csproj -v console -e Microsoft.Extensions* --no-restore > DependencyGraph.App.csproj_Exclude.txt 

..\bin\Debug\net8.0\dependency-graph.exe ..\..\DependencyGraph.App\DependencyGraph.App.csproj -v console -i DependencyGraph* *NuGet* --no-restore > DependencyGraph.App.csproj_Include.txt

..\bin\Debug\net8.0\dependency-graph.exe ..\..\DependencyGraph.App\DependencyGraph.App.csproj -v console -d 2 --no-restore > DependencyGraph.App.csproj_MaxDepth.txt