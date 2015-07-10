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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;
using Rock.Security;
using System.Reflection;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Takes a defined type and returns all defined values and merges them with a liquid template
    /// </summary>
    [DisplayName( "Following By Entity" )]
    [Category( "Core" )]
    [Description( "Takes a entity type and displays a person's following items for that entity using a Lava template." )]
    [EntityTypeField("Entity Type", "The type of entity to show following for.", order: 0)]
    [TextField("Link URL", "The address to use for the link. The text '[Id]' will be replaced with the Id of the entity '[Guid]' will be replaced with the Guid.", false, "/samplepage/[Id]", "", 1, "LinkUrl")]
    [CodeEditorField("Lava Template", "Lava template to use to display content", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% for definedValue in DefinedValues %}
    {{ definedValue.Value }}
{% endfor %}", "", 2, "LavaTemplate" )]
    [BooleanField("Enable Debug", "Show merge data to help you see what's available to you.", order: 3)]
    [IntegerField("Max Results", "The maximum number of results to display.", true, 100, order: 4)]
    public partial class FollowingByEntityLava : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
                LoadContent();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContent();
        }

        

        #endregion

        #region Methods

        protected void LoadContent()
        {
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );
            
            var entityType = EntityTypeCache.Read(GetAttributeValue("EntityType").AsGuid());
            string tableName = entityType.Name.Replace( "Rock.Model.", "" );


            if ( entityType == null || !entityType.Name.StartsWith( "Rock.Model" ) )
            {
                lContent.Text = string.Format( "<div class='alert alert-warning'>This entity is not supported. Only entities of type Rock.Modle are supported." );
            } else
            {

                RockContext rockContext = new RockContext();
                
                switch ( entityType.Name )
                {
                    /*case "Rock.Model.Group":
                        {
                            string sql = string.Format( @"SELECT 
                                                        TOP {0} g.*, gt.* 
                                                    FROM [Group] g 
                                                        INNER JOIN [Following] f ON f.[EntityId] = g.[Id]
                                                        INNER JOIN [PersonAlias] pa ON pa.[Id] = f.[PersonAliasId]
                                                        INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId] 
                                                        INNER JOIN [Person] p ON p.[Id] = pa.[PersonId] AND p.[Id] = {1}", GetAttributeValue( "MaxResults" ).AsInteger(), CurrentPersonId );

                            var results = rockContext.Database.SqlQuery<Group>( sql );
                            
                            /*var followingItems = new FollowingService(rockContext).Queryable()
                                                    .Where(f => f.EntityTypeId == entityType.Id 
                                                                && f.PersonAlias.PersonId == CurrentPersonId)
                                                    .Select( f => f.EntityId);

                            var results = new GroupService(rockContext).Queryable()
                                                .Where( g => followingItems.Contains( g.Id))
                                                
                                                .ToList();
                            mergeFields.Add( "FollowingItems", results );
                            break;
                        }*/
                    default:
                        {
                            Type type = Type.GetType( entityType.AssemblyName );
                            
                            string sql = string.Format( @"SELECT 
                                                        TOP {0} t.* 
                                                    FROM [{1}] t 
                                                        INNER JOIN [Following] f ON f.[EntityId] = t.[Id]
                                                        INNER JOIN [PersonAlias] pa ON pa.[Id] = f.[PersonAliasId]
                                                        INNER JOIN [Person] p ON p.[Id] = pa.[PersonId] AND p.[Id] = {2}", GetAttributeValue( "MaxResults" ).AsInteger(), tableName, CurrentPersonId );

                            var results = rockContext.Database.SqlQuery( type, sql );

                            mergeFields.Add( "FollowingItems", results );
                            break;
                        }
                }

                
            }

            mergeFields.Add( "EntityType", tableName );
            mergeFields.Add( "LinkUrl", GetAttributeValue( "LinkUrl" ) );

            string template = GetAttributeValue( "LavaTemplate" );
            lContent.Text = template.ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
        }

        #endregion
    }
}