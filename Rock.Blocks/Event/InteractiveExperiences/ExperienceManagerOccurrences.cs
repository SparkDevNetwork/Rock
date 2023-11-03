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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.ExperienceManagerOccurrences;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event.InteractiveExperiences
{
    /// <summary>
    /// Displays a list of interactive experience occurrences for the individual to pick from.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Experience Manager Occurrences" )]
    [Category( "Event > Interactive Experiences" )]
    [Description( "Displays a list of interactive experience occurrences for the individual to pick from." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Experience Manager Page",
        IsRequired = true,
        Key = AttributeKey.ExperienceManagerPage,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "08c31c15-7328-4759-b530-49c9d342cdb7" )]
    [Rock.SystemGuid.BlockTypeGuid( "b8be65ec-04cc-4423-944e-b6b30f6eb38c" )]
    public class ExperienceManagerOccurrences : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ExperienceManagerPage = "ExperienceManagerPage";
        }

        private static class PageParameterKey
        {
            public const string InteractiveExperienceId = "InteractiveExperienceId";

            public const string InteractiveExperienceOccurrenceId = "InteractiveExperienceOccurrenceId";
        }

        private static class NavigationUrlKey
        {
            public const string ExperienceManagerPage = "ExperienceManagerPage";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new ExperienceManagerOccurrencesInitializationBox();
                var experience = GetInteractiveExperience( rockContext, PageParameterKey.InteractiveExperienceId );

                if ( experience != null && !experience.IsActive )
                {
                    box.ErrorMessage = "This Interactive Experience is not currently active.";
                    return box;
                }

                var occurrences = GetOccurrenceItemBags( experience, rockContext );

                if ( !occurrences.Any() )
                {
                    box.ErrorMessage = "There are no experiences happening right now.";
                    return box;
                }

                box.ExperienceName = experience?.Name ?? "Experience Occurrences";
                box.SecurityGrantToken = GetSecurityGrantToken();
                box.NavigationUrls = GetBoxNavigationUrls();
                box.Occurrences = occurrences;

                return box;
            }
        }

        /// <summary>
        /// Gets the interactive experience entity from page parameters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="InteractiveExperienceCache"/> to be viewed or edited on the page.</returns>
        private InteractiveExperienceCache GetInteractiveExperience( RockContext rockContext, string entityIdKey )
        {
            var entityId = RequestContext.GetPageParameter( entityIdKey );

            return InteractiveExperienceCache.Get( entityId, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the occurrence item bags for all the active occurrences.
        /// </summary>
        /// <param name="experience">The experience to use when enumerating occurrences.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> objects that represent the occurrences.</returns>
        private static List<ListItemBag> GetOccurrenceItemBags( InteractiveExperienceCache experience, RockContext rockContext )
        {
            var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );
            var qry = occurrenceService.Queryable();

            if ( experience != null )
            {
                var occurrenceIds = experience.GetOrCreateAllCurrentOccurrenceIds();

                qry = qry.Where( ieo => occurrenceIds.Contains( ieo.Id ) );
            }
            else
            {
                qry = occurrenceService.GetActiveOccurrences();
            }

            return qry
                .Select( ieo => new
                {
                    ieo.Id,
                    ieo.InteractiveExperienceSchedule.InteractiveExperience.Name,
                    ieo.CampusId,
                    ieo.OccurrenceDateTime
                } )
                .ToList()
                .Select( ieo => new ListItemBag
                {
                    Value = IdHasher.Instance.GetHash( ieo.Id ),
                    Text = GetOccurrenceTitle( ieo.OccurrenceDateTime, ieo.CampusId, experience == null ? ieo.Name : null )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the occurrence title to use for the specified date, time and campus.
        /// </summary>
        /// <param name="occurrenceDateTime">The occurrence date time.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="experienceName">If provided the campus name will be prefixed to the title.</param>
        /// <returns>The string that should be used when displaying the occurrence.</returns>
        private static string GetOccurrenceTitle( DateTime occurrenceDateTime, int? campusId, string experienceName )
        {
            string campusName;

            if ( campusId.HasValue )
            {
                campusName = CampusCache.Get( campusId.Value )?.Name ?? "Unknown Campus";
            }
            else
            {
                campusName = "All Campuses";
            }

            if ( experienceName.IsNotNullOrWhiteSpace() )
            {
                return $"{experienceName} at {occurrenceDateTime.ToShortDateTimeString()} on {campusName}";
            }
            else
            {
                return $"{occurrenceDateTime.ToShortDateTimeString()} on {campusName}";
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
                [NavigationUrlKey.ExperienceManagerPage] = this.GetLinkedPageUrl( AttributeKey.ExperienceManagerPage, new Dictionary<string, string>
                {
                    [PageParameterKey.InteractiveExperienceOccurrenceId] = "((Id))"
                } )
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

        #endregion
    }
}
