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
    public class NcoaDataBag
    {
        /// <summary>
        /// 
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string IndividualName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string IndividualIdKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FamilyMembers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OriginalAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string NewAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MoveDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal? MoveDistance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProcessStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AddressStatus { get; set; }

    }
}
