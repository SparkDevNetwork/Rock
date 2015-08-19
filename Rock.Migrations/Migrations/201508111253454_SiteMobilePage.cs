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
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class SiteMobilePage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex("dbo.RegistrationInstance", new[] { "AccountId" });
            AddColumn("dbo.Site", "EnableMobileRedirect", c => c.Boolean(nullable: false));
            AddColumn("dbo.Site", "MobilePageId", c => c.Int());
            AddColumn("dbo.Site", "ExternalUrl", c => c.String(maxLength: 260));
            AddColumn("dbo.Site", "RedirectTablets", c => c.Boolean(nullable: false));
            AlterColumn("dbo.RegistrationInstance", "AccountId", c => c.Int());
            AlterColumn("dbo.RegistrationTemplate", "MinimumInitialPayment", c => c.Decimal(precision: 18, scale: 2));
            CreateIndex("dbo.RegistrationInstance", "AccountId");
            CreateIndex("dbo.Site", "MobilePageId");
            AddForeignKey("dbo.Site", "MobilePageId", "dbo.Page", "Id");

            // DT: Content Item Page Parameter on registration linkages
            RockMigrationHelper.AddBlockTypeAttribute( "94230E7A-8EB7-4407-9B8E-888B54C71E39", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Detail Page", "ContentItemDetailPage", "", "The page for viewing details about a content item", 3, @"", "D4C4DDD1-E99E-499B-A388-15EEDB29A9AE" );
            RockMigrationHelper.AddBlockAttributeValue( "828C8FE3-D5F8-4C22-BA81-844D704842EA", "D4C4DDD1-E99E-499B-A388-15EEDB29A9AE", @"d18e837c-9e65-4a38-8647-dff04a595d97" );

            // JE: Update Dashboard Lava
            RockMigrationHelper.AddBlockAttributeValue( "415575C3-70AC-4A7A-8936-B98464C5557F", "D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2", @"<div class='panel panel-block'> 
    <div class='panel-heading'>
        <h4 class='panel-title'>My Assigned Tasks</h4>
    </div>
    <div class='panel-body'>
        {% if Actions.size > 0 %}
            <div class='container-fluid'>
                <div class='row margin-b-sm hidden-sm hidden-xs'>
                    <div class='col-md-2'>
                        <strong>Date Created</strong>
                    </div>
                    <div class='col-md-6'>
                        <strong>Task Name</strong>
                    </div>
                    <div class='col-md-4'>
                        
                    </div>
                </div>
                
                {% for action in Actions %}
                    <div class='row margin-b-sm'>
                        <div class='col-md-2'>
                            {{ action.Activity.Workflow.CreatedDateTime | Date:'M/d/yyyy' }}
                        </div>
                        <div class='col-md-8'>
                            <i class='fa-fw {{ action.Activity.Workflow.WorkflowType.IconCssClass }}'></i>  
                            <a href='~/{% if Role == '0' %}WorkflowEntry/{{ action.Activity.Workflow.WorkflowTypeId }}{% else %}Workflow{% endif %}/{{ action.Activity.Workflow.Id }}'>{{ action.Activity.Workflow.WorkflowType.Name }}: {{ action.Activity.Workflow.Name }}{% if role == '0' %} ({{ action.Activity.ActivityType.Name }}){% endif %}</a>
                        </div>
                        <div class='col-md-2'>
                            {% if action.Activity.Workflow.Status != 'Active' %}
                                <span class='label label-info'>{{ action.Activity.Workflow.Status }}</span>
                            {% endif %}
                        </div>
                    </div>
                    
                    <hr class='visible-xs-block visible-sm-block' />
                {% endfor %}
            </div>
        {% else %}
            <div class='alert alert-info'>There are no open tasks assigned to you.</div>
        {% endif %}
    </div>
</div>
" );

            // Attrib for BlockType: Group Detail:Map HTML
            RockMigrationHelper.AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Map HTML", "MapHTML", "", "The HTML to use for displaying group location maps. Liquid syntax is used to render data from the following data structure: points[type, latitude, longitude], polygons[type, polygon_wkt, google_encoded_polygon]", 3, @"
    {% for point in points %}
        <div class='group-location-map'>
            <h4>{{ point.type }}</h4>
            <img class='img-thumbnail' src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&zoom=13&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}&visual_refresh=true'/>
        </div>
    {% endfor %}
    {% for polygon in polygons %}
        <div class='group-location-map'>
            <h4>{{ polygon.type }}</h4>
            <img class='img-thumbnail' src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60&visual_refresh=true&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}'/>
        </div>
    {% endfor %}
", "0D459868-02FD-4AB7-9A9C-92ACFCBB0FDC" );


            // Attrib for BlockType: Location Detail:Map HTML
            RockMigrationHelper.AddBlockTypeAttribute( "08189564-1245-48F8-86CC-560F4DD48733", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Map HTML", "MapHTML", "", "The HTML to use for displaying group location maps. Liquid syntax is used to render data from the following data structure: points[type, latitude, longitude], polygons[type, polygon_wkt, google_encoded_polygon]", 0, @"
    {% if point or polygon %}
        <div class='group-location-map'>
            <img class='img-thumbnail' src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60{% if point %}&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}{% endif %}{% if polygon %}&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}{% endif %}&visual_refresh=true'/>
        </div>
    {% endif %}
", "A4BE7F16-A8E0-4F2F-AFFD-9323AEE51F1D" );

            RockMigrationHelper.AddBlockAttributeValue( "71334750-1F04-41DE-BEC4-8350BBC7D844", "A4BE7F16-A8E0-4F2F-AFFD-9323AEE51F1D", @"
    {% if point or polygon %}
        <div class='group-location-map'>
            <img class='img-thumbnail' src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60{% if point %}&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}{% endif %}{% if polygon %}&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}{% endif %}&visual_refresh=true'/>
        </div>
    {% endif %}
" );

            // Attrib for BlockType: Group Detail:Map HTML
            RockMigrationHelper.AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Map HTML", "MapHTML", "", "The HTML to use for displaying group location maps. Liquid syntax is used to render data from the following data structure: points[type, latitude, longitude], polygons[type, polygon_wkt, google_encoded_polygon]", 3, @"
    {% for point in points %}
        <div class='group-location-map'>
            <h4>{{ point.type }}</h4>
            <img class='img-thumbnail' src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&zoom=13&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}&visual_refresh=true'/>
        </div>
    {% endfor %}
    {% for polygon in polygons %}
        <div class='group-location-map'>
            <h4>{{ polygon.type }}</h4>
            <img class='img-thumbnail' src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60&visual_refresh=true&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}'/>
        </div>
    {% endfor %}
", "0D459868-02FD-4AB7-9A9C-92ACFCBB0FDC" );

            Sql( MigrationSQL._201508111253454_SiteMobilePage );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Site", "MobilePageId", "dbo.Page");
            DropIndex("dbo.Site", new[] { "MobilePageId" });
            DropIndex("dbo.RegistrationInstance", new[] { "AccountId" });
            AlterColumn("dbo.RegistrationTemplate", "MinimumInitialPayment", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.RegistrationInstance", "AccountId", c => c.Int(nullable: false));
            DropColumn("dbo.Site", "RedirectTablets");
            DropColumn("dbo.Site", "ExternalUrl");
            DropColumn("dbo.Site", "MobilePageId");
            DropColumn("dbo.Site", "EnableMobileRedirect");
            CreateIndex("dbo.RegistrationInstance", "AccountId");
        }
    }
}
