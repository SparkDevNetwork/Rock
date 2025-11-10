using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Rock.CodeGeneration.Pages;
using Rock.CodeGeneration.Utility;

namespace Rock.CodeGeneration.ViewModels
{
    public class ObsidianViewModelsPageViewModel : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Fields

        private ObservableCollection<TypeItem> _typeItems;
        private ObservableCollection<TypeItemGroup> _groupedTypeItems;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the settings.
        /// </summary>
        public SettingsHelper Settings => SettingsHelper.Instance;

        public ObservableCollection<TypeItem> TypeItems
        {
            get => _typeItems;
            set
            {
                if ( _typeItems != value )
                {
                    _typeItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<TypeItemGroup> GroupedTypeItems
        {
            get => _groupedTypeItems;
            set
            {
                if ( _groupedTypeItems != value )
                {
                    _groupedTypeItems = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

        private void OnPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion
    }
}
