using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using ceTe.DynamicPDF.ReportWriter;
using Microsoft.Win32;
using Rock.Wpf;
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
            InitializeComponent();

            PopulateLayoutRadioButtonsFromDisk();
        }

        /// <summary>
        /// Populates the layout radio buttons from disk.
        /// </summary>
        private void PopulateLayoutRadioButtonsFromDisk()
        {
            List<RadioButton> radioButtonList = new List<RadioButton>();
            var rockConfig = RockConfig.Load();
            List<string> filenameList = Directory.GetFiles( ".", "*.dplx" ).ToList();
            foreach ( var fileName in filenameList )
            {
                DplxFile dplxFile = new DplxFile( fileName );
                DocumentLayout documentLayout = new DocumentLayout( dplxFile );
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

            lstLayouts.Items.Clear();
            foreach ( var item in radioButtonList.OrderBy( a => a.Content ) )
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
            dlg.Multiselect = true;

            if ( dlg.ShowDialog() == true )
            {
                foreach ( var fileName in dlg.FileNames )
                {
                    FileInfo sourceLayoutFileInfo = new FileInfo( fileName );
                    FileInfo destLayoutFileInfo = new FileInfo( Path.Combine( appDirectory, sourceLayoutFileInfo.Name ) );

                    if ( sourceLayoutFileInfo.FullName == destLayoutFileInfo.FullName )
                    {
                        // warn and skip if the are selecting the already imported file.
                        MessageBox.Show( "This is already the imported file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation );
                        continue;
                    }

                    if ( destLayoutFileInfo.Exists )
                    {
                        if ( MessageBox.Show( "That layout file already has been imported. Do you want to overwrite it?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question ) != MessageBoxResult.OK )
                        {
                            // skip this one if they chose not to overwrite
                            continue;
                        }
                    }

                    File.Copy( sourceLayoutFileInfo.FullName, destLayoutFileInfo.FullName, true );

                    XmlDocument doc = new XmlDocument();
                    doc.Load( sourceLayoutFileInfo.FullName );

                    // find all the image references in the imported layout file and copy them to the import destination
                    var imageNodes = doc.GetElementsByTagName( "image" );
                    foreach ( var imageNode in imageNodes.OfType<XmlNode>() )
                    {
                        string imagePath = imageNode.Attributes["path"].Value;
                        if ( !string.IsNullOrWhiteSpace( imagePath ) )
                        {
                            if ( string.IsNullOrWhiteSpace( Path.GetDirectoryName( imagePath ) ) )
                            {
                                imagePath = Path.Combine( sourceLayoutFileInfo.DirectoryName, imagePath );
                                FileInfo sourceImageFileInfo = new FileInfo( imagePath );
                                if ( sourceImageFileInfo.Exists )
                                {
                                    FileInfo destImageFileInfo = new FileInfo( Path.Combine( destLayoutFileInfo.DirectoryName, sourceImageFileInfo.Name ) );
                                    if ( destImageFileInfo.Exists )
                                    {
                                        if ( MessageBox.Show( "Image file '" + destImageFileInfo.Name + "' already has been imported. Do you want to overwrite it?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question ) != MessageBoxResult.OK )
                                        {
                                            // skip this imagefile if they chose not to overwrite
                                            continue;
                                        }
                                    }

                                    File.Copy( sourceImageFileInfo.FullName, destImageFileInfo.FullName, true );
                                }
                            }
                        }
                    }
                }

                PopulateLayoutRadioButtonsFromDisk();
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
                ReportOptions.Current.LayoutFile = fileName;
                var rockConfig = RockConfig.Load();
                rockConfig.LayoutFile = fileName;
                rockConfig.Save();
                ProgressPage nextPage = new ProgressPage();
                this.NavigationService.Navigate( nextPage );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnBack_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.GoBack();
        }
    }
}
