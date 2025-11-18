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
    public partial class UpdateDefinedValuePickerLavaShortcode : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update the documentation for the DefinedValuePicker lava shortcode to use 'definedtype'
            // Update the parameters & markup to use 'definedtype' but allow the accidental legacy 'definedtypeid'
            Sql( @"
UPDATE [LavaShortcode]
SET
    [Documentation] = REPLACE([Documentation],
'<ul>
    <li><strong>label</strong>', 
'<ul>
    <li><strong>definedtype</strong> – Specifies the ID or Guid of the Defined Type to use for populating the picker.</li>
    <li><strong>label</strong>' )
    ,[Parameters] = 'value^|includeinactive^false|name^definedvalue|isrequired^false|label^Defined Value|validationmessage^Please select a value.|longlistenabled^false|valuefield^id|allowmultiple^false|additionalattributes^|definedtype^|definedtypeid^1|displaydescriptions^false|showlabel^true'
    ,[Markup] = REPLACE([Markup],
'//- Prep configuration settings
{% assign sc-definedtype = definedtypeid | FromCache:''DefinedType'' %}',
'//- Prep configuration settings
{% if definedtype != '''' %}
	{% assign definedtypeid = definedtype %}
{% endif %}
{% assign sc-definedtype = definedtypeid | FromCache:''DefinedType'' %}' )
WHERE [Guid] = 'E2FC377F-EDCE-4FD3-B734-D06939E65210'
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
