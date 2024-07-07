using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Labels;
using Rock.ViewModels.Rest.CheckIn;
using Rock.Web.Cache;

namespace Rock.Blocks.Mobile.CheckIn
{
    /// <summary>
    /// Allows the user to log in on a mobile application.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Check-in" )]
    [Category( "Mobile > Check-in" )]
    [Description( "Check yourself or family members in/out." )]
    [IconCssClass( "fa fa-clipboard-check" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CHECKIN_CHECKIN )]
    [Rock.SystemGuid.BlockTypeGuid( "85A9DDF0-D199-4D7B-887C-9AE8B3508444" )]
    public class CheckIn : RockBlockType
    {
        #region Block Actions

        /// <summary>
        /// Gets the family members of the current person.
        /// </summary>
        /// <param name="options">The configuration to receive the family members.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetFamilyMembers( MobileFamilyMembersOptionsBag options )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized();
            }

            var familyId = RequestContext.CurrentPerson.PrimaryFamily.IdKey;
            var configuration = GroupTypeCache.GetByIdKey( options.ConfigurationTemplateId, RockContext )?.GetCheckInConfiguration( RockContext );
            var areas = options.AreaIds.Select( id => GroupTypeCache.GetByIdKey( id, RockContext ) ).ToList();

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration was not found." );
            }
            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );

                var locations = options.LocationIds.Select( id => NamedLocationCache.GetByIdKey( id ) ).ToList();
                session.LoadAndPrepareAttendeesForFamily( familyId, areas, null, locations );

                return ActionOk( new FamilyMembersResponseBag
                {
                    FamilyId = familyId,
                    PossibleSchedules = session.GetAllPossibleScheduleBags(),
                    People = session.GetAttendeeBags(),
                    CurrentlyCheckedInAttendances = session.GetCurrentAttendanceBags()
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Gets the opportunities available for a single attendee.
        /// </summary>
        /// <param name="options">The options used to configure the request.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetAttendeeOpportunities( MobileAttendeeOpportunitiesOptionsBag options )
        {
            var configuration = GroupTypeCache.GetByIdKey( options.ConfigurationTemplateId, RockContext )?.GetCheckInConfiguration( RockContext );
            var areas = options.AreaIds.Select( id => GroupTypeCache.GetByIdKey( id, RockContext ) ).ToList();

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration was not found." );
            }

            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );

                var locations = options.LocationIds.Select( id => NamedLocationCache.GetByIdKey( id ) ).ToList();
                session.LoadAndPrepareAttendeesForPerson( options.PersonId, options.FamilyId, areas, null, locations );

                if ( session.Attendees.Count == 0 )
                {
                    return ActionBadRequest( "Individual was not found or is not available for check-in." );
                }

                return ActionOk( new AttendeeOpportunitiesResponseBag
                {
                    Opportunities = session.GetOpportunityCollectionBag( session.Attendees[0].Opportunities )
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Saves an attendance, and optionally confirms it or marks it as pending.
        /// </summary>
        /// <param name="options">The configuration for saving an attendance.</param>
        /// <returns></returns>
        [BlockAction]
        public async Task<BlockActionResult> SaveAttendance( SaveAttendanceOptionsBag options )
        {
            var configuration = GroupTypeCache.GetByIdKey( options.TemplateId, RockContext )?.GetCheckInConfiguration( RockContext );

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration was not found." );
            }

            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );
                var sessionRequest = new AttendanceSessionRequest( options.Session );

                var result = session.SaveAttendance( sessionRequest, options.Requests, null, RequestContext.ClientInformation.IpAddress );

                if ( !options.Session.IsPending )
                {
                    var cts = new CancellationTokenSource( 5000 );
                    await director.LabelProvider.RenderAndPrintCheckInLabelsAsync( result, null, new LabelPrintProvider(), cts.Token );
                }

                return ActionOk( new SaveAttendanceResponseBag
                {
                    Messages = result.Messages,
                    Attendances = result.Attendances
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Confirms all of the attendances for a session (should be previously marked as pending).
        /// </summary>
        /// <param name="options">The configuration for confirming the session.</param>
        /// <returns>A bag containing information about the confirmed attendances.</returns>
        [BlockAction]
        public async Task<BlockActionResult> ConfirmAttendance( ConfirmAttendanceOptionsBag options )
        {
            var configuration = GroupTypeCache.GetByIdKey( options.TemplateId, RockContext )?.GetCheckInConfiguration( RockContext );

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration was not found." );
            }

            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );

                var result = session.ConfirmAttendance( options.SessionGuid );

                var cts = new CancellationTokenSource( 5000 );
                await director.LabelProvider.RenderAndPrintCheckInLabelsAsync( result, null, new LabelPrintProvider(), cts.Token );

                return ActionOk( new ConfirmAttendanceResponseBag
                {
                    Messages = result.Messages,
                    Attendances = result.Attendances
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A helper class for the options used to get attendance information.
        /// We wrap this here because any property we add to the options will not be
        /// supported in the REST call.
        /// </summary>
        public class MobileAttendeeOpportunitiesOptionsBag : AttendeeOpportunitiesOptionsBag
        {
            /// <summary>
            /// Gets or sets the location identifiers that will be used to determine
            /// which options are available for each family member.
            /// </summary>
            public List<string> LocationIds { get; set; }
        }

        /// <summary>
        /// A helper class for the options used to get family members.
        /// We wrap this here because any property we add to the options will not be
        /// supported in the REST call.
        /// </summary>
        public class MobileFamilyMembersOptionsBag : FamilyMembersOptionsBag
        {
            /// <summary>
            /// Gets or sets the location identifiers that will be used to determine
            /// which options are available for each family member.
            /// </summary>
            public List<string> LocationIds { get; set; }
        }

        #endregion
    }
}
