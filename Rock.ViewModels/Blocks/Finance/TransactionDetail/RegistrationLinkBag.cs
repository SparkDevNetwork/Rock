using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Finance.TransactionDetail
{
    /// <summary>
    /// Represents a registration linked to a financial transaction.
    /// </summary>
    public class RegistrationLinkBag
    {
        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the display text (e.g. "Template Name - Instance Name").
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the URL to the registration detail page.
        /// </summary>
        public string Url { get; set; }
    }
}
