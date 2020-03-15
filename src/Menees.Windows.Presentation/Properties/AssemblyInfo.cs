using System.Windows;
using System.Windows.Markup;

// Where theme-specific resource dictionaries are located (used if a resource is not found in the page, or application resource dictionaries)
// Where the generic resource dictionary is located (used if a resource is not found in the page, app, or any theme specific resource dictionaries)
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

// This allows Xaml files in other projects to use xmlns:m="http://menees.com/xaml" instead of
// xmlns:m="clr-namespace:Menees.Windows.Presentation;assembly=Menees.Windows.Presentation".
[assembly: XmlnsDefinition("http://menees.com/xaml", "Menees.Windows.Presentation", AssemblyName = "Menees.Windows.Presentation")]