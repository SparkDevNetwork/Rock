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
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter people based on a person tag" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Has Tag Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "DD28F6C7-6571-4C6A-A021-13A5EB8BD216" )]
    public class TagFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return "Rock.Model.Person"; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string>();

            int entityTypePersonId = EntityTypeCache.GetId( typeof( Rock.Model.Person ) ) ?? 0;
            var tagQry = new TagService( rockContext ).Queryable( "OwnerPersonAlias" ).Where( a => a.EntityTypeId == entityTypePersonId );
            var personalTags = tagQry.Where( a => a.OwnerPersonAlias.PersonId == requestContext.CurrentPerson.Id )
                .OrderBy( a => a.Name )
                .Select( t => new ListItemBag { Text = t.Name, Value = t.Guid.ToString() } )
                .ToList();
            var orgTags = tagQry.Where( a => a.OwnerPersonAlias == null )
                .OrderBy( a => a.Name )
                .Select( t => new ListItemBag { Text = t.Name, Value = t.Guid.ToString() } )
                .ToList();

            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 2 )
            {
                var tagType = selectionValues[0];

                var selectedTagGuid = selectionValues[1];

                if ( tagType == "1" && !personalTags.Where( t => t.Value == selectedTagGuid ).Any() )
                {
                    // The selected tag belongs to someone else's list of personal tags, so we need to include it under a "Current" category
                    // so it can still appear on the list and file all the others as under the "Personal" category to keep them separate.
                    var selectedTag = new TagService( rockContext ).Get( selectedTagGuid.AsGuid() );

                    if ( selectedTag == null )
                    {
                        // I guess this tag doesn't actually exist, so no need to categorize
                    }
                    else
                    {
                        personalTags = personalTags.Select( t => new ListItemBag
                        {
                            Text = t.Text,
                            Value = t.Value,
                            Category = "Personal"
                        } ).ToList();
                        personalTags.Insert( 0, new ListItemBag
                        {
                            Text = string.Format( "{0} ( {1} )", selectedTag.Name, selectedTag.OwnerPersonAlias.Person ),
                            Value = selectedTag.Guid.ToString(),
                            Category = "Current"
                        } );
                    }
                }
            }

            var tagsByType = new Dictionary<string, List<ListItemBag>>
            {
                { "1", personalTags },
                { "2", orgTags },
            };

            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/Person/tagFilter.obs" ),
                Options = new Dictionary<string, string>
                {
                    { "tagOptionsByType", tagsByType.ToCamelCaseJson( false, true ) },
                },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string>();

            int entityTypePersonId = EntityTypeCache.GetId( typeof( Rock.Model.Person ) ) ?? 0;
            var tagQry = new TagService( rockContext ).Queryable( "OwnerPersonAlias" ).Where( a => a.EntityTypeId == entityTypePersonId );
            var personalTags = tagQry.Where( a => a.OwnerPersonAlias.PersonId == requestContext.CurrentPerson.Id )
                .OrderBy( a => a.Name )
                .Select( t => new ListItemBag { Text = t.Name, Value = t.Guid.ToString() } )
                .ToList();

            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length < 2 )
            {
                data.Add( "tagType", "1" );
                data.Add( "tag", null );
            }
            else
            {
                var tagType = selectionValues[0];
                data.Add( "tagType", tagType );

                var selectedTagGuid = selectionValues[1];

                if ( tagType == "1" && !personalTags.Where( t => t.Value == selectedTagGuid ).Any() )
                {
                    // The selected tag belongs to someone else's list of personal tags, so we need to include it under a "Current" category
                    // so it can still appear on the list and file all the others as under the "Personal" category to keep them separate.
                    var selectedTag = new TagService( rockContext ).Get( selectedTagGuid.AsGuid() );

                    if ( selectedTag == null )
                    {
                        // I guess this tag doesn't actually exist, so no need to categorize
                        data.Add( "tag", null );
                    }
                    else
                    {
                        data.Add( "tag", selectedTagGuid );
                    }
                }
                else
                {
                    data.Add( "tag", selectedTagGuid );
                }
            }

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var tagType = data.GetValueOrDefault( "tagType", "" );
            var tag = data.GetValueOrDefault( "tag", "" );

            return $"{tagType}|{tag}";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Person Tag";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
  var tagName = $('.js-tag-filter-list', $content).find(':selected').text()
  var result = 'Tagged as ' + tagName;

  return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Person Tag";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                Guid selectedTagGuid = selectionValues[1].AsGuid();
                var selectedTag = new TagService( new RockContext() ).Get( selectedTagGuid );
                if ( selectedTag != null )
                {
                    result = string.Format( "Tagged as {0}", selectedTag.Name );
                }
            }

            return result;
        }

#if WEBFORMS

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var rblTagType = new RockRadioButtonList();
            rblTagType.ID = filterControl.ID + "_tagType";
            rblTagType.Label = "Tag Type";
            rblTagType.RepeatDirection = RepeatDirection.Horizontal;
            rblTagType.Items.Add( new ListItem( "Personal Tags", "1" ) );
            rblTagType.Items.Add( new ListItem( "Organizational Tags", "2" ) );
            rblTagType.SelectedValue = "1";
            rblTagType.AutoPostBack = true;
            rblTagType.SelectedIndexChanged += rblTagType_SelectedIndexChanged;
            rblTagType.CssClass = "js-tag-type";
            filterControl.Controls.Add( rblTagType );

            var ddlTagList = new RockDropDownList();
            ddlTagList.ID = filterControl.ID + "_ddlTagList";
            ddlTagList.Label = "Tag";
            ddlTagList.AddCssClass( "js-tag-filter-list" );
            filterControl.Controls.Add( ddlTagList );

            PopulateTagList( filterControl );

            return new Control[2] { rblTagType, ddlTagList };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblTagType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblTagType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();
            PopulateTagList( filterField );
        }

        /// <summary>
        /// Populates the tag list.
        /// </summary>
        private void PopulateTagList( FilterField filterField )
        {
            int entityTypePersonId = EntityTypeCache.GetId( typeof( Rock.Model.Person ) ) ?? 0;
            var tagQry = new TagService( new RockContext() ).Queryable( "OwnerPersonAlias" ).Where( a => a.EntityTypeId == entityTypePersonId );

            var rblTagType = filterField.ControlsOfTypeRecursive<RockRadioButtonList>().FirstOrDefault( a => a.HasCssClass( "js-tag-type" ) );
            var ddlTagList = filterField.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.HasCssClass( "js-tag-filter-list" ) );
            RockPage rockPage = rblTagType.Page as RockPage;

            if ( rblTagType.SelectedValueAsInt() == 1 )
            {
                // Personal tags - tags where the ownerid is the current person id
                tagQry = tagQry.Where( a => a.OwnerPersonAlias.PersonId == rockPage.CurrentPersonId ).OrderBy( a => a.Name );
            }
            else
            {
                // Organizational tags - tags where the ownerid is null
                tagQry = tagQry.Where( a => a.OwnerPersonAlias == null ).OrderBy( a => a.Name );
            }

            ddlTagList.Items.Clear();
            var tempTagList = tagQry.ToList();

            foreach ( var tag in tagQry.Select( a => new { a.Guid, a.Name } ) )
            {
                ddlTagList.Items.Add( new ListItem( tag.Name, tag.Guid.ToString() ) );
            }
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            return ( controls[0] as RadioButtonList ).SelectedValue + "|" + ( controls[1] as RockDropDownList ).SelectedValue;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 2 )
            {
                int tagType = selectionValues[0].AsInteger();
                Guid selectedTagGuid = selectionValues[1].AsGuid();

                ( controls[0] as RadioButtonList ).SelectedValue = tagType.ToString();

                var rblTagType = controls[0] as RadioButtonList;

                rblTagType_SelectedIndexChanged( rblTagType, new EventArgs() );

                RockDropDownList ddlTagList = controls[1] as RockDropDownList;

                if ( ddlTagList.Items.FindByValue( selectedTagGuid.ToString() ) != null )
                {
                    ddlTagList.SelectedValue = selectedTagGuid.ToString();
                }
                else
                {
                    // if the selectedTag is a personal tag, but for a different Owner than the current logged in person, include it in the list
                    var selectedTag = new TagService( new RockContext() ).Get( selectedTagGuid );
                    if ( selectedTag != null )
                    {
                        if ( selectedTag.OwnerPersonAliasId.HasValue )
                        {
                            foreach ( var listItem in ddlTagList.Items.OfType<ListItem>() )
                            {
                                listItem.Attributes["OptionGroup"] = "Personal";
                            }

                            string tagText = string.Format( "{0} ( {1} )", selectedTag.Name, selectedTag.OwnerPersonAlias.Person );
                            ListItem currentTagListItem = new ListItem( tagText, selectedTagGuid.ToString() );
                            currentTagListItem.Attributes["OptionGroup"] = "Current";
                            ddlTagList.Items.Insert( 0, currentTagListItem );
                        }
                    }
                }
            }
        }

#endif

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                Guid tagGuid = selectionValues[1].AsGuid();
                var tagItemQry = new TaggedItemService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( x => x.Tag.Guid == tagGuid );

                var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( p => tagItemQry.Any( x => x.EntityGuid == p.Guid ) );

                return FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );
            }

            return null;
        }

        #endregion
    }
}