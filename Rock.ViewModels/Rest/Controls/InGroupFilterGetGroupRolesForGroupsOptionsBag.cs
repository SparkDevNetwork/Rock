using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Options bag for the InGroupFilter's GetGroupRolesForGroups method.
    /// </summary>
    public class InGroupFilterGetGroupRolesForGroupsOptionsBag
    {
        /// <summary>
        /// Guids of the groups that are of the group types we wish to get roles for.
        /// </summary>
        public List<Guid> GroupGuids { get; set; } = new List<Guid>();

        /// <summary>
        /// Whether or not to include the direct child groups of the groups listed in GroupGuids.
        /// If this is true and IncludeSelectedGroups is false, then the groups in GroupGuids will be excluded.
        /// </summary>
        public bool IncludeChildGroups { get; set; } = false;

        /// <summary>
        /// If <see cref="IncludeChildGroups"/> is true and this is true, then the groups in GroupGuids
        /// will be included along with the child groups when determining the group types.
        /// </summary>
        public bool IncludeSelectedGroups { get; set; } = false;

        /// <summary>
        /// If <see cref="IncludeChildGroups"/> is true and this is true, then also include the child groups
        /// all the way down the hierarchy when determining the group types.
        /// </summary>
        public bool IncludeAllDescendants { get; set; } = false;

        /// <summary>
        /// If <see cref="IncludeChildGroups"/> is true and this is true, then also include inactive groups
        /// </summary>
        public bool IncludeInactiveGroups { get; set; } = false;

        /// <summary>
        /// The security grant token.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}
