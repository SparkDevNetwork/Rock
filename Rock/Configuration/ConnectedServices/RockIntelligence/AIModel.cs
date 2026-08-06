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

namespace Rock.Configuration.ConnectedServices.RockIntelligence
{
    /// <summary>
    /// A single AI model that represents a specific role in the Rock Intelligence
    /// provider. This is used to determine which model to use for different types
    /// of tasks.
    /// </summary>
    internal class AIModel
    {
        /// <summary>
        /// The <see cref="Type"/> value that represents the general-purpose
        /// model for chat and standard tasks.
        /// </summary>
        public static readonly string GeneralType = "General";

        /// <summary>
        /// The <see cref="Type"/> value that represents the code-specific
        /// model for programming tasks.
        /// </summary>
        public static readonly string CodeType = "Code";

        /// <summary>
        /// The <see cref="Type"/> value that represents the moderation
        /// model for content review and filtering tasks.
        /// </summary>
        public static readonly string ModerationType = "Moderation";

        /// <summary>
        /// The identifier (name) of the model.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The display name of the model. This is also used to match to the
        /// correct <see cref="Enums.AI.Agent.ModelServiceRole"/> value.
        /// </summary>
        public string Type { get; set; }
    }
}
