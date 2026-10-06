param([string]$Build='Foundation-Normal',[string]$From='external01',[switch]$VisibleForVisualChecks)
$project=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$exe=Join-Path $project "Builds/$Build/Keshiya.exe"
if(-not(Test-Path -LiteralPath $exe)){throw 'Build the Windows player first'}
$suites=@('external01','ux','external','smoke','tool','role','economy','zigzag','work','skills','shop','balance','specialist','world')
$start=[Array]::IndexOf($suites,$From)
if($start -lt 0){throw 'Unknown starting suite'}
$windowStyle=if($VisibleForVisualChecks){'Normal'}else{'Hidden'}
$results=@()
if($start -gt 0){
 $previous=Get-Content -Raw (Join-Path $project 'TestResults-Foundation/runtime-processes.json')|ConvertFrom-Json
 foreach($name in $suites[0..($start-1)]){
  $entry=@($previous|Where-Object {$_.suite -eq $name -and $_.exit -eq 0})
  if($entry.Count -ne 1){throw "Missing passing prerequisite: $name"}
  $results+=$entry[0]
 }
}
foreach($flag in $suites[$start..($suites.Length-1)]){
 $log=Join-Path $project "TestResults-Foundation/runtime-$flag.log"
 $p=Start-Process -FilePath $exe -ArgumentList "--$flag-test -screen-fullscreen 0 -screen-width 1280 -screen-height 800 -logFile `"$log`"" -WindowStyle $windowStyle -PassThru
 Write-Output "START $flag"
 if(-not $p.WaitForExit(300000)){Stop-Process -Id $p.Id;throw "Timeout: $flag"}
 $p.Refresh();$results += [pscustomobject]@{suite=$flag;exit=$p.ExitCode}
 $results|ConvertTo-Json|Set-Content -Encoding utf8 (Join-Path $project 'TestResults-Foundation/runtime-processes.json')
 if($p.ExitCode-ne 0){throw "Failed: $flag"}
 Write-Output "PASS $flag"
}
