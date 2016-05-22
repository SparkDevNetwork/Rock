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
    public partial class GroupMap : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateFieldType( "Value List", "", "Rock", "Rock.Field.Types.ValueListFieldType", "7BDAE237-6E49-47AC-9961-A45AFB69E240" );

            Sql( @"
    UPDATE [Page] SET
	    [InternalName] = 'Attendance Analysis' 
	    ,[PageTitle] = 'Attendance Analysis' 
	    ,[BrowserTitle] = 'Attendance Analysis'  
    WHERE [Guid] = '7A3CF259-1090-403C-83B7-2DB3A53DEE26'

    UPDATE [GroupType] SET
         [GroupTerm] = 'Family'
        ,[GroupMemberTerm] = 'Family Member'
    WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E'

    UPDATE [Block]
      SET [PreHtml] = REPLACE([PreHtml], 'panel-info', 'panel-block')
      WHERE [Guid] IN ('6A648E77-ABA9-4AAF-A8BB-027A12261ED9', 'CB8F9152-08BB-4576-B7A1-B0DDD9880C44', '03FCBF5A-42E0-4F45-B670-BC8E324BD573')

    UPDATE AV
    SET [Value] = REPLACE([Value], 'panel-info', 'panel-block')
    FROM [AttributeValue] AV
        INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
    WHERE A.[Guid] = 'D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2'

    -- Update the MarkerColor attribute to be multi-value instead
    DECLARE @FieldTypeId int = ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '7BDAE237-6E49-47AC-9961-A45AFB69E240' )
    UPDATE [Attribute] SET
        [FieldTypeId] = @FieldTypeId
        ,[Key] = 'Colors'
        ,[Name] = 'Colors'
        ,[Description] = 'The colors to use for markers and/or shapes on the map.'
        ,[DefaultValue] = '#fe7569|#a7fe68|#68fe74|#68f2fe|#7468fe|#bf68fe|'
    WHERE GUID = '215AC212-DEA8-412D-BEA9-06A777D20DFD'

    UPDATE AV SET 
        [Value] = '#ee7624|#446f7a|#afd074|#afd074|#f8eba2|#85d4e7|'
    FROM [AttributeValue] AV
        INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeID]
        INNER JOIN [DefinedValue] DV ON DV.[Id] = AV.[EntityId]
    WHERE A.[Guid] = '215AC212-DEA8-412D-BEA9-06A777D20DFD'
        AND DV.[Guid] = 'FDC5D6BA-A818-4A06-96B1-9EF31B4087AC'
" );
            RockMigrationHelper.AddDefinedTypeAttribute( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Color", "Color", "", 0, "", "23777A50-E000-4F29-994F-26635A357160" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "41540783-D9EF-4C70-8F1D-C9E83D91ED5F", "23777A50-E000-4F29-994F-26635A357160", "#f37833" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "39F491C5-D6AC-4A9B-8AC0-C431CB17D588", "23777A50-E000-4F29-994F-26635A357160", "#396772" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B91BA046-BC1E-400C-B85D-638C1F4E0CE2", "23777A50-E000-4F29-994F-26635A357160", "#afd074" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "727AD896-5310-4EEF-A3EC-10C8DE52D574", "23777A50-E000-4F29-994F-26635A357160", "#CCCCCC" );

            RockMigrationHelper.AddPage( "4E237286-B715-4109-A578-C1445EC02707", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Map", "", "60995C8C-862F-40F5-AFBB-13B49CDA77EB", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Group Map", "Displays a group (and any child groups) on a map.", "~/Blocks/Groups/GroupMap.ascx", "Groups", "967F0D2B-DB76-486A-B034-D22B9D9240D3" );
            // Add Block to Page: Group Map, Site: Rock RMS
            RockMigrationHelper.AddBlock( "60995C8C-862F-40F5-AFBB-13B49CDA77EB", "", "967F0D2B-DB76-486A-B034-D22B9D9240D3", "Group Map", "Main", "", "", 0, "80F0BB81-2209-4744-A6D9-747C6CE10760" );

            // Attrib for BlockType: Group Map:Map Height
            RockMigrationHelper.AddBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Map Height", "MapHeight", "", "Height of the map in pixels (default value is 600px)", 4, @"600", "BB10E9D3-DEDF-40C9-AE13-97F74B47472E" );
            // Attrib for BlockType: Group Map:Map Style
            RockMigrationHelper.AddBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "The map theme that should be used for styling the map.", 3, @"BFC46259-FB66-4427-BF05-2B030A582BEA", "FC4B338F-C2DC-4623-A6A5-C4712B70FDFD" );
            // Attrib for BlockType: Group Detail:Group Map Page
            RockMigrationHelper.AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Map Page", "GroupMapPage", "", "The page to display detailed group map.", 0, @"", "69F9C989-456D-4855-A420-050DB8B9FEB7" );
            // Attrib for BlockType: Group Map:Polygon Colors
            RockMigrationHelper.AddBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Polygon Colors", "PolygonColors", "", "Comma-Delimited list of colors to use when displaying multiple polygons (e.g. #F71E22,#E75C1F,#E7801,#F7A11F).", 5, @"#F71E22,#E75C1F,#E7801,#F7A11F", "71DBCABB-444E-46C3-932E-1D8EF487D23C" );
            // Attrib for BlockType: Group Map:Info Window Contents
            RockMigrationHelper.AddBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Info Window Contents", "InfoWindowContents", "", "Liquid template for the info window. To suppress the window provide a blank template.", 6, @"
<div style='width:250px'>

    <div class='clearfix'>
        <h4 class='pull-left' style='margin-top: 0;'>{{ GroupName }}</h4> 
        <span class='label label-campus pull-right'>{{ Campus.Name }}</span>
    </div>
    
    <div class='clearfix'>
		{% if Location.Street1 and Location.Street1 != '' %}
			<strong>{{ Location.Type }}</strong>
			<br>{{ Location.Street1 }}
			<br>{{ Location.City }}, {{ Location.State }} {{ Location.Zip }}
		{% endif %}
		{% if Members.size > 0 %}
			<br>
			<br><strong>{{ GroupType.GroupMemberTerm }}s</strong><br>
			{% for GroupMember in Members -%}
				<div class='clearfix'>
					{% if GroupMember.PhotoUrl != '' %}
						<div class='pull-left' style='padding: 0 5px 2px 0'>
							<img src='{{ GroupMember.PhotoUrl }}&maxheight=50&maxwidth=50'>
						</div>
					{% endif %}
					<a href='{{ GroupMember.ProfilePageUrl }}'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a> - {{ GroupMember.Role }}
                    {% if groupTypeGuid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus != '' %}
				        <br>{{ GroupMember.ConnectionStatus }}
					{% endif %}
					{% if GroupMember.Email != '' %}
						<br>{{ GroupMember.Email }}
					{% endif %}
					{% for Phone in GroupMember.Person.PhoneNumbers %}
						<br>{{ Phone.NumberTypeValue.Name }}: {{ Phone.NumberFormatted }}
					{% endfor %}
				</div>
				<br>
			{% endfor -%}
		{% endif %}
    </div>
    
    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
		<br>
		<a class='btn btn-xs btn-action' href='{{ DetailPageUrl }}'>View {{ GroupType.GroupTerm }}</a>
		<a class='btn btn-xs btn-action' href='{{ MapPageUrl }}'>View Map</a>
	{% endif %}

</div>
", "92B339D5-D8AF-4810-A7F8-09373DC5D0DE" );
            // Attrib for BlockType: Group Map:Group Page
            RockMigrationHelper.AddBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Page", "GroupPage", "", "The page to display group details.", 0, @"", "783DFFB7-92C0-4566-943C-4084BB82249F" );
            // Attrib for BlockType: Group Map:Person Profile Page
            RockMigrationHelper.AddBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "The page to display person details.", 1, @"", "FB67256F-6807-4AA4-B362-759460A68583" );
            // Attrib for BlockType: Group Map:Map Page
            RockMigrationHelper.AddBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Map Page", "MapPage", "", "The page to display group map (typically this page).", 2, @"", "B74BA0CA-B9A4-4146-86F8-55587F7C849C" );

            // Attrib Value for Block:GroupDetailRight, Attribute:Group Map Page Page: Group Viewer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "88344FE3-737E-4741-A38D-D2D3A1653818", "69F9C989-456D-4855-A420-050DB8B9FEB7", @"60995c8c-862f-40f5-afbb-13b49cda77eb" );

            // Attrib Value for Block:Group Map, Attribute:Map Height Page: Group Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "80F0BB81-2209-4744-A6D9-747C6CE10760", "BB10E9D3-DEDF-40C9-AE13-97F74B47472E", @"600" );
            // Attrib Value for Block:Group Map, Attribute:Map Style Page: Group Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "80F0BB81-2209-4744-A6D9-747C6CE10760", "FC4B338F-C2DC-4623-A6A5-C4712B70FDFD", @"fdc5d6ba-a818-4a06-96b1-9ef31b4087ac" );
            // Attrib Value for Block:Group Map, Attribute:Polygon Colors Page: Group Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "80F0BB81-2209-4744-A6D9-747C6CE10760", "71DBCABB-444E-46C3-932E-1D8EF487D23C", @"#f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc" );
            // Attrib Value for Block:Group Map, Attribute:Info Window Contents Page: Group Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "80F0BB81-2209-4744-A6D9-747C6CE10760", "92B339D5-D8AF-4810-A7F8-09373DC5D0DE", @"<div style='width:250px'>

    <div class='clearfix'>
        <h4 class='pull-left' style='margin-top: 0;'>{{ GroupName }}</h4> 
        <span class='label label-campus pull-right'>{{ Campus.Name }}</span>
    </div>
    
    <div class='clearfix'>
		{% if Location.Street1 and Location.Street1 != '' %}
			<strong>{{ Location.Type }}</strong>
			<br>{{ Location.Street1 }}
			<br>{{ Location.City }}, {{ Location.State }} {{ Location.Zip }}
		{% endif %}
		{% if Members.size > 0 %}
			<br>
			<br><strong>{{ GroupType.GroupMemberTerm }}s</strong><br>
			{% for GroupMember in Members -%}
				<div class='clearfix'>
					{% if GroupMember.PhotoUrl != '' %}
						<div class='pull-left' style='padding: 0 5px 2px 0'>
							<img src='{{ GroupMember.PhotoUrl }}&maxheight=50&maxwidth=50'>
						</div>
					{% endif %}
					<a href='{{ GroupMember.ProfilePageUrl }}'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a> - {{ GroupMember.Role }}
                    {% if groupTypeGuid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus != '' %}
				        ({{ GroupMember.ConnectionStatus }})
					{% endif %}
				</div>
			{% endfor -%}
		{% endif %}
    </div>
    
    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
		<br>
		<a class='btn btn-xs btn-action' href='{{ DetailPageUrl }}'>View {{ GroupType.GroupTerm }}</a>
		<a class='btn btn-xs btn-action' href='{{ MapPageUrl }}'>View Map</a>
	{% endif %}

</div>" );
            // Attrib Value for Block:Group Map, Attribute:Group Page Page: Group Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "80F0BB81-2209-4744-A6D9-747C6CE10760", "783DFFB7-92C0-4566-943C-4084BB82249F", @"4e237286-b715-4109-a578-c1445ec02707" );
            // Attrib Value for Block:Group Map, Attribute:Person Profile Page Page: Group Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "80F0BB81-2209-4744-A6D9-747C6CE10760", "FB67256F-6807-4AA4-B362-759460A68583", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25" );
            // Attrib Value for Block:Group Map, Attribute:Map Page Page: Group Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "80F0BB81-2209-4744-A6D9-747C6CE10760", "B74BA0CA-B9A4-4146-86F8-55587F7C849C", @"60995c8c-862f-40f5-afbb-13b49cda77eb" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "23777A50-E000-4F29-994F-26635A357160" );

            // Attrib for BlockType: Group Map:Map Page
            RockMigrationHelper.DeleteAttribute( "B74BA0CA-B9A4-4146-86F8-55587F7C849C" );
            // Attrib for BlockType: Group Map:Person Profile Page
            RockMigrationHelper.DeleteAttribute( "FB67256F-6807-4AA4-B362-759460A68583" );
            // Attrib for BlockType: Group Map:Group Page
            RockMigrationHelper.DeleteAttribute( "783DFFB7-92C0-4566-943C-4084BB82249F" );
            // Attrib for BlockType: Group Map:Info Window Contents
            RockMigrationHelper.DeleteAttribute( "92B339D5-D8AF-4810-A7F8-09373DC5D0DE" );
            // Attrib for BlockType: Group Map:Polygon Colors
            RockMigrationHelper.DeleteAttribute( "71DBCABB-444E-46C3-932E-1D8EF487D23C" );
            // Attrib for BlockType: Group Detail:Group Map Page
            RockMigrationHelper.DeleteAttribute( "69F9C989-456D-4855-A420-050DB8B9FEB7" );
            // Attrib for BlockType: Group Map:Map Style
            RockMigrationHelper.DeleteAttribute( "FC4B338F-C2DC-4623-A6A5-C4712B70FDFD" );
            // Attrib for BlockType: Group Map:Map Height
            RockMigrationHelper.DeleteAttribute( "BB10E9D3-DEDF-40C9-AE13-97F74B47472E" );

            // Remove Block: Group Map, from Page: Group Map, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "80F0BB81-2209-4744-A6D9-747C6CE10760" );

            RockMigrationHelper.DeleteBlockType( "967F0D2B-DB76-486A-B034-D22B9D9240D3" ); // Group Map
            RockMigrationHelper.DeletePage( "60995C8C-862F-40F5-AFBB-13B49CDA77EB" ); // Page: Group MapLayout: Full Width, Site: Rock RMS
        }
    }
}
