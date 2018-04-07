Atos Sips v1 Payment Gateway
============================

This is a payment gateway for the Atos Sips v1 system.

Install into DNN as a normal module.

Go into Open-Store BO>Admin, the "Atos Sips" option should be listed.

Enter details and save (in ALL languages)

The gateay should now be ready.


NOTE: This install includes the Sips request.exe & response.exe files and on some servers the security restirctions will prevent these from installing.
In this situation you'll need to move them to the server manually, into: \DesktopModules\NBright\OS_Sips\sipsbin

 

 Development
 ===========

 You need to install the NuGet package "MSBuildTasks v1.5.0.235"

 The request.exe & response.exe files have been ignored by Git, unzip the \DesktopModules\NBright\OS_Sips\sipsbin\sipsbin.zip to get them.

