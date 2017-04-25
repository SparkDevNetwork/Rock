using System.Collections.Generic;

namespace Rock.Slingshot.Model
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "{Name}" )]
    public class GroupImport
    {
        /// <summary>
        /// Gets or sets the group foreign identifier.
        /// </summary>
        /// <value>
        /// The group foreign identifier.
        /// </value>
        public int GroupForeignId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the parent group foreign identifier.
        /// </summary>
        /// <value>
        /// The parent group foreign identifier.
        /// </value>
        public int? ParentGroupForeignId { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the group members.
        /// </summary>
        /// <value>
        /// The group members.
        /// </value>
        public List<GroupMemberImport> GroupMemberImports { get; set; }
    }
}
