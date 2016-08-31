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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class UpdateLiquidIncludes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update the path to liquid include files
            Sql( @"
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '{% include ''', '{% include ''~~/Assets/Liquid/' ) WHERE [DefaultValue] like '%{[%] include ''%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/AdDetails', '~~/Assets/Liquid/_AdDetails.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/AdList', '~~/Assets/Liquid/_AdList.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/AdRotator', '~~/Assets/Liquid/_AdRotator.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/BlogItemDetail', '~~/Assets/Liquid/_BlogItemDetail.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/BlogItemList', '~~/Assets/Liquid/_BlogItemList.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/PageListAsBlocks', '~~/Assets/Liquid/_PageListAsBlocks.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/PageListAsTabs', '~~/Assets/Liquid/_PageListAsTabs.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/PageMenu', '~~/Assets/Liquid/_PageMenu.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/PageNav', '~~/Assets/Liquid/_PageNav.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/PageSubNav', '~~/Assets/Liquid/_PageSubNav.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/RSSFeed', '~~/Assets/Liquid/_RSSFeed.liquid' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/RSSFeedItem', '~~/Assets/Liquid/_RSSFeedItem.liquid' ) WHERE [DefaultValue] like '%{[%] include%'

    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '{% include ''', '{% include ''~~/Assets/Liquid/' ) WHERE [Value] like '%{[%] include ''%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/AdDetails', '~~/Assets/Liquid/_AdDetails.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/AdList', '~~/Assets/Liquid/_AdList.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/AdRotator', '~~/Assets/Liquid/_AdRotator.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/BlogItemDetail', '~~/Assets/Liquid/_BlogItemDetail.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/BlogItemList', '~~/Assets/Liquid/_BlogItemList.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/PageListAsBlocks', '~~/Assets/Liquid/_PageListAsBlocks.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/PageListAsTabs', '~~/Assets/Liquid/_PageListAsTabs.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/PageMenu', '~~/Assets/Liquid/_PageMenu.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/PageNav', '~~/Assets/Liquid/_PageNav.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/PageSubNav', '~~/Assets/Liquid/_PageSubNav.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/RSSFeed', '~~/Assets/Liquid/_RSSFeed.liquid' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/RSSFeedItem', '~~/Assets/Liquid/_RSSFeedItem.liquid' ) WHERE [Value] like '%{[%] include%'
" );

            // Remove the 'Blocs' route
            Sql( @"
    DECLARE @BlockTypesPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '5FBE9019-862A-41C6-ACDC-287D7934757D' )
    DELETE [PageRoute] WHERE [PageId] = @BlockTypesPageId AND [Route] = 'Blocs' 
" );

            // Update the 'Person/{PersonId}/StaffDetails' route to 'Person/{PersonId}/Security'
            Sql( @"
    UPDATE [PageRoute] SET [Route] = REPLACE( [Route], 'StaffDetails', 'Security' ) WHERE [Route] like '%StaffDetails%'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
