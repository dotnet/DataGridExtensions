# Data Grid Extensions

Modular extensions for the WPF DataGrid control.

This is a clone of https://datagridextensions.codeplex.com/

Binaries are available on [NuGet](http://nuget.org/packages/DataGridExtensions)

This package contains useful extensions for the WPF DataGrid (System.Windows.Controls.DataGrid).
The current version supports the following features and behaviors:

* Add filtering capabilities to the DataGrid.
* Apply the initial sorting
* Disable another control while in editing mode
* Provide additional column events
* Start editing a cell with Ctrl+Enter
* Provide an extended star-size column behavior

Every feature is individually configurable.

This extension is 
* Easy to use
* Easy to customize
* Attaches to the existing DataGrid

Unlike many other free extensions this package does not introduce a new derived DataGrid class, limiting you a fixed set of features that you have to live with, but transparently attaches to the existing DataGrid, giving you the freedom to use exactly the feature you need, customizing them as you like, and combining them with other useful extensions.

Filtering is enabled by simply adding one attached property to your DataGrid:

`<DataGrid ItemsSource="{Binding Items}" dgx:DataGridFilter.IsAutoFilterEnabled="True"/>`

You will get a simple but efficient text or boolean filter, depending on the column type:
![Sample1](https://github.com/tom-englert/DataGridExtensions/blob/master/Assets/Sample1.jpg)

Every part is easily customizable by providing simple styles or templates:

`<DataGridTextColumn Header="Double/Custom" Binding="{Binding Probability, Mode=OneWay}" dgx:DataGridFilterColumn.Template="{StaticResource FilterWithPopup}"/>`

By overriding the default template you can simply create individual filters:

![Sample2](https://github.com/tom-englert/DataGridExtensions/blob/master/Assets/Sample2.jpg)

For a full functional demo run the sample app or see the [ResX Resource Manager](https://github.com/tom-englert/ResXResourceManager) or [COM Registry Browser](http://comregistrybrowser.codeplex.com)  projects that use the filtering extensions.


Powerd by&nbsp;&nbsp;&nbsp;<a href="http://www.jetbrains.com/resharper/"><img src="http://www.tom-englert.de/Images/icon_ReSharper.png" alt="ReSharper" width="64" height="64" /></a> &nbsp;&nbsp;&nbsp; <a href="http://research.microsoft.com/en-us/projects/contracts/"><img src="http://research.microsoft.com/en-us/projects/contracts/codecontracts_sm.png" alt="CodeContracts" width="113" height="70" /></a>&nbsp;</p>
<p>Support this Project: <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=799WX673GPQM8"> <img style="border: none; margin-bottom: -6px;" title="Donate" src="https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif" alt="Donate" /></a></p>
