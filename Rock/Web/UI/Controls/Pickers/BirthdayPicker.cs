//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
    public class BirthdayPicker : CompositeControl, IRockControl
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
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        private DropDownList monthDropDownList;
        private DropDownList dayDropDownList;
        private DropDownList yearDropDownList;

        /// <summary>
        /// Initializes a new instance of the <see cref="BirthdayPicker"/> class.
        /// </summary>
        public BirthdayPicker()
            : base()
        {
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            monthDropDownList = new DropDownList();
            monthDropDownList.CssClass = "form-control input-width-sm";
            monthDropDownList.ID = "monthDropDownList_" + this.ID;
            monthDropDownList.SelectedIndexChanged += dateList_SelectedIndexChanged;
            
            dayDropDownList = new DropDownList();
            dayDropDownList.CssClass = "form-control input-width-sm";
            dayDropDownList.ID = "dayDropDownList_" + this.ID;
            dayDropDownList.SelectedIndexChanged += dateList_SelectedIndexChanged;

            yearDropDownList = new DropDownList();
            yearDropDownList.CssClass = "form-control input-width-sm";
            yearDropDownList.ID = "yearDropDownList_" + this.ID;
            yearDropDownList.SelectedIndexChanged += dateList_SelectedIndexChanged;

            Controls.Add( monthDropDownList );
            Controls.Add( dayDropDownList );
            Controls.Add( yearDropDownList );

            PopulateDropDowns();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the date dropdown list controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dateList_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( SelectedBirthdayChanged != null )
            {
                SelectedBirthdayChanged( this, e );
            }
        }

        /// <summary>
        /// Occurs when [selected birthday changed].
        /// </summary>
        public event EventHandler SelectedBirthdayChanged;

        /// <summary>
        /// Populates the drop downs.
        /// </summary>
        private void PopulateDropDowns()
        {
            EnsureChildControls();
            monthDropDownList.Items.Clear();
            monthDropDownList.Items.Add( new ListItem( string.Empty, string.Empty ) );
            DateTime date = new DateTime( 2000, 1, 1 );
            for ( int i = 0; i <= 11; i++ )
            {
                monthDropDownList.Items.Add( new ListItem( date.AddMonths( i ).ToString( "MMM" ), ( i + 1 ).ToString() ) );
            }

            dayDropDownList.Items.Clear();
            dayDropDownList.Items.Add( new ListItem( string.Empty, string.Empty ) );
            for ( int day = 1; day <= 31; day++ )
            {
                dayDropDownList.Items.Add( new ListItem( day.ToString(), day.ToString() ) );
            }

            yearDropDownList.Items.Clear();
            yearDropDownList.Items.Add( new ListItem( string.Empty, string.Empty ) );
            for ( int year = DateTime.Now.Year; year >= 1900; year-- )
            {
                yearDropDownList.Items.Add( new ListItem( year.ToString(), year.ToString() ) );
            }

            if ( this.SelectedDate.HasValue )
            {
                monthDropDownList.SelectedValue = this.SelectedDate.Value.Month.ToString();
                dayDropDownList.SelectedValue = this.SelectedDate.Value.Day.ToString();
                yearDropDownList.SelectedValue = this.SelectedDate.Value.Year.ToString();
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
            bool needsAutoPostBack = SelectedBirthdayChanged != null;
            monthDropDownList.AutoPostBack = needsAutoPostBack;
            dayDropDownList.AutoPostBack = needsAutoPostBack;
            yearDropDownList.AutoPostBack = needsAutoPostBack;

            writer.AddAttribute("class", "form-control-group");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            monthDropDownList.RenderControl( writer );
            writer.Write(" <span class='separator'>/</span> ");
            dayDropDownList.RenderControl( writer );
            writer.Write( " <span class='separator'>/</span> " );
            yearDropDownList.RenderControl( writer );

            writer.RenderEndTag();
        }

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
                int? selectedMonth = monthDropDownList.SelectedValueAsInt( true );
                int? selectedDay = dayDropDownList.SelectedValueAsInt( true );
                int? selectedYear = yearDropDownList.SelectedValueAsInt( true );

                if ( selectedMonth.HasValue && selectedDay.HasValue )
                {
                    // if they picked a day of the month that is invalid, just round it to last day that month;
                    int correctedDayOfMonth = Math.Min( DateTime.DaysInMonth( DateTime.MinValue.Year, selectedMonth.Value ), selectedDay.Value );

                    return new DateTime( (selectedYear.HasValue ? selectedYear.Value : DateTime.MinValue.Year), selectedMonth.Value, correctedDayOfMonth );
                }

                return null;
            }

            set
            {
                EnsureChildControls();
                if ( value != null )
                {
                    monthDropDownList.SelectedValue = value.Value.Month.ToString();
                    dayDropDownList.SelectedValue = value.Value.Day.ToString();
                    yearDropDownList.SelectedValue = value.Value.Year != DateTime.MinValue.Year ? value.Value.Year.ToString() : string.Empty;
                }
                else
                {
                    monthDropDownList.SelectedValue = string.Empty;
                    dayDropDownList.SelectedValue = string.Empty;
                    yearDropDownList.SelectedValue = string.Empty;
                }
            }
        }
    }
}