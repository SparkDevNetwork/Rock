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

    [CodeEditorField( "Unsubscribe HTML", "The HTML to inject into email contents when the communication is a Bulk Communication.  Contents will be placed wherever the 'Unsubscribe HTML' merge field is used, or if not used, at the end of the email in email contents.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, @"
<a href='{{ 'Global' | Attribute:'PublicApplicationRoot' }}Unsubscribe/{{ Person | PersonActionIdentifier:'Unsubscribe' }}'>Unsubscribe</a>", "", 2 )]
    [CodeEditorField( "Non-HTML Content", "The text to display for email clients that do not support html content.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, @"
Unfortunately, you cannot view the contents of this email as it contains formatting that is not supported 
by your email client.  

You can view an online version of this email here: 
{{ 'Global' | Attribute:'PublicApplicationRoot' }}GetCommunication.ashx?c={{ Communication.Id }}&p={{ Person | PersonActionIdentifier:'Unsubscribe' }}
", "", 3, "DefaultPlainText" )]
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
    }
}
