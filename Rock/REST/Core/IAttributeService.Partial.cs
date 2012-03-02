//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ServiceModel;

namespace Rock.REST.Core
{
    public partial interface IAttributeService
    {
		/// <summary>
        /// Flushes the specified attribute from cache.
        /// </summary>
		[OperationContract]
        void Flush( string id );

		/// <summary>
        /// Flushes the specified attribute from cache.
        /// </summary>
		[OperationContract]
        void ApiFlush( string id, string apiKey );

        /// <summary>
        /// Flushes the global attributes from cache.
        /// </summary>
        [OperationContract]
        void FlushGlobal( );

        /// <summary>
        /// Flushes the global attributes from cache.
        /// </summary>
        [OperationContract]
        void ApiFlushGlobal( string apiKey );
    }
}
