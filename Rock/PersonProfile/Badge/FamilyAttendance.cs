//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Model;

namespace Rock.PersonProfile.Badge
{
    // TODO: Update to return actual data

    /// <summary>
    /// FamilyAttendance Badge
    /// </summary>
    [Description( "Family Attendance Badge" )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Family Attendance" )]
    public class FamilyAttendance : IconBadge
    {
        /// <summary>
        /// Gets the tool tip text.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetToolTipText( Person person )
        {
            return "Family Attendance Summary for the last 12 months";
        }

        /// <summary>
        /// Gets the icon path.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetIconPath( Person person )
        {
            return "../../../Assets/Mockup/attendence-bars.jpg";
        }
    }
}
