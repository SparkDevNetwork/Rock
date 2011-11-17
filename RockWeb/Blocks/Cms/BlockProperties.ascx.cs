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

namespace RockWeb.Blocks.Cms
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

                    foreach ( Rock.Cms.Cached.Attribute attribute in _blockInstance.Attributes )
                    {
                        HtmlGenericControl li = new HtmlGenericControl( "li" );
                        li.ID = string.Format( "attribute-{0}", attribute.Id );
                        li.ClientIDMode = ClientIDMode.AutoID;
                        olProperties.Controls.Add( li );

                        Label lbl = new Label();
                        lbl.ClientIDMode = ClientIDMode.AutoID;
                        lbl.Text = attribute.Name;
                        lbl.AssociatedControlID = string.Format( "attribute-field-{0}", attribute.Id );
                        li.Controls.Add( lbl );

                        Control attributeControl = attribute.CreateControl( _blockInstance.AttributeValues[attribute.Key].Value, !Page.IsPostBack );
                        attributeControl.ID = string.Format( "attribute-field-{0}", attribute.Id );
                        attributeControl.ClientIDMode = ClientIDMode.AutoID;
                        li.Controls.Add( attributeControl );

                        if ( !string.IsNullOrEmpty( attribute.Description ) )
                        {
                            HtmlAnchor a = new HtmlAnchor();
                            a.ClientIDMode = ClientIDMode.AutoID;
                            a.Attributes.Add( "class", "attribute-description tooltip" );
                            a.InnerHtml = "<span>" + attribute.Description + "</span>";

                            li.Controls.Add( a );
                        }
                    }
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

                foreach ( Rock.Cms.Cached.Attribute attribute in _blockInstance.Attributes )
                {
                    Control control = olProperties.FindControl( string.Format( "attribute-field-{0}", attribute.Id.ToString() ) );
                    if ( control != null )
                        _blockInstance.AttributeValues[attribute.Key] = new KeyValuePair<string, string>( attribute.Name, attribute.FieldType.Field.ReadValue( control ) );
                }

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