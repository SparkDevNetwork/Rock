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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control used by WorkflowDetail block to edit a workflow action
    /// </summary>
    [ToolboxData( "<{0}:WorkflowActionEditor runat=server></{0}:WorkflowActionEditor>" )]
    public class WorkflowActionEditor : CompositeControl, IHasValidationGroup
    {
        private HiddenField _hfActionGuid;
        private Label _lblActionTypeName;
        private Literal _lLastProcessed;
        private Literal _lCompleted;
        private RockCheckBox _cbIsActionCompleted;

        /// <summary>
        /// Gets or sets the action unique identifier.
        /// </summary>
        /// <value>
        /// The activity action identifier.
        /// </value>
        public Guid ActionGuid
        {
            get
            {
                EnsureChildControls();
                return _hfActionGuid.Value.AsGuid();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can edit.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
        /// </value>
        public bool CanEdit
        {
            get
            {
                return ViewState["CanEdit"] as bool? ?? false;
            }
            set
            {
                ViewState["CanEdit"] = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return ViewState["ValidationGroup"] as string;
            }
            set
            {
                ViewState["ValidationGroup"] = value;
            }
        }

        /// <summary>
        /// Gets the workflow action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void GetWorkflowAction( WorkflowAction action )
        {
            EnsureChildControls();

            if ( !action.CompletedDateTime.HasValue && _cbIsActionCompleted.Checked )
            {
                action.CompletedDateTime = RockDateTime.Now;
            }
            else if ( action.CompletedDateTime.HasValue && !_cbIsActionCompleted.Checked )
            {
                action.CompletedDateTime = null;
            }
        }

        /// <summary>
        /// Sets the workflow action.
        /// </summary>
        /// <param name="action">The value.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        public void SetWorkflowAction( WorkflowAction action, bool setValues = false )
        {
            EnsureChildControls();

            _hfActionGuid.Value = action.Guid.ToString();
            _lblActionTypeName.Text = action.ActionTypeCache.Name;

            if ( action.LastProcessedDateTime.HasValue )
            {
                _lLastProcessed.Text = string.Format( "{0} {1} ({2})<br/>",
                    action.LastProcessedDateTime.Value.ToShortDateString(),
                    action.LastProcessedDateTime.Value.ToShortTimeString(),
                    action.LastProcessedDateTime.Value.ToRelativeDateString() );
            }

            if ( action.CompletedDateTime.HasValue )
            {
                _lCompleted.Text = string.Format( "{0} {1} ({2})<br/>",
                    action.CompletedDateTime.Value.ToShortDateString(),
                    action.CompletedDateTime.Value.ToShortTimeString(),
                    action.CompletedDateTime.Value.ToRelativeDateString() );
            }

            _cbIsActionCompleted.Checked = action.CompletedDateTime.HasValue;

        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfActionGuid = new HiddenField();
            Controls.Add( _hfActionGuid );
            _hfActionGuid.ID = this.ID + "_hfActionGuid";

            _lblActionTypeName = new Label();
            Controls.Add( _lblActionTypeName );
            _lblActionTypeName.ClientIDMode = ClientIDMode.Static;
            _lblActionTypeName.ID = this.ID + "_lblActionTypeName";

            _lLastProcessed = new Literal();
            Controls.Add( _lLastProcessed );
            _lLastProcessed.ID = this.ID + "_lLastProcessed";

            _lCompleted = new Literal();
            Controls.Add( _lCompleted );
            _lCompleted.ID = this.ID + "_lCompleted";

            _cbIsActionCompleted = new RockCheckBox();
            Controls.Add( _cbIsActionCompleted );
            _cbIsActionCompleted.ID = this.ID + "_cbIsActionCompleted";
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.RenderBeginTag( HtmlTextWriterTag.Tr );

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            _lblActionTypeName.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            _lLastProcessed.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-select-field" );
            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            _cbIsActionCompleted.Enabled = CanEdit;
            _cbIsActionCompleted.ValidationGroup = ValidationGroup;
            _cbIsActionCompleted.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Td );
            _lCompleted.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

    }
}