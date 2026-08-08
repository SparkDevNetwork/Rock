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

namespace Rock.AI.Agent.Skills;

[Description( "This skill provides functionality to manage prayer." )]
[AgentSkillGuid( "0EF2BBFD-52D9-441B-9BE5-F4C5D2B42ED0" )]
[EntityTypeGuid( "6033D65E-C782-45BA-9A74-23F9B9353A27" )]
internal sealed partial class PrayerSkill : AgentSkillComponent
{
    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PrayerSkill"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public PrayerSkill( ILogger<PrayerSkill> logger )
    {
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
        var prayerRequestEntityType = EntityTypeCache.Get<PrayerRequest>( true, rockContext );
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
