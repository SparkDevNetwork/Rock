﻿// <copyright>
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
            gReport.GridRebind += gReport_GridRebind;

            int tagId = int.MinValue;
            if ( int.TryParse( PageParameter( "tagId" ), out tagId ) && tagId > 0 )
            {
                Tag _tag = new TagService( new RockContext() ).Get( tagId );

                if ( _tag != null )
                {
                    TagId = tagId;
                    TagEntityType = EntityTypeCache.Read( _tag.EntityTypeId );

                    if ( TagEntityType != null )
                    {
                        Type modelType = TagEntityType.GetEntityType();

                        gReport.RowItemText = modelType.Name + " Tag";
                        lTaggedTitle.Text = "Tagged " + modelType.Name.Pluralize();

                        if ( modelType != null )
                        {
                            // If displaying people, set the person id fiels so that merge and communication icons are displayed
                            if ( TagEntityType.Name == "Rock.Model.Person" )
                            {
                                gReport.PersonIdField = "Id";
                            }

                            foreach ( var column in gReport.GetPreviewColumns( modelType ) )
                            {
                                gReport.Columns.Add( column );
                            }

                            // Add a CreatedDateTime if one does not exist
                            var gridBoundColumns = gReport.Columns.OfType<BoundField>();
                            if ( gridBoundColumns.Any( c => c.DataField.Equals( "CreatedDateTime" ) ) == false )
                            {
                                BoundField addedDateTime = new DateField();
                                addedDateTime.DataField = "CreatedDateTime";
                                addedDateTime.SortExpression = "CreatedDateTime";
                                addedDateTime.HeaderText = "Date Tagged";
                                gReport.Columns.Add( addedDateTime );
                            }

                            // Add delete column
                            var deleteField = new DeleteField();
                            gReport.Columns.Add( deleteField );
                            deleteField.Click += gReport_Delete;

                            if ( !Page.IsPostBack )
                            {
                                BindGrid();
                            }
                        }
                    }
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
            int id = int.MinValue;
            if ( TagEntityType != null && int.TryParse( e.RowKeyValue.ToString(), out id ) )
            {
                string routePath = string.Format( "~/{0}/{1}", TagEntityType.FriendlyName.Replace( " ", "" ), id );
                Response.Redirect( routePath, false );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gReport_Delete( object sender, RowEventArgs e )
        {
            int id = int.MinValue;
            if ( TagId.HasValue && int.TryParse( e.RowKeyValue.ToString(), out id ) )
            {
                object obj = InvokeServiceMethod( "Get", new Type[] { typeof( int ) }, new object[] { id } );
                if ( obj != null )
                {
                    Rock.Data.IEntity entity = obj as Rock.Data.IEntity;
                    if ( entity != null )
                    {
                        var rockContext = new RockContext();
                        var service = new TaggedItemService( rockContext );
                        var taggedItem = service.Get( TagId.Value, entity.Guid );
                        if ( taggedItem != null )
                        {
                            string errorMessage;
                            if ( !service.CanDelete( taggedItem, out errorMessage ) )
                            {
                                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                                return;
                            }

                            service.Delete( taggedItem );
                            rockContext.SaveChanges();
                        }
                    }
                }
            }

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var guids = new TaggedItemService( new RockContext() ).Queryable().Where( t => t.TagId == TagId.Value )
                .Select( t => new { t.EntityGuid, t.CreatedDateTime } )
                .ToDictionary( o => o.EntityGuid, o => o.CreatedDateTime );

            var enumerable = InvokeServiceMethod( "GetListByGuids", new Type[] { typeof( List<Guid> ) }, new object[] { guids.Keys.ToList() } ) as System.Collections.IEnumerable;

            // Since we don't really know what is in the "obj" that was returned, we check if it's
            // enumerable then reuse the CreatedDateTime property of the DataSource if it has one.
            // In the future, perhaps consider creating a merged data source so the CreatedDateTime
            // property/column doesn't have to be hijacked.
            if ( enumerable != null )
            {
                if ( enumerable is IEnumerable<Person> )
                {
                    enumerable = ( enumerable as IEnumerable<Person> ).AsQueryable().Sort( gReport.SortProperty ?? new SortProperty { Property = "Id" } ).ToList();
                    gReport.AllowSorting = true;
                }
                else
                {
                    gReport.AllowSorting = false;
                }

                foreach ( var entity in enumerable )
                {
                    var property = entity.GetType().GetProperty( "CreatedDateTime" );
                    var guid = entity.GetType().GetProperty( "Guid" );
                    // Now re-set the CreatedDateTime with the tag's CreatedDateTime (if that property is in the entity)
                    if ( property != null && guid != null )
                    {
                        var val = (Guid)guid.GetValue( entity, null );
                        property.SetValue( entity, guids[val], null );
                    }
                }
            }

            gReport.DataSource = enumerable;

            gReport.DataBind();
        }

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        private object InvokeServiceMethod( string methodName, Type[] types, object[] parameters )
        {
            Type modelType = TagEntityType.GetEntityType();

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
                    MethodInfo method = serviceInstance.GetType().GetMethod( methodName, types );
                    if ( method != null )
                    {
                        return method.Invoke( serviceInstance, parameters );
                    }
                }
            }

            return null;
        }

        #endregion

    }
}