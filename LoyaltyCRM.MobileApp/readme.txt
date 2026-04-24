Create a TestDevice in sdk

Start Emulator

C:\android-sdk\emulator\emulator.exe -avd TestDevice -no-snapshot-load

To run

dotnet build -f net10.0-android "-p:AndroidSdkDirectory=C:\android-sdk" -t:Run

publish Mobile App

dotnet publish -f net10.0-android -c Release -p:AndroidPackageFormat=apk -p:AndroidSdkDirectory=C:\android-sdk

Then find the generated APK in the MAUI output folder, for example:

[LoyaltyCRM.MobileApp\bin\Release\net10.0-android](http://_vscodecontentref_/6)
or the publish subfolder under that path

Put in LoyaltyCRM.WebApp\wwwroot\downloads