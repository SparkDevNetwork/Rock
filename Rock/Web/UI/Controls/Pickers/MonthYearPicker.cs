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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class MonthYearPicker : CompositeControl, IRockControl, IRockChangeHandlerControl
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
            get 
            {
                EnsureChildControls();
                return _monthDropDownList.ValidationGroup;
            }

            set 
            {
                EnsureChildControls();
                _monthDropDownList.ValidationGroup = value;
                _yearDropDownList.ValidationGroup = value;
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

        private DropDownList _monthDropDownList;
        private DropDownList _yearDropDownList;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the selected date.
        /// </summary>
        /// <value>
        /// The selected date.
        /// </value>
        public DateTime? SelectedDate
        {
            get
            {
                EnsureChildControls();
                int? selectedMonth = _monthDropDownList.SelectedValueAsInt( true );
                int? selectedYear = _yearDropDownList.SelectedValueAsInt( true );

                if ( selectedMonth.HasValue && selectedYear.HasValue )
                {
                    return new DateTime( selectedYear.Value, selectedMonth.Value, 1 );
                }

                return null;
            }

            set
            {
                EnsureChildControls();
                if ( value != null )
                {
                    _monthDropDownList.SelectedValue = value.Value.Month.ToString();
                    _yearDropDownList.SelectedValue = value.Value.Year.ToString();
                }
                else
                {
                    _monthDropDownList.SelectedValue = string.Empty;
                    _yearDropDownList.SelectedValue = string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum year.
        /// </summary>
        /// <value>
        /// The minimum year.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The Minimum Year." )
        ]
        public int MinimumYear
        {
            get
            {
                int? year = ViewState["MinimumYear"] as int?;
                return year ?? 1970;
            }

            set
            {
                if ( ( ViewState["MinimumYear"] as int? ?? 1970 ) != value )
                {
                    ViewState["MinimumYear"] = value;
                    EnsureChildControls();
                    BindYears();
                }

            }
        }

        /// <summary>
        /// Gets or sets the maximum year.
        /// </summary>
        /// <value>
        /// The maximum year.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The Maximum Year." )
        ]
        public int MaximumYear
        {
            get
            {
                int? year = ViewState["MaximumYear"] as int?;
                return year ?? RockDateTime.Now.Year + 20;
            }

            set
            {
                if (( ViewState["MaximumYear"] as int? ?? RockDateTime.Now.Year + 20 ) != value)
                {
                    ViewState["MaximumYear"] = value;
                    EnsureChildControls();
                    BindYears();
                }

            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthYearPicker"/> class.
        /// </summary>
        public MonthYearPicker()
            : base()
        {
            RockControlHelper.Init( this );
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

            _monthDropDownList = new DropDownList();
            Controls.Add( _monthDropDownList );
            _monthDropDownList.ID = "monthDropDownList";
            _monthDropDownList.SelectedIndexChanged += monthYearDropDownList_SelectedIndexChanged;
            _monthDropDownList.CssClass = "form-control input-width-sm";

            BindMonths();

            _yearDropDownList = new DropDownList();
            Controls.Add( _yearDropDownList );
            _yearDropDownList.ID = "yearDropDownList_";
            _yearDropDownList.CssClass = "form-control input-width-sm";

            this.RequiredFieldValidator.ControlToValidate = _yearDropDownList.ID;

            BindYears();
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
            bool needsAutoPostBack = SelectedMonthYearChanged != null || ValueChanged != null;
            _monthDropDownList.AutoPostBack = needsAutoPostBack;
            _yearDropDownList.AutoPostBack = needsAutoPostBack;

            writer.AddAttribute("class", "form-control-group");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            _monthDropDownList.RenderControl( writer );
            writer.Write(" <span class='separator'>/</span> ");
            _yearDropDownList.RenderControl( writer );

            writer.RenderEndTag();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the monthYearDropDownList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void monthYearDropDownList_SelectedIndexChanged( object sender, EventArgs e )
        {
            SelectedMonthYearChanged?.Invoke( this, e );
            ValueChanged?.Invoke( this, e );
        }

        /// <summary>
        /// Occurs when [selected month year changed].
        /// </summary>
        public event EventHandler SelectedMonthYearChanged;

        /// <summary>
        /// Occurs when the selected value has changed
        /// </summary>
        public event EventHandler ValueChanged;

        #endregion

        #region Methods

        private void BindMonths()
        {
            string currentValue = _monthDropDownList.SelectedValue;

            _monthDropDownList.Items.Clear();
            _monthDropDownList.SelectedIndex = -1;
            _monthDropDownList.SelectedValue = null;
            _monthDropDownList.ClearSelection();

            _monthDropDownList.Items.Add( new ListItem( string.Empty, string.Empty ) );
            DateTime date = new DateTime( 2000, 1, 1 );
            for ( int i = 0; i <= 11; i++ )
            {
                _monthDropDownList.Items.Add( new ListItem( date.AddMonths( i ).ToString( "MMM" ), ( i + 1 ).ToString() ) );
            }

            _monthDropDownList.SetValue( currentValue );
        }

        private void BindYears()
        {
            string currentValue = _yearDropDownList.SelectedValue;

            _yearDropDownList.Items.Clear();
            _yearDropDownList.SelectedIndex = -1;
            _yearDropDownList.SelectedValue = null;
            _yearDropDownList.ClearSelection(); 
            
            _yearDropDownList.Items.Add( new ListItem( string.Empty, string.Empty ) );
            for ( int year = this.MinimumYear; year <= this.MaximumYear; year++ )
            {
                _yearDropDownList.Items.Add( new ListItem( year.ToString(), year.ToString() ) );
            }

            _yearDropDownList.SetValue( currentValue );
        }


        #endregion

    }
}