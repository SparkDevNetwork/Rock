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
using System.Web.Http;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.BusinessList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    [DisplayName( "Business List" )]
    [Category( "Finance" )]
    [Description( "Displays a list of businesses." )]
    [IconCssClass( "fa fa-building" )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the business details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.BlockTypeGuid( "1e60c390-98c4-404d-aee8-f9e3e9c69705" )]
    [CustomizedGrid]
    public class BusinessList : RockListBlockType<BusinessListBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<BusinessListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        private BusinessListOptionsBag GetBoxOptions()
        {
            var options = new BusinessListOptionsBag();
            return options;
        }

        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized(Authorization.EDIT, RequestContext.CurrentPerson);
        }

        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "BusinessId", "((Key))" )
            };
        }

        protected override IQueryable<BusinessListBag> GetListQueryable(RockContext rockContext)
        {
            var recordTypeValueId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid()).Id;
            int workLocationTypeId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid()).Id;

            var groupTypeIdKnownRelationships = GroupTypeCache.Get(Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid()).Id;
            var groupTypeRoleIdKnownRelationshipsOwner = GroupTypeCache.Get(Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid())
                .Roles.FirstOrDefault(r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid()).Id;

            // Fetch the businesses
            var businessQueryable = new PersonService(rockContext)
                .Queryable("PhoneNumbers, GivingGroup.GroupLocations.Location, PrimaryCampus")
                .Where(p => p.RecordTypeValueId == recordTypeValueId);

            // Retrieve SearchTerm from the current HTTP context
            var searchTerm = PageParameter("SearchTerm");
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                businessQueryable = businessQueryable.Where(p => p.LastName.Contains(searchTerm));
            }

            // Project the data to BusinessListBag
            return businessQueryable.Select(p => new BusinessListBag
            {
                Id = p.Id,
                   BusinessName = p.LastName,
                   Email = p.Email,
                   Phone = p.PhoneNumbers.FirstOrDefault() != null ? p.PhoneNumbers.FirstOrDefault().NumberFormatted : string.Empty,
                   Street = p.GivingGroup.GroupLocations.Any( gl => gl.GroupLocationTypeValueId == workLocationTypeId ) ? p.GivingGroup.GroupLocations.FirstOrDefault( gl => gl.GroupLocationTypeValueId == workLocationTypeId ).Location.Street1 : string.Empty,
                   City = p.GivingGroup.GroupLocations.Any( gl => gl.GroupLocationTypeValueId == workLocationTypeId ) ? p.GivingGroup.GroupLocations.FirstOrDefault( gl => gl.GroupLocationTypeValueId == workLocationTypeId ).Location.City : string.Empty,
                   State = p.GivingGroup.GroupLocations.Any( gl => gl.GroupLocationTypeValueId == workLocationTypeId ) ? p.GivingGroup.GroupLocations.FirstOrDefault( gl => gl.GroupLocationTypeValueId == workLocationTypeId ).Location.State : string.Empty,
                   Zip = p.GivingGroup.GroupLocations.Any( gl => gl.GroupLocationTypeValueId == workLocationTypeId ) ? p.GivingGroup.GroupLocations.FirstOrDefault( gl => gl.GroupLocationTypeValueId == workLocationTypeId ).Location.PostalCode : string.Empty,
                   Campus = new ListItemBag { Value = p.PrimaryCampus.Id.ToString(), Text = p.PrimaryCampus.Name },
                   Contacts = p.Members
                       .Where( m => m.Group.GroupTypeId == groupTypeIdKnownRelationships )
                       .SelectMany( m => m.Group.Members )
                       .Where( member => member.GroupRoleId == groupTypeRoleIdKnownRelationshipsOwner && member.PersonId != p.Id )
                       .Select( member => member.Person.LastName + ", " + member.Person.NickName )
                       .FirstOrDefault()
               } );
        }

        protected override GridBuilder<BusinessListBag> GetGridBuilder()
        {
            return new GridBuilder<BusinessListBag>()
                .WithBlock( this )
                .AddField( "id", b => b.Id )
                .AddTextField( "businessName", b => b.BusinessName )
                .AddTextField( "email", b => b.Email )
                .AddTextField( "phoneNumber", b => b.Phone )
                .AddTextField( "contacts", b => b.Contacts )
                .AddTextField( "campus", b => b.Campus.Text )
                .AddTextField( "street", b => b.Street )
                .AddTextField( "city", b => b.City )
                .AddTextField( "state", b => b.State )
                .AddTextField( "zip", b => b.Zip );
        }
        #endregion

    }
}
