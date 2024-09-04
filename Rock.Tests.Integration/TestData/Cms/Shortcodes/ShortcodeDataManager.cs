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
using System.Linq;
using DotLiquid;
using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Model;
using Rock.Tests.Integration.TestData;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Shortcode
{
    /// <summary>
    /// Provides actions to manage Shortcode data.
    /// </summary>
    public class ShortcodeDataManager
    {
        private static Lazy<ShortcodeDataManager> _dataManager = new Lazy<ShortcodeDataManager>();
        public static ShortcodeDataManager Instance => _dataManager.Value;

        public bool DeleteDynamicShortcode( string shortcodeIdentifier, RockContext rockContext )
        {
            var shortcodeService = new LavaShortcodeService( rockContext );
            var shortcode = shortcodeService.Get( shortcodeIdentifier );

            if ( shortcode == null )
            {
                return false;
            }

            return shortcodeService.Delete( shortcode );
        }

        /// <summary>
        /// Creates a new instance with the required fields.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="name"></param>
        /// <param name="tagName"></param>
        /// <param name="tagType"></param>
        /// <param name="markup"></param>
        /// <returns></returns>
        public LavaShortcode NewDynamicShortcode( Guid guid, string name, string tagName, TagType tagType, string markup )
        {
            var shortcode = new LavaShortcode();

            shortcode.Guid = guid;
            shortcode.Name = name;
            shortcode.TagName = tagName;
            shortcode.TagType = tagType;
            shortcode.Markup = markup;

            return shortcode;
        }

        /// <summary>
        /// Save a new instance to the data store.
        /// </summary>
        /// <param name="newShortcode"></param>
        /// <returns></returns>
        public void SaveDynamicShortcode( LavaShortcode newShortcode, RockContext rockContext, CreateExistingItemStrategySpecifier existingItemStrategy = CreateExistingItemStrategySpecifier.Replace )
        {
            string oldTagName = null;

            rockContext.WrapTransaction( () =>
            {
                var shortcodeService = new LavaShortcodeService( rockContext );
                if ( newShortcode.Guid != Guid.Empty )
                {
                    var existingShortcode = shortcodeService.Get( newShortcode.Guid );
                    if ( existingShortcode != null )
                    {
                        oldTagName = existingShortcode.TagName;

                        if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                        {
                            throw new Exception( "Item exists." );
                        }
                        else if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                        {
                            var isDeleted = DeleteDynamicShortcode( existingShortcode.Guid.ToString(), rockContext );

                            if ( !isDeleted )
                            {
                                throw new Exception( "Could not replace existing item." );
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }

                shortcodeService.Add( newShortcode );

                rockContext.SaveChanges();
            } );
        }

        public void RegisterDynamicShortcodeForLavaEngine( ILavaEngine engine, LavaShortcode shortcode )
        {
            // Register the new shortcode definition.
            if ( engine.GetType() == typeof( RockLiquidEngine ) )
            {
                // Register the new shortcode definition. Note that RockLiquid shortcode tags are case-sensitive.
                if ( shortcode.TagType == TagType.Block )
                {
                    Template.RegisterShortcode<Rock.Lava.Shortcodes.DynamicShortcodeBlock>( shortcode.TagName );
                }
                else
                {
                    Template.RegisterShortcode<Rock.Lava.Shortcodes.DynamicShortcodeInline>( shortcode.TagName );
                }

#pragma warning disable CS0618 // Type or member is obsolete
                // This obsolete code can be deleted when support for the DotLiquid Lava implementation is removed.
                LavaTemplateCache.Clear();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else if ( engine.GetType() == typeof( FluidEngine ) )
            {
                // Register the new shortcode definition.
                engine.RegisterShortcode( shortcode.TagName, ( shortcodeName ) => WebsiteLavaShortcodeProvider.GetShortcodeDefinition( shortcodeName ) );
            }
            else
            {
                throw new Exception( $"Invalid Lava Engine Type. [Type={engine.EngineName}]" );
            }

            LavaService.ClearTemplateCache();
            LavaShortcodeCache.Clear();
        }
    }

    #region Extension Methods

    public static partial class ShortcodeModelExtensions
    {
        /// <summary>
        /// Set the Enabled Lava Commands for the shortcode.
        /// </summary>
        /// <param name="shortcode"></param>
        /// <param name="enabledCommandList"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static LavaShortcode WithEnabledCommands( this LavaShortcode shortcode, string enabledCommandList, string delimiter = "," )
        {
            var enabledCommands = enabledCommandList.SplitDelimitedValues( delimiter );
            shortcode.EnabledLavaCommands = enabledCommands.ToList().AsDelimited( "," );
            return shortcode;
        }
    }

    #endregion
}
