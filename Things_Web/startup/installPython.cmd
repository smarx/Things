cd %~dp0
for /f %%p in ('GetLocalPath.exe Python') do set PYTHONPATH=%%p

msiexec /i python-2.7.1.msi /qn TARGETDIR="%PYTHONPATH%" /log installPython.log

%PYTHONPATH%\python -c "import sys, os; sys.path.insert(0, os.path.abspath('setuptools-0.6c11-py2.7.egg')); from setuptools.command.easy_install import bootstrap; sys.exit(bootstrap())"
%PYTHONPATH%\scripts\easy_install Pygments-1.4-py2.7.egg

echo y| cacls %PYTHONPATH% /t /grant everyone:f

exit /b 0