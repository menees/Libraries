![windows build & test](https://github.com/bmenees/Libraries/workflows/windows%20build%20&%20test/badge.svg)

# Libraries
This repo contains the source code for several .NET libraries used in Menees utilities.

|Library|Description|
|---|---|
|[Menees.Common](src/Menees.Common)|Helpers for files, text, XML, logging, exceptions, etc.|
|[Menees.Diffs](src/Menees.Diffs)|Differencing algorithms for text files, binary files, and directories|
|[Menees.Diffs.Windows.Forms](src/Menees.Diffs.Windows.Forms)|Windows Forms controls for showing diffs of text, files, and directories|
|[Menees.Windows](src/Menees.Windows)|Windows-specific helpers for WMI, shell dialogs, and Visual Studio invocation|
|[Menees.Windows.Forms](src/Menees.Windows.Forms)|Windows Forms helpers and custom controls|
|[Menees.Windows.Presentation](src/Menees.Windows.Presentation)|WPF helpers and custom controls|

## Targets
|Library|.NET Framework|.NET Core|.NET Standard|NuGet|
|---|---|---|---|---|
|[Menees.Common](src/Menees.Common)|4.5|3.1|2.0|[![Nuget](https://img.shields.io/nuget/v/Menees.Common)](https://www.nuget.org/packages/Menees.Common/)|
|[Menees.Diffs](src/Menees.Diffs)|4.5|3.1|2.0|[![Nuget](https://img.shields.io/nuget/v/Menees.Diffs)](https://www.nuget.org/packages/Menees.Diffs/)|
|[Menees.Diffs.Windows.Forms](src/Menees.Diffs.Windows.Forms)|4.5|3.1|--|[![Nuget](https://img.shields.io/nuget/v/Menees.Diffs.Windows.Forms)](https://www.nuget.org/packages/Menees.Diffs.Windows.Forms/)|
|[Menees.Windows](src/Menees.Windows)|4.5|3.1|--|[![Nuget](https://img.shields.io/nuget/v/Menees.Windows)](https://www.nuget.org/packages/Menees.Windows/)|
|[Menees.Windows.Forms](src/Menees.Windows.Forms)|4.5|3.1|--|[![Nuget](https://img.shields.io/nuget/v/Menees.Windows.Forms)](https://www.nuget.org/packages/Menees.Windows.Forms/)|
|[Menees.Windows.Presentation](src/Menees.Windows.Presentation)|4.5|3.1|--|[![Nuget](https://img.shields.io/nuget/v/Menees.Windows.Presentation)](https://www.nuget.org/packages/Menees.Windows.Presentation/)|

## Used By
* [Diff.Net](https://github.com/bmenees/Diff.Net)
* [DiskUsage](https://github.com/bmenees/DiskUsage)
* [Gizmos](https://github.com/bmenees/Gizmos)
* [Hasher](https://github.com/bmenees/Hasher)
* [MegaBuild](https://github.com/bmenees/MegaBuild)
* [Menees VS Tools](http://www.menees.com/VSTools.htm)
* [RPN Calc 3.0](https://github.com/bmenees/RpnCalc)
* [SharpScript](http://www.menees.com/SharpScript.htm)
* [WorkBreak](https://github.com/bmenees/WorkBreak)