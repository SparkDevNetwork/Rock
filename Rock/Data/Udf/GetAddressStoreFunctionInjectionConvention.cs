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
