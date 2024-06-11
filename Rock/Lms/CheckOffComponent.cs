using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Rock.Lms
{
    /// <summary>
    /// The Check-Off activity is a learning activity that requires a participant to check a box for completion.
    /// </summary>
    [Description( "A Learning Activity that requires a participant to check a box of acknowledgement." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "Check-Off" )]

    [Rock.SystemGuid.EntityTypeGuid( "7fae61a2-5f08-4fd9-8bb7-ff7fab410ac5" )]
    public class CheckOffComponent : LearningActivityComponent
    {
        /// <summary>
        /// Gets the Highlight color for the component.
        /// </summary>
        public override string HighlightColor => "#644a88";

        /// <summary>
        /// Gets the icon CSS class for the component.
        /// </summary>
        public override string IconCssClass => "far fa-check-square";

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public override string Name => "Check-Off";

        /// <summary>
        /// Initializes a new instance of the CheckOffComponent.
        /// </summary>
        public CheckOffComponent() : base( @"/Obsidian/Controls/Internal/LearningActivity/checkOffLearningActivity.obs" ) { }
    }
}
