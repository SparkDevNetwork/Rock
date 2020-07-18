﻿// <copyright>
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
        Description = "The HTML to inject into email contents when the communication is a Bulk Communication.  Contents will be placed wherever the 'Unsubscribe HTML' merge field is used, or if not used, at the end of the email in email contents.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = @"
<a href='{{ 'Global' | Attribute:'PublicApplicationRoot' }}Unsubscribe/{{ Person | PersonActionIdentifier:'Unsubscribe' }}'>Unsubscribe</a>",
        Order = 2 )]

    [CodeEditorField( "Non-HTML Content",
        Description = "The text to display for email clients that do not support html content.",
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
        Key = "DefaultPlainText" )]
    public class Email : MediumComponent
    {
        /// <summary>
        /// Gets the type of the communication.
        /// </summary>
        /// <value>
        /// The type of the communication.
        /// </value>
        public override CommunicationType CommunicationType { get { return CommunicationType.Email; } }

        /// <summary>
        /// Gets the control.
        /// </summary>
        /// <param name="useSimpleMode">if set to <c>true</c> [use simple mode].</param>
        /// <returns></returns>
        public override MediumControl GetControl( bool useSimpleMode )
        {
            return new Rock.Web.UI.Controls.Communication.Email( useSimpleMode );
        }

        #region Obsolete 

        /// <summary>
        /// Gets the HTML preview.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "The GetCommunication now creates the HTML Preview directly", true )]
        public override string GetHtmlPreview( Model.Communication communication, Person person )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the read-only message details.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "The CommunicationDetail block now creates the details", true )]
        public override string GetMessageDetails( Model.Communication communication )
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Gets a value indicating whether [supports bulk communication].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports bulk communication]; otherwise, <c>false</c>.
        /// </value>
        [RockObsolete( "1.7" )]
        [Obsolete( "All mediums now support bulk communications", true )]
        public override bool SupportsBulkCommunication
        {
            get
            {
                return true;
            }
        }

        #endregion

    }
}
