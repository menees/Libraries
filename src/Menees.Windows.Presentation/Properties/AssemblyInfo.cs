using System.Windows.Markup;

// This allows Xaml files in other projects to use xmlns:m="http://menees.com/xaml" instead of
// xmlns:m="clr-namespace:Menees.Windows.Presentation;assembly=Menees.Windows.Presentation".
[assembly: XmlnsDefinition("http://menees.com/xaml", "Menees.Windows.Presentation", AssemblyName = "Menees.Windows.Presentation")]