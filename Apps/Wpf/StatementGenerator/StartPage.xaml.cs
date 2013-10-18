using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void btnStart_Click( object sender, RoutedEventArgs e )
        {
            ProgressPage progressPage = new ProgressPage();
            this.NavigationService.Navigate( progressPage );
        }

        private void mnuOptions_Click( object sender, RoutedEventArgs e )
        {

        }
    }
}
