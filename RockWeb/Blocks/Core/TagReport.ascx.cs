// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

            gReport.DataKeyNames = new string[] { "Guid" };
            gReport.GridRebind += gReport_GridRebind;

            int tagId = int.MinValue;
            if ( int.TryParse( PageParameter( "tagId" ), out tagId ) && tagId > 0 )
            {
                Tag _tag = new TagService().Get( tagId );

                if ( _tag != null )
                {
                    TagId = tagId;
                    TagEntityType = EntityTypeCache.Read( _tag.EntityTypeId );

                    if ( TagEntityType != null )
                    {
                        Type modelType = TagEntityType.GetEntityType();

                        lTaggedTitle.Text = "Tagged " + modelType.Name.Pluralize();

                        if ( modelType != null )
                        {
                            Dictionary<string, BoundField> boundFields = gReport.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
                            foreach ( var column in gReport.GetPreviewColumns( modelType ) )
                            {
                                int insertPos = gReport.Columns.IndexOf( gReport.Columns.OfType<DeleteField>().First() );
                                gReport.Columns.Insert( insertPos, column );
                            }

                            if ( TagEntityType.Name == "Rock.Model.Person" )
                            {
                                gReport.PersonIdField = "Id";
                            }

                            BindGrid();
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
            Guid guid = Guid.Empty;
            if ( TagEntityType != null && Guid.TryParse( e.RowKeyValue.ToString(), out guid ) )
            {
                object entity = InvokeServiceMethod( "Get", new Type[] { typeof( Guid ) }, new object[] { guid } );
                if ( entity != null )
                {
                    Rock.Data.IEntity model = entity as Rock.Data.IEntity;
                    if ( model != null )
                    {
                        string routePath = string.Format( "~/{0}/{1}", TagEntityType.FriendlyName.Replace( " ", "" ), model.Id );
                        Response.Redirect( routePath, false );
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
            Guid guid = Guid.Empty;
            if ( TagId.HasValue && Guid.TryParse( e.RowKeyValue.ToString(), out guid ) )
            {
                var service = new TaggedItemService();
                var taggedItem = service.Get( TagId.Value, guid );
                if ( taggedItem != null )
                {
                    string errorMessage;
                    if ( !service.CanDelete( taggedItem, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    service.Delete( taggedItem, CurrentPersonAlias );
                    service.Save( taggedItem, CurrentPersonAlias );
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
            var guids = new TaggedItemService().Queryable().Where( t => t.TagId == TagId.Value ).Select( t => t.EntityGuid ).ToList();

            gReport.DataSource = InvokeServiceMethod( "GetByGuids", new Type[] { typeof( List<Guid> ) }, new object[] { guids } );
            gReport.DataBind();
        }

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible)
        {
            pnlContent.Visible = visible;
        }
        
        private object InvokeServiceMethod(string methodName, Type[] types, object[] parameters)
        {
            Type modelType = TagEntityType.GetEntityType();

            // Get the context type since this may be for a non-rock core object
            Type contextType = null;
            var contexts = Rock.Reflection.SearchAssembly( modelType.Assembly, typeof( System.Data.Entity.DbContext ) );
            if ( contexts.Any() )
            {
                contextType = contexts.First().Value;
            }

            Type genericServiceType = typeof( Rock.Data.Service<> );
            Type modelServiceType = genericServiceType.MakeGenericType( new Type[] { modelType } );

            if ( modelServiceType != null )
            {
                Object serviceInstance = null;

                if ( contextType != null )
                {
                    var context = Activator.CreateInstance( contextType );
                    serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { context } );
                }
                else
                {
                    serviceInstance = Activator.CreateInstance( modelServiceType );
                }

                if ( serviceInstance != null )
                {
                    MethodInfo method = serviceInstance.GetType().GetMethod( methodName, types );
                    if (method != null)
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