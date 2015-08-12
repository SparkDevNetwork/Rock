using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.centralaz.Baptism.Model;
using com.centralaz.Baptism.Data;

using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web;

namespace RockWeb.Plugins.com_centralaz.Baptism
{

    [DisplayName( "Baptism Campus Detail Block" )]
    [Category( "com_centralaz > Baptism" )]
    [Description( "Detail block for Baptism scheduling" )]
    [LinkedPage( "Add Baptism Page", "", true, "", "", 0 )]
    [LinkedPage( "Add Blackout Day Page", "", true, "", "", 0 )]
    [TextField( "Report Font", "", true, "Gotham", "", 0 )]
    public partial class BaptismCampusDetail : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        protected List<Schedule> _blackoutDates;
        protected List<Baptizee> _baptizeeList;
        protected List<Baptizee> _baptizees;
        protected Schedule _blackoutDate;

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( ResolveRockUrl( "~/Plugins/com_centralaz/Baptism/Styles/BaptismCampusDetail.css" ) );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // register btnDumpDiagnostics as a PostBackControl since it is returning a File download
            ScriptManager scriptManager = ScriptManager.GetCurrent( Page );
            scriptManager.RegisterPostBackControl( lbPrintReport );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            GetBlackoutDates();
            UpdateBaptizees();


            if ( !Page.IsPostBack )
            {
                if ( PageParameter( "SelectedDate" ).AsDateTime().HasValue )
                {
                    BindCalendar( PageParameter( "SelectedDate" ).AsDateTime().Value );
                }
                else
                {
                    BindCalendar();
                }
            }
            else
            {
                //  UpdateScheduleList();
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? groupId = PageParameter( pageReference, "GroupId" ).AsIntegerOrNull();
            if ( groupId != null )
            {
                Group group = new GroupService( new RockContext() ).Get( groupId.Value );
                if ( group != null )
                {
                    breadCrumbs.Add( new BreadCrumb( group.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Group", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the Click event of the lbAddBaptism control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddBaptism_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> dictionaryInfo = new Dictionary<string, string>();
            dictionaryInfo.Add( "GroupId", PageParameter( "GroupId" ) );
            dictionaryInfo.Add( "SelectedDate", calBaptism.SelectedDate.ToShortDateString() );
            NavigateToLinkedPage( "AddBaptismPage", dictionaryInfo );
        }

        /// <summary>
        /// Handles the Click event of the lbAddBlackout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddBlackout_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> dictionaryInfo = new Dictionary<string, string>();
            dictionaryInfo.Add( "GroupId", PageParameter( "GroupId" ) );
            dictionaryInfo.Add( "SelectedDate", calBaptism.SelectedDate.ToShortDateString() );
            NavigateToLinkedPage( "AddBlackoutDayPage", dictionaryInfo );
        }

        /// <summary>
        /// Handles the Click event of the lbEditBlackout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEditBlackout_Click( object sender, EventArgs e )
        {
            _blackoutDate = _blackoutDates.Where( b => b.GetCalenderEvent().DTStart.Date == calBaptism.SelectedDate.Date ).FirstOrDefault();
            Dictionary<string, string> dictionaryInfo = new Dictionary<string, string>();
            dictionaryInfo.Add( "GroupId", PageParameter( "GroupId" ) );
            dictionaryInfo.Add( "SelectedDate", calBaptism.SelectedDate.ToShortDateString() );
            dictionaryInfo.Add( "BlackoutId", _blackoutDate.Id.ToString() );
            NavigateToLinkedPage( "AddBlackoutDayPage", dictionaryInfo );
        }

        /// <summary>
        /// Handles the Click event of the lbPrintReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbPrintReport_Click( object sender, EventArgs e )
        {
            //Get the data
            DateTime[] dateRange = GetTheDateRange( calBaptism.SelectedDate );
            Group group = new GroupService( new RockContext() ).Get( PageParameter( "GroupId" ).AsInteger() );
            _baptizeeList = new BaptizeeService( new BaptismContext() ).GetBaptizeesByDateRange( dateRange[0], dateRange[1], group.Id );
            String font = GetAttributeValue( "ReportFont" );

            //Setup the document
            var document = new Document( PageSize.A4, 50, 50, 25, 25 );

            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance( document, output );

            document.Open();

            var titleFont = FontFactory.GetFont( font, 16, Font.BOLD );
            var subTitleFont = FontFactory.GetFont( font, 14, Color.GRAY );

            //Add church logo
            try
            {
                var logo = iTextSharp.text.Image.GetInstance( Server.MapPath( ResolveRockUrl( "~/Plugins/com_centralaz/Baptism/Assets/Icons/CentralChristianChurchArizona_165_90.png" ) ) );
                logo.Alignment = iTextSharp.text.Image.RIGHT_ALIGN;
                logo.ScaleAbsolute( 100, 55 );
                document.Add( logo );
            }
            catch
            {

            }

            //Write the document
            String title = String.Format( "{0}: {1} - {2}", group.Name, dateRange[0].ToString( "MMMM d" ), dateRange[1].ToString( "MMMM d" ) );
            document.Add( new Paragraph( title, titleFont ) );

            String campusName = ( group.Campus != null ) ? group.Campus.ToString() : "Unknown Campus";
            String subTitle = String.Format( "Baptism Schedule for {0}", campusName );
            document.Add( new Paragraph( subTitle, subTitleFont ) );

            //Populate the Lists
            DateTime current = DateTime.MinValue;
            foreach ( Baptizee b in _baptizeeList )
            {
                if ( current != b.BaptismDateTime )
                {
                    current = b.BaptismDateTime;
                    BuildPDFItemListHeader( current, document, font );
                }

                BuildPDFListItem( b, document, font );
            }

            document.Close();

            Response.ClearHeaders();
            Response.ClearContent();
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader( "Content-Disposition", string.Format( "attachment;filename={0} Baptism Schedule.pdf", campusName ) );
            Response.BinaryWrite( output.ToArray() );
            Response.Flush();
            Response.End();
            return;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the calBaptism control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void calBaptism_SelectionChanged( object sender, EventArgs e )
        {
            //    PageParameter( "SelectedDate" ) = calBaptism.SelectedDate.ToShortDateString();
            UpdateSchedulePanel();
        }

        /// <summary>
        /// Handles the DayRender event of the calBaptisms control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DayRenderEventArgs" /> instance containing the event data.</param>
        protected void calBaptisms_DayRender( object sender, DayRenderEventArgs e )
        {
            DateTime day = e.Day.Date;
            if ( _baptizees != null )
            {
                if ( _baptizees.Any( b => b.BaptismDateTime.Date == day.Date ) )
                {
                    e.Cell.Style.Add( "font-weight", "bold" );
                    e.Cell.AddCssClass( "alert-success" );
                }
            }
            if ( _blackoutDates.Any( b => b.GetCalenderEvent().DTStart.Date == day.Date ) )
            {
                e.Cell.AddCssClass( "alert-danger" );
            }

        }

        #endregion

        #region Methods

        /// <summary>
        /// Fills the baptizee List with all baptizees for that group
        /// </summary>
        protected void UpdateBaptizees()
        {
            _baptizees = new BaptizeeService( new BaptismContext() ).GetAllBaptizees( PageParameter( "GroupId" ).AsInteger() );
        }

        /// <summary>
        /// Fills the blackout date list with all blackout dates for that group
        /// </summary>
        protected void GetBlackoutDates()
        {
            Group group = new GroupService( new RockContext() ).Get( PageParameter( "GroupId" ).AsInteger() );
            group.LoadAttributes();
            Guid categoryguid = group.GetAttributeValue( "BlackoutDates" ).AsGuid();
            CategoryCache category = CategoryCache.Read( categoryguid );
            _blackoutDates = new ScheduleService( new RockContext() ).Queryable()
                .Where( s => s.CategoryId == category.Id )
                .ToList();
        }

        /// <summary>
        /// Sets the calendar's seleccted date to today
        /// </summary>
        protected void BindCalendar()
        {

            calBaptism.SelectedDate = DateTime.Today;
            UpdateSchedulePanel();
        }

        /// <summary>
        /// Sets the calendar's selected date to selectedDate
        /// </summary>
        /// <param name="selectedDate"></param>
        protected void BindCalendar( DateTime selectedDate )
        {

            calBaptism.SelectedDate = selectedDate;
            UpdateSchedulePanel();
        }

        /// <summary>
        /// Updates the baptism schedule panel
        /// </summary>
        protected void UpdateSchedulePanel()
        {
            DateTime[] dateRange = GetTheDateRange( calBaptism.SelectedDate );
            Group group = new GroupService( new RockContext() ).Get( PageParameter( "GroupId" ).AsInteger() );
            if ( group == null )
            {
                RockPage.Layout.Site.RedirectToDefaultPage();
            }
            else
            {
                lPanelHeadingDateRange.Text = String.Format( "{0}: {1} - {2}", group.Name, dateRange[0].ToString( "MMMM d" ), dateRange[1].ToString( "MMMM d" ) );
                _blackoutDate = _blackoutDates.Where( b => b.GetCalenderEvent().DTStart.Date == calBaptism.SelectedDate.Date ).FirstOrDefault();
                nbNoBaptisms.Visible = false;
                if ( _blackoutDate != null )
                {
                    nbBlackOutWeek.Title = String.Format( "{0} has been blacked out!</br>", calBaptism.SelectedDate.ToLongDateString() );
                    nbBlackOutWeek.Text = String.Format( "{0}", _blackoutDate.Description );
                    lbEditBlackout.Visible = true;
                    nbBlackOutWeek.Visible = true;
                    //PopulateWithBlackoutMessage( blackoutDate );
                }
                else
                {
                    lbEditBlackout.Visible = false;
                    nbBlackOutWeek.Visible = false;
                    _baptizeeList = new BaptizeeService( new BaptismContext() ).GetBaptizeesByDateRange( dateRange[0], dateRange[1], group.Id );
                    if ( _baptizeeList.Count == 0 )
                    {
                        nbNoBaptisms.Text = "No baptisms scheduled for the selected week!";
                        nbNoBaptisms.Visible = true;
                    }
                    else
                    {
                        nbNoBaptisms.Visible = false;
                        PopulateScheduleList( _baptizeeList );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the first and last day of the week for the date selected
        /// </summary>
        /// <param name="daySelected">The date selected</param>
        /// <returns>dateRange[0] returns the Monday of theat week, and dateRange[1] returns the Sunday, based off of a Monday-first week type</returns>
        protected DateTime[] GetTheDateRange( DateTime daySelected )
        {
            DateTime[] dateRange = new DateTime[2];
            DayOfWeek x1 = DayOfWeek.Monday;
            DayOfWeek x2 = daySelected.DayOfWeek;
            int delta;
            if ( x2 == DayOfWeek.Sunday )
            {
                delta = -6;
            }
            else
            {
                delta = x1 - x2;
            }
            dateRange[0] = daySelected.AddDays( delta );
            dateRange[1] = dateRange[0].AddDays( 6 );
            return dateRange;
        }

        /// <summary>
        /// Builds the baptizee schedule in the schedule panel
        /// </summary>
        /// <param name="baptizeeList"></param>
        protected void PopulateScheduleList( List<Baptizee> baptizeeList )
        {
            DateTime current = DateTime.MinValue;
            foreach ( Baptizee b in baptizeeList )
            {
                if ( current != b.BaptismDateTime )
                {
                    current = b.BaptismDateTime;
                    BuildItemListHeader( current );
                }

                BuildListItem( b );
            }
        }

        /// <summary>
        /// Builds the header for a new baptizee date in the panel
        /// </summary>
        /// <param name="date">The baptism dateTime</param>
        protected void BuildItemListHeader( DateTime date )
        {
            Literal lHeader = new Literal();
            lHeader.Text = string.Format( "<h3>Service: {0} - {1} {2}</h3>",
                date.ToShortTimeString(), date.DayOfWeek, date.ToString( "MM/d" ) );
            phBaptismList.Controls.Add( lHeader );
            phBaptismList.Controls.Add( new LiteralControl( "<div class='row'>" ) );
            phBaptismList.Controls.Add( new LiteralControl( "<div class='col-md-2'><h5>Baptizee</h5></div>" ) );
            phBaptismList.Controls.Add( new LiteralControl( "<div class='col-md-2'><h5>Phone Number</h5></div>" ) );
            phBaptismList.Controls.Add( new LiteralControl( "<div class='col-md-2'><h5>Baptized By</h5></div>" ) );
            phBaptismList.Controls.Add( new LiteralControl( "<div class='col-md-2'><h5>Approved By</h5></div>" ) );
            phBaptismList.Controls.Add( new LiteralControl( "<div class='col-md-2'><h5>Confirmed</h5></div>" ) );
            phBaptismList.Controls.Add( new LiteralControl( "</div>" ) );
        }

        /// <summary>
        /// Builds the header for a new baptizee date in the pdf
        /// </summary>
        /// <param name="date">The baptism dateTime</param>
        /// <param name="document">The document we're writing the header into</param>
        /// <param name="font">The font that the header is in</param>
        protected void BuildPDFItemListHeader( DateTime date, Document document, String font )
        {
            //Fonts
            var listHeaderFont = FontFactory.GetFont( font, 12, Font.BOLD, Color.DARK_GRAY );
            var listSubHeaderFont = FontFactory.GetFont( font, 10, Font.BOLD, Color.DARK_GRAY );

            //Build Header
            document.Add( Chunk.NEWLINE );
            String listHeader = date.ToString( "dddd, MMMM d, yyyy hh:mm" );
            document.Add( new Paragraph( listHeader, listHeaderFont ) );

            //Build Subheaders
            var listSubHeaderTable = new PdfPTable( 5 );
            listSubHeaderTable.LockedWidth = true;
            listSubHeaderTable.TotalWidth = PageSize.A4.Width - document.LeftMargin - document.RightMargin;
            listSubHeaderTable.HorizontalAlignment = 0;
            listSubHeaderTable.SpacingBefore = 10;
            listSubHeaderTable.SpacingAfter = 0;
            listSubHeaderTable.DefaultCell.BorderWidth = 0;
            listSubHeaderTable.DefaultCell.BorderWidthBottom = 1;
            listSubHeaderTable.DefaultCell.BorderColorBottom = Color.DARK_GRAY;

            listSubHeaderTable.AddCell( new Phrase( "Being Baptized", listSubHeaderFont ) );
            listSubHeaderTable.AddCell( new Phrase( "Phone Number", listSubHeaderFont ) );
            listSubHeaderTable.AddCell( new Phrase( "Baptized By", listSubHeaderFont ) );
            listSubHeaderTable.AddCell( new Phrase( "Approved By", listSubHeaderFont ) );
            listSubHeaderTable.AddCell( new Phrase( "Confirmed", listSubHeaderFont ) );

            document.Add( listSubHeaderTable );

            //LineSeparator lineSeparator = new LineSeparator();
            //lineSeparator.LineWidth = 4;
            //document.Add( new Chunk( new LineSeparator() ) );
        }

        /// <summary>
        /// Builds a row for the baptisee and fills it in the panel
        /// </summary>
        /// <param name="baptizee">The baptizee</param>
        protected void BuildListItem( Baptizee baptizee )
        {
            phBaptismList.Controls.Add( new LiteralControl( "<div class='row'>" ) );
            string url = ResolveUrl( string.Format( "~/Person/{0}", baptizee.Person.PersonId ) );
            String theString = String.Format( "<div class='col-md-2'><a href=\"{0}\">{1}</a></div>", url, baptizee.Person.Person.FullName );
            phBaptismList.Controls.Add( new LiteralControl( theString ) );

            theString = String.Format( "<div class='col-md-2'>{0}</div>", baptizee.Person.Person.PhoneNumbers.FirstOrDefault() );
            phBaptismList.Controls.Add( new LiteralControl( theString ) );

            phBaptismList.Controls.Add( new LiteralControl( "<div class='col-md-2'>" ) );
            if ( baptizee.Baptizer1 != null )
            {
                url = ResolveUrl( string.Format( "~/Person/{0}", baptizee.Baptizer1.PersonId ) );
                phBaptismList.Controls.Add( new LiteralControl( string.Format( "<li><a href=\"{0}\">{1}</a></li>", url, baptizee.Baptizer1.Person.FullName ?? "" ) ) );
            }
            else
            {
                phBaptismList.Controls.Add( new LiteralControl( "" ) );
            }
            if ( baptizee.Baptizer2 != null )
            {
                url = ResolveUrl( string.Format( "~/Person/{0}", baptizee.Baptizer2.PersonId ) );
                phBaptismList.Controls.Add( new LiteralControl( string.Format( "<li><a href=\"{0}\">{1}</a></li>", url, baptizee.Baptizer2.Person.FullName ?? "" ) ) );
            }
            else
            {
                phBaptismList.Controls.Add( new LiteralControl( "" ) );
            } phBaptismList.Controls.Add( new LiteralControl( "</div>" ) );

            if ( baptizee.Approver != null )
            {
                url = ResolveUrl( string.Format( "~/Person/{0}", baptizee.Approver.PersonId ) );
                theString = String.Format( "<div class='col-md-2'><a href=\"{0}\">{1}</a></div>", url, baptizee.Approver.Person.FullName ?? "" );
            }
            else
            {
                theString = "<div class='col-md-2'></div>";
            }
            phBaptismList.Controls.Add( new LiteralControl( theString ) );

            Dictionary<string, string> dictionaryInfo = new Dictionary<string, string>();
            dictionaryInfo.Add( "GroupId", PageParameter( "GroupId" ) );
            dictionaryInfo.Add( "SelectedDate", calBaptism.SelectedDate.ToShortDateString() );
            dictionaryInfo.Add( "BaptizeeId", baptizee.Id.ToString() );
            theString = LinkedPageUrl( "AddBaptismPage", dictionaryInfo );

            LinkButton lbEdit = new LinkButton
            {
                Text = "<i class='fa fa-pencil'></i>",
                PostBackUrl = theString
            };
            //  lbEdit.Click += lbEdit_Click;
            phBaptismList.Controls.Add( new LiteralControl( "<div class='col-md-2'>" ) );
            if ( baptizee.IsConfirmed )
            {
                phBaptismList.Controls.Add( new LiteralControl( "<i class='fa fa-check'></i>" ) );

            }
            phBaptismList.Controls.Add( new LiteralControl( "  </div>" ) );
            phBaptismList.Controls.Add( new LiteralControl( "<div class='col-md-2'>" ) );
            phBaptismList.Controls.Add( lbEdit );
            phBaptismList.Controls.Add( new LiteralControl( "</div>" ) );
            phBaptismList.Controls.Add( new LiteralControl( "</div>" ) );

            ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbEdit );
        }

        /// <summary>
        /// Builds a row for the baptisee and fills it in the pdf
        /// </summary>
        /// <param name="baptizee">The baptizee</param>
        /// <param name="document">The document we're writing the row into</param>
        /// <param name="font">The font that the row is in</param>
        protected void BuildPDFListItem( Baptizee baptizee, Document document, String font )
        {
            //Fonts
            var listItemFont = FontFactory.GetFont( font, 8, Font.NORMAL );

            //Build the list item table
            var listItemTable = new PdfPTable( 5 );
            listItemTable.LockedWidth = true;
            listItemTable.TotalWidth = PageSize.A4.Width - document.LeftMargin - document.RightMargin;
            listItemTable.HorizontalAlignment = 0;
            listItemTable.SpacingBefore = 0;
            listItemTable.SpacingAfter = 1;
            listItemTable.DefaultCell.BorderWidth = 0;

            //Add the list items
            listItemTable.AddCell( new Phrase( baptizee.Person.Person.FullName, listItemFont ) );

            String baptizeePhone = "";
            if ( baptizee.Person.Person.PhoneNumbers.FirstOrDefault() != null )
            {
                baptizeePhone = baptizee.Person.Person.PhoneNumbers.FirstOrDefault().ToString();
            }
            listItemTable.AddCell( new Phrase( baptizeePhone, listItemFont ) );

            String baptizerNames = "";
            if ( baptizee.Baptizer1 != null && baptizee.Baptizer2 != null )
            {
                baptizerNames = String.Format( "{0}, {1}", baptizee.Baptizer1.Person.FullName, baptizee.Baptizer2.Person.FullName );
            }
            else if ( baptizee.Baptizer1 != null )
            {
                baptizerNames = String.Format( "{0}", baptizee.Baptizer1.Person.FullName );
            }
            else if ( baptizee.Baptizer2 != null )
            {
                baptizerNames = String.Format( "{0}", baptizee.Baptizer2.Person.FullName );
            }
            listItemTable.AddCell( new Phrase( baptizerNames, listItemFont ) );

            String approverName = "";
            if ( baptizee.Approver != null )
            {
                approverName = baptizee.Approver.Person.FullName;
            }

            listItemTable.AddCell( new Phrase( approverName, listItemFont ) );

            listItemTable.AddCell( new Phrase( baptizee.IsConfirmed.ToYesNo(), listItemFont ) );

            document.Add( listItemTable );
        }

        #endregion
    }
}