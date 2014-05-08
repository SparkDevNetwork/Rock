// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Communication;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sends email
    /// </summary>
    [Description( "Sends an email.  The recipient can either be a person or email address determined by the 'To Attribute' value, or an email address entered in the 'To' field." )]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Send Email")]

    [TextField("To", "The To address that email should be sent to.", false, "", "", 0)]
    [WorkflowAttribute("To Attribute", "An attribute that contains the person or email address that email should be sent to.", false, "", "", 1)]
    [TextField("From", "The From address that email should be sent from  (will default to organization email).", false, "", "", 2)]
    [TextField("Subject", "The subject that should be used when sending email.", true, "", "", 3)]
    [CodeEditorField("Body", "The body of the email that should be sent", Web.UI.Controls.CodeEditorMode.Html, Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 4)]
    public class SendEmail : CompareAction
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            if (TestCompare(action))
            {
                

            }
            
            return true;
        }
    }
}