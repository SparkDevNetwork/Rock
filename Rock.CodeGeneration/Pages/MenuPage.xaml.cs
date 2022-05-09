using System.Windows;
using System.Windows.Controls;

namespace Rock.CodeGeneration.Pages
{
    /// <summary>
    /// Interaction logic for MenuPage.xaml
    /// </summary>
    public partial class MenuPage : Page
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        private async void ModelGenerationButton_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ModelGenerationPage() );
        }

        private async void ObsidianDetailBlock_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ObsidianDetailBlockPage() );
        }

        private async void ObsidianViewModelsButton_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ObsidianViewModelsPage() );
        }

        private async void ObsidianEnumsButton_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ObsidianEnumsPage() );
        }

        private async void ObsidianSystemGuidsButton_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ObsidianSystemGuidsPage() );
        }
    }
}
