using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Labels;
using Rock.Data;
using Rock.ViewModels.CheckIn;
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

    [CodeEditorField(
        "Loading Screen Template",
        Description = "The template to use for the loading screen. Lava is disabled.",
        DefaultValue = "",
        IsRequired = false,
        Key = AttributeKey.LoadingScreenTemplate,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Order = 0 )]

    [CodeEditorField(
        "Completion Screen Template",
        Description = "The template to use for the success screen.",
        DefaultValue = "",
        IsRequired = false,
        Key = AttributeKey.CompletionScreenTemplate,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Order = 1 )]

    [CodeEditorField(
        "Login Screen Template",
        Description = "The template to use for the screen that displays if a person is not logged in. Lava is disabled.",
        DefaultValue = "",
        IsRequired = false,
        Key = AttributeKey.LoginScreenTemplate,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CHECKIN_CHECKIN )]
    [Rock.SystemGuid.BlockTypeGuid( "85A9DDF0-D199-4D7B-887C-9AE8B3508444" )]
    public class CheckIn : RockBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the defined loading screen template for the block.
        /// </summary>
        protected string LoadingScreenTemplate => GetAttributeValue( AttributeKey.LoadingScreenTemplate );

        /// <summary>
        /// Gets the defined completion screen template for the block.
        /// </summary>
        protected string CompletionScreenTemplate => GetAttributeValue( AttributeKey.CompletionScreenTemplate );

        /// <summary>
        /// Gets the defined login screen template for the block.
        /// </summary>
        protected string LoginScreenTemplate => GetAttributeValue( AttributeKey.LoginScreenTemplate );

        #endregion

        #region Keys

        /// <summary>
        /// Keys to use for attribute storage.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The attribute key for the loading screen template attribute.
            /// </summary>
            public const string LoadingScreenTemplate = "LoadingScreenTemplate";

            /// <summary>
            /// The attribute key for the completion screen template attribute.
            /// </summary>
            public const string CompletionScreenTemplate = "CompletionScreenTemplate";

            /// <summary>
            /// The attribute key for the login screen template attribute.
            /// </summary>
            public const string LoginScreenTemplate = "LoginScreenTemplate";
        }

        #endregion

        #region IRockMobileBlockType

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                LoadingScreenTemplate = LoadingScreenTemplate,
                LoginScreenTemplate = LoginScreenTemplate
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the XAML template used to display the completion screen.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string GetCompletionScreenTemplate( List<RecordedAttendanceBag> attendances )
        {
            if( CompletionScreenTemplate.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var mergeFields = RequestContext.GetCommonMergeFields();

            var lavaSafeAttendances = attendances.Select( a => new Lava.LavaDataWrapper( a ) ).ToList();
            mergeFields.Add( "RecordedAttendances", lavaSafeAttendances );
             
            return CompletionScreenTemplate.ResolveMergeFields( mergeFields );
        }

        #endregion

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

            DeviceCache kiosk = null;
            if ( options.KioskId.IsNotNullOrWhiteSpace() )
            {
                kiosk = DeviceCache.GetByIdKey( options.KioskId, RockContext );
            }

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
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
                session.LoadAndPrepareAttendeesForFamily( familyId, areas, kiosk, locations );

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

            DeviceCache kiosk = null;
            if ( options.KioskId.IsNotNullOrWhiteSpace() )
            {
                kiosk = DeviceCache.GetByIdKey( options.KioskId, RockContext );
            }

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
            }

            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );

                var locations = options.LocationIds.Select( id => NamedLocationCache.GetByIdKey( id ) ).ToList();
                session.LoadAndPrepareAttendeesForPerson( options.PersonId, options.FamilyId, areas, kiosk, locations );

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

            DeviceCache kiosk = null;

            if ( options.KioskId.IsNotNullOrWhiteSpace() )
            {
                kiosk = DeviceCache.GetByIdKey( options.KioskId, RockContext );
            }

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
            }

            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );
                var sessionRequest = new AttendanceSessionRequest( options.Session );

                var result = session.SaveAttendance( sessionRequest, options.Requests, kiosk, RequestContext.ClientInformation.IpAddress );

                if ( !options.Session.IsPending )
                {
                    var cts = new CancellationTokenSource( 5000 );
                    await director.LabelProvider.RenderAndPrintCheckInLabelsAsync( result, null, new LabelPrintProvider(), cts.Token );
                }

                return ActionOk( new MobileCheckInResultBag
                {
                    Messages = result.Messages,
                    Attendances = result.Attendances,
                    Template = GetCompletionScreenTemplate( result.Attendances )
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

            DeviceCache kiosk = null;

            if ( options.KioskId.IsNotNullOrWhiteSpace() )
            {
                kiosk = DeviceCache.GetByIdKey( options.KioskId, RockContext );
            }

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
            }

            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );

                var result = session.ConfirmAttendance( options.SessionGuid );

                var cts = new CancellationTokenSource( 5000 );
                await director.LabelProvider.RenderAndPrintCheckInLabelsAsync( result, kiosk, new LabelPrintProvider(), cts.Token );

                return ActionOk( new MobileCheckInResultBag
                {
                    Messages = result.Messages,
                    Attendances = result.Attendances,
                    Template = GetCompletionScreenTemplate( result.Attendances )
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Checks out the specified attendances.
        /// </summary>
        /// <param name="options">The configuration for checkout.</param>
        /// <returns>A bag containing information about the checkout.</returns>
        [BlockAction]
        public async Task<BlockActionResult> Checkout( CheckoutOptionsBag options )
        {
            var configuration = GroupTypeCache.GetByIdKey( options.TemplateId, RockContext )?.GetCheckInConfiguration( RockContext );
            DeviceCache kiosk = null;

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration was not found." );
            }

            if ( options.KioskId.IsNotNullOrWhiteSpace() )
            {
                kiosk = DeviceCache.GetByIdKey( options.KioskId, RockContext );
            }

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk was not found." );
            }

            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );
                var sessionRequest = new AttendanceSessionRequest( options.Session );

                var result = session.Checkout( sessionRequest, options.AttendanceIds, kiosk );

                var cts = new CancellationTokenSource( 5000 );
                await director.LabelProvider.RenderAndPrintCheckoutLabelsAsync( result, kiosk, new LabelPrintProvider(), cts.Token );

                return ActionOk( result );
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

        /// <summary>
        /// A helper class that extends the standard check-in result bag to include
        /// a parsed XAML template.
        /// </summary>
        public class MobileCheckInResultBag : CheckInResultBag
        {
            /// <summary>
            /// The XAML to display.
            /// </summary>
            public string Template { get; set; }
        }

        #endregion
    }
}
