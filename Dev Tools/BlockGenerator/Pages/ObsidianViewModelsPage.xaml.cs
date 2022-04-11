using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using BlockGenerator.FileGenerators;
using BlockGenerator.Utility;

namespace BlockGenerator.Pages
{
    /// <summary>
    /// Interaction logic for ObsidianViewModelsPage.xaml
    /// </summary>
    public partial class ObsidianViewModelsPage : Page
    {
        public ObsidianViewModelsPage()
        {
            InitializeComponent();

            RockViewModelsOutOfDateAlert.Visibility = SupportTools.IsSourceNewer( Path.Combine( "Rock.ViewModels", "bin", "Debug", "Rock.ViewModels.dll" ), "Rock.ViewModels" )
                ? Visibility.Visible
                : Visibility.Collapsed;

            var types = GetViewModelTypes();

            var typeItems = types.Select( t => new TypeItem( t ) )
                .OrderBy( t => t.Name )
                .ToList();

            foreach ( var item in typeItems )
            {
                var unsupported = GetUnsupportedProperties( item.Type, types );

                if ( unsupported.Any() )
                {
                    item.IsUnsupported = true;
                    item.UnsupportedReason = $"The following properties are not supported: {string.Join( ", ", unsupported.Select( p => p.Name ) )}";
                }
            }

            ViewModelsListBox.ItemsSource = typeItems;
        }

        private List<string> GetDuplicateTypeNames( IList<Type> types )
        {
            return types.Select( t => t.FullName.Split( '`' )[0] )
                .GroupBy( t => t )
                .Where( t => t.Count() > 1 )
                .Select( t => t.Key )
                .ToList();
        }

        private List<PropertyInfo> GetUnsupportedProperties( Type type, IList<Type> viewModelTypes )
        {
            return type.GetProperties()
                .Where( p => !IsSupportedPropertyType( p.PropertyType, viewModelTypes ) )
                .ToList();
        }

        private bool IsSupportedPropertyType( Type type, IList<Type> viewModelTypes )
        {
            if ( EntityProperty.IsSupportedPropertyType( type ) )
            {
                return true;
            }

            // If type is one of the types we will be generating then it is supported.
            if ( viewModelTypes.Contains( type ) )
            {
                return true;
            }

            // If type is in the Rock.Enums assembly, it's supported.
            if ( type.IsEnum && type.Assembly == typeof( Rock.Enums.Reporting.FieldFilterSourceType ).Assembly )
            {
                return true;
            }

            // Check for some generic types that are supported.
            if ( type.IsGenericType )
            {
                if ( type.GetGenericTypeDefinition() == typeof( Dictionary<,> ) )
                {
                    if ( type.GetGenericArguments()[0] != typeof( string ) )
                    {
                        return false;
                    }

                    return IsSupportedPropertyType( type.GetGenericArguments()[1], viewModelTypes );
                }
            }

            // Named generic parameters are supported.
            if ( type.IsGenericParameter )
            {
                return true;
            }

            return type == typeof( object );
        }

        private List<Type> GetViewModelTypes()
        {
            return typeof( Rock.ViewModels.Utility.IViewModel ).Assembly
                .GetExportedTypes()
                .Where( t => t.Name.Split( '`' )[0].EndsWith( "Bag" ) || t.Name.Split( '`' )[0].EndsWith( "Box" ) )
                .Where( t => t.Namespace != "Rock.ViewModels.Entities" )
                .Where( t => !t.IsAbstract && !t.IsInterface )
                .ToList();
        }

        private string GetPathForType( Type type )
        {
            var components = type.Namespace.Replace( "Rock.ViewModels", string.Empty ).Trim( '.' ).Split( '.' );

            return Path.Combine( "Rock.JavaScript.Obsidian", "Framework", "ViewModels", string.Join( "\\", components ) );
        }

        private string GetFileNameForType( Type type )
        {
            return $"{type.Name.Split( '`' )[0].CamelCase()}.d.ts";
        }

        private IList<Type> GetSelectedTypes()
        {
            return ViewModelsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( t => t.IsExporting )
                .Select( t => t.Type )
                .ToList();
        }

        private void SelectAll_Click( object sender, RoutedEventArgs e )
        {
            ViewModelsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( t => !t.IsUnsupported )
                .ToList()
                .ForEach( i => i.IsExporting = true );
        }

        private void SelectNone_Click( object sender, RoutedEventArgs e )
        {
            ViewModelsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( t => !t.IsUnsupported )
                .ToList()
                .ForEach( i => i.IsExporting = false );
        }

        private async void Preview_Click( object sender, RoutedEventArgs e )
        {
            var files = new List<GeneratedFile>();

            var generator = new TypeScriptViewModelGenerator();
            foreach ( var type in GetSelectedTypes() )
            {
                var source = generator.GenerateViewModelForType( type );
                files.Add( new GeneratedFile( GetFileNameForType( type ), GetPathForType( type ), source ) );
            }

            await this.Navigation().PushPageAsync( new GeneratedFilePreviewPage( files ) );
        }

        private class TypeItem : IComparable, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public Type Type { get; set; }

            public string Name { get; set; }

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

            public bool IsUnsupported { get; set; }

            public string UnsupportedReason { get; set; }

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

            protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
            {
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
            }

            public int CompareTo( object obj )
            {
                return Type.AssemblyQualifiedName.CompareTo( obj );
            }
        }
    }
}
