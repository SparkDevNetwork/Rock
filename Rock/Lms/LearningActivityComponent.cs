// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;

using Rock.Obsidian;

namespace Rock.Lms
{
    /// <summary>
    /// Base class for learning activity components
    /// </summary>
    public abstract class LearningActivityComponent : ObsidianComponent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LearningActivityComponent"/> class.
        /// </summary>
        /// <param name="componentFilePath">The path to the Obsidian component's .obs file.</param>
        public LearningActivityComponent(string componentFilePath ): base (componentFilePath )
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LearningActivityComponent" /> class.
        /// </summary>
        public LearningActivityComponent() : base( )
        {
        }

        #endregion

        /// <summary>
        /// Gets the supported configuration.
        /// </summary>
        public readonly string ConfigurationScreen;

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get => new Dictionary<string, string>
            {
                { "Active", "True" },
                { "Order", "0" }
            };
        }

        /// <summary>
        /// Gets or sets the Icon CSS Class for this component.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/> CSS class for the icon.
        /// </value>
        public virtual string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the highlight color of the icon for this component.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/> hex highlight color of the icon.
        /// </value>
        public virtual string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the name of this component.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/> name of the component.
        /// </value>
        public virtual string Name { get; set; }

        //#region Abstract Methods

        ///// <summary>
        ///// Gets the HTML for the configuration screen based on the <see cref="Rock.Model.LearningActivity"/>.
        ///// </summary>
        ///// <param name="learningActivity">The learning activity.</param>
        ///// <returns>HTML markup for configuring the <see cref="LearningActivity"/>.</returns>
        //public abstract string ConfigurationScreenHtml( LearningActivity learningActivity );

        ///// <summary>
        ///// Gets the HTML for the completion screen based on the <see cref="Rock.Model.LearningActivity"/>
        ///// and <see cref="Rock.Model.LearningParticipant"/> accessing the component.
        ///// </summary>
        ///// <param name="learningActivity">The learning activity.</param>
        ///// <param name="participant">The learning participant the HTML screen will be shown to.</param>
        ///// <returns>HTML markup for completing the <see cref="LearningActivity"/>.</returns>
        //public abstract string CompletionScreenHtml( LearningActivity learningActivity, LearningParticipant participant );

        ///// <summary>
        ///// Gets the HTML for the scoring screen based on the <see cref="Rock.Model.LearningActivity"/>.
        ///// </summary>
        ///// <param name="learningActivity">The learning activity.</param>
        ///// <returns>HTML markup for the <see cref="Rock.Model.LearningParticipant">Facilitator</see> to score the <see cref="LearningActivity"/>.</returns>
        //public abstract string FacilitatorScoringScreenHtml( LearningActivity learningActivity );

        ///// <summary>
        ///// Gets the HTML for the summary screen based on the <see cref="Rock.Model.LearningActivity"/>.
        ///// </summary>
        ///// <param name="learningActivity">The learning activity.</param>
        ///// <returns>HTML markup for configuring the <see cref="LearningActivity"/>.</returns>
        //public abstract string SummaryScreenHtml( LearningActivity learningActivity );

        //#endregion Abstract Methods
    }
}
