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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Humanizer;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.ValueFilter"/> control for editing a simple filter
    /// </summary>
    [ToolboxData( "<{0}:ValueFilter runat=server></{0}:ValueFilter>" )]
    public class ValueFilter : CompositeControl, IRockControl
    {
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
            get
            {
                return ViewState["Label"] as string ?? string.Empty;
            }

            set
            {
                ViewState["Label"] = value;
            }
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
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                ViewState["Required"] = value;
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

        private HiddenField _hfData;
        private CustomValidator _customValidator;

        #endregion

        #region Properties

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
                return ( string ) ViewState["ValidationGroup"];
            }
            set
            {
                ViewState["ValidationGroup"] = value;

                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ValidationGroup = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the filter mode selection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the filter mode selection should be hidden; otherwise, <c>false</c>.
        /// </value>
        public bool HideFilterMode
        {
            get
            {
                return ( bool? ) ViewState["HideFilterMode"] ?? false;
            }
            set
            {
                ViewState["HideFilterMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the supported comparison types.
        /// </summary>
        /// <value>
        /// The supported comparison types.
        /// </value>
        public Model.ComparisonType ComparisonTypes
        {
            get
            {
                return ( Model.ComparisonType? ) ViewState["ComparisonTypes"] ?? ( Reporting.ComparisonHelper.StringFilterComparisonTypes | Model.ComparisonType.RegularExpression );
            }
            set
            {
                ViewState["ComparisonTypes"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>
        /// The filter.
        /// </value>
        public CompoundFilterExpression Filter
        {
            get
            {
                EnsureChildControls();

                return ( CompoundFilterExpression ) FilterExpression.FromJsonOrNull( _hfData.Value ) ?? new CompoundFilterExpression();
            }
            set
            {
                EnsureChildControls();

                _hfData.Value = value.ToJson();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFilter"/> class.
        /// </summary>
        public ValueFilter()
            : base()
        {
            RockControlHelper.Init( this );
            RequiredFieldValidator = null;
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _hfData = new HiddenField
            {
                ID = $"{ this.ID }_hfData",
            };
            Controls.Add( _hfData );

            _customValidator = new CustomValidator
            {
                ID = ID + "_cfv",
                CssClass = "validation-error help-inline js-filtered-text-validator",
                ClientValidationFunction = "Rock.controls.valueFilter.clientValidate",
                ErrorMessage = RequiredErrorMessage,
                Enabled = true,
                Display = ValidatorDisplay.Dynamic,
                ValidationGroup = ValidationGroup
            };
            Controls.Add( _customValidator );
        }

        /// <summary>
        /// Called just before rendering begins on the page.
        /// </summary>
        /// <param name="e">The EventArgs that describe this event.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            RegisterStartupScript();
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
            if ( this.Visible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                {
                    _hfData.RenderControl( writer );
                }
                writer.RenderEndTag();

                _customValidator.RenderControl( writer );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        private void RegisterStartupScript()
        {
            string errorMessage;

            if ( !string.IsNullOrWhiteSpace( RequiredErrorMessage ) )
            {
                errorMessage = RequiredErrorMessage;
            }
            else if ( !string.IsNullOrWhiteSpace( Label ) )
            {
                errorMessage = Label + " Is Required";
            }
            else
            {
                errorMessage = "Filter Field Is Required";
            }

            var comparisionTypeList = ComparisonTypes.GetFlags<Model.ComparisonType>().OrderBy( v => v )
                .Select( v => new
                {
                    Value = ( int ) v,
                    Text = v.Humanize( LetterCasing.Title )
                } )
                .ToList();

            var script = string.Format(
@"
Rock.controls.valueFilter.initialize({{
    controlId: '{0}',
    required: {1},
    requiredMessage: '{2}',
    btnToggleOnClass: '{3}',
    btnToggleOffClass: '{4}',
    hideFilterMode: {5},
    comparisonTypes: {6}
}});
",
                this.ClientID, // {0}
                this.Required.ToString().ToLower(), // {1}
                errorMessage.Replace( "'", "\\'" ), // {2}
                "btn-info", // {3}
                "btn-default", // {4}
                this.HideFilterMode.ToString().ToLower(), // {5}
                comparisionTypeList.ToJson() // {6}
                );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "ValueFilterInitialization_" + this.ClientID, script, true );
        }

        #endregion
    }
}
