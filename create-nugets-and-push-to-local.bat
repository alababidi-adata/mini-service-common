set local_feed=C:\Local_Nuget_Feed

:: Prepare timestamp for local packages
@echo off
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YY=%dt:~2,2%" & set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
set "HH=%dt:~8,2%" & set "Min=%dt:~10,2%" & set "Sec=%dt:~12,2%"
set "fullstamp=%YYYY%-%MM%-%DD%--%HH%-%Min%-%Sec%"
@echo on

:: Register nuget feed
if not exist %local_feed% dotnet nuget add source %local_feed% -n %local_feed%

:: Create folder for feed
if not exist %local_feed% mkdir %local_feed%

:: Pack all projects in the folder
dotnet pack --output %local_feed% --version-suffix local-%fullstamp% 
::--no-restore

::  dotnet pack --output nupkgs
:: Find all nuget packages and push to local nuget
::  for /r %%v in (*.nupkg) do (
	::  del "%local_feed%\%%~nxv"
	::  dotnet nuget push "%%v" -s %local_feed%
::  )

rem To install updated same version you need to clear VS .nuget cache and reinstall package to the solution

pause