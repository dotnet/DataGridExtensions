2.4.7
- Package is now digitally signed

2.4.6
- Fix #43: After enabling filtering HeaderStringFormats are being ignored

2.4.5
- Fix #42: ApplyInitialSorting does not consider grouping

2.4.4
- Fix #41: ApplyInitialSorting may cause exceptions

2.4.3
- Fix #40: defer scroll-into-view, to ensure it's working always

2.4.2
- Fix #40: When relaxing filter criteria, the selected item should stay in view

2.4.1
- Fix #35: DataGridFilterColumn doesn't work for combobox column

2.4
- Fix  #23: Column virtualization is not supported

2.3
- Fix #32: SelectAll() is not available when SelectionUnit="Cell"

2.2
- #29: Support default for ComboBoxColumns.

2.1.1
- Fix #27: Change notification for SourceValues when collection of item changes.

2.1
- Update Microsoft.Xaml.Behaviors.Wpf

2.0.1
- Fix #25: Missing resources in netcoreapp3.0 target
 
2.0.0
- Migrate System.Windows.Interactivity to Microsoft.Xaml.Behaviors
- Drop support for .Net < 4.5, add support for Net Core 3.0

1.0.41
- Fix #16: exception clicking select all on a single select grid

1.0.40
- Use embedded pdbs and license

1.0.39
- "SelectAll" button clears all filters if the data grid does not display any rows, because column headers can't be scrolled horizontally in this case. Now also works if column header style of dg was set already explicit

1.0.38
- "SelectAll" button clears all filters if the data grid does not display any rows, because column headers can't be scrolled horizontally in this case.
- Provide an explicit .net45 version to fix issues with referenced packages (System.Windows.Interactivity). 

1.0.37
* Include ExternalAnnotations for R# 

1.0.36
* Fix #2: Added an additional DataGridFilterColumnControl.SelectableValues property that holds all values that are currently selectable in the column.

1.0.35
* Source values must treat null values as empty values to make multiple choice filters work in all cases.

1.0.34
* Change resource keys to workaround https://connect.microsoft.com/VisualStudio/feedback/details/2993889/

1.0.33
* Fix SourceItems for custom filters change while filtering when ItemsSource of DataGrid is an ICollectionView.

1.0.32
* Fix issue with  Copy/Paste support for cell selection when columns are reordered.

1.0.31
* Introduce resource locator to be able to completely override the resource location.

1.0.30
* Custom column styles must not be overridden by default column styles.

1.0.29
* Provide a means to override default column styles.
* Values and SourceValues need to be updated after a row change.

1.0.28
* Add feature: ForceCommitOnLostFocus 
* Fix null reference exception in HasRectangularCellSelection when cell selection is invalid.

1.0.27
* Improve Copy/Paste support for cell selection. Also supports n:m => x*n:y*m

1.0.26
* Copy/Paste support for cell selection.
* Multiline editing for text columns.
* Template columns now have a default filter (same as text column).
* Fix RegexContentFilter: Search for empty cells is possible with "^$".

1.0.25
* Include a clear button in the text filters text box to improve usability.

1.0.24
* Release after fixing additional CC warnings.

1.0.23
* Fix ExtendedStarSizeBehavior for frozen columns

1.0.22
* Improve usability of ExtendedStarSizeBehavior

1.0.21
* Improve keyboard navigation: Step into filters only with navigation keys, but not with TAB.

1.0.20
* WI1427: Combining Data Grid Extensions filters with other filtering: Expose a global filter.

1.0.19
* Improve extended star size behavior.

1.0.18

1.0.17
* Using CodeContracts.

1.0.16
* Added BeginEditOnCtrlEnterBehavior
* Added DisableTargetWhileEditingBehavior
* Added ExtendedStarSizeBehavior
* Added extended events

1.0.15
* DataGrid.Items.Filter must not be touched when filtering is disabled.
* Add a SourceValues property to DataGridFilterColumnsControl to support specialized filters with extended selection capabilities.

1.0.14
* Add the GroupStyleBinding helper class.

1.0.13
* Simplify access to filter by code.

1.0.12
* Fix WI1245: Loss of filter text when loading data.

1.0.11
* Fix WI1109: InvalidOperation Exception: "'Filter' is not allowed during an AddNew or EditItem transaction.".

1.0.10
* Make the filter symbol more flexible and extend the samples.

1.0.9
* WI1051: Make the filter evaluation throttle delay configurable.

1.0.8
* WI1039: Custom default column header style is ignored.

1.0.7
* New feature: Apply initial sorting.

1.0.6
* WI989: Filtering is broken after unload/load of the data grid.

1.0.5
* WI978: It should be possible to enable and disable filters at run time.

1.0.4
* Fix WI964: Possible null reference exception in DataGridFilterColumnControl.

1.0.3
* Fix: binaries were built from outdated source.

1.0.2
* Fix WI896: IContentFilter.IsMatch should be called even if cell content is null.
* Add some public accessors so it's possible to manipulate filters by code.