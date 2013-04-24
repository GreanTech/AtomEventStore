$WINDIR/Microsoft.NET/Framework/v4.0.30319/MSBuild.exe AtomEventStore.sln /property:Configuration=Release
nuget pack AtomEventStore.nuspec -prop Configuration=Release