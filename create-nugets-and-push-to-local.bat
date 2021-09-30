set local_feed=C:\Local_Nuget_Feed

:: Register nuget feed
if not exist %local_feed% dotnet nuget add source %local_feed% -n %local_feed%

:: Create folder for feed
if not exist %local_feed% mkdir %local_feed%

:: Pack all projects in the folder
dotnet pack --output %local_feed% --no-build

::  dotnet pack --output nupkgs
:: Find all nuget packages and push to local nuget
::  for /r %%v in (*.nupkg) do (
	::  del "%local_feed%\%%~nxv"
	::  dotnet nuget push "%%v" -s %local_feed%
::  )

rem To install updated same version you need to clear VS .nuget cache and reinstall package to the solution

pause