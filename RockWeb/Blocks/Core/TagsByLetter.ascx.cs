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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.HtmlControls;
using Rock.Security;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Tags By Letter" )]
    [Category( "Core" )]
    [Description( "Lists tags grouped by the first letter of the name with counts for people to select." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    [BooleanField( "User-Selectable Entity Type",
        Description = "Should user be able to select the entity type to show tags for?",
        DefaultValue = "true",
        Order = 0,
        Key = AttributeKey.ShowEntityType )]

    [EntityTypeField( "Entity Type",
        IncludeGlobalAttributeOption = false,
        Description = "The entity type to display tags for. If entity type is user-selectable, this will be the default entity type",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.EntityType.PERSON,
        Order = 1,
        Key = AttributeKey.EntityType )]

    public partial class TagsByLetter : Rock.Web.UI.RockBlock
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ShowEntityType = "ShowEntityType";
            public const string EntityType = "EntityType";
        }

        #region Fields

        // used for private variables
        protected string personalTagsCss = string.Empty;
        protected string publicTagsCss = string.Empty;
        private bool _showEntityType = false;

        #endregion

        #region Properties

        public int? EntityTypeId
        {
            get { return Session["EntityTypeId"] as int?; }
            set { Session["EntityTypeId"] = value; }
        }

        public string TagCloudTab
        {
            get { return Session["TagCloudTab"] as string ?? "personal"; }
            set { Session["TagCloudTab"] = value; }
        }

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        protected override void OnInit( EventArgs e )
        {
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlTagCloud );

            base.OnInit( e );

            _showEntityType = GetAttributeValue( AttributeKey.ShowEntityType ).AsBoolean();

            if ( !EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( GetAttributeValue( AttributeKey.EntityType ).AsGuid() );
                EntityTypeId = entityType != null ? entityType.Id : ( int? ) null;
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowControls();
                DisplayTags();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the TagsByLetter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowControls();
            DisplayTags();
        }

        protected void ddlEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            EntityTypeId = ddlEntityType.SelectedValueAsInt() ?? 0;
            DisplayTags();
        }

        protected void lbPersonalTags_Click( object sender, EventArgs e )
        {
            TagCloudTab = "personal";
            DisplayTags();
        }

        protected void lbPublicTags_Click( object sender, EventArgs e )
        {
            TagCloudTab = "organization";
            DisplayTags();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tgl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tgl_CheckedChanged( object sender, EventArgs e )
        {
            DisplayTags();
        }

        #endregion

        #region Methods

        private void ShowControls()
        {
            ddlEntityType.Visible = _showEntityType;
            if ( _showEntityType )
            {
                ddlEntityType.Items.Clear();
                ddlEntityType.Items.Add( new System.Web.UI.WebControls.ListItem() );
                new EntityTypeService( new RockContext() ).GetEntityListItems().ForEach( l => ddlEntityType.Items.Add( l ) );
                ddlEntityType.SetValue( EntityTypeId.HasValue && EntityTypeId.Value != 0 ? EntityTypeId.Value.ToString() : "" );
            }
        }

        private void DisplayTags()
        {
            int? ownerId = null;

            if ( TagCloudTab == "organization" )
            {
                publicTagsCss = "active";
                personalTagsCss = "";
            }
            else
            {
                ownerId = CurrentPersonId ?? 0;
                publicTagsCss = "";
                personalTagsCss = "active";
            }

            // get tags
            var tagQry = new TagService( new RockContext() ).Queryable();

            int? entityId = _showEntityType ? ddlEntityType.SelectedValueAsId() : EntityTypeId;
            if ( entityId.HasValue && entityId.Value != 0 )
            {
                tagQry = tagQry.Where( t => t.EntityTypeId.HasValue && t.EntityTypeId.Value == entityId.Value );
            }

            if ( !tglStatus.Checked )
            {
                tagQry = tagQry.Where( t => t.IsActive );
            }

            if ( ownerId.HasValue )
            {
                tagQry = tagQry.Where( t => t.OwnerPersonAlias != null && t.OwnerPersonAlias.PersonId == ownerId.Value );
            }

            var qry = tagQry.Select( t => new
            {
                Tag = t,
                Count = t.TaggedItems.Count()
            } )
                .OrderBy( t => t.Tag.Name );

            // create dictionary to group by first letter of name
            Dictionary<char, List<TagSummary>> tagAlphabit = new Dictionary<char, List<TagSummary>>();

            // load alphabit into dictionary
            for ( char c = 'A'; c <= 'Z'; c++ )
            {
                tagAlphabit.Add( c, new List<TagSummary>() );
            }

            tagAlphabit.Add( '#', new List<TagSummary>() );
            tagAlphabit.Add( '*', new List<TagSummary>() );

            // load tags
            var tags = qry.ToList();

            foreach ( var tagInfo in tags )
            {
                bool canView =
                    ( tagInfo.Tag.OwnerPersonAlias != null && ownerId.HasValue && tagInfo.Tag.OwnerPersonAlias.PersonId == ownerId.Value ) ||
                    ( tagInfo.Tag.OwnerPersonAlias == null && tagInfo.Tag.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) );

                if ( canView )
                {
                    var tagSummary = new TagSummary { Id = tagInfo.Tag.Id, Name = tagInfo.Tag.Name, Count = tagInfo.Count };
                    char key = ( char ) tagSummary.Name.Substring( 0, 1 ).ToUpper()[0];

                    if ( Char.IsNumber( key ) )
                    {
                        key = '#';
                    }
                    else
                    {
                        if ( !Char.IsLetter( key ) )
                        {
                            key = '*';
                        }
                    }

                    tagAlphabit[key].Add( tagSummary );
                }
            }

            // display tags
            StringBuilder tagOutput = new StringBuilder();
            StringBuilder letterOutput = new StringBuilder();

            letterOutput.Append( "<ul class='list-inline tag-letterlist'>" );
            tagOutput.Append( "<ul class='list-unstyled taglist'>" );
            foreach ( var letterItem in tagAlphabit )
            {
                if ( letterItem.Value.Count > 0 )
                {
                    letterOutput.Append( String.Format( "<li><a href='#{0}'>{0}</a></li>", letterItem.Key.ToString() ) );

                    // add tags to display
                    tagOutput.Append( String.Format( "<li class='clearfix'><h3><a name='{0}'></a>{1}</h3><ul class='list-inline'>", letterItem.Key.ToString(), letterItem.Key.ToString() ) );

                    foreach ( TagSummary tag in letterItem.Value )
                    {
                        Dictionary<string, string> queryString = new Dictionary<string, string>();
                        queryString.Add( "TagId", tag.Id.ToString() );
                        var detailPageUrl = LinkedPageUrl( AttributeKey.DetailPage, queryString );

                        tagOutput.Append( string.Format( "<a href='{0}'><span class='tag'>{1} <small>({2})</small></span></a>", detailPageUrl, tag.Name, tag.Count ) );
                    }

                    tagOutput.Append( "</ul></li>" );
                }
                else
                {
                    letterOutput.Append( String.Format( "<li>{0}</li>", letterItem.Key.ToString() ) );
                }


            }

            tagOutput.Append( "</ul>" );
            letterOutput.Append( "</ul>" );

            // if no tags add message instead
            if ( tags.Count() == 0 )
            {
                tagOutput.Clear();
                tagOutput.Append( string.Format( @"<div class='alert alert-info'><h4>Note</h4>No {0} tags exist.</div>", TagCloudTab ) );
            }

            lTagList.Text = tagOutput.ToString();
            lLetters.Text = letterOutput.ToString();
        }

        #endregion

    }

    class TagSummary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}