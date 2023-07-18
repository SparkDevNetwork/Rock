using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.LocationDetail
{
    /// <summary>
    /// AddressStandardizationResultBag
    /// Implements the <see cref="IViewModel" />
    /// </summary>
    /// <seealso cref="IViewModel" />
    public sealed class AddressStandardizationResultBag : IViewModel
    {
        /// <summary>
        /// Gets or sets the address fields.
        /// </summary>
        /// <value>The address fields.</value>
        public AddressControlBag AddressFields { get; set; }

        /// <summary>
        /// Gets or sets the standardize attempted result.
        /// </summary>
        /// <value>The standardize attempted result.</value>
        public string StandardizeAttemptedResult { get; set; }

        /// <summary>
        /// Gets or sets the geocode attempted result.
        /// </summary>
        /// <value>The geocode attempted result.</value>
        public string GeocodeAttemptedResult { get; set; }
    }                 
}
