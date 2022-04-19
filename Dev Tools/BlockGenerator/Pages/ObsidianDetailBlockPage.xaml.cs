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

using BlockGenerator.Dialogs;
using BlockGenerator.FileGenerators;
using BlockGenerator.Lava;
using BlockGenerator.Utility;

using Rock;

namespace BlockGenerator.Pages
{
    /// <summary>
    /// Interaction logic for ObsidianDetailBlock.xaml
    /// </summary>
    public partial class ObsidianDetailBlockPage : Page
    {
        private Type _selectedEntityType;

        private List<PropertyItem> _propertyItems;

        private static readonly string[] _systemProperties = new[]
        {
            "Attributes",
            "AttributeValues",
            "Guid",
            "Id"
        };

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

        public ObsidianDetailBlockPage()
        {
            InitializeComponent();

            RockOutOfDateAlert.Visibility = SupportTools.IsSourceNewer( Path.Combine( "Rock", "bin", "Debug", "Rock.dll" ), "Rock" )
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void SetEntity( Type entityType )
        {
            _selectedEntityType = entityType;

            SelectedEntityName.Content = _selectedEntityType?.FullName ?? string.Empty;
            UpdateEntityProperties();
        }

        private void UpdateEntityProperties()
        {
            _propertyItems = GetEntityProperties( _selectedEntityType )
                .Select( p => new PropertyItem( p ) )
                .ToList();

            foreach ( var item in _propertyItems )
            {
                if ( !ValidatePropertyTypes( new[] { item.Property }, out var invalidProperties ) )
                {
                    item.InvalidReason = $"Property type {item.Property.PropertyType.GetFriendlyName()} is not supported.";
                }
            }

            PropertiesListBox.ItemsSource = _propertyItems;
        }

        private IEnumerable<PropertyInfo> GetProperties( Type entityType )
        {
            return entityType?.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                .Where( p => p.GetCustomAttribute<DataMemberAttribute>() != null || typeof( Rock.Data.IEntity ).IsAssignableFrom( p.PropertyType ) )
                .Where( p => p.GetCustomAttribute<NotMappedAttribute>() == null )
                .Where( p => !_systemProperties.Contains( p.Name ) )
                .Where( p => ShowAdvancedPropertiesCheckBox.IsChecked == true || !_advancedProperties.Contains( p.Name ) )
                .ToList()
                ?? new List<PropertyInfo>();
        }

        private IEnumerable<PropertyInfo> GetEntityProperties( Type entityType )
        {
            return GetProperties( entityType )
                .OrderBy( p => p.Name )
                .ToList();
        }

        private static bool ValidatePropertyTypes( IEnumerable<PropertyInfo> properties, out PropertyInfo[] invalidProperties )
        {
            invalidProperties = properties.Where( p => !EntityProperty.IsSupportedPropertyType( p.PropertyType ) ).ToArray();

            return invalidProperties.Length == 0;
        }

        private IList<GeneratedFile> GenerateFiles( DetailBlockParameters parameters )
        {
            var files = new List<GeneratedFile>();
            var domain = parameters.EntityType.GetCustomAttribute<Rock.Data.RockDomainAttribute>()?.Name ?? "Unknown";
            var bagPath = $"Rock.ViewModels\\Blocks\\{domain}\\{parameters.EntityType.Name}Detail";
            var blockPath = $"Rock.Blocks\\{domain}";
            var typeScriptBlockPath = $"Rock.JavasScript.Obsidian\\Framework\\Blocks\\{domain}";
            var bagNamespace = $"Rock.ViewModels.Blocks.{domain}.{parameters.EntityType.Name}Detail";
            var generator = new CSharpViewModelGenerator();

            var mergeFields = new Dictionary<string, object>
            {
                ["EntityName"] = parameters.EntityType.Name,
                ["ServiceName"] = parameters.ServiceType.Name,
                ["Domain"] = domain,
                ["Properties"] = parameters.Properties,
                ["UseAttributeValues"] = parameters.UseAttributeValues,
                ["UseDescription"] = parameters.Properties.Any( p => p.Name == "Description" ),
                ["UseIsActive"] = parameters.Properties.Any( p => p.Name == "IsActive" ),
                ["UseIsSystem"] = parameters.Properties.Any( p => p.Name == "IsSystem" ),
                ["UseName"] = parameters.Properties.Any( p => p.Name == "Name" )
            };

            // Generate the entity bag.
            var content = generator.GenerateEntityBag( parameters.EntityType.Name, bagNamespace, parameters.Properties );
            files.Add( new GeneratedFile( $"{parameters.EntityType.Name}Bag.cs", bagPath, content ) );

            // Generate the custom options.
            content = generator.GenerateOptionsBag( $"{parameters.EntityType.Name}DetailOptionsBag", bagNamespace );
            files.Add( new GeneratedFile( $"{parameters.EntityType.Name}DetailOptionsBag.cs", bagPath, content ) );

            // Generate the main <Entity>Detail.cs file.
            using ( var reader = new StreamReader( GetType().Assembly.GetManifestResourceStream( "BlockGenerator.Resources.EntityDetailBlock-cs.lava" ) ) )
            {
                var lavaTemplate = reader.ReadToEnd();

                var result = LavaHelper.Render( lavaTemplate, mergeFields );

                files.Add( new GeneratedFile( $"{parameters.EntityType.Name}Detail.cs", blockPath, result ) );
            }

            // Generate the Obsidian <entity>Detail.ts file.
            using ( var reader = new StreamReader( GetType().Assembly.GetManifestResourceStream( "BlockGenerator.Resources.EntityDetailBlock-ts.lava" ) ) )
            {
                var lavaTemplate = reader.ReadToEnd();

                var result = LavaHelper.Render( lavaTemplate, mergeFields );

                files.Add( new GeneratedFile( $"{parameters.EntityType.Name.CamelCase()}Detail.ts", typeScriptBlockPath, result ) );
            }

            // Generate the Obsidian <Entity>Detail\viewPanel.ts file.
            using ( var reader = new StreamReader( GetType().Assembly.GetManifestResourceStream( "BlockGenerator.Resources.ViewPanel-ts.lava" ) ) )
            {
                var lavaTemplate = reader.ReadToEnd();

                var result = LavaHelper.Render( lavaTemplate, mergeFields );

                files.Add( new GeneratedFile( $"viewPanel.ts", $"{typeScriptBlockPath}\\{parameters.EntityType.Name}", result ) );
            }

            // Generate the Obsidian <Entity>Detail\editPanel.ts file.
            using ( var reader = new StreamReader( GetType().Assembly.GetManifestResourceStream( "BlockGenerator.Resources.EditPanel-ts.lava" ) ) )
            {
                var lavaTemplate = reader.ReadToEnd();

                var result = LavaHelper.Render( lavaTemplate, mergeFields );

                files.Add( new GeneratedFile( $"editPanel.ts", $"{typeScriptBlockPath}\\{parameters.EntityType.Name}", result ) );
            }

            return files;
        }

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

        private void ShowAdvancedPropertiesCheckBox_CheckChanged( object sender, RoutedEventArgs e )
        {
            UpdateEntityProperties();
        }

        private void Preview_Click( object sender, RoutedEventArgs e )
        {
            var selectedProperties = _propertyItems
                .Where( p => p.IsSelected )
                .Select( p => p.Property )
                .ToList();

            var serviceType = Reflection.FindTypes( typeof( Rock.Data.Service<> ).MakeGenericType( _selectedEntityType ) )
                .Select( kvp => kvp.Value )
                .FirstOrDefault();

            if ( serviceType == null )
            {
                MessageBox.Show( Window.GetWindow( this ), $"Cannot determine the service instance for {_selectedEntityType.Name}.", "Invalid service" );
            }

            var parameters = new DetailBlockParameters
            {
                EntityType = _selectedEntityType,
                ServiceType = serviceType,
                Properties = selectedProperties.Select( p => new EntityProperty( p ) ).ToList(),
                UseAttributeValues = UseAttributeValuesCheckBox.IsChecked == true
            };

            var files = GenerateFiles( parameters );

            this.Navigation().PushPageAsync( new GeneratedFilePreviewPage( files ) );
        }

        private void SelectAll_Click( object sender, RoutedEventArgs e )
        {
            _propertyItems
                .Where( p => !p.IsInvalid )
                .ToList()
                .ForEach( i => i.IsSelected = true );
        }

        private void SelectNone_Click( object sender, RoutedEventArgs e )
        {
            _propertyItems
                .Where( p => !p.IsInvalid )
                .ToList()
                .ForEach( i => i.IsSelected = false );
        }

        private class PropertyItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public PropertyInfo Property { get; }

            public string Name => Property.Name;

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

            public bool IsInvalid => InvalidReason.IsNotNullOrWhiteSpace();

            public string InvalidReason { get; set; }

            public PropertyItem( PropertyInfo propertyInfo )
            {
                Property = propertyInfo;
            }

            protected void OnPropertyChanged( [CallerMemberName] string propertyName = null )
            {
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        private class DetailBlockParameters
        {
            public Type EntityType { get; set; }

            public Type ServiceType { get; set; }

            public List<EntityProperty> Properties { get; set; }

            public bool UseAttributeValues { get; set; }
        }
    }
}
