using System.Windows.Controls;
using System.Windows.Media;

using BlockGenerator.Utility;
using BlockGenerator.ViewModels;

namespace BlockGenerator.Controls
{
    /// <summary>
    /// A bootstrap style alert that will display messages.
    /// </summary>
    public partial class Alert : UserControl
    {
        #region Properties

        /// <summary>
        /// Gets or sets the type of the alert.
        /// </summary>
        /// <value>The type of the alert.</value>
        public AlertType AlertType
        {
            get => _alertType;
            set
            {
                _alertType = value;
                SyncViewModel();
            }
        }
        private AlertType _alertType;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                SyncViewModel();
            }
        }
        private string _text;

        #endregion

        #region Fields

        /// <summary>
        /// The view model that is used in data binding.
        /// </summary>
        private readonly AlertViewModel _viewModel = new AlertViewModel();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Alert"/> class.
        /// </summary>
        public Alert()
        {
            InitializeComponent();

            SyncViewModel();

            DataContext = _viewModel;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Synchronizes the view model to the data in our own properties.
        /// </summary>
        private void SyncViewModel()
        {
            _viewModel.Text = Text;

            switch ( AlertType )
            {
                case AlertType.Warning:
                default:
                    _viewModel.BackgroundBrush = new SolidColorBrush( Color.FromRgb( 0xff, 0xfa, 0xe5 ) );
                    _viewModel.BorderBrush = new SolidColorBrush( Color.FromRgb( 0xff, 0xc8, 0x70 ) );
                    _viewModel.ForegroundBrush = new SolidColorBrush( Color.FromRgb( 0x8a, 0x6d, 0x3b ) );
                    _viewModel.Icon = FontAwesome.Sharp.IconChar.ExclamationTriangle;
                    break;
            }
        }

        #endregion
    }
}
