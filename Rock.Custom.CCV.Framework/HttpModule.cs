using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Rock.Custom.CCV
{
    class HttpModule : Rock.Helpers.HttpModule
    {
        public override void Application_Start( HttpApplication context )
        {
            Rock.Models.Cms.Page.Adding += Page_Adding;
            Rock.Models.Cms.Page.Updating += Page_Updating;
        }

        void Page_Adding( object sender, Models.ModelUpdatingEventArgs e )
        {
            PrependCCV( e.Model );
        }

        void Page_Updating( object sender, Models.ModelUpdatingEventArgs e )
        {
            PrependCCV( e.Model );
        }

        private void PrependCCV( Rock.Models.IModel model )
        {
            var page = model as Rock.Models.Cms.Page;
            if ( page != null )
            {
                if ( !page.Name.StartsWith( "CCV:" ) )
                    page.Name = "CCV:" + page.Name;
            }
        }
    }
}
