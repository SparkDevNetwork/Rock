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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:WorkflowActionFormEditor runat=server></{0}:WorkflowActionFormEditor>" )]
    public class WorkflowFormEditor : CompositeControl
    {
        private HiddenField _hfFormGuid;
        private RockTextBox _tbHeaderText;
        private RockTextBox _tbFooterText;

        public WorkflowForm Form 
        {
            get
            {
                EnsureChildControls();
                WorkflowForm form = new WorkflowForm();
                form.Guid = _hfFormGuid.Value.AsGuid();
                if ( form.Guid != Guid.Empty )
                {
                    form.Header = _tbHeaderText.Text;
                    form.Footer = _tbFooterText.Text;
                    return form;
                }
                return null;
            }

            set
            {
                EnsureChildControls();
                if ( Form != null )
                {
                    _hfFormGuid.Value = value.Guid.ToString();
                    _tbHeaderText.Text = value.Header;
                    _tbFooterText.Text = value.Footer;
                }
                else
                {
                    _hfFormGuid.Value = string.Empty;
                    _tbHeaderText.Text = string.Empty;
                    _tbFooterText.Text = string.Empty;
                }
            }
        }

        public WorkflowActionType WorkflowActionType { get; set; }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _tbHeaderText = new RockTextBox();
            _tbHeaderText.Label = "Header Text";
            _tbHeaderText.ID = this.ID + "_tbHeaderText";
            Controls.Add( _tbHeaderText );
            
            _tbFooterText = new RockTextBox();
            _tbFooterText.Label = "Label Text";
            _tbFooterText.ID = this.ID + "_tbFooterText";
            Controls.Add( _tbFooterText );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl ( HtmlTextWriter writer )
        {
            if ( _hfFormGuid.Value.AsGuid() != Guid.Empty )
            {
                _tbHeaderText.RenderControl( writer );
                _tbFooterText.RenderControl( writer );
            }
        }

    }
}