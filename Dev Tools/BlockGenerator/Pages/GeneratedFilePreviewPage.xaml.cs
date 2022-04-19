using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using BlockGenerator.Utility;

using Rock;

namespace BlockGenerator.Pages
{
    /// <summary>
    /// Interaction logic for GeneratedFilePreview.xaml
    /// </summary>
    public partial class GeneratedFilePreviewPage : Page
    {
        private readonly List<ExportFile> _exportFiles;

        private int _selectedDiffIndex = 0;
        private readonly Brush _unselectedButtonBackground = new SolidColorBrush( Color.FromRgb( 0xdd, 0xdd, 0xdd ) );
        private readonly Brush _unselectedButtonForeground = new SolidColorBrush( Color.FromRgb( 0, 0, 0 ) );
        private readonly Brush _selectedButtonBackground = new SolidColorBrush( Color.FromRgb( 0xee, 0x77, 0x25 ) );
        private readonly Brush _selectedButtonForeground = new SolidColorBrush( Color.FromRgb( 0xf0, 0xf0, 0xf0 ) );

        public GeneratedFilePreviewPage( IList<GeneratedFile> files )
        {
            InitializeComponent();

            SyncDiffButtonState();

            _exportFiles = files.Select( f => new ExportFile( f ) ).ToList();

            _exportFiles.ForEach( UpdateExportFile );

            _exportFiles = _exportFiles.OrderByDescending( f => f.IsWriteNeeded )
                .ThenBy( f => f.File.FileName )
                .ToList();

            FileListBox.ItemsSource = _exportFiles;
        }

        private static void UpdateExportFile( ExportFile file )
        {
            var solutionPath = GetSolutionPath();

            // Check if the file needs to be written.
            if ( solutionPath.IsNotNullOrWhiteSpace() )
            {
                var filePath = Path.Combine( solutionPath, file.File.SolutionRelativePath );

                if ( File.Exists( filePath ) )
                {
                    var fileContents = File.ReadAllText( filePath );

                    file.OldContent = fileContents;
                    file.IsWriteNeeded = fileContents != file.File.Content;
                }
                else
                {
                    file.IsWriteNeeded = true;
                }
            }
            else
            {
                file.IsWriteNeeded = true;
            }

            file.IsUpToDate = !file.IsWriteNeeded;
            file.IsExporting = !file.IsUpToDate;
        }

        private static string GetSolutionPath()
        {
            var directoryInfo = new DirectoryInfo( Directory.GetCurrentDirectory() );

            while ( directoryInfo != null )
            {
                if ( File.Exists( Path.Combine( directoryInfo.FullName, "Rock.sln" ) ) )
                {
                    return directoryInfo.FullName;
                }

                directoryInfo = directoryInfo.Parent;
            }

            return null;
        }

        private static void EnsureDirectoryExists( string path )
        {
            var directories = new Stack<DirectoryInfo>();

            var directory = new DirectoryInfo( path );

            while ( directory != null && !directory.Exists )
            {
                directories.Push( directory );
                directory = directory.Parent;
            }

            while ( directories.Count > 0 )
            {
                directory = directories.Pop();
                directory.Create();
            }
        }

        private void SyncDiffButtonState()
        {
            ContentButton.Background = _selectedDiffIndex == 0 ? _selectedButtonBackground : _unselectedButtonBackground;
            ContentButton.Foreground = _selectedDiffIndex == 0 ? _selectedButtonForeground : _unselectedButtonForeground;

            UnifiedDiffButton.Background = _selectedDiffIndex == 1 ? _selectedButtonBackground : _unselectedButtonBackground;
            UnifiedDiffButton.Foreground = _selectedDiffIndex == 1 ? _selectedButtonForeground : _unselectedButtonForeground;

            SideBySideDiffButton.Background = _selectedDiffIndex == 2 ? _selectedButtonBackground : _unselectedButtonBackground;
            SideBySideDiffButton.Foreground = _selectedDiffIndex == 2 ? _selectedButtonForeground : _unselectedButtonForeground;

            FilePreviewContent.Visibility = _selectedDiffIndex == 0 ? Visibility.Visible : Visibility.Hidden;
            FilePreviewDiffView.Visibility = _selectedDiffIndex != 0 ? Visibility.Visible : Visibility.Hidden;
            FilePreviewDiffView.IsSideBySide = _selectedDiffIndex == 2;
        }

        private void Save_Click( object sender, RoutedEventArgs e )
        {
            var solutionPath = GetSolutionPath();

            if ( solutionPath.IsNullOrWhiteSpace() )
            {
                MessageBox.Show( Window.GetWindow( this ), "Unable to determine solution path." );
                return;
            }

            var files = _exportFiles
                .Where( f => f.IsExporting )
                .Select( f => f.File )
                .ToList();

            var failedFiles = new List<GeneratedFile>();

            foreach ( var file in files )
            {
                try
                {
                    var filePath = Path.Combine( solutionPath, file.SolutionRelativePath );

                    EnsureDirectoryExists( Path.GetDirectoryName( filePath ) );

                    File.WriteAllText( filePath, file.Content );
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( $"Error processing file '{file.SolutionRelativePath}': {ex.Message}" );
                    failedFiles.Add( file );
                }
            }

            if ( failedFiles.Any() )
            {
                var errorMessage = $"The following files had errors:\n{string.Join( "\n", failedFiles.Select( f => f.SolutionRelativePath ) )}";
                MessageBox.Show( Window.GetWindow( this ), errorMessage, "Some files failed to process." );
            }
            else
            {
                MessageBox.Show( "Selected files have been created or updated." );
            }
        }

        private void ContentButton_Click( object sender, RoutedEventArgs e )
        {
            _selectedDiffIndex = 0;
            SyncDiffButtonState();
        }

        private void UnifiedDiffButton_Click( object sender, RoutedEventArgs e )
        {
            _selectedDiffIndex = 1;
            SyncDiffButtonState();
        }

        private void SideBySideDiffButton_Click( object sender, RoutedEventArgs e )
        {
            _selectedDiffIndex = 2;
            SyncDiffButtonState();
        }

        private void FileListBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if ( FileListBox.SelectedItem is ExportFile exportFile )
            {
                FilePreviewDiffView.OldText = exportFile.OldContent;
                FilePreviewDiffView.NewText = exportFile.File.Content;
                FilePreviewContent.Text = exportFile.File.Content;
                FilePreviewContent.ScrollToHome();
                FilePreviewPath.Text = $"Path: {exportFile.File.SolutionRelativePath}";
            }
            else
            {
                FilePreviewDiffView.OldText = string.Empty;
                FilePreviewDiffView.NewText = string.Empty;
                FilePreviewContent.Text = string.Empty;
                FilePreviewPath.Text = string.Empty;
            }
        }

        private class ExportFile : IComparable
        {
            public bool IsExporting { get; set; }

            public bool IsWriteNeeded { get; set; }

            public bool IsUpToDate { get; set; }

            public GeneratedFile File { get; set; }

            public string OldContent { get; set; } = string.Empty;

            public ExportFile( GeneratedFile file )
            {
                IsExporting = true;
                File = file;
            }

            public int CompareTo( object obj )
            {
                return File.FileName.CompareTo( obj );
            }
        }
    }
}
