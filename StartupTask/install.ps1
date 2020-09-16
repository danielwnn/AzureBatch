Write-Output $(Get-ChildItem env:)

$vc_dist = $env:AZ_BATCH_APP_PACKAGE_VisualCPlus_Dist
Start-Process -FilePath "$vc_dist\vc_redist.x64.exe" -ArgumentList "/install /passive /norestart" -NoNewWindow -Wait
Write-Output "VC++ Redist installed."

$python = $env:AZ_BATCH_APP_PACKAGE_python
Start-Process -FilePath "$python\python-3.7.9-amd64.exe" -ArgumentList "/quiet Include_launcher=0 InstallAllUsers=1 PrependPath=1 Include_pip=1 Include_lib=1 Include_tcltk=1 Include_tools=1" -NoNewWindow -Wait
Write-Output "python installed."

# create C:\tmp directory, the C++ has hard-coded directory path somewhere, so it needs the test data in C:\tmp folder
if (Test-Path 'C:\tmp' -PathType Any) {
    Remove-Item 'C:\tmp' -Recurse -Force
}
New-Item -Path 'C:\tmp' -ItemType Directory
$cxr = $env:AZ_BATCH_APP_PACKAGE_CXR
Copy-Item -Path $cxr\CXR -Destination C:\tmp -Recurse -Force

if (Test-Path 'C:\PythonML' -PathType Any) {
    Remove-Item 'C:\PythonML' -Recurse -Force
}
New-Item -Path 'C:\PythonML' -ItemType Directory
$pythonML = $env:AZ_BATCH_APP_PACKAGE_PythonML
Copy-Item -Path $pythonML\PythonML -Destination C:\ -Recurse -Force

cd C:\PythonML

SET PATH=%PATH%;"C:\Program Files\Python37";"C:\Program Files\Python37\Scripts"
$env:Path = $env:path + ";C:\Program Files\Python37;C:\Program Files\Python37\Scripts"

pip install -r requirements.txt

# python tube_locate.py .\sample_case.csv .\cxr.pk --model .\cxr_model.3\cxr_model --ga_conf .\ga_conf\conf_ga_cxr.yml --no_gene --parallel 'local'
