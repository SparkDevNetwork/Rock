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
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Rock.Data
{
    /// <summary>
    /// pattern from https://github.com/divega/UdfCodeFirstSample
    /// </summary>
    public class GetSpousePersonIdFromPersonIdStoreFunctionInjectionConvention : IStoreModelConvention<EdmModel>
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

            var returnValue = FunctionParameter.Create(
                "result",
                model.GetStorePrimitiveType( PrimitiveTypeKind.Int32 ),
                ParameterMode.ReturnValue );

            var function = item.CreateAndAddFunction(
                "ufnCrm_GetSpousePersonIdFromPersonId",
                new[] { personIdParameter },
                new[] { returnValue } );
        }
    }
}
