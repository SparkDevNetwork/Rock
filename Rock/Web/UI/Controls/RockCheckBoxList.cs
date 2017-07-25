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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Constants;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:RockCheckBoxList runat=server></{0}:RockCheckBoxList>" )]
    public class RockCheckBoxList : CheckBoxList, IRockControl
    {
        #region Controls

        // hidden field to assist in figuring out which items are checked
        private HiddenField _hfCheckListBoxId;

        #endregion

        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }
            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }
            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return CustomValidator != null ? CustomValidator.ErrorMessage : string.Empty;
            }
            set
            {
                if ( CustomValidator != null )
                {
                    CustomValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || CustomValidator == null || CustomValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        /// <summary>
        /// Gets or sets the custom validator.
        /// </summary>
        /// <value>
        /// The custom validator.
        /// </value>
        public CustomValidator CustomValidator { get; set; }

        /// <summary>
        /// Gets or sets the group of controls for which the control that is derived from the <see cref="T:System.Web.UI.WebControls.ListControl" /> class causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the derived <see cref="T:System.Web.UI.WebControls.ListControl" /> causes validation when it posts back to the server. The default is an empty string ("").</returns>
        public override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }
            set
            {
                base.ValidationGroup = value;
                if ( CustomValidator != null )
                {
                    CustomValidator.ValidationGroup = value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            EnsureChildControls();
            base.OnInit( e );

            string script = string.Format( @" function ValidateCheckboxList_{0}(source, args) {{
                var checkboxes = $(""input[id ^= '{0}']"");
                var isValid = false;
                for ( var i = 0; i < checkboxes.length; i++ )
                {{
                    if ( checkboxes[i].checked) {{
                        isValid = true;
                        break;
                    }}
                }}

                var control = $(""label[for='{0}']"").closest('.rock-check-box-list');
                if (isValid) {{
                    control.removeClass('has-error');
                }} else {{
                    control.addClass('has-error');
                }}

                args.IsValid = isValid;

        }}", this.ClientID);
            ScriptManager.RegisterClientScriptBlock( this, typeof( RockCheckBoxList ), "RockCheckBoxListScript_" + this.ClientID, script, true );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCheckBoxList"/> class.
        /// </summary>
        public RockCheckBoxList()
            : base()
        {
            CustomValidator = new CustomValidator();
            CustomValidator.ValidationGroup = this.ValidationGroup;

            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            _hfCheckListBoxId = new HiddenField();
            _hfCheckListBoxId.ID = "hf";
            _hfCheckListBoxId.Value = "1";

            Controls.Add( _hfCheckListBoxId );

            // add custom validator
            CustomValidator.ID = this.ID + "_cfv";
            CustomValidator.ClientValidationFunction = "ValidateCheckboxList_" + this.ClientID;
            CustomValidator.ErrorMessage = this.Label != string.Empty ? this.Label + " is Required." : string.Empty;
            CustomValidator.CssClass = "validation-error help-inline";
            CustomValidator.Enabled = this.Required;
            CustomValidator.Display = ValidatorDisplay.Dynamic;

            Controls.Add( CustomValidator );

            RockControlHelper.CreateChildControls( this, Controls );
        }

        /// <summary>
        /// Processes the posted data for the <see cref="T:System.Web.UI.WebControls.CheckBoxList" /> control.
        /// </summary>
        /// <param name="postDataKey">The key identifier for the control, used to index the <see cref="T:System.Collections.Specialized.NameValueCollection" /> specified in the <paramref name="postCollection" /> parameter.</param>
        /// <param name="postCollection">A <see cref="T:System.Collections.Specialized.NameValueCollection" /> that contains value information indexed by control identifiers.</param>
        /// <returns>
        /// true if the state of the <see cref="T:System.Web.UI.WebControls.CheckBoxList" /> is different from the last posting; otherwise, false.
        /// </returns>
        protected override bool LoadPostData( string postDataKey, System.Collections.Specialized.NameValueCollection postCollection )
        {
            EnsureChildControls();
            
            // make sure we are dealing with a postback for this control by seeing if the hidden field is included
            if ( postDataKey == _hfCheckListBoxId.UniqueID )
            {
                // Hack to get the selected items on postback.  
                for ( int i = 0; i < this.Items.Count; i++ )
                {
                    this.Items[i].Selected = ( postCollection[string.Format( "{0}${1}", this.UniqueID, i )] != null );
                }

                return false;
            }
            else
            {
                return base.LoadPostData( postDataKey, postCollection );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            _hfCheckListBoxId.RenderControl( writer );
            
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( Items.Count == 0 )
            {
                writer.Write( None.TextHtml );
            }

            base.RenderControl( writer );
            CustomValidator.RenderControl( writer );
            writer.RenderEndTag();
        }

        /// <summary>
        /// Selects the values.
        /// </summary>
        /// <value>
        /// The selected values.
        /// </value>
        public List<string> SelectedValues
        {
            get
            {
                return this.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value ).ToList();
            }
        }

        /// <summary>
        /// Selects the values as int.
        /// </summary>
        /// <value>
        /// The selected values as int.
        /// </value>
        public List<int> SelectedValuesAsInt
        {
            get
            {
                var values = new List<int>();
                foreach ( string stringValue in SelectedValues )
                {
                    int numValue = int.MinValue;
                    if ( int.TryParse( stringValue, out numValue ) )
                    {
                        values.Add( numValue );
                    }
                }
                return values;
            }
        }

    }
}
 
