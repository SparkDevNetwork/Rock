using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model.Engagement.StreakType
{
    /// <summary>
    /// Used by the StructureSettingsJSON of the <seealso cref="Rock.Model.Engagement.StreakType"/>.
    /// </summary>
    [Serializable]
    public class StreakTypeSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether [include child accounts].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include child accounts]; otherwise, <c>false</c>.</value>
        public bool IncludeChildAccounts { get; set; }
    }
}
