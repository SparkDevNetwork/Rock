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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestData.AI
{
    /// <summary>
    /// Provides actions to manage data related to AI features.
    /// </summary>
    public class AIDataManager
    {
        private static Lazy<AIDataManager> _dataManager = new Lazy<AIDataManager>();
        public static AIDataManager Instance => _dataManager.Value;

        public bool DeleteAiProvider( string identifier )
        {
            using ( var context = new RockContext() )
            {
                var result = DeleteAiProvider( identifier, context );

                context.SaveChanges();

                return result;
            }
        }

        private bool DeleteAiProvider( string identifier, RockContext context )
        {
            var service = new AIProviderService( context );
            var entity = service.Get( identifier );

            if ( entity == null )
            {
                return false;
            }

            return service.Delete( entity );
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
        public AIProvider NewAIProvider( Guid guid, string name, int aiProviderEntityTypeId )
        {
            var newProvider = new AIProvider();

            newProvider.Name = name;
            //newProvider.Description = "Test Provider for AI Features.";
            newProvider.Guid = guid;
            newProvider.IsActive = true;
            newProvider.Order = 1;

            // Set the default AI Provider component.
            //var openAIProviderEntityTypeId = EntityTypeCache.GetId( _openAiProviderEntityTypeGuid );

            //Assert.That.IsNotNull( openAIProviderEntityTypeId, "Open AI Provider not found" );

            newProvider.ProviderComponentEntityTypeId = aiProviderEntityTypeId;

            return newProvider;
        }

        /// <summary>
        /// Save a new instance to the data store.
        /// </summary>
        /// <param name="newProvider"></param>
        /// <returns></returns>
        public void SaveAiProvider( AIProvider newProvider, CreateExistingItemStrategySpecifier existingItemStrategy = CreateExistingItemStrategySpecifier.Replace )
        {
            var rockContext = new RockContext();

            rockContext.WrapTransaction( () =>
            {
                var service = new AIProviderService( rockContext );
                if ( newProvider.Guid != Guid.Empty )
                {
                    var existingProvider = service.Get( newProvider.Guid );
                    if ( existingProvider != null )
                    {
                        if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Fail )
                        {
                            throw new Exception( "Item exists." );
                        }
                        else if ( existingItemStrategy == CreateExistingItemStrategySpecifier.Replace )
                        {
                            var isDeleted = DeleteAiProvider( existingProvider.Guid.ToString(), rockContext );

                            if ( !isDeleted )
                            {
                                throw new Exception( "Could not replace existing item." );
                            }
                        }
                    }
                }

                service.Add( newProvider );

                rockContext.SaveChanges();
            } );
        }

        private const string _openAiDefaultProviderGuid = "2AA26B14-94CB-4A30-9E97-C7250BA464BB";
        private const string _openAiCompatibleProviderGuid = "2A15B464-A07C-404C-9C51-6970E46C01FE";
        private const string _openAiProviderEntityTypeGuid = "8D3F25B1-4891-31AA-4FA6-365F5C808563";

        public void AddSampleData()
        {
            // Set the default AI Provider component.
            var openAIProviderEntityTypeId = EntityTypeCache.GetId( _openAiProviderEntityTypeGuid );

            Assert.That.IsNotNull( openAIProviderEntityTypeId, "Open AI Provider not found" );

            // Add Default Provider.
            var newProvider = AIDataManager.Instance.NewAIProvider( _openAiDefaultProviderGuid.AsGuid(),
                "Test ({newProviderGuid})",
                openAIProviderEntityTypeId.Value );

            // Add OpenAICompatible Provider.
            var compatibleProvider = AIDataManager.Instance.NewAIProvider( _openAiCompatibleProviderGuid.AsGuid(),
                "OpenAI-Compatible Provider",
                openAIProviderEntityTypeId.Value );

            AIDataManager.Instance.SaveAiProvider( compatibleProvider, CreateExistingItemStrategySpecifier.Replace );

            // Add 
        }
    //}




//
//      public void RegisterDynamicShortcodeForLavaEngine( ILavaEngine engine, LavaShortcode shortcode )
//        {
//            // Register the new shortcode definition.
//            if ( engine.GetType() == typeof( RockLiquidEngine ) )
//            {
//                // Register the new shortcode definition. Note that RockLiquid shortcode tags are case-sensitive.
//                if ( shortcode.TagType == TagType.Block )
//                {
//                    Template.RegisterShortcode<Rock.Lava.Shortcodes.DynamicShortcodeBlock>( shortcode.TagName );
//                }
//                else
//                {
//                    Template.RegisterShortcode<Rock.Lava.Shortcodes.DynamicShortcodeInline>( shortcode.TagName );
//                }

//#pragma warning disable CS0618 // Type or member is obsolete
//                // This obsolete code can be deleted when support for the DotLiquid Lava implementation is removed.
//                LavaTemplateCache.Clear();
//#pragma warning restore CS0618 // Type or member is obsolete
//            }
//            else if ( engine.GetType() == typeof( FluidEngine ) )
//            {
//                // Register the new shortcode definition.
//                engine.RegisterShortcode( shortcode.TagName, ( shortcodeName ) => WebsiteLavaShortcodeProvider.GetShortcodeDefinition( shortcodeName ) );
//            }
//            else
//            {
//                throw new Exception( $"Invalid Lava Engine Type. [Type={engine.EngineName}]" );
//            }

//            LavaService.ClearTemplateCache();
//            LavaShortcodeCache.Clear();
//        }
    }

    #region Extension Methods

    public static partial class AIProviderModelExtensions
    {
        /// <summary>
        /// Set the Enabled Lava Commands for the shortcode.
        /// </summary>
        /// <param name="shortcode"></param>
        /// <param name="enabledCommandList"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        //public static AIProvider WithEnabledCommands( this AIProvider shortcode, string enabledCommandList, string delimiter = "," )
        //{
        //    var enabledCommands = enabledCommandList.SplitDelimitedValues( delimiter );
        //    shortcode.EnabledLavaCommands = enabledCommands.ToList().AsDelimited( "," );
        //    return shortcode;
        //}
    }

    #endregion
}
