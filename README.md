# CSharpWpfShazam
This is a C# WPF app that identifies songs like the popular mobile app Shazam and save song info in Azure SQL DB via REST API or in local MySQL DB.

# Shazam tab
![image](https://github.com/psun247/CSharpWpfShazam/assets/31531761/014e1450-94ab-4ea9-99ea-b9ada5da0d1b)

# Azure (REST API) tab
![image](https://github.com/psun247/CSharpWpfShazam/assets/31531761/74e29665-d048-4b66-a3e1-763eac83026e)

# MySQL (could be MS SQL Server) tab
![image](https://github.com/psun247/CSharpWpfShazam/assets/31531761/1ff3a4bb-dfbf-4db8-9c9b-ab04ed575198)

# Build
Build CSharpWpfShazam.sln with Visual Studio Professional 2022 (64-bit) or Visual Studio Community 2022 (64-bit).  This app is targeted for .NET 7. To compile for .NET 6, simply modify CSharpWpfShazam.csproj.

# Run
To run the app without compiling it,
1. Click CSharpWpfShazam_v1.1 under Releases on the right side of this page
2. Download CSharpWpfShazam_v1.1_net6.0-windows.zip
3. Unzip the file and run CSharpWpfShazam.exe

# Usage
Audio devices will be automatically queried and displayed in the dropdown list.  You will need to select a proper device for 'Listen to'.  Add and Delete buttons are for Azure SQL DB (via REST API) or local MySQL DB (MS SQL Server could easily be used instead). The blue arrow on the right side of the screen will expand or collapse the song info section.

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
