xcopy /y .\YokeEmulatorServer\bin\x86\Release\Emulator.config 			.\YokeEmulatorServer-Pack\x86\
xcopy /y .\YokeEmulatorServer\bin\x86\Release\msvcr120.dll 				.\YokeEmulatorServer-Pack\x86\
xcopy /y .\YokeEmulatorServer\bin\x86\Release\vJoyInterface.dll 		.\YokeEmulatorServer-Pack\x86\
xcopy /y .\YokeEmulatorServer\bin\x86\Release\vJoyInterfaceWrap.dll 	.\YokeEmulatorServer-Pack\x86\
xcopy /y .\YokeEmulatorServer\bin\x86\Release\YokeEmulatorServer.exe 	.\YokeEmulatorServer-Pack\x86\

xcopy /y .\YokeEmulatorServer\bin\x64\Release\Emulator.config 			.\YokeEmulatorServer-Pack\x64\
xcopy /y .\YokeEmulatorServer\bin\x64\Release\msvcr120.dll 				.\YokeEmulatorServer-Pack\x64\
xcopy /y .\YokeEmulatorServer\bin\x64\Release\vJoyInterface.dll 		.\YokeEmulatorServer-Pack\x64\
xcopy /y .\YokeEmulatorServer\bin\x64\Release\vJoyInterfaceWrap.dll 	.\YokeEmulatorServer-Pack\x64\
xcopy /y .\YokeEmulatorServer\bin\x64\Release\YokeEmulatorServer.exe 	.\YokeEmulatorServer-Pack\x64\

tar cvf Package.tar.gz YokeEmulatorServer-Pack ReadMe\ReadMe.pdf opentrack-2.3-rc15p1 vJoy_205_050515.exe SimConnect.msi