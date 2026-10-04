param([string]$Build='Foundation-Normal')
$project=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$exe=Join-Path $project "Builds/$Build/Keshiya.exe"
if(-not(Test-Path -LiteralPath $exe)){throw 'Build the Windows player first'}
$results=@()
foreach($flag in @('external01','ux','external','smoke','tool','role','economy','zigzag','work','skills','shop','balance','specialist','world')){
 $log=Join-Path $project "TestResults-Foundation/runtime-$flag.log"
 $p=Start-Process -FilePath $exe -ArgumentList "--$flag-test -screen-fullscreen 0 -screen-width 1280 -screen-height 800 -logFile `"$log`"" -WindowStyle Normal -PassThru
 Write-Output "START $flag"
 if(-not $p.WaitForExit(300000)){Stop-Process -Id $p.Id;throw "Timeout: $flag"}
 $p.Refresh();$results += [pscustomobject]@{suite=$flag;exit=$p.ExitCode}
 $results|ConvertTo-Json|Set-Content -Encoding utf8 (Join-Path $project 'TestResults-Foundation/runtime-processes.json')
 if($p.ExitCode-ne 0){throw "Failed: $flag"}
 Write-Output "PASS $flag"
}
