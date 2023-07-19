using Rock.ViewModels.Utility;
using System;

namespace Rock.ViewModels.Blocks.Event.EventCalendarDetail
{
    public class EventAttributeBag
    {
        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public PublicEditableAttributeBag Attribute { get; set; }

        /// <summary>
        /// Gets or sets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public string FieldType { get; set; }
    }
}
