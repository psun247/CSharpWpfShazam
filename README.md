# CSharpWpfShazam
This is a C# WPF app that identifies songs like the popular mobile app Shazam and save song info with lyrics in MySQL database.

![image](https://github.com/psun247/CSharpWpfShazam/assets/31531761/a5212399-1ea0-478f-bc79-828ef3de3bb9)

![image](https://github.com/psun247/CSharpWpfShazam/assets/31531761/82f2e11f-8e6c-48ec-8336-5dd56dce6476)

# Build
Build CSharpWpfShazam.sln with Visual Studio Professional 2022 (64-bit) or Visual Studio Community 2022 (64-bit).  This app is targeted for .NET 7. To compile for .NET 6, simply modify CSharpWpfShazam.csproj.

# Run
To run the app without compiling it,
1. Click CSharpWpfShazam_v1.0 under Releases on the right side of this page
2. Download CSharpWpfShazam_v1.0_net6.0-windows.zip
3. Unzip the file and run CSharpWpfShazam.exe

# Usage
Audio devices will be automatically queried and displayed in the dropdown list.  You will need to select a proper device for 'Listen to'.  Add and Delete buttons are disabled when MySQL is not installed or turned off (i.e. Demo Mode on MySQL tab). The blue arrow on the right side of the screen will expand or collapse the song info section.

# Supporting Libraries
CommunityToolkit.Mvvm
 
https://www.nuget.org/packages/CommunityToolkit.Mvvm

Pomelo.EntityFrameworkCore.MySql

https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql
 
ModernWpfUI
 
https://www.nuget.org/packages/ModernWpfUI/

NAudio

https://www.nuget.org/packages/NAudio
 
RestoreWindowPlace

https://www.nuget.org/packages/RestoreWindowPlace
