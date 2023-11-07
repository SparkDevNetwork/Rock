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

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PersonalLinkService : Service<PersonalLink>
    {
        /// <summary>
        /// Returns a queryable of the ordered personal link sections (shared and non-shared) that would be available to the specified person.
        /// They will be sorted by the order that the specified person put them in,
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>IOrderedQueryable&lt;PersonalLinkSection&gt;.</returns>
        public IOrderedQueryable<PersonalLinkSection> GetOrderedPersonalLinkSectionsQuery( Person currentPerson )
        {
            var rockContext = this.Context as RockContext;

            var personAliasQuery = new PersonAliasService( rockContext ).Queryable().Where( a => a.PersonId == currentPerson.Id );
            var sectionOrderQuery = GetSectionOrderQuery( currentPerson );

            var orderedPersonalLinkSectionQuery = new PersonalLinkSectionService( rockContext )
                .Queryable()
                .Where( a => a.IsShared || ( a.PersonAliasId.HasValue && personAliasQuery.Any( xx => xx.Id == a.PersonAliasId.Value ) ) )
                .OrderBy( a => sectionOrderQuery.Where( xx => xx.SectionId == a.Id ).Select( s => s.Order ).FirstOrDefault() )
                .ThenBy( a => a.Name );

            return orderedPersonalLinkSectionQuery;
        }

        /// <summary>
        /// Finds any missing personal link section orders
        /// and <see cref="Rock.Data.Service{T}.Add(T)">adds</see> them  (but doesn't .SaveChanges)
        /// If this returns true, call SaveChanges to save them to the database.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>List&lt;PersonalLinkSectionOrder&gt;.</returns>
        public bool AddMissingPersonalLinkSectionOrders( Person currentPerson )
        {
            var primaryAliasId = currentPerson.PrimaryAliasId;
            if ( !primaryAliasId.HasValue )
            {
                // shouldn't happen, but just in case
                return false;
            }

            var rockContext = this.Context as RockContext;

            var personalLinkSectionService = new PersonalLinkSectionService( rockContext );

            var personAliasQuery = new PersonAliasService( rockContext ).Queryable().Where( a => a.PersonId == currentPerson.Id );
            var personalLinkSectionsQuery = personalLinkSectionService
                .Queryable()
                .Where( a =>
                    a.IsShared
                    || ( a.PersonAliasId.HasValue && personAliasQuery.Any( aa => aa.Id == a.PersonAliasId.Value ) ) );

            var missingSectionOrders = personalLinkSectionsQuery
                .Where( a => !a.PersonalLinkSectionOrders.Any( xx => personAliasQuery.Any( pa => pa.Id == xx.PersonAliasId ) ) )
                .ToList()
                .OrderBy( a => a.Name )
                .Select( a => new PersonalLinkSectionOrder
                {
                    PersonAliasId = primaryAliasId.Value,
                    SectionId = a.Id,
                    Order = 0
                } ).ToList();

            if ( missingSectionOrders.Any() )
            {
                // add the new order for sections to the bottom of the list for that section (in order by name)
                var personalLinkSectionOrderService = new PersonalLinkSectionOrderService( rockContext );

                var lastSectionOrder = personalLinkSectionOrderService.Queryable().Where( a => a.PersonAlias.PersonId == currentPerson.Id ).Max( a => ( int? ) a.Order ) ?? 0;
                foreach ( var missingSectionOrder in missingSectionOrders )
                {
                    missingSectionOrder.Order = lastSectionOrder++;
                }

                personalLinkSectionOrderService.AddRange( missingSectionOrders );
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the section order query.
        /// Use this to make sure to get the best match for in cases where
        /// a person has more than one sort order for a section (which could happen if there was person merge)
        /// </summary>
        /// <remarks>
        /// If you are unsure if there are some sections that don't have an order, use <see cref="AddMissingPersonalLinkSectionOrders(Person)"/> first.
        /// </remarks>
        /// <param name="person">The person.</param>
        /// <returns>System.Linq.IQueryable&lt;Rock.Model.PersonalLinkSectionOrder&gt;.</returns>
        public IOrderedQueryable<PersonalLinkSectionOrder> GetSectionOrderQuery( Person person )
        {
            var rockContext = this.Context as RockContext;

            var personAliasQuery = new PersonAliasService( rockContext ).Queryable().Where( a => a.PersonId == person.Id );

            // There could be duplicate Orders for a Section for a person if the person was merged.
            // So, to ensure a consistent Order winner, also sort by ModifiedDateTime. That will
            // ensure the the most recent sort will be one the current person sees.
            var personalLinkSectionOrder = new PersonalLinkSectionOrderService( rockContext ).Queryable()
                .Where( a => personAliasQuery.Any( xx => xx.Id == a.PersonAliasId ) )
                .GroupBy( a => a.SectionId )
                .Select( a => a.OrderBy( xx => xx.Order ).ThenByDescending( xx => xx.ModifiedDateTime ).FirstOrDefault() )
                .OrderBy( a => a.Order );

            return personalLinkSectionOrder;
        }

        #region Helpers

        /// <summary>
        /// Gets the personal links modification hash.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>System.String.</returns>
        public static string GetPersonalLinksModificationHash( Person currentPerson )
        {
            var modificationData = new
            {
                LastSharedLinksUpdateRockDateTime = SharedPersonalLinkSectionCache.LastModifiedDateTime,
                LastNonSharedLinksUpdateDateTime = PersonalLinksHelper.GetPersonalLinksLastModifiedDateTime( currentPerson ),
                PersonGuid = currentPerson?.Guid,
            };

            var modificationJson = modificationData.ToJson();

            return modificationJson.XxHash();
        }

        /// <summary>
        /// Gets the quick links local storage key.
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>System.String.</returns>
        public static string GetQuickLinksLocalStorageKey( Person currentPerson )
        {
            // we want quickLinks local storage to be specific to person (just in case multiple people log in on the same browser)
            return $"quickLinks_{currentPerson?.Guid.ToString().XxHash()}";
        }

        /// <summary>
        /// Gets the personal links data that the specified person is authorized to view
        /// </summary>
        /// <param name="currentPerson">The current person.</param>
        /// <returns>PersonalLinksData.</returns>
        public static PersonalLinksData GetPersonalLinksData( Person currentPerson )
        {
            var rockContext = new RockContext();

            // get the sections (with the Links) that the user is authorized to view
            var orderedPersonalLinkSectionsWithLinks = new PersonalLinkService( rockContext )
                .GetOrderedPersonalLinkSectionsQuery( currentPerson )
                .Include( a => a.PersonAlias )
                .Include( a => a.PersonalLinks.Select( x => x.PersonAlias ) )
                .AsNoTracking()
                .ToList()
                .Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .Where( a => a.PersonalLinks.Any() ).ToList();

            int sectionOrder = 0;

            var personalLinksDataList = orderedPersonalLinkSectionsWithLinks.Select( a =>
            {
                {
                    PersonLinksSectionData result = new PersonLinksSectionData
                    {
                        Id = a.Id,
                        Name = a.Name,
                        IsShared = a.IsShared,
                        Order = sectionOrder++
                    };

                    result.PersonalLinks = a.PersonalLinks
                        .Where( xx => xx.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                        .Select( l => new PersonalLinkData
                        {
                            Id = l.Id,
                            Name = l.Name,
                            Url = l.Url,
                            SectionId = l.SectionId,
                            Order = l.Order
                        } )
                        .OrderBy( xx => xx.Order )
                        .ThenBy( xx => xx.Name )
                        .ToList();

                    return result;
                }
            } ).ToList();

            // The number of links for a section could be empty after remove ones that we aren't authorized to view (like other people's links).
            // If so, don't include those sections
            personalLinksDataList = personalLinksDataList.Where( a => a.PersonalLinks.Any() ).ToList();

            var personalLinksStorageData = new PersonalLinksData( personalLinksDataList, currentPerson );

            return personalLinksStorageData;
        }

        /// <summary>
        /// Class PersonalLinksData.
        /// </summary>
        public class PersonalLinksData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonalLinksData"/> class.
            /// </summary>
            public PersonalLinksData()
            {
                //
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PersonalLinksData" /> class.
            /// </summary>
            /// <param name="personLinksSectionList">The person links section list.</param>
            /// <param name="currentPerson">The current person.</param>
            public PersonalLinksData( List<PersonLinksSectionData> personLinksSectionList, Person currentPerson ) :
                this()
            {
                PersonLinksSectionList = personLinksSectionList.ToArray();
                ModificationHash = PersonalLinkService.GetPersonalLinksModificationHash( currentPerson );
                LastNonSharedLinksModifiedDateTime = PersonalLinksHelper.GetPersonalLinksLastModifiedDateTime( currentPerson ) ?? RockDateTime.Now;
            }

            /// <summary>
            /// Gets or sets the person links section list.
            /// </summary>
            /// <value>The person links section list.</value>
            public PersonLinksSectionData[] PersonLinksSectionList { get; private set; }

            /// <summary>
            /// Gets or sets the last shared links update date time ( in <see cref="RockDateTime" /> )
            /// </summary>
            /// <value>The last shared links update date time.</value>
            public DateTime LastSharedLinksUpdateRockDateTime => SharedPersonalLinkSectionCache.LastModifiedDateTime;

            /// <summary>
            /// Gets the last non shared links update date time.
            /// </summary>
            /// <value>The last non shared links update date time.</value>
            public DateTime LastNonSharedLinksModifiedDateTime { get; set; }

            /// <summary>
            /// Returns a Hash that can help determine if the LocalStorage needs to be updated
            /// </summary>
            /// <value>The person hash.</value>
            public string ModificationHash { get; private set; }
        }

        /// <summary>
        /// Class PersonLinksSectionData.
        /// </summary>
        public class PersonLinksSectionData
        {
            /// <summary>
            /// Gets or sets the section identifier.
            /// </summary>
            /// <value>The section identifier.</value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the section.
            /// </summary>
            /// <value>The name of the section.</value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is shared.
            /// </summary>
            /// <value><c>true</c> if this instance is shared; otherwise, <c>false</c>.</value>
            public bool IsShared { get; set; }

            /// <summary>
            /// Gets or sets the order.
            /// </summary>
            /// <value>The order.</value>
            public int Order { get; set; }

            /// <summary>
            /// Gets or sets the personal links.
            /// </summary>
            /// <value>The personal links.</value>
            public List<PersonalLinkData> PersonalLinks { get; set; }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
            public override string ToString()
            {
                return this.Name;
            }
        }

        /// <summary>
        /// Class PersonalLinkData.
        /// </summary>
        public class PersonalLinkData
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>The identifier.</value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            /// <value>The URL.</value>
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets the section identifier.
            /// </summary>
            /// <value>The section identifier.</value>
            public int SectionId { get; set; }

            /// <summary>
            /// Gets or sets the order.
            /// </summary>
            /// <value>The order.</value>
            public int Order { get; set; }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
            public override string ToString()
            {
                return $"{this.Name} ({this.Url})";
            }
        }       

        #endregion Helpers
    }
}
