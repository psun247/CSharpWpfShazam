# CSharpWpfShazam
This is a C# WPF app that identifies songs like the popular mobile app Shazam and save song info with lyrics in MySQL database.

![image](https://github.com/psun247/CSharpWpfShazam/assets/31531761/7412791c-db52-4618-adaf-ca3f90f9f786)

# Build
Build CSharpWpfShazam.sln with Visual Studio Professional 2022 (64-bit) or Visual Studio Community 2022 (64-bit).  This app is targeted for .NET 7. To compile for .NET 6, simply modify CSharpWpfShazam.csproj.

# Usage
Audio devices will be automatically queried and displayed in the dropdown list.  You will need to select a proper device for 'Listen to'.  Add and Delete buttons are disabled when MySQL is not installed or turned off (i.e. Demo Mode on MySQL tab).

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
