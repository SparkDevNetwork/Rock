using System.Web.Http.Controllers;
using Westwind.Web.WebApi;

namespace System.Web.Http
{
    /// <summary>
    /// An attribute that captures the entire content body and stores it
    /// into the parameter of type string or byte[].
    /// </summary>
    /// <remarks>
    /// The parameter marked up with this attribute should be the only parameter as it reads the
    /// entire request body and assigns it to that parameter.    
    /// </remarks>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true )]
    public sealed class NakedBodyAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding( HttpParameterDescriptor parameter )
        {
            if ( parameter == null )
                throw new ArgumentException( "Invalid parameter" );

            return new NakedBodyParameterBinding( parameter );
        }
    }
}