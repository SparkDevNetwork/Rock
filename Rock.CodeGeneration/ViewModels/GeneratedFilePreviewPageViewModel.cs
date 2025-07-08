using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Rock.CodeGeneration.Pages;
using Rock.CodeGeneration.Utility;

namespace Rock.CodeGeneration.ViewModels
{
    public class GeneratedFilePreviewPageViewModel : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Fields

        private ObservableCollection<ExportFile> _exportFiles;
        private ObservableCollection<ExportFileGroup> _exportFileGroups;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the settings.
        /// </summary>
        public SettingsHelper Settings => SettingsHelper.Instance;
        
        /// <summary>
        /// The files that will be displayed to the user to determine if they
        /// should be saved to disk or not.
        /// </summary>
        public ObservableCollection<ExportFile> ExportFiles
        {
            get => _exportFiles;
            set
            {
                if ( _exportFiles != value )
                {
                    _exportFiles = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ExportFileGroup> ExportFileGroups
        {
            get => _exportFileGroups;
            set
            {
                if ( _exportFileGroups != value )
                {
                    _exportFileGroups = value;
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
