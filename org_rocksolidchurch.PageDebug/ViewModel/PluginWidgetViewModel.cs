using org_rocksolidchurch.PageDebug.Model;
using Rock.ViewModel;

namespace org_rocksolidchurch.PageDebug.ViewModel
{
    /// <summary>
    /// Example Model View Model
    /// </summary>
    [ViewModelOf( typeof( PluginWidget ) )]
    public class PluginWidgetViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the API threshold.
        /// </summary>
        /// <value>
        /// The API threshold.
        /// </value>
        public string ThisIsTheString { get; set; }

        /// <summary>
        /// Gets or sets the API timeout.
        /// </summary>
        /// <value>
        /// The API timeout.
        /// </value>
        public int ThisIsTheInt { get; set; }
    }
}