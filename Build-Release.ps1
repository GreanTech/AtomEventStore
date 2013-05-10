.$env:windir\Microsoft.NET\Framework\v4.0.30319\MSBuild BuildRelease.msbuild /property:Configuration=Release
nuget pack AtomEventStore.nuspec -prop Configuration=Release
nuget pack AtomEventStore.AzureBlob.nuspec -prop Configuration=Release