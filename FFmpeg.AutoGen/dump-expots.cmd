set dumpbin="%PROGRAMFILES(X86)%\Microsoft Visual Studio 12.0\VC\bin\amd64\dumpbin.exe" 
del .\ffmpeg\exports.txt
cd .\ffmpeg\bin\x64\
%dumpbin% /EXPORTS avcodec-57.dll avdevice-57.dll avfilter-6.dll avformat-57.dll avutil-55.dll postproc-54.dll swresample-2.dll swscale-4.dll >> ..\..\exports.txt
cd ..\..\..\