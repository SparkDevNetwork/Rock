using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Rock.CodeGeneration.Utility;

namespace Rock.CodeGeneration.Controls
{
    /// <summary>
    /// Interaction logic for NavigationFrame.xaml
    /// </summary>
    public partial class NavigationFrame : UserControl, INavigation
    {
        #region Fields

        /// <summary>
        /// The navigation stack containing all the pages currently active.
        /// </summary>
        private readonly List<Page> _navigationStack = new List<Page>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the root page at the bottom of the stack.
        /// </summary>
        /// <value>The root page.</value>
        /// <exception cref="System.Exception">Cannot change root page after creation.</exception>
        public Page RootPage
        {
            get => _navigationStack[0];
            set
            {
                if ( _navigationStack.Count != 0 )
                {
                    throw new Exception( "Cannot change root page after creation." );
                }

                _navigationStack.Add( value );

                SyncNavigation();
            }
        }

        /// <summary>
        /// Gets the current page at the top of the stack.
        /// </summary>
        /// <value>The current page displayed on screen.</value>
        public Page CurrentPage => _navigationStack.Last();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationFrame"/> class.
        /// </summary>
        public NavigationFrame()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Synchronizes the UI with the data we have.
        /// </summary>
        private void SyncNavigation()
        {
            if ( ContentFrame.Content != CurrentPage )
            {
                ContentFrame.Content = CurrentPage;

                // Remove any back items in the content frame otherwise it shows
                // its own navigation bar.
                while ( ContentFrame.CanGoBack )
                {
                    ContentFrame.RemoveBackEntry();
                }
            }

            // Update the state of various UI controls.
            PageTitle.Content = CurrentPage.Title;
            BackButton.Visibility = _navigationStack.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Pushes the page onto the navigation stack.
        /// </summary>
        /// <param name="page">The page to be pushed onto the navigation stack.</param>
        /// <returns>A <see cref="Task" /> that represents when the page has been pushed.</returns>
        public Task PushPageAsync( Page page )
        {
            _navigationStack.Add( page );
            SyncNavigation();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the top-most page off the navigation stack.
        /// </summary>
        /// <returns>A <see cref="Task" /> that represents when the page has been removed.</returns>
        /// <exception cref="System.Exception">Attempt to pop root page is not allowed.</exception>
        public Task PopPageAsync()
        {
            if ( _navigationStack.Count <= 1 )
            {
                throw new Exception( "Attempt to pop root page is not allowed." );
            }

            _navigationStack.RemoveAt( _navigationStack.Count - 1 );
            SyncNavigation();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes all pages from the navigation stack except for the root page.
        /// </summary>
        /// <returns>A <see cref="Task" /> that represents when all pages have been removed.</returns>
        public Task PopToRootAsync()
        {
            // Already at the root page, nothing to do.
            if ( _navigationStack.Count <= 1 )
            {
                return Task.CompletedTask;
            }

            while ( _navigationStack.Count > 1 )
            {
                _navigationStack.RemoveAt( 1 );
            }

            SyncNavigation();

            return Task.CompletedTask;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void BackButton_Click( object sender, RoutedEventArgs e )
        {
            await PopPageAsync();
        }

        #endregion
    }
}
