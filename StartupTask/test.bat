SET PATH=%PATH%;C:\Program Files\Python37;C:\Program Files\Python37\Scripts

if exist "C:\tmp\CXR\test" (rmdir /Q /S "C:\tmp\CXR\test")

C:

cd \PythonML

python blob.py %1

python tube_locate.py .\sample_case.csv .\cxr.pk --model .\cxr_model.3\cxr_model --ga_conf .\ga_conf\conf_ga_cxr.yml --no_gene --parallel 'local'

exit 0