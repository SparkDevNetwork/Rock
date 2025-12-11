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
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonalDevices;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Crm
{
    [DisplayName( "Personal Devices" )]
    [Category( "CRM" )]
    [Description( "Shows a list of all personal devices." )]

    [LinkedPage(
        "Interactions Page",
        Key = NavigationUrlKey.InteractionsPage,
        Description = "The interactions associated with a specific personal device.",
        Order = 0 )]

    [BooleanField(
        "Show Device Last Seen DateTime",
        Description = "Checking this option will display the time when the device was last seen.",
        DefaultBooleanValue = false,
        Order = 1,
        Key = AttributeKey.ShowDeviceLastSeenDateTime )]

    [ContextAware( typeof( Person ) )]
    // was [Rock.SystemGuid.BlockTypeGuid( "9A504904-8AF6-4351-AE31-CBC4DB2F55BA" )]
    [Rock.SystemGuid.BlockTypeGuid( "2D90562E-7332-46DB-9100-0C4106151CA1" )]
    public class PersonalDevices : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ShowDeviceLastSeenDateTime = "ShowDeviceLastSeenDateTime";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
            public const string PersonGuid = "PersonGuid";
        }

        private static class NavigationUrlKey
        {
            public const string InteractionsPage = "InteractionsPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
			var box = new CustomBlockBox<PersonalDevicesBag, PersonalDevicesOptionsBag>();

			var person = GetPerson();

			var personalDevices = GetPersonalDevices( person?.Id );

			box.Bag = new PersonalDevicesBag
			{
				PersonalDevices = personalDevices,
                PersonName = person?.FullName ?? string.Empty
            };

            box.Options.ShowDeviceLastSeenDateTime = GetAttributeValue( AttributeKey.ShowDeviceLastSeenDateTime ).AsBoolean();
            box.Options.DeviceTypeOptions = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_TYPE.AsGuid() ).DefinedValues.ToListItemBagList();

            return box;
        }
            
		/// <summary>
		/// Gets the current person either from the context or the page parameter.
		/// </summary>
		/// <returns>The resolved person or null.</returns>
		private Person GetPerson()
		{
			var person = this.RequestContext.GetContextEntity<Person>();
			if ( person != null )
			{
				return person;
			}

			var personIdKey = PageParameter( PageParameterKey.PersonId );
			if ( personIdKey.IsNotNullOrWhiteSpace() )
			{
				return new PersonService( RockContext ).Get( personIdKey, !PageCache.Layout.Site.DisablePredictableIds );
			}

            var personGuidKey = PageParameter( PageParameterKey.PersonGuid );
            if ( personGuidKey.IsNotNullOrWhiteSpace() )
            {
                var personGuid = personGuidKey.AsGuidOrNull();
                if ( personGuid.HasValue )
                {
                    return new PersonService( RockContext ).Get( personGuid.Value );
                }
            }

			return null;
		}

		/// <summary>
		/// Gets the personal devices for the specified person identifier.
		/// </summary>
		/// <param name="personId">The person identifier.</param>
		/// <returns>A list of personal device list item bags.</returns>
		private List<PersonalDeviceListItemBag> GetPersonalDevices( int? personId )
		{
			if ( !personId.HasValue )
			{
				return new List<PersonalDeviceListItemBag>();
			}

			var personalDeviceService = new PersonalDeviceService( RockContext );

			var personalDevices = personalDeviceService
				.Queryable()
				.AsNoTracking()
				.Include( pd => pd.PersonalDeviceType )
				.Include( pd => pd.Platform )
				.Where( pd => pd.PersonAlias != null && pd.PersonAlias.PersonId == personId.Value )
				.ToList();

			var items = personalDevices.Select( pd => new PersonalDeviceListItemBag
			{
				Name = pd.Name,
				IsActive = pd.IsActive,
				Guid = pd.Guid,
				DeviceType = pd.PersonalDeviceType != null
					? pd.PersonalDeviceType.ToListItemBag()
					: null,
				IconCssClass = pd.PersonalDeviceTypeValueId.HasValue
					? DefinedValueCache.Get( pd.PersonalDeviceTypeValueId.Value )?.GetAttributeValue( "IconCssClass" )
					: null,
				Platform = pd.Platform != null
					? pd.Platform.ToListItemBag()
					: null,
				DeviceVersion = pd.DeviceVersion,
				MacAddress = pd.MACAddress,
				NotificationsEnabled = pd.NotificationsEnabled,
				LocationPermissionStatus = pd.LocationPermissionStatus,
				IsPreciseLocationEnabled = pd.IsPreciseLocationEnabled,
				IsBeaconMonitoringEnabled = pd.IsBeaconMonitoringEnabled,
				CreatedDateTime = pd.CreatedDateTime,
				LastSeenDateTime = pd.LastSeenDateTime,
			} ).ToList();

            return items;
		}

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified personal device.
        /// </summary>
        /// <param name="key">The personal device key (IdKey) identifying the device to delete.</param>
        [BlockAction]
        public BlockActionResult DeletePersonalDevice( string key )
        {
            var personalDeviceService = new PersonalDeviceService( RockContext );
            var personalDevice = personalDeviceService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( personalDevice == null )
            {
                return ActionBadRequest( "Personal Device not found." );
            }

            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to delete this personal device." );
            }

            if ( !personalDeviceService.CanDelete( personalDevice, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            personalDeviceService.Delete( personalDevice );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the interactions page URL for the specified personal device.
        /// </summary>
        /// <param name="key">The identifier of the personal device.</param>
        /// <returns>The URL to navigate to for viewing interactions.</returns>
        [BlockAction]
        public BlockActionResult GetInteractionsUrl( string key )
        {
            var personalDeviceService = new PersonalDeviceService( RockContext );
            var personalDevice = personalDeviceService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( personalDevice == null )
            {
                return ActionBadRequest( "Personal Device not found." );
            }

            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to view interactions for this personal device." );
            }

            var queryParams = new Dictionary<string, string>
            {
                ["PersonalDeviceId"] = personalDevice.IdKey
            };

            var url = this.GetLinkedPageUrl( NavigationUrlKey.InteractionsPage, queryParams );

            if ( string.IsNullOrWhiteSpace( url ) )
            {
                return ActionBadRequest( "Interactions page is not configured." );
            }

            return ActionOk( url );
        }

        #endregion Block Actions
    }
}
