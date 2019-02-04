namespace DataGridExtensions.Behaviors
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interactivity;

    using JetBrains.Annotations;

    /// <summary>
    /// Clears the text of the <see cref="TextBox"/> when the associated button is clicked.
    /// </summary>
    public class ClearTextBoxOnButtonClickBehavior : Behavior<Button>
    {
        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the AssociatedObject.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            var button = AssociatedObject;

            button.Click += AssociatedObject_Click;
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>
        /// Override this to unhook functionality from the AssociatedObject.
        /// </remarks>
        protected override void OnDetaching()
        {
            var button = AssociatedObject;

            button.Click -= AssociatedObject_Click;

            base.OnDetaching();
        }

        /// <summary>
        /// Gets or sets the text box to clear.
        /// </summary>
        [CanBeNull]
        public TextBox TextBox
        {
            get => (TextBox)GetValue(TextBoxProperty);
            set => SetValue(TextBoxProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="TextBox"/> dependency property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty TextBoxProperty =
            DependencyProperty.Register("TextBox", typeof (TextBox), typeof (ClearTextBoxOnButtonClickBehavior));

        private void AssociatedObject_Click([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            var textBox = TextBox;

            if (textBox != null)
                textBox.Text = string.Empty;
        }
    }
}
