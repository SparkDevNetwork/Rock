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
using System.Linq.Dynamic.Core;

using Microsoft.Extensions.Logging;

using Rock.Data;
using Rock.Logging;
using Rock.ViewModels.Core.Automation.Triggers;

namespace Rock.Core.Automation.Triggers
{
    /// <summary>
    /// Represents the criteria defined for a single Entity Change automation
    /// trigger. This is cached to get the best performance we can and is
    /// replaced when the configuration for the trigger has changed.
    /// </summary>
    internal class EntityChangeCriteria
    {
        #region Fields

        /// <summary>
        /// The filter mode. 0 = Simple, 1 = Advanced
        /// </summary>
        private readonly int _filterMode;

        /// <summary>
        /// If the filter mode is simple, this indicates if all rules
        /// must match or if only one needs to match.
        /// </summary>
        private readonly bool _areAllSimpleRulesRequired;

        /// <summary>
        /// The rules that are used to filter the entity changes when in simple
        /// mode.
        /// </summary>
        private readonly List<EntityChangeCriteriaRule> _simpleRules;

        /// <summary>
        /// The delegate that will handle filtering an entity in advanced mode.
        /// </summary>
        private readonly Delegate _advancedDelegate;

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new instance of the <see cref="EntityChangeCriteria"/> class.
        /// </summary>
        /// <param name="type">The type of entity that will be filtered by this instance.</param>
        /// <param name="filterMode">The filter mode to use when filtering items.</param>
        /// <param name="simpleCriteria">If filter mode is simple, this should contain the bag representing the simple criteria.</param>
        /// <param name="advancedCriteria">If filter mode is advanced, this should contain the string representing the dynamic LINQ statement.</param>
        public EntityChangeCriteria( Type type, int filterMode, EntityChangeSimpleCriteriaBag simpleCriteria, string advancedCriteria )
        {
            _filterMode = filterMode;
            _areAllSimpleRulesRequired = simpleCriteria?.AreAllRulesRequired ?? false;

            if ( filterMode == 1 )
            {
                try
                {
                    var lambda = DynamicExpressionParser.ParseLambda( false, type, typeof( bool ), advancedCriteria );

                    _advancedDelegate = lambda.Compile();
                }
                catch ( Exception ex )
                {
                    var logger = RockLogger.LoggerFactory.CreateLogger<EntityChangeCriteria>();

                    logger.LogError( ex, "Error parsing advanced criteria {criteria}: {error}", advancedCriteria, ex.Message );

                    _advancedDelegate = null;
                }
            }
            else
            {
                _simpleRules = simpleCriteria?.Rules
                    .Select( r => new EntityChangeCriteriaRule( type, r ) )
                    .ToList();
            }
        }

        /// <summary>
        /// Determines if the save entry matches the criteria defined in this instance.
        /// </summary>
        /// <param name="entry">The save entry that describes the current entity save operation.</param>
        /// <returns><c>true</c> if the entry matches the criteria; otherwise <c>false</c>.</returns>
        public bool IsMatch( IEntitySaveEntry entry )
        {
            try
            {
                if ( _filterMode == 1 )
                {
                    // In this case, no delegate means a parse error so don't match.
                    if ( _advancedDelegate == null )
                    {
                        return false;
                    }

                    return ( bool ) _advancedDelegate.DynamicInvoke( entry.Entity );
                }
                else
                {
                    // No rules means there is a match.
                    if ( _simpleRules == null || _simpleRules.Count == 0 )
                    {
                        return true;
                    }

                    if ( _areAllSimpleRulesRequired)
                    {
                        return _simpleRules.All( r => r.IsMatch( entry ) );
                    }
                    else
                    {
                        return _simpleRules.Any( r => r.IsMatch( entry ) );
                    }
                }
            }
            catch ( Exception ex )
            {
                var logger = RockLogger.LoggerFactory.CreateLogger<EntityChangeCriteria>();

                logger.LogError( ex, "Error processing entity change criteria: {error}", ex.Message );

                return false;
            }
        }

        #endregion
    }
}
