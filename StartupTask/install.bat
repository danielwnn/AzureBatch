SET

%AZ_BATCH_APP_PACKAGE_VisualCPlus_Dist%\vc_redist.x64.exe /install /passive /norestart
echo "Installed C++"

%AZ_BATCH_APP_PACKAGE_python%\python-3.7.9-amd64.exe /quiet Include_launcher=0 InstallAllUsers=1 PrependPath=1 Include_pip=1 Include_lib=1 Include_tcltk=1 Include_tools=1
echo "python installed."

if exist "C:\tmp" (rmdir /Q /S C:\tmp) else (mkdir C:\tmp)
move %AZ_BATCH_APP_PACKAGE_CXR%\CXR C:\tmp\

rmdir /Q /S C:\Code
mkdir C:\Code
move %AZ_BATCH_APP_PACKAGE_PythonML%\PythonML C:\Code

cd C:\Code\PythonML

python -m venv env
.\env\scripts\activate.bat

pip install -r requirements.txt

python tube_locate.py .\sample_case.csv .\cxr.pk --model .\cxr_model.3\cxr_model --ga_conf .\ga_conf\conf_ga_cxr.yml --no_gene --parallel 'local'


