@echo off

set platform=Win32
set configuration=Release
set version=

set nuget=%~dp0nuget
set nupkg=%~dp0ht4n

for %%p in (%*) do (
	if /i "%%~p" == "x86" (
		set platform=Win32
	) else (
		if /i "%%~p" == "win32" (
			set platform=Win32
		) else (
			if /i "%%~p" == "x64" (
				set platform=x64
			) else (
				if /i "%%~p" == "win64" (
					set platform=x64
				) else (
					if /i "%%~p" == "release" (
						set configuration=Release
					) else (
						if /i "%%~p" == "debug" (
							set configuration=Debug
						) else (
							set version=%%p
						)
					)
				)
			)
		)
	)
)

%nuget% update -self

set target=
if /i "%platform%"=="Win32" (
	set target=x86
) else (
	set target=x64
)

if not exist %nupkg%.%target%.%version%.nupkg goto :missing_nupkg
%nuget% push %nupkg%.%target%.%version%.nupkg

goto :done

:missing_nupkg
echo %nupkg%.%target%.%version%.nupkg does not exists
goto done:

:done

