using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Crm.NcoaResults
{
    internal class NcoaResultsBag : BlockBox
    {
        /// <summary>
        /// 
        /// </summary>
        public List<NcoaRowBag> Rows { get; set; }
    }
}
