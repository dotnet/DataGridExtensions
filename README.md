# Data Grid Extensions [![Build Status](https://dev.azure.com/tom-englert/Open%20Source/_apis/build/status/DataGridExtensions?repoName=dotnet%2FDataGridExtensions&branchName=master)](https://dev.azure.com/tom-englert/Open%20Source/_build/latest?definitionId=37&repoName=dotnet%2FDataGridExtensions&branchName=master) [![NuGet Status](http://img.shields.io/nuget/v/DataGridExtensions.svg?style=flat)](https://www.nuget.org/packages/DataGridExtensions/)

![Icon](Icon.png)

Modular extensions for the WPF DataGrid control.

## Code of Conduct
This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

## .NET Foundation
This project is supported by the 

[<img src="https://raw.githubusercontent.com/dotnet-foundation/swag/master/logo/dotnetfoundation_v4.svg" alt=".NET Foundation" width=100>](https://dotnetfoundation.org)



## Summary
This package contains useful extensions for the WPF DataGrid (`System.Windows.Controls.DataGrid`).

The current version supports the following features and behaviors:

* Add filtering capabilities to the DataGrid
* Apply the initial sorting
* Disable another control while in editing mode
* Provide additional column events
* Start editing a cell with Ctrl+Enter
* Provide an extended star-size column behavior
* Methods to easily implement Excel-like copy/paste

The binaries are available as [NuGet](http://nuget.org/packages/DataGridExtensions) packages.

Read the [Documentation](Documentation/README.md) about how to use these features.
The [API Documentation](https://dotnet.github.io/DataGridExtensions/) describes the global API for DGX.

Every feature is individually configurable.

This extension is 
* Easy to use
* Easy to customize
* Attaches to the existing DataGrid

Unlike many other free extensions this package does not introduce a new derived `DataGrid` class, limiting you a fixed set of features 
that you have to live with, but transparently attaches to the existing `DataGrid`, giving you the freedom to use exactly the feature you need, 
customizing them as you like, and combining them with other useful extensions.

Filtering is enabled by simply adding one attached property to your DataGrid:

```xml
<DataGrid ItemsSource="{Binding Items}" 
          dgx:DataGridFilter.IsAutoFilterEnabled="True"/>
```

You will get a simple but efficient text or boolean filter, depending on the column type:
![Sample1](Assets/Sample1.jpg)

Every part is easily customizable by providing simple styles or templates:

```xml
<DataGridTextColumn Header="Double/Custom" 
                    Binding="{Binding Probability, Mode=OneWay}" 
                    dgx:DataGridFilterColumn.Template="{StaticResource FilterWithPopup}"/>
```

By overriding the default template you can simply create individual filters:

![Sample2](Assets/Sample2.jpg)

For a full functional demo run the sample app or see the [ResX Resource Manager](https://github.com/dotnet/ResXResourceManager) 
or [Project Configuration Manager](https://github.com/tom-englert/ProjectConfigurationManager) projects that use the filtering extensions.


Powered by&nbsp;&nbsp;&nbsp;<a href="http://www.jetbrains.com/resharper/"><img src="http://www.tom-englert.de/Images/icon_ReSharper.png" alt="ReSharper" width="64" height="64" /></a> &nbsp;&nbsp;&nbsp;
<p>Support this Project: <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=799WX673GPQM8"> <img style="border: none; margin-bottom: -6px;" title="Donate" src="https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif" alt="Donate" /></a></p>
