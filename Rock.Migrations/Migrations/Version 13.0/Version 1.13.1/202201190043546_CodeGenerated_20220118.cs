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
    public partial class CodeGenerated_20220118 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
           // Add/Update BlockType 
           //   Name: Related Entity Connect
           //   Category: Core
           //   Path: ~/Blocks/Core/RelatedEntityConnect.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Related Entity Connect","Connects two related entities with options.","~/Blocks/Core/RelatedEntityConnect.ascx","Core","5F40F4FD-338A-4711-87F7-980ED1FAE615");

           // Add/Update BlockType 
           //   Name: Related Entity List
           //   Category: Core
           //   Path: ~/Blocks/Core/RelatedEntityList.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Related Entity List","Lists information about related entities.","~/Blocks/Core/RelatedEntityList.ascx","Core","28516B18-7423-4A97-9223-B97537BD0F79");

           // Add/Update BlockType 
           //   Name: Group Schedule Toolbox v2
           //   Category: Group Scheduling
           //   Path: ~/Blocks/GroupScheduling/GroupScheduleToolboxV2.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Group Schedule Toolbox v2","Allows management of group scheduling for a specific person (worker).","~/Blocks/GroupScheduling/GroupScheduleToolboxV2.ascx","Group Scheduling","18A6DCE3-376C-4A62-B1DD-5E5177C11595");

            // Add/Update Obsidian Block Type
            //   Name:Attributes
            //   Category:Obsidian > Core
            //   EntityType:Rock.Blocks.Core.Attributes
            RockMigrationHelper.UpdateMobileBlockType("Attributes", "Allows for the managing of attributes.", "Rock.Blocks.Core.Attributes", "Obsidian > Core", "8B34B1D7-B429-4F3E-8CE6-4510E0DE8D2A");

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Connection
            //   Attribute: Activity Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Activity Lava Template", "Activity Lava Template", "Activity Lava Template", @"This Lava template will be used to display the activity records.
                         <i>(Note: The Lava will include the following merge fields:
                            <p><strong>ConnectionRequest, CurrentPerson, Context, PageParameter, Campuses</strong>)</p>
                         </i>", 8, @"
{% comment %}
   This is the default lava template for the ConnectionRequestDetail block's Activity List.

   Available Lava Fields:
       ConnectionRequest
       CurrentPerson
       Context
       PageParameter
       Campuses
{% endcomment %}
<style>
    .card:hover {
      transform: scale(1.01);
      box-shadow: 0 10px 20px rgba(0,0,0,.12), 0 4px 8px rgba(0,0,0,.06);
    }

    .person-image-small {
        position: relative;
        box-sizing: border-box;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 40px;
        height: 40px;
        vertical-align: top;
        background: center/cover #cbd4db;
        border-radius: 50%;
        box-shadow: inset 0 0 0 1px rgba(0,0,0,0.07)
    }

  .delete-button {
        color: black !important;
   }

  .delete-button:hover {
        color: red !important;
    }
</style>

    <div class='row>
       <div class='col-xs-12>
           <h2>Activity</h2>
       </div>
    </div>

{% for connectionRequestActivity in ConnectionRequest.ConnectionRequestActivities %}
   {% if connectionRequestActivity.CreatedByPersonAliasId == CurrentPerson.PrimaryAliasId or connectionRequestActivity.ConnectorPersonAliasId == CurrentPerson.PrimaryAliasId %}
      {%if connectionRequestActivity.ConnectionActivityType.ConnectionTypeId %}
          {% assign canEdit = true %}
      {% else %}
          {% assign canEdit = false %}
      {% endif %}
   {% endif %}

    <a href='{{ DetailPage | Default:'0' | PageRoute }}?ConnectionTypeGuid={{ connectionType.Guid }}' stretched-link>
        <div class='card mb-2'>
            <div class='card-body'>
                <div class='row pt-2' style='height:60px;'>
                    <div class='col-xs-2 col-md-1 mx-auto'>
                        <img class='person-image-small' src='{{ connectionRequestActivity.ConnectorPersonAlias.Person.PhotoUrl | Default: '/Assets/Images/person-no-photo-unknown.svg'  }}' alt=''>
                    </div>     
                    <div class='col-xs-6 col-md-9 pl-md-0 mx-auto'>
                       <strong class='text-color'>{{ connectionRequestActivity.ConnectorPersonAlias.Person.FullName | Default: 'Unassigned' }}</strong>
                       <br/>
                       {% if connectionRequestActivity.Note | StripNewlines | Trim | Size > 0 %}
                          <span class='text-muted'><small><strong>{{ connectionRequestActivity.ConnectionActivityType.Name }}</strong>: {{ connectionRequestActivity.Note }}</small></span>
                       {% else %}
                          <span class='text-muted'><small><strong>{{ connectionRequestActivity.ConnectionActivityType.Name }}</strong></small></span>         
                       {% endif %}
                    </div>
                    <div class='col-xs-4 col-md-2 mx-auto text-right'>
                        <small class='text-muted'>{{ connectionRequestActivity.CreatedDateTime | Date:'M/d/yy' }}</small>
                    </div>
                </div>
                <div class='row grid-actions text-right'>
                    <div class='col-xs-12'>
                         {% if canEdit == true %}
                             <a title='Delete' class='btn btn-grid-action btn-sm grid-delete-button delete-button' href='javascript:void(0);' onclick=""{{ connectionRequestActivity.Id | Postback : 'DeleteActivity' }}"">
                             <i class='fa fa-times' style='font-size:22px;'></i>
                         </a>
                         {% else %}
                             <a title='Delete' class='btn btn-grid-action btn-sm grid-delete-button aspNetDisabled' href='javascript:void(0);'>
                                 <i class='fa fa-times' style='font-size:22px;'></i>
                            </a>
                         {% endif %}
                    </div>
                </div>
            </div>
        </div>
    </a>
{% endfor %}

{% comment %} {{ 'Lava' | Debug }} {% endcomment %}", "34CE545D-167F-4EC6-97A4-603C85ED8243" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Display Country Code
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34275D0E-BC7E-4A9C-913E-623D086159A1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Country Code", "DisplayCountryCode", "Display Country Code", @"When enabled prepends the country code to all phone numbers.", 2, @"False", "77B2BA79-B632-46AE-9A15-A9DBB66F54F6" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Display Government Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34275D0E-BC7E-4A9C-913E-623D086159A1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Government Id", "DisplayGovernmentId", "Display Government Id", @"Display the government identifier.", 3, @"True", "FC2BFF21-5E7A-4084-975D-A781C197F2B9" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Display Middle Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34275D0E-BC7E-4A9C-913E-623D086159A1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Middle Name", "DisplayMiddleName", "Display Middle Name", @"Display the middle name of the person.", 4, @"False", "8E25EB26-23A4-432D-8DD0-FD2CFF9F6F5C" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Workflow Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34275D0E-BC7E-4A9C-913E-623D086159A1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Detail Page", "WorkflowDetailPage", "Workflow Detail Page", @"Page used to display details about a workflow.", 6, @"BA547EED-5537-49CF-BD4E-C583D760788C", "7BCE9779-BED6-43F2-A4A8-D8FB5C022026" );

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Workflow Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34275D0E-BC7E-4A9C-913E-623D086159A1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "Workflow Entry Page", @"Page used to launch a new workflow of the selected type.", 7, @"0550D2AA-A705-4400-81FF-AB124FDF83D7", "11E5B09E-E835-4D5B-A7B8-D86544E2179E" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B34B1D7-B429-4F3E-8CE6-4510E0DE8D2A", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "Entity", @"Entity Name", 0, @"", "4905B2D8-21C3-4748-AB4E-7C6A69FCE692" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B34B1D7-B429-4F3E-8CE6-4510E0DE8D2A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "Entity Qualifier Column", @"The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "07B4D043-168F-4630-AB13-B9E52ED2750E" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B34B1D7-B429-4F3E-8CE6-4510E0DE8D2A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "Entity Qualifier Value", @"The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "69B84180-5B6C-447E-B906-A3D8511148D4" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B34B1D7-B429-4F3E-8CE6-4510E0DE8D2A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "Allow Setting of Values", @"Should UI be available for setting values of the specified Entity ID?", 3, @"false", "5F3FC9FB-1815-4AEA-BE66-8BFFFDE43F77" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B34B1D7-B429-4F3E-8CE6-4510E0DE8D2A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "Entity Id", @"The entity id that values apply to", 4, @"0", "A94814B9-614F-4519-A17D-986A3064409B" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B34B1D7-B429-4F3E-8CE6-4510E0DE8D2A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "Enable Show In Grid", @"Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"false", "ABEB4044-3919-4D7E-BBBF-A89A4ABF9028" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B34B1D7-B429-4F3E-8CE6-4510E0DE8D2A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "Category Filter", @"A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "5F639EE6-EFF1-4320-BAFE-CCD2BD8117FC" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8B34B1D7-B429-4F3E-8CE6-4510E0DE8D2A", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden.", 7, @"", "97979723-0F10-4FD1-8A39-B5E735A5504C" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Source Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Source Entity Type", "SourceEntityType", "Source Entity Type", @"The type of entity to that will be the source.", 0, @"", "9DF6277C-1378-4584-832A-5B8F4FE2D3F6" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Target Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Target Entity Type", "TargetEntityType", "Target Entity Type", @"The type of entity to that will be the target.", 1, @"", "093585C3-1070-4539-9680-B2E72E4C47E2" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Purpose Key
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Purpose Key", "PurposeKey", "Purpose Key", @"The purpose key to use for linking the two entities together. While this is not required, it is highly recommended that you provide one.", 2, @"", "9BFE4046-23B1-45A3-B5DF-9D5B133D50F2" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Parameter Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Parameter Type", "ParameterType", "Parameter Type", @"Determines the type of the paramters that are being passed in for the source and target. Guids are more secure but requires 2 additional lookups to convert them to integers. The query string parameters are 'Source' and 'Target'", 3, @"Guid", "7FA01B7D-5F2D-4B30-8792-0769070D69E5" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Enable Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Edit", "EnableEdit", "Enable Edit", @"Determines if existing relationships should be editable or if this should always add a new value.", 4, @"True", "B03AF8EA-46B4-4B2D-9E78-EF1CCE98BE3A" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Show Quantity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Quantity", "ShowQuantity", "Show Quantity", @"Determines if the quantity field should be shown.", 5, @"False", "5C6E3C6D-F9F1-4ED7-87E3-6AC49ACD1E35" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Show Note
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Note", "ShowNote", "Show Note", @"Determines if the note field should be shown.", 6, @"False", "25693AEB-9DD2-43E9-922E-40EF72F2FF60" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Enable Attribute Editing
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Attribute Editing", "EnableAttributeEditing", "Enable Attribute Editing", @"Determines if the attributes of the related entities should be allowed to be edited.", 7, @"True", "9CD89F31-F7D5-4558-9C0C-43B1FF9FA207" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Lava Template", "HeaderLavaTemplate", "Header Lava Template", @"The Lava template to use for the header", 8, @"", "7813F236-AE07-4C65-BD5F-67637BA79AAC" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Header Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Header Title", "HeaderTitle", "Header Title", @"The title for the panel heading.", 9, @"Related Entity Connector", "4BAAE325-4D30-4B01-A31D-37067574ACCF" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Header Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Header Icon CSS Class", "HeaderIconCssClass", "Header Icon CSS Class", @"The CSS icon for the panel heading.", 10, @"fa fa-link", "D7B4479B-E073-429C-A9EA-EE9BA97F438C" );

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Attribute Columns
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F40F4FD-338A-4711-87F7-980ED1FAE615", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Attribute Columns", "AttributeColumns", "Attribute Columns", @"How many columns should the attribute editor use.", 11, @"2", "46C303B6-C3D7-48C5-B35C-E909FDEF76F5" );

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Source Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28516B18-7423-4A97-9223-B97537BD0F79", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Source Entity Type", "SourceEntityType", "Source Entity Type", @"The type of entity to that will be the source.", 0, @"", "58A22A5C-1719-457B-A38F-D94BB8D1F883" );

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Source Is Current Person
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28516B18-7423-4A97-9223-B97537BD0F79", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Source Is Current Person", "SourceIsCurrentPerson", "Source Is Current Person", @"Determines if the current person should be used as the source. If true the Source Entity Type should be set to PersonAlias.", 1, @"False", "031ABF7B-39FF-4B70-99E3-CF027A4CEDFE" );

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Target Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28516B18-7423-4A97-9223-B97537BD0F79", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Target Entity Type", "TargetEntityType", "Target Entity Type", @"The type of entity to that will be the target.", 2, @"", "577179E8-71AE-4312-96B2-39F24280FBD3" );

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Purpose Key
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28516B18-7423-4A97-9223-B97537BD0F79", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Purpose Key", "PurposeKey", "Purpose Key", @"Comma delimited list of purpose key(s) to use for linking the two entities together. While this is not required, it is highly recommended that you provide at least one.", 3, @"", "C7E85161-F8C9-48CF-9B68-EE0B1613DEAD" );

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Parameter Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28516B18-7423-4A97-9223-B97537BD0F79", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Parameter Type", "ParameterType", "Parameter Type", @"Determines the type of the paramters that are being passed in for the source and target. Guids are more secure but requires 2 additional lookups to convert them to integers. The query string parameters are 'Source' and 'Target'", 4, @"Guid", "0F3F0020-B6E7-4D01-A628-E03EECD0EBCB" );

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28516B18-7423-4A97-9223-B97537BD0F79", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The Lava template to use for the header", 5, @"", "49C8046C-FD45-49AF-878D-2C03C2E21F82" );

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Header Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28516B18-7423-4A97-9223-B97537BD0F79", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Header Title", "HeaderTitle", "Header Title", @"The title for the panel heading.", 6, @"Related Entity List", "BA5DD6C2-E488-4524-A0A7-A960613B554C" );

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Header Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "28516B18-7423-4A97-9223-B97537BD0F79", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Header Icon CSS Class", "HeaderIconCssClass", "Header Icon CSS Class", @"The CSS icon for the panel heading.", 7, @"fa fa-link", "CA2228CD-F867-4A3F-82B8-89153EED814F" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Number of Future Weeks To Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Future Weeks To Show", "FutureWeeksToShow", "Number of Future Weeks To Show", @"The number of weeks into the future to allow users to sign up for a schedule.", 0, @"6", "CA39D985-FE68-4DC4-8D15-03426367A2FC" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Sign Up Instructions
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Sign Up Instructions", "SignupInstructions", "Sign Up Instructions", @"Instructions here will show up on Sign Up tab. <span class='tip tip-lava'></span>", 1, @"<div class=""alert alert-info"">
    {%- if IsSchedulesAvailable -%}
        {%- if CurrentPerson.Id == Person.Id -%}
            Sign up to attend a group and location on the given date.
        {%- else -%}
            Sign up {{ Person.FullName }} to attend a group and location on a given date.
        {%- endif -%}
     {%- else -%}
        No sign-ups available.
     {%- endif -%}
</div>", "03FA1A9E-98D0-4D24-AA02-83F7E0D76838" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Decline Reason Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Decline Reason Page", "DeclineReasonPage", "Decline Reason Page", @"If the group type has enabled 'RequiresReasonIfDeclineSchedule' then specify the page to provide that reason here.", 2, @"EA14B522-E2A6-4CA7-8AF0-9CDF0B84C8CF", "2CBD9C52-74DF-4001-A1CB-9D0D16D2E4A3" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Scheduler Receive Confirmation Emails
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Scheduler Receive Confirmation Emails", "SchedulerReceiveConfirmationEmails", "Scheduler Receive Confirmation Emails", @"If checked, the scheduler will receive an email response for each confirmation or decline.", 3, @"False", "55B5538E-8172-425F-8DE4-0152E6DF6AEC" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Scheduling Response Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Scheduling Response Email", "SchedulingResponseEmail", "Scheduling Response Email", @"The system email that will be used for sending responses back to the scheduler.", 4, @"D095F78D-A5CF-4EF6-A038-C7B07E250611", "CB92F222-C76D-44E0-A595-C7B30A3B863E" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Action Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Action Header Lava Template", "ActionHeaderLavaTemplate", "Action Header Lava Template", @"The content to show between the scheduled items and the action buttons. <span class='tip tip-lava'></span>", 5, @"<h4>Actions</h4>", "857C727D-6669-4674-A204-78FD235B7CC9" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Enable Additional Time Sign-Up
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Additional Time Sign-Up", "EnableAdditionalTimeSignUp", "Enable Additional Time Sign-Up", @"When enabled, a button will allow the individual to sign up for upcoming schedules for their group.", 6, @"True", "81E861B2-5DD4-402D-BA34-9356A5AB6689" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Enable Update Schedule Preferences
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Update Schedule Preferences", "EnableUpdateSchedulePreferences", "Enable Update Schedule Preferences", @"When enabled, a button will allow the individual to set their group reminder preferences and preferred schedule.", 7, @"True", "3A3291C3-7687-4629-9D8E-DAAE4AADEECC" );

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Enable Schedule Unavailability
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "18A6DCE3-376C-4A62-B1DD-5E5177C11595", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Schedule Unavailability", "EnableScheduleUnavailability", "Enable Schedule Unavailability", @"When enabled, a button will allow the individual to specify dates or date ranges when they will be unavailable to serve.", 8, @"True", "DE457F43-8F13-4410-AEE5-60214876F5C0" );

            // Attribute for BlockType
            //   BlockType: Prayer Session
            //   Category: Mobile > Prayer
            //   Attribute: Include Group Requests
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Group Requests", "IncludeGroupRequests", "Include Group Requests", @"Includes prayer requests that are attached to a group.", 7, @"False", "69DF4D6F-730B-4F16-BF47-7FFEAFC2EB2D" );

            // Attribute for BlockType
            //   BlockType: My Prayer Requests
            //   Category: Mobile > Prayer
            //   Attribute: Include Group Requests
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C095B269-36E2-446A-B73E-2C8CC4B7BF37", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Group Requests", "IncludeGroupRequests", "Include Group Requests", @"Includes prayer requests that are attached to a group.", 6, @"False", "AB9D18E7-3A33-4641-B872-43F28BCEC5C0" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Always Hide Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Always Hide Campus", "AlwaysHideCampus", "Always Hide Campus", @"Hides the campus picker and disables filtering by campus.", 3, @"False", "54C2C92F-07F3-43B6-804A-47D7C9ACE4AA" );

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Include Group Requests
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA75C558-9345-47E7-99AF-D8191D31D00D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Group Requests", "IncludeGroupRequests", "Include Group Requests", @"Includes prayer requests that are attached to a group.", 12, @"False", "63A63454-1DAA-46F4-AB8C-16D454C25A32" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Connection > WebView
            //   Attribute: Opportunity Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B2E0E4E3-30B1-45BD-B808-C55BCD540894", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Opportunity Template", "OpportunityTemplate", "Opportunity Template", @"This Lava template will be used to display the Connection Types.
                         <i>(Note: The Lava will include the following merge fields:
                            <p><strong>ConnectionOpportunities, DetailPage, ConnectionRequestCounts, CurrentPerson, Context, PageParameter, Campuses</strong>)</p>
                         </i>", 1, @"
{% comment %}
   This is the default lava template for the ConnectionOpportunitySelect block

   Available Lava Fields:
       ConnectionOpportunities
       DetailPage (Detail Page GUID)
       ConnectionRequestCounts
       CurrentPerson
       Context
       PageParameter
       Campuses
{% endcomment %}
<style>
    .card:hover {
      transform: scale(1.01);
      box-shadow: 0 10px 20px rgba(0,0,0,.12), 0 4px 8px rgba(0,0,0,.06);
    }
</style>
{% for connectionOpportunity in ConnectionOpportunities %}
    <a href='{{ DetailPage | Default:'0' | PageRoute }}?ConnectionOpportunityGuid={{ connectionOpportunity.Guid }}' stretched-link>
        <div class='card mb-2'>
            <div class='card-body'>
              <div class='row pt-2' style='height:60px;'>
                    <div class='col-xs-2 col-md-1 mx-auto'>
                        <i class='{{ connectionOpportunity.IconCssClass }} text-gray-600' style=';font-size:30px;'></i>
                    </div>
                    <div class='col-xs-8 col-md-10 pl-md-0 mx-auto'>
                        <span class='text-black'><strong>{{ connectionOpportunity.Name }}</strong></span>
                        </br>
                        <span class='text-gray-600'><small>{{ connectionOpportunity.Description | Truncate:100,'...' }}</small></span>
                    </div>
                    <div class='col-xs-1 col-md-1 text-right mx-auto'>
                        <span class='badge badge-pill badge-primary bg-blue-500'><small>{{ ConnectionRequestCounts[connectionOpportunity.Id] | Map: 'Value' }}</small></span>
                    </div>
                </div>
            </div>
        </div>
       </a>
{% endfor %}
", "A1837E0C-3EFE-44AA-A684-0BFB86A54EAF" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Connection > WebView
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B2E0E4E3-30B1-45BD-B808-C55BCD540894", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to link to when user taps on a connection opportunity. ConnectionOpportunityGuid is passed in the query string.", 2, @"", "C837FE52-18DE-46D7-B1DA-C9B7ED402F88" );

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Connection > WebView
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6BAA42C-D799-4189-ABC9-4A8CA1B91D5A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "RequestTemplate", "Lava Template", @"This Lava template will be used to display the Connection Types.
                         <i>(Note: The Lava will include the following merge fields:
                            <p><strong>ConnectionRequests, ConnectionOpportunity, DetailPage, CurrentPerson, Context, PageParameter, Campuses</strong>)</p>
                         </i>", 1, @"
{% comment %}
   This is the default lava template for the ConnectionOpportunitySelect block

   Available Lava Fields:
       ConnectionRequests
       ConnectionOpportunity
       DetailPage (Detail Page GUID)
       CurrentPerson
       Context
       PageParameter
       Campuses
{% endcomment %}
<style>
    .card:hover {
      transform: scale(1.01);
      box-shadow: 0 10px 20px rgba(0,0,0,.12), 0 4px 8px rgba(0,0,0,.06);
    }

    .person-image-small {
        position: relative;
        box-sizing: border-box;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 40px;
        height: 40px;
        vertical-align: top;
        background: center/cover #cbd4db;
        border-radius: 50%;
        box-shadow: inset 0 0 0 1px rgba(0,0,0,0.07)
    }
</style>
{% for connectionRequest in ConnectionRequests %}
    <a href='{{ DetailPage | Default:'0' | PageRoute }}?ConnectionRequestId={{ connectionRequest.Id }}&ConnectionOpportunityId={{connectionRequest.ConnectionOpportunityId }}' stretched-link>
        <div class='card mb-2'>
            <div class='card-body'>
                <div class='row pt-2' style='height:60px;'>
                    <div class='col-xs-2 col-md-1 mx-auto'>
                        <img class='person-image-small' src='{{ connectionRequest.ConnectorPersonAlias.Person.PhotoUrl | Default: '/Assets/Images/person-no-photo-unknown.svg'  }}' alt=''>
                    </div>
                    <div class='col-xs-6 col-md-9 pl-md-0 mx-auto'>
                       <strong class='text-color'>{{ connectionRequest.ConnectorPersonAlias.Person.FullName | Default: 'Unassigned' }}</strong>
                       <small class='pl-1 text-muted'>{{ connectionRequest.Campus.Name | Default: 'Main Campus' }}</small>
                       </br>
                       {% assign lastActivity = connectionRequest.ConnectionRequestActivities | Last %}
                       <small class='text-muted'>Last Activity: {{ lastActivity.Note | Default: 'No Activity' | Capitalize  }}
                           {% if lastActivity.CreatedDateTime %}
                               ({{ lastActivity.CreatedDateTime | DaysFromNow }})
                           {% endif %}
                       </small>
                    </div>
                    <div class='col-xs-4 col-md-2 mx-auto text-right'>
                        <small class='text-muted'>{{ connectionRequest.CreatedDateTime | Date:'M/d/yyyy' }}</small>
                    </div>
                </div>
            </div>
        </div>
       </a>
{% endfor %}
", "EA43B570-9F84-4A9D-B6AF-CB77776F72CB" );

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Connection > WebView
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6BAA42C-D799-4189-ABC9-4A8CA1B91D5A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to link to when user taps on a connection request. ConnectionRequestGuid is passed in the query string.", 2, @"50f04e77-8d3b-4268-80ab-bc15dd6cb262", "67EB8802-6F24-4B19-8AF4-C78C1A89FF1D" );

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Connection > WebView
            //   Attribute: Max Requests to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6BAA42C-D799-4189-ABC9-4A8CA1B91D5A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Requests to Show", "MaxRequestsToShow", "Max Requests to Show", @"The maximum number of requests to show in a single load, a Load More button will be visible if there are more requests to show.", 3, @"50", "1F7F76C2-0FFB-4EA6-AF1D-A0FAC6178B2C" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Connection > WebView
            //   Attribute: Type Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "887F66AF-944F-4959-87F0-087E3999BAC3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Type Template", "TypeTemplate", "Type Template", @"This Lava template will be used to display the Connection Types.
                         <i>(Note: The Lava will include the following merge fields:
                            <p><strong>ConnectionTypes, DetailPage, ConnectionRequestCounts, CurrentPerson, Context, PageParameter, Campuses</strong>)</p>
                         </i>", 1, @"
{% comment %}
   This is the default lava template for the ConnectionOpportunitySelect block

   Available Lava Fields:
       ConnectionTypes
       DetailPage (Detail Page GUID)
       ConnectionRequestCounts
       CurrentPerson
       Context
       PageParameter
       Campuses
{% endcomment %}
<style>
    .card:hover {
      transform: scale(1.01);
      box-shadow: 0 10px 20px rgba(0,0,0,.12), 0 4px 8px rgba(0,0,0,.06);
    }
</style>
{% for connectionType in ConnectionTypes %}
{% assign count = ConnectionRequestCounts[connectionType.Id] | Map:'Value' | First | AsInteger %}
{% if count >0 %}
    <a href='{{ DetailPage | Default:'0' | PageRoute }}?ConnectionTypeGuid={{ connectionType.Guid }}' stretched-link>
        <div class='card mb-2'>
            <div class='card-body'>
              <div class='row pt-2' style='height:60px;'>
                    <div class='col-xs-2 col-md-1 mx-auto'>
                        <i class='{{ connectionType.IconCssClass }} text-muted' style=';font-size:30px;'></i>
                    </div>
                    <div class='col-xs-8 col-md-10 pl-md-0 mx-auto'>
                        <span class='text-color'><strong>{{ connectionType.Name }}</strong></span>
                        </br>
                        <span class='text-muted'><small>{{ connectionType.Description }}</small></span>
                    </div>
                    <div class='col-xs-1 col-md-1 mx-auto text-right'>
                        <span class='badge badge-pill badge-primary bg-blue-500'><small>{{ count }}</small></span>
                    </div>
                </div>
            </div>
        </div>
       </a>
{%endif %}
{% endfor %}", "CF1B0024-A780-4145-A716-04A378534F89" );

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Connection > WebView
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "887F66AF-944F-4959-87F0-087E3999BAC3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to link to when user taps on a connection type. ConnectionTypeGuid is passed in the query string.", 2, @"", "D35F5EAE-8ED7-4A2D-B9CD-A3AE443A5349" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Connection > WebView
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("D35F5EAE-8ED7-4A2D-B9CD-A3AE443A5349");

            // Attribute for BlockType
            //   BlockType: Connection Type List
            //   Category: Connection > WebView
            //   Attribute: Type Template
            RockMigrationHelper.DeleteAttribute("CF1B0024-A780-4145-A716-04A378534F89");

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Connection > WebView
            //   Attribute: Max Requests to Show
            RockMigrationHelper.DeleteAttribute("1F7F76C2-0FFB-4EA6-AF1D-A0FAC6178B2C");

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Connection > WebView
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("67EB8802-6F24-4B19-8AF4-C78C1A89FF1D");

            // Attribute for BlockType
            //   BlockType: Connection Request List
            //   Category: Connection > WebView
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute("EA43B570-9F84-4A9D-B6AF-CB77776F72CB");

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Connection > WebView
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("C837FE52-18DE-46D7-B1DA-C9B7ED402F88");

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Connection > WebView
            //   Attribute: Opportunity Template
            RockMigrationHelper.DeleteAttribute("A1837E0C-3EFE-44AA-A684-0BFB86A54EAF");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Include Group Requests
            RockMigrationHelper.DeleteAttribute("63A63454-1DAA-46F4-AB8C-16D454C25A32");

            // Attribute for BlockType
            //   BlockType: Prayer Card View
            //   Category: Mobile > Prayer
            //   Attribute: Always Hide Campus
            RockMigrationHelper.DeleteAttribute("54C2C92F-07F3-43B6-804A-47D7C9ACE4AA");

            // Attribute for BlockType
            //   BlockType: My Prayer Requests
            //   Category: Mobile > Prayer
            //   Attribute: Include Group Requests
            RockMigrationHelper.DeleteAttribute("AB9D18E7-3A33-4641-B872-43F28BCEC5C0");

            // Attribute for BlockType
            //   BlockType: Prayer Session
            //   Category: Mobile > Prayer
            //   Attribute: Include Group Requests
            RockMigrationHelper.DeleteAttribute("69DF4D6F-730B-4F16-BF47-7FFEAFC2EB2D");

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Enable Schedule Unavailability
            RockMigrationHelper.DeleteAttribute("DE457F43-8F13-4410-AEE5-60214876F5C0");

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Enable Update Schedule Preferences
            RockMigrationHelper.DeleteAttribute("3A3291C3-7687-4629-9D8E-DAAE4AADEECC");

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Enable Additional Time Sign-Up
            RockMigrationHelper.DeleteAttribute("81E861B2-5DD4-402D-BA34-9356A5AB6689");

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Action Header Lava Template
            RockMigrationHelper.DeleteAttribute("857C727D-6669-4674-A204-78FD235B7CC9");

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Scheduling Response Email
            RockMigrationHelper.DeleteAttribute("CB92F222-C76D-44E0-A595-C7B30A3B863E");

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Scheduler Receive Confirmation Emails
            RockMigrationHelper.DeleteAttribute("55B5538E-8172-425F-8DE4-0152E6DF6AEC");

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Decline Reason Page
            RockMigrationHelper.DeleteAttribute("2CBD9C52-74DF-4001-A1CB-9D0D16D2E4A3");

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Sign Up Instructions
            RockMigrationHelper.DeleteAttribute("03FA1A9E-98D0-4D24-AA02-83F7E0D76838");

            // Attribute for BlockType
            //   BlockType: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Attribute: Number of Future Weeks To Show
            RockMigrationHelper.DeleteAttribute("CA39D985-FE68-4DC4-8D15-03426367A2FC");

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Header Icon CSS Class
            RockMigrationHelper.DeleteAttribute("CA2228CD-F867-4A3F-82B8-89153EED814F");

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Header Title
            RockMigrationHelper.DeleteAttribute("BA5DD6C2-E488-4524-A0A7-A960613B554C");

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteAttribute("49C8046C-FD45-49AF-878D-2C03C2E21F82");

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Parameter Type
            RockMigrationHelper.DeleteAttribute("0F3F0020-B6E7-4D01-A628-E03EECD0EBCB");

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Purpose Key
            RockMigrationHelper.DeleteAttribute("C7E85161-F8C9-48CF-9B68-EE0B1613DEAD");

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Target Entity Type
            RockMigrationHelper.DeleteAttribute("577179E8-71AE-4312-96B2-39F24280FBD3");

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Source Is Current Person
            RockMigrationHelper.DeleteAttribute("031ABF7B-39FF-4B70-99E3-CF027A4CEDFE");

            // Attribute for BlockType
            //   BlockType: Related Entity List
            //   Category: Core
            //   Attribute: Source Entity Type
            RockMigrationHelper.DeleteAttribute("58A22A5C-1719-457B-A38F-D94BB8D1F883");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Attribute Columns
            RockMigrationHelper.DeleteAttribute("46C303B6-C3D7-48C5-B35C-E909FDEF76F5");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Header Icon CSS Class
            RockMigrationHelper.DeleteAttribute("D7B4479B-E073-429C-A9EA-EE9BA97F438C");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Header Title
            RockMigrationHelper.DeleteAttribute("4BAAE325-4D30-4B01-A31D-37067574ACCF");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Header Lava Template
            RockMigrationHelper.DeleteAttribute("7813F236-AE07-4C65-BD5F-67637BA79AAC");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Enable Attribute Editing
            RockMigrationHelper.DeleteAttribute("9CD89F31-F7D5-4558-9C0C-43B1FF9FA207");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Show Note
            RockMigrationHelper.DeleteAttribute("25693AEB-9DD2-43E9-922E-40EF72F2FF60");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Show Quantity
            RockMigrationHelper.DeleteAttribute("5C6E3C6D-F9F1-4ED7-87E3-6AC49ACD1E35");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Enable Edit
            RockMigrationHelper.DeleteAttribute("B03AF8EA-46B4-4B2D-9E78-EF1CCE98BE3A");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Parameter Type
            RockMigrationHelper.DeleteAttribute("7FA01B7D-5F2D-4B30-8792-0769070D69E5");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Purpose Key
            RockMigrationHelper.DeleteAttribute("9BFE4046-23B1-45A3-B5DF-9D5B133D50F2");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Target Entity Type
            RockMigrationHelper.DeleteAttribute("093585C3-1070-4539-9680-B2E72E4C47E2");

            // Attribute for BlockType
            //   BlockType: Related Entity Connect
            //   Category: Core
            //   Attribute: Source Entity Type
            RockMigrationHelper.DeleteAttribute("9DF6277C-1378-4584-832A-5B8F4FE2D3F6");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute("97979723-0F10-4FD1-8A39-B5E735A5504C");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.DeleteAttribute("5F639EE6-EFF1-4320-BAFE-CCD2BD8117FC");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.DeleteAttribute("ABEB4044-3919-4D7E-BBBF-A89A4ABF9028");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.DeleteAttribute("A94814B9-614F-4519-A17D-986A3064409B");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.DeleteAttribute("5F3FC9FB-1815-4AEA-BE66-8BFFFDE43F77");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.DeleteAttribute("69B84180-5B6C-447E-B906-A3D8511148D4");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.DeleteAttribute("07B4D043-168F-4630-AB13-B9E52ED2750E");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.DeleteAttribute("4905B2D8-21C3-4748-AB4E-7C6A69FCE692");

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Workflow Entry Page
            RockMigrationHelper.DeleteAttribute("11E5B09E-E835-4D5B-A7B8-D86544E2179E");

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Workflow Detail Page
            RockMigrationHelper.DeleteAttribute("7BCE9779-BED6-43F2-A4A8-D8FB5C022026");

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Display Middle Name
            RockMigrationHelper.DeleteAttribute("8E25EB26-23A4-432D-8DD0-FD2CFF9F6F5C");

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Display Government Id
            RockMigrationHelper.DeleteAttribute("FC2BFF21-5E7A-4084-975D-A781C197F2B9");

            // Attribute for BlockType
            //   BlockType: Benevolence Request Detail
            //   Category: Finance
            //   Attribute: Display Country Code
            RockMigrationHelper.DeleteAttribute("77B2BA79-B632-46AE-9A15-A9DBB66F54F6");

            // Attribute for BlockType
            //   BlockType: Connection Request Detail
            //   Category: Connection
            //   Attribute: Activity Lava Template
            RockMigrationHelper.DeleteAttribute("34CE545D-167F-4EC6-97A4-603C85ED8243");

            // Delete BlockType 
            //   Name: Group Schedule Toolbox v2
            //   Category: Group Scheduling
            //   Path: ~/Blocks/GroupScheduling/GroupScheduleToolboxV2.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("18A6DCE3-376C-4A62-B1DD-5E5177C11595");

            // Delete BlockType 
            //   Name: Related Entity List
            //   Category: Core
            //   Path: ~/Blocks/Core/RelatedEntityList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("28516B18-7423-4A97-9223-B97537BD0F79");

            // Delete BlockType 
            //   Name: Related Entity Connect
            //   Category: Core
            //   Path: ~/Blocks/Core/RelatedEntityConnect.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("5F40F4FD-338A-4711-87F7-980ED1FAE615");

            // Delete BlockType 
            //   Name: Attributes
            //   Category: Obsidian > Core
            //   Path: -
            //   EntityType: Attributes
            RockMigrationHelper.DeleteBlockType("8B34B1D7-B429-4F3E-8CE6-4510E0DE8D2A");
        }
    }
}
