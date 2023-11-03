using System.Windows;
using System.Windows.Controls;

namespace Rock.CodeGeneration.Pages
{
    /// <summary>
    /// Interaction logic for MenuPage.xaml
    /// </summary>
    public partial class MenuPage : Page
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuPage"/> class.
        /// </summary>
        public MenuPage()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the ModelGenerationButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ModelGenerationButton_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ModelGenerationPage() );
        }

        /// <summary>
        /// Handles the Click event of the ObsidianDetailBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ObsidianDetailBlock_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ObsidianDetailBlockPage() );
        }

        /// <summary>
        /// Handles the Click event of the ObsidianListBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ObsidianListBlock_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ObsidianListBlockPage() );
        }

        /// <summary>
        /// Handles the Click event of the ObsidianViewModelsButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ObsidianViewModelsButton_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ObsidianViewModelsPage() );
        }

        /// <summary>
        /// Handles the Click event of the ObsidianEnumsButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ObsidianEnumsButton_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ObsidianEnumsPage() );
        }

        /// <summary>
        /// Handles the Click event of the ObsidianSystemGuidsButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ObsidianSystemGuidsButton_Click( object sender, RoutedEventArgs e )
        {
            await this.Navigation().PushPageAsync( new ObsidianSystemGuidsPage() );
        }

        #endregion
    }
}
