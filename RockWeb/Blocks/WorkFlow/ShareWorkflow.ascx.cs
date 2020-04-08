// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility.EntityCoding;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.WorkFlow
{
    [DisplayName( "Share Workflow" )]
    [Category( "WorkFlow" )]
    [Description( "Export and import workflows from Rock." )]
    public partial class ShareWorkflow : Rock.Web.UI.RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( btnExport );
            if ( !Page.IsPostBack )
            {
                wtpExport.SetValue( PageParameter( "WorkflowTypeId" ).AsInteger() );
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Binds the preview grid.
        /// </summary>
        protected void BindPreviewGrid()
        {
            List<PreviewEntity> previewEntities = ( List<PreviewEntity> ) ViewState["PreviewEntities"];

            if ( previewEntities == null )
            {
                previewEntities = new List<PreviewEntity>();
            }

            var query = previewEntities.AsQueryable();

            if ( gPreview.SortProperty != null )
            {
                query = query.Sort( gPreview.SortProperty );
            }

            gPreview.DataSource = query;
            gPreview.DataBind();
        }

        /// <summary>
        /// Get a friendly name for the entity, optionally including the short name for the
        /// entity type. This attempts a ToString() on the entity and if that returns what
        /// appears to be a valid name (no &lt; character and less than 40 characters) then
        /// it is used as the name. Otherwise the Guid is used for the name.
        /// </summary>
        /// <param name="entity">The entity whose name we wish to retrieve.</param>
        /// <returns>A string that can be displayed to the user to identify this entity.</returns>
        static protected string EntityFriendlyName( IEntity entity )
        {
            string name;

            name = entity.ToString();
            if ( name.Length > 40 || name.Contains( "<" ) )
            {
                name = entity.Guid.ToString();
            }

            return name;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the btnPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPreview_Click( object sender, EventArgs e )
        {

            // Clean-up UI
            gPreview.Visible = true;
            ltImportResults.Text = string.Empty;

            RockContext rockContext = new RockContext();
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( wtpExport.SelectedValueAsId().Value );
            var coder = new EntityCoder( new RockContext() );
            var exporter = new WorkflowTypeExporter();
            coder.EnqueueEntity( workflowType, exporter );

            List<PreviewEntity> previewEntities = new List<PreviewEntity>();

            foreach ( var qe in coder.Entities )
            {
                string shortType = CodingHelper.GetEntityType( qe.Entity ).Name;

                if ( shortType == "Attribute" || shortType == "AttributeValue" || shortType == "AttributeQualifier" || shortType == "WorkflowActionFormAttribute" )
                {
                    continue;
                }

                var preview = new PreviewEntity
                {
                    Guid = qe.Entity.Guid,
                    Name = EntityFriendlyName( qe.Entity ),
                    ShortType = shortType,
                    IsCritical = qe.IsCritical,
                    IsNewGuid = qe.RequiresNewGuid,
                    Paths = qe.ReferencePaths.Select( p => p.ToString() ).ToList()
                };

                previewEntities.Add( preview );
            }

            ViewState["PreviewEntities"] = previewEntities;

            BindPreviewGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnExport_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( wtpExport.SelectedValueAsId().Value );
            var coder = new EntityCoder( new RockContext() );
            coder.EnqueueEntity( workflowType, new WorkflowTypeExporter() );

            var container = coder.GetExportedEntities();

            Page.EnableViewState = false;
            Page.Response.Clear();
            Page.Response.ContentType = "application/json";
            Page.Response.AppendHeader( "Content-Disposition", string.Format( "attachment; filename=\"{0}_{1}.json\"", workflowType.Name.MakeValidFileName(), RockDateTime.Now.ToString( "yyyyMMddHHmm" ) ) );
            Page.Response.Write( Newtonsoft.Json.JsonConvert.SerializeObject( container, Newtonsoft.Json.Formatting.Indented ) );
            Page.Response.Flush();
            Page.Response.End();
        }

        /// <summary>
        /// Handles the Click event of the lbImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbImport_Click( object sender, EventArgs e )
        {
            gPreview.Visible = false;

            if ( !fuImport.BinaryFileId.HasValue || !cpImportCategory.SelectedValueAsId().HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = binaryFileService.Get( fuImport.BinaryFileId ?? 0 );
                var categoryService = new CategoryService( rockContext );

                var container = Newtonsoft.Json.JsonConvert.DeserializeObject<ExportedEntitiesContainer>( binaryFile.ContentsToString() );
                List<string> messages;

                var decoder = new EntityDecoder( new RockContext() );
                decoder.UserValues.Add( "WorkflowCategory", categoryService.Get( cpImportCategory.SelectedValueAsId().Value ) );

                var success = decoder.Import( container, cbDryRun.Checked, out messages );

                ltImportResults.Text = string.Empty;
                foreach ( var msg in messages )
                {
                    ltImportResults.Text += string.Format( "{0}\n", msg.EncodeHtml() );
                }

                pnlImportResults.Visible = true;

                if ( success )
                {
                    fuImport.BinaryFileId = null;
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gPreview_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindPreviewGrid();
        }

        #endregion

        [Serializable]
        protected class PreviewEntity
        {
            public Guid Guid { get; set; }

            public string Name { get; set; }

            public string ShortType { get; set; }

            public bool IsCritical { get; set; }

            public bool IsNewGuid { get; set; }

            public List<string> Paths { get; set; }
        }
    }
}
