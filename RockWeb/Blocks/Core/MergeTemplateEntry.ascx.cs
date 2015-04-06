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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Merge Template Entry" )]
    [Category( "Core" )]
    [Description( "Used for merging data into output documents, such as Word, Html, using a pre-defined template." )]
    public partial class MergeTemplateEntry : RockBlock
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
                mtPicker.MergeTemplateOwnership = MergeTemplateOwnership.PersonalAndGlobal;

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

            if ( entitySet.EntityTypeId.HasValue )
            {
                var entityTypeCache = EntityTypeCache.Read( entitySet.EntityTypeId.Value );
                bool isPersonEntitySet = entityTypeCache.Guid == Rock.SystemGuid.EntityType.PERSON.AsGuid();
                cbCombineFamilyMembers.Visible = isPersonEntitySet;
            }
            else
            {
                cbCombineFamilyMembers.Visible = false;
            }

            int itemsCount = entitySetItemsService.Queryable().Where( a => a.EntitySetId == entitySetId ).Count();

            nbNumberOfRecords.Text = string.Format( "There are {0} {1} to merge", itemsCount, "row".PluralizeIf( itemsCount != 1 ) );
        }

        /// <summary>
        /// Handles the Click event of the btnMerge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMerge_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            List<object> mergeObjectsList = GetMergeObjectList( rockContext );

            MergeTemplate mergeTemplate = new MergeTemplateService( rockContext ).Get( mtPicker.SelectedValue.AsInteger() );
            if ( mergeTemplate == null )
            {
                nbWarningMessage.Text = "Unable to get merge template";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            MergeTemplateType mergeTemplateType = this.GetMergeTemplateType( rockContext, mergeTemplate );
            if ( mergeTemplateType == null )
            {
                nbWarningMessage.Text = "Unable to get merge template type";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            var globalMergeFields = GlobalAttributesCache.GetMergeFields( this.CurrentPerson );
            globalMergeFields.Add( "CurrentPerson", this.CurrentPerson );

            var outputBinaryFileDoc = mergeTemplateType.CreateDocument( mergeTemplate, mergeObjectsList, globalMergeFields );

            if ( mergeTemplateType.Exceptions != null && mergeTemplateType.Exceptions.Any() )
            {
                if ( mergeTemplateType.Exceptions.Count == 1 )
                {
                    this.LogException( mergeTemplateType.Exceptions[0] );
                }
                else if ( mergeTemplateType.Exceptions.Count > 50 )
                {
                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions for top 50.", mergeTemplate.Name ), mergeTemplateType.Exceptions.Take( 50 ).ToList() ) );
                }
                else
                {
                    this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions", mergeTemplate.Name ), mergeTemplateType.Exceptions.ToList() ) );
                }
            }

            var uri = new UriBuilder( outputBinaryFileDoc.Url );
            var qry = System.Web.HttpUtility.ParseQueryString( uri.Query );
            qry["attachment"] = true.ToTrueFalse();
            uri.Query = qry.ToString();
            Response.Redirect( uri.ToString(), false );
            Context.ApplicationInstance.CompleteRequest();

            return;
        }

        /// <summary>
        /// Gets the type of the merge template.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <returns></returns>
        private MergeTemplateType GetMergeTemplateType( RockContext rockContext, MergeTemplate mergeTemplate )
        {
            mergeTemplate = new MergeTemplateService( rockContext ).Get( mtPicker.SelectedValue.AsInteger() );
            if ( mergeTemplate == null )
            {
                return null;
            }

            var mergeTemplateTypeEntityType = EntityTypeCache.Read( mergeTemplate.MergeTemplateTypeEntityTypeId );
            if ( mergeTemplateTypeEntityType == null )
            {
                return null;
            }

            return MergeTemplateTypeContainer.GetComponent( mergeTemplateTypeEntityType.Name );
        }

        /// <summary>
        /// Gets the merge object list for the current EntitySet
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="fetchCount">The fetch count.</param>
        /// <returns></returns>
        private List<object> GetMergeObjectList( RockContext rockContext, int? fetchCount = null )
        {
            int entitySetId = hfEntitySetId.Value.AsInteger();
            var entitySetService = new EntitySetService( rockContext );
            var entitySet = entitySetService.Get( entitySetId );
            Dictionary<int, object> mergeObjectsDictionary = new Dictionary<int, object>();

            // If this EntitySet contains IEntity Items, add those first
            if ( entitySet.EntityTypeId.HasValue )
            {
                var qryEntity = entitySetService.GetEntityQuery( entitySetId );

                if ( fetchCount.HasValue )
                {
                    qryEntity = qryEntity.Take( fetchCount.Value );
                }

                var entityTypeCache = EntityTypeCache.Read( entitySet.EntityTypeId.Value );
                bool isPersonEntityType = entityTypeCache != null && entityTypeCache.Guid == Rock.SystemGuid.EntityType.PERSON.AsGuid();
                bool combineFamilyMembers = cbCombineFamilyMembers.Visible && cbCombineFamilyMembers.Checked;

                if ( isPersonEntityType && combineFamilyMembers )
                {
                    var qryPersons = qryEntity;
                    Guid familyGroupType = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
                    var qryFamilyGroupMembers = new GroupMemberService( rockContext ).Queryable()
                        .Where( a => a.Group.GroupType.Guid == familyGroupType )
                        .Where( a => qryPersons.Any( aa => aa.Id == a.PersonId ) );

                    var qryCombined = qryFamilyGroupMembers.Join(
                        qryPersons,
                        m => m.PersonId,
                        p => p.Id,
                        ( m, p ) => new { GroupMember = m, Person = p } )
                        .GroupBy( a => a.GroupMember.GroupId )
                        .Select( x => new
                        {
                            GroupId = x.Key,
                            Persons = x.Select( xx => xx.Person )
                        } );

                    foreach ( var combinedFamilyItem in qryCombined )
                    {
                        object mergeObject;

                        string commaPersonIds = combinedFamilyItem.Persons.Select( a => a.Id ).Distinct().ToList().AsDelimited( "," );

                        var primaryGroupPerson = combinedFamilyItem.Persons.FirstOrDefault() as Person;

                        if ( mergeObjectsDictionary.ContainsKey( primaryGroupPerson.Id ) )
                        {
                            foreach ( var person in combinedFamilyItem.Persons )
                            {
                                if ( !mergeObjectsDictionary.ContainsKey( person.Id ) )
                                {
                                    primaryGroupPerson = person as Person;
                                    break;
                                }
                            }
                        }

                        if ( combinedFamilyItem.Persons.Count() > 1 )
                        {
                            var combinedPerson = primaryGroupPerson.ToJson().FromJsonOrNull<MergeTemplateCombinedPerson>();

                            var familyTitle = RockUdfHelper.ufnCrm_GetFamilyTitle( rockContext, null, combinedFamilyItem.GroupId, commaPersonIds, true );
                            combinedPerson.FullName = familyTitle;

                            var firstNameList = combinedFamilyItem.Persons.Select( a => ( a as Person ).FirstName ).ToList();
                            var nickNameList = combinedFamilyItem.Persons.Select( a => ( a as Person ).NickName ).ToList();

                            combinedPerson.FirstName = firstNameList.AsDelimited( ", ", " & " );
                            combinedPerson.NickName = firstNameList.AsDelimited( ", ", " & " );
                            combinedPerson.LastName = primaryGroupPerson.LastName;
                            combinedPerson.SuffixValueId = null;
                            combinedPerson.SuffixValue = null;
                            mergeObject = combinedPerson;
                        }
                        else
                        {
                            mergeObject = primaryGroupPerson;
                        }

                        mergeObjectsDictionary.Add( primaryGroupPerson.Id, mergeObject );
                    }
                }
                else
                {
                    foreach ( var item in qryEntity.AsNoTracking() )
                    {
                        mergeObjectsDictionary.Add( item.Id, item );
                    }
                }
            }

            var entitySetItemService = new EntitySetItemService( rockContext );
            string[] emptyJson = new string[] { string.Empty, "{}" };
            var entitySetItemMergeValuesQry = entitySetItemService.GetByEntitySetId( entitySetId, true ).Where( a => !emptyJson.Contains( a.AdditionalMergeValuesJson ) );

            if ( fetchCount.HasValue )
            {
                entitySetItemMergeValuesQry = entitySetItemMergeValuesQry.Take( fetchCount.Value );
            }

            // now, add the additional MergeValues regardless of if the EntitySet contains IEntity items or just Non-IEntity items
            foreach ( var additionalMergeValuesItem in entitySetItemMergeValuesQry.AsNoTracking() )
            {
                object mergeObject;
                if ( mergeObjectsDictionary.ContainsKey( additionalMergeValuesItem.EntityId ) )
                {
                    mergeObject = mergeObjectsDictionary[additionalMergeValuesItem.EntityId];
                }
                else
                {
                    if ( entitySet.EntityTypeId.HasValue )
                    {
                        // if already have real entities in our list, don't add additional items to the mergeObjectsDictionary
                        continue;
                    }

                    mergeObject = new object();
                    mergeObjectsDictionary.Add( additionalMergeValuesItem.EntityId, mergeObject );
                }

                foreach ( var additionalMergeValue in additionalMergeValuesItem.AdditionalMergeValues )
                {
                    // if we have additionalMergeValues, convert the MergeObject into a Hash (instead of IEntity) and add the additional fields
                    DotLiquid.Hash mergeObjectHash;
                    if ( mergeObject is DotLiquid.Hash )
                    {
                        mergeObjectHash = mergeObject as DotLiquid.Hash;
                    }
                    else if ( mergeObject is IDictionary<string, object> )
                    {
                        mergeObjectHash = DotLiquid.Hash.FromDictionary( mergeObject as IDictionary<string, object> );
                    }
                    else if ( mergeObject is IEntity )
                    {
                        // convert the object to a Dictionary so we can add additional fields to it
                        mergeObjectHash = DotLiquid.Hash.FromDictionary( ( mergeObject as IEntity ).ToDictionary() );
                    }
                    else
                    {
                        // anonymous object with no fields yet
                        mergeObjectHash = new DotLiquid.Hash();
                    }

                    mergeObjectHash.AddOrIgnore( additionalMergeValue.Key, additionalMergeValue.Value );

                    // ensure the mergeObject is updated in case it was converted to a mergeObjectHash
                    mergeObjectsDictionary[additionalMergeValuesItem.EntityId] = mergeObjectHash;
                    mergeObject = mergeObjectsDictionary[additionalMergeValuesItem.EntityId];
                }
            }

            var result = mergeObjectsDictionary.Select( a => a.Value );
            if ( fetchCount.HasValue )
            {
                // make sure the result is limited to fetchCount (even though the above queries are also limited to fetch count)
                result = result.Take( fetchCount.Value );
            }

            return result.ToList();
        }

        /// <summary>
        /// Handles the Click event of the btnShowDataPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowDataPreview_Click( object sender, EventArgs e )
        {
            if ( pnlPreview.Visible )
            {
                pnlPreview.Visible = false;
                return;
            }

            var rockContext = new RockContext();

            int entitySetId = hfEntitySetId.Value.AsInteger();
            var entitySetService = new EntitySetService( rockContext );
            var entitySet = entitySetService.Get( entitySetId );
            if ( entitySet.EntityTypeId.HasValue )
            {
                var qry = entitySetService.GetEntityQuery( entitySetId ).Take( 15 );

                EntityTypeCache itemEntityType = EntityTypeCache.Read( entitySet.EntityTypeId ?? 0 );
                gPreview.CreatePreviewColumns( itemEntityType.GetEntityType() );

                gPreview.DataSource = qry.ToList();
                gPreview.DataBind();
            }
            else
            {
                var entitySetItemService = new EntitySetItemService( rockContext );
                var qry = entitySetItemService.GetByEntitySetId( entitySetId, true ).Take( 15 );
                var list = qry.ToList().Select( a => a.AdditionalMergeValuesJson.FromJsonOrNull<Dictionary<string, object>>() ).ToList();
                if ( list.Any() )
                {
                    gPreview.Columns.Clear();
                    foreach ( var s in list[0] )
                    {
                        var gridField = Grid.GetGridField( s.Value != null ? s.Value.GetType() : typeof( string ) );
                        gridField.HeaderText = s.Key.SplitCase();
                        gridField.DataField = s.Key;
                        gPreview.Columns.Add( gridField );
                    }

                    gPreview.DataSource = qry.ToList().Select( a => a.AdditionalMergeValuesJson.FromJsonOrNull<object>() ).ToList();
                    gPreview.DataBind();
                }
            }

            pnlPreview.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnShowMergeFieldsHelp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowMergeFieldsHelp_Click( object sender, EventArgs e )
        {
            pnlMergeFieldsHelp.Visible = !pnlMergeFieldsHelp.Visible;

            if ( pnlMergeFieldsHelp.Visible )
            {
                ShowLavaHelp();
            }
        }

        /// <summary>
        /// Shows the lava help.
        /// </summary>
        private void ShowLavaHelp()
        {
            var rockContext = new RockContext();
            List<object> mergeObjectsList = GetMergeObjectList( rockContext, 1 );
            var globalMergeFields = GlobalAttributesCache.GetMergeFields( this.CurrentPerson );
            globalMergeFields.Add( "CurrentPerson", this.CurrentPerson );

            MergeTemplate mergeTemplate = new MergeTemplateService( rockContext ).Get( mtPicker.SelectedValue.AsInteger() );
            MergeTemplateType mergeTemplateType = null;
            if ( mergeTemplate != null )
            {
                mergeTemplateType = this.GetMergeTemplateType( rockContext, mergeTemplate );
            }

            if ( mergeTemplateType != null )
            {
                // have the mergeTemplateType generate the help text
                lShowMergeFields.Text = mergeTemplateType.GetLavaDebugInfo( mergeObjectsList, globalMergeFields );
            }
            else
            {
                string preText = "<div class='alert alert-warning'>Select a Merge Template to see Merge Fields help for that template type.</div>";
                lShowMergeFields.Text = MergeTemplateType.GetDefaultLavaDebugInfo( mergeObjectsList, globalMergeFields, preText );
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the mtPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mtPicker_SelectItem( object sender, EventArgs e )
        {
            if ( pnlMergeFieldsHelp.Visible )
            {
                ShowLavaHelp();
            }
        }

        /// <summary>
        /// Special class that overrides Person so that FullName can be set (vs readonly/derived)
        /// The class is specifically for MergeTemplates
        /// </summary>
        public class MergeTemplateCombinedPerson : Person
        {
            /// <summary>
            /// Overide of FullName that should be set to whatever the FamilyTitle should be
            /// </summary>
            /// <value>
            /// A <see cref="System.String" /> representing the Family Title of a combined person
            /// </value>
            [DataMember]
            public new string FullName { get; set; }
        }

        #endregion
    }
}