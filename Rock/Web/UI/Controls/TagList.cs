//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:TagList runat=server></{0}:TagList>" )]
    public class TagList : CompositeControl
    {
        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int EntityTypeId
        {
            get { return ViewState["EntityTypeId"] as int? ?? 0; }
            set { ViewState["EntityTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the entity qualifier column.
        /// </summary>
        /// <value>
        /// The entity qualifier column.
        /// </value>
        public string EntityQualifierColumn
        {
            get { return ViewState["EntityQualifierColumn"] as string ?? string.Empty; }
            set { ViewState["EntityQualifierColumn"] = value; }
        }

        /// <summary>
        /// Gets or sets the entity qualifier value.
        /// </summary>
        /// <value>
        /// The entity qualifier value.
        /// </value>
        public string EntityQualifierValue
        {
            get { return ViewState["EntityQualifierValue"] as string ?? string.Empty; }
            set { ViewState["EntityQualifierValue"] = value; }
        }

        /// <summary>
        /// Gets or sets the entity GUID.
        /// </summary>
        /// <value>
        /// The entity GUID.
        /// </value>
        public Guid EntityGuid
        {
            get
            {
                string entityGuid = ViewState["EntityGuid"] as string;
                if ( !string.IsNullOrWhiteSpace( entityGuid ) )
                {
                    return new Guid( entityGuid );
                }
                return Guid.Empty;
            }
            set
            {
                ViewState["EntityGuid"] = value.ToString();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddScriptLink( Page, ResolveUrl( "~/Scripts/jquery.tagsinput.js" ) );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                var sb = new StringBuilder();

                var service = new TaggedItemService();
                foreach ( dynamic item in service.Get(
                    EntityTypeId, EntityQualifierColumn, EntityQualifierValue, rockPage.CurrentPersonId, EntityGuid )
                    .Select( i => new
                    {
                        OwnerId = i.Tag.OwnerId,
                        Name = i.Tag.Name
                    } ) )
                {
                    if ( sb.Length > 0 )
                        sb.Append( ',' );
                    sb.Append( item.Name );
                    if ( rockPage.CurrentPersonId.HasValue && item.OwnerId == rockPage.CurrentPersonId.Value )
                        sb.Append( "^personal" );
                }

                writer.AddAttribute( "class", "tag-wrap" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                
                var input = new HtmlGenericControl( "input" );
                input.ID = this.ID;
                input.Attributes.Add( "value", sb.ToString() );
                input.RenderControl( writer );
                writer.RenderEndTag();

                var script = string.Format( @"
Rock.controls.tagList.initialize({{
    controlId: '{0}',
    entityTypeId: '{1}',
    currentPersonId: '{2}',
    entityGuid: '{3}',
    entityQualifierColumn: '{4}',
    entityQualifierValue: '{5}'
}});",
                    this.ID,
                    EntityTypeId,
                    rockPage.CurrentPersonId,
                    EntityGuid.ToString(),
                    string.IsNullOrWhiteSpace( EntityQualifierColumn ) ? string.Empty : EntityQualifierColumn,
                    string.IsNullOrWhiteSpace( EntityQualifierValue ) ? string.Empty : EntityQualifierValue );
                ScriptManager.RegisterStartupScript(this, this.GetType(), "tag_picker_" + this.ID, script, true);
            }
        }
    }
}