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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for selecting colors from a list.
    /// </summary>
    [ToolboxData( "<{0}:ColorSelector runat=server></{0}:ColorSelector>" )]
    public class ColorSelector : CheckBoxList, IRockControl
    {
        #region IRockControl implementation

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
        /// Gets or sets a value indicating whether this <see cref="ColorSelector"/> is required.
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

        /// <inheritdoc/>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }
            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <inheritdoc/>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <inheritdoc/>
        public HelpBlock HelpBlock { get; set; }

        /// <inheritdoc/>
        public WarningBlock WarningBlock { get; set; }

        /// <inheritdoc/>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        /// <summary>
        /// Hidden field to assist in figuring out which items are checked.
        /// </summary>
        private HiddenField _hfCheckBoxListId;
        
        #endregion

        #region Properties

        /// <summary>
        /// <c>true</c> if multiple selection is allowed; otherwise, <c>false</c> for single selection.
        /// </summary>
        public bool AllowMultiple { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorSelector"/> class.
        /// </summary>
        public ColorSelector() : base()
        {
            this.HelpBlock = new HelpBlock();
            this.WarningBlock = new WarningBlock();
            RequiredFieldValidator = new HiddenFieldValidator();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <inheritdoc/>
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "color-selector-items" );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Hidden field
            writer.WriteLine();
            _hfCheckBoxListId.RenderControl( writer );
            writer.WriteLine();

            base.RenderControl( writer );

            var script = $"Rock.controls.colorSelector.initialize({{ controlId: '{this.ClientID}', allowMultiple: {AllowMultiple.ToJavaScriptValue()} }})";

            // On startup, modify the checkboxes so they look like color boxes.
            ScriptManager.RegisterStartupScript( this, typeof( ColorSelector ), "ColorSelectorScript_" + this.ClientID, script, true );

            writer.RenderEndTag();
        }

        #endregion

        #region Private Methods

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            _hfCheckBoxListId = new HiddenField
            {
                ID = "hf",
                Value = "1"
            };
            Controls.Add( _hfCheckBoxListId );

            RockControlHelper.CreateChildControls( this, Controls );

            this.RequiredFieldValidator.ControlToValidate = _hfCheckBoxListId.ID;
        }
        
        /// <inheritdoc/>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            if ( this.Visible && !ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                RockPage.AddScriptLink( Page, "~/Scripts/Rock/Controls/colorSelector.js" );
            }

            EnsureChildControls();
        }


        /// <inheritdoc/>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
        }

        /// <inheritdoc />
        protected override bool LoadPostData( string postDataKey, NameValueCollection postCollection )
        {
            // Make sure we are dealing with a postback for this control by seeing if the hidden field is included.
            if ( postDataKey == _hfCheckBoxListId.UniqueID )
            {
                var hasChanged = false;

                // Hack to get the selected items on postback.
                for ( var i = 0; i < this.Items.Count; i++ )
                {
                    var newCheckState = postCollection[$"{this.UniqueID}{i}"] != null;

                    if ( this.Items[i].Selected != newCheckState )
                    {
                        this.Items[i].Selected = newCheckState;
                        hasChanged = true;
                    }
                }

                return hasChanged;
            }
            else
            {
                return base.LoadPostData( postDataKey, postCollection );
            }
        }

        #endregion
    }
}