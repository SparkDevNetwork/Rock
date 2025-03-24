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

using Rock.AI.Provider;
using Rock.Enums.AI;
using Rock.Model;

namespace Rock.AI.Automations
{
    /// <summary>
    /// POCO for storing <see cref="Category"/> <see cref="AttributeValue">AttributeValues</see> for the 'AI Automations" Attribute Category.
    /// </summary>
    /// <remarks>
    /// Each "AI Automations" AttributeValue for the Category will be parsed into it's respective property.
    /// </remarks>
    public class AIAutomation
    {
        /// <summary>
        /// The <see cref="Rock.Model.AIProvider"/> to be used by the <see cref="Category"/>.
        /// </summary>
        public AIProvider AIProvider { get; set; }

        private AIProviderComponent _aiComponent;

        /// <summary>
        /// The AI provider component for interacting with an LLM.
        /// </summary>
        public AIProviderComponent AIProviderComponent
        {
            get
            {
                if (_aiComponent == null )
                {
                    _aiComponent = AIProvider.GetAIComponent();
                }

                return _aiComponent;
            }
        }

        /// <summary>
        /// The Model to use with the AI provider for completions.
        /// </summary>
        public string AIModel { get; set; }

        /// <summary>
        /// Whether text enhancement should be performed by the AI automation.
        /// </summary>
        public TextEnhancement TextEnhancement { get; set; }

        /// <summary>
        /// Whether names should be removed by the AI automation.
        /// </summary>
        public NameRemoval RemoveNames { get; set; }

        /// <summary>
        /// Whether the emotional sentiment of the text should be classified by the AI automation.
        /// </summary>
        public bool ClassifySentiment { get; set; }

        /// <summary>
        /// Whether the AI automation should attempt to auto-categorize the text based on a provided list of categories.
        /// </summary>
        public bool AutoCategorize { get; set; }

        /// <summary>
        /// Whether the AI automation should attempt to determine if the text is appropriate for public viewing.
        /// </summary>
        public bool CheckPublicAppropriateness { get; set; }

        /// <summary>
        /// The list of child categories for the category.
        /// </summary>
        /// <remarks>
        /// This value will only be populated if the <see cref="AutoCategorize"/> property is <c>true</c>.
        /// </remarks>
        public IEnumerable<Category> ChildCategories { get; set; }

        /// <summary>
        /// Whether the AI automation should perform moderation on the text.
        /// </summary>
        public bool EnableAIModeration { get; set; }

        /// <summary>
        /// The workflow to trigger if any moderation category is detected by the AI automation.
        /// </summary>
        public WorkflowType ModerationAlertWorkflowType { get; set; }
    }
}
