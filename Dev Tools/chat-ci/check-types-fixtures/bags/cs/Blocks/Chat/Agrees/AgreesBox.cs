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
        /// Gets a label computed from the other members. Its body is an expression, and the
        /// generator writes it as it writes any property.
        /// </summary>
        public string Label => OrganizationName;

        /// <summary>
        /// Gets the kind. The generator reads public properties with no flags, which takes in a
        /// class's own static ones, so it writes this.
        /// </summary>
        public static string Kind { get; } = "box";

        /// <summary>
        /// Gets or sets the summary, overriding the base box's: one member, written once.
        /// </summary>
        public override string Summary { get; set; }

        /// <summary>
        /// Gets a value by key. To the generator an indexer is a property named Item.
        /// </summary>
        public string this[string key] => key;

        /// <summary>
        /// A method, which is not a member of the declaration.
        /// </summary>
        public string Describe() { return OrganizationName; }
    }
}
