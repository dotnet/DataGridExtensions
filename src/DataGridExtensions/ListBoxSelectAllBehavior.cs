namespace DataGridExtensions
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    using Microsoft.Xaml.Behaviors;

    using TomsToolbox.Wpf;

    /// <summary>
    /// A behavior for a list box to handle the interaction between the list box and a "select all" checkbox.
    /// </summary>
    /// <seealso cref="Behavior{T}" />
    internal class ListBoxSelectAllBehavior : Behavior<ListBox>
    {
        private readonly DispatcherThrottle _collectionChangedThrottle;
        private bool _isListBoxUpdating;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxSelectAllBehavior"/> class.
        /// </summary>
        public ListBoxSelectAllBehavior()
        {
            _collectionChangedThrottle = new DispatcherThrottle(ListBox_CollectionChanged);
        }

        /// <summary>
        /// Gets or sets a flag indicating if all files are selected. Bind this property to the <see cref="ToggleButton.IsChecked"/> property of a three state check box.
        /// </summary>
        public bool? AreAllFilesSelected
        {
            get => (bool?)GetValue(AreAllFilesSelectedProperty);
            set => SetValue(AreAllFilesSelectedProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="AreAllFilesSelected"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AreAllFilesSelectedProperty =
            DependencyProperty.Register("AreAllFilesSelected", typeof(bool?), typeof(ListBoxSelectAllBehavior), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, e) => ((ListBoxSelectAllBehavior)sender)?.AreAllFilesSelected_Changed((bool?)e.NewValue)));

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the AssociatedObject.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            var listBox = AssociatedObject;
            if ((listBox == null) || DesignerProperties.GetIsInDesignMode(listBox))
                return;

            listBox.SelectAll();

            listBox.SelectionChanged += ListBox_SelectionChanged;
            ((INotifyCollectionChanged)listBox.Items).CollectionChanged += (_, __) => _collectionChangedThrottle.Tick();
        }

        private void ListBox_SelectionChanged(object? sender, EventArgs? e)
        {
            var listBox = AssociatedObject;
            if (listBox == null)
                return;

            try
            {
                _isListBoxUpdating = true;

                if (listBox.Items.Count == listBox.SelectedItems.Count)
                {
                    AreAllFilesSelected = true;
                }
                else if (listBox.SelectedItems.Count == 0)
                {
                    AreAllFilesSelected = false;
                }
                else
                {
                    AreAllFilesSelected = null;
                }
            }
            finally
            {
                _isListBoxUpdating = false;
            }
        }

        private void ListBox_CollectionChanged()
        {
            var listBox = AssociatedObject;

            if (AreAllFilesSelected.GetValueOrDefault())
            {
                listBox?.SelectAll();
            }
        }

        private void AreAllFilesSelected_Changed(bool? newValue)
        {
            var listBox = AssociatedObject;
            if (listBox == null)
                return;

            if (_isListBoxUpdating)
                return;

            if (newValue == null)
            {
                Dispatcher?.BeginInvoke(() => AreAllFilesSelected = false);
                return;
            }

            if (newValue.GetValueOrDefault())
            {
                listBox.SelectAll();
            }
            else
            {
                listBox.SelectedIndex = -1;
            }
        }
    }
}