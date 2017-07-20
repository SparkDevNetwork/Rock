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

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Tags By Letter" )]
    [Category( "Core" )]
    [Description( "Lists tags grouped by the first letter of the name with counts for people to select." )]

    [LinkedPage("Detail Page")]
    public partial class TagsByLetter : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        protected string personalTagsCss = string.Empty;
        protected string publicTagsCss = string.Empty;
        private string _tagCloudTab = string.Empty;
        private bool _showInActive = false;

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        protected override void OnInit( EventArgs e )
        {
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlTagCloud);

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            _tagCloudTab = Session["TagCloudTab"] as string;
            if ( !Page.IsPostBack )
            {
                if ( !string.IsNullOrWhiteSpace( _tagCloudTab ) && _tagCloudTab == "organization" )
                {
                    DisplayTags( null, 15 );
                    publicTagsCss = "active";
                }
                else
                {
                    DisplayTags( CurrentPerson.Id, 15 );
                    personalTagsCss = "active";
                }
            }

           
            _showInActive = tglStatus.Checked;

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
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {

        }

        protected void lbPersonalTags_Click(object sender, EventArgs e)
        {
            DisplayTags(CurrentPerson.Id, 15);
            personalTagsCss = "active";
            publicTagsCss = string.Empty;

            Session["TagCloudTab"] = "personal";

        }
        protected void lbPublicTags_Click(object sender, EventArgs e)
        {
            DisplayTags(null, 15);
            personalTagsCss = string.Empty;
            publicTagsCss = "active";

            Session["TagCloudTab"] = "organization";
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tgl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tgl_CheckedChanged( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( _tagCloudTab ) && _tagCloudTab == "organization" )
            {
                DisplayTags( null, 15 );
                publicTagsCss = "active";
            }
            else
            {
                DisplayTags( CurrentPerson.Id, 15 );
                personalTagsCss = "active";
            }
        }

        #endregion

        #region Methods

        private void DisplayTags(int? ownerId, int entityId)
        {
            // get tags
            var qry = new TagService( new RockContext() )
                .Queryable()
                .Where(t =>
                     t.EntityTypeId == entityId &&
                     (
                         ( t.OwnerPersonAlias == null && !ownerId.HasValue ) ||
                         ( t.OwnerPersonAlias != null && ownerId.HasValue && t.OwnerPersonAlias.PersonId == ownerId.Value )
                     ) &&
                     ( t.IsActive || _showInActive ) )
                .Select(t => new
                {
                    Tag = t,
                    Count = t.TaggedItems.Count()
                })
                .OrderBy(t => t.Tag.Name);

            // create dictionary to group by first letter of name
            Dictionary<char, List<TagSummary>> tagAlphabit = new Dictionary<char, List<TagSummary>>();

            // load alphabit into dictionary
            for (char c = 'A'; c <= 'Z'; c++)
            {
                tagAlphabit.Add(c, new List<TagSummary>());
            }

            tagAlphabit.Add('#', new List<TagSummary>());
            tagAlphabit.Add( '*', new List<TagSummary>() );

            // load tags
            var tags = qry.ToList();

            foreach ( var tagInfo in tags )
            {
                bool canView = ownerId.HasValue && tagInfo.Tag.OwnerPersonAlias.PersonId == ownerId.Value;
                canView = canView || tagInfo.Tag.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson );
                if ( canView )
                {
                    var tagSummary = new TagSummary { Id = tagInfo.Tag.Id, Name = tagInfo.Tag.Name, Count = tagInfo.Count };
                    char key = (char)tagSummary.Name.Substring( 0, 1 ).ToUpper()[0];

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

            letterOutput.Append("<ul class='list-inline tag-letterlist'>");
            tagOutput.Append("<ul class='list-unstyled taglist'>");
            foreach (var letterItem in tagAlphabit)
            {
                if (letterItem.Value.Count > 0)
                {
                    letterOutput.Append(String.Format("<li><a href='#{0}'>{0}</a></li>", letterItem.Key.ToString()));

                    // add tags to display
                    tagOutput.Append(String.Format("<li class='clearfix'><h3><a name='{0}'></a>{1}</h3><ul class='list-inline'>", letterItem.Key.ToString(), letterItem.Key.ToString()));

                    foreach (TagSummary tag in letterItem.Value)
                    {
                        Dictionary<string, string> queryString = new Dictionary<string, string>();
                        queryString.Add("tagId", tag.Id.ToString());
                        var detailPageUrl = LinkedPageUrl("DetailPage", queryString);

                        tagOutput.Append(string.Format("<a href='{0}'><span class='tag'>{1} <small>({2})</small></span></a>", detailPageUrl, tag.Name, tag.Count));
                    }

                    tagOutput.Append("</ul></li>");
                }
                else
                {
                    letterOutput.Append(String.Format("<li>{0}</li>", letterItem.Key.ToString()));
                }


            }

            tagOutput.Append("</ul>");
            letterOutput.Append("</ul>");

            // if no tags add message instead
            if ( tags.Count() == 0 )
            {
                tagOutput.Clear();
                tagOutput.Append("<div class='alert alert-info'><h4>Note</h4>No personal tags exist.</div>");
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