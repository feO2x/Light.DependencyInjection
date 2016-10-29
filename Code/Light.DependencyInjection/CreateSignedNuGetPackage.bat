copy bin\Release\Light.DependencyInjection.dll bin\SignedRelease\ /Y
copy bin\Release\Light.DependencyInjection.xml bin\SignedRelease\ /Y

ildasm bin\SignedRelease\Light.DependencyInjection.dll /out:bin\SignedRelease\Light.DependencyInjection.il
del bin\SignedRelease\Light.DependencyInjection.dll
ilasm bin\SignedRelease\Light.DependencyInjection.il /dll /key=bin\SignedRelease\Light.DependencyInjection.snk
del bin\SignedRelease\Light.DependencyInjection.il
del bin\SignedRelease\Light.DependencyInjection.res

nuget pack Light.DependencyInjection.nuspec