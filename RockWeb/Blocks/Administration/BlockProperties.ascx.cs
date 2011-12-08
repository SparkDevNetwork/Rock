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
    public partial class BlockProperties : Rock.Web.UI.Block
    {
        private Rock.Web.Cache.BlockInstance _blockInstance = null;
        private string _zoneName = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                int blockInstanceId = Convert.ToInt32( PageParameter( "BlockInstance" ) );
                _blockInstance = Rock.Web.Cache.BlockInstance.Read( blockInstanceId );

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
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                Rock.CMS.BlockInstanceService blockInstanceService = new Rock.CMS.BlockInstanceService();
                Rock.CMS.BlockInstance blockInstance = blockInstanceService.Get( _blockInstance.Id );

                Rock.Attribute.Helper.LoadAttributes( blockInstance );

                blockInstance.Name = tbBlockName.Text;
                blockInstance.OutputCacheDuration = Int32.Parse( tbCacheDuration.Text );
                blockInstanceService.Save( blockInstance, CurrentPersonId );

                Rock.Attribute.Helper.GetEditValues( olProperties, _blockInstance );
                _blockInstance.SaveAttributeValues( CurrentPersonId );

                Rock.Web.Cache.BlockInstance.Flush( _blockInstance.Id );
            }

            phClose.Controls.AddAt(0, new LiteralControl( @"
    <script type='text/javascript'>
        window.parent.$('#modalDiv').dialog('close');
    </script>
" ));
        }
    }
}