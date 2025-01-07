using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Labels;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CheckIn.CheckInKiosk;
using Rock.ViewModels.Blocks.CheckIn.Configuration;
using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Rest.CheckIn;
using Rock.ViewModels.Utility;
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
        DefaultValue = _defaultLoadingTemplate,
        IsRequired = false,
        Key = AttributeKey.LoadingScreenTemplate,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Order = 0 )]

    [CodeEditorField(
        "Completion Screen Template",
        Description = "The template to use for the success screen.",
        DefaultValue = _defaultCompletionTemplate,
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

    [BooleanField(
        "Allow Add Family Member",
        Description = "Whether or not this block should display the option to add a family member.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.AllowAddFamilyMember )]

    [AttributeCategoryField( "Add Person Attributes",
        Description = "The attributes to display when adding a person.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.Person",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.AddPersonAttributes )]

    // Custom settings

    [TextField(
        "Configuration Template",
        Description = "The check-in configuration to use for this block.",
        IsRequired = false,
        Key = AttributeKey.ConfigurationTemplate,
        Category = "CustomSetting" )]

    [TextField(
        "Areas",
        Description = "The areas to use for this block.",
        IsRequired = false,
        Key = AttributeKey.Areas,
        Category = "CustomSetting" )]

    [TextField(
        "Kiosk",
        Description = "The kiosk to use for this block.",
        IsRequired = false,
        Key = AttributeKey.Kiosk,
        Category = "CustomSetting" )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CHECKIN_CHECKIN )]
    [Rock.SystemGuid.BlockTypeGuid( "85A9DDF0-D199-4D7B-887C-9AE8B3508444" )]
    public class CheckIn : RockBlockType, IHasCustomActions
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

        /// <summary>
        /// Gets the check-in configuration to use for the block.
        /// </summary>
        protected string ConfigurationTemplateId => GetAttributeValue( AttributeKey.ConfigurationTemplate );

        /// <summary>
        /// Gets the add person attribute categories.
        /// </summary>
        protected List<Guid> AddPersonAttributeCategories => GetAttributeValues( AttributeKey.AddPersonAttributes ).AsGuidList();

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

            /// <summary>
            /// The attribute key for the check-in configuration attribute.
            /// </summary>
            public const string ConfigurationTemplate = "ConfigurationTemplate";

            /// <summary>
            /// The attribute key for the areas attribute.
            /// </summary>
            public const string Areas = "Areas";

            /// <summary>
            /// The attribute key for the kiosk attribute.
            /// </summary>
            public const string Kiosk = "Kiosk";

            /// <summary>
            /// The attribute key for the allow add family member attribute.
            /// </summary>
            public const string AllowAddFamilyMember = "AllowAddFamilyMember";

            /// <summary>
            /// The attribute key for the add person attributes attribute.
            /// </summary>
            public const string AddPersonAttributes = "AddPersonAttributes";
        }

        #endregion

        #region IRockMobileBlockType

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            using( var rockContext = new RockContext() )
            {
                return new
                {
                    LoadingScreenTemplate = LoadingScreenTemplate,
                    LoginScreenTemplate = LoginScreenTemplate,
                    AreaIds = GetAttributeValue( AttributeKey.Areas )?.Split( ',' ).ToList() ?? new List<string>(),
                    KioskId = GetAttributeValue( AttributeKey.Kiosk ),
                    ConfigurationTemplateId = ConfigurationTemplateId,
                    SuffixValues = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid(), RockContext )
                                    ?.DefinedValues
                                    .OrderBy( dv => dv.Order )
                                    .ToListItemBagList(),
                    AllowAddFamilyMember = GetAttributeValue( AttributeKey.AllowAddFamilyMember ).AsBoolean(),
                    GradeValues = GetGradeValues(),
                    AddPersonAttributes = GetAttributeCategoryAttributes( rockContext, AddPersonAttributeCategories ).Select( a => new
                    {
                        AttributeGuid = a.Guid,
                        Key = a.Key,
                        Title = a.Name,
                        IsRequired = a.IsRequired,
                        ConfigurationValues = a.QualifierValues.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Value ),
                        FieldTypeGuid = a.FieldType.Guid
                    } )
                    .ToList()
                };
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the attributes for the specified attribute categories.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attributeCategoryGuids">The attribute category guids.</param>
        /// <returns>The attributes for the specified attribute categories.</returns>
        private List<AttributeCache> GetAttributeCategoryAttributes( RockContext rockContext, List<Guid> attributeCategoryGuids )
        {
            var attributeService = new AttributeService( rockContext );
            var attributes = new List<AttributeCache>();

            foreach ( var categoryGuid in attributeCategoryGuids )
            {
                var category = CategoryCache.Get( categoryGuid );

                if ( category != null )
                {
                    foreach ( var attribute in attributeService.GetByCategoryId( category.Id, false ) )
                    {
                        if ( !attributes.Any( a => a.Guid == attribute.Guid ) )
                        {
                            attributes.Add( AttributeCache.Get( attribute ) );
                        }
                    }
                }
            }

            return attributes;
        }

        /// <summary>
        /// Gets the XAML template used to display the completion screen.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string GetCompletionScreenTemplate( List<RecordedAttendanceBag> attendances )
        {
            if ( CompletionScreenTemplate.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var mergeFields = RequestContext.GetCommonMergeFields();

            var lavaSafeAttendances = attendances.Select( a => new Lava.LavaDataWrapper( a ) ).ToList();
            mergeFields.Add( "RecordedAttendances", lavaSafeAttendances );

            return CompletionScreenTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant()
                .ToToken();
        }

        /// <summary>
        /// Gets the custom actions that the block supports.
        /// </summary>
        /// <param name="canEdit"></param>
        /// <param name="canAdministrate"></param>
        /// <returns></returns>
        public List<BlockCustomActionBag> GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "fa fa-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/CheckIn/mobileCheckInCustomSettings.obs"
                } );
            }

            return actions;
        }

        /// <summary>
        /// Gets the grades that are available for selection.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetGradeValues()
        {
            var schoolGrades = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid(), RockContext );
            var grades = schoolGrades.DefinedValues
                .OrderBy( a => a.Value.AsInteger() )
                .Select( g => new ListItemBag
                {
                    Value = g.Value,
                    Text = g.Description
                } );

           
            return grades.ToList();
        }

        #endregion

        #region Check-in Flow Block Actions

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
            var configurationGroupType = GroupTypeCache.GetByIdKey( options.ConfigurationTemplateId, RockContext );
            var configuration = configurationGroupType?.GetCheckInConfiguration( RockContext );
            var areas = options.AreaIds.Select( id => GroupTypeCache.GetByIdKey( id, RockContext ) ).ToList();

            if ( configuration == null )
            {
                return ActionBadRequest( "Configuration was not found." );
            }

            try
            {
                var director = new CheckInDirector( RockContext );
                var session = director.CreateSession( configuration );
                var locations = options.LocationIds?.Select( id => NamedLocationCache.GetByIdKey( id ) ).ToList();

                if ( locations != null && locations.Count == 0 )
                {
                    locations = null;
                }

                session.LoadAndPrepareAttendeesForFamily( familyId, areas, kiosk, locations );

                return ActionOk( new MobileFamilyMembersResponseBag
                {
                    FamilyId = familyId,

                    // Include this along with the FamilyId for backwards compatibility.
                    PrimaryFamily = new ListItemBag
                    {
                        Text = RequestContext.CurrentPerson.PrimaryFamily.Name,
                        Value = familyId
                    },
                    PossibleSchedules = session.GetAllPossibleScheduleBags(),
                    People = session.GetAttendeeBags(),
                    CurrentlyCheckedInAttendances = session.GetCurrentAttendanceBags(),
                    ConfigurationTemplate = director.GetConfigurationTemplateBag( configurationGroupType )
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

                var locations = options.LocationIds?.Select( id => NamedLocationCache.GetByIdKey( id ) ).ToList();

                if ( locations != null && locations.Count == 0 )
                {
                    locations = null;
                }
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

        #region Custom Setting Block Actions

        /// <summary>
        /// Gets the custom settings for the block.
        /// </summary>
        /// <param name="kioskId">The kiosk to specify the custom setting options to.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings( string kioskId )
        {
            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var director = new CheckInDirector( RockContext );
            DeviceCache kiosk = null;

            if ( kioskId.IsNotNullOrWhiteSpace() )
            {
                kiosk = DeviceCache.GetByIdKey( kioskId, RockContext );

                if ( kiosk == null )
                {
                    return ActionBadRequest( "Kiosk was not found." );
                }
            }

            try
            {
                return ActionOk( new CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag>
                {
                    Options = new CustomSettingsOptionsBag
                    {
                        CheckInAreas = director.GetCheckInAreaSummaries( kiosk, null ),
                        CheckInConfigurationOptions = director.GetConfigurationTemplateBags(),
                        CampusesAndKiosks = Rock.Blocks.CheckIn.CheckInKioskSetup.GetCampusesAndKiosks( RockContext )
                    },
                    Settings = new CustomSettingsBag
                    {
                        Kiosk = GetAttributeValue( AttributeKey.Kiosk ),
                        CheckInAreas = GetAttributeValue( AttributeKey.Areas )?.Split( ',' ).ToList() ?? new List<string>(),
                        CheckInConfiguration = GetAttributeValue( AttributeKey.ConfigurationTemplate )
                    },
                    SecurityGrantToken = GetSecurityGrantToken()
                } );

            }
            catch ( CheckInMessageException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Saves the custom settings for the block.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag> box )
        {
            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var block = new BlockService( RockContext ).Get( BlockId );
            block.LoadAttributes( RockContext );

            box.IfValidProperty( nameof( box.Settings.CheckInConfiguration ),
                    () => block.SetAttributeValue( AttributeKey.ConfigurationTemplate, box.Settings.CheckInConfiguration ) );

            box.IfValidProperty( nameof( box.Settings.CheckInAreas ),
                    () => block.SetAttributeValue( AttributeKey.Areas, string.Join( ",", box.Settings.CheckInAreas ) ) );

            box.IfValidProperty( nameof( box.Settings.Kiosk ),
                    () => block.SetAttributeValue( AttributeKey.Kiosk, box.Settings.Kiosk ) );

            block.SaveAttributeValues( RockContext );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        #region Registration Block Actions

        /// <summary>
        /// Saves changes made to a family on a kiosk registration screen.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>An instance of <see cref="SaveFamilyResponseBag"/> that describes if the operation was successful or not.</returns>
        [BlockAction]
        public BlockActionResult SaveFamily( SaveFamilyOptionsBag options )
        {
            var template = GroupTypeCache.GetByIdKey( options.TemplateId, RockContext )
                ?.GetCheckInConfiguration( RockContext );
            var kiosk = DeviceCache.GetByIdKey( options.KioskId, RockContext );

            if ( template == null )
            {
                return ActionBadRequest( "Check-in configuration template not found." );
            }

            if ( kiosk == null )
            {
                return ActionBadRequest( "Kiosk not found." );
            }

            if ( !kiosk.GetAttributeValue( "core_device_RegistrationMode" ).AsBoolean() )
            {
                return ActionBadRequest( "This kiosk does not support family registration." );
            }

            var registration = new Rock.CheckIn.v2.FamilyRegistration( RockContext, RequestContext.CurrentPerson, template );
            var existingRegistrants = registration.GetFamilyMemberBags( RequestContext.CurrentPerson.PrimaryFamily, null );
            var newRegistrants = options.People.Where( box => box.Bag.Id.IsNullOrWhiteSpace() );

            existingRegistrants.AddRange( newRegistrants );

            var result = registration.SaveRegistration( options.Family, existingRegistrants, kiosk.GetCampusId(), options.RemovedPersonIds );

            // We have to manually go update our attribute values for the newly
            // created person records. This is a little tricky because we need the person that
            // was just created as well as the options that were provided in the request.
            foreach ( var person in result.NewPersonList )
            {
                var registrationPerson = options.People.Where( rp => rp.Bag.NickName == person.NickName && rp.Bag.LastName == person.LastName ).FirstOrDefault();

                if ( registrationPerson != null )
                {
                    UpdatePersonAttributeValues( person, registrationPerson, GetAttributeCategoryAttributes( RockContext, AddPersonAttributeCategories ).Select( a => a.Guid ).ToList(), false );
                }
            }

            registration.ProcessSaveResult( result );

            var response = new SaveFamilyResponseBag
            {
                FamilyId = result.PrimaryFamily.IdKey,
                IsCheckInAllowed = template.IsCheckInAfterRegistrationAllowed,
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.ErrorMessage
            };

            return ActionOk( response );
        }


        /// <summary>
        /// Updates the person's attribute values based on the registration
        /// details and the check-in template configuration. If
        /// <paramref name="saveEmptyValues"/> is <c>false</c> then any blank
        /// attribute values will be ignored.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> whose attribute values will be updated.</param>
        /// <param name="registrationPerson">The registration details describing the values to be updated.</param>
        /// <param name="saveEmptyValues"><c>true</c> if empty values should be written; otherwise <c>false</c>.</param>
        internal void UpdatePersonAttributeValues( Person person, ValidPropertiesBox<RegistrationPersonBag> registrationPerson, List<Guid> attributeGuids, bool saveEmptyValues )
        {
            if ( !registrationPerson.IsValidProperty( nameof( registrationPerson.Bag.AttributeValues ) ) )
            {
                return;
            }

            if ( registrationPerson.Bag.AttributeValues == null )
            {
                return;
            }        

            var attributes = AttributeCache.GetMany( attributeGuids, RockContext );

            if ( person.Attributes == null )
            {
                person.LoadAttributes( RockContext );
            }

            foreach ( var attributeValue in registrationPerson.Bag.AttributeValues )
            {
                // Make sure we only update attributes that are configured to
                // be available.
                if ( !attributes.Any( a => a.Key == attributeValue.Key ) )
                {
                    continue;
                }

                if ( attributeValue.Value.IsNotNullOrWhiteSpace() || saveEmptyValues )
                {
                    person.SetPublicAttributeValue( attributeValue.Key, attributeValue.Value, RequestContext.CurrentPerson, false );
                }
            }

            person.SaveAttributeValues( RockContext );
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
        /// A helper class that extends the standard family members response bag to include
        /// more information about the family.
        /// </summary>
        public class MobileFamilyMembersResponseBag : FamilyMembersResponseBag
        {
            /// <summary>
            /// Gets or sets the primary family of the person.
            /// </summary>
            public ListItemBag PrimaryFamily { get; set; }

            /// <summary>
            /// Gets or sets the configuration template.
            /// Mobile skips the "GetKioskConfiguration" step, so we
            /// need to include additional info about the configuration template here.
            /// </summary>
            public ConfigurationTemplateBag ConfigurationTemplate { get; set; }
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

        #region Default Templates

        private const string _defaultLoadingTemplate = @"<VerticalStackLayout Spacing=""24""
    VerticalOptions=""Center""
    HorizontalOptions=""Center"">
    
    <ActivityIndicator IsVisible=""true"" IsRunning=""true"" />
        
    <Label Text=""Hang on while we fetch your details!""
        StyleClass=""text-interface-strongest, bold, subheadline"" />
        
</VerticalStackLayout>";

        private const string _defaultCompletionTemplate = @"{% assign hasNewAchievements = false %}

{% for attendance in RecordedAttendances %}
    {% assign newAchievementSize = attendance.JustCompletedAchievements | Size %}
    {% if newAchievementSize > 0 %}
        {% assign hasNewAchievements = true %}
    {% endif %}
{% endfor %}

<Grid>
    <StackLayout Spacing=""24"">
    
        //- Header Row
        <VerticalStackLayout>
            <Label Text=""Check-In Complete""
                StyleClass=""title1, bold, text-interface-strongest"" />
                
            <Label Text=""Below are the details of your check-in""
                StyleClass=""footnote, text-interface-strong"" />
        </VerticalStackLayout>
        
        //- Achievement Bar
        {% if hasNewAchievements %}
            <VerticalStackLayout>
                {% for attendance in RecordedAttendances.JustCompletedAchievements %}
                    <Grid ColumnDefinitions=""Auto, *"">
                        <Label Text=""test"" />
                    </Grid>        
                {% endfor %}
            </VerticalStackLayout>
        {% endif %}
    
        //- Attendance Details
        <VerticalStackLayout Spacing=""24"">
            {% for savedAttendance in RecordedAttendances %}
                <Rock:StyledBorder StyleClass=""p-16, bg-interface-softest, border, border-interface-soft, rounded"">
                    <VerticalStackLayout Spacing=""8"">
    
                        //- Avatar and person name
                        <HorizontalStackLayout Spacing=""16""
                            HorizontalOptions=""Center"">
                            <Rock:Avatar Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ savedAttendance.Attendance.Person.PhotoUrl | Escape }}"" 
                                HeightRequest=""32""
                                WidthRequest=""32"" />
        
                            <Label Text=""{{ savedAttendance.Attendance.Person.FullName | Escape }}""
                                StyleClass=""title3, bold, text-interface-stronger""
                                VerticalOptions=""Center"" />
                        </HorizontalStackLayout>
    
                        //- Checked into group
                        <Grid RowSpacing=""4""
                            RowDefinitions=""Auto, Auto"">
                            <Label Text=""Checked into""
                                StyleClass=""footnote, text-interface-strong"" />
                            
                            <Rock:StyledBorder StyleClass=""bg-primary-strong, px-8, py-4, rounded""
                                Grid.Row=""1"">
                                <Grid ColumnDefinitions=""*, Auto""
                                    VerticalOptions=""Center"">
                                    <Label Text=""{{ savedAttendance.Attendance.Location.Name | Escape }}""
                                        StyleClass=""body, bold, text-primary-soft"" />
                                    
                                    <Label Text=""{{ savedAttendance.Attendance.Schedule.Name | Escape }}""
                                        Grid.Column=""1""
                                        StyleClass=""body, text-primary-soft""/>
                                </Grid>
                            </Rock:StyledBorder>
                        </Grid>
                    </VerticalStackLayout>
                </Rock:StyledBorder>
            {% endfor %} 
        </VerticalStackLayout>
    </StackLayout>
    
    <Rock:ConfettiView IsAnimationEnabled=""True"" InputTransparent=""true"" />
</Grid>";

        #endregion
    }
}
