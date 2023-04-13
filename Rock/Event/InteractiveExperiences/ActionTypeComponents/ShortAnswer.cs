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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Event.InteractiveExperiences;

namespace Rock.Event.InteractiveExperiences.ActionTypeComponents
{
    /// <summary>
    /// The Short Answer action type. This displays a question and allows the
    /// individual to provide a short text answer.
    /// </summary>
    [Description( "Displays a question and allows the individual to provide a short text answer." )]
    [Export( typeof( ActionTypeComponent ) )]
    [ExportMetadata( "ComponentName", "Short Answer" )]

    [MemoField( "Question",
        IsRequired = true,
        AllowHtml = true, // We HTML escape the text, so it is safe to allow < and > characters.
        Order = 0,
        Key = AttributeKey.Question )]

    [Rock.SystemGuid.EntityTypeGuid( "5ffe1f8f-5f0b-4b34-9c3f-1706d9093210" )]
    internal class ShortAnswer : ActionTypeComponent
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Question = "actionQuestion";
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string IconCssClass => "fa fa-comment-o";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string GetDisplayTitle( InteractiveExperienceAction action )
        {
            if ( action.Attributes == null )
            {
                LoadAttributes( action );
            }

            return GetAttributeValue( action, AttributeKey.Question );
        }

        /// <inheritdoc/>
        public override ActionRenderConfigurationBag GetRenderConfiguration( InteractiveExperienceAction action )
        {
            var bag = base.GetRenderConfiguration( action );

            bag.ConfigurationValues.Add( "question", GetAttributeValue( action, AttributeKey.Question ) );

            return bag;
        }

        #endregion
    }
}
