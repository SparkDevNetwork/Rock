using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.SystemGuid;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides functionality to manage prayer.
    /// </summary>
    [AgentSkillGuid( "0EF2BBFD-52D9-441B-9BE5-F4C5D2B42ED0" )]
    [EntityTypeGuid( "6033D65E-C782-45BA-9A74-23F9B9353A27" )]
    [Description( "This skill provides functionality to manage prayer." )]
    internal sealed partial class PrayerSkill : AgentSkillComponent
    {
        #region Fields

        private readonly ILogger<PrayerSkill> _logger;
        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PrayerSkill"/> class.
        /// </summary>
        /// <param name="rockContextFactory">Factory to create rock contexts.</param>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public PrayerSkill( IRockContextFactory rockContextFactory, ILogger<PrayerSkill> logger )
        {
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Configuration

        /// <summary>
        /// The configuration keys that are used to configure this skill.
        /// </summary>
        private static class ConfigurationKey
        {
            /// <summary>
            /// The parent category to use by default when listing or adding prayer requests.
            /// </summary>
            public const string ParentCategory = "parentCategory";
        }

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/AI/Skills/prayerSkill.obs" ),
                Options = new Dictionary<string, string>
                {
                    ["prayerCategories"] = GetPrayerCategoryOptions( rockContext )?.ToCamelCaseJson( false, false )
                },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfiguration( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var publicConfiguration = new Dictionary<string, string>();

            if ( privateConfiguration.TryGetValue( ConfigurationKey.ParentCategory, out var prayerCategoriesString ) )
            {
                publicConfiguration[ConfigurationKey.ParentCategory] = prayerCategoriesString;
            }

            return publicConfiguration;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfiguration( Dictionary<string, string> publicConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var privateConfiguration = new Dictionary<string, string>();

            if ( publicConfiguration.TryGetValue( ConfigurationKey.ParentCategory, out var groupTypesString ) )
            {
                privateConfiguration[ConfigurationKey.ParentCategory] = groupTypesString;
            }

            return privateConfiguration;
        }

        #endregion

        #region Shared Helpers

        /// <summary>
        /// Gets the queryable version of the prayer categories.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private IQueryable<Rock.Model.Category> GetPrayerCategoriesQueryable( RockContext rockContext )
        {
            var prayerRequestEntityType = EntityTypeCache.Get<PrayerRequest>( false );

            if ( prayerRequestEntityType == null )
            {
                return null;
            }

            var categoryService = new CategoryService( rockContext );
            return categoryService.GetByEntityTypeId( prayerRequestEntityType.Id );
        }

        /// <summary>
        /// Gets list item bags representing the prayer categories.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private List<ListItemBag> GetPrayerCategoryOptions( RockContext rockContext )
        {
            return GetPrayerCategoriesQueryable( rockContext )?.ToListItemBagList();
        }

        #endregion
    }
}
