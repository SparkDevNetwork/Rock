using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

using Rock.CodeGeneration.Dialogs;
using Rock.CodeGeneration.FileGenerators;
using Rock.CodeGeneration.Lava;
using Rock.CodeGeneration.Utility;

using PropertyItem = Rock.CodeGeneration.Pages.ObsidianDetailBlockPage.PropertyItem;

namespace Rock.CodeGeneration.Pages
{
    /// <summary>
    /// Interaction logic for ObsidianListBlockPage.xaml
    /// </summary>
    public partial class ObsidianListBlockPage : Page
    {
        #region Fields

        /// <summary>
        /// The currently selected entity type in the UI.
        /// </summary>
        private Type _selectedEntityType;

        /// <summary>
        /// The properties that are known on the entity.
        /// </summary>
        private List<PropertyItem> _entityProperties;

        /// <summary>
        /// The properties that are valid on columns.
        /// </summary>
        private List<PropertyItem> _columnProperties;

        /// <summary>
        /// The columns to be included in the list block.
        /// </summary>
        private IList<ColumnItem> _columnItems = new ObservableCollection<ColumnItem>();

        /// <summary>
        /// The properties that are considered system level and always excluded.
        /// </summary>
        private static readonly string[] _systemProperties = new[]
        {
            "Attributes",
            "AttributeValues",
            "Guid",
            "Id"
        };

        /// <summary>
        /// The properties that are considered advanced and will only show up
        /// if they are specifically requested.
        /// </summary>
        private static readonly string[] _advancedProperties = new[]
        {
            "CreatedByPersonAlias",
            "CreatedByPersonAliasId",
            "CreatedDateTime",
            "ForeignGuid",
            "ForeignId",
            "ForeignKey",
            "ModifiedByPersonAlias",
            "ModifiedByPersonAliasId",
            "ModifiedDateTime"
        };

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObsidianListBlockPage"/> class.
        /// </summary>
        public ObsidianListBlockPage()
        {
            InitializeComponent();

            // Check if the Rock project is out of date and needs to be built.
            RockOutOfDateAlert.Visibility = SupportTools.IsSourceNewer( typeof( Rock.Data.IEntity ).Assembly.Location, "Rock" )
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the currently selected entity.
        /// </summary>
        /// <param name="entityType">Type of the entity that will now be selected.</param>
        private void SetEntity( Type entityType )
        {
            _selectedEntityType = entityType;

            SelectedEntityName.Content = _selectedEntityType?.FullName ?? string.Empty;
            UpdateEntityProperties( true );

            // Set default for Use Attribute Values.
            UseAttributeValuesCheckBox.IsChecked = typeof( Rock.Attribute.IHasAttributes ).IsAssignableFrom( entityType );
            UseAttributeValuesCheckBox.IsEnabled = UseAttributeValuesCheckBox.IsChecked ?? false;

            // Set default for Security From.
            if ( typeof( Rock.Security.ISecured ).IsAssignableFrom( entityType ) )
            {
                SecurityFromEntityRadioButton.IsChecked = true;
            }
            else
            {
                SecurityFromCmsRadioButton.IsChecked = true;
            }

            // Set default for Show Reorder.
            ShowReorder.IsChecked = typeof( Rock.Data.IOrdered ).IsAssignableFrom( entityType );
            ShowReorder.IsEnabled = ShowReorder.IsChecked ?? false;

            // Set default for Show Security.
            ShowSecurity.IsChecked = typeof( Rock.Security.ISecured ).IsAssignableFrom( entityType );
            ShowSecurity.IsEnabled = ShowSecurity.IsChecked ?? false;

            // Set default for Show Delete.
            ShowDelete.IsChecked = true;
        }

        /// <summary>
        /// Updates the list of entity properties for the selected entity.
        /// </summary>
        private void UpdateEntityProperties( bool resetColumns )
        {
            _entityProperties = GetEntityProperties( _selectedEntityType )
                .Where( p => IsValidPropertyType( p ) )
                .Select( p => new PropertyItem( p ) )
                .ToList();

            _columnProperties = GetColumnProperties( _entityProperties ).ToList();

            var stringProperties = _entityProperties
                .Where( p => p.Property.PropertyType == typeof( string ) )
                .OrderBy( p => p.Name )
                .Select( p => p.Name )
                .ToList();

            stringProperties.Insert( 0, string.Empty );

            ToolTipSource.ItemsSource = stringProperties;

            if ( resetColumns )
            {
                ColumnsListBox.ItemsSource = _columnItems = new ObservableCollection<ColumnItem>();
                ToolTipSource.Text = string.Empty;
                SkeletonCount.Text = "";
            }
            else
            {
                foreach ( var columnItem in _columnItems )
                {
                    columnItem.ValidNames = _columnProperties.Select( p => p.Name );
                }
            }
        }

        /// <summary>
        /// Gets the properties that exist on the specified entity type. This automatically
        /// handles filtering out system and advanced properties.
        /// </summary>
        /// <param name="entityType">Type of the entity whose properties are to be enumerated.</param>
        /// <returns>An enumeration of the valid properties that match the filtering options.</returns>
        private IEnumerable<PropertyInfo> GetProperties( Type entityType )
        {
            var properties = entityType?.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                .Where( p => p.GetCustomAttribute<DataMemberAttribute>() != null || typeof( Data.IEntity ).IsAssignableFrom( p.PropertyType ) )
                .Where( p => p.GetCustomAttribute<NotMappedAttribute>() == null )
                .Where( p => !_systemProperties.Contains( p.Name ) )
                .Where( p => ShowAdvancedPropertiesCheckBox.IsChecked == true || !_advancedProperties.Contains( p.Name ) )
                .OrderBy( p => p.Name )
                .ToList()
                ?? new List<PropertyInfo>();

            // Filter out any EntityId properties if we have a navigation
            // property to the entity.
            if ( ShowAdvancedPropertiesCheckBox.IsChecked != true )
            {
                properties = properties
                    .Where( p => !p.Name.EndsWith( "Id" )
                        || !properties.Any( p2 => p2.Name == p.Name.Substring( 0, p.Name.Length - 2 ) ) )
                    .ToList();
            }

            return properties;
        }

        /// <summary>
        /// Gets the properties that are valid to use when generating columns.
        /// </summary>
        /// <param name="properties">The properties that exist on the entity.</param>
        /// <returns>A set of <see cref="PropertyItem"/> objects.</returns>
        private IEnumerable<PropertyItem> GetColumnProperties( IEnumerable<PropertyItem> properties )
        {
            return properties
                .Where( p => p.Property.PropertyType == typeof( string )
                    || p.Property.PropertyType == typeof( Guid ) || p.Property.PropertyType == typeof( Guid? )
                    || p.Property.PropertyType == typeof( bool ) || p.Property.PropertyType == typeof( bool? )
                    || p.Property.PropertyType == typeof( int ) || p.Property.PropertyType == typeof( int? )
                    || p.Property.PropertyType == typeof( decimal ) || p.Property.PropertyType == typeof( decimal? )
                    || p.Property.PropertyType == typeof( float ) || p.Property.PropertyType == typeof( float? )
                    || p.Property.PropertyType == typeof( double ) || p.Property.PropertyType == typeof( double? )
                    || typeof(Rock.Data.IEntity).IsAssignableFrom( p.Property.PropertyType ) );
        }

        /// <summary>
        /// Gets all the entity properties without applying any filtering.
        /// </summary>
        /// <param name="entityType">Type of the entity whose properties should be enumerated.</param>
        /// <returns>An enumeration of all the entity's properties.</returns>
        private IEnumerable<PropertyInfo> GetEntityProperties( Type entityType )
        {
            return GetProperties( entityType )
                .OrderBy( p => p.Name )
                .ToList();
        }

        /// <summary>
        /// Determines if the property has a valid type that can be used during
        /// automatic code generation.
        /// </summary>
        /// <param name="property">The property to be validated.</param>
        /// <returns><c>true</c> if the property type is known and valid, <c>false</c> otherwise.</returns>
        private static bool IsValidPropertyType( PropertyInfo property )
        {
            return EntityColumn.IsSupportedPropertyType( property.PropertyType );
        }

        /// <summary>
        /// Generates all the files required to make a skeleton list block
        /// for the selected entity type.
        /// </summary>
        /// <param name="options">The options that describe all the options to use when generating code.</param>
        /// <returns>A collection of <see cref="GeneratedFile"/> objects that represent the files to be created or updated.</returns>
        private IList<GeneratedFile> GenerateFiles( ListBlockOptions options )
        {
            var files = new List<GeneratedFile>();
            var domain = options.EntityType.GetCustomAttribute<Data.RockDomainAttribute>()?.Name ?? "Unknown";
            var domainNamespace = SupportTools.GetDomainFolderName( domain );
            var bagPath = $"Rock.ViewModels\\Blocks\\{domainNamespace}\\{options.EntityType.Name}List";
            var blockPath = $"Rock.Blocks\\{domainNamespace}";
            var typeScriptBlockPath = $"Rock.JavaScript.Obsidian.Blocks\\src\\{domainNamespace}";
            var bagNamespace = $"Rock.ViewModels.Blocks.{domainNamespace}.{options.EntityType.Name}List";
            var generator = new CSharpViewModelGenerator();
            var tsGenerator = new TypeScriptViewModelGenerator();

            // Create the standard merge fields that will be used by the Lava engine
            // when generating all the files.
            var mergeFields = new Dictionary<string, object>
            {
                ["EntityName"] = options.EntityType.Name,
                ["ServiceName"] = options.ServiceType.Name,
                ["Domain"] = domain,
                ["DomainNamespace"] = domainNamespace,
                ["Columns"] = options.Columns,
                ["GridImports"] = options.Columns.SelectMany( c => c.GridImports ).Distinct().OrderBy( i => i ).ToList(),
                ["UseIsSystem"] = options.EntityType.GetProperty( "IsSystem" ) != null,
                ["UseAttributeValues"] = options.UseAttributeValues,
                ["UseEntitySecurity"] = options.UseEntitySecurity,
                ["ToolTipSource"] = options.ToolTipSource,
                ["ShowReorder"] = options.ShowReorder,
                ["ShowSecurity"] = options.ShowSecurity,
                ["ShowDelete"] = options.ShowDelete,
                ["ExpectedRowCount"] = options.ExpectedRowCount
            };

            // Generate the <Entity>ListOptionsBag.cs file.
            var content = generator.GenerateOptionsBag( $"{options.EntityType.Name}ListOptionsBag", bagNamespace );
            files.Add( new GeneratedFile( $"{options.EntityType.Name}ListOptionsBag.cs", bagPath, content ) );

            // Generate the main <Entity>List.cs file.
            using ( var reader = new StreamReader( GetType().Assembly.GetManifestResourceStream( "Rock.CodeGeneration.Resources.EntityListBlock-cs.lava" ) ) )
            {
                var lavaTemplate = reader.ReadToEnd();

                var result = LavaHelper.Render( lavaTemplate, mergeFields );

                files.Add( new GeneratedFile( $"{options.EntityType.Name}List.cs", blockPath, result ) );
            }

            // Generate the Obsidian <entity>List.obs file.
            using ( var reader = new StreamReader( GetType().Assembly.GetManifestResourceStream( "Rock.CodeGeneration.Resources.EntityListBlock-ts.lava" ) ) )
            {
                var lavaTemplate = reader.ReadToEnd();

                var result = LavaHelper.Render( lavaTemplate, mergeFields );

                files.Add( new GeneratedFile( $"{options.EntityType.Name.CamelCase()}List.obs", typeScriptBlockPath, result ) );
            }

            // Generate the Obsidian <Entity>List\types.partial.ts file.
            content = tsGenerator.GenerateListBlockTypeDefinitionFile( new Dictionary<string, string>
            {
                ["DetailPage"] = "DetailPage"
            } );
            files.Add( new GeneratedFile( "types.partial.ts", $"{typeScriptBlockPath}\\{options.EntityType.Name}List", content ) );

            return files;
        }

        /// <summary>
        /// Performs any post-processing actions for files that were generated.
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
                    // For each file that was created, make sure it
                    // has been added to the project file automatically.
                    for ( int i = 0; i < createdFiles.Count; i++ )
                    {
                        var file = createdFiles[i];
                        var filename = Path.Combine( solutionPath, file.SolutionRelativePath );

                        if ( filename.EndsWith( ".cs" ) || filename.EndsWith( ".ts" ) || filename.EndsWith( ".obs" ) )
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
        /// Handles the Click event of the SelectEntity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SelectEntity_Click( object sender, RoutedEventArgs e )
        {
            var dialog = new SelectEntityDialog
            {
                Owner = Window.GetWindow( this )
            };

            if ( dialog.ShowDialog() == true )
            {
                SetEntity( dialog.SelectedEntity );
            }
        }

        /// <summary>
        /// Handles the CheckChanged event of the ShowAdvancedPropertiesCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ShowAdvancedPropertiesCheckBox_CheckChanged( object sender, RoutedEventArgs e )
        {
            UpdateEntityProperties( false );
        }

        /// <summary>
        /// Handles the Click event of the Preview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Preview_Click( object sender, RoutedEventArgs e )
        {
            // Get the properties that should be included in the list block.
            var selectedProperties = _columnItems
                .Select( p => _columnProperties.FirstOrDefault( cp => cp.Name == p.Name )?.Property )
                .Where( cp => cp != null )
                .ToList();

            // Find the entity service object type.
            var serviceType = Reflection.FindTypes( typeof( Data.Service<> ).MakeGenericType( _selectedEntityType ) )
                .Select( kvp => kvp.Value )
                .FirstOrDefault();

            if ( serviceType == null )
            {
                MessageBox.Show( Window.GetWindow( this ), $"Cannot determine the service instance for {_selectedEntityType.Name}.", "Invalid service" );
            }

            // Build the list of files the need to be written to disk.
            var options = new ListBlockOptions
            {
                EntityType = _selectedEntityType,
                ServiceType = serviceType,
                Columns = selectedProperties.Select( p => new EntityColumn( p ) ).ToList(),
                ExpectedRowCount = SkeletonCount.Text.AsIntegerOrNull(),
                UseAttributeValues = UseAttributeValuesCheckBox.IsChecked == true,
                UseEntitySecurity = SecurityFromEntityRadioButton.IsChecked == true,
                ShowReorder = ShowReorder.IsChecked == true,
                ShowSecurity = ShowSecurity.IsChecked == true,
                ShowDelete = ShowDelete.IsChecked == true,
                ToolTipSource = ToolTipSource.SelectedItem?.ToString()
            };

            var files = GenerateFiles( options );

            // Show the preview page so the user can verify all the generated
            // data and write it to disk.
            var previewPage = new GeneratedFilePreviewPage( files )
            {
                PostSaveAction = ProcessPostSaveFiles
            };

            this.Navigation().PushPageAsync( previewPage );
        }

        /// <summary>
        /// Handles the Click event of the AddColumn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AddColumn_Click( object sender, RoutedEventArgs e )
        {
            _columnItems.Add( new ColumnItem( _columnProperties?.Select( p => p.Name ) ) );
        }

        /// <summary>
        /// Handles the Click event of the RemoveAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RemoveAll_Click( object sender, RoutedEventArgs e )
        {
            _columnItems.Clear();
        }

        /// <summary>
        /// Handles the Click event of the RemoveColumn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void RemoveColumn_Click( object sender, RoutedEventArgs e )
        {
            if ( e.Source is Button button )
            {
                _columnItems.Remove( button.DataContext as ColumnItem );
            }
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// An item that can be displayed in the listbox.
        /// </summary>
        private class ColumnItem : INotifyPropertyChanged
        {
            #region Events

            /// <summary>
            /// Occurs when a property value changes.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            #region Properties

            /// <summary>
            /// Gets the name of the property.
            /// </summary>
            /// <value>The name of the property.</value>
            public string Name
            {
                get => _name;
                set
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
            private string _name;

            /// <summary>
            /// Gets or sets the valid property names.
            /// </summary>
            /// <value>The valid property names.</value>
            public IEnumerable<string> ValidNames
            {
                get => _validNames;
                set
                {
                    _validNames = value ?? new List<string>();
                    OnPropertyChanged();

                    if ( !_validNames.Contains( _name ) )
                    {
                        Name = string.Empty;
                    }
                }
            }
            private IEnumerable<string> _validNames;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ColumnItem"/> class.
            /// </summary>
            /// <param name="validNames">The property names that can be selected.</param>
            public ColumnItem( IEnumerable<string> validNames )
            {
                _validNames = validNames ?? new List<string>();
                _name = _validNames.FirstOrDefault() ?? string.Empty;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called when a property value has changed.
            /// </summary>
            /// <param name="propertyName">The name of the property.</param>
            protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
            {
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
            }

            #endregion
        }

        /// <summary>
        /// Contains the options to use when generating the list block files.
        /// </summary>
        private class ListBlockOptions
        {
            /// <summary>
            /// Gets or sets the type of the entity that will be edited by the block.
            /// </summary>
            /// <value>The type of the entity taht will be edited by the block.</value>
            public Type EntityType { get; set; }

            /// <summary>
            /// Gets or sets the type of the service that will handle database access.
            /// </summary>
            /// <value>The type of the service that will handle database access.</value>
            public Type ServiceType { get; set; }

            /// <summary>
            /// Gets or sets the properties to be included in the block.
            /// </summary>
            /// <value>The properties to be included in the block.</value>
            public List<EntityColumn> Columns { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether code for working with
            /// attribute values should be included.
            /// </summary>
            /// <value><c>true</c> if attribute values should be included; otherwise, <c>false</c>.</value>
            public bool UseAttributeValues { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether entity security should
            /// be used instead of CMS security.
            /// </summary>
            /// <value><c>true</c> if entity security should be used; otherwise, <c>false</c>.</value>
            public bool UseEntitySecurity { get; set; }

            /// <summary>
            /// Gets or sets the tool tip source field.
            /// </summary>
            /// <value>The tool tip source field.</value>
            public string ToolTipSource { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to show the reorder column.
            /// </summary>
            /// <value><c>true</c> if the reorder column should be visible; otherwise, <c>false</c>.</value>
            public bool ShowReorder { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to show the security column.
            /// </summary>
            /// <value><c>true</c> if the security column should be visible; otherwise, <c>false</c>.</value>
            public bool ShowSecurity { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to show the delete column.
            /// </summary>
            /// <value><c>true</c> if the delete column should be visible; otherwise, <c>false</c>.</value>
            public bool ShowDelete { get; set; }

            /// <summary>
            /// Gets or sets the expected row count.
            /// </summary>
            /// <value>The expected row count.</value>
            public int? ExpectedRowCount { get; set; }
        }

        #endregion
    }
}
