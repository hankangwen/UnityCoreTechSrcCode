@echo off
set tool=..\3Party\protobuf-net\net

rem======================================
rem Support
set proto=common.proto
%tool%\protogen.exe -i:%proto% -o:%proto%.cs -q

set proto=login.proto
%tool%\protogen.exe -i:%proto% -o:%proto%.cs -q

pause