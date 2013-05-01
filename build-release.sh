#!/bin/sh
$WINDIR/Microsoft.NET/Framework/v4.0.30319/MSBuild.exe AtomEventStore.AzureBlob.sln /property:Configuration=Release
nuget pack AtomEventStore.nuspec -prop Configuration=Release
nuget pack AtomEventStore.AzureBlob.nuspec -prop Configuration=Release