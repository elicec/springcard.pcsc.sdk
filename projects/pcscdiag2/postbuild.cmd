@echo off
pushd %0\..
copy scripts\* ..\..\_output\win32 /Y
rem call i:\builder\tools\signtool.cmd ..\..\_output\win32\pcscdiag2.exe
