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

        //Rock.Services.Cms.PageService pageService = new Rock.Services.Cms.PageService();
        //List<Rock.Models.Cms.Page> pages;

        protected override void OnInit( EventArgs e )
        {
            MembershipUser user = Membership.GetUser();

            int blockInstanceId = Convert.ToInt32( PageParameter( "BlockInstance" ) );
            _blockInstance = Rock.Cms.Cached.BlockInstance.Read(blockInstanceId);

            if (_blockInstance.Authorized("Configure", user))
            {
                HtmlGenericControl fieldset = new HtmlGenericControl( "fieldset" );
                fieldset.ClientIDMode = ClientIDMode.AutoID;
                phAttributes.Controls.Add( fieldset );

                HtmlGenericControl ol = new HtmlGenericControl( "ol" );
                ol.ClientIDMode = ClientIDMode.AutoID;
                fieldset.Controls.Add( ol );

                foreach (Rock.Cms.Cached.Attribute attribute in _blockInstance.Attributes)
                {
                    HtmlGenericControl li = new HtmlGenericControl( "li" );
                    li.ID = string.Format( "attribute-{0}", attribute.Id );
                    li.ClientIDMode = ClientIDMode.AutoID;
                    ol.Controls.Add( li );

                    Label lbl = new Label();
                    lbl.ClientIDMode = ClientIDMode.AutoID;
                    lbl.Text = attribute.Name;
                    lbl.AssociatedControlID = string.Format( "attribute-field-{0}", attribute.Id );
                    li.Controls.Add( lbl );

                    Control attributeControl = attribute.CreateControl(_blockInstance.AttributeValues[attribute.Key].Value, !Page.IsPostBack);
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

                Button btnSaveAttributes = new Button();
                btnSaveAttributes.Text = "Save";
                btnSaveAttributes.Click += new EventHandler( btnSaveAttributes_Click );
                phAttributes.Controls.Add(btnSaveAttributes);
            }

            base.OnInit( e );
        }

        void btnSaveAttributes_Click(object sender, EventArgs e)
        {
            foreach (Rock.Cms.Cached.Attribute attribute in _blockInstance.Attributes)
            {
                //HtmlGenericControl editCell = ( HtmlGenericControl )blockWrapper.FindControl( string.Format( "attribute-{0}", attribute.Id.ToString() ) );
                Control control = phAttributes.FindControl(string.Format("attribute-field-{0}", attribute.Id.ToString()));
                if (control != null)
                    _blockInstance.AttributeValues[attribute.Key] = new KeyValuePair<string, string>(attribute.Name, attribute.FieldType.Field.ReadValue(control));
            }

            _blockInstance.SaveAttributeValues(CurrentPersonId);

            phAttributes.Controls.Clear();
            phAttributes.Controls.Add(new LiteralControl(@"
    <script type='text/javascript'>
        window.parent.$('#modalDiv').dialog('close');
    </script>
"));
            //        if (BlockInstanceAttributesUpdated != null)
            //            BlockInstanceAttributesUpdated(sender, new BlockInstanceAttributesUpdatedEventArgs(blockInstanceId));
            //    }
            //}
        }
    }
}