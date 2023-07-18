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

using System;
using System.Collections.Generic;

namespace Rock.AI.Classes.Moderations
{
    /// <summary>
    /// The class for holding the response from a completion.
    /// </summary>
    public class ModerationsResponseCategories
    {
        /// <summary>
        /// Is the text hateful.
        /// </summary>
        public bool IsHate { get; set; }

        /// <summary>
        /// Level of hate in the text.
        /// </summary>
        public double HateScore { get; set; }

        /// <summary>
        /// Is the text a threat.
        /// </summary>
        public bool IsThreat { get; set; }

        /// <summary>
        /// Level of threat in the text.
        /// </summary>s
        public double ThreatScore { get; set; }

        /// <summary>
        /// Does the text indicate self-harm.
        /// </summary>
        public bool IsSelfHarm { get; set; }

        /// <summary>
        /// Level of self-harm in the text.
        /// </summary>
        public double SelfHarmScore { get; set; }

        /// <summary>
        /// Is the text sexual.
        /// </summary>
        public bool IsSexual { get; set; }

        /// <summary>
        /// The level sexual content in the text.
        /// </summary>
        public double SexualScore { get; set; }

        /// <summary>
        /// Does the text indicate sexual content with a minor.
        /// </summary>
        public bool IsSexualMinor { get; set; }

        /// <summary>
        /// Level of indication of sexual content with a minor.
        /// </summary>
        public double SexualMinorScore { get; set; }

        /// <summary>
        /// Is the text violent in nature.
        /// </summary>
        public bool IsViolent { get; set; }

        /// <summary>
        /// Level of violence indicated in the text.
        /// </summary>
        public double ViolentScore { get; set; }

        /// <summary>
        /// Returns the moderation score.
        /// </summary>
        public Enums.AI.ModerationFlags ModerationFlags
        {
            get
            {
                var categories = Enums.AI.ModerationFlags.None;

                if ( IsHate )
                {
                    categories |= Enums.AI.ModerationFlags.Hate;
                }

                if ( IsThreat )
                {
                    categories |= Enums.AI.ModerationFlags.Threat;
                }

                if ( IsSelfHarm )
                {
                    categories |= Enums.AI.ModerationFlags.SelfHarm;
                }

                if ( IsSexual )
                {
                    categories |= Enums.AI.ModerationFlags.Sexual;
                }

                if ( IsViolent )
                {
                    categories |= Enums.AI.ModerationFlags.Violent;
                }

                if ( IsSexualMinor )
                {
                    categories |= Enums.AI.ModerationFlags.SexualMinor;
                }

                return categories;
            }
        }
    }
}
