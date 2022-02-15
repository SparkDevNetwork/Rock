using System;
using System.Linq;
using System.Reflection;

namespace Rock.Rest.Utility
{
    /// <summary>
    /// An interface that can be implemented on api models that want to use the <see cref="ApiModelMapper.CreateMapped{TTarget}(IMappedApiModel, string[])"></see> method.
    /// That allows properties to be copied from the incoming api model to a target instance class.
    /// </summary>
    internal interface IMappedApiModel { }

    /// <summary>
    /// Class ApiModelMapper.
    /// </summary>
    internal static class ApiModelMapper
    {

        /// <summary>
        /// Copies properties from the source that implements <see cref="IMappedApiModel"/> to the target instance class.
        /// </summary>
        /// <typeparam name="TTarget">The instanced type of the target.</typeparam>
        /// <param name="sourceType">The source that is an <see cref="IMappedApiModel"/> implemented type.</param>
        /// <param name="ignoreProperties">Property names to ignore. Note: Must match case.</param>
        /// <returns>TTarget.</returns>
        internal static TTarget CreateMapped<TTarget>( this IMappedApiModel sourceType, params string[] ignoreProperties )
            where TTarget : new()
        {
            var targetType = new TTarget();

            const BindingFlags flags =
               BindingFlags.Public |
               BindingFlags.IgnoreCase |
               BindingFlags.Instance;

            // Get properties from the source class the implements IMappedApiModel which is used a constraint for this extension method.
            // Also allow you to exclude properties from being copied that may exist in source and destination
            var sourceProperties = sourceType.GetType().GetProperties( flags )
                                     .Where( v => v.CanRead &
                                     (!ignoreProperties?.Contains( v.Name )).GetValueOrDefault(true)
                                     )
                                     .Select( v => new
                                     {
                                         Name = v.Name,
                                         Type = Nullable.GetUnderlyingType( v.PropertyType ) ?? v.PropertyType
                                     } ).ToList();

            // Get properties from the target class.

            var targetProperties = targetType.GetType().GetProperties( flags )
                                     .Where( v => v.CanRead )
                                     .Select( v => new
                                     {
                                         Name = v.Name,
                                         Type = Nullable.GetUnderlyingType( v.PropertyType ) ?? v.PropertyType
                                     } ).ToList();

            // Find common properties between the source and destination classes
            var inCommonProperties = sourceProperties.Intersect( targetProperties ).ToList();

            // Assign the property values to the target for properties that can be written to
            foreach ( var property in inCommonProperties )
            {
                var sourcePropertyValue = sourceType.GetType().GetProperty( property.Name ).GetValue( sourceType, null );
                PropertyInfo targetProperty = targetType.GetType().GetProperty( property.Name );

                if ( targetProperty.CanWrite )
                {
                    targetProperty.SetValue( targetType, sourcePropertyValue, null );
                }
            }
            return targetType;
        }
    }
}
