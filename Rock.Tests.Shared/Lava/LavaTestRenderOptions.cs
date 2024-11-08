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

using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Model;

namespace Rock.Tests.Shared.Lava
{
    /// <summary>
    /// A set of options for testing the rendering of a Lava template.
    /// </summary>
    public class LavaTestRenderOptions
    {
        #region Factory methods

        /// <summary>
        /// Returns a new configuration for the all available Lava engines.
        /// </summary>
        public static LavaTestRenderOptions AllEngines
        {
            get
            {
                var options = New
                    .WithFluidEngine()
                    .WithRockLiquidEngine();
                return options;
            }
        }

        /// <summary>
        /// Returns a new configuration for the Lava default engine only.
        /// The current default engine is Fluid.
        /// </summary>
        public static LavaTestRenderOptions DefaultEngine
        {
            get
            {
                var options = New
                    .WithFluidEngine();
                return options;
            }
        }

        /// <summary>
        /// Returns a new configuration simulating an active Rock Web application session:
        /// [LavaEngine] = Fluid, [CurrentPerson] = Ted Decker.
        /// </summary>
        public static LavaTestRenderOptions WebApplicationSession
        {
            get
            {
                var options = New
                    .WithFluidEngine()
                    .WithCurrentPerson( TestGuids.TestPeople.TedDecker );
                return options;
            }
        }

        /// <summary>
        /// Returns a new default configuration.
        /// </summary>
        public static LavaTestRenderOptions New
        {
            get
            {
                var options = new LavaTestRenderOptions();
                return options;
            }
        }

        #endregion

        public IDictionary<string, object> MergeFields = null;
        public string EnabledCommands = null;
        public string EnabledCommandsDelimiter = ",";

        public bool IgnoreWhiteSpace = true;
        public bool IgnoreCase = false;

        public List<string> Wildcards = new List<string>();

        public LavaTestOutputMatchTypeSpecifier OutputMatchType = LavaTestOutputMatchTypeSpecifier.Equal;

        public ExceptionHandlingStrategySpecifier? ExceptionHandlingStrategy;

        public List<Type> LavaEngineTypes = new List<Type>();
    }

    #region Extension Methods

    /// <summary>
    /// Extension methods for constructing Lava render configurations.
    /// </summary>
    public static class LavaTestRenderOptionsExtensions
    {
        /// <summary>
        /// Set the enabled commands.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="enabledCommands"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static LavaTestRenderOptions WithEnabledCommands( this LavaTestRenderOptions options, string enabledCommands, string delimiter = "," )
        {
            options.EnabledCommands = enabledCommands;
            options.EnabledCommandsDelimiter = delimiter;
            return options;
        }

        /// <summary>
        /// Set the value of a variable for this render context.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static LavaTestRenderOptions WithContextVariable( this LavaTestRenderOptions options, string variableName, object value )
        {
            if ( options.MergeFields == null )
            {
                options.MergeFields = new Dictionary<string, object>();
            }

            options.MergeFields[variableName] = value;
            return options;
        }

        /// <summary>
        /// Set the exception handling strategy for this render context.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="strategy"></param>
        /// <returns></returns>
        public static LavaTestRenderOptions WithExceptionHandling( this LavaTestRenderOptions options, ExceptionHandlingStrategySpecifier strategy )
        {
            options.ExceptionHandlingStrategy = strategy;
            return options;
        }

        /// <summary>
        /// Set the CurrentPerson for this render context.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="personIdentifier"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static LavaTestRenderOptions WithCurrentPerson( this LavaTestRenderOptions options, string personIdentifier )
        {
            Person person;
            if ( personIdentifier.IsNullOrWhiteSpace() )
            {
                person = null;
            }
            else
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );

                person = personService.Get( personIdentifier );

                if ( person == null )
                {
                    throw new Exception( "Person identifier is invalid" );
                }
            }

            return options.WithContextVariable( "CurrentPerson", person );
        }

        /// <summary>
        /// Set the Lava engines for this test context.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="engineTypes"></param>
        /// <returns></returns>
        public static LavaTestRenderOptions WithEngines( this LavaTestRenderOptions options, IEnumerable<Type> engineTypes )
        {
            foreach ( var engineType in engineTypes )
            {
                if ( !options.LavaEngineTypes.Contains( engineType ) )
                {
                    options.LavaEngineTypes.Add( engineType );
                }
            }
            return options;
        }

        /// <summary>
        /// Add the Fluid Engine to the test configuration.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static LavaTestRenderOptions WithFluidEngine( this LavaTestRenderOptions options )
        {
            return options.WithEngines( new Type[] { typeof( FluidEngine ) } );
        }

        /// <summary>
        /// Add the Rock LiquidFluid Engine to the test configuration.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static LavaTestRenderOptions WithRockLiquidEngine( this LavaTestRenderOptions options )
        {
            return options.WithEngines( new Type[] { typeof( RockLiquidEngine ) } );
        }

        /// <summary>
        /// Set the WhiteSpace ignore option.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static LavaTestRenderOptions WithIgnoreWhiteSpace( this LavaTestRenderOptions options, bool ignoreWhiteSpace = true )
        {
            options.IgnoreWhiteSpace = ignoreWhiteSpace;
            return options;
        }
    }

    #endregion
}
