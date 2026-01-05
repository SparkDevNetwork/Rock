using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rock.CodeGeneration.Utility
{
    public sealed class SettingsHelper : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Fields

        public static readonly SettingsHelper Instance = new SettingsHelper();
        private readonly Properties.Settings _settings = Properties.Settings.Default;

        #endregion

        #region Properties

        public bool ObsidianViewModelsIsGroupedView
        {
            get => _settings.ObsidianViewModelsIsGroupedView;
            set
            {
                if ( _settings.ObsidianViewModelsIsGroupedView != value )
                {
                    _settings.ObsidianViewModelsIsGroupedView = value;
                    _settings.Save(); // Save immediately so other code can see the change.

                    OnPropertyChanged();
                }                
            }
        }

        public int ObsidianViewModelsDiffType
        {
            get => _settings.ObsidianViewModelsDiffType;
            set
            {
                if ( _settings.ObsidianViewModelsDiffType != value )
                {
                    _settings.ObsidianViewModelsDiffType = value;
                    _settings.Save(); // Save immediately so other code can see the change.

                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        private SettingsHelper()
        {
            // If another part of the app changes Settings, keep the VM in sync.
            _settings.PropertyChanged += settings_PropertyChanged;
        }

        #endregion

        #region Methods

        private void settings_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if ( e.PropertyName == nameof( _settings.ObsidianViewModelsIsGroupedView ) )
            {
                OnPropertyChanged( nameof( _settings.ObsidianViewModelsIsGroupedView ) );
            }
        }

        private void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion
    }
}
