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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.UI;

using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock;
using System.Web.UI.WebControls;
using Rock.Web.UI.Controls;
using Rock.Data;
using Rock.Security;
using System.Data.Entity;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for viewing entities with a selected tag
    /// </summary>
    [DisplayName( "Tag Report" )]
    [Category( "Core" )]
    [Description( "Block for viewing entities with a selected tag" )]
    public partial class TagReport : Rock.Web.UI.RockBlock, ISecondaryBlock
    {

        #region Properties

        /// <summary>
        /// Gets or sets the tag identifier.
        /// </summary>
        /// <value>
        /// The tag identifier.
        /// </value>
        public int? TagId { get; set; }

        /// <summary>
        /// Gets or sets the type of the tag entity.
        /// </summary>
        /// <value>
        /// The type of the tag entity.
        /// </value>
        public EntityTypeCache TagEntityType { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gReport.DataKeyNames = new string[] { "Id" };
            gReport.Actions.ShowAdd = false;
            gReport.GridRebind += gReport_GridRebind;

            TagId = PageParameter( "TagId" ).AsIntegerOrNull();
            if ( TagId.HasValue && TagId.Value > 0 )
            {
                Tag _tag = new TagService( new RockContext() ).Get( TagId.Value );
                if ( _tag != null && _tag.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    pnlGrid.Visible = true;
                    lTaggedTitle.Text = "Tagged Items";

                    TagEntityType = EntityTypeCache.Read( _tag.EntityTypeId ?? 0 );
                    if ( TagEntityType != null )
                    {
                        if ( TagEntityType.Name == "Rock.Model.Person" )
                        {
                            gReport.ColumnsOfType<SelectField>().First().Visible = true;
                            gReport.PersonIdField = "PersonId";
                        }

                        var entityType = TagEntityType.GetEntityType();
                        if ( entityType != null )
                        {
                            lTaggedTitle.Text = "Tagged " + entityType.Name.Pluralize().SplitCase();
                            gReport.ColumnsOfType<RockTemplateField>().First( c => c.HeaderText == "Item" ).HeaderText = entityType.Name.SplitCase();
                        }
                    }

                    gReport.ColumnsOfType<DeleteField>().First().Visible = _tag.IsAuthorized( "Tag", CurrentPerson );

                    if ( !Page.IsPostBack )
                    {
                        BindGrid();
                    }
                }
                else
                {
                    pnlGrid.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gReport_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gReport_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var taggedItem = new TaggedItemService( rockContext ).Get( e.RowKeyId );
                if ( taggedItem != null )
                {
                    var entityType = EntityTypeCache.Read( taggedItem.EntityTypeId );
                    if ( entityType != null )
                    {
                        var entity = GetGenericEntity( entityType.GetEntityType(), taggedItem.EntityGuid ) as IEntity;
                        if ( entity != null )
                        {
                            string url = string.Format( "~/{0}/{1}", entityType.FriendlyName.Replace( " ", "" ), entity.Id );
                            if ( entityType.LinkUrlLavaTemplate.IsNotNullOrWhitespace() )
                            {
                                url = entityType.LinkUrlLavaTemplate.ResolveMergeFields( new Dictionary<string, object> { { "Entity", entity } } );
                            }
                            Response.Redirect( url, false );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gReport_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var taggedItemService = new TaggedItemService( rockContext );
                var taggedItem = taggedItemService.Get( e.RowKeyId );
                if ( taggedItem != null && taggedItem.IsAuthorized( "Tag", CurrentPerson ) )
                {
                    string errorMessage;
                    if ( !taggedItemService.CanDelete( taggedItem, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    taggedItemService.Delete( taggedItem );
                    rockContext.SaveChanges();
                }

                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new TaggedItemService( rockContext );
                var people = new PersonService( rockContext ).Queryable().AsNoTracking();

                IQueryable<TaggedItemRow> results = null;

                if ( TagEntityType != null && TagEntityType.Name == "Rock.Model.Person" )
                {
                    results = service.Queryable().AsNoTracking()
                        .Where( t =>
                            t.TagId == TagId.Value )
                        .Join( people, t => t.EntityGuid, p => p.Guid, ( t, p ) => new TaggedItemRow
                        {
                            Id = t.Id,
                            EntityTypeId = t.EntityTypeId,
                            EntityGuid = t.EntityGuid,
                            CreatedDateTime = t.CreatedDateTime,
                            PersonId = p.Id,
                        } );
                }
                else
                {
                    results = service.Queryable().AsNoTracking()
                        .Where( t =>
                            t.TagId == TagId.Value )
                        .Select( t => new TaggedItemRow
                        {
                            Id = t.Id,
                            EntityTypeId = t.EntityTypeId,
                            EntityGuid = t.EntityGuid,
                            CreatedDateTime = t.CreatedDateTime
                        } );
                }

                gReport.DataSource = results.ToList();
                gReport.DataBind();
            }
        }

        public string GetItemName( int entityTypeId, Guid entityGuid )
        {
            var entityType = EntityTypeCache.Read( entityTypeId );
            if ( entityType != null )
            {
                var entity = GetGenericEntity( entityType.GetEntityType(), entityGuid ) as IEntity;
                if ( entity != null )
                {
                    return entity.ToString();
                }
            }

            return "Item?";
        }

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        private object GetGenericEntity( Type modelType, Guid guid )
        {
            // Get the context type since this may be for a non-rock core object
            Type contextType = null;
            var contexts = Rock.Reflection.SearchAssembly( modelType.Assembly, typeof( System.Data.Entity.DbContext ) );
            if ( contexts.Any() )
            {
                contextType = contexts.First().Value;
            }
            else
            {
                contextType = typeof( RockContext );
            }

            Type genericServiceType = typeof( Rock.Data.Service<> );
            Type modelServiceType = genericServiceType.MakeGenericType( new Type[] { modelType } );

            if ( modelServiceType != null )
            {
                var context = Activator.CreateInstance( contextType );
                Object serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { context } );
                if ( serviceInstance != null )
                {
                    MethodInfo method = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( Guid ) } );
                    if ( method != null )
                    {
                        return method.Invoke( serviceInstance, new object[] { guid } );
                    }
                }
            }

            return null;
        }

        #endregion

        private class TaggedItemRow
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public int EntityTypeId { get; set; }
            public Guid EntityGuid { get; set; }
            public DateTime? CreatedDateTime { get; set; }
        }
    }

    
}