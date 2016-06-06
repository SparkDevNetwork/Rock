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
    public partial class UpdateLavaIncludes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update the path to liquid include files
            Sql( @"
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_AdDetails.liquid', '~~/Assets/Lava/AdDetails.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_AdAdDetails.liquid', '~~/Assets/Lava/AdDetails.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_AdList.liquid', '~~/Assets/Lava/AdList.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_AdRotator.liquid', '~~/Assets/Lava/AdRotator.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_BlogItemDetail.liquid', '~~/Assets/Lava/BlogItemDetail.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_BlogItemList.liquid', '~~/Assets/Lava/BlogItemList.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_PageListAsBlocks.liquid', '~~/Assets/Lava/PageListAsBlocks.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_PageListAsTabs.liquid', '~~/Assets/Lava/PageListAsTabs.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_PageMenu.liquid', '~~/Assets/Lava/PageMenu.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_PageNav.liquid', '~~/Assets/Lava/PageNav.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_PageSubNav.liquid', '~~/Assets/Lava/PageSubNav.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_RSSFeed.liquid', '~~/Assets/Lava/RSSFeed.lava' ) WHERE [DefaultValue] like '%{[%] include%'
    UPDATE [Attribute] SET [DefaultValue] = REPLACE( [DefaultValue], '~~/Assets/Liquid/_RSSFeedItem.liquid', '~~/Assets/Lava/RSSFeedItem.lava' ) WHERE [DefaultValue] like '%{[%] include%'

    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_AdDetails.liquid', '~~/Assets/Lava/AdDetails.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_AdAdDetails.liquid', '~~/Assets/Lava/AdDetails.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_AdList.liquid', '~~/Assets/Lava/AdList.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_AdRotator.liquid', '~~/Assets/Lava/AdRotator.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_BlogItemDetail.liquid', '~~/Assets/Lava/BlogItemDetail.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_BlogItemList.liquid', '~~/Assets/Lava/BlogItemList.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_PageListAsBlocks.liquid', '~~/Assets/Lava/PageListAsBlocks.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_PageListAsTabs.liquid', '~~/Assets/Lava/PageListAsTabs.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_PageMenu.liquid', '~~/Assets/Lava/PageMenu.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_PageNav.liquid', '~~/Assets/Lava/PageNav.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_PageSubNav.liquid', '~~/Assets/Lava/PageSubNav.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_RSSFeed.liquid', '~~/Assets/Lava/RSSFeed.lava' ) WHERE [Value] like '%{[%] include%'
    UPDATE [AttributeValue] SET [Value] = REPLACE( [Value], '~~/Assets/Liquid/_RSSFeedItem.liquid', '~~/Assets/Lava/RSSFeedItem.lava' ) WHERE [Value] like '%{[%] include%'
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
