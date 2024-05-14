using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Rock.Lms
{
    /// <summary>
    /// The Check-Off activity is a learning activity that requires a participant to check a box for completion.
    /// </summary>
    [Description( "A Learning Activity that has the facilitator evaluating a presentation or other material." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "Point Assessment" )]

    [Rock.SystemGuid.EntityTypeGuid( "a6e91c3c-4a4c-4fc2-816a-a6b1e6422381" )]
    public class PointAssessmentComponent : LearningActivityComponent
    {
        /// <summary>
        /// Gets the Highlight color for the component.
        /// </summary>
        public override string HighlightColor => "#fbf3c9";

        /// <summary>
        /// Gets the icon CSS class for the component.
        /// </summary>
        public override string IconCssClass => "fa fa-list";

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public override string Name => "Point Assessment";

        /// <summary>
        /// Initializes a new instance of the CheckOffComponent.
        /// </summary>
        public PointAssessmentComponent(): base( @"/Obsidian/Controls/Internal/LearningActivity/pointAssessmentLearningActivity.obs" ) { }
    }
}
