using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.BulkImport
{
    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Import to POST to ~/api/Attendances/Import" )]
    public class AttendancesImport
    {
        /// <summary>
        /// Gets or sets the attendances.
        /// </summary>
        /// <value>
        /// The attendances.
        /// </value>
        public List<AttendanceImport> Attendances { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Attendances.Count} Attendances";
        }
    }
}
