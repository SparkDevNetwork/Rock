using System.Collections.Generic;
using System.Linq;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents the Path 'Area 1 > Area2 > Area3' of CheckInAreas
    /// </summary>
    public class CheckinAreaPath
    {
        /// <summary>
        /// Gets the checkin area ancestors path, not including the specified rootGroupType
        /// </summary>
        /// <param name="checkinArea">The checkin area.</param>
        /// <param name="rootGroupType">Type of the root group.</param>
        /// <returns></returns>
        private void SetCheckinAreaAncestorsPathAndSortOrder( GroupTypeCache checkinArea, GroupTypeCache rootGroupType )
        {
            List<GroupTypeCache> parentGroupTypeList = new List<GroupTypeCache>();
            var parentGroupType = checkinArea;

            bool hasMultipleParents = false;

            while ( parentGroupType != null )
            {
                if ( rootGroupType != null && parentGroupType.Id == rootGroupType.Id )
                {
                    // don't include the specified rootGroupType in the path
                    break;
                }

                if ( parentGroupTypeList.Any( a => a.Id == parentGroupType.Id ) )
                {
                    // if we already saw this group, we are in a circular reference, so just go with we have at this point.
                    break;
                }

                parentGroupTypeList.Insert( 0, parentGroupType );

                if ( parentGroupType.ParentGroupTypes.Count > 1 )
                {
                    hasMultipleParents = true;

                    /* A CheckinArea shouldn't have more than 1 parent, but since this one does,
                       we'll have to parent path the makes the most sense
                    */

                    // if the group type has the specified rootGroupType as a parent,
                    // we can stop there since that would be the best path 
                    if ( parentGroupType.ParentGroupTypes.Any( x => x.Id == rootGroupType.Id ) )
                    {
                        break;
                    }

                    // We can eliminate parent group types that we have already discovered,
                    // then pick the first of whatever are remaining, preferring
                    // ones that aren't a Checkin Template group type (like Weekend Service)
                    parentGroupType = parentGroupType.ParentGroupTypes.Where( a => !parentGroupTypeList.Any( p => p.Id == a.Id ) )
                        .OrderByDescending( x => x.GroupTypePurposeValue?.Guid != Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() )
                        .FirstOrDefault();
                }
                else
                {
                    parentGroupType = parentGroupType.ParentGroupTypes.FirstOrDefault();
                }
            }

            if ( hasMultipleParents )
            {
                // In a normal case, the Checkin Template (Weekend Service, Volunteer, etc) doesn't get
                // included as part of the path string, but if there were multiple parent CheckIn Types (normally there shouldn't be),
                // the ParentGroupType list might start with a Checkin Template (Weekend Service, Volunteer, etc).
                // For example, if Elementary Area is in both Weekend Service and Volunteer area,
                // the Path String might get built as 'Volunteer Area > Elementary Area',
                // instead of just 'Elementary Area'.
                // If that happens, we'll trim off the Check-in Template group type from the list.
                var firstParentGroupType = parentGroupTypeList.FirstOrDefault();
                if ( firstParentGroupType != null )
                {
                    if ( firstParentGroupType.GroupTypePurposeValue?.Guid == Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() )
                    {
                        parentGroupTypeList.Remove( firstParentGroupType );
                    }
                }
            }

            this.Path = parentGroupTypeList.Select( a => a.Name ).ToList().AsDelimited( " > " );

            // We're basically building a hierarchy ordering path using padded zeros of the GroupType's order
            // such that the results of the HierarchyOrder looks something like this:
            //// 
            //// |000
            //// |000
            //// |001
            //// |002
            //// |002|000
            //// |002|000|000
            //// |002|001
            //// |003
            //// |004
            this.HierarchyPathString = parentGroupTypeList.Select( a => a.Order.ToString().PadLeft( 3 ) ).ToList().AsDelimited( "|" );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckinAreaPath"/> class.
        /// </summary>
        /// <param name="checkinArea">The checkin area.</param>
        /// <param name="rootGroupType">Type of the root group.</param>
        public CheckinAreaPath( GroupTypeCache checkinArea, GroupTypeCache rootGroupType )
        {
            GroupTypeId = checkinArea.Id;
            this.SetCheckinAreaAncestorsPathAndSortOrder( checkinArea, rootGroupType );
        }

        /// <summary>
        /// Gets or sets the ID of the GroupType (Checkin Area)
        /// </summary>
        /// <value>
        /// ID of the GroupType.
        /// </value>
        public int GroupTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the full associated ancestor path ('Area 1 > Area2 > Area51') of the parent checkin areas (group types).
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the hierarchy path string. <seealso cref="SetCheckinAreaAncestorsPathAndSortOrder"/>
        /// </summary>
        /// <value>
        /// The hierarchy path string.
        /// </value>
        internal string HierarchyPathString { get; private set; }

        /// <summary>
        /// Returns the Path of the CheckinAreaPath
        /// </summary>
        /// <returns>
        /// Returns <seealso cref="Path"/>
        /// </returns>
        public override string ToString()
        {
            return this.Path;
        }
    }
}
