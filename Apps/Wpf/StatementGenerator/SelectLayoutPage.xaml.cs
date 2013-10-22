using System;
using System.Collections.Generic;
using System.IO;
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
using ceTe.DynamicPDF.ReportWriter;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for SelectLayoutPage.xaml
    /// </summary>
    public partial class SelectLayoutPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectLayoutPage"/> class.
        /// </summary>
        public SelectLayoutPage()
        {
            List<RadioButton> radioButtonList = new List<RadioButton>();
            var rockConfig = RockConfig.Load();
            InitializeComponent();
            List<string> filenameList = Directory.GetFiles( "", "*.dplx" ).ToList();
            foreach ( var fileName in filenameList )
            {
                DplxFile dplxFile = new DplxFile( fileName );
                DocumentLayout documentLayout = new DocumentLayout(dplxFile);
                RadioButton radLayout = new RadioButton();
                if ( !string.IsNullOrWhiteSpace( documentLayout.Title ) )
                {
                    radLayout.Content = documentLayout.Title.Trim();
                }
                else
                {
                    radLayout.Content = fileName;
                }

                radLayout.Tag = fileName;
                radLayout.IsChecked = rockConfig.LayoutFile == fileName;
                radioButtonList.Add( radLayout );
            }

            if ( !radioButtonList.Any( a => a.IsChecked ?? false ) )
            {
                if ( radioButtonList.FirstOrDefault() != null )
                {
                    radioButtonList.First().IsChecked = true;
                }
            }

            foreach (var item in radioButtonList.OrderBy(a=> a.Content))
            {
                lstLayouts.Items.Add( item );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnImport_Click( object sender, RoutedEventArgs e )
        {
            string appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string appDirectory = Path.GetDirectoryName( appPath );
            
            
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".dplx";
            dlg.Filter = "Report Files (.dplx)|*.dplx";

            if ( dlg.ShowDialog() == true )
            {
                foreach ( var fileName in dlg.FileNames )
                {
                    FileInfo sourceFileInfo = new FileInfo(fileName);
                    FileInfo destFileInfo = new FileInfo(Path.Combine(appDirectory, sourceFileInfo.Name));

                    if ( sourceFileInfo.FullName == destFileInfo.FullName )
                    {
                        // warn and skip if the are selecting the already imported file.
                        MessageBox.Show( "This is already the imported file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation );
                        continue;
                    }

                    if ( destFileInfo.Exists )
                    {
                        if ( MessageBox.Show( "That file already has been imported. Do you want to overwrite it?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question ) != MessageBoxResult.OK )
                        {
                            // skip this one if they chose not to overwrite
                            continue;
                        }
                    }

                    File.Copy( sourceFileInfo.FullName, destFileInfo.FullName, true );
                    DocumentLayout doc = new DocumentLayout( sourceFileInfo.FullName );

                    var imgLogo = doc.GetReportElementById( "imgLogo" );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click( object sender, RoutedEventArgs e )
        {
            var selected = lstLayouts.Items.OfType<RadioButton>().First( a => a.IsChecked == true );
            if ( selected != null )
            {
                string fileName = selected.Tag.ToString();
                ReportOptions.Current.LayoutFile = new DplxFile( fileName );
                var rockConfig = RockConfig.Load();
                rockConfig.LayoutFile = fileName;
                rockConfig.Save();
                ProgressPage nextPage = new ProgressPage();
                this.NavigationService.Navigate( nextPage );
            }
        }
    }
}
