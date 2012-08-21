using Rock.CMS;

namespace Rock.Rest.CMS
{
    public class PagesController : Rock.Rest.ApiController<Page, PageDTO>
    {
        public PagesController() : base( new Rock.CMS.PageService() ) { }
    }
}
