using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Security;
using Rock.Web.UI.Controls;


namespace RockWeb.Plugins.org_newpointe.HTMLContext
{
    /// <summary>
    /// Adds an editable HTML fragment to the page.
    /// </summary>
    [DisplayName( "HTML Context Detail" )]
    [Category( "NewPointe -> CMS" )]
    [Description( "Lists and allows editing of context-sensitive HTML fragments." )]

    public partial class HTMLContextDetail : Rock.Web.UI.RockBlock
    {

        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                BindGrid();
            }
        }

        protected void BindGrid()
        {

            SortProperty sortProp = rgHtmlFragments.SortProperty;

            var HtmlContents = new HtmlContentService( new RockContext() ).Queryable().Where( hc => hc.BlockId == BlockId || ( hc.EntityValue != null && hc.EntityValue != "" ) ).ToList().Select( hc =>
            {
                var context = HttpUtility.ParseQueryString( hc.EntityValue );
                var cName = context["ContextName"];
                context.Remove( "ContextName" );
                var cParams = context.ToString();
                return new
                {
                    Id = hc.Id,
                    BlockId = hc.BlockId,
                    ContextName = cName,
                    ContextParameters = cParams,
                    Version = hc.Version,
                    IsApproved = hc.IsApproved,
                    ApprovedDateTime = hc.ApprovedDateTime,
                    StartDateTime = hc.StartDateTime,
                    ExpireDateTime = hc.ExpireDateTime
                };
            } ).AsQueryable();


            if ( sortProp != null )
            {
                rgHtmlFragments.DataSource = HtmlContents.Sort( sortProp ).ToList();
            }
            else
            {
                rgHtmlFragments.DataSource = HtmlContents.OrderBy( "Version" ).OrderBy( "ContextName" ).ToList();
            }
            rgHtmlFragments.DataKeyNames = new String[] { "Id" };
            rgHtmlFragments.DataBind();
        }

        protected void rgHtmlFragments_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        protected void rgHtmlFragments_EditHTML( object sender, RowEventArgs e )
        {
            htmlEditor.Visible = true;
            ceHtml.Visible = false;
            rgHtmlFragments_Edit( e.RowKeyId );
        }

        protected void rgHtmlFragments_EditCode( object sender, RowEventArgs e )
        {
            htmlEditor.Visible = false;
            ceHtml.Visible = true;
            rgHtmlFragments_Edit( e.RowKeyId );
        }

        protected void rgHtmlFragments_Edit( int contentId )
        {
            pnlList.Visible = false;
            pnlEdit.Visible = true;

            var hc = new HtmlContentService( new RockContext() ).Get( contentId );

            hfHTMLContentGUID.Value = hc.Guid.ToString();

            rtbBlockId.Text = hc.BlockId.ToString();

            var context = HttpUtility.ParseQueryString( hc.EntityValue );
            rtbContextName.Text = context["ContextName"];

            context.Remove( "ContextName" );
            rtbContextParameter.Text = context.ToString();

            rtbVersion.Text = hc.Version.ToString();

            dtpStartTime.SelectedDateTime = hc.StartDateTime;
            dtpExpire.SelectedDateTime = hc.ExpireDateTime;

            rcbApproved.Checked = hc.IsApproved;

            if ( htmlEditor.Visible )
            {
                htmlEditor.Text = hc.Content;
            }
            else
            {
                ceHtml.Text = hc.Content;
            }

        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            pnlList.Visible = true;
            pnlEdit.Visible = false;
            BindGrid();
        }

        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var hc = new HtmlContentService( rockContext ).Get( hfHTMLContentGUID.Value.AsGuid() );
            if ( hc != null )
            {
                var oldBlockId = hc.BlockId;
                hc.BlockId = rtbBlockId.Text.AsIntegerOrNull() ?? BlockId;
                var oldEntityValue = hc.EntityValue;
                hc.EntityValue = rtbContextParameter.Text + ( String.IsNullOrWhiteSpace( rtbContextName.Text ) ? "" : ( "&ContextName=" + rtbContextName.Text ) );
                hc.Version = rtbVersion.Text.AsInteger();
                hc.StartDateTime = dtpStartTime.SelectedDateTime;
                hc.ExpireDateTime = dtpExpire.SelectedDateTime;

                if ( !hc.IsApproved && rcbApproved.Checked )
                {
                    hc.ApprovedDateTime = DateTime.Now;
                }
                else if ( !rcbApproved.Checked )
                {
                    hc.ApprovedDateTime = null;
                }
                hc.IsApproved = rcbApproved.Checked;
                if ( htmlEditor.Visible )
                {
                    hc.Content = htmlEditor.Text;
                }
                else
                {
                    hc.Content = ceHtml.Text;
                }

                rockContext.SaveChanges();
                HtmlContentService.FlushCachedContent( oldBlockId, oldEntityValue );
                HtmlContentService.FlushCachedContent( hc.BlockId, hc.EntityValue );

            }
            pnlList.Visible = true;
            pnlEdit.Visible = false;
            BindGrid();

        }

        protected void lbSaveAs_Click( object sender, EventArgs e )
        {
            var hc = new HtmlContent();
            hc.BlockId = rtbBlockId.Text.AsIntegerOrNull() ?? BlockId;
            hc.EntityValue = rtbContextParameter.Text + ( string.IsNullOrWhiteSpace( rtbContextName.Text ) ? "" : ( "&ContextName=" + rtbContextName.Text ) );
            hc.Version = rtbVersion.Text.AsInteger();
            hc.StartDateTime = dtpStartTime.SelectedDateTime;
            hc.ExpireDateTime = dtpExpire.SelectedDateTime;
            hc.IsApproved = rcbApproved.Checked;
            if (hc.IsApproved)
            {
                hc.ApprovedDateTime = DateTime.Now;
            }
            if ( htmlEditor.Visible )
            {
                hc.Content = htmlEditor.Text;
            }
            else
            {
                hc.Content = ceHtml.Text;
            }

            var rockContext = new RockContext();
            var hcServ = new HtmlContentService( rockContext );
            hcServ.Add( hc );
            rockContext.SaveChanges();

            pnlList.Visible = true;
            pnlEdit.Visible = false;
            BindGrid();

        }
    }
}