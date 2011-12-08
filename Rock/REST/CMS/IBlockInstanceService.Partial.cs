//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ServiceModel;

namespace Rock.REST.CMS
{
    public partial interface IBlockInstanceService
    {
		/// <summary>
		/// Moves a block instance from one zone to another and sets the new order
		/// </summary>
		[OperationContract]
        void Move( string id, Rock.CMS.DTO.BlockInstance BlockInstance );

		/// <summary>
        /// Moves a block instance from one zone to another and sets the new order
        /// </summary>
		[OperationContract]
        void ApiMove( string id, string apiKey, Rock.CMS.DTO.BlockInstance BlockInstance );
    }
}
