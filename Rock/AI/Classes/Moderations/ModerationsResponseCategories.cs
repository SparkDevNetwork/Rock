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

using Rock.Enums.AI;

namespace Rock.AI.Classes.Moderations
{
    /// <summary>
    /// The class for holding the response from a moderations completion.
    /// </summary>
    public class ModerationsResponseCategories
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModerationsResponseCategories"/> class.
        /// </summary>
        public ModerationsResponseCategories() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModerationsResponseCategories"/> class
        /// with the flags set based on the provided <paramref name="moderationFlags"/>.
        /// </summary>
        /// <param name="moderationFlags"></param>
        public ModerationsResponseCategories( ModerationFlags moderationFlags )
        {
            IsHate = moderationFlags == ModerationFlags.Hate;
            IsThreat = moderationFlags == ModerationFlags.Threat;
            IsSelfHarm = moderationFlags == ModerationFlags.SelfHarm;
            IsSexual = moderationFlags == ModerationFlags.Sexual;
            IsViolent = moderationFlags == ModerationFlags.Violent;
            IsSexualMinor = moderationFlags == ModerationFlags.SexualMinor;
        }

        /// <summary>
        /// Is the text hateful.
        /// </summary>
        /// <remarks>
        /// Content that expresses, incites, or promotes hate based on race, gender,
        /// ethnicity, religion, nationality, sexual orientation, disability status, or caste.
        /// Hateful content aimed at non-protected groups (e.g. chess players) is harassment. 
        /// </remarks>
        public bool IsHate { get; set; }

        /// <summary>
        /// Level of hate in the text.
        /// </summary>
        public double HateScore { get; set; }

        /// <summary>
        /// Is the text a threat.
        /// </summary>
        /// <remarks>
        /// Harassment content that also includes violence or serious harm towards any target.
        /// </remarks>
        public bool IsThreat { get; set; }

        /// <summary>
        /// Level of threat in the text.
        /// </summary>s
        public double ThreatScore { get; set; }

        /// <summary>
        /// Does the text indicate self-harm.
        /// </summary>
        /// <remarks>
        /// Content that promotes, encourages, or depicts acts of self-harm,
        /// such as suicide, cutting, and eating disorders.
        /// </remarks>
        public bool IsSelfHarm { get; set; }

        /// <summary>
        /// Level of self-harm in the text.
        /// </summary>
        public double SelfHarmScore { get; set; }

        /// <summary>
        /// Is the text sexual.
        /// </summary>
        /// <remarks>
        /// Content meant to arouse sexual excitement, such as the description of sexual activity,
        /// or that promotes sexual services (excluding sex education and wellness).
        /// </remarks>
        public bool IsSexual { get; set; }

        /// <summary>
        /// The level sexual content in the text.
        /// </summary>
        public double SexualScore { get; set; }

        /// <summary>
        /// Does the text indicate sexual content with a minor.
        /// </summary>
        /// <remarks>
        /// Sexual content that includes an individual who is under 18 years old.
        /// </remarks>
        public bool IsSexualMinor { get; set; }

        /// <summary>
        /// Level of indication of sexual content with a minor.
        /// </summary>
        public double SexualMinorScore { get; set; }

        /// <summary>
        /// Is the text violent in nature.
        /// </summary>
        /// <remarks>
        /// Content that depicts death, violence, or physical injury.
        /// </remarks>
        public bool IsViolent { get; set; }

        /// <summary>
        /// Level of violence indicated in the text.
        /// </summary>
        public double ViolentScore { get; set; }

        /// <summary>
        /// Returns the moderation score.
        /// </summary>
        public ModerationFlags ModerationFlags
        {
            get
            {
                var categories = ModerationFlags.None;

                if ( IsHate )
                {
                    categories |= ModerationFlags.Hate;
                }

                if ( IsThreat )
                {
                    categories |= ModerationFlags.Threat;
                }

                if ( IsSelfHarm )
                {
                    categories |= ModerationFlags.SelfHarm;
                }

                if ( IsSexual )
                {
                    categories |= ModerationFlags.Sexual;
                }

                if ( IsViolent )
                {
                    categories |= ModerationFlags.Violent;
                }

                if ( IsSexualMinor )
                {
                    categories |= ModerationFlags.SexualMinor;
                }

                return categories;
            }
        }
    }
}
