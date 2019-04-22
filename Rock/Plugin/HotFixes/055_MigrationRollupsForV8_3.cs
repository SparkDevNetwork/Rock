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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 55, "1.8.2" )]
    public class MigrationRollupsForV8_3 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // GP: Update Homepage Migration
//            Sql( @"UPDATE [dbo].[Metric]
//SET [SourceSql] = 'SELECT COUNT(*) 
//FROM [Person]
//WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)'
//WHERE GUID = 'ecb1b552-9a3d-46fc-952b-d57dbc4a329d'

//UPDATE [dbo].[Metric]
//SET [SourceSql] = 'DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = ''790e3215-3b10-442b-af69-616c0dcb998e'')
//SELECT COUNT( DISTINCT(g.[Id])) 
//FROM [Person] p
//    INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
//    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = @FamilyGroupTypeId
//WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)'
//WHERE GUID = '491061b7-1834-44da-8ea1-bb73b2d52ad3'" );

//            Sql( @"UPDATE [dbo].[MetricValue]
//SET [YValue] = (SELECT COUNT(*) 
//FROM [Person]
//WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)),
//[MetricValueDateTime] = '" + RockDateTime.Now.ToString( "s" ) + @"'
//WHERE [Guid] = '34325795-9016-47e9-a9d9-6283d1a84275'" ); // Active Records

//            Sql( @"UPDATE [dbo].[MetricValue]
//SET [YValue] = (SELECT COUNT( DISTINCT(g.[Id])) 
//FROM [Person] p
//    INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
//    INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] AND g.[GroupTypeId] = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '790e3215-3b10-442b-af69-616c0dcb998e')
//WHERE ([RecordTypeValueId] = 1) AND ([RecordStatusValueId] = 3)),
//[MetricValueDateTime] = '" + RockDateTime.Now.ToString( "s" ) + @"'
//WHERE [Guid] = '932479dd-9612-4d07-b9cd-9227976cf5dd'" ); //Active Families

//            Sql( @"UPDATE [dbo].[MetricValue]
//SET [YValue] = (SELECT COUNT(*) 
//FROM [ConnectionRequest]
//WHERE [ConnectionState] = 0),
//[MetricValueDateTime] = '" + RockDateTime.Now.ToString( "s" ) + @"'
//WHERE [Guid] = '90cd5a83-3079-4656-b7ce-bfa21055c980'" ); // Active Connection Requests

//            // GJ: Update Wordcloud Shortcode
//            Sql( @"UPDATE [LavaShortCode] 
//SET [Markup] = '{% javascript id:''d3-layout-cloud'' url:''~/Scripts/d3-cloud/d3.layout.cloud.js'' %}{% endjavascript %}
//{% javascript id:''d3-min'' url:''~/Scripts/d3-cloud/d3.min.js'' %}{% endjavascript %}

//<div id=""{{ uniqueid }}"" style=""width: {{ width }}; height: {{ height }};""></div>

//{%- assign anglecount = anglecount | Trim -%}
//{%- assign anglemin = anglemin | Trim -%}
//{%- assign anglemax = anglemax | Trim -%}

//{% javascript disableanonymousfunction:''true'' %}
//    $( document ).ready(function() {
//        Rock.controls.wordcloud.initialize({
//            inputTextId: ''hf-{{ uniqueid }}'',
//            visId: ''{{ uniqueid }}'',
//            width: ''{{ width }}'',
//            height: ''{{ height }}'',
//            fontName: ''{{ fontname }}'',
//            maxWords: {{ maxwords }},
//            scaleName: ''{{ scalename }}'',
//            spiralName: ''{{ spiralname}}'',
//            colors: [ ''{{ colors | Replace:'','',""'',''"" }}''],
//            {%- if anglecount != '''' %}
//            anglecount: {{ anglecount }}{%- if anglemin != '''' or anglemax != '''' -%},{%- endif -%}
//            {%- endif -%}
//            {%- if anglemin != '''' %}
//            anglemin: {{ anglemin }}{%- if anglemax != '''' -%},{%- endif -%}
//            {%- endif -%}
//            {%- if anglemax != '''' %}
//            anglemax: {{ anglemax }}
//            {%- endif -%}
//        });
//    });
//{% endjavascript %}

//<input type=""hidden"" id=""hf-{{ uniqueid }}"" value=""{{ blockContent }}"" />' WHERE [Guid] = '4B6452EF-6FEA-4A66-9FB9-1A7CCE82E7A4'" );

            // This plug-in Migration is intentionally left out of release migrations
//            // GP: Disable NCOA
//            Sql( @"DECLARE @entityTypeId int
//SET @entityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '" + Rock.SystemGuid.EntityType.PAGE.AsGuid() + @"')

//DECLARE @pageId int
//SET @pageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0591e498-0ad6-45a5-b8ca-9bca5c771f03')

//INSERT INTO [dbo].[Auth]
//           ([EntityTypeId]
//           ,[EntityId]
//           ,[Order]
//           ,[Action]
//           ,[AllowOrDeny]
//           ,[SpecialRole]
//           ,[GroupId]
//           ,[Guid])
//     VALUES
//           (@entityTypeId
//           ,@pageId
//           ,0
//           ,'View'
//           ,'D'
//           ,1
//           ,NULL
//           ,'8628653e-f4d8-42b4-4793-38647fada3f1')

//UPDATE [dbo].[ServiceJob]
//   SET [IsActive] = 0, [Description] = 'Job that gets National Change of Address (NCOA) data - coming soon'
// WHERE Guid = 'D2D6EA6C-F94A-39A0-481B-A23D08B887D6'" );


        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
