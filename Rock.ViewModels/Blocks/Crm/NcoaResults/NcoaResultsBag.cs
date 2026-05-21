using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Crm.NcoaResults
{
    /// <summary>
    /// 
    /// </summary>
    public class NcoaResultsBag
    {
        /// <summary>
        /// 
        /// </summary>
        public List<NcoaFamilyGroupBag> NcoaList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? TotalResults { get; set; }
    }
}
