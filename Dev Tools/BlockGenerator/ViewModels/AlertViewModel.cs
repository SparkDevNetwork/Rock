using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace BlockGenerator.ViewModels
{
    /// <summary>
    /// The view model used by the <see cref="Controls.Alert"/> control when
    /// displaying its content.
    /// </summary>
    public class AlertViewModel : INotifyPropertyChanged
    {
        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the border brush.
        /// </summary>
        /// <value>The border brush.</value>
        public Brush BorderBrush
        {
            get => _borderBrush;
            set
            {
                _borderBrush = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the background brush.
        /// </summary>
        /// <value>The background brush.</value>
        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            set
            {
                _backgroundBrush = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the foreground brush.
        /// </summary>
        /// <value>The foreground brush.</value>
        public Brush ForegroundBrush
        {
            get => _foregroundBrush;
            set
            {
                _foregroundBrush = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the text of the alert message.
        /// </summary>
        /// <value>The text of the alert message.</value>
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public FontAwesome.Sharp.IconChar Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Fields

        private Brush _borderBrush = new SolidColorBrush( Color.FromRgb( 0xff, 0xc8, 0x70 ) );
        private Brush _backgroundBrush = new SolidColorBrush( Color.FromRgb( 0xff, 0xfa, 0xe5 ) );
        private Brush _foregroundBrush = new SolidColorBrush( Color.FromRgb( 0x8a, 0x6d, 0x3b ) );
        private string _text;
        private FontAwesome.Sharp.IconChar _icon = FontAwesome.Sharp.IconChar.ExclamationTriangle;

        #endregion

        #region Methods

        /// <summary>
        /// Emits the property changed events for the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion
    }
}
