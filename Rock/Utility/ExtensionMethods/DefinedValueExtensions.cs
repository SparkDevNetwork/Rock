
namespace Rock
{
    /// <summary>
    /// Defined Value Extensions
    /// </summary>
    public static class DefinedValueExtensions
    {
        #region Int Extensions

        /// <summary>
        /// Gets the Defined Value name associated with this id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static string DefinedValue( this int? id )
        {
            if ( !id.HasValue )
                return string.Empty;

            var definedValue = Rock.Web.Cache.DefinedValueCache.Read( id.Value );
            if ( definedValue != null )
                return definedValue.Value;
            else
                return string.Empty;
        }

        #endregion Int Extensions
    }
}
