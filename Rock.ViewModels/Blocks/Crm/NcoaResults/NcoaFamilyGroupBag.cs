using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Crm.NcoaResults
{
    /// <summary>
    /// Represents a family group of NCOA history items for display in the NCOA Results block.
    /// </summary>
    public class NcoaFamilyGroupBag
    {
        /// <summary>
        /// The name of the family group (from the Rock family group record).
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// The list of NCOA history items belonging to this family group.
        /// </summary>
        public List<NcoaDataBag> NcoaItems { get; set; }
    }
}
