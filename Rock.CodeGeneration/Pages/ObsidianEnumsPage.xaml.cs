using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Rock.CodeGeneration.FileGenerators;
using Rock.CodeGeneration.Utility;

namespace Rock.CodeGeneration.Pages
{
    /// <summary>
    /// Interaction logic for ObsidianEnumsPage.xaml
    /// </summary>
    public partial class ObsidianEnumsPage : Page
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObsidianEnumsPage"/> class.
        /// </summary>
        public ObsidianEnumsPage()
        {
            InitializeComponent();

            // Check if the Rock.Enums DLL is up to date or if it needs
            // to be built.
            RockEnumsOutOfDateAlert.Visibility = SupportTools.IsSourceNewer( typeof( Model.Gender ).Assembly.Location, "Rock.Enums" )
                ? Visibility.Visible
                : Visibility.Collapsed;

            var typeItems = GetEnumTypes()
                .Select( t => new TypeItem( t ) )
                .ToList();

            foreach ( var item in typeItems )
            {
                // An enum is considered unsupported if it isn't in the Rock.Enums
                // namespace and does not have a EnumDomainAttribute applied to it.
                var unsupported = !item.Type.FullName.StartsWith( "Rock.Enums." )
                    && item.Type.GetCustomAttributes().FirstOrDefault( a => a.GetType().FullName == "Rock.Enums.EnumDomainAttribute" ) == null;

                if ( unsupported )
                {
                    item.IsExporting = false;
                    item.InvalidReason = $"This enum is not in the correct namespace and cannot be exported.";
                }
            }

            // Sort the items so that invalid items are at the top.
            typeItems = typeItems
                .OrderBy( t => !t.IsInvalid )
                .ThenBy( t => t.Name )
                .ToList();

            EnumsListBox.ItemsSource = typeItems;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the enum types that should be considered for exporting.
        /// </summary>
        /// <returns>A collection of Type objects that represent the enums.</returns>
        private List<Type> GetEnumTypes()
        {
            return typeof( Enums.Reporting.FieldFilterSourceType ).Assembly
                .GetExportedTypes()
                .Where( t => t.IsEnum )
                .ToList();
        }

        /// <summary>
        /// Gets the path to use for where the file should be written.
        /// </summary>
        /// <param name="type">The type that will be written to a file.</param>
        /// <returns>A string that represents the directory that will contain the file.</returns>
        /// <exception cref="Exception">Attempt to export an enum with an invalid namespace, this shouldn't happen.</exception>
        private string GetPathForType( Type type )
        {
            // If the type isn't in the Rock.Enums namespace then use the
            // EnumDomain attribute to determine the actual domain it's in.
            if ( !type.Namespace.StartsWith( "Rock.Enums" ) )
            {
                var domainAttribute = type.GetCustomAttributes()
                    .FirstOrDefault( a => a.GetType().FullName == "Rock.Enums.EnumDomainAttribute" );

                if ( domainAttribute == null )
                {
                    throw new Exception( "Attempt to export an enum with an invalid namespace, this shouldn't happen." );
                }

                var domain = ( string ) domainAttribute.GetType().GetProperty( "Domain" ).GetValue( domainAttribute );
                domain = SupportTools.GetDomainFolderName( domain );

                return Path.Combine( "Rock.JavaScript.Obsidian", "Framework", "Enums", domain );
            }

            var components = type.Namespace.Replace( "Rock.Enums", string.Empty ).Trim( '.' ).Split( '.' );

            return Path.Combine( "Rock.JavaScript.Obsidian", "Framework", "Enums", string.Join( "\\", components ) );
        }

        /// <summary>
        /// Gets the file name that should be used when writing the type.
        /// </summary>
        /// <param name="type">The type to be written.</param>
        /// <returns>A string that represents the file name.</returns>
        private string GetFileNameForType( Type type )
        {
            var name = type.Name.Split( '`' )[0];

            // If the type name is all CAPS then just convert the whole thing
            // to lowercase. Otherwise convert to camelCase.
            if ( name.All( c => char.IsUpper( c ) ) )
            {
                name = name.ToLower();
            }
            else
            {
                name = name.CamelCase();
            }

            return $"{name}.ts";
        }

        /// <summary>
        /// Gets the selected types.
        /// </summary>
        /// <returns>A collection of types that have been selected for export.</returns>
        private IList<Type> GetSelectedTypes()
        {
            return EnumsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( t => t.IsExporting )
                .Select( t => t.Type )
                .ToList();
        }

        /// <summary>
        /// Performs post-processing of any exported files.
        /// </summary>
        /// <param name="files">The files that were listed to be possibly exported.</param>
        /// <param name="context">The context to provide status information back.</param>
        private void ProcessPostSaveFiles( IReadOnlyList<GeneratedFile> files, GeneratedFilePreviewPage.PostSaveContext context )
        {
            var createdFiles = files.Where( f => f.SaveState == GeneratedFileSaveState.Created ).ToList();

            if ( !createdFiles.Any() )
            {
                return;
            }

            var solutionPath = SupportTools.GetSolutionPath();
            var solutionFileName = Path.Combine( solutionPath, "Rock.sln" );

            if ( solutionFileName == null )
            {
                context.ShowMessage( "Cannot update projects", "Could not determine the solution path to update the project files." );
                return;
            }

            try
            {
                using ( var solution = SolutionHelper.LoadSolution( solutionFileName ) )
                {
                    // Loop through each created file and make sure it exists in
                    // the appropriate project file.
                    for ( int i = 0; i < createdFiles.Count; i++ )
                    {
                        var file = createdFiles[i];
                        var filename = Path.Combine( solutionPath, file.SolutionRelativePath );

                        if ( filename.EndsWith( ".cs" ) )
                        {
                            var projectName = file.SolutionRelativePath.Split( '\\' )[0];

                            solution.AddCompileFileToProject( projectName, filename );
                        }

                        context.SetProgress( i + 1, createdFiles.Count );
                    }

                    solution.Save();
                }
            }
            catch ( Exception ex )
            {
                context.ShowMessage( "Failed to update projects", $"Unable to add one or more files to projects. Please check to make sure everything has been added.\n{ex.Message}" );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the SelectAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SelectAll_Click( object sender, RoutedEventArgs e )
        {
            EnumsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( i => !i.IsInvalid )
                .ToList()
                .ForEach( i => i.IsExporting = true );
        }

        /// <summary>
        /// Handles the Click event of the SelectNone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SelectNone_Click( object sender, RoutedEventArgs e )
        {
            EnumsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( i => !i.IsInvalid )
                .ToList()
                .ForEach( i => i.IsExporting = false );
        }

        /// <summary>
        /// Handles the Click event of the Preview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Preview_Click( object sender, RoutedEventArgs e )
        {
            var button = sender as Button;

            button.IsEnabled = false;

            try
            {
                var selectedTypes = GetSelectedTypes();
                var files = new List<GeneratedFile>();

                PreviewProgressBar.Maximum = selectedTypes.Count;
                PreviewProgressBar.Value = 0;
                PreviewProgressBar.IsIndeterminate = false;
                PreviewProgressBar.Visibility = Visibility.Visible;

                await Task.Run( () =>
                {
                    // Generate the file for each selected enum type.
                    var generator = new TypeScriptViewModelGenerator();
                    foreach ( var type in GetSelectedTypes() )
                    {
                        var source = generator.GenerateViewModelForEnum( type );
                        files.Add( new GeneratedFile( GetFileNameForType( type ), GetPathForType( type ), source ) );

                        Dispatcher.Invoke( () => PreviewProgressBar.Value += 1 );
                    }
                } );

                var previewPage = new GeneratedFilePreviewPage( files )
                {
                    PostSaveAction = ProcessPostSaveFiles
                };

                await this.Navigation().PushPageAsync( previewPage );
            }
            finally
            {
                PreviewProgressBar.Visibility = Visibility.Collapsed;
                button.IsEnabled = true;
            }
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// An item that will be used to represent the type in the listbox.
        /// </summary>
        private class TypeItem : INotifyPropertyChanged
        {
            #region Events

            /// <summary>
            /// Occurs when a property value changes.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            #region Properties

            /// <summary>
            /// Gets or sets the type represented by this item.
            /// </summary>
            /// <value>The type represented by this item.</value>
            public Type Type { get; set; }

            /// <summary>
            /// Gets or sets the name to display for this type.
            /// </summary>
            /// <value>The name to display for this type.</value>
            public string Name { get; }

            /// <summary>
            /// Gets or sets a value indicating whether this item is selected for export.
            /// </summary>
            /// <value><c>true</c> if this item is selected for export; otherwise, <c>false</c>.</value>
            public bool IsExporting
            {
                get => _isExporting;
                set
                {
                    _isExporting = value;
                    OnPropertyChanged();
                }
            }
            private bool _isExporting;

            /// <summary>
            /// Gets a value indicating whether this item is invalid.
            /// </summary>
            /// <value><c>true</c> if this item is invalid; otherwise, <c>false</c>.</value>
            public bool IsInvalid => InvalidReason.IsNotNullOrWhiteSpace();

            /// <summary>
            /// Gets or sets the reason this item is invalid.
            /// </summary>
            /// <value>The reason this item is invalid.</value>
            public string InvalidReason { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeItem"/> class.
            /// </summary>
            /// <param name="type">The type to be represented by this item.</param>
            public TypeItem( Type type )
            {
                Type = type;
                Name = type.Name;
                IsExporting = true;

                if ( Name.StartsWith( "Rock.ViewModels." ) )
                {
                    Name = Name.Substring( 15 );
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called when a property value has changed.
            /// </summary>
            /// <param name="propertyName">The name of the property.</param>
            protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
            {
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
            }

            #endregion
        }

        #endregion
    }
}
