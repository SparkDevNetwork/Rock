using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Chat.Agrees
{
    /// <summary>
    /// A box whose declaration agrees with it once the base box's members and a leading acronym
    /// lowered whole are counted. The public members named in this comment are not members.
    /// </summary>
    public class AgreesBox : FixtureBox
    {
        /// <summary>
        /// Gets or sets the reminder offset, whose leading acronym is lowered whole.
        /// </summary>
        public int RSVPReminderOffsetDays { get; set; }

        /// <summary>
        /// Gets or sets the organization name.
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the campuses.
        /// </summary>
        public List<Dictionary<string, int?>> Campuses { get; set; }

        /// <summary>
        /// A static member, which the generator does not write.
        /// </summary>
        public static string Kind { get; } = "box";

        /// <summary>
        /// A method, which is not a member of the declaration.
        /// </summary>
        public string Describe() { return OrganizationName; }
    }
}
