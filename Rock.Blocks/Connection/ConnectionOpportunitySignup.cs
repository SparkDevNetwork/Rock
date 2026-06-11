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

using Rock.Attribute;
using Rock.Crm.RecordSource;
using Rock.Enums.Blocks.Connection.ConnectionOpportunitySignup;
using Rock.Model;
using Rock.ViewModels.Blocks.Connection.ConnectionOpportunitySignup;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Connection
{
    [DisplayName( "Connection Opportunity Signup" )]
    [Category( "Connection" )]
    [Description( "Block used to sign up for a connection opportunity." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes
    [BooleanField( "Display Home Phone",
        Description = "Whether to display the home phone field.",
        DefaultBooleanValue = true,
        Order = 0,
        Key = AttributeKey.DisplayHomePhone )]

    [BooleanField( "Display Mobile Phone",
        Description = "Whether to display the mobile phone field.",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.DisplayMobilePhone )]

    [CodeEditorField( "Lava Template",
        Description = "Lava template to use to display the response message.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/OpportunityResponseMessage.lava' %}",
        Order = 2,
        Key = AttributeKey.LavaTemplate )]

    [BooleanField( "Enable Campus Context",
        Description = "If the page has a campus context its value will be used as a filter.",
        DefaultBooleanValue = true,
        Order = 3,
        Key = AttributeKey.EnableCampusContext )]

    [DefinedValueField( "Connection Status",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to use for new individuals (default: 'Web Prospect').",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Order = 4,
        Key = AttributeKey.ConnectionStatus )]

    [DefinedValueField( "Record Status",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        Description = "The record status to use for new individuals (default: 'Pending').",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = "283999EC-7346-42E3-B807-BCE9B2BABB49",
        Order = 5,
        Key = AttributeKey.RecordStatus )]

    [DefinedValueField( "Record Source",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.RECORD_SOURCE_TYPE,
        Description = "The record source to use for new individuals (default = 'Serving Connection'). If a 'RecordSource' page parameter is found, it will be used instead.",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_SERVING_CONNECTION,
        Order = 6,
        Key = AttributeKey.RecordSource )]

    [ConnectionOpportunityField( Name = "Connection Opportunity",
        Description = "If a Connection Opportunity is set, only details for it will be displayed (regardless of the querystring parameters).",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 7,
        Key = AttributeKey.ConnectionOpportunity )]

    [AttributeCategoryField(
        "Include Attribute Categories",
        Description = "Attributes in these Categories will be displayed.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.ConnectionRequest",
        IsRequired = false,
        Order = 8,
        Key = AttributeKey.IncludeAttributeCategories )]

    [AttributeCategoryField(
        "Exclude Attribute Categories",
        Description = "Attributes in these Categories will not be displayed.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.ConnectionRequest",
        IsRequired = false,
        Order = 9,
        Key = AttributeKey.ExcludeAttributeCategories )]

    [BooleanField( "Exclude Non-Public Connection Request Attributes",
        Description = "Attributes without 'Public' checked will not be displayed.",
        DefaultBooleanValue = true,
        Order = 10,
        Key = AttributeKey.ExcludeNonPublicAttributes )]

    [TextField( "Comment Field Label",
        Description = "The label to apply to the comment field.",
        DefaultValue = "Comments",
        IsRequired = false,
        Order = 11,
        Key = AttributeKey.CommentFieldLabel )]

    [BooleanField(
        "Disable Captcha Support",
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        DefaultBooleanValue = false,
        Order = 12,
        Key = AttributeKey.DisableCaptchaSupport )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "A10BF374-F97E-49FA-955C-3B22A9F31787" )]
    [Rock.SystemGuid.BlockTypeGuid( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F" )]
    [ContextAware( typeof( Campus ) )]
    public class ConnectionOpportunitySignup : RockBlockType
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DisplayHomePhone = "DisplayHomePhone";
            public const string DisplayMobilePhone = "DisplayMobilePhone";
            public const string LavaTemplate = "LavaTemplate";
            public const string EnableCampusContext = "EnableCampusContext";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string RecordSource = "RecordSource";
            public const string ConnectionOpportunity = "ConnectionOpportunity";
            public const string IncludeAttributeCategories = "IncludeAttributeCategories";
            public const string ExcludeAttributeCategories = "ExcludeAttributeCategories";
            public const string ExcludeNonPublicAttributes = "ExcludeNonPublicAttributes";
            public const string CommentFieldLabel = "CommentFieldLabel";
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";
        }

        #endregion Attribute Keys

        #region Keys

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
            public const string PersonGuid = "PersonGuid";
            public const string OpportunityId = "OpportunityId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ConnectionOpportunitySignupInitializationBox();

            box.DisplayHomePhone = GetAttributeValue( AttributeKey.DisplayHomePhone ).AsBoolean();
            box.DisplayMobilePhone = GetAttributeValue( AttributeKey.DisplayMobilePhone ).AsBoolean();

            box.DisableCaptchaSupport = Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() );

            var opportunity = GetConnectionOpportunity();

            if ( opportunity == null || !opportunity.IsActive || opportunity.ConnectionType == null || !opportunity.ConnectionType.IsActive )
            {
                box.ErrorMessage = "The requested opportunity is not available.";
                return box;
            }

            Person person = GetPerson();

            if ( person != null )
            {
                box.FirstName = person.FirstName;
                box.LastName = person.LastName;
                box.Email = person.Email;

                var homePhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                var mobilePhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );

                if ( homePhone != null )
                {
                    var homePhoneNumber = person.PhoneNumbers.Where( p => p.NumberTypeValueId == homePhone.Id ).FirstOrDefault();

                    if ( homePhoneNumber != null )
                    {
                        box.HomePhone = new PhoneNumberBoxWithSmsControlBag
                        {
                            Number = homePhoneNumber.NumberFormatted,
                            CountryCode = homePhoneNumber.CountryCode,
                            IsMessagingEnabled = false
                        };
                    }
                }

                if ( mobilePhone != null )
                {
                    var mobilePhoneNumber = person.PhoneNumbers.Where( p => p.NumberTypeValueId == mobilePhone.Id ).FirstOrDefault();

                    if ( mobilePhoneNumber != null )
                    {
                        box.MobilePhone = new PhoneNumberBoxWithSmsControlBag
                        {
                            Number = mobilePhoneNumber.NumberFormatted,
                            CountryCode = mobilePhoneNumber.CountryCode,
                            IsMessagingEnabled = false
                        };
                    }
                }
            }

            var campuses = CampusCache.All()
                .Where( c => c.IsActive ?? false )
                .Where( c => opportunity.ConnectionOpportunityCampuses.Any( oc => oc.CampusId == c.Id ) )
                .ToList();

            box.Campuses = campuses
                .Select( c => new ListItemBag { Value = c.Id.ToString(), Text = c.Name } )
                .OrderBy( l => l.Text )
                .ToList();

            int? selectedCampusId = null;

            // Set selectedCampus to use the campus from context if available and valid
            if ( GetAttributeValue( AttributeKey.EnableCampusContext ).AsBoolean() )
            {
                var campusContext = RequestContext.GetContextEntity<Campus>();
                if ( campusContext != null && campuses.Any( c => c.Id == campusContext.Id ) )
                {
                    selectedCampusId = campusContext.Id;
                }
            }

            // Otherwise, try to use the person's campus
            if ( !selectedCampusId.HasValue )
            {
                int? personCampusId = null;
                if ( person != null )
                {
                    personCampusId = person.PrimaryCampusId;
                    if ( !personCampusId.HasValue )
                    {
                        personCampusId = person.GetCampus()?.Id;
                    }
                }
                if ( personCampusId.HasValue && campuses.Any( c => c.Id == personCampusId.Value ) )
                {
                    selectedCampusId = personCampusId.Value;
                }
            }

            // Fallback
            if ( !selectedCampusId.HasValue && campuses.Any() )
            {
                selectedCampusId = campuses.First().Id;
            }

            box.SelectedCampusId = selectedCampusId;

            var connectionRequest = new ConnectionRequest();

            // Both of these properties are needed in order for the new v17 attribute "inheritance" to work:
            connectionRequest.ConnectionOpportunityId = opportunity.Id;
            connectionRequest.ConnectionTypeId = opportunity.ConnectionTypeId;

            var categoryService = new CategoryService( this.RockContext );
            var categoryNames = categoryService.Queryable().ToDictionary( c => c.Guid, c => c.Name );

            var includedCategoryGuidList = GetAttributeValue( AttributeKey.IncludeAttributeCategories ).SplitDelimitedValues().AsGuidList();
            var includedCategoryNameSet = new HashSet<string>( includedCategoryGuidList.Where( g => categoryNames.TryGetValue( g, out var name ) ).Select( g => categoryNames[g] ) );

            var excludedCategoryGuidList = GetAttributeValue( AttributeKey.ExcludeAttributeCategories ).SplitDelimitedValues().AsGuidList();
            var excludedCategoryNameSet = new HashSet<string>( excludedCategoryGuidList.Where( g => categoryNames.TryGetValue( g, out var name ) ).Select( g => categoryNames[g] ) );

            // Load attributes and apply filters
            connectionRequest.LoadAttributes();

            var attributes = connectionRequest.Attributes?.Values?.ToList() ?? new List<AttributeCache>();

            // Apply include category filter
            if ( includedCategoryNameSet != null && includedCategoryNameSet.Any() )
            {
                attributes = attributes.Where( a => a.Categories.Any( c => includedCategoryNameSet.Contains( c.Name ) ) ).ToList();
            }

            // Apply exclude category filter
            if ( excludedCategoryNameSet != null && excludedCategoryNameSet.Any() )
            {
                attributes = attributes.Where( a => !a.Categories.Any( c => excludedCategoryNameSet.Contains( c.Name ) ) ).ToList();
            }

            // Apply ExcludeNonPublic filter
            if ( GetAttributeValue( AttributeKey.ExcludeNonPublicAttributes ).AsBoolean() )
            {
                attributes = attributes.Where( a => a.IsPublic ).ToList();
            }

            // Convert to PublicAttributeBag format
            box.Attributes = attributes
                .ToDictionary( a => a.Key, a => PublicAttributeHelper.GetPublicAttributeForEdit( a ) );

            box.CommentFieldLabel = GetAttributeValue( AttributeKey.CommentFieldLabel );

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <returns>The matching Person or null.</returns>
        private Person GetPerson()
        {
            var personGuid = PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();
            if ( personGuid.HasValue )
            {
                return new PersonService( this.RockContext ).Get( personGuid.Value );
            }

            var personId = PageParameter( PageParameterKey.PersonId );
            if ( !personId.IsNullOrWhiteSpace() )
            {
                return new PersonService( this.RockContext ).Get( personId, !PageCache.Layout.Site.DisablePredictableIds );
            }

            return GetCurrentPerson();
        }

        /// <summary>
        /// Gets the connection opportunity based on block attribute or page parameter.
        /// </summary>
        /// <returns>The matching ConnectionOpportunity or null.</returns>
        private ConnectionOpportunity GetConnectionOpportunity()
        {
            var connectionOpportunityGuid = GetAttributeValue( AttributeKey.ConnectionOpportunity ).AsGuidOrNull();
            int? opportunityId = null;

            if ( !connectionOpportunityGuid.HasValue )
            {
                opportunityId = PageParameter( PageParameterKey.OpportunityId ).AsIntegerOrNull();
                if ( !opportunityId.HasValue || opportunityId.Value == 0 )
                {
                    return null;
                }
            }

            var qry = new ConnectionOpportunityService( this.RockContext )
                .Queryable()
                .Include( o => o.ConnectionType );

            if ( connectionOpportunityGuid.HasValue )
            {
                return qry.FirstOrDefault( o => o.Guid == connectionOpportunityGuid.Value );
            }
            else
            {
                return qry.FirstOrDefault( o => o.Id == opportunityId.Value );
            }
        }

        /// <summary>
        /// Saves or updates a phone number for the given person and phone type.
        /// </summary>
        /// <param name="number">The phone number.</param>
        /// <param name="countryCode">The country code.</param>
        /// <param name="person">The person to update.</param>
        /// <param name="phoneTypeGuid">The phone type GUID.</param>
        private void SavePhone( string number, string countryCode, Person person, Guid phoneTypeGuid )
        {
            var numberType = DefinedValueCache.Get( phoneTypeGuid );
            if ( numberType != null )
            {
                var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberType.Id );
                string newPhoneNumber = PhoneNumber.CleanNumber( number );

                if ( !string.IsNullOrWhiteSpace( newPhoneNumber ) )
                {
                    if ( phone == null )
                    {
                        phone = new PhoneNumber();
                        person.PhoneNumbers.Add( phone );
                        phone.NumberTypeValueId = numberType.Id;
                    }
                    phone.CountryCode = PhoneNumber.CleanNumber( countryCode );
                    phone.Number = newPhoneNumber;
                }
            }
        }


        /// <summary>
        /// Gets the record source to use for new individuals.
        /// </summary>
        /// <returns>
        /// The identifier of the Record Source Type <see cref="DefinedValue"/> to use.
        /// </returns>
        private int? GetRecordSourceValueId()
        {
            return RecordSourceHelper.GetSessionRecordSourceValueId()
                ?? DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordSource ).AsGuid() )?.Id;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Handles the signup action for a connection opportunity.
        /// </summary>
        /// <param name="bag">The signup request bag.</param>
        /// <returns>The result of the signup action.</returns>
        [BlockAction]
        public BlockActionResult Signup( ConnectionOpportunitySignupRequestBag bag )
        {
            var resultBag = new ConnectionOpportunitySignupResultBag();
            try
            {
                bool disableCaptcha = Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() );

                if ( !disableCaptcha && !RequestContext.IsCaptchaValid )
                {
                    resultBag.ResultType = ConnectionOpportunitySignupResultType.CaptchaInvalid;
                    resultBag.ResponseMessage = "Captcha was not valid.";
                    return ActionBadRequest( resultBag.ResponseMessage );
                }

                if ( bag == null )
                {
                    resultBag.ResultType = ConnectionOpportunitySignupResultType.InvalidRequest;
                    resultBag.ResponseMessage = "Invalid request.";
                    return ActionBadRequest( resultBag.ResponseMessage );
                }

                var opportunity = GetConnectionOpportunity();

                if ( opportunity == null )
                {
                    resultBag.ResultType = ConnectionOpportunitySignupResultType.OpportunityNotFound;
                    resultBag.ResponseMessage = "The opportunity you are trying to sign up for does not exist, or is no longer available.";
                    return ActionNotFound( resultBag.ResponseMessage );
                }

                var defaultStatusId = opportunity.ConnectionType.ConnectionStatuses
                    .Where( s => s.IsDefault )
                    .Select( s => s.Id )
                    .FirstOrDefault();

                if ( defaultStatusId == 0 )
                {
                    resultBag.ResultType = ConnectionOpportunitySignupResultType.InvalidRequest;
                    resultBag.ResponseMessage = "Sorry, we are unable to process your signup at this time.";
                    return ActionBadRequest( resultBag.ResponseMessage );
                }

                int? campusId = bag.CampusId;

                var person = GetPerson();
                var currentPerson = GetCurrentPerson();

                if ( person == null ||
                    !bag.LastName.Equals( person.LastName, StringComparison.OrdinalIgnoreCase ) ||
                    !( bag.FirstName.Equals( person.NickName, StringComparison.OrdinalIgnoreCase ) || bag.FirstName.Equals( person.FirstName, StringComparison.OrdinalIgnoreCase ) ) ||
                    !bag.Email.Equals( person.Email, StringComparison.OrdinalIgnoreCase ) )
                {
                    var personQuery = new PersonService.PersonMatchQuery( bag.FirstName, bag.LastName, bag.Email, bag.MobilePhone?.Number );
                    person = new PersonService( this.RockContext ).FindPerson( personQuery, true );
                }

                if ( person == null || !person.PrimaryAliasId.HasValue )
                {
                    var connectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
                    var recordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

                    person = new Person
                    {
                        FirstName = bag.FirstName,
                        LastName = bag.LastName,
                        Email = bag.Email,
                        IsEmailActive = true,
                        EmailPreference = EmailPreference.EmailAllowed,
                        RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ).Id,
                        ConnectionStatusValueId = connectionStatus?.Id,
                        RecordStatusValueId = recordStatus?.Id,
                        RecordSourceValueId = GetRecordSourceValueId()
                    };

                    PersonService.SaveNewPerson( person, this.RockContext, campusId, false );
                }

                if ( bag.HomePhone != null && !string.IsNullOrWhiteSpace( bag.HomePhone.Number ) )
                {
                    SavePhone( bag.HomePhone.Number, bag.HomePhone.CountryCode, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
                }

                if ( bag.MobilePhone != null && !string.IsNullOrWhiteSpace( bag.MobilePhone.Number ) )
                {
                    SavePhone( bag.MobilePhone.Number, bag.MobilePhone.CountryCode, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                }

                // Create the connection request
                var connectionRequest = new ConnectionRequest
                {
                    PersonAliasId = person.PrimaryAliasId.Value,
                    Comments = bag.Comments ?? string.Empty,
                    ConnectionOpportunityId = opportunity.Id,
                    ConnectionTypeId = opportunity.ConnectionTypeId,
                    ConnectionState = ConnectionState.Active,
                    ConnectionStatusId = defaultStatusId,
                    CampusId = campusId,
                    ConnectorPersonAliasId = opportunity.GetDefaultConnectorPersonAliasId( campusId )
                };

                connectionRequest.LoadAttributes( this.RockContext );

                if ( bag.AttributeValues != null )
                {
                    connectionRequest.SetPublicAttributeValues(
                        bag.AttributeValues,
                        currentPerson,
                        enforceSecurity: false
                    );
                }

                if ( !connectionRequest.IsValid )
                {
                    resultBag.ResultType = ConnectionOpportunitySignupResultType.InvalidConnectionRequest;
                    resultBag.ResponseMessage = "Invalid connection request.";
                    return ActionBadRequest( resultBag.ResponseMessage );
                }

                var connectionRequestService = new ConnectionRequestService( this.RockContext );

                this.RockContext.WrapTransaction( () =>
                {
                    connectionRequestService.Add( connectionRequest );
                    this.RockContext.SaveChanges();
                    connectionRequest.SaveAttributeValues( this.RockContext );
                } );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson, new Rock.Lava.CommonMergeFieldsOptions() );
                mergeFields.Add( "Opportunity", opportunity );
                mergeFields.Add( "Person", person );

                var responseMessage = GetAttributeValue( AttributeKey.LavaTemplate ).ResolveMergeFields( mergeFields, currentPerson );

                resultBag.ResultType = ConnectionOpportunitySignupResultType.Success;
                resultBag.ResponseMessage = responseMessage;
                return ActionOk( resultBag );
            }
            catch ( Exception ex )
            {
                resultBag.ResultType = ConnectionOpportunitySignupResultType.UnknownError;
                resultBag.ResponseMessage = "An unknown error occurred: " + ex.Message;
                return ActionInternalServerError( resultBag.ResponseMessage );
            }
        }

        #endregion Block Actions
    }
}
