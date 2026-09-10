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
using System.Linq;

using Rock.Attribute;
using Rock.SystemGuid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Adaptive Message Field Type. 
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Administrative )]
    [FieldTypeGuid( SystemGuid.FieldType.ADAPTIVE_MESSAGE )]
    internal class AdaptiveMessageFieldType : UniversalItemTreePickerFieldType
    {
        /// <inheritdoc/>
        protected override string GetRootRestUrl( Dictionary<string, string> privateConfigurationValues )
        {
            return "/api/v2/controls/AdaptiveMessagePickerGetAdaptiveMessages";
        }

        /// <inheritdoc/>
        protected override List<ListItemBag> GetItemBags( IEnumerable<string> values, Dictionary<string, string> privateConfigurationValues )
        {
            var guids = values.Select( v => v.AsGuid() ).ToList();

            var messages = AdaptiveMessageCache.GetMany( guids );
            var msgBags = messages
                .Select( m => new ListItemBag
                {
                    Value = m.Guid.ToString(),
                    Text = m.Name
                } ).ToList();

            return msgBags;
        }

        /// <inheritdoc/>
        protected override bool GetFolderSelectionDisabled( Dictionary<string, string> privateConfigurationValues )
        {
            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The picker shows a tree of categories with messages beneath them, but a
        /// category is never a valid value here because folder selection is disabled.
        /// Only the message guids are stored.
        /// </remarks>
        internal override FieldTypeHints GetFieldHints( Dictionary<string, string> privateConfigurationValues )
        {
            return new FieldTypeHints
            {
                IsCompleteList = false,
                ValueFormat = "One or more guids identifying rows in the AdaptiveMessage table, separated by commas. Not their ids or idKeys, and never the guid of a category the messages are filed under.",
                Instructions = "To find the correct values, read the adaptive messages and take the guid of each one you want."
            };
        }
    }
}
