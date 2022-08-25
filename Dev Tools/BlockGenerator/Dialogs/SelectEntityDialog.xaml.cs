using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BlockGenerator.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectEntityDialog.xaml
    /// </summary>
    public partial class SelectEntityDialog : Window
    {
        public Type SelectedEntity { get; private set; }

        public SelectEntityDialog()
        {
            DataContext = this;

            InitializeComponent();

            EntityListBox.ItemsSource = Rock.Reflection.FindTypes( typeof( Rock.Data.IEntity ) )
                .Select( t => new EntityItem
                {
                    Name = t.Value.FullName,
                    Type = t.Value
                } )
                .OrderBy( t => t )
                .ToList();
        }

        private void EntityListBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            SelectedEntity = ( EntityListBox.SelectedItem as EntityItem ).Type;

            DialogResult = true;
        }

        private class EntityItem : IComparable
        {
            public string Name { get; set; }

            public Type Type { get; set; }

            public int CompareTo( object obj )
            {
                return Name.CompareTo( ( obj as EntityItem )?.Name );
            }
        }
    }
}
