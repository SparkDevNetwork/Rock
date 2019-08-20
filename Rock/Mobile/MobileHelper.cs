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
using System.Linq;
using System.Web;

using Rock.Attribute;
using Rock.Data;
using Rock.Mobile.Common;
using Rock.Mobile.Common.Enums;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Mobile
{
    /// <summary>
    /// 
    /// </summary>
    public static class MobileHelper
    {
        /// <summary>
        /// Get the current site as specified by the X-Rock-App-Id header and optionally
        /// validate the X-Rock-Mobile-Api-Key against that site.
        /// </summary>
        /// <param name="validateApiKey"><c>true</c> if the X-Rock-Mobile-Api-Key header should be validated.</param>
        /// <param name="rockContext">The Rock context to use when accessing the database.</param>
        /// <returns>A SiteCache object or null if the request was not valid.</returns>
        public static SiteCache GetCurrentApplicationSite( bool validateApiKey = true, Data.RockContext rockContext = null )
        {
            var appId = HttpContext.Current?.Request?.Headers?["X-Rock-App-Id"];

            if ( !appId.AsIntegerOrNull().HasValue )
            {
                return null;
            }

            //
            // Lookup the site from the App Id.
            //
            var site = SiteCache.Get( appId.AsInteger() );
            if ( site == null )
            {
                return null;
            }

            //
            // If we have been requested to validate the Api Key then do so.
            //
            if ( validateApiKey )
            {
                var appApiKey = System.Web.HttpContext.Current?.Request?.Headers?["X-Rock-Mobile-Api-Key"];
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

                //
                // Ensure we have valid site configuration.
                //
                if ( additionalSettings == null || !additionalSettings.ApiKeyId.HasValue )
                {
                    return null;
                }

                rockContext = rockContext ?? new Data.RockContext();
                var userLogin = new UserLoginService( rockContext ).GetByApiKey( appApiKey ).FirstOrDefault();

                if ( userLogin != null && userLogin.Id == additionalSettings.ApiKeyId )
                {
                    return site;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return site;
            }
        }

        /// <summary>
        /// Get the MobilePerson object for the specified Person.
        /// </summary>
        /// <param name="person">The person to be converted into a MobilePerson object.</param>
        /// <param name="site">The site to use for configuration data.</param>
        /// <returns>A MobilePerson object.</returns>
        public static MobilePerson GetMobilePerson( Person person, SiteCache site )
        {
            var baseUrl = GlobalAttributesCache.Value( "PublicApplicationRoot" );
            var homePhoneTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() ).Id;
            var mobilePhoneTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;

            var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

            if ( person.Attributes == null )
            {
                person.LoadAttributes();
            }

            var personAttributes = person.Attributes
                .Select( a => a.Value )
                .Where( a => a.Categories.Any( c => additionalSettings.PersonAttributeCategories.Contains( c.Id ) ) );

            var roleGuids = RoleCache.AllRoles()
                .Where( r => r.IsPersonInRole( person.Guid ) )
                .Select( r => r.Guid )
                .ToList();

            return new MobilePerson
            {
                FirstName = person.FirstName,
                NickName = person.NickName,
                LastName = person.LastName,
                Gender = ( Common.Enums.Gender ) person.Gender,
                BirthDate = person.BirthDate,
                Email = person.Email,
                HomePhone = person.PhoneNumbers.Where( p => p.NumberTypeValueId == homePhoneTypeId ).Select( p => p.NumberFormatted ).FirstOrDefault(),
                MobilePhone = person.PhoneNumbers.Where( p => p.NumberTypeValueId == mobilePhoneTypeId ).Select( p => p.NumberFormatted ).FirstOrDefault(),
                HomeAddress = GetMobileAddress( person.GetHomeLocation() ),
                CampusGuid = person.GetCampus()?.Guid,
                PersonAliasId = person.PrimaryAliasId.Value,
                PhotoUrl = ( person.PhotoId.HasValue ? $"{baseUrl}{person.PhotoUrl}" : null ),
                SecurityGroupGuids = roleGuids,
                PersonalizationSegmentGuids = new List<Guid>(),
                PersonGuid = person.Guid,
                PersonId = person.Id,
                AttributeValues = GetMobileAttributeValues( person, personAttributes )
            };
        }

        /// <summary>
        /// Generate an authentication token (.ROCK Cookie) for the given username.
        /// </summary>
        /// <param name="username">The username whose token should be generated for.</param>
        /// <returns>A string that represents the user's authentication token.</returns>
        public static string GetAuthenticationToken( string username )
        {
            var ticket = new System.Web.Security.FormsAuthenticationTicket( 1,
                username,
                RockDateTime.Now,
                RockDateTime.Now.Add( System.Web.Security.FormsAuthentication.Timeout ),
                true,
                false.ToString() );

            return System.Web.Security.FormsAuthentication.Encrypt( ticket );
        }

        /// <summary>
        /// Gets the mobile address.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public static MobileAddress GetMobileAddress( Location location )
        {
            if ( location == null )
            {
                return null;
            }

            return new MobileAddress
            {
                Street1 = location.Street1,
                City = location.City,
                State = location.State,
                PostalCode = location.PostalCode,
                Country = location.Country
            };
        }

        /// <summary>
        /// Gets the mobile attribute values.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        public static Dictionary<string, MobileAttributeValue> GetMobileAttributeValues( IHasAttributes entity, IEnumerable<AttributeCache> attributes )
        {
            var mobileAttributeValues = new Dictionary<string, MobileAttributeValue>();

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes();
            }

            foreach ( var attribute in attributes )
            {
                var value = entity.GetAttributeValue( attribute.Key );
                var formattedValue = entity.AttributeValues.ContainsKey( attribute.Key ) ? entity.AttributeValues[attribute.Key].ValueFormatted : attribute.DefaultValueAsFormatted;

                var mobileAttributeValue = new MobileAttributeValue
                {
                    Value = value,
                    FormattedValue = formattedValue
                };

                mobileAttributeValues.AddOrReplace( attribute.Key, mobileAttributeValue );
            }

            return mobileAttributeValues;
        }

        /// <summary>
        /// Builds the mobile package that can be archived for deployment.
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="deviceType">The type of device to build for.</param>
        /// <returns>An update package for the specified application and device type.</returns>
        public static UpdatePackage BuildMobilePackage( int applicationId, DeviceType deviceType )
        {
            var site = SiteCache.Get( applicationId );
            string applicationRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" );
            var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

            //
            // Get all the system phone formats.
            //
            var phoneFormats = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE )
                .DefinedValues
                .Select( dv => new MobilePhoneFormat
                {
                    CountryCode = dv.Value,
                    MatchExpression = dv.GetAttributeValue( "MatchRegEx" ),
                    FormatExpression = dv.GetAttributeValue( "FormatRegEx" )
                } )
                .ToList();

            //
            // Initialize the base update package settings.
            //
            var package = new UpdatePackage
            {
                ApplicationType = additionalSettings.ShellType ?? ShellType.Blank,
                ApplicationVersionId = ( int ) ( RockDateTime.Now.ToJavascriptMilliseconds() / 1000 ),
                CssStyles = additionalSettings?.CssStyle ?? string.Empty,
                LoginPageGuid = site.LoginPageId.HasValue ? PageCache.Get( site.LoginPageId.Value )?.Guid : null,
                ProfileDetailsPageGuid = additionalSettings.ProfilePageId.HasValue ? PageCache.Get( additionalSettings.ProfilePageId.Value )?.Guid : null,
                PhoneFormats = phoneFormats
            };

            //
            // Setup the appearance settings.
            //
            package.AppearanceSettings.BarBackgroundColor = additionalSettings.BarBackgroundColor;
            package.AppearanceSettings.MenuButtonColor = additionalSettings.MenuButtonColor;
            package.AppearanceSettings.ActivityIndicatorColor = additionalSettings.ActivityIndicatorColor;
            package.AppearanceSettings.FlyoutXaml = additionalSettings.FlyoutXaml;

            if ( site.FavIconBinaryFileId.HasValue )
            {
                package.AppearanceSettings.LogoUrl = $"{applicationRoot}/GetImage.ashx?Id={site.FavIconBinaryFileId.Value}";
            }

            //
            // Load all the layouts.
            //
            foreach ( var layout in LayoutCache.All().Where( l => l.SiteId == site.Id ) )
            {
                var mobileLayout = new MobileLayout
                {
                    LayoutGuid = layout.Guid,
                    Name = layout.Name,
                    LayoutXaml = deviceType == DeviceType.Tablet ? layout.LayoutMobileTablet : layout.LayoutMobilePhone
                };

                package.Layouts.Add( mobileLayout );
            }

            //
            // Load all the pages.
            //
            using ( var rockContext = new RockContext() )
            {
                AddPagesToUpdatePackage( package, applicationRoot, rockContext, new[] { PageCache.Get( site.DefaultPageId.Value ) } );
            }

            //
            // Load all the blocks.
            //
            foreach ( var block in BlockCache.All().Where( b => b.Page != null && b.Page.SiteId == site.Id && b.BlockType.EntityTypeId.HasValue ).OrderBy( b => b.Order ) )
            {
                var blockEntityType = block.BlockType.EntityType.GetEntityType();

                if ( typeof( Rock.Blocks.IRockMobileBlockType ).IsAssignableFrom( blockEntityType ) )
                {
                    var additionalBlockSettings = block.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();

                    var mobileBlockEntity = ( Rock.Blocks.IRockMobileBlockType ) Activator.CreateInstance( blockEntityType );

                    mobileBlockEntity.BlockCache = block;
                    mobileBlockEntity.PageCache = block.Page;
                    mobileBlockEntity.RequestContext = new Net.RockRequestContext();

                    var attributes = block.Attributes
                        .Select( a => a.Value )
                        .Where( a => a.Categories.Any( c => c.Name == "custommobile" ) );

                    var mobileBlock = new MobileBlock
                    {
                        PageGuid = block.Page.Guid,
                        Zone = block.Zone,
                        BlockGuid = block.Guid,
                        RequiredAbiVersion = mobileBlockEntity.RequiredMobileAbiVersion,
                        BlockType = mobileBlockEntity.MobileBlockType,
                        ConfigurationValues = mobileBlockEntity.GetMobileConfigurationValues(),
                        Order = block.Order,
                        AttributeValues = GetMobileAttributeValues( block, attributes ),
                        PreXaml = block.PreHtml,
                        PostXaml = block.PostHtml,
                        CssClasses = block.CssClass,
                        ShowOnTablet = additionalBlockSettings.ShowOnTablet,
                        ShowOnPhone = additionalBlockSettings.ShowOnPhone,
                        RequiresNetwork = additionalBlockSettings.RequiresNetwork,
                        NoNetworkContent = additionalBlockSettings.NoNetworkContent,
                        AuthorizationRules = string.Join( ",", GetOrderedExplicitAuthorizationRules( block ) )
                    };

                    package.Blocks.Add( mobileBlock );
                }
            }

            //
            // Load all the campuses.
            //
            foreach ( var campus in CampusCache.All().Where( c => c.IsActive ?? true ) )
            {
                var mobileCampus = new MobileCampus
                {
                    Guid = campus.Guid,
                    Name = campus.Name
                };

                if ( campus.Location != null )
                {
                    if ( campus.Location.Latitude.HasValue && campus.Location.Longitude.HasValue )
                    {
                        mobileCampus.Latitude = campus.Location.Latitude;
                        mobileCampus.Longitude = campus.Location.Longitude;
                    }

                    if ( !string.IsNullOrWhiteSpace( campus.Location.Street1 ) )
                    {
                        mobileCampus.Street1 = campus.Location.Street1;
                        mobileCampus.City = campus.Location.City;
                        mobileCampus.State = campus.Location.State;
                        mobileCampus.PostalCode = campus.Location.PostalCode;
                    }
                }

                package.Campuses.Add( mobileCampus );
            }

            return package;
        }

        /// <summary>
        /// Adds the pages to update package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="applicationRoot">The application root.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="pages">The pages.</param>
        /// <param name="depth">The depth.</param>
        private static void AddPagesToUpdatePackage( UpdatePackage package, string applicationRoot, RockContext rockContext, IEnumerable<PageCache> pages, int depth = 0 )
        {
            foreach ( var page in pages )
            {
                var additionalPageSettings = page.AdditionalSettings.FromJsonOrNull<AdditionalPageSettings>() ?? new AdditionalPageSettings();

                var mobilePage = new MobilePage
                {
                    LayoutGuid = page.Layout.Guid,
                    DisplayInNav = page.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed,
                    Title = page.PageTitle,
                    PageGuid = page.Guid,
                    Order = page.Order,
                    ParentPageGuid = page.ParentPage?.Guid,
                    IconUrl = page.IconBinaryFileId.HasValue ? $"{ applicationRoot }GetImage.ashx?Id={ page.IconBinaryFileId.Value }" : null,
                    LavaEventHandler = additionalPageSettings.LavaEventHandler,
                    DepthLevel = depth,
                    AuthorizationRules = string.Join( ",", GetOrderedExplicitAuthorizationRules( page ) )
                };

                package.Pages.Add( mobilePage );

                var childPages = page.GetPages( rockContext );
                if ( childPages.Any() )
                {
                    AddPagesToUpdatePackage( package, applicationRoot, rockContext, childPages, depth + 1 );
                }
            }
        }

        /// <summary>
        /// Gets the ordered explicit authorization rules for an entity. The first rule in the list
        /// should be checked first, and so on. The order follows from the first explicit rule on the
        /// entity to the last explicit rule on the entity, then the first explicit rule on the parent
        /// entity to the last explicit rule on the parent entity and so on.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A collection of explicit authorization rules in the proper order.</returns>
        private static List<string> GetOrderedExplicitAuthorizationRules( ISecured entity )
        {
            var explicitRules = new List<string>();

            if ( entity == null )
            {
                return explicitRules;
            }

            //
            // Get the ancestor authorization rules.
            //
            var parentEntity = entity.ParentAuthority;
            if ( parentEntity != null )
            {
                explicitRules = GetOrderedExplicitAuthorizationRules( parentEntity );
            }

            var authRules = Authorization.AuthRules( entity.TypeId, entity.Id, Authorization.VIEW );

            //
            // Walk each rule in descending order so that the final order is correct
            // since we insert rules at index 0.
            //
            foreach ( var rule in authRules.OrderByDescending( a => a.Order ) )
            {
                string entityIdentifier;

                if ( rule.SpecialRole != SpecialRole.None )
                {
                    entityIdentifier = $"S:{( int ) rule.SpecialRole}";
                }
                else if ( rule.GroupId.HasValue )
                {
                    var role = RoleCache.Get( rule.GroupId.Value );

                    if ( role == null )
                    {
                        continue;
                    }

                    entityIdentifier = $"G:{role.Guid}";
                }
                else if ( rule.PersonId.HasValue )
                {
                    /* Not currently supported, maybe in the future. -dsh */
                    continue;
                }
                else
                {
                    continue;
                }

                explicitRules.Insert( 0, $"{entityIdentifier}:{rule.AllowOrDeny}" );
            }

            return explicitRules;
        }
    }
}
