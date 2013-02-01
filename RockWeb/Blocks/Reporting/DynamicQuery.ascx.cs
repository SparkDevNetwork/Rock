//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [LinkedPage("Person Page", "PersonPageGuid")]
    [BooleanField(1, "Update Page", true, "UpdatePage", "", "If True, provides fields for updating the parent page's Name and Description")] 
    [TextField( 2, "Query", "The query to execute", false )]
    [TextField( 3, "Query Params", "Parameters to pass to query", false )]
    [TextField( 4, "Hidden Columns", "The query to execute", false )]
    [TextField( 5, "Url Mask", "The Url to redirect to when a row is clicked", false )]
    [BooleanField( 4, "Show Columns", false, "ShowColumns", "", "Should 'Hidden Columns' instead be the only columns shown")]
    public partial class DynamicQuery : RockBlock
    { 
        #region Control Methods

        bool updatePage = true;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !bool.TryParse( GetAttributeValue( "UpdatePage" ), out updatePage ) )
            {
                updatePage = true;
            }

            string scriptKey = "edit-report";
            if ( !this.Page.ClientScript.IsClientScriptBlockRegistered( scriptKey ) )
            {
                string script = @"
    function toggleDetails() {
        $('.dynamic-report-details').slideToggle();
    }"
;
                this.Page.ClientScript.RegisterStartupScript( this.Page.GetType(), scriptKey, script, true );
            }

        }
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            gReport.GridRebind += gReport_GridRebind;
            gReport.RowSelected += gReport_RowSelected;

            bool allowEdit = IsUserAuthorized( "Edit" );
            phEdit.Visible = allowEdit;
            phDetails.Visible = allowEdit;

            if ( !Page.IsPostBack )
            {
                lDesc.Visible = updatePage;
                if ( updatePage )
                {
                    lDesc.Text = CurrentPage.Description;
                }

                if ( allowEdit )
                {
                    tbName.Visible = updatePage;
                    tbDesc.Visible = updatePage;
                    if ( updatePage )
                    {
                        tbName.Text = CurrentPage.Title;
                        tbDesc.Text = CurrentPage.Description;
                    }

                    tbQuery.Text = GetAttributeValue( "Query" );
                    tbParams.Text = GetAttributeValue( "QueryParams" );
                    tbHide.Text = GetAttributeValue( "HiddenColumns" );
                    tbUrlMask.Text = GetAttributeValue( "UrlMask" );
                    ddlHideShow.SelectedValue = GetAttributeValue( "ShowColumns");
                }

                BindGrid();
            }
        }

        void gReport_RowSelected( object sender, RowEventArgs e )
        {
            string url = GetAttributeValue( "UrlMask" );
            if ( !string.IsNullOrWhiteSpace( url ) )
            {
                foreach ( string key in gReport.DataKeyNames )
                {
                    url = url.Replace( "{" + key + "}", gReport.DataKeys[e.RowIndex][key].ToString() );
                }
                Response.Redirect( url, false );
            }
        }

        void gReport_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( updatePage && ( CurrentPage.Title != tbName.Text ||
                CurrentPage.Description != tbDesc.Text ) )
            {
                var service = new PageService();
                var page = service.Get( CurrentPage.Id );
                page.Title = tbName.Text;
                page.Description = tbDesc.Text;
                service.Save( page, CurrentPersonId );

                Rock.Web.Cache.PageCache.Flush( page.Id );

                CurrentPage.Title = page.Title;
                CurrentPage.Description = page.Description;

                lDesc.Text = page.Description;

                ( (RockPage)Page ).SetTitle( page.Title );
            }

            SetAttributeValue( "Query", tbQuery.Text );
            SetAttributeValue( "QueryParams", tbParams.Text );
            SetAttributeValue( "HiddenColumns", tbHide.Text );
            SetAttributeValue( "UrlMask", tbUrlMask.Text );
            SetAttributeValue( "ShowColumns", ddlHideShow.SelectedValue );
            SaveAttributeValues( CurrentPersonId );

            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            gReport.Visible = true;
            pnlError.Visible = false;

            string query = GetAttributeValue( "Query" );
            if ( !string.IsNullOrWhiteSpace( query ) )
            {
                try
                {
                    string urlMask = GetAttributeValue( "UrlMask" );
                    if ( !string.IsNullOrWhiteSpace( urlMask ) )
                    {
                        Regex pattern = new Regex( @"\{.+\}" );
                        var matches = pattern.Matches( urlMask );
                        if ( matches.Count > 0 )
                        {
                            string[] keyNames = new string[matches.Count];
                            for ( int i = 0; i < matches.Count; i++ )
                            {
                                keyNames[i] = matches[i].Value.TrimStart( '{' ).TrimEnd( '}' );
                            }
                            gReport.DataKeyNames = keyNames;
                        }

                    }

                    DataTable dataTable = null;

                    string[] queryParams = GetAttributeValue("QueryParams").SplitDelimitedValues();
                    if ( queryParams.Length > 0 )
                    {
                        var parameters = new Dictionary<string, object>();
                        foreach(string queryParam in queryParams)
                        {
                            string[] paramParts = queryParam.Split('=');
                            if (paramParts.Length == 2)
                            {
                                string queryParamName = paramParts[0];
                                string queryParamValue = paramParts[1];

                                // Remove leading '@' character if was included
                                if (queryParamName.StartsWith("@"))
                                {
                                    queryParamName = queryParamName.Substring(1);
                                }

                                // If a page parameter (query or form) value matches, use it's value instead
                                string pageValue = PageParameter(queryParamName);
                                if (!string.IsNullOrWhiteSpace(pageValue))
                                {
                                    queryParamValue = pageValue;
                                }

                                // If current person id, use the value of the current person id
                                else if (queryParamName.ToLower() == "currentpersonid" && CurrentPersonId.HasValue)
                                {
                                    queryParamValue = CurrentPersonId.Value.ToString();
                                }

                                parameters.Add(queryParamName, queryParamValue);
                            }
                        }
                        dataTable = new Service().GetDataTable( GetAttributeValue( "Query" ), System.Data.CommandType.StoredProcedure, parameters );
                    }
                    else
                    {
                        dataTable = new Service().GetDataTable( GetAttributeValue( "Query" ), System.Data.CommandType.Text, null );
                    }

                    bool showColumns = bool.Parse(GetAttributeValue( "ShowColumns"));
                    var columnList = GetAttributeValue( "HiddenColumns" ).SplitDelimitedValues().ToList();

                    int rowsToEval = 10;
                    if ( dataTable.Rows.Count < 10 )
                    {
                        rowsToEval = dataTable.Rows.Count;
                    }

                    gReport.Columns.Clear();
                    foreach(DataColumn dtColumn in dataTable.Columns)
                    {
                        if ( columnList.Count > 0 && 
                            ((showColumns && !columnList.Contains(dtColumn.ColumnName)) ||
                                (!showColumns && columnList.Contains(dtColumn.ColumnName))))
                        {
                            continue;
                        }

                        BoundField bf = new BoundField();

                        if ( dtColumn.DataType == typeof( Boolean ) )
                        {
                            bf = new BoolField();
                        }

                        if ( dtColumn.DataType == typeof( DateTime ) )
                        {
                            bf = new DateField();

                            for ( int i = 0; i < rowsToEval; i++ )
                            {
                                object dateObj = dataTable.Rows[i][dtColumn];
                                if ( dateObj is DateTime )
                                {
                                    DateTime dateTime = (DateTime)dateObj;
                                    if ( dateTime.TimeOfDay.Seconds != 0 )
                                    {
                                        bf = new DateTimeField();
                                        break;
                                    }
                                }
                            }
                        }

                        bf.DataField = dtColumn.ColumnName;
                        bf.SortExpression = dtColumn.ColumnName;
                        bf.HeaderText = dtColumn.ColumnName.SplitCase();
                        gReport.Columns.Add( bf );
                    }

                    DataView dataView = dataTable.DefaultView;

                    SortProperty sortProperty = gReport.SortProperty;
                    if (sortProperty != null)
                    {
                        dataView.Sort = string.Format( "{0} {1}", 
                            sortProperty.Property, 
                            sortProperty.DirectionString);
                    }

                    gReport.DataSource = dataView;
                    gReport.DataBind();
                }
                catch ( System.Exception ex )
                {
                    gReport.Visible = false;
                    lError.Text = ex.Message;
                    pnlError.Visible = true;
                }
            }
        }

        #endregion

    }
}