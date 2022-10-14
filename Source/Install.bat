REM ################ Mod build and install script (c) Andreas Pardeike 2020 ################
REM MODIFIED BY Anthony Chenevier 2020: modified for my personal preferences for solution directory format and install location.
REM Also updated for recent changes to 1.1 & harmony usage, and took out the extra xcopys to a standalone install of rimworld 
REM (most likely for the debugging mod, might add that back if debugging hooks become viable again)
REM All credit goes to Andreas Pardeike. Original script at https://gist.github.com/pardeike/08ff826bf40ee60452f02d85e59f32ff
REM
REM Call this script from Visual Studio's Build Events post-build event command line box:
REM "$(ProjectDir)Install.bat" $(ConfigurationName) "$(ProjectDir)" "$(ProjectName)" "About Common v1.1" "LoadFolders.xml"
REM < 0 this script        >< 1 Release/Debug ><2 location of solution><3 Mod name ><4 folders to copy > <5 files to copy >
REM 
REM The project structure should look like this: 
REM Modname
REM +- .git
REM +- .vs
REM +- Source
REM |	+- packages
REM |	|	+- Lib.Harmony.2.x.x
REM |	+- .vs
REM |	+- obj
REM |	|  +- Debug
REM |	|  +- Release
REM |	+- Properties
REM |	+- Modname.csproj
REM |	+- Modname.csproj.user
REM |	+- packages.config
REM |	+- Install.bat                  <----- this script
REM |	+- Staging
REM |		+- About
REM |		|  +- About.xml
REM |		|  +- Preview.png
REM |		|  +- PublishedFileId.txt
REM |		+- Assemblies                      <----- this is for RW1.0 + Harmony 1
REM |		|  +- 0Harmony.dll
REM |		|  +- 0Harmony.dll.mbd
REM |		|  +- 0Harmony.pdb
REM |		|  +- Modname.dll
REM |		|  +- Modname.dll.mbd
REM |		|  +- Modname.pdb
REM |		+- Languages
REM |		|  +- English
REM |		|  		+- Modname.pdb
REM |		+- Textures
REM |		+- v1.1
REM |		|  +- Assemblies                   <----- this is for RW1.1 + Harmony 2
REM |		|     +- 0Harmony.dll
REM |		|     +- 0Harmony.dll.mbd
REM |		|     +- 0Harmony.pdb
REM |		|     +- Modname.dll
REM |		|     +- Modname.dll.mbd
REM |		|     +- Modname.pdb
REM |		+- LoadFolders.xml
REM +- .gitattributes
REM +- .gitignore
REM +- LICENSE
REM +- README.md
REM +- Modname.sln
REM
REM Also needed are the following environment variables in the system settings (example values):
REM
REM RIMWORLD_DIR_STEAM = C:\Program Files (x86)\Steam\steamapps\common\RimWorld
REM
REM Finally, configure Visual Studio's Debug configuration with the rimworld exe as an external
REM program and set the working directory to the directory containing the exe.
REM
REM To debug, build the project (this script will install the mod), then run "Debug" (F5) which
REM will start RimWorld in paused state. Finally, choose "Debug -> Attach Unity Debugger" and
REM press "Input IP" and accept the default 127.0.0.1 : 56000
@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

SET RIMWORLD_DIR=%RIMWORLD_DIR_STEAM%

SET BUILD_CONFIGURATION=%1
SET SOLUTION_DIR=%~2
SET MOD_NAME=%~3
SET FOLDERS_TO_COPY=%~4
SET FILES_TO_COPY=%~5

SET MOD_DIR=%SOLUTION_DIR:~0,-8%\Staging

SET ZIP_FILE=%SOLUTION_DIR:~0,-8%\%MOD_NAME%

SET TARGET_DIR=%RIMWORLD_DIR%\Mods\%MOD_NAME%


FOR %%G IN (FOLDERS_TO_COPY) DO ( SET LAST_FOLDER=%%G )
SET MOD_DLL_PATH=%MOD_DIR%\%LAST_FOLDER%\Assemblies\%MOD_NAME%.dll

SET ZIP_EXE="C:\Program Files\7-Zip\7z.exe"



ECHO Running post-build script:
ECHO ==========================

IF %BUILD_CONFIGURATION%==Debug (
	IF EXIST "%MOD_DLL_PATH:~0,-4%.pdb" (
		ECHO Creating mdb at '%MOD_DLL_PATH%'
		"%PDB2MDB_PATH%" "%MOD_DLL_PATH%"
	)
)

IF %BUILD_CONFIGURATION%==Release (
	IF EXIST "%MOD_DLL_PATH%.mdb" (
		ECHO "Deleting %MOD_DLL_PATH%.mdb"
		DEL "%MOD_DLL_PATH%.mdb"
	)
)

IF EXIST "%RIMWORLD_DIR%" (
	IF "%TARGET_DIR%" == "%SOLUTION_DIR%" (
		ECHO Solution and mod target directory match. Skipping copy operation.
	) ELSE (
		ECHO Copying to %TARGET_DIR%
		IF NOT EXIST "%TARGET_DIR%" (
			MKDIR "%TARGET_DIR%"
		) ELSE (
			ECHO WARNING-'%TARGET_DIR%' already exists. Old files will not be automatically deleted. Make sure no unused files are still there.
		)

		FOR %%G IN (FOLDERS_TO_COPY) DO (
			SETLOCAL FOLDER=%%G
			ECHO Copying folder '%MOD_DIR%\%FOLDER%' to '%TARGET_DIR%\%FOLDER%'
			XCOPY /I /Y /E "%MOD_DIR%\%FOLDER%" "%TARGET_DIR%\%FOLDER%"
		)
		FOR %%G IN (FILES_TO_COPY) DO (
			SETLOCAL FILE=%%G
			ECHO Copying file '%MOD_DIR%\%FILE%' to '%TARGET_DIR%\%FILE%'
			XCOPY /Y "%MOD_DIR%\%FILE%" "%TARGET_DIR%\%FILE%"
		)
	)
	IF EXIST "%ZIP_FILE%.zip" (
		ECHO Deleting old '%ZIP_FILE%.zip'
		DEL "%ZIP_FILE%.zip"
	)
	ECHO Adding mod files to '%ZIP_FILE%.zip'
	FOR %%G IN (FOLDERS_TO_COPY) DO (
			SETLOCAL FOLDER=%%G
			ECHO Copying folder '%MOD_DIR%\%FOLDER%' to '%ZIP_FILE%.zip'
			%ZIP_EXE% a "%ZIP_FILE%.zip" "%MOD_DIR%\%FOLDER%"
		)
		FOR %%G IN (FILES_TO_COPY) DO (
			SETLOCAL FILE=%%G
			ECHO Copying file '%MOD_DIR%\%FILE%' to '%ZIP_FILE%.zip'
			%ZIP_EXE% a "%ZIP_FILE%.zip" "%MOD_DIR%\%FILE%"
		)
ECHO ==========================
ECHO post-build script complete
)