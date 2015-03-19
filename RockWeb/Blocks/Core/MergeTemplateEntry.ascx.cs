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
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Merge Template Entry" )]
    [Category( "Core" )]
    [Description( "Used for merging data into output documents, such as Word, Html, using a pre-defined template." )]
    public partial class MergeTemplateEntry : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
                int? entitySetId = this.PageParameter( "Set" ).AsIntegerOrNull();
                pnlEntry.Visible = entitySetId.HasValue;

                if ( entitySetId.HasValue )
                {
                    ShowMergeForEntitySetId( entitySetId.Value );
                }
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
            //
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the merge for entity set identifier.
        /// </summary>
        /// <param name="entitySetId">The entity set identifier.</param>
        protected void ShowMergeForEntitySetId( int entitySetId )
        {
            hfEntitySetId.Value = entitySetId.ToString();
            var rockContext = new RockContext();
            var entitySetService = new EntitySetService( rockContext );
            var entitySetItemsService = new EntitySetItemService( rockContext );
            var entitySet = entitySetService.Get( entitySetId );
            if ( entitySet == null )
            {
                nbWarningMessage.Text = "Merge Records not found";
                nbWarningMessage.Title = "Warning";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Warning;
                pnlEntry.Visible = false;
                return;
            }

            if ( !entitySet.EntityTypeId.HasValue )
            {
                nbWarningMessage.Text = "Unable to determine Entity Type";
                nbWarningMessage.Title = "Error";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                pnlEntry.Visible = false;
                return;
            }

            var entityTypeCache = EntityTypeCache.Read( entitySet.EntityTypeId.Value );
            bool isPersonEntitySet = entityTypeCache.Guid == Rock.SystemGuid.EntityType.PERSON.AsGuid();
            cbCombineFamilyMembers.Visible = isPersonEntitySet;

            int itemsCount = entitySetItemsService.Queryable().Where( a => a.EntitySetId == entitySetId ).Count();

            nbNumberOfRecords.Text = string.Format( "There are {0} {1} to merge", itemsCount, entityTypeCache.FriendlyName.PluralizeIf( itemsCount != 1 ) );

            LoadDropDowns();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        protected void LoadDropDowns()
        {
            var rockContext = new RockContext();
            var mergeTemplateService = new MergeTemplateService( rockContext );
            var list = mergeTemplateService.Queryable().OrderBy( a => a.Name ).ToList();
            ddlMergeTemplate.Items.Clear();
            ddlMergeTemplate.Items.Add( new ListItem() );

            foreach ( var item in list )
            {
                ddlMergeTemplate.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMerge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMerge_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var mergeTemplateService = new MergeTemplateService( rockContext );

            var mergeTemplate = mergeTemplateService.Get( ddlMergeTemplate.SelectedValue.AsInteger() );
            if ( mergeTemplate != null )
            {
                List<IEntity> itemList = null;
                var providerEntityType = EntityTypeCache.Read( mergeTemplate.MergeTemplateProviderEntityTypeId );
                if ( providerEntityType != null )
                {
                    var mergeTemplateProvider = MergeTemplateProviderContainer.GetComponent( providerEntityType.Name );

                    if ( mergeTemplateProvider != null )
                    {
                        var entitySetService = new EntitySetService( rockContext );
                        int entitySetId = hfEntitySetId.Value.AsInteger();
                        var entitySet = entitySetService.Get( entitySetId );

                        var entitySetItemsService = new EntitySetItemService( rockContext );
                        var entityItemQry = entitySetItemsService.Queryable().Where( a => a.EntitySetId == entitySetId ).OrderBy( a => a.Order );

                        var itemEntityType = EntityTypeCache.Read( entitySet.EntityTypeId.Value );
                        bool isPersonEntitySet = itemEntityType.Guid == Rock.SystemGuid.EntityType.PERSON.AsGuid();
                        bool combineFamilyMembers = cbCombineFamilyMembers.Visible && cbCombineFamilyMembers.Checked;
                        List<Dictionary<string, object>> mergeObjectsList = new List<Dictionary<string, object>>();

                        if ( itemEntityType.AssemblyName != null )
                        {
                            Type entityType = itemEntityType.GetEntityType();
                            if ( entityType != null )
                            {
                                Type[] modelType = { entityType };
                                Type genericServiceType = typeof( Rock.Data.Service<> );
                                Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                                Rock.Data.IService serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;

                                MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                                var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                                var joinQry = entityItemQry.Join( entityQry, k => k.EntityId, i => i.Id, ( setItem, item ) => item );

                                itemList = joinQry.ToList();
                            }
                        }

                        if ( itemList != null )
                        {
                            var globalMergeObjects = GlobalAttributesCache.GetMergeFields( this.CurrentPerson );
                            foreach ( var item in itemList.OfType<Rock.Data.IModel>() )
                            {
                                var mergeObjects = new Dictionary<string, object>();
                                foreach ( var g in globalMergeObjects )
                                {
                                    mergeObjects.Add( g.Key, g.Value );
                                }

                                mergeObjects.Add( "CurrentPerson", this.CurrentPerson );
                                mergeObjects.Add( itemEntityType.FriendlyName.Replace( " ", string.Empty ), item );
                                mergeObjectsList.Add( mergeObjects );
                            }

                            var outputBinaryFileDoc = mergeTemplateProvider.CreateDocument( mergeTemplate, mergeObjectsList );

                            Response.Redirect( outputBinaryFileDoc.Url, false );
                            Context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                    }
                }

                nbWarningMessage.Text = "Something went wrong";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }
        }

        #endregion
    }
}