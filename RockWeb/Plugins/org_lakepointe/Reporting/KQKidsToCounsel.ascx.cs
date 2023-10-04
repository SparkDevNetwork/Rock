using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;



namespace RockWeb.Plugins.org_lakepointe.Reporting
{
    [DisplayName( "KQ Kids to Counsel" )]
    [Category( "LPC > Reporting" )]
    [Description( "Filter settings for an SQL report." )]
    
    public partial class KQKidsToCounsel : RockBlock
    {
        private RockContext _context;
        private const int RegistrationEventGroupType = 476;

        #region Properties

        string _registrationInstanceId = string.Empty;
        //string _kidQuestEventGroupType = string.Empty;
        string _colorGroup = string.Empty;
        string _minimumGrade = string.Empty;
        string _maximumGrade = string.Empty;
        string _attendanceStartDateTime = string.Empty;
        string _attendanceEndDateTime = string.Empty;

        #endregion
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _context = new RockContext();

            _registrationInstanceId = PageParameter( "registrationInstanceId" );
            //_kidQuestEventGroupType = PageParameter( "kidQuestEventGroupType" );
            _colorGroup = PageParameter( "colorGroup" );
            _minimumGrade = PageParameter( "minimumGrade" );
            _maximumGrade = PageParameter( "maximumGrade" );
            _attendanceStartDateTime = PageParameter( "attendanceStartDateTime" );
            _attendanceEndDateTime = PageParameter( "attendanceEndDateTime" );
        }


//        SELECT ri.*
//FROM dbo.[RegistrationTemplate] rt
//            JOIN dbo.[RegistrationInstance] ri ON ri.RegistrationTemplateId = rt.Id
//        WHERE rt.IsActive = 1
//            -- AND rt.GroupTypeId IN (340, 476, 477)

//    AND( rt.Name LIKE '%KQ%' OR rt.Name LIKE '%KidQuest%')

//    AND ri.IsActive = 1
//	AND ri.EndDateTime >= GETDATE()

        protected override void OnLoad( EventArgs e )
        {
            base.OnInit( e );

            if ( !Page.IsPostBack )
            {
                BindRegistrationList();
                rddlRegistrationInstance.SelectedValue = _registrationInstanceId;

                BuildColorGroups();
                var ids = _colorGroup.Split( '+' );
                foreach ( var id in ids )
                {
                    if ( id.IsNotNullOrWhiteSpace() )
                    {
                        rcblColorGroup.Items.FindByValue( id ).Selected = true;
                    }
                }

                DateTime result;
                if ( DateTime.TryParse( _attendanceStartDateTime, out result ) )
                {
                    dpAttendanceStartDate.SelectedDate = result;
                }
                if ( DateTime.TryParse( _attendanceEndDateTime, out result ) )
                {
                    dpAttendanceEndDate.SelectedDate = result;
                }

                nbMinimum.Text = _minimumGrade;
                nbMaximum.Text = _maximumGrade;
            }
        }

        #endregion  
        #region Events

        protected void bbExecute_Click( object sender, EventArgs e )
        {
            ReloadPage();
        }

        #endregion
        #region Methods

        private void ReloadPage()
        {
            Dictionary<string, string> qs = new Dictionary<string, string>();

            qs.Add( "registrationInstanceId", rddlRegistrationInstance.SelectedValue );

            var sb = new StringBuilder("+");
            rcblColorGroup.SelectedValues.ForEach( c => sb.AppendFormat( "{0}+", c ) );
            qs.Add( "colorGroup", sb.ToString() );

            qs.Add( "attendanceStartDateTime", dpAttendanceStartDate.SelectedDate.Value.ToShortDateString() );
            qs.Add( "attendanceEndDateTime", dpAttendanceEndDate.SelectedDate.Value.ToShortDateString() );

            qs.Add( "minimumGrade", nbMinimum.Text );
            qs.Add( "maximumGrade", nbMaximum.Text );

            NavigateToCurrentPage( qs );
        }

        private void BindRegistrationList()
        {
            DateTime cutoff = RockDateTime.Today.AddMonths( -1 ); // keep the query working for a month after registration closes

            var registrationList = new RegistrationInstanceService( _context ).Queryable().AsNoTracking()
                .Where( r => ( r.RegistrationTemplate.GroupTypeId == RegistrationEventGroupType ) && r.IsActive && ( r.EndDateTime > cutoff ) )
                .Select( r => new { r.Id, r.Name } )
                .ToList();

            rddlRegistrationInstance.DataSource = registrationList;
            rddlRegistrationInstance.DataBind();
        }

        private void BuildColorGroups()
        {
            // Blue, Green, Purple, Red, Yellow
            rcblColorGroup.DataSource = new List<object>()
            {
                new { Id = 1, Name = "Blue" },
                new { Id = 2, Name = "Green" },
                new { Id = 3, Name = "Purple" },
                new { Id = 4, Name = "Red" },
                new { Id = 5, Name = "Yellow" },
            };

            rcblColorGroup.DataBind();
        }

        #endregion
    }
}