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
    /// Interaction logic for ObsidianViewModelsPage.xaml
    /// </summary>
    public partial class ObsidianViewModelsPage : Page
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObsidianViewModelsPage"/> class.
        /// </summary>
        public ObsidianViewModelsPage()
        {
            InitializeComponent();

            // Check if the Rock.ViewModels DLL is up to date or if it needs
            // to be built.
            RockViewModelsOutOfDateAlert.Visibility = SupportTools.IsSourceNewer( typeof( Rock.ViewModels.Utility.ListItemBag ).Assembly.Location, "Rock.ViewModels" )
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Get the view model types and the items to represent them.
            var types = GetViewModelTypes();
            var typeItems = types.Select( t => new TypeItem( t ) ).ToList();

            // Check for any type that is invalid.
            foreach ( var item in typeItems )
            {
                var unsupported = GetUnsupportedProperties( item.Type, types );

                if ( unsupported.Any() )
                {
                    item.InvalidReason = $"The following properties are not supported: {string.Join( ", ", unsupported.Select( p => p.Name ) )}";
                }
            }

            // Sort the items so taht invalid ones are at the top.
            typeItems = typeItems.OrderByDescending( t => t.IsInvalid )
                .ThenBy( t => t.Name )
                .ToList();

            ViewModelsListBox.ItemsSource = typeItems;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the unsupported properties that exist on the type.
        /// </summary>
        /// <param name="type">The type to be checked for unsupported properties.</param>
        /// <param name="validTypes">The additional types that will be considered as valid types.</param>
        /// <returns>A collection of properties that are not supported.</returns>
        private List<PropertyInfo> GetUnsupportedProperties( Type type, IList<Type> validTypes )
        {
            return type.GetProperties()
                .Where( p => !IsSupportedPropertyType( p.PropertyType, validTypes ) )
                .ToList();
        }

        /// <summary>
        /// Determines whether the type is supported as a property type in
        /// a view model.
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <param name="validTypes">The additional types that are considered valid.</param>
        /// <returns><c>true</c> if the type is supported; otherwise, <c>false</c>.</returns>
        private bool IsSupportedPropertyType( Type type, IList<Type> validTypes )
        {
            // Check if it is a well known supported property type.
            if ( EntityProperty.IsSupportedPropertyType( type ) )
            {
                return true;
            }

            // Check if type is one of the types that is known to be supported.
            if ( validTypes.Contains( type ) )
            {
                return true;
            }

            // If type is in the Rock.Enums assembly, it's supported.
            var underlyingType = Nullable.GetUnderlyingType( type ) ?? type;
            if ( underlyingType.IsEnum && underlyingType.Assembly == typeof( Enums.Reporting.FieldFilterSourceType ).Assembly )
            {
                return true;
            }

            // Check for some generic types that are supported.
            if ( type.IsGenericType )
            {
                if ( type.GetGenericTypeDefinition() == typeof( Dictionary<,> ) )
                {
                    if ( type.GetGenericArguments()[0] != typeof( string ) && type.GetGenericArguments()[0] != typeof( Guid ) )
                    {
                        return false;
                    }

                    return IsSupportedPropertyType( type.GetGenericArguments()[1], validTypes );
                }
            }

            // Named generic parameters are supported.
            if ( type.IsGenericParameter )
            {
                return true;
            }

            // Otherwise, we will accept a generic object.
            return type == typeof( object );
        }

        /// <summary>
        /// Gets the view model types that should be considered as available
        /// options for exporting.
        /// </summary>
        /// <returns>A collection of types that represent the view models.</returns>
        private List<Type> GetViewModelTypes()
        {
            // We only include types that end in "Bag" or "Box".
            return typeof( Rock.ViewModels.Utility.EntityBagBase ).Assembly
                .GetExportedTypes()
                .Where( t => t.Name.Split( '`' )[0].EndsWith( "Bag" ) || t.Name.Split( '`' )[0].EndsWith( "Box" ) )
                .Where( t => !t.IsAbstract && !t.IsInterface )
                .ToList();
        }

        /// <summary>
        /// Gets the path to the directory that the type file will be placed in.
        /// </summary>
        /// <param name="type">The type to be exported.</param>
        /// <returns>A string that represents the directory path.</returns>
        private string GetPathForType( Type type )
        {
            var components = type.Namespace.Replace( "Rock.ViewModels", string.Empty ).Trim( '.' ).Split( '.' );

            return Path.Combine( "Rock.JavaScript.Obsidian", "Framework", "ViewModels", string.Join( "\\", components ) );
        }

        /// <summary>
        /// Gets the file name to use for the type.
        /// </summary>
        /// <param name="type">The type to be exported.</param>
        /// <returns>A string that represents the file name.</returns>
        private string GetFileNameForType( Type type )
        {
            return $"{type.Name.Split( '`' )[0].CamelCase()}.d.ts";
        }

        /// <summary>
        /// Gets the selected types to be exported.
        /// </summary>
        /// <returns>A collection of types that should be exported.</returns>
        private IList<Type> GetSelectedTypes()
        {
            return ViewModelsListBox.ItemsSource
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

                        if ( filename.EndsWith( ".cs" ) || filename.EndsWith( ".ts" ) )
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
            ViewModelsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( t => !t.IsInvalid )
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
            ViewModelsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( t => !t.IsInvalid )
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
                    // Generate each file that was selected to be exported.
                    var generator = new TypeScriptViewModelGenerator();
                    foreach ( var type in GetSelectedTypes() )
                    {
                        var source = generator.GenerateViewModelForType( type );
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
        /// An item that represents a type to be shown in the list box.
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
            /// Gets the type represented by this item.
            /// </summary>
            /// <value>The type represented by this item.</value>
            public Type Type { get; }

            /// <summary>
            /// Gets the name to display in the listbox.
            /// </summary>
            /// <value>The name to display in the listbox.</value>
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
            /// <param name="propertyName">Name of the property.</param>
            protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
            {
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
            }

            #endregion
        }

        #endregion
    }
}
