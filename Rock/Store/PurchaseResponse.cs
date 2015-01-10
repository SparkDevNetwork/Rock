using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Store
{
    /// <summary>
    /// Represents a store category for packages.
    /// </summary>
    public class PurchaseResponse : StoreModel
    {
        /// <summary>
        /// Gets or sets the result of the purchase. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> representing the result of the purchase.
        /// </value>
        public PurchaseResult PurchaseResult { get; set; }

        /// <summary>
        /// Gets or sets the message of the purchase result. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing message of the purchase result.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the list of install steps. 
        /// </summary>
        /// <value>
        /// A <see cref="List<PackageInstallStep>"/> representing a list of package install steps.
        /// </value>
        public List<PackageInstallStep> PackageInstallSteps { get; set; }
    }

    public enum PurchaseResult
    {
        Success,
        AuthenicationFailed,
        NotAuthorized,
        NoCardOnFile,
        PaymentFailed,
        Error
    }
}
