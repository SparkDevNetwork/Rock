using Rock.CodeGeneration.FileGenerators;
using Rock.CodeGeneration.Utility;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Rock.CodeGeneration.Pages
{
    /// <summary>
    /// Interaction logic for ObsidianSystemGuidsPage.xaml
    /// </summary>
    public partial class ObsidianSystemGuidsPage : Page
    {
        public ObsidianSystemGuidsPage()
        {
            InitializeComponent();

            RockOutOfDateAlert.Visibility = SupportTools.IsSourceNewer( typeof( Rock.Data.IEntity ).Assembly.Location, "Rock" )
                ? Visibility.Visible
                : Visibility.Collapsed;

            var types = GetSystemGuidTypes();

            var typeItems = types.Select( t => new TypeItem( t ) )
                .OrderBy( t => !t.IsInvalid )
                .ThenBy( t => t.Name )
                .ToList();

            SystemGuidsListBox.ItemsSource = typeItems;
        }

        private List<Type> GetSystemGuidTypes()
        {
            return typeof( Rock.SystemGuid.DefinedType ).Assembly
                .GetExportedTypes()
                .Where( t => t.Namespace == "Rock.SystemGuid" )
                .ToList();
        }

        private string GetPath()
        {
            return Path.Combine( "Rock.JavaScript.Obsidian", "Framework", "SystemGuids", "CodeGenerated" );
        }

        private string GetFileNameForType( Type type )
        {
            return $"{type.Name.CamelCase()}.d.ts";
        }

        private IList<Type> GetSelectedTypes()
        {
            return SystemGuidsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( t => t.IsExporting )
                .Select( t => t.Type )
                .ToList();
        }

        private void SelectAll_Click( object sender, RoutedEventArgs e )
        {
            SystemGuidsListBox.ItemsSource
                .Cast<TypeItem>()
                .Where( i => !i.IsInvalid )
                .ToList()
                .ForEach( i => i.IsExporting = true );
        }

        private void SelectNone_Click( object sender, RoutedEventArgs e )
        {
            SystemGuidsListBox.ItemsSource
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
                var source = generator.GenerateSystemGuidForType( type );
                files.Add( new GeneratedFile( GetFileNameForType( type ), GetPath(), source ) );
            }

            var indexSource = generator.GenerateSystemGuidIndexForTypes( GetSystemGuidTypes() );
            files.Add( new GeneratedFile( "generated-index.d.ts", GetPath(), indexSource ) );

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
