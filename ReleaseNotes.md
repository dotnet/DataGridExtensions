1.0.36.0
* Fix #2: Added an additional DataGridFilterColumnControl.SelectableValues property that holds all values that are currently selectable in the column.

1.0.35.0
* Source values must treat null values as empty values to make multiple choice filters work in all cases.

1.0.34.0
* Change resource keys to workaround https://connect.microsoft.com/VisualStudio/feedback/details/2993889/

1.0.33.0
* Fix SourceItems for custom filters change while filtering when ItemsSource of DataGrid is an ICollectionView.

1.0.32.0
* Fix issue with  Copy/Paste support for cell selection when columns are reordered.

1.0.31.0
* Introduce resource locator to be able to completely override the resource location.

1.0.30.0
* Custom column styles must not be overridden by default column styles.

1.0.29.0
* Provide a means to override default column styles.
* Values and SourceValues need to be updated after a row change.

1.0.28.0
* Add feature: ForceCommitOnLostFocus 
* Fix null reference exception in HasRectangularCellSelection when cell selection is invalid.

1.0.27.0
* Improve Copy/Paste support for cell selection. Also supports n:m => x*n:y*m

1.0.26.0
* Copy/Paste support for cell selection.
* Multiline editing for text columns.
* Template columns now have a default filter (same as text column).
* Fix RegexContentFilter: Search for empty cells is possible with "^$".

1.0.25.0
* Include a clear button in the text filters text box to improve usability.

1.0.24.0
* Release after fixing additional CC warnings.

1.0.23.0
* Fix ExtendedStarSizeBehavior for frozen columns

1.0.22.0
* Improve usability of ExtendedStarSizeBehavior

1.0.21.0
* Improve keyboard navigation: Step into filters only with navigation keys, but not with TAB.

1.0.20.0
* WI1427: Combining Data Grid Extensions filters with other filtering: Expose a global filter.

1.0.19.0
* Improve extended star size behavior.

1.0.18.0

1.0.17.0
* Using CodeContracts.

1.0.16.0
* Added BeginEditOnCtrlEnterBehavior
* Added DisableTargetWhileEditingBehavior
* Added ExtendedStarSizeBehavior
* Added extended events

1.0.15.0
* DataGrid.Items.Filter must not be touched when filtering is disabled.
* Add a SourceValues property to DataGridFilterColumnsControl to support specialized filters with extended selection capabilities.

1.0.14.0
* Add the GroupStyleBinding helper class.

1.0.13.0
* Simplify access to filter by code.

1.0.12.0
* Fix WI1245: Loss of filter text when loading data.

1.0.11.0
* Fix WI1109: InvalidOperation Exception: "'Filter' is not allowed during an AddNew or EditItem transaction.".

1.0.10.0
* Make the filter symbol more flexible and extend the samples.

1.0.9.0
* WI1051: Make the filter evaluation throttle delay configurable.

1.0.8.0
* WI1039: Custom default column header style is ignored.

1.0.7.0
* New feature: Apply initial sorting.

1.0.6.0
* WI989: Filtering is broken after unload/load of the data grid.

1.0.5.0
* WI978: It should be possible to enable and disable filters at run time.

1.0.4.0
* Fix WI964: Possible null reference exception in DataGridFilterColumnControl.

1.0.3.0
* Fix: binaries were built from outdated source.

1.0.2.0
* Fix WI896: IContentFilter.IsMatch should be called even if cell content is null.
* Add some public accessors so it's possible to manipulate filters by code.