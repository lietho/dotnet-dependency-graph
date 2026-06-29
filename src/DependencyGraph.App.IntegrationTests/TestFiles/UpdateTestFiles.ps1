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

# solution (slnx)
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.slnx -v dgml -o TestSolution.slnx.dgml --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.slnx -v dgml -o TestSolution.slnx_Exclude.dgml -e Microsoft.Extensions* --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.slnx -v dgml -o TestSolution.slnx_Include.dgml -i ClassLibrary* *Mapper* --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.slnx -v dgml -o TestSolution.slnx_MaxDepth.dgml -d 2 --no-restore
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.slnx -v console --no-restore > TestSolution.slnx.txt
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.slnx -v console -e Microsoft.Extensions* --no-restore > TestSolution.slnx_Exclude.txt
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.slnx -v console -i ClassLibrary* *Mapper* --no-restore > TestSolution.slnx_Include.txt
..\bin\Debug\dependency-graph.exe print TestSolution\TestSolution.slnx -v console -d 2 --no-restore > TestSolution.slnx_MaxDepth.txt


# framework-provided dependencies (.NET package pruning)
..\bin\Debug\dependency-graph.exe print FrameworkProvided\FrameworkProvided.csproj -v dgml -o FrameworkProvided.csproj.dgml --no-restore
..\bin\Debug\dependency-graph.exe print FrameworkProvided\FrameworkProvided.csproj -v dgml -o FrameworkProvided.csproj_ExcludeFrameworkProvided.dgml --exclude-framework-provided --no-restore
..\bin\Debug\dependency-graph.exe print FrameworkProvided\FrameworkProvided.csproj -v console --no-restore > FrameworkProvided.csproj.txt
..\bin\Debug\dependency-graph.exe print FrameworkProvided\FrameworkProvided.csproj -v console --exclude-framework-provided --no-restore > FrameworkProvided.csproj_ExcludeFrameworkProvided.txt
# trace
..\bin\Debug\dependency-graph.exe trace TestSolution\WebApplication\WebApplication.csproj System.Runtime.CompilerServices* --no-restore > WebApplication.csproj_Trace_Pattern.txt
..\bin\Debug\dependency-graph.exe trace TestSolution\ClassLibrary\ClassLibrary.csproj AutoMapper --no-restore > ClassLibrary.csproj_Trace.txt
..\bin\Debug\dependency-graph.exe trace TestSolution\WebApplication\WebApplication.csproj AutoMapper -v 12.0 --no-restore > WebApplication.csproj_Trace_MinVersion.txt

# trace (solution)
..\bin\Debug\dependency-graph.exe trace TestSolution\TestSolution.sln AutoMapper --no-restore > TestSolution.sln_Trace.txt
..\bin\Debug\dependency-graph.exe trace TestSolution\TestSolution.slnx AutoMapper --no-restore > TestSolution.slnx_Trace.txt
..\bin\Debug\dependency-graph.exe trace FrameworkProvided\FrameworkProvided.csproj System.Configuration.ConfigurationManager --no-restore > FrameworkProvided.csproj_Trace.txt
