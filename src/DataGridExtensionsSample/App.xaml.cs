namespace DataGridExtensionsSample
{
    using System.Windows;

    using Ninject;

    using TomsToolbox.Composition;
    using TomsToolbox.Composition.Ninject;
    using TomsToolbox.Wpf.Composition;
    using TomsToolbox.Wpf.Styles;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public partial class App
    {
        private readonly IKernel _kernel = new StandardKernel();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // modern styling
            Resources.MergedDictionaries.Add(WpfStyles.GetDefaultStyles());
            Resources.RegisterDefaultWindowStyle();

            // setup visual composition infrastructure, using Ninject
            _kernel.BindExports(GetType().Assembly);
            IExportProvider exportProvider = new ExportProvider(_kernel);
            _kernel.Bind<IExportProvider>().ToConstant(exportProvider);

            // setup global export provider locator for XAML
            ExportProviderLocator.Register(exportProvider);

            // register all controls tagged as data templates
            var dynamicDataTemplates = DataTemplateManager.CreateDynamicDataTemplates(exportProvider);
            Resources.MergedDictionaries.Add(dynamicDataTemplates);

            MainWindow = exportProvider.GetExportedValue<MainWindow>();
            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _kernel.Dispose();
        }
    }
}
