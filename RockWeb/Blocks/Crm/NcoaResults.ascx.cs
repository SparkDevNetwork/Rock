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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.NCOA;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Ncoa Results" )]
    [Category( "CRM" )]
    [Description( "Display the Ncoa History Record" )]

    [IntegerField(
        "Result Count",
        Key = AttributeKey.ResultCount,
        Description = "Number of result to display per page (default 20).",
        IsRequired = false,
        DefaultIntegerValue = 20,
        Order = 0 )]

    [Rock.SystemGuid.BlockTypeGuid( "3997FE75-E069-4879-B8BA-C8B19C367CD3" )]
    public partial class NcoaResults : RockBlock
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string ResultCount = "ResultCount";
        }
        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfNcoaFilter.ClearFilterClick += gfNcoaFilter_ClearFilterClick;
            gfNcoaFilter.ApplyFilterClick += gfNcoaFilter_ApplyFilterClick;
            gfNcoaFilter.DisplayFilterValue += gfNcoaFilter_DisplayFilterValue;

            this.BlockUpdated += NcoaResults_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upNcoaResults );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindFilter();
                ShowView();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the NcoaResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void NcoaResults_BlockUpdated( object sender, EventArgs e )
        {
            BindFilter();
            ShowView();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfNcoaFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfNcoaFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfNcoaFilter.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfNcoaFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfNcoaFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfNcoaFilter.SetFilterPreference( "Processed", ddlProcessed.SelectedValue.IsNullOrWhiteSpace() ? Processed.All.ConvertToInt().ToString() : ddlProcessed.SelectedValue );
            gfNcoaFilter.SetFilterPreference( "Move Date", sdpMoveDate.DelimitedValues );
            gfNcoaFilter.SetFilterPreference( "NCOA Processed Date", sdpProcessedDate.DelimitedValues );
            gfNcoaFilter.SetFilterPreference( "Move Type", ddlMoveType.SelectedValue );
            gfNcoaFilter.SetFilterPreference( "Address Status", ddlAddressStatus.SelectedValue );
            gfNcoaFilter.SetFilterPreference( "Address Invalid Reason", ddlInvalidReason.SelectedValue );
            gfNcoaFilter.SetFilterPreference( "Last Name", tbLastName.Text );
            gfNcoaFilter.SetFilterPreference( "Move Distance", nbMoveDistance.Text );
            gfNcoaFilter.SetFilterPreference( "Campus", cpCampus.SelectedValue );

            NavigateToCurrentPage();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfNcoaFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs"/> instance containing the event data.</param>
        protected void gfNcoaFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Move Date":
                case "NCOA Processed Date":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }

                case "Processed":
                    {
                        var processed = e.Value.ConvertToEnumOrNull<Processed>();
                        if ( processed.HasValue && processed.Value != Processed.All )
                        {
                            e.Value = processed.ConvertToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case "Move Type":
                    {
                        var moveType = e.Value.ConvertToEnumOrNull<MoveType>();
                        if ( moveType.HasValue )
                        {
                            e.Value = moveType.ConvertToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case "Address Status":
                    {
                        var addressStatus = e.Value.ConvertToEnumOrNull<AddressStatus>();
                        if ( addressStatus.HasValue )
                        {
                            e.Value = addressStatus.ConvertToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case "Invalid Reason":
                    {
                        var invalidReason = e.Value.ConvertToEnumOrNull<AddressInvalidReason>();
                        if ( invalidReason.HasValue )
                        {
                            e.Value = invalidReason.ConvertToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
                case "Campus":
                    {
                        var campus = CampusCache.Get( e.Value.AsInteger() );
                        if ( campus != null )
                        {
                            e.Value = campus.Name;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptNcoaResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptNcoaResults_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var ncoaRow = e.Item.DataItem as NcoaRow;
            var familyMembers = e.Item.FindControl( "lMembers" ) as Literal;

            DescriptionList mmembers = new DescriptionList();
            bool individual = false;
            if ( ncoaRow.Individual != null )
            {
                individual = true;
                mmembers.Add( "Individual", ncoaRow.Individual.FullName );
            }
            if ( ncoaRow.FamilyMembers != null && ncoaRow.FamilyMembers.Count > 0 )
            {
                if ( !individual )
                {
                    mmembers.Add( "Family Members", ncoaRow.FamilyMembers.Select( a => a.FullName ).ToList().AsDelimited( "<Br/>" ) );
                }
                else
                {
                    mmembers.Add( "Other Family Members", ncoaRow.FamilyMembers.Select( a => a.FullName ).ToList().AsDelimited( "<Br/>" ) );

                    var warninglabel = e.Item.FindControl( "lWarning" ) as Literal;
                    warninglabel.Text = "Auto processing this move would result in a split family.";
                }
            }
            familyMembers.Text = mmembers.Html;

        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptNcoaResultsFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptNcoaResultsFamily_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                Repeater childRepeater = ( Repeater ) e.Item.FindControl( "rptNcoaResults" );
                childRepeater.DataSource = ( e.Item.DataItem as IGrouping<string, NcoaRow> );
                childRepeater.DataBind();
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptNcoaResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptNcoaResults_ItemCommand( object sender, RepeaterCommandEventArgs e )
        {
            var ncoaHistoryId = e.CommandArgument.ToStringSafe().AsIntegerOrNull();
            if (!ncoaHistoryId.HasValue)
            {
                return;
            }

            if ( e.CommandName == "MarkAddressAsPrevious" )
            {
                using ( var rockContext = new RockContext() )
                {
                    var ncoaHistory = new NcoaHistoryService( rockContext ).Get( ncoaHistoryId.Value );
                    if ( ncoaHistory != null )
                    {
                        var groupService = new GroupService( rockContext );
                        var groupLocationService = new GroupLocationService( rockContext );

                        var changes = new History.HistoryChangeList();

                        Ncoa ncoa = new Ncoa();
                        var previousValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                        int? previousValueId = previousValue == null ? ( int? ) null : previousValue.Id;
                        var previousGroupLocation = ncoa.MarkAsPreviousLocation( ncoaHistory, groupLocationService, previousValueId, changes );
                        if ( previousGroupLocation != null )
                        {
                            ncoaHistory.Processed = Processed.Complete;

                            // If there were any changes, write to history
                            if ( changes.Any() )
                            {
                                var family = groupService.Get( ncoaHistory.FamilyId );
                                if ( family != null )
                                {
                                    foreach ( var fm in family.Members )
                                    {
                                        HistoryService.SaveChanges(
                                            rockContext,
                                            typeof( Person ),
                                            Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                                            fm.PersonId,
                                            changes,
                                            family.Name,
                                            typeof( Group ),
                                            family.Id,
                                            false );
                                    }
                                }
                            }
                        }
                    }

                    rockContext.SaveChanges();
                }
            }
            else if ( e.CommandName == "MarkProcessed" )
            {
                using ( RockContext rockContext = new RockContext() )
                {
                    var ncoa = ( new NcoaHistoryService( rockContext ) ).Get( ncoaHistoryId.Value );
                    ncoa.Processed = Processed.Complete;
                    rockContext.SaveChanges();
                }
            }

            NcoaResults_BlockUpdated( null, null );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlProcessed.BindToEnum<Processed>( true, new Processed[] { Processed.All } );
            int? processedId = gfNcoaFilter.GetFilterPreference( "Processed" ).AsIntegerOrNull();
            if ( processedId.HasValue )
            {
                if ( processedId.Value != Processed.All.ConvertToInt() )
                {
                    ddlProcessed.SetValue( processedId.Value.ToString() );
                }
            }
            else
            {
                ddlProcessed.SetValue( Processed.ManualUpdateRequiredOrNotProcessed.ConvertToInt().ToString() );
                gfNcoaFilter.SetFilterPreference( "Processed", ddlProcessed.SelectedValue );
            }

            sdpMoveDate.DelimitedValues = gfNcoaFilter.GetFilterPreference( "Move Date" );

            sdpProcessedDate.DelimitedValues = gfNcoaFilter.GetFilterPreference( "NCOA Processed Date" );

            ddlMoveType.BindToEnum<MoveType>( true );
            int? moveTypeId = gfNcoaFilter.GetFilterPreference( "Move Type" ).AsIntegerOrNull();
            if ( moveTypeId.HasValue )
            {
                ddlMoveType.SetValue( moveTypeId.Value.ToString() );
            }

            ddlAddressStatus.BindToEnum<AddressStatus>( true );
            int? addressStatusId = gfNcoaFilter.GetFilterPreference( "Address Status" ).AsIntegerOrNull();
            if ( addressStatusId.HasValue )
            {
                ddlAddressStatus.SetValue( addressStatusId.Value.ToString() );
            }

            ddlInvalidReason.BindToEnum<AddressInvalidReason>( true );
            int? addressInvalidReasonId = gfNcoaFilter.GetFilterPreference( "Address Invalid Reason" ).AsIntegerOrNull();
            if ( addressInvalidReasonId.HasValue )
            {
                ddlInvalidReason.SetValue( addressInvalidReasonId.Value.ToString() );
            }

            string lastNameFilter = gfNcoaFilter.GetFilterPreference( "Last Name" );
            tbLastName.Text = !string.IsNullOrWhiteSpace( lastNameFilter ) ? lastNameFilter : string.Empty;

            string moveDistanceFilter = gfNcoaFilter.GetFilterPreference( "Move Distance" );
            nbMoveDistance.Text = moveDistanceFilter.ToString();

            cpCampus.SetValue( gfNcoaFilter.GetFilterPreference( "Campus" ) );

        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        protected void ShowView()
        {
            var rockContext = new RockContext();

            int resultCount = Int32.Parse( GetAttributeValue( AttributeKey.ResultCount ) );
            int pageNumber = 0;

            if ( !String.IsNullOrEmpty( PageParameter( "Page" ) ) )
            {
                pageNumber = Int32.Parse( PageParameter( "Page" ) );
            }

            var skipCount = pageNumber * resultCount;

            var query = new NcoaHistoryService( rockContext ).Queryable();

            var processed = gfNcoaFilter.GetFilterPreference( "Processed" ).ConvertToEnumOrNull<Processed>();
            if ( processed.HasValue )
            {
                if ( processed.Value != Processed.All && processed.Value != Processed.ManualUpdateRequiredOrNotProcessed )
                {
                    query = query.Where( i => i.Processed == processed );
                }
                else if ( processed.Value == Processed.ManualUpdateRequiredOrNotProcessed )
                {
                    query = query.Where( i => i.Processed == Processed.ManualUpdateRequired || i.Processed == Processed.NotProcessed );
                }

            }

            var moveDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( gfNcoaFilter.GetFilterPreference( "Move Date" ) );
            if ( moveDateRange.Start.HasValue )
            {
                query = query.Where( e => e.MoveDate.HasValue && e.MoveDate.Value >= moveDateRange.Start.Value );
            }
            if ( moveDateRange.End.HasValue )
            {
                query = query.Where( e => e.MoveDate.HasValue && e.MoveDate.Value < moveDateRange.End.Value );
            }

            var ncoaDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( gfNcoaFilter.GetFilterPreference( "NCOA Processed Date" ) );
            if ( ncoaDateRange.Start.HasValue )
            {
                query = query.Where( e => e.NcoaRunDateTime >= ncoaDateRange.Start.Value );
            }
            if ( ncoaDateRange.End.HasValue )
            {
                query = query.Where( e => e.NcoaRunDateTime < ncoaDateRange.End.Value );
            }

            var moveType = gfNcoaFilter.GetFilterPreference( "Move Type" ).ConvertToEnumOrNull<MoveType>();
            if ( moveType.HasValue )
            {
                query = query.Where( i => i.MoveType == moveType );
            }

            var addressStatus = gfNcoaFilter.GetFilterPreference( "Address Status" ).ConvertToEnumOrNull<AddressStatus>();
            if ( addressStatus.HasValue )
            {
                query = query.Where( i => i.AddressStatus == addressStatus );
            }

            var addressInvalidReason = gfNcoaFilter.GetFilterPreference( "Address Invalid Reason" ).ConvertToEnumOrNull<AddressInvalidReason>();
            if ( addressInvalidReason.HasValue )
            {
                query = query.Where( i => i.AddressInvalidReason == addressInvalidReason );
            }

            decimal? moveDistance = gfNcoaFilter.GetFilterPreference( "Move Distance" ).AsDecimalOrNull();
            if ( moveDistance.HasValue )
            {
                query = query.Where( i => i.MoveDistance <= moveDistance.Value );
            }

            string lastName = gfNcoaFilter.GetFilterPreference( "Last Name" );
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                var personAliasQuery = new PersonAliasService( rockContext )
                    .Queryable()
                    .Where( p =>
                        p.Person != null &&
                        p.Person.LastName.Contains( lastName ) )
                    .Select( p => p.Id );
                query = query.Where( i => personAliasQuery.Contains( i.PersonAliasId ) );
            }

            var campusId = gfNcoaFilter.GetFilterPreference( "Campus" ).AsIntegerOrNull();
            if ( campusId.HasValue )
            {
                var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                var personAliasQuery = new PersonAliasService( rockContext ).Queryable().AsNoTracking();
                var campusQuery = new GroupMemberService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.Group.GroupTypeId == familyGroupType.Id &&
                        m.Group.CampusId.HasValue &&
                        m.Group.CampusId.Value == campusId.Value )
                    .Select( m => m.PersonId )
                    .Join( personAliasQuery, m => m, p => p.PersonId, ( m, p ) => p.Id );

                query = query.Where( i => campusQuery.Contains( i.PersonAliasId ) );
            }

            var filteredRecords = query.ToList();
            lTotal.Text = string.Format( "Records: {0}", filteredRecords.Count() );

            #region Grouping rows

            var ncoaRows = filteredRecords
                       .Where( a => a.MoveType != MoveType.Individual )
                       .GroupBy( a => new { a.FamilyId, a.MoveType, a.MoveDate } )
                       .Select( a => new NcoaRow
                       {
                           Id = a.Select( b => b.Id ).Max(),
                           FamilyMemberPersonAliasIds = a.Select( b => b.PersonAliasId ).ToList()
                       } ).ToList();

            var ncoaIndividualRows = filteredRecords
                        .Where( a => a.MoveType == MoveType.Individual )
                       .Select( a => new NcoaRow
                       {
                           Id = a.Id,
                           IndividualPersonAliasId = a.PersonAliasId
                       } ).ToList();

            ncoaRows.AddRange( ncoaIndividualRows );

            #endregion

            var pagedNcoaRows = ncoaRows.OrderBy( a => a.Id ).Skip( skipCount ).Take( resultCount + 1 ).ToList();
            var familyMemberPersonAliasIds = pagedNcoaRows.SelectMany( r => r.FamilyMemberPersonAliasIds ).ToList();
            var individualPersonAliasIds = pagedNcoaRows.Select( r => r.IndividualPersonAliasId ).ToList();

            var people = new PersonAliasService( rockContext )
                .Queryable().AsNoTracking()
                .Where( p =>
                    familyMemberPersonAliasIds.Contains( p.Id ) ||
                    individualPersonAliasIds.Contains( p.Id ) )
                .Select( p => new
                {
                    PersonAliasId = p.Id,
                    Person = p.Person
                } )
                .ToList();

            foreach ( var ncoaRow in pagedNcoaRows )
            {
                ncoaRow.FamilyMembers = people
                    .Where( p => ncoaRow.FamilyMemberPersonAliasIds.Contains( p.PersonAliasId ) )
                    .Select( p => p.Person )
                    .ToList();

                ncoaRow.Individual = people
                    .Where( p => p.PersonAliasId == ncoaRow.IndividualPersonAliasId )
                    .Select( p => p.Person )
                    .FirstOrDefault();

                var ncoaHistoryRecord = filteredRecords.Single( a => a.Id == ncoaRow.Id );

                ncoaRow.OriginalAddress = FormattedAddress( ncoaHistoryRecord.OriginalStreet1, ncoaHistoryRecord.OriginalStreet2,
                                         ncoaHistoryRecord.OriginalCity, ncoaHistoryRecord.OriginalState, ncoaHistoryRecord.OriginalPostalCode )
                                         .ConvertCrLfToHtmlBr();
                ncoaRow.Status = ncoaHistoryRecord.Processed == Processed.Complete ? "Processed" : "Not Processed";
                ncoaRow.StatusCssClass = ncoaHistoryRecord.Processed == Processed.Complete ? "label-success" : "label-default";
                ncoaRow.ShowButton = false;

                var family = new GroupService( rockContext ).Get( ncoaHistoryRecord.FamilyId );
                var person = ncoaRow.Individual ?? ncoaRow.FamilyMembers.First();
                if ( family == null )
                {
                    family = person.GetFamily( rockContext );
                }

                var personService = new PersonService( rockContext );

                ncoaRow.FamilyName = family.Name;
                ncoaRow.HeadOftheHousehold = personService.GetHeadOfHousehold( person, family );

                if ( ncoaHistoryRecord.MoveType != MoveType.Individual )
                {
                    ncoaRow.FamilyMembers = personService.GetFamilyMembers( family, person.Id, true ).Select( a => a.Person ).ToList();
                }
                else
                {
                    ncoaRow.FamilyMembers = personService.GetFamilyMembers( family, person.Id, false ).Select( a => a.Person ).ToList();
                }

                if ( ncoaHistoryRecord.AddressStatus == AddressStatus.Invalid )
                {
                    ncoaRow.TagLine = "Invalid Address";
                    ncoaRow.TagLineCssClass = "label-warning";

                    if ( ncoaHistoryRecord.Processed != Processed.Complete )
                    {
                        ncoaRow.CommandName = "MarkAddressAsPrevious";
                        ncoaRow.CommandText = "Mark Address As Previous";
                        ncoaRow.ShowButton = true;
                    }
                }

                if ( ncoaHistoryRecord.NcoaType == NcoaType.Month48Move )
                {
                    ncoaRow.TagLine = "48 Month Move";
                    ncoaRow.TagLineCssClass = "label-info";

                    if ( ncoaHistoryRecord.Processed != Processed.Complete )
                    {
                        ncoaRow.CommandName = "MarkAddressAsPrevious";
                        ncoaRow.CommandText = "Mark Address As Previous";
                        ncoaRow.ShowButton = true;
                    }
                }

                if ( ncoaHistoryRecord.NcoaType == NcoaType.Move )
                {
                    ncoaRow.TagLine = ncoaHistoryRecord.MoveType.ConvertToString();
                    ncoaRow.TagLineCssClass = "label-success";
                    ncoaRow.MoveDate = ncoaHistoryRecord.MoveDate;
                    ncoaRow.MoveDistance = ncoaHistoryRecord.MoveDistance;
                    ncoaRow.NewAddress = FormattedAddress( ncoaHistoryRecord.UpdatedStreet1, ncoaHistoryRecord.UpdatedStreet2,
                                           ncoaHistoryRecord.UpdatedCity, ncoaHistoryRecord.UpdatedState, ncoaHistoryRecord.UpdatedPostalCode )
                                           .ConvertCrLfToHtmlBr();
                    if ( ncoaHistoryRecord.Processed != Processed.Complete )
                    {
                        ncoaRow.CommandText = "Mark Processed";
                        ncoaRow.CommandName = "MarkProcessed";
                        ncoaRow.ShowButton = true;
                    }
                }

            }

            rptNcoaResultsFamily.DataSource = pagedNcoaRows.Take( resultCount ).GroupBy( n => n.FamilyName );
            rptNcoaResultsFamily.DataBind();

            if ( pagedNcoaRows.Count() > resultCount )
            {
                hlNext.Visible = hlNext.Enabled = true;
                Dictionary<string, string> queryStringNext = new Dictionary<string, string>();
                queryStringNext.Add( "Page", ( pageNumber + 1 ).ToString() );
                var pageReferenceNext = new Rock.Web.PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId, queryStringNext );
                hlNext.NavigateUrl = pageReferenceNext.BuildUrl();
            }
            else
            {
                hlNext.Visible = hlNext.Enabled = false;
            }

            // build prev button
            if ( pageNumber == 0 )
            {
                hlPrev.Visible = hlPrev.Enabled = false;
            }
            else
            {
                hlPrev.Visible = hlPrev.Enabled = true;
                Dictionary<string, string> queryStringPrev = new Dictionary<string, string>();
                queryStringPrev.Add( "Page", ( pageNumber - 1 ).ToString() );
                var pageReferencePrev = new Rock.Web.PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId, queryStringPrev );
                hlPrev.NavigateUrl = pageReferencePrev.BuildUrl();
            }

        }

        /// <summary>
        /// Formats the address.
        /// </summary>
        /// <param name="street1">The street1.</param>
        /// <param name="street2">The street2.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <returns>The formated address</returns>
        private string FormattedAddress( string street1, string street2, string city, string state, string postalCode )
        {
            if ( string.IsNullOrWhiteSpace( street1 ) &&
            string.IsNullOrWhiteSpace( street2 ) &&
            string.IsNullOrWhiteSpace( city ) )
            {
                return string.Empty;
            }

            string result = string.Format( "{0} {1} {2}, {3} {4}",
              street1, street2, city, state, postalCode ).ReplaceWhileExists( "  ", " " );

            // Remove blank lines
            while ( result.Contains( Environment.NewLine + Environment.NewLine ) )
            {
                result = result.Replace( Environment.NewLine + Environment.NewLine, Environment.NewLine );
            }
            while ( result.Contains( "\x0A\x0A" ) )
            {
                result = result.Replace( "\x0A\x0A", "\x0A" );
            }

            if ( string.IsNullOrWhiteSpace( result.Replace( ",", string.Empty ) ) )
            {
                return string.Empty;
            }

            return result;
        }

        #endregion

        #region nested classes

        public class NcoaRow
        {
            public NcoaRow()
            {
                FamilyMemberPersonAliasIds = new List<int>();
                FamilyMembers = new List<Person>();
            }

            public int Id { get; set; }

            public string TagLine { get; set; }

            public string TagLineCssClass { get; set; }

            public DateTime? MoveDate { get; set; }

            public string OriginalAddress { get; set; }

            public string NewAddress { get; set; }

            public decimal? MoveDistance { get; set; }

            public List<int> FamilyMemberPersonAliasIds { get; set; }

            public List<Person> FamilyMembers { get; set; }

            public int IndividualPersonAliasId { get; set; }

            public Person Individual { get; set; }

            public Person HeadOftheHousehold { get; set; }

            public string Status { get; set; }

            public string StatusCssClass { get; set; }

            public string CommandName { get; set; }

            public string CommandText { get; set; }

            public string FamilyName { get; set; }

            public bool ShowButton { get; set; }
        }

        #endregion

    }
}