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

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.SystemKey;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonPreferences;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Allows the person to set their personal preferences.
    /// </summary>
    [DisplayName( "Person Preferences" )]
    [Category( "CRM" )]
    [Description( "Allows the person to set their personal preferences." )]
    [IconCssClass( "ti ti-user" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "F0F74227-7025-4330-BC08-33262C327A52" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "B185A172-488A-40D1-9478-49F7EAB0E94F" )]
    [Rock.SystemGuid.BlockTypeGuid( "D2049782-C286-4EE1-94E8-039111E16794" )]
    public class PersonPreferences : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys for attributes referenced by this block.
        /// </summary>
        private static class AttributeKey
        {
            public const string InternalPhoneType = "InternalPhoneType";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<PersonPreferencesBag, PersonPreferencesOptionsBag>();
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                box.ErrorMessage = "You must be logged in to set your preferences.";
                return box;
            }

            var preferences = GetGlobalPersonPreferences();

            box.Bag = new PersonPreferencesBag
            {
                DefaultSmsPhoneNumber = preferences.GetValue( PersonPreferenceKey.DEFAULT_SMS_PHONE_NUMBER ),
                EmailClosingPhrase = preferences.GetValue( PersonPreferenceKey.EMAIL_CLOSING_PHRASE )
            };

            // Resolve the PBX component once; both the options' visibility flag and the bag's default
            // call origination source depend on it, so it must not be looked up twice.
            var pbxComponent = Rock.Pbx.PbxContainer.GetAllowedActiveComponentWithOriginationSupport( currentPerson );

            box.Options = GetBoxOptions( currentPerson, pbxComponent );

            if ( pbxComponent != null )
            {
                // Fall back to the component's configured default phone type when the person has no saved preference.
                var phoneTypeId = preferences.GetValue( PersonPreferenceKey.ORIGINATE_CALL_SOURCE ).AsIntegerOrNull()
                    ?? pbxComponent.GetAttributeValue( AttributeKey.InternalPhoneType ).AsIntegerOrNull();

                box.Bag.CallOriginationSource = phoneTypeId.HasValue
                    ? DefinedValueCache.Get( phoneTypeId.Value )?.ToListItemBag()
                    : null;
            }

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render.
        /// </summary>
        /// <param name="currentPerson">The person whose SMS number authorization is evaluated.</param>
        /// <param name="pbxComponent">The active, origination-capable PBX component for the person, or <c>null</c> when none is available. The call origination source is only shown when this is provided.</param>
        /// <returns>The options bag used to initialize the block.</returns>
        private PersonPreferencesOptionsBag GetBoxOptions( Person currentPerson, Rock.Pbx.PbxComponent pbxComponent )
        {
            return new PersonPreferencesOptionsBag
            {
                DefaultSmsPhoneNumberOptions = GetSmsPhoneNumberOptions( currentPerson ),

                // The call origination source is only shown when an authorized, origination-capable PBX component is active.
                IsCallOriginationSourceVisible = pbxComponent != null
            };
        }

        /// <summary>
        /// Gets the SMS-enabled system phone numbers the person is authorized to view.
        /// </summary>
        /// <param name="currentPerson">The person whose authorization is evaluated.</param>
        /// <returns>The list of phone number options keyed by their identifier.</returns>
        private List<ListItemBag> GetSmsPhoneNumberOptions( Person currentPerson )
        {
            return SystemPhoneNumberCache.All()
                .Where( spn => spn.IsSmsEnabled && spn.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .Select( spn => new ListItemBag { Value = spn.Id.ToString(), Text = spn.Name } )
                .ToList();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Persists the person's preferences.
        /// </summary>
        /// <param name="bag">The preference values to save.</param>
        /// <returns>An empty successful result, or an error result when the request is invalid.</returns>
        [BlockAction]
        public BlockActionResult Save( PersonPreferencesBag bag )
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            if ( bag == null )
            {
                return ActionBadRequest( "No preferences were provided." );
            }

            var preferences = GetGlobalPersonPreferences();

            // Store the call origination source as a phone-type Id, but persist nothing when it matches the
            // PBX component's default so administrator default changes continue to take effect.
            var pbxComponent = Rock.Pbx.PbxContainer.GetAllowedActiveComponentWithOriginationSupport( currentPerson );
            var defaultPhoneTypeId = pbxComponent?.GetAttributeValue( AttributeKey.InternalPhoneType ).AsIntegerOrNull();

            var selectedPhoneTypeGuid = bag.CallOriginationSource?.Value.AsGuidOrNull();
            var selectedPhoneTypeId = selectedPhoneTypeGuid.HasValue
                ? DefinedValueCache.Get( selectedPhoneTypeGuid.Value )?.Id
                : null;

            var originateCallSource = selectedPhoneTypeId.HasValue && selectedPhoneTypeId != defaultPhoneTypeId
                ? selectedPhoneTypeId.Value.ToString()
                : string.Empty;

            preferences.SetValue( PersonPreferenceKey.ORIGINATE_CALL_SOURCE, originateCallSource );
            preferences.SetValue( PersonPreferenceKey.DEFAULT_SMS_PHONE_NUMBER, bag.DefaultSmsPhoneNumber ?? string.Empty );
            preferences.SetValue( PersonPreferenceKey.EMAIL_CLOSING_PHRASE, bag.EmailClosingPhrase ?? string.Empty );

            preferences.Save();

            return ActionOk();
        }

        #endregion
    }
}
