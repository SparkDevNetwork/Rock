using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

using Rock;
using Rock.CodeGeneration.Dialogs;
using Rock.CodeGeneration.FileGenerators;
using Rock.CodeGeneration.Lava;
using Rock.CodeGeneration.Utility;

namespace Rock.CodeGeneration.Pages
{
    /// <summary>
    /// Interaction logic for ObsidianDetailBlock.xaml
    /// </summary>
    public partial class ObsidianDetailBlockPage : Page
    {
        #region Fields

        /// <summary>
        /// The currently selected entity type in the UI.
        /// </summary>
        private Type _selectedEntityType;

        /// <summary>
        /// The properties for the currently selected entity type.
        /// </summary>
        private List<PropertyItem> _propertyItems;

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
        /// Initializes a new instance of the <see cref="ObsidianDetailBlockPage"/> class.
        /// </summary>
        public ObsidianDetailBlockPage()
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
            UpdateEntityProperties();

            UseAttributeValuesCheckBox.IsChecked = typeof( Rock.Attribute.IHasAttributes ).IsAssignableFrom( entityType );
            UseAttributeValuesCheckBox.IsEnabled = UseAttributeValuesCheckBox.IsChecked ?? false;
        }

        /// <summary>
        /// Updates the list of entity properties for the selected entity.
        /// </summary>
        private void UpdateEntityProperties()
        {
            _propertyItems = GetEntityProperties( _selectedEntityType )
                .Select( p => new PropertyItem( p ) )
                .ToList();

            // Check all the properties and see if any of them are unsupported.
            foreach ( var item in _propertyItems )
            {
                if ( !IsValidPropertyType( item.Property ) )
                {
                    item.InvalidReason = $"Property type {item.Property.PropertyType.GetFriendlyName()} is not supported.";
                }
            }

            PropertiesListBox.ItemsSource = _propertyItems;
        }

        /// <summary>
        /// Gets the properties that exist on the specified entity type. This automatically
        /// handles filtering out system and advanced properties.
        /// </summary>
        /// <param name="entityType">Type of the entity whose properties are to be enumerated.</param>
        /// <returns>An enumeration of the valid properties that match the filtering options.</returns>
        private IEnumerable<PropertyInfo> GetProperties( Type entityType )
        {
            return entityType?.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                .Where( p => p.GetCustomAttribute<DataMemberAttribute>() != null || typeof( Data.IEntity ).IsAssignableFrom( p.PropertyType ) )
                .Where( p => p.GetCustomAttribute<NotMappedAttribute>() == null )
                .Where( p => !_systemProperties.Contains( p.Name ) )
                .Where( p => ShowAdvancedPropertiesCheckBox.IsChecked == true || !_advancedProperties.Contains( p.Name ) )
                .ToList()
                ?? new List<PropertyInfo>();
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
            return EntityProperty.IsSupportedPropertyType( property.PropertyType );
        }

        /// <summary>
        /// Generates all the files required to make a skeleton detail block
        /// for the selected entity type.
        /// </summary>
        /// <param name="options">The options that describe all the options to use when generating code.</param>
        /// <returns>A collection of <see cref="GeneratedFile"/> objects that represent the files to be created or updated.</returns>
        private IList<GeneratedFile> GenerateFiles( DetailBlockOptions options )
        {
            var files = new List<GeneratedFile>();
            var domain = options.EntityType.GetCustomAttribute<Data.RockDomainAttribute>()?.Name ?? "Unknown";
            var domainNamespace = SupportTools.GetDomainFolderName( domain );
            var bagPath = $"Rock.ViewModels\\Blocks\\{domainNamespace}\\{options.EntityType.Name}Detail";
            var blockPath = $"Rock.Blocks\\{domainNamespace}";
            var typeScriptBlockPath = $"Rock.JavaScript.Obsidian.Blocks\\src\\{domainNamespace}";
            var bagNamespace = $"Rock.ViewModels.Blocks.{domainNamespace}.{options.EntityType.Name}Detail";
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
                ["Properties"] = options.Properties,
                ["UseAttributeValues"] = options.UseAttributeValues,
                ["UseDescription"] = options.Properties.Any( p => p.Name == "Description" ),
                ["UseEntitySecurity"] = options.UseEntitySecurity,
                ["UseIsActive"] = options.Properties.Any( p => p.Name == "IsActive" ),
                ["UseIsSystem"] = options.Properties.Any( p => p.Name == "IsSystem" ),
                ["UseOrder"] = options.Properties.Any( p => p.Name == "Order" ),
                ["UseName"] = options.Properties.Any( p => p.Name == "Name" )
            };

            // Generate the <Entity>Bag.cs file.
            var content = generator.GenerateEntityBag( options.EntityType.Name, bagNamespace, options.Properties );
            files.Add( new GeneratedFile( $"{options.EntityType.Name}Bag.cs", bagPath, content ) );

            // Generate the <Entity>DetailOptionsBag.cs file.
            content = generator.GenerateOptionsBag( $"{options.EntityType.Name}DetailOptionsBag", bagNamespace );
            files.Add( new GeneratedFile( $"{options.EntityType.Name}DetailOptionsBag.cs", bagPath, content ) );

            // Generate the main <Entity>Detail.cs file.
            using ( var reader = new StreamReader( GetType().Assembly.GetManifestResourceStream( "Rock.CodeGeneration.Resources.EntityDetailBlock-cs.lava" ) ) )
            {
                var lavaTemplate = reader.ReadToEnd();

                var result = LavaHelper.Render( lavaTemplate, mergeFields );

                files.Add( new GeneratedFile( $"{options.EntityType.Name}Detail.cs", blockPath, result ) );
            }

            // Generate the Obsidian <entity>Detail.obs file.
            using ( var reader = new StreamReader( GetType().Assembly.GetManifestResourceStream( "Rock.CodeGeneration.Resources.EntityDetailBlock-ts.lava" ) ) )
            {
                var lavaTemplate = reader.ReadToEnd();

                var result = LavaHelper.Render( lavaTemplate, mergeFields );

                files.Add( new GeneratedFile( $"{options.EntityType.Name.CamelCase()}Detail.obs", typeScriptBlockPath, result ) );
            }

            // Generate the Obsidian <Entity>Detail\viewPanel.partial.obs file.
            using ( var reader = new StreamReader( GetType().Assembly.GetManifestResourceStream( "Rock.CodeGeneration.Resources.ViewPanel-ts.lava" ) ) )
            {
                var lavaTemplate = reader.ReadToEnd();

                var result = LavaHelper.Render( lavaTemplate, mergeFields );

                files.Add( new GeneratedFile( $"viewPanel.partial.obs", $"{typeScriptBlockPath}\\{options.EntityType.Name}Detail", result ) );
            }

            // Generate the Obsidian <Entity>Detail\editPanel.partial.obs file.
            using ( var reader = new StreamReader( GetType().Assembly.GetManifestResourceStream( "Rock.CodeGeneration.Resources.EditPanel-ts.lava" ) ) )
            {
                var lavaTemplate = reader.ReadToEnd();

                var result = LavaHelper.Render( lavaTemplate, mergeFields );

                files.Add( new GeneratedFile( $"editPanel.partial.obs", $"{typeScriptBlockPath}\\{options.EntityType.Name}Detail", result ) );
            }

            // Generate the Obsidian <Entity>Detail\types.partial.ts file.
            content = tsGenerator.GenerateDetailBlockTypeDefinitionFile( new Dictionary<string, string>
            {
                ["ParentPage"] = "ParentPage"
            } );
            files.Add( new GeneratedFile( "types.partial.ts", $"{typeScriptBlockPath}\\{options.EntityType.Name}Detail", content ) );

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
            UpdateEntityProperties();
        }

        /// <summary>
        /// Handles the Click event of the Preview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Preview_Click( object sender, RoutedEventArgs e )
        {
            // Get the properties that should be included in the detail block.
            var selectedProperties = _propertyItems
                .Where( p => p.IsSelected )
                .Select( p => p.Property )
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
            var options = new DetailBlockOptions
            {
                EntityType = _selectedEntityType,
                ServiceType = serviceType,
                Properties = selectedProperties.Select( p => new EntityProperty( p ) ).ToList(),
                UseAttributeValues = UseAttributeValuesCheckBox.IsChecked == true,
                UseEntitySecurity = SecurityFromEntityRadioButton.IsChecked == true
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
        /// Handles the Click event of the SelectAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SelectAll_Click( object sender, RoutedEventArgs e )
        {
            _propertyItems
                .Where( p => !p.IsInvalid )
                .ToList()
                .ForEach( i => i.IsSelected = true );
        }

        /// <summary>
        /// Handles the Click event of the SelectNone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SelectNone_Click( object sender, RoutedEventArgs e )
        {
            _propertyItems
                .Where( p => !p.IsInvalid )
                .ToList()
                .ForEach( i => i.IsSelected = false );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// An item that can be displayed in the listbox and handles tracking
        /// the IsChecked state.
        /// </summary>
        /// <seealso cref="INotifyPropertyChanged" />
        internal class PropertyItem : INotifyPropertyChanged
        {
            #region Events

            /// <summary>
            /// Occurs when a property value changes.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            #region Properties

            /// <summary>
            /// Gets the property that is being displayed.
            /// </summary>
            /// <value>The property that is being displayed.</value>
            public PropertyInfo Property { get; }

            /// <summary>
            /// Gets the name of the property.
            /// </summary>
            /// <value>The name of the property.</value>
            public string Name => Property.Name;

            /// <summary>
            /// Gets or sets a value indicating whether this property is selected.
            /// </summary>
            /// <value><c>true</c> if this property is selected; otherwise, <c>false</c>.</value>
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
            private bool _isSelected;

            /// <summary>
            /// Gets a value indicating whether this instance is invalid.
            /// </summary>
            /// <value><c>true</c> if this instance is invalid; otherwise, <c>false</c>.</value>
            public bool IsInvalid => InvalidReason.IsNotNullOrWhiteSpace();

            /// <summary>
            /// Gets or sets the invalid reason.
            /// </summary>
            /// <value>The invalid reason.</value>
            public string InvalidReason { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyItem"/> class.
            /// </summary>
            /// <param name="propertyInfo">The property information.</param>
            public PropertyItem( PropertyInfo propertyInfo )
            {
                Property = propertyInfo;
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
        /// Contains the options to use when generating the detail block files.
        /// </summary>
        private class DetailBlockOptions
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
            public List<EntityProperty> Properties { get; set; }

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
        }

        #endregion
    }
}
