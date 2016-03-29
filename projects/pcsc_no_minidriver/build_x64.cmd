@echo on
pushd %0\..
set TARGET=pcsc_no_minidriver64
call %DGIT_DRIVE%\builder\WIN_SDK_7_1\setenv.cmd /Release /x64
set SOURCE_DIR=.\
set OBJECT_DIR=..\..\_obj\%TARGET%
set TARGET_DIR=..\..\_output\win64
mkdir %OBJECT_DIR% > NUL:
mkdir %TARGET_DIR% > NUL:
set CCFLAGS=/X /I%INCLUDE_DIR1% /I%INCLUDE_DIR2%  /I%SOURCE_DIR% /W3 /c
set LDFLAGS=/incremental:no /MACHINE:x64 /LIBPATH:%LIBRARY_DIR1% /LIBPATH:%LIBRARY_DIR2% 
make
copy scripts\* %TARGET_DIR% /Y
pause