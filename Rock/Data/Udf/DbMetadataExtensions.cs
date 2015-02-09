// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Rock.Data
{
    /// <summary>
    /// from https://github.com/divega/UdfCodeFirstSample
    /// </summary>
    public static class DbMetadataExtensions
    {
        /// <summary>
        /// Creates the and add function.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="name">The name.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="returnValues">The return values.</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public static EdmFunction CreateAndAddFunction( this EdmModel item, string name,
            IList<FunctionParameter> parameters, IList<FunctionParameter> returnValues, string body = null )
        {
            var payload = new EdmFunctionPayload
            {
                StoreFunctionName = name,
                Parameters = parameters,
                ReturnParameters = returnValues,
                Schema = item.GetDefaultSchema()
            };

            EdmFunction function = EdmFunction.Create( name, item.GetDefaultNamespace(), item.DataSpace, payload, null );

            item.AddItem( function );

            return function;
        }

        /// <summary>
        ///     Translate a conceptual primitive type to an adequate store specific primitive type according to the
        ///     provider configuration of the model.
        /// </summary>
        /// <param name="model">A DbModel instance with provider information</param>
        /// <param name="typeKind">A PrimitiveTypeKind instance representing the conceptual primitive type to be translated</param>
        /// <returns>An EdmType instance representing the store primitive type</returns>
        public static EdmType GetStorePrimitiveType( this DbModel model, PrimitiveTypeKind typeKind )
        {
            return model
                .ProviderManifest
                .GetStoreType(
                    TypeUsage.CreateDefaultTypeUsage(
                        PrimitiveType.GetEdmPrimitiveType( typeKind ) ) ).EdmType;
        }

        /// <summary>
        ///     Obtain the namespace name from existing model defined types.
        /// </summary>
        /// <param name="layerModel">An EdmModel instance representing conceptual or store model.</param>
        /// <returns>A string contining the name of the namespace.</returns>
        /// <remarks>
        ///     Only one namespace is allowed. Throws if there are multiple namespaces or if there aren't any types defined in the
        ///     model.
        /// </remarks>
        public static string GetDefaultNamespace( this EdmModel layerModel )
        {
            return layerModel
                .GlobalItems
                .OfType<EdmType>()
                .Select( t => t.NamespaceName )
                .Distinct()
                .Single();
        }

        /// <summary>
        ///     Obtains the default schema from existing entity sets in the model.
        /// </summary>
        /// <param name="layerModel">An instance of EdmModel representing either the conceptual or the store model.</param>
        /// <returns>A string containing the name of the schema.</returns>
        /// <remarks>
        ///     Throws if more than one schema is used or if the model contains no entity sets.
        /// </remarks>
        public static string GetDefaultSchema( this EdmModel layerModel )
        {
            return layerModel
                .Container
                .EntitySets
                .Select( s => s.Schema )
                .Distinct()
                .SingleOrDefault();
        }
    }
}
