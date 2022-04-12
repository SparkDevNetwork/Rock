using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using BlockGenerator.Utility;

namespace BlockGenerator.Controls
{
    /// <summary>
    /// Interaction logic for NavigationFrame.xaml
    /// </summary>
    public partial class NavigationFrame : UserControl, INavigation
    {
        private readonly List<Page> _navigationStack = new List<Page>();

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

        public Page CurrentPage => _navigationStack.Last();

        public NavigationFrame()
        {
            InitializeComponent();
        }

        private void SyncNavigation()
        {
            if ( ContentFrame.Content != CurrentPage )
            {
                ContentFrame.Content = CurrentPage;
                var back = ContentFrame.BackStack;
                while ( ContentFrame.CanGoBack )
                {
                    ContentFrame.RemoveBackEntry();
                }
            }

            PageTitle.Content = CurrentPage.Title;

            BackButton.Visibility = _navigationStack.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Task PushPageAsync( Page page )
        {
            _navigationStack.Add( page );
            SyncNavigation();

            return Task.CompletedTask;
        }

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

        public Task PopToRootAsync()
        {
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

        private async void BackButton_Click( object sender, RoutedEventArgs e )
        {
            await PopPageAsync();
        }
    }
}
