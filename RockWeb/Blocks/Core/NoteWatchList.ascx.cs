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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Note Watch List" )]
    [Category( "Core" )]
    [Description( "Block for viewing a list of note watches" )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]
    
    [EntityTypeField( "Entity Type",
        Description = "Set an Entity Type to limit this block to Note Types and Entities for a specific entity type.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EntityType )]

    [NoteTypeField( "Note Type",
        Description = "Set Note Type to limit this block to a specific note type",
        AllowMultiple = false,
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.NoteType )]

    // Context Aware will limit the list to watchers that are equal to the Person or Group context
    [ContextAware( typeof( Rock.Model.Group ), typeof( Rock.Model.Person ) )]
    public partial class NoteWatchList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string EntityType = "EntityType";
            public const string NoteType = "NoteType";
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gList.DataKeyNames = new string[] { "Id" };
            gList.Actions.AddClick += gList_Add;
            gList.GridRebind += gList_GridRebind;

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gList.Actions.ShowAdd = canAddEditDelete;
            gList.IsDeleteEnabled = canAddEditDelete;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void gList_Add( object sender, EventArgs e )
        {
            NavigateToNoteWatchDetailPage( 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToNoteWatchDetailPage( e.RowKeyId );
        }

        /// <summary>
        /// Navigates to note watch detail page including PersonId and GroupId if there is a Person or Group context set
        /// </summary>
        /// <param name="noteWatchId">The note watch identifier.</param>
        protected void NavigateToNoteWatchDetailPage( int noteWatchId )
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add( "NoteWatchId", noteWatchId.ToString() );
            var contextPerson = ContextEntity<Person>();
            var contextGroup = ContextEntity<Group>();
            if ( contextPerson != null )
            {
                queryParams.Add( "PersonId", contextPerson.Id.ToString() );
            }

            if ( contextGroup != null )
            {
                queryParams.Add( "GroupId", contextGroup.Id.ToString() );
            }

            NavigateToLinkedPage( AttributeKey.DetailPage, queryParams );
        }

        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new NoteWatchService( rockContext );
            var noteWatch = service.Get( e.RowKeyId );
            if ( noteWatch != null )
            {
                string errorMessage;
                if ( !service.CanDelete( noteWatch, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( noteWatch );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            var noteWatch = e.Row.DataItem as NoteWatch;
            if ( noteWatch == null )
            {
                return;
            }

            var lWatchingEntityType = e.Row.FindControl( "lWatchingEntityType" ) as Literal;
            if ( lWatchingEntityType != null )
            {
                if ( noteWatch.EntityTypeId.HasValue )
                {
                    var entityType = EntityTypeCache.Get( noteWatch.EntityTypeId.Value );
                    if ( entityType != null )
                    {
                        lWatchingEntityType.Text = entityType.FriendlyName;

                        if ( noteWatch.EntityId.HasValue && noteWatch.EntityTypeId.HasValue )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                IEntity entity = new EntityTypeService( new RockContext() ).GetEntity( noteWatch.EntityTypeId.Value, noteWatch.EntityId.Value );
                                if ( entity != null )
                                {
                                    lWatchingEntityType.Text = entityType.FriendlyName + " (" + entity.ToString() + ")";
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            NoteWatchService noteWatchService = new NoteWatchService( rockContext );

            var qry = noteWatchService.Queryable().Include( a => a.WatcherPersonAlias.Person ).Include( a => a.WatcherGroup );

            Guid? blockEntityTypeGuid = this.GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            Guid? blockNoteTypeGuid = this.GetAttributeValue( AttributeKey.NoteType ).AsGuidOrNull();
            if ( blockNoteTypeGuid.HasValue )
            {
                // if a NoteType was specified in block settings, only list note watches for the specified note type
                int noteTypeId = EntityTypeCache.Get( blockNoteTypeGuid.Value ).Id;
                qry = qry.Where( a => a.NoteTypeId.HasValue && a.NoteTypeId == noteTypeId );
            }
            else if ( blockEntityTypeGuid.HasValue )
            {
                // if an EntityType was specific in block settings, only list note watches for the specified entity type (or for NoteTypes of the specified EntityType)
                int entityTypeId = EntityTypeCache.Get( blockEntityTypeGuid.Value ).Id;
                qry = qry.Where( a =>
                    ( a.EntityTypeId.HasValue && a.EntityTypeId.Value == entityTypeId )
                    || ( a.NoteTypeId.HasValue && a.NoteType.EntityTypeId == entityTypeId ) );
            }

            var contextPerson = ContextEntity<Person>();
            var contextGroup = ContextEntity<Group>();

            if ( contextPerson != null )
            {
                // if there is a Person context, only list note watches that where the watcher is the person context
                qry = qry.Where( a => a.WatcherPersonAliasId.HasValue && a.WatcherPersonAlias.PersonId == contextPerson.Id );
            }
            else if ( contextGroup != null )
            {
                // if there is a Group context, only list note watches that where the watcher is the group context
                qry = qry.Where( a => a.WatcherGroupId.HasValue && a.WatcherGroupId == contextGroup.Id );
            }

            var sortProperty = gList.SortProperty;

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( d => d.EntityType.Name ).ThenBy( a => a.NoteType.Name );
            }

            gList.SetLinqDataSource( qry );
            gList.DataBind();
        }

        #endregion
    }
}