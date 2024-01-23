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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Event.InteractiveExperiences;

namespace Rock.Event.InteractiveExperiences.ActionTypeComponents
{
    /// <summary>
    /// The Poll action type. This displays a question and allows the
    /// individual to select from a fixed set of answers.
    /// </summary>
    [Description( "Displays a question and allows the individual to select from a fixed set of answers." )]
    [Export( typeof( ActionTypeComponent ) )]
    [ExportMetadata( "ComponentName", "Poll" )]

    [MemoField( "Question",
        IsRequired = true,
        AllowHtml = true, // We HTML escape the text, so it is safe to allow < and > characters.
        Order = 0,
        Key = AttributeKey.Question )]

    [ValueListField( "Answers",
        IsRequired = true,
        AllowHtml = true, // We HTML escape the text, so it is safe to allow < and > characters.
        Order = 1,
        Key = AttributeKey.Answers )]

    [Rock.SystemGuid.EntityTypeGuid( "9256a5b7-480d-4ffa-86d1-03b8aefc254e" )]
    internal class Poll : ActionTypeComponent
    {
        #region Keys

        internal static class AttributeKey
        {
            public const string Question = "actionQuestion";

            public const string Answers = "actionAnswers";
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string IconCssClass => "fa fa-list";

        /// <inheritdoc/>
        public override bool IsModerationSupported => false;

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

            if ( action.Attributes == null )
            {
                LoadAttributes( action );
            }

            var rawAnswers = GetAttributeValue( action, AttributeKey.Answers );
            var values = rawAnswers?.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) ?? new string[0];
            values = values.Select( s => System.Net.WebUtility.UrlDecode( s ) ).ToArray();

            bag.ConfigurationValues.Add( "question", GetAttributeValue( action, AttributeKey.Question ) );
            bag.ConfigurationValues.Add( "answers", values.ToJson() );

            return bag;
        }

        #endregion
    }
}
