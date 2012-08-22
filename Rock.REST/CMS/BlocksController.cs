using Rock.CMS;

namespace Rock.Rest.CMS
{
    public class BlocksController : Rock.Rest.ApiController<Block, BlockDTO>
    {
        public BlocksController() : base( new Rock.CMS.BlockService() ) { }
    }
}
