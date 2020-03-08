using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.lcbcchurch.NewVisitor.Settings
{
    /// <summary>
    /// Scoring Item Type
    /// </summary>
    public enum ScoringItemType
    {
        /// <summary>
        /// The attendance In group of type
        /// </summary>
        AttendanceInGroupOfType,

        /// <summary>
        /// The attendance in group of type cumulative
        /// </summary>
        AttendanceInGroupOfTypeCumulative,

        /// <summary>
        /// The member of GroupType
        /// </summary>
        MemberOfGroupType,

        /// <summary>
        /// The person attributes
        /// </summary>
        PersonAttribute,

        /// <summary>
        /// The given to an account
        /// </summary>
        GivenToAnAccount,

        /// <summary>
        /// The In Data View
        /// </summary>
        InDataView,

        /// <summary>
        /// The member of group with group type having an attribute
        /// </summary>
        MemberOfGroupWithGroupTypeHavingAnAttribute
    }
}
