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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Data;
using Rock.Event.InteractiveExperiences;
using Rock.Lava;
using Rock.Model;
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.LiveExperienceOccurrences;
using Rock.Web.Cache;

namespace Rock.Blocks.Event.InteractiveExperiences
{
    /// <summary>
    /// Displays a list of interactive experience occurrences for the user to pick from.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Live Experience Occurrences" )]
    [Category( "Event > Interactive Experiences" )]
    [Description( "Displays a list of interactive experience occurrences for the individual to pick from." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Destination Page",
        Description = "The page to link to when selecting an occurrence.",
        IsRequired = true,
        Key = AttributeKey.DestinationPage,
        Order = 0 )]

    [LinkedPage( "Login Page",
        Description = "The page to use when showing the login page. If not set then the default site login page will be used instead.",
        IsRequired = false,
        Key = AttributeKey.LoginPage,
        Order = 1 )]

    [BooleanField( "Show All",
        Description = "When enabled, normal filtering is not performed and all active occurrences will be shown. Intended for use on admin pages.",
        IsRequired = true,
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowAll,
        Order = 2 )]

    [BooleanField( "Always Request Location",
        Description = "When enabled, the device location will always be requested. Otherwise it will only be used if it has already been requested in the past.",
        DefaultBooleanValue = false,
        Key = AttributeKey.AlwaysRequestLocation,
        Order = 3 )]

    [CodeEditorField( "Template",
        Description = "The lava template to use when rendering the occurrences on the page.",
        IsRequired = true,
        DefaultValue = AttributeDefault.Template,
        Key = AttributeKey.Template,
        Order = 4 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b7e9ac68-35db-4d18-a5db-a250832e8ad9" )]
    [Rock.SystemGuid.BlockTypeGuid( "8b384269-3d54-4c84-b230-2061be4866f9" )]
    public class LiveExperienceOccurrences : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string AlwaysRequestLocation = "AlwaysRequestLocation";
            public const string DestinationPage = "DestinationPage";
            public const string LoginPage = "LoginPage";
            public const string ShowAll = "ShowAll";
            public const string Template = "Template";
        }

        private static class PageParameterKey
        {
            public const string InteractiveExperienceOccurrenceId = "InteractiveExperienceOccurrenceId";
        }

        private static class AttributeDefault
        {
            public const string Template = @"{% if Occurrences == empty %}
    <div class=""alert alert-info"">
        There are not any live experiences in progress.
    </div>
{% endif %}

{% for occurrence in Occurrences %}
    {% if occurrence.Campus != null %}
        {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }} at {{ occurrence.Campus.Name }}{% endcapture %}
    {% else %}
        {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }}{% endcapture %}
    {% endif %}

    <a class=""d-flex rounded overflow-hidden mb-2 align-items-stretch border border-gray-400 bg-white"" href=""{{ occurrence.PageUrl | Escape }}"">
        <div class=""p-2 d-flex align-items-center align-self-stretch bg-info text-white"">
            <span>
                <i class=""fa fa-calendar-alt""></i>
            </span>
        </div>

        <div class=""p-2 d-flex align-items-center align-self-stretch flex-grow-1 text-body"">
            {{ occurrenceName | Escape }}
        </div>

        <div class=""p-2 mr-2 d-flex align-items-center align-self-stretch text-info"">
            <span>
                <i class=""fa fa-arrow-circle-right""></i>
            </span>
        </div>
    </a>
{% endfor %}

{% if LoginRecommended == true %}
    <div class=""alert alert-info"">
        There may be more experiences available to you if you <a href=""{{ LoginUrl | Escape }}"">login</a>.
    </div>
{% endif %}

{% if GeoLocationRecommended == true %}
    <div class=""alert alert-info"">
        There may be more experiences available to you if you <a href=""{{ ProvideLocationUrl | Escape }}"">provide your location</a>.
    </div>
{% endif %}";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new LiveExperienceOccurrencesInitializationBox
                {
                    AlwaysRequestLocation = GetAttributeValue( AttributeKey.AlwaysRequestLocation ).AsBoolean(),
                    ProvideLocationKey = GetProvideLocationKey(),
                    SecurityGrantToken = GetSecurityGrantToken(),
                    NavigationUrls = GetBoxNavigationUrls()
                };

                return box;
            }
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
            };
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Gets the page route URL for given occurrence based on the block settings.
        /// </summary>
        /// <param name="occurrence">The occurrence whose page route is to be determined.</param>
        /// <returns>The page route for viewing the occurrence.</returns>
        private string GetOccurrenceUrl( InteractiveExperienceOccurrence occurrence )
        {
            return this.GetLinkedPageUrl( AttributeKey.DestinationPage, new Dictionary<string, string>
            {
                [PageParameterKey.InteractiveExperienceOccurrenceId] = occurrence.IdKey
            } );
        }

        /// <summary>
        /// Gets the URL to use when clicking the login button.
        /// </summary>
        /// <returns>A string representing the URL of the login page.</returns>
        private string GetLoginUrl()
        {
            if ( GetAttributeValue( AttributeKey.LoginPage ).IsNotNullOrWhiteSpace() )
            {
                return this.GetLinkedPageUrl( AttributeKey.LoginPage, new Dictionary<string, string>
                {
                    ["returnurl"] = this.GetCurrentPageUrl()
                } );
            }
            else
            {
                return this.GetLoginPageUrl( this.GetCurrentPageUrl() );
            }
        }

        /// <summary>
        /// Gets the key used for the provide location callback.
        /// </summary>
        /// <returns>A string that represents the key name of callback in the window object.</returns>
        private string GetProvideLocationKey()
        {
            return $"block-{BlockCache.Id}-provideLocation";
        }

        /// <summary>
        /// Gets the page content HTML to use when rendering the dynamic list of
        /// occurrences that this individual should see.
        /// </summary>
        /// <param name="latitude">The optional latitude for geolocation matching.</param>
        /// <param name="longitude">The optional longitude for geolocation matching.</param>
        /// <returns>A string that contains the HTML to be rendered.</returns>
        private string GetContentHtml( double? latitude, double? longitude )
        {
            var showAll = GetAttributeValue( AttributeKey.ShowAll ).AsBoolean();

            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );
                var validOccurrences = showAll
                    ? new ValidOccurrencesResult( occurrenceService.GetActiveOccurrences().ToList(), false, false )
                    : occurrenceService.GetValidOccurrences( RequestContext.CurrentPerson, latitude, longitude );

                var lavaTemplate = GetAttributeValue( AttributeKey.Template );
                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "Occurrences", validOccurrences.Occurrences
                    .Select( o => new LavaOccurrence( o, GetOccurrenceUrl( o ) ) )
                    .ToList() );
                mergeFields.Add( "LoginRecommended", validOccurrences.LoginRecommended );
                mergeFields.Add( "GeoLocationRecommended", validOccurrences.GeoLocationRecommended );
                mergeFields.Add( "LoginUrl", GetLoginUrl() );
                mergeFields.Add( "ProvideLocationUrl", $"javascript:window[\"{GetProvideLocationKey()}\"]()" );

                return lavaTemplate.ResolveMergeFields( mergeFields );
            }
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult GetContent( double? latitude, double? longitude )
        {
            return ActionOk( new
            {
                Content = GetContentHtml( latitude, longitude )
            } );
        }

        #endregion

        #region Support Classes

        private class LavaOccurrence : LavaDataDictionaryExtension
        {
            public string PageUrl { get; }

            public LavaOccurrence( InteractiveExperienceOccurrence occurrence, string pageUrl )
                : base( occurrence )
            {
                PageUrl = pageUrl;
            }
        }

        private class LavaDataDictionaryExtension : ILavaDataDictionary, DotLiquid.ILiquidizable, DotLiquid.IIndexable
        {
            private readonly ILavaDataDictionary _baseObject;
            private readonly Dictionary<string, PropertyInfo> _additionalKeys;

            public LavaDataDictionaryExtension( ILavaDataDictionary baseObject )
            {
                _baseObject = baseObject;

                _additionalKeys = GetType()
                    .GetProperties( BindingFlags.Public | BindingFlags.Instance )
                    .ToDictionary( p => p.Name, p => p );
            }

            private List<string> GetAllAvailableKeys()
            {
                var keys = new List<string>( _baseObject.AvailableKeys );

                keys.AddRange( _additionalKeys.Keys );

                return keys;
            }

            private object GetValue( string key )
            {
                if ( _additionalKeys.TryGetValue( key, out var pi ) )
                {
                    return pi.GetValue( this );
                }

                var value = _baseObject.GetValue( key );

                return value;
            }

            public override string ToString()
            {
                return _baseObject.ToString();
            }

            #region ILavaDataDictionary

            List<string> ILavaDataDictionary.AvailableKeys => GetAllAvailableKeys();

            bool ILavaDataDictionary.ContainsKey( string key )
            {
                return _additionalKeys.ContainsKey( key ) || _baseObject.ContainsKey( key );
            }

            object ILavaDataDictionary.GetValue( string key )
            {
                return GetValue( key );
            }

            string ILavaDataDictionary.ToString()
            {
                return ToString();
            }

            #endregion

            #region ILiquidizable

            object DotLiquid.ILiquidizable.ToLiquid()
            {
                return this;
            }

            #endregion

            #region IIndexable

            object DotLiquid.IIndexable.this[object key] => GetValue( key.ToString() );

            bool DotLiquid.IIndexable.ContainsKey( object key )
            {
                return GetAllAvailableKeys().Contains( key );
            }

            #endregion
        }

        #endregion
    }
}
