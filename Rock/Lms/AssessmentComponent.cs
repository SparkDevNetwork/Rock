using System;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Newtonsoft.Json.Linq;

using Rock.Model;

namespace Rock.Lms
{
    /// <summary>
    /// The Assessment activity is an activity that requires a partcipant to complete multiple items.
    /// </summary>
    [Description( "A Learning Activity that requires a partcipant to complete multiple items." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "Assessment" )]

    [Rock.SystemGuid.EntityTypeGuid( "a585c101-02e8-4953-bf77-c783c7cfdfdc" )]
    public class AssessmentComponent : LearningActivityComponent
    {
        /// <summary>
        /// Gets the Highlight color for the component.
        /// </summary>
        public override string HighlightColor => "#a9551d";

        /// <summary>
        /// Gets the icon CSS class for the component.
        /// </summary>
        public override string IconCssClass => "fa fa-list";

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public override string Name => "Assessment";

        /// <summary>
        /// Initializes a new instance of the CheckOffComponent.
        /// </summary>
        public AssessmentComponent(): base( @"/Obsidian/Controls/Internal/LearningActivity/assessmentLearningActivity.obs" ) { }

        /// <summary>
        /// Removes the isCorrect flag from any multiple choice assessment items.
        /// </summary>
        /// <param name="rawConfigurationJsonString"></param>
        /// <returns>The json string stripped of any information that might identify correct answers.</returns>
        public override string StudentScrubbedConfiguration( string rawConfigurationJsonString )
        {
            try
            {
                const string multipleChoiceItemTypeName = "Multiple Choice";
                var correctAnswerPath = $"$.items[?(@.typeName == '{multipleChoiceItemTypeName}')].correctAnswer";
                var jObject = JObject.Parse( rawConfigurationJsonString );
                var correctAnswers = jObject.SelectTokens( correctAnswerPath );

                foreach ( var correctAnswer in correctAnswers)
                {
                    correctAnswer.Parent.Remove();
                }

                return jObject.ToJson();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }

            // If there was an error don't return anything (to prevent leaking answers).
            return string.Empty;
        }
    }
}
