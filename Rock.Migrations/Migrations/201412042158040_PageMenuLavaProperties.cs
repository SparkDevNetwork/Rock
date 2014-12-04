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
namespace Rock.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PageMenuLavaProperties : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '1322186A-862A-4CF1-B349-28ECB67229BA' )
    UPDATE [AttributeValue] 
	    SET [Value] = 
		    REPLACE( REPLACE( REPLACE( REPLACE( REPLACE( REPLACE(
		    REPLACE( REPLACE( REPLACE( REPLACE( REPLACE( REPLACE( 
		    REPLACE( 
		    [Value] COLLATE SQL_Latin1_General_CP1_CS_AS,
		    'page.', 'Page.') COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.pages', '.Pages' ) COLLATE SQL_Latin1_General_CP1_CS_AS, 
		    '.icon-url', '.IconUrl' ) COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.description', '.Description' ) COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.icon-css-class', '.IconCssClass' ) COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.display-child-pages', '.DisplayChildPages' ) COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.display-icon', '.DisplayIcon' ) COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.display-description', '.DisplayDescription' ) COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.url', '.Url' ) COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.isParentOfCurrent', '.IsParentOfCurrent' ) COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.current', '.Current' ) COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.title', '.Title' ) COLLATE SQL_Latin1_General_CP1_CS_AS,
		    '.id', '.Id' )
    WHERE [AttributeId] = @AttributeId
" );

            var httpContext = System.Web.HttpContext.Current;
            if (httpContext != null && httpContext.Server != null)
            {
                var rockDir = new DirectoryInfo( httpContext.Server.MapPath( "~" ) );
                if (rockDir.Exists)
                {
                    FixPageLavaTemplates( rockDir.GetFiles( "*.lava", SearchOption.AllDirectories ) );
                    FixPageLavaTemplates( rockDir.GetFiles( "*.liquid", SearchOption.AllDirectories ) );
                }
            }
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void FixPageLavaTemplates(FileInfo[] lavaFiles)
        {
            foreach ( FileInfo file in lavaFiles )
            {
                string contents = File.ReadAllText( file.FullName );
                if ( contents.Contains( "page.pages" ) )
                {
                    contents = contents.Replace( "page.", "Page." );
                    contents = contents.Replace( ".pages", ".Pages" );
                    contents = contents.Replace( ".icon-url", ".IconUrl" );
                    contents = contents.Replace( ".description", ".Description" );
                    contents = contents.Replace( ".icon-css-class", ".IconCssClass" );
                    contents = contents.Replace( ".display-child-pages", ".DisplayChildPages" );
                    contents = contents.Replace( ".display-icon", ".DisplayIcon" );
                    contents = contents.Replace( ".display-description", ".DisplayDescription" );
                    contents = contents.Replace( ".url", ".Url" );
                    contents = contents.Replace( ".isParentOfCurrent", ".IsParentOfCurrent" );
                    contents = contents.Replace( ".current", ".Current" );
                    contents = contents.Replace( ".title", ".Title" );
                    contents = contents.Replace( ".id", ".Id" );
                    File.WriteAllText( file.FullName, contents );
                }
            }
        }
    }
}
