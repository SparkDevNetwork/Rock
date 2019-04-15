using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Apps.CheckScannerUtility.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Client.FinancialBatch" />
    public class FinancialBatchWithControlVariance : Rock.Client.FinancialBatch
    {
        public bool? HasVariance { get; set; }
    }
}
