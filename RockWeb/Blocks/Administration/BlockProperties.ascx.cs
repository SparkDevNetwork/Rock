using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Administration
{
    public partial class BlockProperties : Rock.Cms.CmsBlock
    {
        private Rock.Cms.Cached.BlockInstance _blockInstance = null;
        private string _zoneName = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                int blockInstanceId = Convert.ToInt32( PageParameter( "BlockInstance" ) );
                _blockInstance = Rock.Cms.Cached.BlockInstance.Read( blockInstanceId );

                if ( _blockInstance.Authorized( "Configure", CurrentUser ) )
                {
                    tbBlockName.Text = _blockInstance.Name;
                    tbCacheDuration.Text = _blockInstance.OutputCacheDuration.ToString();

                    foreach ( HtmlGenericControl li in Rock.Attribute.Helper.GetEditControls( _blockInstance, !Page.IsPostBack ) )
                        olProperties.Controls.Add( li );
                }
                else
                {
                    DisplayError( "You are not authorized to edit this block" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                Rock.Services.Cms.BlockInstanceService blockInstanceService = new Rock.Services.Cms.BlockInstanceService();
                Rock.Models.Cms.BlockInstance blockInstance = blockInstanceService.Get( _blockInstance.Id );

                blockInstance.Name = tbBlockName.Text;
                blockInstance.OutputCacheDuration = Int32.Parse( tbCacheDuration.Text );
                blockInstanceService.Save( blockInstance, CurrentPersonId );

                Rock.Attribute.Helper.GetEditValues( olProperties, blockInstance );
                _blockInstance.SaveAttributeValues( CurrentPersonId );

                Rock.Cms.Cached.BlockInstance.Flush( _blockInstance.Id );
            }

            phClose.Controls.AddAt(0, new LiteralControl( @"
    <script type='text/javascript'>
        window.parent.$('#modalDiv').dialog('close');
    </script>
" ));
        }
    }
}