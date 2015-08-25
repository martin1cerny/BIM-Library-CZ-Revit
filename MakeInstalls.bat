rem Build and pack 2015
"c:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe" BimLibraryAddin.sln /build Release2015
"c:\Program Files (x86)\NSIS\Unicode\makensis.exe" InstallSetup2015.nsi 
rem Build and pack 2016
"c:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe" BimLibraryAddin.sln /build Release2016
"c:\Program Files (x86)\NSIS\Unicode\makensis.exe" InstallSetup2016.nsi 
