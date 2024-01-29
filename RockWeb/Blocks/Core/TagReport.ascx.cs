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
    /// </summary>
    [DisplayName( "Tag Report" )]
    [Category( "Core" )]
    [Description( "Block for viewing entities with a selected tag" )]
    [Rock.SystemGuid.BlockTypeGuid( "005E5980-E2D2-4958-ACB6-BECBC6D1F5C4" )]
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
            gReport.EntityIdField = "Id";
            gReport.Actions.ShowAdd = false;
            gReport.GridRebind += gReport_GridRebind;
            gReport.Actions.AddClick += gReport_AddClick;

            TagId = PageParameter( "TagId" ).AsIntegerOrNull();
            if ( TagId.HasValue && TagId.Value > 0 )
            {
                Tag _tag = new TagService( new RockContext() ).Get( TagId.Value );
                if ( _tag != null && _tag.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    pnlGrid.Visible = true;
                    lTaggedTitle.Text = "Tagged Items";

                    TagEntityType = EntityTypeCache.Get( _tag.EntityTypeId ?? 0 );
                    if ( TagEntityType != null )
                    {
                        if ( TagEntityType.Name == "Rock.Model.Person" )
                        {
                            gReport.ColumnsOfType<SelectField>().First().Visible = true;
                            // The order of the DataKeyNames is important here, the grid uses the first value as the identifier
                            // and so placing the EntityId first means when performing a Communication action the PersonId will be used,
                            // and the gReport_RowSelected and gReport_Delete event handlers also use the second indexed value as the Identifier, thus
                            // ensuring the actual TaggedItemId is used when a row item is selected.
                            gReport.DataKeyNames = new string[] { "EntityId", "Id" };
                            gReport.Actions.ShowAdd = _tag.IsAuthorized( Rock.Security.Authorization.TAG, CurrentPerson );
                        }

                        var entityType = TagEntityType.GetEntityType();
                        if ( entityType != null )
                        {
                            lTaggedTitle.Text = "Tagged " + entityType.Name.Pluralize().SplitCase();
                            gReport.ColumnsOfType<RockTemplateField>().First( c => c.HeaderText == "Item" ).HeaderText = entityType.Name.SplitCase();
                        }
                    }

                    gReport.ColumnsOfType<DeleteField>().First().Visible = _tag.IsAuthorized( Rock.Security.Authorization.TAG, CurrentPerson );

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
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var personPickerStartupScript = @"Sys.Application.add_load(function () {

                // if the person picker is empty then open it for quick entry
                var personPicker = $('.js-newperson');
                var currentPerson = personPicker.find('.picker-selectedperson').html();
                if (currentPerson != null && currentPerson.length == 0) {
                    $(personPicker).find('a.picker-label').trigger('click');
                }

            });";

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), "StartupScript", personPickerStartupScript, true );
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gReport_GridRebind( object sender, EventArgs e )
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
                var id = e.RowKeyId;
                if ( e.RowKeyValues != null )
                {
                    // If multiple RowKeyValues are in use, the second value represents the actual TaggedItemId.
                    id = e.RowKeyValues.Count > 1 ? ( int ) e.RowKeyValues[1] : e.RowKeyId;
                }

                var taggedItem = new TaggedItemService( rockContext ).Get( id );
                if ( taggedItem != null )
                {
                    var entityType = EntityTypeCache.Get( taggedItem.EntityTypeId );
                    if ( entityType != null )
                    {
                        var entity = Reflection.GetIEntityForEntityType( entityType.GetEntityType(), taggedItem.EntityGuid );
                        if ( entity != null )
                        {
                            string url = string.Format( "~/{0}/{1}", entityType.FriendlyName.Replace( " ", "" ), entity.Id );
                            if ( entityType.LinkUrlLavaTemplate.IsNotNullOrWhiteSpace() )
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
                var id = e.RowKeyId;
                if ( e.RowKeyValues != null )
                {
                    // If multiple RowKeyValues are in use, the second value represents the actual TaggedItemId.
                    id = e.RowKeyValues.Count > 1 ? ( int ) e.RowKeyValues[1] : e.RowKeyId;
                }

                var taggedItemService = new TaggedItemService( rockContext );
                var taggedItem = taggedItemService.Get( id );
                if ( taggedItem != null && taggedItem.IsAuthorized( Rock.Security.Authorization.TAG, CurrentPerson ) )
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

        /// <summary>
        /// Handles the AddClick event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gReport_AddClick( object sender, EventArgs e )
        {
            nbAddPersonExists.Visible = false;
            mdAddPerson.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddPerson_SaveClick( object sender, EventArgs e )
        {
            if ( !AddPerson( ppNewPerson.SelectedValue.Value ) )
            {
                nbAddPersonExists.Visible = true;
                return;
            }

            mdAddPerson.Hide();
            BindGrid();
        }

        /// <summary>
        /// Handles the SaveThenAddClick event of the mdAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddPerson_SaveThenAddClick( object sender, EventArgs e )
        {
            if ( !AddPerson( ppNewPerson.SelectedValue.Value ) )
            {
                nbAddPersonExists.Visible = true;
                return;
            }

            BindGrid();
            ppNewPerson.SetValue( null );
            nbAddPersonExists.Visible = false;
        }

        /// <summary>
        /// Add the person to the tag.
        /// </summary>
        /// <returns>True if the person was added, false if they already existed in the tag.</returns>
        private bool AddPerson( int personId )
        {
            using ( var rockContext = new RockContext() )
            {
                var taggedItemService = new TaggedItemService( rockContext );
                Tag tag = new TagService( rockContext ).Get( TagId.Value );
                var person = new PersonService( rockContext ).Get( personId );

                if ( taggedItemService.Get( tag.Id, person.Guid ) != null )
                {
                    return false;
                }

                var taggedItem = new TaggedItem();
                taggedItem.TagId = TagId.Value;
                taggedItem.EntityTypeId = TagEntityType.Id;
                taggedItem.EntityGuid = person.Guid;
                taggedItemService.Add( taggedItem );

                rockContext.SaveChanges();

                return true;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( TagEntityType == null )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var service = new TaggedItemService( rockContext );

                var tagEntityTypeType = TagEntityType.GetEntityType();

                // Join TaggedItemRow with whatever IEntity query is for the TagEntityType (PersonService.Queryable(), GroupService.Queryable(), etc)
                // That way we can get the Entity.Id and tell the Grid what the EntityId is for each item (Person, Group, etc)
                IService serviceInstance = Reflection.GetServiceForEntityType( tagEntityTypeType, rockContext );
                MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                var entityQuery = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                IQueryable<TaggedItemRow> results = service.Queryable().AsNoTracking()
                    .Where( t => t.TagId == TagId.Value )
                        .Join( entityQuery, t => t.EntityGuid, e => e.Guid, ( t, e ) => new TaggedItemRow
                        {
                            Id = t.Id,
                            EntityTypeId = t.EntityTypeId,
                            EntityGuid = t.EntityGuid,
                            CreatedDateTime = t.CreatedDateTime,
                            EntityId = e.Id,
                        } );

                var sortProperty = gReport.SortProperty;
                if ( gReport.AllowSorting && sortProperty != null )
                {
                    results = results.Sort( sortProperty );
                }

                // Tell the grid that it has a list of the EntityType for the Tag (Person, Group, etc).
                // Also tell it to get the Entities (Group, Person, etc) using EntityId (instead of Id)
                gReport.EntityTypeId = TagEntityType.Id;
                gReport.EntityIdField = "EntityId";
                if ( TagEntityType.Name == "Rock.Model.Person" )
                {
                    gReport.PersonIdField = "EntityId";
                }

                gReport.DataSource = results.ToList();
                gReport.DataBind();
            }
        }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <returns></returns>
        public string GetItemName( int entityTypeId, Guid entityGuid )
        {
            var entityType = EntityTypeCache.Get( entityTypeId );
            if ( entityType != null )
            {
                var entity = Reflection.GetIEntityForEntityType( entityType.GetEntityType(), entityGuid );
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

        #endregion

        private class TaggedItemRow
        {
            public int Id { get; set; }

            public int EntityTypeId { get; set; }

            public Guid EntityGuid { get; set; }

            public int EntityId { get; set; }

            public DateTime? CreatedDateTime { get; set; }
        }
    }
}