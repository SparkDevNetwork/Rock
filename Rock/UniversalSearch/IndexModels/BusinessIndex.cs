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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;
using Rock.Web.Cache;

namespace Rock.UniversalSearch.IndexModels
{
    /// <summary>
    /// Business Index
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexModels.IndexModelBase" />
    public class BusinessIndex : IndexModelBase
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the contacts.
        /// </summary>
        /// <value>
        /// The contacts.
        /// </value>
        public string Contacts { get; set; }


        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        /// <value>
        /// The street address.
        /// </value>
        [RockIndexField]
        public string StreetAddress { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [RockIndexField]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [RockIndexField]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [RockIndexField]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [RockIndexField]
        public string Country { get; set; }

        /// <summary>
        /// Gets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public override string IconCssClass
        {
            get
            {
                return "fa fa-building";
            }
        }

        /// <summary>
        /// Loads by model.
        /// </summary>
        /// <param name="business">The business.</param>
        /// <returns></returns>
        public static BusinessIndex LoadByModel( Person business )
        {
            var businessIndex = new BusinessIndex();
            businessIndex.SourceIndexModel = "Rock.Model.Person";
            businessIndex.ModelConfiguration = "nofilters";

            businessIndex.ModelOrder = 6;

            businessIndex.Id = business.Id;
            businessIndex.Name = business.LastName;
            businessIndex.DocumentName = business.LastName;

            // get mailing address
            var address = business.GetMailingLocation();

            if ( address != null )
            {
                businessIndex.StreetAddress = address.Street1 + " " + address.Street2;
                businessIndex.City = address.City;
                businessIndex.State = address.State;
                businessIndex.PostalCode = address.PostalCode;
                businessIndex.Country = address.Country;
            }

            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            var knownRelationshipOwnerRoleId = knownRelationshipGroupType.Roles.Where( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ).FirstOrDefault().Id;
            var knownRelationshipBusinessContactId = knownRelationshipGroupType.Roles.Where( r => r.Guid == SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT.AsGuid() ).FirstOrDefault().Id;

            RockContext rockContext = new RockContext();
            var contactGroup = new GroupMemberService( rockContext ).Queryable()
                                        .Where( m =>
                                             m.Group.GroupTypeId == knownRelationshipGroupType.Id
                                             && m.GroupRoleId == knownRelationshipOwnerRoleId
                                             && m.PersonId == business.Id )
                                        .FirstOrDefault();

            if ( contactGroup != null )
            {
                var contacts = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                    .Where( m =>
                                         m.Group.GroupTypeId == knownRelationshipGroupType.Id
                                         && m.GroupId == contactGroup.GroupId
                                         && m.GroupRoleId == knownRelationshipBusinessContactId )
                                    .Select( m => m.Person.NickName + " " + m.Person.LastName ).ToList();

                if ( contacts != null )
                {
                    businessIndex.Contacts = string.Join( ", ", contacts );
                }
            }

            return businessIndex;
        }
    }
}
