//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Block for viewing entities with a selected tag
    /// </summary>
    [Description( "Block for viewing entities with a selected tag" )]
    public partial class TagReport : Rock.Web.UI.RockBlock, IDimmableBlock
    {
        public string EntityTypeName
        {
            get { return ViewState["EntityTypeName"] as string ?? string.Empty; }
            set { ViewState["EntityTypeName"] = value; }
        }

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gReport.DataKeyNames = new string[] { "id" };
            gReport.GridRebind += gReport_GridRebind;
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
            string routePath = string.Format("~/{0}/{1}", EntityTypeName, e.RowKeyValue.ToString() );
            Response.Redirect( routePath, false );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            string tagId = PageParameter( "tagId" );
            if ( !string.IsNullOrWhiteSpace( tagId ) )
            {
                int id = int.MinValue;
                if ( int.TryParse( tagId, out id ) && id > 0 )
                {
                    using ( new Rock.Data.UnitOfWorkScope() )
                    {
                        Tag _tag = new TagService().Get( id );
                        if ( _tag != null )
                        {
                            var entityTypeCache = EntityTypeCache.Read( _tag.EntityTypeId );
                            if ( entityTypeCache != null )
                            {
                                EntityTypeName = entityTypeCache.FriendlyName.Replace( " ", "" );

                                Type entityType = entityTypeCache.GetEntityType();
                                if ( entityType != null )
                                {
                                    gReport.CreatePreviewColumns( entityType );

                                    if ( entityTypeCache.Name == "Rock.Model.Person" )
                                    {
                                        gReport.PersonIdField = "Id";
                                    }

                                    Type[] modelType = { entityType };
                                    Type genericServiceType = typeof( Rock.Data.Service<> );
                                    Type modelServiceType = genericServiceType.MakeGenericType( modelType );

                                    object serviceInstance = Activator.CreateInstance( modelServiceType );

                                    var guids = _tag.TaggedItems.Select( t => t.EntityGuid ).ToList();
                                    if ( serviceInstance != null )
                                    {
                                        Type[] guidType = { typeof( Guid ) };
                                        Type genericList = typeof( List<> );
                                        Type guidList = genericList.MakeGenericType( guidType );

                                        MethodInfo getMethod = serviceInstance.GetType().GetMethod( "GetByGuids", new Type[] { guidList } );
                                        if ( getMethod != null )
                                        {
                                            gReport.DataSource = getMethod.Invoke( serviceInstance, new object[] { guids } );
                                            gReport.DataBind();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the dimmed.
        /// </summary>
        /// <param name="dimmed">if set to <c>true</c> [dimmed].</param>
        public void SetDimmed( bool dimmed )
        {
            gReport.Enabled = !dimmed;
        }
        
        #endregion

}
}