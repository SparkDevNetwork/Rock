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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:TagList runat=server></{0}:TagList>" )]
    public class TagList : TextBox
    {
        #region Properties

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int EntityTypeId
        {
            get { return ViewState["EntityTypeId"] as int? ?? 0; }
            set { ViewState["EntityTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the entity qualifier column.
        /// </summary>
        /// <value>
        /// The entity qualifier column.
        /// </value>
        public string EntityQualifierColumn
        {
            get { return ViewState["EntityQualifierColumn"] as string ?? string.Empty; }
            set { ViewState["EntityQualifierColumn"] = value; }
        }

        /// <summary>
        /// Gets or sets the entity qualifier value.
        /// </summary>
        /// <value>
        /// The entity qualifier value.
        /// </value>
        public string EntityQualifierValue
        {
            get { return ViewState["EntityQualifierValue"] as string ?? string.Empty; }
            set { ViewState["EntityQualifierValue"] = value; }
        }

        /// <summary>
        /// Gets or sets the category unique identifier.
        /// </summary>
        /// <value>
        /// The category unique identifier.
        /// </value>
        public Guid? CategoryGuid
        {
            get { return ViewState["CategoryGuid"] as Guid?; }
            set { ViewState["CategoryGuid"] = value; }
        }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        public int? CategoryId
        {
            get
            {
                if ( CategoryGuid.HasValue )
                {
                    var cat = CategoryCache.Get( CategoryGuid.Value );
                    return cat != null ? cat.Id : (int?)null;
                }
                return null;
            }
            set
            {
                if ( value.HasValue )
                {
                    var cat = CategoryCache.Get( CategoryGuid.Value );
                    CategoryGuid = cat != null ? cat.Guid : (Guid?)null;
                }
                else
                {
                    CategoryGuid = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity GUID.
        /// </summary>
        /// <value>
        /// The entity GUID.
        /// </value>
        public Guid EntityGuid
        {
            get
            {
                string entityGuid = ViewState["EntityGuid"] as string;
                if ( !string.IsNullOrWhiteSpace( entityGuid ) )
                {
                    return new Guid( entityGuid );
                }
                return Guid.Empty;
            }
            set
            {
                ViewState["EntityGuid"] = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether user should be able to create new tags
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow new tags]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowNewTags
        {
            get { return ViewState["AllowNewTags"] as bool? ?? true; }
            set { ViewState["AllowNewTags"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether tags should not be created immediately.
        /// </summary>
        /// <value>
        ///   <c>true</c> if true, tags will not be created as they are entered by user.  
        ///   Instead the SaveTagValues() method will need to be called to save the tags
        /// </value>
        public bool DelaySave
        {
            get { return ViewState["DelaySave"] as bool? ?? false; }
            set { ViewState["DelaySave"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Inactive tags should be displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Show InActive Tags]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInActiveTags
        {
            get { return ViewState["ShowInActiveTags"] as bool? ?? false; }
            set { ViewState["ShowInActiveTags"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddScriptLink( Page, "~/Scripts/jquery.tagsinput.js" );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                writer.AddAttribute( "class", "taglist " + this.CssClass );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( "class", "tag-wrap" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                string cssTemp = this.CssClass;
                this.CssClass = string.Empty;
                base.RenderControl( writer );
                this.CssClass = cssTemp;

                writer.RenderEndTag();

                writer.RenderEndTag();

                var script = string.Format( @"
Rock.controls.tagList.initialize({{
    controlId: '{0}',
    entityTypeId: '{1}',
    currentPersonId: '{2}',
    entityGuid: '{3}',
    entityQualifierColumn: '{4}',
    entityQualifierValue: '{5}',
    preventTagCreation: {6},
    delaySave: {7},
    categoryGuid: '{8}',
    includeInactive: {9}
}});",
                    this.ClientID,
                    EntityTypeId,
                    rockPage.CurrentPerson != null ? rockPage.CurrentPerson.Id.ToString() : "",
                    EntityGuid.ToString(),
                    string.IsNullOrWhiteSpace( EntityQualifierColumn ) ? string.Empty : EntityQualifierColumn,
                    string.IsNullOrWhiteSpace( EntityQualifierValue ) ? string.Empty : EntityQualifierValue,
                    ( !AllowNewTags ).ToString().ToLower(),
                    DelaySave.ToString().ToLower(),
                    CategoryGuid.HasValue ? CategoryGuid.Value.ToString() : "",
                    this.ShowInActiveTags.ToString().ToLower() );
                ScriptManager.RegisterStartupScript( this, this.GetType(), "tag_picker_" + this.ID, script, true );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the control with the current tags that exist for the current entity
        /// </summary>
        /// <param name="currentPersonId">The current person identifier.</param>
        public void GetTagValues( int? currentPersonId )
        {
            var sb = new StringBuilder();

            using ( var rockContext = new RockContext() )
            {
                var service = new TaggedItemService( rockContext );
                var qry = service.Get(
                    EntityTypeId, EntityQualifierColumn, EntityQualifierValue, currentPersonId, EntityGuid, CategoryGuid, ShowInActiveTags )
                    .Where( c => c.Tag.IsActive || ( ShowInActiveTags ) );

                var items = qry
                    .Select( a => a.Tag )
                    .OrderBy( a => a.Name );

                var person = GetCurrentPerson();

                foreach ( var item in items )
                {
                    if ( item.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                    {
                        if ( sb.Length > 0 )
                            sb.Append( ',' );
                        sb.Append( item.Name );
                        if ( currentPersonId.HasValue && item?.OwnerPersonAlias?.PersonId == currentPersonId.Value )
                            sb.Append( "^personal" );
                    }
                }
            }

            this.Text = sb.ToString();
        }

        private Person GetCurrentPerson()
        {
            var rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                return rockPage.CurrentPerson;
            }

            return null;
        }

        /// <summary>
        /// Saves the tag values that user entered for the entity
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        public void SaveTagValues( PersonAlias personAlias )
        {
            int? currentPersonId = null;
            if ( personAlias != null )
            {
                currentPersonId = personAlias.PersonId;
            }

            if ( EntityGuid != Guid.Empty )
            {
                var rockContext = new RockContext();
                var tagService = new TagService( rockContext );
                var taggedItemService = new TaggedItemService( rockContext );
                var person = currentPersonId.HasValue ? new PersonService( rockContext ).Get( currentPersonId.Value ) : null;

                // Get the existing tagged items for this entity
                var existingTaggedItems = new List<TaggedItem>();
                foreach ( var taggedItem in taggedItemService.Get( EntityTypeId, EntityQualifierColumn, EntityQualifierValue, currentPersonId, EntityGuid, CategoryGuid, ShowInActiveTags ) )
                {
                    if ( taggedItem.IsAuthorized( Authorization.VIEW, person ) )
                    {
                        existingTaggedItems.Add( taggedItem );
                    }
                }

                // Get tag values after user edit
                var currentTags = new List<Tag>();
                foreach ( var value in this.Text.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    string tagName = value;
                    if ( tagName.Contains( '^' ) )
                    {
                        tagName = tagName.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries )[0];
                    }

                    // Only if this is a new tag, create it
                    Tag tag = tagService.Get( EntityTypeId, EntityQualifierColumn, EntityQualifierValue, currentPersonId, tagName, CategoryGuid, ShowInActiveTags );
                    if ( personAlias != null && tag == null )
                    {
                        tag = new Tag();
                        tag.EntityTypeId = EntityTypeId;
                        tag.CategoryId = CategoryId;
                        tag.EntityTypeQualifierColumn = EntityQualifierColumn;
                        tag.EntityTypeQualifierValue = EntityQualifierValue;
                        tag.OwnerPersonAliasId = personAlias.Id;
                        tag.Name = tagName;
                        tagService.Add( tag );
                    }

                    if ( tag != null )
                    {
                        currentTags.Add( tag );
                    }
                }

                rockContext.SaveChanges();

                var currentNames = currentTags.Select( t => t.Name ).ToList();
                var existingNames = existingTaggedItems.Select( t => t.Tag.Name ).ToList();

                // Delete any tagged items that user removed
                foreach ( var taggedItem in existingTaggedItems )
                {
                    if ( !currentNames.Contains( taggedItem.Tag.Name, StringComparer.OrdinalIgnoreCase )  && taggedItem.IsAuthorized( Rock.Security.Authorization.TAG, person ) )
                    {
                        existingNames.Remove( taggedItem.Tag.Name );
                        taggedItemService.Delete( taggedItem );
                    }
                }
                rockContext.SaveChanges();

                // Add any tagged items that user added
                foreach ( var tag in currentTags )
                {
                    // If the tagged item was not already there, and (it's their personal tag OR they are authorized to use it) then add it.
                    if ( !existingNames.Contains( tag.Name, StringComparer.OrdinalIgnoreCase ) &&
                         (
                            ( tag.OwnerPersonAliasId != null && tag.OwnerPersonAliasId == personAlias?.Id ) ||
                            tag.IsAuthorized( Rock.Security.Authorization.TAG, person )
                         )
                       )
                    {
                        var taggedItem = new TaggedItem();
                        taggedItem.TagId = tag.Id;
                        taggedItem.EntityTypeId = this.EntityTypeId;
                        taggedItem.EntityGuid = EntityGuid;
                        taggedItemService.Add( taggedItem );
                    }
                }
                rockContext.SaveChanges();
            }
        }

        #endregion
    }
}