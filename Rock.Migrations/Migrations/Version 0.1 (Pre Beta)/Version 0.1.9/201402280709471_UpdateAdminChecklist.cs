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
    public partial class UpdateAdminChecklist : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

UPDATE [DefinedValue] SET
[Description] = REPLACE([Description], 'Administration >', 'Admin Tools >')
WHERE [Description] LIKE '%Administration >%' AND [DefinedTypeId] in (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '4BF34677-37E9-4E71-BD03-252B66C9373D')


UPDATE [DefinedValue]
SET
	[Description] = 'You created an administrator account during the install.  You should now create your own personal account. Start by adding your family <span class=""navigation-tip"">People > New Family</span>.  Once you have your family entered, navigate to your person detail record by searching for yourself at the top of the page. On this page you can add your account information and roles under the <span class=""navigation-tip"">Security</span> tab. From now on login using the new account.'
WHERE
	[Guid] = '4C94C85B-2180-48CD-A1B6-62327B407245'

/* ---------------------------------------------------------------- */

UPDATE [DefinedValue]
SET
	[Description] = 'Add your organization''s address under <span class=""navigation-tip"">Admin Tools > General Settings > Global Attributes > Organization Address</span>.'
WHERE
	[Guid] = '9CB1677A-2016-41B0-B3FE-D2979C52FC7E'


/* ---------------------------------------------------------------- */

UPDATE [DefinedValue]
SET
	[Name] = 'Consider Setting Up An Address Standardization Service'
	,[Description] = 'Rock integrates with several third-party services that help to increase the quality of the addresses entered. These services can:
<ul>
<li>Fix common formatting issues</li>
<li>Append missing information including zip+4</li>
</ul>

For more information on the options available and how to configure them, please see the <a href=""http://www.rockrms.com/Rock/BookContent/9#standardizationservices"">Rock Admin Hero Guide</a>.'
WHERE
	[Guid] = 'B1CE8ECA-6584-45DC-A022-40490FC753E8'

/* ---------------------------------------------------------------- */

UPDATE [DefinedValue]
SET
	[Description] = '<p>Before adding individuals to your database, consider what custom attributes about a person you want to add.  These attributes can be defined in <span class=""navigation-tip"">Admin Tools > Person Settings > Person Attributes</span>. Need some inspiration? Consider:</p>
<ul>
<li>Do you have lots of military?  Maybe you want to track ''Deployment Date'' and care for the family that remains home.</li>
<li>Do you have a diverse congregation?  Maybe you want to track ''Languages Spoken'' by people within your congregation. </li>
<li>Does your ministry seem to always be handing out t-shirts?  Store an individual''s t-shirt preference.</li>
</ul>

<p>Note: When you create a new category of person attributes you''ll want to add that new category to the <span class=""navigation-tip"">Person Profile</span> page to allow for entry/edit. To do this go to any individual''s detail page and add a new ''Attributes Value'' block to the zone you''d like it to appear.</p>

<p>For more information on managing Person Attributes see the <a href=""http://www.rockrms.com/Rock/BookContent/5#personattributes"">Rock Person & Family Field Guide</a>.</p>
'
WHERE
	[Guid] = '5808DF4C-4F5D-4488-9C54-B05DE20B5FCF'


/* ---------------------------------------------------------------- */

UPDATE [DefinedValue]
SET
	[Description] = 'Rock allows you to pattern the software to match your ministry. One way to do that is to create custom group types to match the ministry teams of your organization. The <a href=""http://www.rockrms.com/Rock/Book/7"">Rock Your Groups Guide</a> can give you a better understanding of the power that you have in creating these new group types.'
WHERE
	[Guid] = 'B0E46522-921F-47AA-B548-F0072F22B903'

/* ---------------------------------------------------------------- */

INSERT INTO [DefinedValue] 
	(IsSystem
	,DefinedTypeId
	,[Order]
	,[Name]
	,[Guid]
	,[Description])
VALUES
	(0
	,35
	,2
	,'Setup Geocoding Service'
	,'B3FAB89D-4EAD-4ED8-AD7F-06E150E4CD6B'
	,'<p>Geocoding is the process of converting a physical street address into a latitude/longitude point. This allows Rock to measure distances between two points and determine if addresses are within defined boundaries.</p>

<p> While Rock ships with the ability to integrate with several geocoding services we recommend using the Bing Maps API as they generously allow you to geocode addresses for free. In order to configure this you will need to obtain a free license key. Follow these directions to obtain and save your key:

<ul>
    <li>Goto the <a href=""https://www.bingmapsportal.com/"">Bing Maps Portal</a>.</li>
    <li>Sign in using a Microsoft Account or create a new account.</li> 
    <li>Select ""Create or View Keys"" from the menu on the left side of the page.</li>
    <li>Create a new key. Choose ""Basic"" for the key type and ""Public Website"" or ""Non-Profit"" for the application type (depending on what fits your organization).</li>
    <li>Input your key under <span class=""navigation-tip"">Admin Tools > System Settings > Geocoding Services > Bing</span>.
    
</ul>

</p>

<p>
For more information on geocoding, including other geocoding services that are available, see the <a href=""http://www.rockrms.com/Rock/BookContent/9#geocodingservices"">Admin Hero Guide</a>.
</p>')

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
