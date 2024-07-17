using Rock.CloudPrint.Desktop.Pages;

namespace Rock.CloudPrint.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += ( _, _ ) => RootNavigation.Navigate( typeof( DashboardPage ) );
        }
    }
}