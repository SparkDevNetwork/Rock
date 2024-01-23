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

using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication.Medium
{
    /// <summary>
    /// An email communication
    /// </summary>
    [Description( "An email communication" )]
    [Export( typeof( MediumComponent ) )]
    [ExportMetadata( "ComponentName", "Email" )]

    [CodeEditorField( "Unsubscribe HTML",
        Description = "The HTML to inject into email contents when the communication is a Bulk Communication.  Contents will be placed wherever the 'Unsubscribe HTML' merge field is used, or if not used, at the end of the email in email contents. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Key = AttributeKey.UnsubscribeHTML,
        DefaultValue = @"
<a href='{{ 'Global' | Attribute:'PublicApplicationRoot' }}Unsubscribe/{{ Person | PersonActionIdentifier:'Unsubscribe' }}?CommunicationId={{ Communication.Id }}'>Unsubscribe</a>",
        Order = 2 )]

    [CodeEditorField( "Non-HTML Content",
        Description = "The text to display for email clients that do not support html content. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = @"
Unfortunately, you cannot view the contents of this email as it contains formatting that is not supported 
by your email client.  

You can view an online version of this email here: 
{{ 'Global' | Attribute:'PublicApplicationRoot' }}GetCommunication.ashx?c={{ Communication.Guid }}&p={{ Person | PersonActionIdentifier:'Unsubscribe' }}
",
        Category = "",
        Order = 3,
        Key = AttributeKey.DefaultPlainText )]

    [BooleanField( "CSS Inlining Enabled",
        Description = "Enable to move CSS styles to inline attributes. This can help maximize compatibility with email clients.",
        DefaultBooleanValue = true,
        Key = AttributeKey.CSSInliningEnabled,
        Order = 4)]

    [IntegerField( "Bulk Email Threshold",
        Description = "Auto-hides the 'Is Bulk Email' option when starting a new communication if the recipient count exceeds the specified threshold.",
        IsRequired = false,
        Key = AttributeKey.BulkEmailThreshold,
        Order = 5 )]

    [EmailField( "Request Unsubscribe Email",
        Description = "Used for the 'Mailto Method' of the List-Unsubscribe email header. When blank, the global 'Organization Email' setting is used.",
        IsRequired = false,
        Key = AttributeKey.RequestUnsubscribeEmail,
        Order = 6 )]

    [BooleanField( "Enable One-Click Unsubscribe",
        Description = "When enabled, email clients will use the native one-click to unsubscribe feature to remove themselves from lists.",
        DefaultBooleanValue = true,
        Key = AttributeKey.EnableOneClickUnsubscribe,
        Order = 7 )]

    [TextField( "Unsubscribe URL",
        Description = "Used in the List-Unsubscribe email header when the one-click option is disabled. <span class='tip tip-lava'></span>",
        DefaultValue = "{{ 'Global' | Attribute:'PublicApplicationRoot' }}Unsubscribe/{{ Person | PersonActionIdentifier:'Unsubscribe' }}?CommunicationId={{ Communication.Id }}",
        IsRequired = false,
        Key = AttributeKey.UnsubscribeURL,
        Order = 8 )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL )]
    public class Email : MediumComponent
    {
        #region Keys

        private static class AttributeKey
        {
            public const string UnsubscribeHTML = "UnsubscribeHTML";
            public const string DefaultPlainText = "DefaultPlainText";
            public const string CSSInliningEnabled = "CSSInliningEnabled";
            public const string BulkEmailThreshold = "BulkEmailThreshold";
            public const string RequestUnsubscribeEmail = "RequestUnsubscribeEmail";
            public const string EnableOneClickUnsubscribe = "EnableOneClickUnsubscribe";
            public const string UnsubscribeURL = "UnsubscribeURL";
        }

        #endregion

        /// <summary>
        /// Gets the type of the communication.
        /// </summary>
        /// <value>
        /// The type of the communication.
        /// </value>
        public override CommunicationType CommunicationType {
            get
            {
                return CommunicationType.Email;
            }
        }

        /// <summary>
        /// Gets the control.
        /// </summary>
        /// <param name="useSimpleMode">if set to <c>true</c> [use simple mode].</param>
        /// <returns></returns>
        public override MediumControl GetControl( bool useSimpleMode )
        {
            return new Rock.Web.UI.Controls.Communication.Email( useSimpleMode );
        }

        /// <summary>
        /// Checks whether the bulk email threshold has been exceeded.
        /// </summary>
        /// <param name="recipientCount">The number of communication recipients.</param>
        /// <returns><see langword="true"/> if the bulk email threshold has been exceeded; otherwise, <see langword="false"/>.</returns>
        public bool IsBulkEmailThresholdExceeded( int recipientCount )
        {
            var threshold = GetAttributeValue( AttributeKey.BulkEmailThreshold ).AsIntegerOrNull();

            if ( !threshold.HasValue )
            {
                // No threshold so return false.
                return false;
            }

            return recipientCount > threshold.Value;
        }
    }
}
