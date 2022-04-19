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
    /// Interaction logic for ObsidianEnumsPage.xaml
    /// </summary>
    public partial class ObsidianEnumsPage : Page
    {
        public ObsidianEnumsPage()
        {
            InitializeComponent();

            RockEnumsOutOfDateAlert.Visibility = SupportTools.IsSourceNewer( Path.Combine( "Rock.Enums", "bin", "Debug", "Rock.Enums.dll" ), "Rock.Enums" )
                ? Visibility.Visible
                : Visibility.Collapsed;

            var types = GetEnumTypes();

            var typeItems = types.Select( t => new TypeItem( t ) )
                .ToList();

            foreach ( var item in typeItems )
            {
                var unsupported = !item.Type.FullName.StartsWith( "Rock.Enums." )
                    && item.Type.GetCustomAttributes().FirstOrDefault( a => a.GetType().FullName == "Rock.Enums.EnumDomainAttribute" ) == null;

                if ( unsupported )
                {
                    item.IsInvalid = true;
                    item.IsExporting = false;
                    item.InvalidReason = $"This enum is not in the correct namespace and cannot be exported.";
                }
            }

            typeItems = typeItems
                .OrderBy( t => !t.IsInvalid )
                .ThenBy( t => t.Name )
                .ToList();

            EnumsListBox.ItemsSource = typeItems;
        }

        private List<Type> GetEnumTypes()
        {
            return typeof( Rock.Enums.Reporting.FieldFilterSourceType ).Assembly
                .GetExportedTypes()
                .Where( t => t.IsEnum )
                .ToList();
        }

        private string GetPathForType( Type type )
        {
            if ( type.Namespace.StartsWith( "Rock.Model" ) )
            {
                var domainAttribute = type.GetCustomAttributes()
                    .FirstOrDefault( a => a.GetType().FullName == "Rock.Enums.EnumDomainAttribute" );

                if ( domainAttribute == null )
                {
                    throw new Exception( "Attempt to export an enum with an invalid namespace, this shouldn't happen." );
                }

                var domain = ( string ) domainAttribute.GetType().GetProperty( "Domain" ).GetValue( domainAttribute );

                return Path.Combine( "Rock.JavaScript.Obsidian", "Framework", "Enums", domain );
            }

            var components = type.Namespace.Replace( "Rock.Enums", string.Empty ).Trim( '.' ).Split( '.' );

            return Path.Combine( "Rock.JavaScript.Obsidian", "Framework", "Enums", string.Join( "\\", components ) );
        }

        private string GetFileNameForType( Type type )
        {
            return $"{type.Name.Split( '`' )[0].CamelCase()}.d.ts";
        }

        private IList<Type> GetSelectedTypes()
        {
            return EnumsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( t => t.IsExporting )
                .Select( t => t.Type )
                .ToList();
        }

        private void SelectAll_Click( object sender, RoutedEventArgs e )
        {
            EnumsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( i => !i.IsInvalid )
                .ToList()
                .ForEach( i => i.IsExporting = true );
        }

        private void SelectNone_Click( object sender, RoutedEventArgs e )
        {
            EnumsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( i => !i.IsInvalid )
                .ToList()
                .ForEach( i => i.IsExporting = false );
        }

        private async void Preview_Click( object sender, RoutedEventArgs e )
        {
            var files = new List<GeneratedFile>();

            var generator = new TypeScriptViewModelGenerator();
            foreach ( var type in GetSelectedTypes() )
            {
                var source = generator.GenerateViewModelForEnum( type );
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

            public bool IsInvalid { get; set; }

            public string InvalidReason { get; set; }

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
