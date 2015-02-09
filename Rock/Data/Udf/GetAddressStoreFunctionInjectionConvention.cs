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
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Rock.Data
{
    /// <summary>
    /// pattern from https://github.com/divega/UdfCodeFirstSample
    /// </summary>
    public class GetAddressStoreFunctionInjectionConvention : IStoreModelConvention<EdmModel>
    {
        /// <summary>
        /// Applies the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="model">The model.</param>
        public void Apply( EdmModel item, DbModel model )
        {
            var personIdParameter = FunctionParameter.Create(
                "PersonId",
                model.GetStorePrimitiveType( PrimitiveTypeKind.Int32 ),
                ParameterMode.In );

            var addressTypeParameter = FunctionParameter.Create(
                "AddressType",
                model.GetStorePrimitiveType( PrimitiveTypeKind.String ),
                ParameterMode.In );

            var addressComponentParameter = FunctionParameter.Create(
                "AddressComponent",
                model.GetStorePrimitiveType( PrimitiveTypeKind.String ),
                ParameterMode.In );

            var returnValue = FunctionParameter.Create(
                "result",
                model.GetStorePrimitiveType( PrimitiveTypeKind.String ),
                ParameterMode.ReturnValue );

            var function = item.CreateAndAddFunction(
                "ufnCrm_GetAddress",
                new[] { personIdParameter, addressTypeParameter, addressComponentParameter },
                new[] { returnValue } );
        }
    }
}
