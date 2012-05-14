//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using AjaxControlToolkit;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{   
    /// <summary>
    /// A modal dialog.
    /// </summary>
    [
        AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal),
        AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal),
        Designer( typeof( ModalDialog ) ),
        DefaultProperty("Title"),
        ToolboxData( "<{0}:ModalDialog runat=server></{0}:ModalDialog>" )
    ]
    public class ModalDialog : CompositeControl
    {
        private ITemplate contentValue;
        private TemplateOwner ownerValue;

        [
            Browsable( false ),
            DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden )
        ]
        public TemplateOwner Owner
        {
            get { return ownerValue; }
        }
        
        [
            Browsable( false ),
            PersistenceMode( PersistenceMode.InnerProperty ),
            DefaultValue( typeof( ITemplate ), "" ),
            Description( "Content Template" ),
            TemplateContainer( typeof( ModalDialog ) )
        ]
        public virtual ITemplate Content
        {
            get
            {
                return contentValue;
            }
            set
            {
                contentValue = value;
            }
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            ownerValue = new TemplateOwner();

            ITemplate temp = contentValue;
            if ( temp != null )
               temp.InstantiateIn( ownerValue );

            this.Controls.Add( ownerValue );
        }

        public override void DataBind()
        {
            CreateChildControls();
            ChildControlsCreated = true;
            base.DataBind();
        }
    }

    [ToolboxItem( false )]
    public class TemplateOwner : WebControl
    {
    }

}