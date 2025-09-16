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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Microsoft.SemanticKernel;

using Rock.AI.Agent.Classes.Common;
using Rock.Enums.AI.Agent;
using Rock.Lava;
using Rock.Net;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Support class to handle executing a native function backed by a
    /// Lava template.
    /// </summary>
    internal class LavaToolFunction
    {
        #region Fields

        /// <summary>
        /// The agent request context that this function will be executed for.
        /// </summary>
        private readonly AgentRequestContext _agentRequestContext;

        /// <summary>
        /// The current request context. This is used to provide common merge
        /// fields as well as knowing who the current person is in Lava.
        /// </summary>
        private readonly RockRequestContext _rockRequestContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="LavaToolFunction"/>.
        /// </summary>
        /// <param name="requestContext">The agent request context that this function will be executed for.</param>
        /// <param name="rockRequestContext">The current request context.</param>
        public LavaToolFunction( AgentRequestContext requestContext, RockRequestContext rockRequestContext )
        {
            _agentRequestContext = requestContext;
            _rockRequestContext = rockRequestContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the Lava template defined in the function and returns the
        /// results from resolving the template and merge fields.
        /// </summary>
        /// <param name="function">The function to be executed.</param>
        /// <param name="args">The arguments from the language model that will be passed to the Lava template.</param>
        /// <returns>The output from the Lava template.</returns>
        public RockToolResult ExecuteLava( AgentTool function, KernelArguments args )
        {
            var mergeFields = _rockRequestContext.GetCommonMergeFields();
            var proxyFunctionResponse = new Dictionary<string, object>();

            mergeFields["AgentContext"] = _agentRequestContext;
            mergeFields[$"{LavaHelper.InternalMergeFieldPrefix}ProxyFunction"] = proxyFunctionResponse;

            AddParametersToMergeFields( mergeFields, function.Parameters, args );

            // Because only administrators (or those granted access by an
            // administrator) can create or edit functions, we can safely
            // just enable all lava commands.
            try
            {
                var output = function.Prompt.ResolveMergeFields( mergeFields, "All", throwExceptionOnErrors: true ).Trim();

                if ( proxyFunctionResponse.TryGetValue( "ToolResult", out var resultObject ) && resultObject is RockToolResult toolResult )
                {
                    return toolResult;
                }
                else if ( output.IsNotNullOrWhiteSpace() )
                {
                    return RockToolResult.Success( output );
                }
                else
                {
                    return RockToolResult.NoData();
                }
            }
            catch ( LavaToolException ex ) when ( ex.ErrorResult != null )
            {
                return ex.ErrorResult;
            }
            catch ( Exception ex )
            {
                return RockToolResult.Error( $"An error occurred while executing the function: {ex.Message}" )
                    .WithInstructions( "An internal error has occurred. The error message should be displayed so the user can diagnose the problem." );
            }
        }

        /// <summary>
        /// Adds the parameters defined in the function to the Lava merge fields.
        /// </summary>
        /// <param name="mergeFields">The merge fields object to add the parameter values to.</param>
        /// <param name="parameters">The parameters that were defined on the function.</param>
        /// <param name="args">The arguments passed from the language model to the function.</param>
        internal static void AddParametersToMergeFields( Dictionary<string, object> mergeFields, List<ParameterSchema> parameters, KernelArguments args )
        {
            foreach ( var parameter in parameters )
            {
                if ( !args.ContainsKey( parameter.Name ) )
                {
                    mergeFields.Add( parameter.Name, null );

                    continue;
                }

                if ( parameter.IsCollection )
                {
                    var value = args[parameter.Name];

                    mergeFields.Add( parameter.Name, ConvertValueToCollection( value, parameter.DataType ) );
                }
                else
                {
                    var value = args[parameter.Name];

                    mergeFields.Add( parameter.Name, ConvertValueToType( value, parameter.DataType ) );
                }
            }
        }

        /// <summary>
        /// Converts the value to the specified parameter data type.
        /// </summary>
        /// <param name="value">The value from the language model to be converted.</param>
        /// <param name="dataType">The target data type to convert the value to.</param>
        /// <returns>The converted value.</returns>
        internal static object ConvertValueToType( object value, ParameterSchemaDataType dataType )
        {
            if ( dataType == ParameterSchemaDataType.String )
            {
                return value?.ToString();
            }
            else if ( dataType == ParameterSchemaDataType.Number )
            {
                if ( value == null )
                {
                    return null;
                }
                else if ( value.GetType() == typeof( int ) )
                {
                    return ( int ) value;
                }
                else if ( value.GetType() == typeof( double ) )
                {
                    return ( double ) value;
                }
                else
                {
                    return value.ToString().AsDoubleOrNull() ?? 0.0;
                }
            }
            else if ( dataType == ParameterSchemaDataType.Boolean )
            {
                if ( value == null )
                {
                    return null;
                }
                else if ( value.GetType() == typeof( bool ) )
                {
                    return ( bool ) value;
                }
                else
                {
                    return value.ToString().AsBoolean();
                }
            }
            else if ( dataType == ParameterSchemaDataType.Date || dataType == ParameterSchemaDataType.DateTime )
            {
                if ( value == null )
                {
                    return null;
                }

                if ( DateTime.TryParse( value.ToString(), out var dateTime ) )
                {
                    return dateTime;
                }
                else
                {
                    return value?.ToString();
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts the value to a collection of the specified data type.
        /// </summary>
        /// <param name="value">The value from the language model to be converted.</param>
        /// <param name="dataType">The target data type to convert each of the values to.</param>
        /// <returns>The converted values.</returns>
        internal static List<object> ConvertValueToCollection( object value, ParameterSchemaDataType dataType )
        {
            if ( value is ICollection collection )
            {
                return collection
                    .Cast<object>()
                    .Select( a => ConvertValueToType( a, dataType ) )
                    .ToList();
            }
            else if ( value is string stringValue )
            {
                // Value might be a JSON encoded string of array values.
                try
                {
                    var jsonArray = JsonSerializer.Deserialize<List<object>>( stringValue );

                    return jsonArray
                        ?.Select( a => ConvertValueToType( a, dataType ) )
                        .ToList()
                        ?? new List<object>();
                }
                catch
                {
                    // Ignore any errors. If it fails to deserialize, we'll
                    // just treat it as a list of one value.
                    return new List<object>
                    {
                        ConvertValueToType( stringValue, dataType )
                    };
                }
            }
            else
            {
                return new List<object>
                {
                    ConvertValueToType( value, dataType )
                };
            }
        }

        #endregion
    }
}
