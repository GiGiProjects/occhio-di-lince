@echo off
rem Compila Occhio.exe in locale con il compilatore C# incluso in Windows (stesso comando usato da GitHub).
cd /d "%~dp0"
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist dist mkdir dist
"%CSC%" /nologo /target:exe /platform:anycpu /optimize+ /win32manifest:src\app.manifest /win32icon:assets\occhio.ico /r:System.Web.Extensions.dll /out:dist\Occhio.exe src\Agent.cs src\AgentConfig.cs
if errorlevel 1 (echo Compilazione FALLITA & exit /b 1)
echo Creato dist\Occhio.exe
