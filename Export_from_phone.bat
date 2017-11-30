%cd%\adb\adb devices
%cd%\adb\adb wait-for-device
%cd%\adb\adb backup -f export_data.ab com.huami.watch.hmwatchmanager
java -jar %cd%\adb\abe.jar unpack export_data.ab export_data.tar
"C:\Program Files\7-Zip\7z.exe" e -odb export_data.tar apps\com.huami.watch.hmwatchmanager\db\companion-aa.* 
