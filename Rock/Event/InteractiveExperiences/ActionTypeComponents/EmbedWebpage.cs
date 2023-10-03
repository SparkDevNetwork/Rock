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
    /// The Embed Webpage action type. Embeds the webpage specified by
    /// the URL and fills the entire question and answer area.
    /// </summary>
    [Description( "Embeds the webpage specified by the URL and fills the entire question and answer area." )]
    [Export( typeof( ActionTypeComponent ) )]
    [ExportMetadata( "ComponentName", "Embed Webpage" )]

    [UrlLinkField( "URL",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.URL )]

    [Rock.SystemGuid.EntityTypeGuid( "3aefe3aa-41a1-4812-86e2-38255b965001" )]
    internal class EmbedWebpage : ActionTypeComponent
    {
        #region Keys

        private static class AttributeKey
        {
            public const string URL = "actionUrl";
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string IconCssClass => "fa fa-pager";

        /// <inheritdoc/>
        public override bool IsModerationSupported => false;

        /// <inheritdoc/>
        public override bool IsQuestionSupported => false;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string GetDisplayTitle( InteractiveExperienceAction action )
        {
            if ( action.Attributes == null )
            {
                LoadAttributes( action );
            }

            return GetAttributeValue( action, AttributeKey.URL );
        }

        /// <inheritdoc/>
        public override ActionRenderConfigurationBag GetRenderConfiguration( InteractiveExperienceAction action )
        {
            var bag = base.GetRenderConfiguration( action );

            if ( action.Attributes == null )
            {
                LoadAttributes( action );
            }

            var url = GetAttributeValue( action, AttributeKey.URL );

            bag.ConfigurationValues.Add( "url", url );

            return bag;
        }

        #endregion
    }
}
