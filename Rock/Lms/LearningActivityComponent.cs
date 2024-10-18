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
        public LearningActivityComponent( string componentFilePath ) : base( componentFilePath )
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LearningActivityComponent" /> class.
        /// </summary>
        public LearningActivityComponent() : base()
        {
        }

        #endregion

        /// <summary>
        /// Scrubs the configuration data, removing anything the student should not know before completion.
        /// </summary>
        /// <remarks>
        ///     This method should be used to remove any indication of correct answers from the configuration
        ///     before the configuration data is sent to the <see cref="Model.LearningParticipant">Student</see> for completion.
        /// </remarks>
        /// <param name="rawConfigurationJsonString">The unparsed activity configuration data.</param>
        /// <returns>The configuration data scrubbed of sensitive content as a JSON <c>string</c></returns>
        public virtual string StudentScrubbedConfiguration( string rawConfigurationJsonString ) => rawConfigurationJsonString;

        /// <summary>
        /// Gets the completion JSON string that should be persisted to the database.
        /// </summary>
        /// <remarks>
        /// By default the rawCompletionJsonString is returned, but if the StudentScrubbedConfiguration method removed something that
        /// should be persisted with the final completion JSON string, this method could be overridden to re-add those values from the rawConfigurationJsonString.
        /// </remarks>
        /// <param name="rawCompletionJsonString"> The unparsed activity completion data.</param>
        /// <param name="rawConfigurationJsonString">The unparsed activity configuration data.</param>
        /// <returns>The unparsed completion data that should be persisted to the database.</returns>
        public virtual string GetCompletionJsonToPersist( string rawCompletionJsonString, string rawConfigurationJsonString ) => rawCompletionJsonString;

        /// <summary>
        /// Calculates the points earned based on the configuration and completion JSON's and the maximum points possible.
        /// </summary>
        /// <remarks>
        /// This method returns all points by default. To change this behavior, override the <see cref="CalculatePointsEarned"/> method.
        /// </remarks>
        /// <param name="rawConfigurationJsonString">The unparsed activity configuration data.</param>
        /// <param name="rawCompletionJsonString"> The unparsed activity completion data.</param>
        /// <param name="pointsPossible">The maximum points possible for the activity./></param>
        /// <returns>The actual points earned.</returns>
        public virtual int CalculatePointsEarned( string rawConfigurationJsonString, string rawCompletionJsonString, int pointsPossible ) => pointsPossible;

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

    }
}
