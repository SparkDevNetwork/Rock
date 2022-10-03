using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Rock.CodeGeneration.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectEntityDialog.xaml
    /// </summary>
    public partial class SelectEntityDialog : Window, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The <see cref="FilterValue"/> backing field.
        /// </summary>
        private string _filterValue;

        /// <summary>
        /// The backing field that stores all items to be filtered.
        /// </summary>
        private readonly List<EntityItem> _allItems;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the filter value used to filter the types.
        /// </summary>
        /// <value>The filter value used to filter the types.</value>
        public string FilterValue
        {
            get => _filterValue;
            set
            {
                _filterValue = value;
                PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( nameof( Items ) ) );
            }
        }

        /// <summary>
        /// Gets the items that should be shown in the list box.
        /// </summary>
        /// <value>The items that should be shown in the list box.</value>
        public List<EntityItem> Items => _allItems.Where( i => string.IsNullOrWhiteSpace( FilterValue ) || i.Name.ToLower().Contains( FilterValue.ToLower() ) ).ToList();

        /// <summary>
        /// Gets the selected entity.
        /// </summary>
        /// <value>The selected entity.</value>
        public Type SelectedEntity { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectEntityDialog"/> class.
        /// </summary>
        public SelectEntityDialog()
        {
            // Find all entities in the Rock DLL and add them to the list.
            _allItems = Reflection.FindTypes( typeof( Data.IEntity ) )
                .Select( t => new EntityItem
                {
                    Name = t.Value.FullName,
                    Type = t.Value
                } )
                .OrderBy( t => t.Name )
                .ToList();

            DataContext = this;

            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the SelectionChanged event of the EntityListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void EntityListBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            SelectedEntity = ( EntityListBox.SelectedItem as EntityItem ).Type;

            DialogResult = true;
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// A single item displayed in the list box.
        /// </summary>
        public class EntityItem
        {
            #region Properties

            /// <summary>
            /// Gets or sets the name shown in the listbox.
            /// </summary>
            /// <value>The name shown in the listbox.</value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the type represented by this item.
            /// </summary>
            /// <value>The type represented by this item.</value>
            public Type Type { get; set; }

            #endregion
        }

        #endregion
    }
}
