using System;
using System.Collections.Generic;

using Rock.ViewModels.Blocks.Lms.LearningActivityCompletionDetail;
using Rock.ViewModels.Blocks.Lms.LearningClassDetail;
using Rock.ViewModels.Blocks.Lms.LearningParticipantDetail;

namespace Rock.ViewModels.Blocks.Lms.PublicLearningClassWorkspace
{
    public class PublicLearningClassWorkspaceBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the list of activities for this learning class.
        /// </summary>
        public List<LearningActivityCompletionBag> Activities { get; set; }

        /// <summary>
        /// Gets or sets the id for the class.
        /// </summary>
        public string ClassIdKey { get; set; }

        /// <summary>
        /// Gets or sets the id for the course.
        /// </summary>
        public string CourseIdKey { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the image for the course.
        /// </summary>
        public Guid? CourseImageGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the course.
        /// </summary>
        public string CourseName { get; set; }

        /// <summary>
        /// Gets or sets the summary of the course.
        /// </summary>
        public string CourseSummary { get; set; }

        /// <summary>
        /// Gets or sets the list of facilitators for the learning class.
        /// </summary>
        public List<LearningClassFacilitatorBag> Facilitators { get; set; }

        /// <summary>
        /// Gets or sets the participant accessing the course for the learning class.
        /// </summary>
        public LearningParticipantBag ParticipantBag { get; set; }
    }
}
