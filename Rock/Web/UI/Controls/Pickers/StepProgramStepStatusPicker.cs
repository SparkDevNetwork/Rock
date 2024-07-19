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
    /// Control that can be used to select a step program and then a step status from that program
    /// </summary>
    public class StepProgramStepStatusPicker : CompositeControl, IRockControl
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
                return _sspStepStatusPicker.Required;
            }
            set
            {
                EnsureChildControls();
                _sspStepStatusPicker.Required = value;
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

        private StepProgramPicker _sppStepProgramPicker;
        private StepStatusPicker _sspStepStatusPicker;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the default step program identifier. If this is set, then the step program selected is locked and not visible.
        /// </summary>
        /// <value>
        /// The default step program identifier.
        /// </value>
        public int? DefaultStepProgramId
        {
            get
            {
                return ViewState["DefaultStepProgramId"] as int?;
            }

            set
            {
                ViewState["DefaultStepProgramId"] = value;
                EnsureChildControls();
                _sppStepProgramPicker.SelectedValue = value.ToStringSafe();
                _sspStepStatusPicker.StepProgramId = value;
            }
        }

        /// <summary>
        /// Gets or sets the step program id.
        /// </summary>
        public int? StepProgramId
        {
            get
            {
                EnsureChildControls();
                return DefaultStepProgramId ?? _sppStepProgramPicker.SelectedValue.AsIntegerOrNull();
            }

            set
            {
                if ( DefaultStepProgramId.HasValue )
                {
                    return;
                }

                EnsureChildControls();
                _sppStepProgramPicker.SelectedValue = value.ToStringSafe();
                _sspStepStatusPicker.StepProgramId = value;
            }
        }

        /// <summary>
        /// Gets or sets the step status identifier.
        /// </summary>
        public int? StepStatusId
        {
            get
            {
                EnsureChildControls();
                return _sspStepStatusPicker.SelectedValue.AsIntegerOrNull();
            }

            set
            {
                EnsureChildControls();
                var stepStatusId = value ?? 0;

                if ( _sspStepStatusPicker.SelectedValue != stepStatusId.ToString() )
                {
                    if ( !DefaultStepProgramId.HasValue && ( !StepProgramId.HasValue || StepProgramId.Value == 0 ) )
                    {
                        var stepStatus = new StepStatusService( new RockContext() ).Queryable().AsNoTracking().FirstOrDefault( st => st.Id == stepStatusId );

                        if ( stepStatus != null && _sppStepProgramPicker.SelectedValue != stepStatus.StepProgramId.ToString() )
                        {
                            _sppStepProgramPicker.SelectedValue = stepStatus.StepProgramId.ToString();
                            _sspStepStatusPicker.StepProgramId = stepStatus.StepProgramId;
                        }
                    }

                    _sspStepStatusPicker.SelectedValue = stepStatusId.ToString();
                }
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StepProgramStepStatusPicker"/> class.
        /// </summary>
        public StepProgramStepStatusPicker()
            : base()
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

            _sppStepProgramPicker = new StepProgramPicker();
            _sppStepProgramPicker.ID = this.ID + "_sppStepProgramPicker";
            _sppStepProgramPicker.AutoPostBack = true;
            _sppStepProgramPicker.SelectedIndexChanged += _sppStepProgramPicker_SelectedIndexChanged;
            _sppStepProgramPicker.Label = "Step Program";
            StepProgramPicker.LoadDropDownItems( _sppStepProgramPicker, true );
            Controls.Add( _sppStepProgramPicker );

            _sspStepStatusPicker = new StepStatusPicker();
            _sspStepStatusPicker.ID = this.ID + "_stpStepStatusPicker";
            _sspStepStatusPicker.Label = "Step Status";
            Controls.Add( _sspStepStatusPicker );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _sppStepProgramPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _sppStepProgramPicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            _sspStepStatusPicker.StepProgramId = _sppStepProgramPicker.SelectedValueAsId();
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
            // Don't remove Id as this is required if this control is defined as attribute Field in Bulk Update
            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "class", "form-control-group " + this.FormGroupCssClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _sppStepProgramPicker.Visible = !DefaultStepProgramId.HasValue;
            _sspStepStatusPicker.Visible = StepProgramId.HasValue;
            _sppStepProgramPicker.RenderControl( writer );
            _sspStepStatusPicker.RenderControl( writer );

            writer.RenderEndTag();
        }
    }
}