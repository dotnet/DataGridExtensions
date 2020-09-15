### Filtering

Filtering is enabled by simply adding one attached property to your DataGrid:
```xml
<DataGrid ItemsSource="{Binding Items}" 
          dgx:DataGridFilter.IsAutoFilterEnabled="True"/>
```
You will get a simple but efficient text or boolean filter, depending on the column type:
![Sample1](Filtering_Sample1.jpg)

Every part is easily customizable by providing simple styles or templates:
```xml
<DataGridTextColumn Header="Double/Custom" 
                    Binding="{Binding Probability, Mode=OneWay}" 
                    dgx:DataGridFilterColumn.Template="{StaticResource FilterWithPopup}"/>
```
By overriding the default template you can simply create individual filters:

![Sample2](Filtering_Sample2.jpg)

For further details take a tour through the code of the sample application to see the many possibilities how to use and customize it.
