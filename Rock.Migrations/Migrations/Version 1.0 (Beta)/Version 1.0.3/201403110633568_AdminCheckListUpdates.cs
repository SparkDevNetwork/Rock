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
    public partial class AdminCheckListUpdates : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // update type in current item
            Sql( @"UPDATE [DefinedValue]
SET [Description] = '<p>Before adding individuals to your database, consider what custom attributes about a person you want to add.  These attributes can be defined in <span class=""navigation-tip"">Admin Tools > General Settings > Person Attributes</span>. Need some inspiration? Consider:</p>
<ul>
<li>Do you have lots of military?  Maybe you want to track ''Deployment Date'' and care for the family that remains home.</li>
<li>Do you have a diverse congregation?  Maybe you want to track ''Languages Spoken'' by people within your congregation. </li>
<li>Does your ministry seem to always be handing out t-shirts?  Store an individual''s t-shirt preference.</li>
</ul>

<p>Note: When you create a new category of person attributes you''ll want to add that new category to the <span class=""navigation-tip"">Person Profile</span> page to allow for entry/edit. To do this go to any individual''s detail page and add a new ''Attributes Value'' block to the zone you''d like it to appear.</p>

<p>For more information on managing Person Attributes see the <a href=""http://www.rockrms.com/Rock/BookContent/5#personattributes"">Rock Person & Family Field Guide</a>.</p>
'
WHERE 
	[Guid] = '5808DF4C-4F5D-4488-9C54-B05DE20B5FCF'" );

            // add item to update the rock install
            Sql(@"INSERT INTO [DefinedValue]
	([IsSystem]
	,[DefinedTypeId]
	,[Order]
	,[Name]
	,[Description]
	,[Guid] )
VALUES
	(0
	,(SELECT [Id] FROM [DefinedType] WHERE [Guid] = '4BF34677-37E9-4E71-BD03-252B66C9373D')
	,0
	,'Update Your Install'
	,'<p>Install the latest updates from <span class=""navigation-tip"">Admin Tools > General Settings > Rock Update</span>. </p>'
	,'ABC4457F-3C2A-4856-9FA8-D4B303F23B93')");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [DefinedValue] WHERE [Guid] = 'ABC4457F-3C2A-4856-9FA8-D4B303F23B93'" );
        }
    }
}
