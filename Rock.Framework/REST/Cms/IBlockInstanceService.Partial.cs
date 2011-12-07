using System.ServiceModel;

namespace Rock.REST.Cms
{
    public partial interface IBlockInstanceService
    {
		/// <summary>
		/// Moves a block instance from one zone to another and sets the new order
		/// </summary>
		[OperationContract]
        void Move( string id, Rock.DataTransferObjects.Cms.BlockInstance BlockInstance );

		/// <summary>
        /// Moves a block instance from one zone to another and sets the new order
        /// </summary>
		[OperationContract]
        void ApiMove( string id, string apiKey, Rock.DataTransferObjects.Cms.BlockInstance BlockInstance );
    }
}
