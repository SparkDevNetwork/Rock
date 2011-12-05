using System.ServiceModel;

using Rock.Models.Cms;

namespace Rock.Api.Cms
{
    public partial interface IBlockInstanceService
    {
        /// <summary>
        /// Moves a block from one zone to another zone.
        /// </summary>
        /// <remarks>
        /// The zone will be inserted as the last block in the new zone relative to the
        /// other zones with the same parent (layout or page).
        /// </remarks>
        /// <param name="id">The id of the block instance.</param>
        /// <param name="blockInstance">The block instance.</param>
        /// <returns></returns>
        [OperationContract]
        void Move( string id, Rock.DataTransferObjects.Cms.BlockInstance blockInstance );
    }
}
