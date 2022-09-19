﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Rock;
using Rock.CodeGeneration.Utility;

namespace Rock.CodeGeneration.Pages
{
    /// <summary>
    /// Interaction logic for GeneratedFilePreview.xaml
    /// </summary>
    public partial class GeneratedFilePreviewPage : Page
    {
        #region Fields

        /// <summary>
        /// The files that will be displayed to the user to determine if they
        /// should be saved to disk or not.
        /// </summary>
        private readonly List<ExportFile> _exportFiles;

        /// <summary>
        /// The selected button index for how to view the file changes.
        /// </summary>
        private int _selectedDiffIndex = 0;

        /// <summary>
        /// The unselected button background for the button group.
        /// </summary>
        private readonly Brush _unselectedButtonBackground = new SolidColorBrush( Color.FromRgb( 0xdd, 0xdd, 0xdd ) );

        /// <summary>
        /// The unselected button foreground for the button group.
        /// </summary>
        private readonly Brush _unselectedButtonForeground = new SolidColorBrush( Color.FromRgb( 0, 0, 0 ) );

        /// <summary>
        /// The selected button background for the button group.
        /// </summary>
        private readonly Brush _selectedButtonBackground = new SolidColorBrush( Color.FromRgb( 0xee, 0x77, 0x25 ) );

        /// <summary>
        /// The selected button foreground for the button group.
        /// </summary>
        private readonly Brush _selectedButtonForeground = new SolidColorBrush( Color.FromRgb( 0xf0, 0xf0, 0xf0 ) );

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the post save action handler.
        /// </summary>
        /// <value>The post save action handler.</value>
        public Action<IReadOnlyList<GeneratedFile>, PostSaveContext> PostSaveAction { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedFilePreviewPage"/> class.
        /// </summary>
        /// <param name="files">The files that were generated and should be considered for exporting.</param>
        public GeneratedFilePreviewPage( IList<GeneratedFile> files )
        {
            InitializeComponent();

            SyncDiffButtonState();

            _exportFiles = files.Select( f => new ExportFile( f ) ).ToList();

            _exportFiles.ForEach( UpdateExportFile );

            // Order the files so that those that need to be written are on top
            // and then order by the filename.
            _exportFiles = _exportFiles.OrderByDescending( f => f.IsWriteNeeded )
                .ThenBy( f => f.File.FileName )
                .ToList();

            FileListBox.ItemsSource = _exportFiles;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the export file properties to match the current state of
        /// the filesystem.
        /// </summary>
        /// <param name="file">The file to be updated.</param>
        private static void UpdateExportFile( ExportFile file )
        {
            var solutionPath = SupportTools.GetSolutionPath();

            // Check if the file needs to be written.
            if ( solutionPath.IsNotNullOrWhiteSpace() )
            {
                var filePath = Path.Combine( solutionPath, file.File.SolutionRelativePath );

                // If the file exists, compare the contents to see if it needs
                // to be written.
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

        /// <summary>
        /// Ensures the directory exists that make up the path. If any do not
        /// exist then they are created.
        /// </summary>
        /// <param name="path">The path.</param>
        private static void EnsureDirectoryExists( string path )
        {
            var directories = new Stack<DirectoryInfo>();

            var directory = new DirectoryInfo( path );

            // Start at the bottom of the path tree and find any that don't
            // exist then add them to a stack to be created.
            while ( directory != null && !directory.Exists )
            {
                directories.Push( directory );
                directory = directory.Parent;
            }

            // Create any directories that were missing.
            while ( directories.Count > 0 )
            {
                directory = directories.Pop();
                directory.Create();
            }
        }

        /// <summary>
        /// Synchronizes the state of the button group for viewing file differences.
        /// </summary>
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

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the Save control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Save_Click( object sender, RoutedEventArgs e )
        {
            var button = sender as Button;
            var solutionPath = SupportTools.GetSolutionPath();

            if ( solutionPath.IsNullOrWhiteSpace() )
            {
                MessageBox.Show( Window.GetWindow( this ), "Unable to determine solution path." );
                return;
            }

            button.IsEnabled = false;

            try
            {
                SaveProgressBar.Maximum = _exportFiles.Count;
                SaveProgressBar.Value = 0;
                SaveProgressBar.IsIndeterminate = false;
                SaveProgressBar.Visibility = Visibility.Visible;

                await Task.Run( () =>
                {
                    // Attempt to write each file to disk. If something goes wrong
                    // then add it to the list of failed files.
                    foreach ( var file in _exportFiles )
                    {
                        if ( !file.IsExporting )
                        {
                            file.File.SaveState = GeneratedFileSaveState.NoChange;
                            Dispatcher.Invoke( () => SaveProgressBar.Value += 1 );
                            continue;
                        }

                        try
                        {
                            var filePath = Path.Combine( solutionPath, file.File.SolutionRelativePath );

                            EnsureDirectoryExists( Path.GetDirectoryName( filePath ) );
                            var exists = File.Exists( filePath );

                            File.WriteAllText( filePath, file.File.Content );

                            file.File.SaveState = exists ? GeneratedFileSaveState.Updated : GeneratedFileSaveState.Created;
                        }
                        catch ( Exception ex )
                        {
                            System.Diagnostics.Debug.WriteLine( $"Error processing file '{file.File.SolutionRelativePath}': {ex.Message}" );
                            file.File.SaveState = GeneratedFileSaveState.Failed;
                        }

                        Dispatcher.Invoke( () => SaveProgressBar.Value += 1 );
                    }

                    // Process any post-save action requested by the owner.
                    Dispatcher.Invoke( () => SaveProgressBar.IsIndeterminate = true );
                    var context = new PostSaveContext( this );
                    PostSaveAction?.Invoke( _exportFiles.Select( f => f.File ).ToList(), context );
                } );

                var failedFiles = _exportFiles.Where( f => f.File.SaveState == GeneratedFileSaveState.Failed );

                // If any files had errors, display which ones.
                if ( failedFiles.Any() )
                {
                    var errorMessage = $"The following files had errors:\n{string.Join( "\n", failedFiles.Select( f => f.File.SolutionRelativePath ) )}";
                    MessageBox.Show( Window.GetWindow( this ), errorMessage, "Some files failed to process." );
                }
                else
                {
                    MessageBox.Show( "Selected files have been created or updated.", "Files Saved" );
                }
            }
            finally
            {
                SaveProgressBar.Visibility = Visibility.Collapsed;
                button.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the ContentButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ContentButton_Click( object sender, RoutedEventArgs e )
        {
            _selectedDiffIndex = 0;
            SyncDiffButtonState();
        }

        /// <summary>
        /// Handles the Click event of the UnifiedDiffButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UnifiedDiffButton_Click( object sender, RoutedEventArgs e )
        {
            _selectedDiffIndex = 1;
            SyncDiffButtonState();
        }

        /// <summary>
        /// Handles the Click event of the SideBySideDiffButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SideBySideDiffButton_Click( object sender, RoutedEventArgs e )
        {
            _selectedDiffIndex = 2;
            SyncDiffButtonState();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the FileListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void FileListBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            // If the selected item is an export file, then fill in all the
            // details so they can preview the file.
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

        #endregion

        #region Support Classes

        /// <summary>
        /// Wraps the generated file in a class that can be used in the list box
        /// to display more detailed information about the file.
        /// </summary>
        private class ExportFile
        {
            #region Properties

            /// <summary>
            /// Gets or sets a value indicating whether this file will be exported.
            /// </summary>
            /// <value><c>true</c> if this file will be exported; otherwise, <c>false</c>.</value>
            public bool IsExporting { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether file needs to be written.
            /// </summary>
            /// <value><c>true</c> if this file needs to be written; otherwise, <c>false</c>.</value>
            public bool IsWriteNeeded { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this file is up to date.
            /// </summary>
            /// <value><c>true</c> if this file is up to date; otherwise, <c>false</c>.</value>
            public bool IsUpToDate { get; set; }

            /// <summary>
            /// Gets or sets the file that was generated.
            /// </summary>
            /// <value>The file that was generated.</value>
            public GeneratedFile File { get; set; }

            /// <summary>
            /// Gets or sets the old content of the existing file, if any.
            /// </summary>
            /// <value>The old content of the existing file, if any.</value>
            public string OldContent { get; set; } = string.Empty;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ExportFile"/> class.
            /// </summary>
            /// <param name="file">The file that was generated.</param>
            public ExportFile( GeneratedFile file )
            {
                IsExporting = true;
                File = file;
            }

            #endregion
        }

        /// <summary>
        /// Provides state context to post save actions.
        /// </summary>
        public class PostSaveContext : IDisposable
        {
            /// <summary>
            /// The page that owns this context.
            /// </summary>
            private GeneratedFilePreviewPage _page;

            /// <summary>
            /// Creates a new instance of <see cref="PostSaveContext"/> class.
            /// </summary>
            /// <param name="page">The page that will own this context.</param>
            public PostSaveContext( GeneratedFilePreviewPage page )
            {
                _page = page;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing,
            /// releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _page = null;
            }

            /// <summary>
            /// Sets the progress of the post save action.
            /// </summary>
            /// <param name="current">The current step that was completed.</param>
            /// <param name="total">The total number of steps to be completed.</param>
            public void SetProgress( int current, int total )
            {
                _page.Dispatcher.Invoke( () =>
                {
                    _page.SaveProgressBar.Maximum = total;
                    _page.SaveProgressBar.Value = current;
                    _page.SaveProgressBar.IsIndeterminate = false;
                } );
            }

            /// <summary>
            /// Shows the message to the user and waits for it to be dismissed
            /// before returning.
            /// </summary>
            /// <param name="title">The title of the alert message.</param>
            /// <param name="message">The message body text.</param>
            public void ShowMessage( string title, string message )
            {
                _page.Dispatcher.Invoke( () =>
                {
                    MessageBox.Show( Window.GetWindow( _page ), message, title );
                } );
            }
        }

        #endregion
    }
}
