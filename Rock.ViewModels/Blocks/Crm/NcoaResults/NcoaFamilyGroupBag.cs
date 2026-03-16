using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Crm.NcoaResults
{
    /// <summary>
    /// Family group of Ncoa history items
    /// </summary>
    public class NcoaFamilyGroupBag
    {
        /// <summary>
        /// 
        /// </summary>
        public string FamilyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<NcoaDataBag> NcoaItems { get; set; }
    }
}
