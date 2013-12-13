//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.
    /// </summary>
    [Description( "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure." )]
    [BooleanField( "Update Page", "If True, provides fields for updating the parent page's Name and Description", true, "", 0 )] 
    [CodeEditorField( "Query", "The query to execute", CodeEditorMode.Sql, CodeEditorTheme.Rock, 400, false, "", "", 1 )]
    [TextField( "Query Params", "Parameters to pass to query", false, "", "", 2 )]
    [TextField( "Url Mask", "The Url to redirect to when a row is clicked", false, "", "", 3 )]
    [BooleanField( "Show Columns", "Should the 'Columns' specified below be the only ones shown (vs. the only ones hidden)", false, "", 4)]
    [TextField( "Columns", "The columns to hide or show", false, "", "", 5 )]
    [CodeEditorField( "Formatted Output", "Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %}",
        CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, false, "", "", 7 )]
    [BooleanField( "Person Report", "Is this report a list of people.?", false, "", 8 )]
    [TextField( "Merge Fields", "Any fields to make available as merge fields for any new communications", false, "", "", 9 )]
    public partial class DynamicData : RockBlock
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
                if ( allowEdit )
                {
                    if ( updatePage )
                    {
                        var pageCache = PageCache.Read( RockPage.PageId );
                        tbName.Text = pageCache != null ? pageCache.Title : string.Empty;
                        tbDesc.Text = pageCache != null ? pageCache.Description : string.Empty;
                    }

                    tbName.Visible = updatePage;
                    tbDesc.Visible = updatePage;

                    ceQuery.Text = GetAttributeValue( "Query" );
                    tbParams.Text = GetAttributeValue( "QueryParams" );
                    tbUrlMask.Text = GetAttributeValue( "UrlMask" );
                    ddlHideShow.SelectedValue = GetAttributeValue( "ShowColumns");
                    tbColumns.Text = GetAttributeValue( "Columns" );
                    ceFormattedOutput.Text = GetAttributeValue( "FormattedOutput" );
                    cbPersonReport.Checked = Boolean.Parse( GetAttributeValue( "PersonReport" ) );
                    tbMergeFields.Text = GetAttributeValue( "MergeFields" );
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

        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( updatePage )
            {
                var pageCache = PageCache.Read( RockPage.PageId );
                if ( pageCache != null &&
                    ( pageCache.Title != tbName.Text || pageCache.Description != tbDesc.Text ) )
                {
                    var service = new PageService();
                    var page = service.Get( pageCache.Id );
                    page.Title = tbName.Text;
                    page.Description = tbDesc.Text;
                    service.Save( page, CurrentPersonId );

                    Rock.Web.Cache.PageCache.Flush( page.Id );
                    pageCache = PageCache.Read( RockPage.PageId );

                    var breadCrumb = RockPage.BreadCrumbs.Where( c => c.Url == RockPage.PageReference.BuildUrl() ).FirstOrDefault();
                    if (breadCrumb != null)
                    {
                        breadCrumb.Name = pageCache.BreadCrumbText;
                    }

                    Page.Title = tbName.Text;
                }
            }

            SetAttributeValue( "Query", ceQuery.Text );
            SetAttributeValue( "QueryParams", tbParams.Text );
            SetAttributeValue( "UrlMask", tbUrlMask.Text );
            SetAttributeValue( "Columns", tbColumns.Text );
            SetAttributeValue( "ShowColumns", ddlHideShow.SelectedValue );
            SetAttributeValue( "FormattedOutput", ceFormattedOutput.Text );
            SetAttributeValue( "PersonReport", cbPersonReport.Checked.ToString());
            SetAttributeValue( "MergeFields", tbMergeFields.Text );
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
            bool personReport = Boolean.Parse( GetAttributeValue( "PersonReport" ) );
            
            if (personReport)
            {
                gReport.PersonIdField = "Id";
            }
            else
            {
                gReport.PersonIdField = null;
            }

            gReport.CommunicateMergeFields = GetAttributeValue( "MergeFields" ).SplitDelimitedValues().ToList<string>();
            gReport.Visible = true;
            nbError.Visible = false;

            string query = GetAttributeValue( "Query" );
            if ( !string.IsNullOrWhiteSpace( query ) )
            {
                try
                {
                    query = query.ResolveMergeFields( PageParameters() );

                    var parameters = GetParameters();
                    DataTable dataTable = new Service().GetDataTable( query, ( parameters != null ? CommandType.StoredProcedure : CommandType.Text ), parameters );

                    if (string.IsNullOrWhiteSpace(ceFormattedOutput.Text))
                    {
                        gReport.Visible = true;
                        phHtml.Visible = false;

                        AddGridColumns( dataTable );
                        SetDataKeyNames();
                        gReport.DataSource = GetSortedView( dataTable ); ;
                        gReport.DataBind();
                    }
                    else
                    {
                        gReport.Visible = false;
                        phHtml.Visible = true;
                        
                        var dropRows = new List<DataRowDrop>();
                        foreach(DataRow row in dataTable.Rows)
                        {
                            dropRows.Add(new DataRowDrop(row));
                        }
                        var dropTable = new Dictionary<string, object>();
                        dropTable.Add("rows", dropRows);

                        phHtml.Controls.Add( new LiteralControl( ceFormattedOutput.Text.ResolveMergeFields( dropTable ) ) );
                    }
                }
                catch ( System.Exception ex )
                {
                    gReport.Visible = false;
                    phHtml.Visible = false;

                    nbError.Text = ex.Message;
                    nbError.Visible = true;
                }
            }
        }

        private void SetDataKeyNames()
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
        }

        private Dictionary<string, object> GetParameters()
        {
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

                return parameters;
            }

            return null;
        }

        private void AddGridColumns(DataTable dataTable)
        {
            bool showColumns = bool.Parse( GetAttributeValue( "ShowColumns" ) );
            var columnList = GetAttributeValue( "Columns" ).SplitDelimitedValues().ToList();

            int rowsToEval = 10;
            if ( dataTable.Rows.Count < 10 )
            {
                rowsToEval = dataTable.Rows.Count;
            }

            gReport.Columns.Clear();
            foreach(DataColumn dtColumn in dataTable.Columns)
            {
                if ( columnList.Count > 0 && 
                    ((showColumns && !columnList.Contains(dtColumn.ColumnName, StringComparer.OrdinalIgnoreCase)) ||
                        ( !showColumns && columnList.Contains( dtColumn.ColumnName, StringComparer.OrdinalIgnoreCase ) ) ) )
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
        }

        private System.Data.DataView GetSortedView( DataTable dataTable )
        {
            System.Data.DataView dataView = dataTable.DefaultView;

            SortProperty sortProperty = gReport.SortProperty;
            if ( sortProperty != null )
            {
                dataView.Sort = string.Format( "{0} {1}",
                    sortProperty.Property,
                    sortProperty.DirectionString );
            }

            return dataView;
        }

        #endregion

        class DataRowDrop : DotLiquid.Drop
        {
            private readonly DataRow _dataRow;

            public DataRowDrop(DataRow dataRow)
            {
                _dataRow = dataRow;
            }

            public override object BeforeMethod( string method )
            {
                if ( _dataRow.Table.Columns.Contains( method ) )
                {
                    return _dataRow[method];
                }
                return null;
            }

        }
    }
}