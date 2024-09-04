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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a interaction channel and then an interaction component of that channel
    /// </summary>
    public class InteractionChannelInteractionComponentPicker : CompositeControl, IRockControl
    {
        #region IRockControl implementation (Custom implementation)

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
            get
            {
                EnsureChildControls();
                return _icompComponentPicker.Required;
            }
            set
            {
                EnsureChildControls();
                _icompComponentPicker.Required = value;
            }
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

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
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
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
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

        #endregion

        #region Controls

        private InteractionChannelPicker _ichanChannelPicker;
        private InteractionComponentPicker _icompComponentPicker;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the default interaction channel identifier. If this is set, then the channel selected is locked and not visible.
        /// </summary>
        public int? DefaultInteractionChannelId
        {
            get
            {
                return ViewState["DefaultInteractionChannelId"] as int?;
            }

            set
            {
                ViewState["DefaultInteractionChannelId"] = value;
                EnsureChildControls();
                _ichanChannelPicker.SelectedValue = value.ToStringSafe();
                _icompComponentPicker.InteractionChannelId = value;
            }
        }

        /// <summary>
        /// Gets or sets the interaction channel id.
        /// </summary>
        public int? InteractionChannelId
        {
            get
            {
                EnsureChildControls();
                return DefaultInteractionChannelId ?? _ichanChannelPicker.SelectedValue.AsIntegerOrNull();
            }

            set
            {
                if ( DefaultInteractionChannelId.HasValue )
                {
                    return;
                }

                EnsureChildControls();
                _ichanChannelPicker.SelectedValue = value.ToStringSafe();
                _icompComponentPicker.InteractionChannelId = value;
            }
        }

        /// <summary>
        /// Gets or sets the interaction component identifier.
        /// </summary>
        public int? InteractionComponentId
        {
            get
            {
                EnsureChildControls();
                return _icompComponentPicker.SelectedValue.AsIntegerOrNull();
            }

            set
            {
                EnsureChildControls();
                var componentId = value ?? 0;

                if ( _icompComponentPicker.SelectedValue != componentId.ToString() )
                {
                    if ( !DefaultInteractionChannelId.HasValue && ( !InteractionChannelId.HasValue || InteractionChannelId.Value == 0 ) )
                    {
                        var rockContext = new RockContext();
                        var interactionComponentService = new InteractionComponentService( rockContext );
                        var component = interactionComponentService.Queryable().AsNoTracking().FirstOrDefault( st => st.Id == componentId );

                        if ( component != null && _ichanChannelPicker.SelectedValue != component.InteractionChannelId.ToString() )
                        {
                            _ichanChannelPicker.SelectedValue = component.InteractionChannelId.ToString();
                            _icompComponentPicker.InteractionChannelId = component.InteractionChannelId;
                        }
                    }

                    _icompComponentPicker.SelectedValue = componentId.ToString();
                }
            }
        }

        /// <summary>
        /// Occurs when [selected index changed].
        /// </summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>
        /// Gets or sets a value indicating whether [automatic post back].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic post back]; otherwise, <c>false</c>.
        /// </value>
        public bool AutoPostBack { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionChannelInteractionComponentPicker"/> class.
        /// </summary>
        public InteractionChannelInteractionComponentPicker() : base()
        {
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
            RockControlHelper.CreateChildControls( this, Controls );

            _ichanChannelPicker = new InteractionChannelPicker();
            _ichanChannelPicker.ID = this.ID + "_ichanChannelPicker";
            _ichanChannelPicker.Help = this.Help;
            Help = string.Empty;
            _ichanChannelPicker.AutoPostBack = true;
            _ichanChannelPicker.SelectedIndexChanged += _ichanChannelPicker_SelectedIndexChanged;
            _ichanChannelPicker.Label = "Interaction Channel";
            InteractionChannelPicker.LoadDropDownItems( _ichanChannelPicker, true );
            Controls.Add( _ichanChannelPicker );

            _icompComponentPicker = new InteractionComponentPicker();
            _icompComponentPicker.ID = this.ID + "_icompComponentPicker";
            _icompComponentPicker.Label = "Interaction Component";
            _icompComponentPicker.AutoPostBack = AutoPostBack;
            _icompComponentPicker.SelectedIndexChanged += _icompComponentPicker_SelectedIndexChanged;
            Controls.Add( _icompComponentPicker );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ichanChannelPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _ichanChannelPicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            _icompComponentPicker.InteractionChannelId = _ichanChannelPicker.SelectedValueAsId();
            SelectedIndexChanged?.Invoke( sender, e );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _icompComponentPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _icompComponentPicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            SelectedIndexChanged?.Invoke( sender, e );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information
        /// about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( Visible )
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
            // Don't remove Id as this is required if this control is defined as attribute Field in Bulk Update
            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", "form-control-group " + this.FormGroupCssClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _ichanChannelPicker.Visible = !DefaultInteractionChannelId.HasValue;
            _icompComponentPicker.Visible = InteractionChannelId.HasValue;
            _ichanChannelPicker.RenderControl( writer );
            _icompComponentPicker.RenderControl( writer );

            writer.RenderEndTag();
        }
    }
}