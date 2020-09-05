namespace DataGridExtensionsSample
{
    using System.Windows;
    using TomsToolbox.Wpf.Styles;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Resources.MergedDictionaries.Add(WpfStyles.GetDefaultStyles());
            Resources.RegisterDefaultWindowStyle();
        }
    }
}
