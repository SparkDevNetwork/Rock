using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Plugins.org_lakepointe.Event
{
    [DisplayName( "Advanced Event Nametags" )]
    [Category( "LPC > Event" )]
    [Description( "Generates a report that will allow users to create nametags and other camp related mail merge and communicaitons" )]
    [ContextAware]

    [TextField( "Participant Campus Key", "Attribute Key of the campus that the particpant is attending with.", false, "Campus", "Configuration" )]
    [TextField( "Volunteer Campus Key", "Attribute Key for the campus that the volunteer is attending with.", false, "Campus", "Configuration" )]
    [BooleanField( "Include Fees", "A boolean flag indicating if fees should be included.", true, "Configuration" )]

    public partial class AdvancedEventNametags : RockBlock
    {
        #region Advanced Event Group Types
        private const string ACTIVITY_GROUPTYPE_GUID = "D5BAEA4B-4768-46DC-8761-E5B1D19DD491";
        private const string COUNSELOR_GROUPTYPE_GUID = "C1854B74-9E83-4D29-8EB1-2627BE98A8A5";
        private const string HOSTHOME_GROUPTYPE_GUID = "F58D293E-AA58-4A1B-86FA-AC15AE0D6F62";
        private const string LODGING_GROUPTYPE_GUID = "FD9E1860-4BC1-4A7D-8B00-6522BE7CA78A";
        private const string TEAM_GROUPTYPE_GUID = "39909EBC-BB51-413F-845C-2FCDFE45564B";
        private const string RECTEAM_GROUPTYPE_GUID = "A4EB1470-E705-4D63-9142-F48EFE3FD609";
        private const string TRANSPORTATION_GROUPTYPE_GUID = "FB69859D-6315-4188-B15A-4E17EF24CB28";
        private const string VOLUNTEER_GROUPTYPE_GUID = "ABC66BCD-2A78-444B-AD20-97351769B74B";

        #endregion

        #region Grid Column Index          
        private const int ACTIVITY1_GRID_INDEX = 11;
        private const int ACTIVITY2_GRID_INDEX = 12;
        private const int ACTIVITY3_GRID_INDEX = 13;
        private const int COUNSELOR_GRID_INDEX = 14;
        private const int FEESPAID_GRID_INDEX = 15;
        private const int LODGING_GRID_INDEX = 8;
        private const int TEAM_GRID_INDEX = 9;
        private const int RECTEAM_GRID_INDEX = 10;
        private const int TRANSPORTATION_GRID_INDEX = 7;
        #endregion

        #region Fields
        RockContext _context = null;

        bool _attributesAreLoaded = false;

        #endregion

        #region Properties

        public bool AttributesAreLoaded
        {
            get
            {
                return _attributesAreLoaded;
            }
            set
            {
                _attributesAreLoaded = value;
            }
        }
        public string CampusKey { get; set; }
        public RegistrationInstance CurrentRegistration { get; set; }



        #endregion

        #region Base Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _context = new RockContext();
            this.BlockUpdated += AdvancedEventNametags_BlockUpdated;
            gAttendeeList.Actions.ShowBulkUpdate = false;
            gAttendeeList.Actions.ShowExcelExport = true;
            gAttendeeList.Actions.ShowMergeTemplate = true;
            gAttendeeList.Actions.ShowMergePerson = false;
            gAttendeeList.DataKeyNames = new string[] { "Id", "RegistrantId" };
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            nbWarningMessage.Visible = false;

            AddRegistrationInstanceScript();
            if ( !IsPostBack )
            {
                lReportHeader.Text = "Event Nametags";
                var contextEntity = this.ContextEntity();
                if ( contextEntity != null && contextEntity is RegistrationInstance )
                {
                    CurrentRegistration = contextEntity as RegistrationInstance;
                    CurrentRegistration.LoadAttributes( _context );
                    riPicker.RegistrationInstanceId = CurrentRegistration.Id;
                    LoadCampusFilter();
                    LoadRegistrationDetail();
                }
                else
                {
                    var selectedAttendees = gfAttendeeFilter.GetUserPreference( "Attendee Type" ).SplitDelimitedValues( false );
                    if ( selectedAttendees.Count() > 0 )
                    {
                        cblAttendees.SetValues( selectedAttendees );
                    }

                    var registrationInstanceId = gfAttendeeFilter.GetUserPreference( "Registration Instance" ).AsIntegerOrNull();

                    if ( registrationInstanceId.HasValue )
                    {
                        CurrentRegistration = new RegistrationInstanceService( _context ).Get( registrationInstanceId.Value );
                        if ( CurrentRegistration != null )
                        {
                            CurrentRegistration.LoadAttributes();
                            riPicker.RegistrationInstanceId = CurrentRegistration.Id;
                            LoadCampusFilter();

                            var selectedCampuses = gfAttendeeFilter.GetUserPreference( "Campuses" ).SplitDelimitedValues( false );
                            cblCampuses.SetValues( selectedCampuses );

                            LoadRegistrationDetail();
                        }

                    }
                    else
                    {
                        gAttendeeList.DataSource = new List<NametagEventParticpant>();
                        gAttendeeList.DataBind();
                    }

                }

            }
        }
        #endregion

        #region Events
        private void AdvancedEventNametags_BlockUpdated( object sender, EventArgs e )
        {
            LoadCurrentRegistrationInstance();
            LoadRegistrationDetail();
        }

        protected void btnRegistrationInstance_Click( object sender, EventArgs e )
        {
            LoadCurrentRegistrationInstance();
            LoadCampusFilter();
            //LoadRegistrationDetail();
        }
        protected void gAttendeeList_GridRebind( object sender, GridRebindEventArgs e )
        {
            LoadCurrentRegistrationInstance();
            LoadRegistrationDetail();
        }
        protected void gfAttendeeFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfAttendeeFilter.SaveUserPreference( "Registration Instance", riPicker.RegistrationInstanceId.ToString() );
            gfAttendeeFilter.SaveUserPreference( "Campuses", cblCampuses.SelectedValues.AsDelimited( "," ) );
            gfAttendeeFilter.SaveUserPreference( "Attendee Type", cblAttendees.SelectedValues.AsDelimited( "," ) );

            LoadCurrentRegistrationInstance();
            LoadRegistrationDetail();
        }

        protected void gfAttendeeFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfAttendeeFilter.DeleteUserPreferences();
            riPicker.RegistrationTemplateId = null;
            riPicker.RegistrationInstanceId = null;
            cblCampuses.Items.Clear();

            foreach ( ListItem li in cblAttendees.Items )
            {
                li.Selected = true;
            }

            gAttendeeList.DataSource = new List<NametagEventParticpant>();
            gAttendeeList.DataBind();
        }

        protected void gfAttendeeFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Registration Instance":
                    var registrationInstance = new RegistrationInstanceService( _context ).Get( e.Value.AsInteger() );
                    if ( registrationInstance != null && registrationInstance.Id > 0 )
                    {
                        e.Value = registrationInstance.Name;
                    }
                    break;
                case "Campus":
                    var campusSB = new System.Text.StringBuilder();
                    foreach ( var c in e.Value.SplitDelimitedValues() )
                    {
                        if ( c.IsNullOrWhiteSpace() )
                        {
                            campusSB.Append( "(blank), " );
                        }
                        else
                        {
                            campusSB.AppendFormat( "{0}, ", c );
                        }
                    }
                    if ( campusSB.Length >= 2 )
                    {
                        e.Value = campusSB.ToString().Substring( 0, campusSB.Length - 2 );
                    }

                    break;
                case "Attendee Type":
                    var selectedValues = e.Value.SplitDelimitedValues();
                    var sb = new System.Text.StringBuilder();
                    foreach ( var sv in selectedValues )
                    {
                        if ( sv.AsIntegerOrNull() == 0 )
                        {
                            sb.Append( "Registrants, " );
                        }
                        else if ( sv.AsIntegerOrNull() == 1 )
                        {
                            sb.Append( "Volunteers, " );
                        }
                    }
                    if ( sb.ToString().Length > 2 )
                    {
                        e.Value = sb.ToString().Substring( 0, sb.ToString().Length - 2 );
                    }
                    break;
            }
        }
        #endregion

        #region Methods

        private Rock.Model.Attribute GetCampusAttribute()
        {
            var campusKey = GetAttributeValue( "ParticipantCampusKey" );

            if ( String.IsNullOrWhiteSpace( campusKey ) )
            {
                return null;
            }

            var campusAttribute = CurrentRegistration.RegistrationTemplate.Forms.SelectMany( f => f.Fields )
                .Where( ff => ff.Attribute != null )
                .Where( ff => ff.Attribute.Key == campusKey )
                .Select( ff => ff.Attribute )
                .FirstOrDefault();

            return campusAttribute;
        }



        private void LoadCurrentRegistrationInstance()
        {
            if ( CurrentRegistration == null && riPicker.RegistrationInstanceId.HasValue )
            {
                CurrentRegistration = new RegistrationInstanceService( _context ).Get( riPicker.RegistrationInstanceId.Value );
                if ( CurrentRegistration != null )
                {
                    CurrentRegistration.LoadAttributes( _context );
                }
            }
        }

        private void LoadCampusFilter()
        {
            cblCampuses.Items.Clear();

            if ( CurrentRegistration == null )
            {
                return;
            }

            var campusAttribute = GetCampusAttribute();

            var campuses = new Dictionary<string, string>();
            var singleSelectField = FieldTypeCache.Get( Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid(), _context );


            if ( campusAttribute != null && campusAttribute.FieldTypeId == singleSelectField.Id )
            {
                var values = campusAttribute.AttributeQualifiers.Where( q => q.Key == "values" ).Select( q => q.Value ).FirstOrDefault();

                var mergeOptions = new Rock.Lava.CommonMergeFieldsOptions();
                mergeOptions.GetLegacyGlobalMergeFields = false;
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, mergeOptions );

                values = values.ResolveMergeFields( mergeFields );

                if ( values.ToUpper().Contains( "SELECT" ) && values.ToUpper().Contains( "FROM" ) )
                {
                    System.Data.DataTable dataTable = Rock.Data.DbService.GetDataTable( values, System.Data.CommandType.Text, null );

                    if ( dataTable != null && dataTable.Columns.Contains( "Value" ) && dataTable.Columns.Contains( "Text" ) )
                    {
                        foreach ( System.Data.DataRow row in dataTable.Rows )
                        {
                            campuses.Add( row["value"].ToString(), row["text"].ToString() );
                        }

                    }
                }
                else
                {
                    foreach ( string keyvalue in values.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                    {
                        var keyValueArray = keyvalue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                        if ( keyValueArray.Length > 0 )
                        {
                            campuses.AddOrIgnore( keyValueArray[0].Trim(), keyValueArray.Length > 1 ? keyValueArray[1].Trim() : keyValueArray[0].Trim() );
                        }
                    }
                }
            }
            else
            {
                campuses = CampusCache.All( false )
                    .OrderBy( c => c.Order )
                    .ThenBy( c => c.Name )
                    .Select( c => new
                    {
                        c.Id,
                        c.Name
                    } )
                    .ToDictionary( c => c.Id.ToString(), c => c.Name );
            }

            ListItem liBlank = new ListItem( "(blank)", "(blank)" );
            liBlank.Selected = true;
            cblCampuses.Items.Add( liBlank );
            foreach ( var campus in campuses.OrderBy( c => c.Key ) )
            {
                ListItem li = new ListItem( campus.Value, campus.Key );
                li.Selected = true;
                cblCampuses.Items.Add( li );
            }

        }

        private void LoadRegistrationDetail()
        {
            if ( CurrentRegistration == null )
            {
                nbWarningMessage.Title = "No Registration Selected";
                nbWarningMessage.Text = "Please select a registration instance to continue.";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Validation;
                nbWarningMessage.Visible = true;
                return;
            }

            lReportHeader.Text = String.Format( "{0} Nametags", CurrentRegistration.Name );

            var registrationRegistrantEntityId = EntityTypeCache.Get( "8A25E5CE-1B4F-4825-BCEA-216167836305".AsGuid(), _context ).Id;
            var personEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.PERSON.AsGuid(), _context ).Id;

            bool includeParticipants = cblAttendees.SelectedValuesAsInt.Contains( 0 );
            bool includeVolunteers = cblAttendees.SelectedValuesAsInt.Contains( 1 );

            var selectedCampuses = cblCampuses.SelectedValues;


            var registrantService = new RegistrationRegistrantService( _context );

            var registrantWithCampus = new List<RegistrantItem>();

            if ( cblAttendees.SelectedValues.Contains( "0" ) )
            {
                var registrantQry = registrantService.Queryable( "PersonAlias.Person,Fees" ).AsNoTracking()
                    .Where( rr => rr.Registration.RegistrationInstanceId == riPicker.RegistrationInstanceId.Value )
                    .Where( rr => rr.OnWaitList == false );

                var registrantCampusKey = GetAttributeValue( "ParticipantCampusKey" );

                var registrantCampusAttributeQry = new AttributeValueService( _context ).Queryable().AsNoTracking()
                    .Where( av => av.Attribute.EntityTypeId == registrationRegistrantEntityId )
                    .Where( av => av.Attribute.Key == registrantCampusKey );
                //.Where( av => av.Attribute.EntityTypeQualifierColumn == "RegistrationTemplateId" );



                registrantWithCampus.AddRange( registrantQry
                .GroupJoin( registrantCampusAttributeQry,
                     r => r.Id,
                        a => a.EntityId ?? 0,
                        ( r, a ) => new { Registrant = r, Campus = a.Select( a1 => a1.Value ).FirstOrDefault(), IsVolunteer = false } )
                .Select( r => new RegistrantItem { Registrant = r.Registrant, Campus = r.Campus, IsVolunteer = r.IsVolunteer } )
                .ToList() );

            }   

            if(cblAttendees.SelectedValues.Contains("1"))
            {
                var volGroupMembers = GetParticipantGroupMembers( VOLUNTEER_GROUPTYPE_GUID.AsGuid() );
                var volunteerCampusKey = GetAttributeValue( "VolunteerCampusKey" );

                if ( volGroupMembers != null )
                {
                    var volCampusAttributeQry = new AttributeValueService( _context ).Queryable().AsNoTracking()
                        .Where( av => av.Attribute.EntityTypeId == registrationRegistrantEntityId )
                        .Where( av => av.Attribute.Key == volunteerCampusKey );


                    registrantWithCampus.AddRange( volGroupMembers
                       .Where( v => !v.IsArchived && v.GroupMemberStatus == GroupMemberStatus.Active )
                       .Join( registrantService.Queryable(), v => v.Id, r => r.GroupMemberId,
                           ( v, r ) => r )
                       .GroupJoin( volCampusAttributeQry,
                           r => r.Id,
                           a => a.EntityId ?? 0,
                           ( r, a ) => new { Registrant = r, Campus = a.Select( a1 => a1.Value ).FirstOrDefault(), IsVolunteer = true } )
                       .Select( r => new RegistrantItem { Registrant = r.Registrant, Campus = r.Campus, IsVolunteer = r.IsVolunteer } )
                       .ToList() );
                }

            }

            var transportationAssignments = GetParticipantGroupMembers( TRANSPORTATION_GROUPTYPE_GUID.AsGuid() );
            var teamAssignments = GetParticipantGroupMembers( TEAM_GROUPTYPE_GUID.AsGuid() );
            var lodgingAssignments = GetParticipantGroupMembers( LODGING_GROUPTYPE_GUID.AsGuid() );
            var activityAssignments = GetParticipantGroupMembers( ACTIVITY_GROUPTYPE_GUID.AsGuid() );
            var recAssignments = GetParticipantGroupMembers( RECTEAM_GROUPTYPE_GUID.AsGuid() );
            var counselorAssignments = GetParticipantGroupMembers( COUNSELOR_GROUPTYPE_GUID.AsGuid() );



            var participantNametags = registrantWithCampus
                .Where( r => cblCampuses.SelectedValues.Contains( r.Campus ) )
                .Select( r => new NametagEventParticpant
                {
                    RegistrantId = r.Registrant.Id,
                    RegistrationId = r.Registrant.RegistrationId,
                    PersonAliasId = r.Registrant.PersonAliasId,
                    Id = r.Registrant.PersonAlias.PersonId,
                    CampusName = r.Campus,
                    LastName = r.Registrant.PersonAlias.Person.LastName,
                    NickName = r.Registrant.PersonAlias.Person.NickName,
                    IsVolunteer = r.IsVolunteer,
                    Transportation = transportationAssignments.Where( t => t.PersonId == r.Registrant.PersonAlias.PersonId ).Select( t => t.Group.Name ).FirstOrDefault(),
                    Lodging = lodgingAssignments.Where( t => t.PersonId == r.Registrant.PersonAlias.PersonId ).Select( t => t.Group.Name ).FirstOrDefault(),
                    Team = teamAssignments.Where( t => t.PersonId == r.Registrant.PersonAlias.PersonId ).Select( t => t.Group.Name ).FirstOrDefault(),
                    RecTeam = recAssignments.Where( t => t.PersonId == r.Registrant.PersonAlias.PersonId ).Select( t => t.Group.Name ).FirstOrDefault(),
                    Activities = activityAssignments.Where( t => t.PersonId == r.Registrant.PersonAlias.PersonId )
                        .Select( a => new NametagGroupMember { GroupMemberId = a.Id, GroupId = a.GroupId, GroupName = a.Group.Name, PersonId = a.PersonId } )
                        .ToList(),
                    Counselor = counselorAssignments.Where( t => t.PersonId == r.Registrant.PersonAlias.PersonId ).Select( t => t.Group.Name ).FirstOrDefault(),
                    Fees = r.Registrant.Fees.Select( f => new NametagFees
                    {
                        RegistrantId = r.Registrant.Id,
                        RegistrantFeeId = f.RegistrationTemplateFeeId,
                        RegistrationTemplateFeeId = f.RegistrationTemplateFeeId,
                        Name = f.RegistrationTemplateFee.Name,
                        Quantity = f.Quantity,
                        Cost = f.Cost,
                        Option = f.Option
                    } ).ToList()
                } )
                .ToList();
            


            SortProperty sortProperty = gAttendeeList.SortProperty;

            if ( sortProperty == null )
            {
                participantNametags = participantNametags.OrderBy( p => p.FullNameReveresed ).ToList();
            }
            else if ( sortProperty.Property == "FullNameReversed" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.FullNameReveresed ).ToList();
                }
            }
            else if ( sortProperty.Property == "CampusName" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.CampusName )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.CampusName )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
            }
            else if ( sortProperty.Property == "Transportation" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.Transportation )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.Transportation )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
            }
            else if ( sortProperty.Property == "Lodging" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.Lodging )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.Lodging )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
            }
            else if ( sortProperty.Property == "Team" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.Team )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.Team )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
            }
            else if ( sortProperty.Property == "Activity1" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.Activity1 )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.Activity1 )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
            }
            else if ( sortProperty.Property == "Activity2" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.Activity2 )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.Activity2 )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
            }
            else if ( sortProperty.Property == "Activity3" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.Activity3 )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.Activity3 )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
            }
            else if ( sortProperty.Property == "Counselor" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.Counselor )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.Counselor )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
            }
            else if ( sortProperty.Property == "FeesPaid" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.FeesPaid )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.FeesPaid )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
            }
            else if ( sortProperty.Property == "IsVolunteer" )
            {
                if ( sortProperty.Direction == SortDirection.Ascending )
                {
                    participantNametags = participantNametags.OrderBy( p => p.IsVolunteer )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
                else
                {
                    participantNametags = participantNametags.OrderByDescending( p => p.IsVolunteer )
                        .ThenBy( p => p.FullNameReveresed ).ToList();
                }
            }
            else
            {
                participantNametags = participantNametags.OrderBy( p => p.FullNameReveresed ).ToList();
            }

            gAttendeeList.DataSource = participantNametags;
            gAttendeeList.DataBind();
        }

        private void AddRegistrationInstanceScript()
        {
            var scriptBuilder = new System.Text.StringBuilder();
            scriptBuilder.AppendLine( "$('[id*=\"ddlRegistrationInstance\"]').change(function() {" );
            scriptBuilder.AppendLine( " var selectedValue = $(this).val();" );
            scriptBuilder.AppendLine( " if( selectedValue != '' && selectedValue != '0' ) {" );
            scriptBuilder.AppendLine( "      $('[id*=\"btnRegistrationInstance\"]').click();" );
            scriptBuilder.AppendLine( " }" );
            scriptBuilder.AppendLine( "});" );

            ScriptManager.RegisterStartupScript( upContent, upContent.GetType(), "RegistrationInstanceUpdate" + RockDateTime.Now.Ticks, scriptBuilder.ToString(), true );
        }

        private List<GroupMember> GetParticipantGroupMembers( Guid groupTypeGuid )
        {
            var groupType = GroupTypeCache.Get( groupTypeGuid, _context );
            var groupEntityTypeId = EntityTypeCache.Get( typeof( Group ) ).Id;
            var registrationInstanceEntityTypeId = EntityTypeCache.Get( typeof( RegistrationInstance ) ).Id;
            var registrationTemplateEntityTypeId = EntityTypeCache.Get( typeof( RegistrationTemplate ) ).Id;



            var placementService = new RegistrationTemplatePlacementService( _context );

            var placements = placementService.Queryable().AsNoTracking()
                .Where( p => p.RegistrationTemplateId == CurrentRegistration.RegistrationTemplateId )
                .Where( p => p.GroupTypeId == groupType.Id )
                .ToList();

            if ( placements.Count == 0 )
            {
                return new List<GroupMember>();
            }

            var groupService = new GroupService( _context );
            var relatedEntityService = new RelatedEntityService( _context );
            var templateGroupMembers = relatedEntityService.Queryable().AsNoTracking()
                .Where( r => r.SourceEntityTypeId == registrationTemplateEntityTypeId )
                .Where( r => r.TargetEntityTypeId == groupEntityTypeId )
                .Where( r => r.PurposeKey == RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate )
                .Where( r => r.SourceEntityId == riPicker.RegistrationTemplateId )
                .Join( groupService.Queryable().AsNoTracking(), r => r.TargetEntityId, g => g.Id,
                    ( r, g ) => g )
                .Where(g => g.GroupTypeId == groupType.Id)
                .SelectMany( g => g.Members );

            var instanceGroupMembers = relatedEntityService.Queryable()
                .Where( r => r.PurposeKey == RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement )
                .Where( r => r.SourceEntityTypeId == registrationInstanceEntityTypeId )
                .Where( r => r.TargetEntityTypeId == groupEntityTypeId )
                .Where( r => r.SourceEntityId == CurrentRegistration.Id )
                .Join( groupService.Queryable().AsNoTracking(), r => r.TargetEntityId, g => g.Id,
                    ( r, g ) => g )
                .Where(g => g.GroupTypeId == groupType.Id)
                .SelectMany( g => g.Members );

            return templateGroupMembers.Union( instanceGroupMembers ).ToList();
            


        }

        #endregion

    }

    public class RegistrantItem
    {
        public RegistrationRegistrant Registrant { get; set; }
        public string Campus { get; set; }
        public bool IsVolunteer { get; set; }
    }

    public class NametagEventParticpant
    {
        public int RegistrantId { get; set; }
        public int RegistrationId { get; set; }
        public int? PersonAliasId { get; set; }
        public int Id { get; set; }
        public string CampusName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string Transportation { get; set; }
        public string Lodging { get; set; }
        public string Team { get; set; }
        public string RecTeam { get; set; }
        public string Counselor { get; set; }
        public List<NametagGroupMember> Activities { get; set; }
        public bool IsVolunteer { get; set; }
        public List<NametagFees> Fees { get; set; }
        public string FeesPaid
        {
            get
            {
                return BuildFeesString( ", " );
            }
        }
        public string FullName
        {
            get
            {
                return string.Concat( NickName, " ", LastName );
            }
        }

        public string FullNameReveresed
        {
            get
            {
                return string.Concat( LastName, " ", NickName );
            }
        }

        public string Activity1
        {
            get
            {
                if ( Activities != null && Activities.Count() >= 1 )
                {
                    return Activities[0].GroupName;
                }
                return null;
            }
        }
        public string Activity2
        {
            get
            {
                if ( Activities != null && Activities.Count() >= 2 )
                {
                    return Activities[1].GroupName;
                }
                return null;
            }
        }

        public string Activity3
        {
            get
            {
                if ( Activities != null && Activities.Count() >= 3 )
                {
                    return Activities[2].GroupName;
                }
                return null;
            }
        }


        private string BuildFeesString( string delimiter )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if ( Fees == null )
            {
                return null;
            }

            foreach ( var fee in Fees )
            {
                sb.AppendFormat( "{0}{1}{2}{3}",
                    fee.Quantity > 1 ? fee.Quantity.ToString() + " " : String.Empty,
                    fee.Name,
                    !String.IsNullOrWhiteSpace( fee.Option ) ? " (" + fee.Option + ")" : String.Empty,
                    delimiter );
            }

            var value = sb.ToString();
            if ( value.Length >= delimiter.Length )
            {
                value = value.Substring( 0, value.Length - delimiter.Length ).Trim();
            }

            return value;
        }


    }

    public class NametagGroupMember
    {
        public int? GroupMemberId { get; set; }
        public int? GroupId { get; set; }
        public string GroupName { get; set; }
        public int? PersonId { get; set; }

    }

    public class NametagFees
    {
        public int RegistrantId { get; set; }
        public int RegistrantFeeId { get; set; }
        public int RegistrationTemplateFeeId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
        public string Option { get; set; }
    }
}