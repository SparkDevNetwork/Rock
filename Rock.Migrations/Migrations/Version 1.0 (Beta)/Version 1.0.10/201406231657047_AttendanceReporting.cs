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
    public partial class AttendanceReporting : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /* Attendance Reporting Related... */
            RockMigrationHelper.AddPage( "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Attendance", "", "7A3CF259-1090-403C-83B7-2DB3A53DEE26", "" ); // Site:Rock RMS
            
            RockMigrationHelper.UpdateBlockType( "Attendance Reporting", "Shows a graph of attendance statistics which can be configured to show attendance for specific groups, date range, etc.", "~/Blocks/CheckIn/AttendanceReporting.ascx", "Check-in", "3CD3411C-C076-4344-A9D5-8F3B4F01E31D" );

            // Add Block to Page: Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlock( "7A3CF259-1090-403C-83B7-2DB3A53DEE26", "", "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "Attendance Reporting", "Main", "", "", 0, "3EF007F1-6B46-4BCD-A435-345C03EBCA17" );

            // Attrib for BlockType: Attendance Reporting:Chart Style
            RockMigrationHelper.AddBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", "", 0, @"", "AD789FB4-2DBF-43E8-9B3F-F85AFCB06979" );

            // Attrib for BlockType: Attendance Reporting:Group Type Template
            RockMigrationHelper.AddBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type Template", "GroupTypeTemplate", "", "", 0, @"", "6CD6EDD9-5DDD-4EAA-9447-A7B61091754D" );

            // Attrib for BlockType: Attendance Reporting:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Select the page to navigate to when the chart is clicked", 0, @"", "D7F38538-8FB1-40C3-9073-A259F197BA3F" );

            // Attrib Value for Block:Attendance Reporting, Attribute:Chart Style Page: Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3EF007F1-6B46-4BCD-A435-345C03EBCA17", "AD789FB4-2DBF-43E8-9B3F-F85AFCB06979", @"b45da8e1-b9a6-46fd-9a2b-e8440d7d6aac" );

            // Attrib Value for Block:Attendance Reporting, Attribute:Detail Page Page: Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3EF007F1-6B46-4BCD-A435-345C03EBCA17", "D7F38538-8FB1-40C3-9073-A259F197BA3F", @"" );

            // Attrib Value for Block:Attendance Reporting, Attribute:Group Type Template Page: Attendance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3EF007F1-6B46-4BCD-A435-345C03EBCA17", "6CD6EDD9-5DDD-4EAA-9447-A7B61091754D", @"fedd389a-616f-4a53-906c-63d8255631c5" );

            /* Metrics Related... */
            RockMigrationHelper.UpdateBlockType( "Line Chart DashboardWidget", "Line Chart dashboard widget for developers to use to start a new LineChartDashboardWidget block.", "~/Blocks/Reporting/Dashboard/LineChartDashboardWidget.ascx", "Dashboard", "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1" );
            RockMigrationHelper.UpdateBlockType( "Bar Chart", "DashboardWidget using flotcharts", "~/Blocks/Reporting/Dashboard/BarChartDashboardWidget.ascx", "Dashboard", "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD" );
            RockMigrationHelper.UpdateBlockType( "Pie Chart", "DashboardWidget using flotcharts", "~/Blocks/Reporting/Dashboard/PieChartDashboardWidget.ascx", "Dashboard", "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D" );
            RockMigrationHelper.UpdateBlockType( "Liquid Dashboard Widget", "DashboardWidget from Liquid", "~/Blocks/Reporting/Dashboard/LiquidDashboardWidget.ascx", "Dashboard", "AC19A4F3-2E88-487E-8E88-377C1C20DBD5" );
            RockMigrationHelper.UpdateBlockType( "Campus Context Setter", "Block that can be used to set the campus context for the current page.", "~/Blocks/Core/CampusContextSetter.ascx", "Core", "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85" );

            // Attrib for BlockType: Line Chart DashboardWidget:Title
            RockMigrationHelper.AddBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "The title of the widget", 0, @"", "88FF63E2-827B-4E81-B090-BD9B4D3CBD2E" );

            // Attrib for BlockType: Line Chart DashboardWidget:Subtitle
            RockMigrationHelper.AddBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "", "The subtitle of the widget", 1, @"", "E60082CC-E014-45D5-9C51-515302EF4F62" );

            // Attrib for BlockType: Line Chart DashboardWidget:Column Width
            RockMigrationHelper.AddBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Column Width", "ColumnWidth", "", "The width of the widget.", 2, @"4", "E2BE16B9-DEF7-4910-B24E-601F8D4D6C65" );

            // Attrib for BlockType: Line Chart DashboardWidget:Metric Value Types
            RockMigrationHelper.AddBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Metric Value Types", "MetricValueTypes", "", "Select which metric value types to display in the chart", 4, @"Measure", "FD81255C-F9B5-40F8-A6D0-DBD766100491" );

            // Attrib for BlockType: Line Chart DashboardWidget:Chart Style
            RockMigrationHelper.AddBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", "", 3, @"", "03F83BCD-EE3D-4A80-9B3D-78CD5665A0AD" );

            // Attrib for BlockType: Line Chart DashboardWidget:Metric
            RockMigrationHelper.AddBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "3A7FB32E-1CCD-4F79-B085-BDBADEB56CCF", "Metric", "Metric", "", "Select the metric and the filter", 5, @"", "68E1C377-D487-4B56-AB00-7B50D5734A5C" );

            // Attrib for BlockType: Line Chart DashboardWidget:Date Range
            RockMigrationHelper.AddBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "SlidingDateRange", "", "", 6, @"1||4||", "B0B67695-090D-4644-B8EC-75215028EACB" );

            // Attrib for BlockType: Line Chart DashboardWidget:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Select the page to navigate to when the chart is clicked", 7, @"", "CC59F4BD-E7E0-4F99-861E-E51021941352" );

            // Attrib for BlockType: Bar Chart:Title
            RockMigrationHelper.AddBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "The title of the widget", 0, @"", "6A38DFA2-4C77-4BA1-9331-8BF805FE4483" );

            // Attrib for BlockType: Bar Chart:Subtitle
            RockMigrationHelper.AddBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "", "The subtitle of the widget", 1, @"", "848373AA-60DA-4CEC-9FB8-F4ED46FE1E2F" );

            // Attrib for BlockType: Bar Chart:Column Width
            RockMigrationHelper.AddBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Column Width", "ColumnWidth", "", "The width of the widget.", 2, @"4", "74E65479-7EE3-495A-A817-0A0413CDAFC0" );

            // Attrib for BlockType: Bar Chart:Metric Value Types
            RockMigrationHelper.AddBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Metric Value Types", "MetricValueTypes", "", "Select which metric value types to display in the chart", 4, @"Measure", "816620DE-6724-4D6A-B948-6144032F49E6" );

            // Attrib for BlockType: Bar Chart:Metric
            RockMigrationHelper.AddBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "3A7FB32E-1CCD-4F79-B085-BDBADEB56CCF", "Metric", "Metric", "", "Select the metric and the filter", 5, @"", "BD319138-D634-4B95-8115-598E6168AC3A" );

            // Attrib for BlockType: Bar Chart:Date Range
            RockMigrationHelper.AddBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "SlidingDateRange", "", "", 6, @"1||4||", "20E9AFA9-699C-439D-94BB-C9A77C0DA4E1" );

            // Attrib for BlockType: Bar Chart:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Select the page to navigate to when the chart is clicked", 7, @"", "65E1CDAB-3F0D-4E33-A240-B481E38A392C" );

            // Attrib for BlockType: Bar Chart:Chart Style
            RockMigrationHelper.AddBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", "", 3, @"", "E95C9843-84DF-4898-BE9B-E66012598638" );

            // Attrib for BlockType: Pie Chart:Chart Style
            RockMigrationHelper.AddBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", "", 3, @"", "5F0EEA60-34ED-463F-AABA-FB7EDB572726" );

            // Attrib for BlockType: Pie Chart:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Select the page to navigate to when the chart is clicked", 7, @"", "720BB502-273C-47D3-AABD-B5D245E8C534" );

            // Attrib for BlockType: Pie Chart:Metric Value Type
            RockMigrationHelper.AddBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Metric Value Type", "MetricValueType", "", "Select which metric value type to display in the chart", 5, @"Measure", "2457975C-1587-46A1-B7BA-61AB55B70046" );

            // Attrib for BlockType: Pie Chart:Date Range
            RockMigrationHelper.AddBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "SlidingDateRange", "", "", 6, @"1||4||", "A16889F1-6695-4B2A-9775-34637E095C04" );

            // Attrib for BlockType: Pie Chart:Metrics
            RockMigrationHelper.AddBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "F5334A8E-B7E2-415C-A6EC-A6D8FA5341C4", "Metrics", "MetricCategories", "", "Select the metrics to include in the pie chart.  Each Metric will be a section of the pie.", 4, @"", "B4CEDE63-BFA7-4AC3-B962-309C70011191" );

            // Attrib for BlockType: Pie Chart:Column Width
            RockMigrationHelper.AddBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Column Width", "ColumnWidth", "", "The width of the widget.", 2, @"4", "70718E08-5A47-4F67-9AE7-72A9708EDE77" );

            // Attrib for BlockType: Pie Chart:Title
            RockMigrationHelper.AddBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "The title of the widget", 0, @"", "68084C17-0C7D-4490-8D09-5E723583AFF8" );

            // Attrib for BlockType: Pie Chart:Subtitle
            RockMigrationHelper.AddBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "", "The subtitle of the widget", 1, @"", "68B88202-0D9D-4DE3-A2F5-6037515D0AB0" );

            // Attrib for BlockType: Liquid Dashboard Widget:Subtitle
            RockMigrationHelper.AddBlockTypeAttribute( "AC19A4F3-2E88-487E-8E88-377C1C20DBD5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "", "The subtitle of the widget", 1, @"", "CCA18FC4-3B51-4217-8BD2-3D4DB07FBE48" );

            // Attrib for BlockType: Liquid Dashboard Widget:Title
            RockMigrationHelper.AddBlockTypeAttribute( "AC19A4F3-2E88-487E-8E88-377C1C20DBD5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "The title of the widget", 0, @"", "DAED2B67-36C9-46C5-84AA-9B1CE1EBEDFE" );

            // Attrib for BlockType: Liquid Dashboard Widget:Column Width
            RockMigrationHelper.AddBlockTypeAttribute( "AC19A4F3-2E88-487E-8E88-377C1C20DBD5", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Column Width", "ColumnWidth", "", "The width of the widget.", 2, @"4", "9F0DC8B3-5265-4E44-9521-6B124D34F7C4" );

            // Attrib for BlockType: Liquid Dashboard Widget:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "AC19A4F3-2E88-487E-8E88-377C1C20DBD5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Outputs the object graph to help create your liquid syntax.", 7, @"False", "3423FCC5-B144-4307-8DCB-42AD9FD33EDE" );

            // Attrib for BlockType: Liquid Dashboard Widget:Round Values
            RockMigrationHelper.AddBlockTypeAttribute( "AC19A4F3-2E88-487E-8E88-377C1C20DBD5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Round Values", "RoundValues", "", "Round Y values to the nearest whole number. For example, display 25.00 as 25.", 5, @"True", "14B67F69-066D-471B-B319-774D73263F6C" );

            // Attrib for BlockType: Liquid Dashboard Widget:Entity
            RockMigrationHelper.AddBlockTypeAttribute( "AC19A4F3-2E88-487E-8E88-377C1C20DBD5", "B50968BD-7643-4288-9237-6E89D2065363", "Entity", "Entity", "", "Select the Entity (Campus, Group, etc) to be used to limit the metric values for the selected metrics", 3, @"", "33D81829-1D91-48E1-A438-47B0926F0C79" );

            // Attrib for BlockType: Liquid Dashboard Widget:Metric
            RockMigrationHelper.AddBlockTypeAttribute( "AC19A4F3-2E88-487E-8E88-377C1C20DBD5", "F5334A8E-B7E2-415C-A6EC-A6D8FA5341C4", "Metric", "MetricCategories", "", "Select the metric(s) to be made available to liquid", 4, @"", "ED861C7D-24AD-4BE3-B2EE-C14178D38C6E" );

            // Attrib for BlockType: Liquid Dashboard Widget:Display Text
            RockMigrationHelper.AddBlockTypeAttribute( "AC19A4F3-2E88-487E-8E88-377C1C20DBD5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Display Text", "DisplayText", "", "The text (or html) to display as a dashboard widget", 6, @"
{% for metric in Metrics %}
    <h1>{{ metric.Title }}</h1>
    <h4>{{ metric.Subtitle }}</h4>
    <p>{{ metric.Description }}</p>
    <div class='row'>    
        <div class='col-md-6'>
            {{ metric.LastValueDate | Date: 'MMM' }}
              <span style='font-size:40px'>{{ metric.LastValue }}</span>
            <p>YTD {{ metric.CumulativeValue }} GOAL {{ metric.GoalValue }}</p>
        </div>
        <div class='col-md-6'>
            <i class='{{ metric.IconCssClass }} fa-5x'></i>
        </div>
    </div>
{% endfor %}
", "EE736CAE-5BAA-4FA4-B190-70F4F7DE92AB" );

            // Attrib Value for Block:Line Chart DashboardWidget, Attribute:Title Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4274AB1E-D8CB-4F57-903C-BB18F316956E", "88FF63E2-827B-4E81-B090-BD9B4D3CBD2E", @"Adult Attendance" );

            // Attrib Value for Block:Line Chart DashboardWidget, Attribute:Subtitle Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4274AB1E-D8CB-4F57-903C-BB18F316956E", "E60082CC-E014-45D5-9C51-515302EF4F62", @"" );

            // Attrib Value for Block:Line Chart DashboardWidget, Attribute:Column Width Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4274AB1E-D8CB-4F57-903C-BB18F316956E", "E2BE16B9-DEF7-4910-B24E-601F8D4D6C65", @"4" );

            // Attrib Value for Block:Line Chart DashboardWidget, Attribute:Chart Style Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4274AB1E-D8CB-4F57-903C-BB18F316956E", "03F83BCD-EE3D-4A80-9B3D-78CD5665A0AD", @"2abb2ea0-b551-476c-8f6b-478cd08c2227" );

            // Attrib Value for Block:Line Chart DashboardWidget, Attribute:Metric Value Types Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4274AB1E-D8CB-4F57-903C-BB18F316956E", "FD81255C-F9B5-40F8-A6D0-DBD766100491", @"Goal,Measure" );

            // Attrib Value for Block:Line Chart DashboardWidget, Attribute:Metric Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4274AB1E-D8CB-4F57-903C-BB18F316956E", "68E1C377-D487-4B56-AB00-7B50D5734A5C", @"d4752628-dfc9-4681-adb3-01936b8f38ca||False|False" );

            // Attrib Value for Block:Line Chart DashboardWidget, Attribute:Date Range Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4274AB1E-D8CB-4F57-903C-BB18F316956E", "B0B67695-090D-4644-B8EC-75215028EACB", @"Current||Year||" );

            // Attrib Value for Block:Line Chart DashboardWidget, Attribute:Detail Page Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4274AB1E-D8CB-4F57-903C-BB18F316956E", "CC59F4BD-E7E0-4F99-861E-E51021941352", @"cdf2c599-d341-42fd-b7dc-cd402ea96050" );

            // Attrib Value for Block:Pie Chart, Attribute:Title Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CAF651C-322A-4F5E-9A87-D560A56C858C", "68084C17-0C7D-4490-8D09-5E723583AFF8", @"Youth Attendance" );

            // Attrib Value for Block:Pie Chart, Attribute:Subtitle Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CAF651C-322A-4F5E-9A87-D560A56C858C", "68B88202-0D9D-4DE3-A2F5-6037515D0AB0", @"" );

            // Attrib Value for Block:Pie Chart, Attribute:Column Width Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CAF651C-322A-4F5E-9A87-D560A56C858C", "70718E08-5A47-4F67-9AE7-72A9708EDE77", @"4" );

            // Attrib Value for Block:Pie Chart, Attribute:Chart Style Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CAF651C-322A-4F5E-9A87-D560A56C858C", "5F0EEA60-34ED-463F-AABA-FB7EDB572726", @"2abb2ea0-b551-476c-8f6b-478cd08c2227" );

            // Attrib Value for Block:Pie Chart, Attribute:Metrics Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CAF651C-322A-4F5E-9A87-D560A56C858C", "B4CEDE63-BFA7-4AC3-B962-309C70011191", @"3b339e3c-d0b0-487f-9922-326d30a18fad,0a6f5c58-b3f8-4ecb-a461-6503d149d880,36ac92b2-4914-4076-9069-4ccecebb378b,43c74bbc-a213-49ca-ab54-8c6912639188" );

            // Attrib Value for Block:Pie Chart, Attribute:Date Range Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CAF651C-322A-4F5E-9A87-D560A56C858C", "A16889F1-6695-4B2A-9775-34637E095C04", @"Last|1|Week||" );

            // Attrib Value for Block:Pie Chart, Attribute:Detail Page Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CAF651C-322A-4F5E-9A87-D560A56C858C", "720BB502-273C-47D3-AABD-B5D245E8C534", @"9f6918de-23a4-47f1-8b4d-fe7c656f284c" );

            // Attrib Value for Block:Pie Chart, Attribute:Metric Value Type Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CAF651C-322A-4F5E-9A87-D560A56C858C", "2457975C-1587-46A1-B7BA-61AB55B70046", @"Measure" );



            /* Other... */
            RockMigrationHelper.UpdateBlockType( "Groups Context Setter", "Block that can be used to set the default groups context for the site.", "~/Blocks/Core/GroupContextSetter.ascx", "Core", "62F749F7-67DF-4A84-B7DD-84CA8E10E205" );

            RockMigrationHelper.UpdateBlockType( "Business Detail", "Displays the details of the given business.", "~/Blocks/Finance/BusinessDetail.ascx", "Finance", "3CB1F9F0-11B2-4A46-B9D1-464811E5015C" );
            RockMigrationHelper.UpdateBlockType( "Business List", "Lists all businesses and provides filtering by business name and owner", "~/Blocks/Finance/BusinessList.ascx", "Finance", "1ACCF349-73A5-4568-B801-2A6A620791D9" );
            
            RockMigrationHelper.UpdateBlockType( "Rest Key Detail", "Displays the details of the given REST API Key.", "~/Blocks/Security/RestKeyDetail.ascx", "Security", "A2C41730-BF79-4F8C-8368-2C4D5F76129D" );
            RockMigrationHelper.UpdateBlockType( "Rest Key List", "Lists all the REST API Keys", "~/Blocks/Security/RestKeyList.ascx", "Security", "C4FBF612-C1F6-428B-97FD-8AB0B8EA31FC" );

            // Attrib for BlockType: Group List:Display Active Status Column
            RockMigrationHelper.AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Active Status Column", "DisplayActiveStatusColumn", "", "Should the Active Status column be displayed?", 6, @"False", "FCB5F8B3-9C0E-46A8-974A-15353447FCD7" );

            // Attrib for BlockType: Prayer Request Detail:Expires After (Days)
            RockMigrationHelper.AddBlockTypeAttribute( "F791046A-333F-4B2A-9815-73B60326162D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpireDays", "", "Number of days until the request will expire (only applies when auto-approved is enabled).", 0, @"14", "2E707E72-E379-4B14-9F14-BED65CEDB657" );

            // Attrib for BlockType: Prayer Request Detail:Default Category
            RockMigrationHelper.AddBlockTypeAttribute( "F791046A-333F-4B2A-9815-73B60326162D", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "", "If a category is not selected, choose a default category to use for all new prayer requests.", 1, @"4B2D88F5-6E45-4B4B-8776-11118C8E8269", "6096F8EB-7E30-4CF8-B5BF-9BE2A33E6E72" );

            // Attrib for BlockType: Device Detail:Map Style
            RockMigrationHelper.AddBlockTypeAttribute( "8CD3C212-B9EE-4258-904C-91BA3570EE11", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "The map theme that should be used for styling the GeoPicker map.", 0, @"FDC5D6BA-A818-4A06-96B1-9EF31B4087AC", "EF6E64B3-5D6F-4D1F-B49F-21E23D0213FD" );

            // Attrib for BlockType: Exception List:Chart Style
            RockMigrationHelper.AddBlockTypeAttribute( "6302B319-9830-4BE3-A402-17801C88F7E4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", "", 0, @"", "A53B228E-6D2E-48DA-8C9F-A85B10A8F7D1" );

            // Attrib for BlockType: Rock Control Gallery:Map Style
            RockMigrationHelper.AddBlockTypeAttribute( "55468258-18B9-4FAE-90E8-F173F7704E23", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "The map theme that should be used for styling the GeoPicker map.", 0, @"FDC5D6BA-A818-4A06-96B1-9EF31B4087AC", "33368551-CE9F-4174-AB19-0F7D089398E5" );

            // Attrib for BlockType: Groups Context Setter:Group Filter
            RockMigrationHelper.AddBlockTypeAttribute( "62F749F7-67DF-4A84-B7DD-84CA8E10E205", "CC34CE2C-0B0E-4BB3-9549-454B2A7DF218", "Group Filter", "GroupFilter", "", "Select group type and root group filter groups by root group. Leave root group blank to filter by group type.", 0, @"", "F2A0D8B1-304D-4C5F-9708-CD0A562F9DD5" );

            // Attrib Value for Block:Exception List, Attribute:Chart Style Page: Exception List, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "557E75A4-1841-4CBE-B976-F36DF209AA17", "A53B228E-6D2E-48DA-8C9F-A85B10A8F7D1", @"2abb2ea0-b551-476c-8f6b-478cd08c2227" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Attendance Reporting:Group Type Template
            RockMigrationHelper.DeleteAttribute( "6CD6EDD9-5DDD-4EAA-9447-A7B61091754D" );
            // Attrib for BlockType: Attendance Reporting:Chart Style
            RockMigrationHelper.DeleteAttribute( "AD789FB4-2DBF-43E8-9B3F-F85AFCB06979" );
            // Attrib for BlockType: Attendance Reporting:Detail Page
            RockMigrationHelper.DeleteAttribute( "D7F38538-8FB1-40C3-9073-A259F197BA3F" );
            // Attrib for BlockType: Prayer Request Detail:Default Category
            RockMigrationHelper.DeleteAttribute( "6096F8EB-7E30-4CF8-B5BF-9BE2A33E6E72" );
            // Attrib for BlockType: Prayer Request Detail:Expires After (Days)
            RockMigrationHelper.DeleteAttribute( "2E707E72-E379-4B14-9F14-BED65CEDB657" );
            // Attrib for BlockType: Pie Chart:Metric Value Type
            RockMigrationHelper.DeleteAttribute( "2457975C-1587-46A1-B7BA-61AB55B70046" );
            // Attrib for BlockType: Pie Chart:Detail Page
            RockMigrationHelper.DeleteAttribute( "720BB502-273C-47D3-AABD-B5D245E8C534" );
            // Attrib for BlockType: Pie Chart:Date Range
            RockMigrationHelper.DeleteAttribute( "A16889F1-6695-4B2A-9775-34637E095C04" );
            // Attrib for BlockType: Pie Chart:Chart Style
            RockMigrationHelper.DeleteAttribute( "5F0EEA60-34ED-463F-AABA-FB7EDB572726" );
            // Attrib for BlockType: Pie Chart:Metrics
            RockMigrationHelper.DeleteAttribute( "B4CEDE63-BFA7-4AC3-B962-309C70011191" );
            // Attrib for BlockType: Liquid Dashboard Widget:Round Values
            RockMigrationHelper.DeleteAttribute( "14B67F69-066D-471B-B319-774D73263F6C" );
            // Attrib for BlockType: Groups Context Setter:Group Filter
            RockMigrationHelper.DeleteAttribute( "F2A0D8B1-304D-4C5F-9708-CD0A562F9DD5" );
            // Attrib for BlockType: Line Chart DashboardWidget:Date Range
            RockMigrationHelper.DeleteAttribute( "B0B67695-090D-4644-B8EC-75215028EACB" );
            // Attrib for BlockType: Bar Chart:Date Range
            RockMigrationHelper.DeleteAttribute( "20E9AFA9-699C-439D-94BB-C9A77C0DA4E1" );
            // Attrib for BlockType: Liquid Dashboard Widget:Entity
            RockMigrationHelper.DeleteAttribute( "33D81829-1D91-48E1-A438-47B0926F0C79" );
            // Attrib for BlockType: Liquid Dashboard Widget:Metric
            RockMigrationHelper.DeleteAttribute( "ED861C7D-24AD-4BE3-B2EE-C14178D38C6E" );
            // Attrib for BlockType: Liquid Dashboard Widget:Title
            RockMigrationHelper.DeleteAttribute( "DAED2B67-36C9-46C5-84AA-9B1CE1EBEDFE" );
            // Attrib for BlockType: Liquid Dashboard Widget:Subtitle
            RockMigrationHelper.DeleteAttribute( "CCA18FC4-3B51-4217-8BD2-3D4DB07FBE48" );
            // Attrib for BlockType: Liquid Dashboard Widget:Column Width
            RockMigrationHelper.DeleteAttribute( "9F0DC8B3-5265-4E44-9521-6B124D34F7C4" );
            // Attrib for BlockType: Liquid Dashboard Widget:Enable Debug
            RockMigrationHelper.DeleteAttribute( "3423FCC5-B144-4307-8DCB-42AD9FD33EDE" );
            // Attrib for BlockType: Liquid Dashboard Widget:Display Text
            RockMigrationHelper.DeleteAttribute( "EE736CAE-5BAA-4FA4-B190-70F4F7DE92AB" );
            // Attrib for BlockType: Exception List:Chart Style
            RockMigrationHelper.DeleteAttribute( "A53B228E-6D2E-48DA-8C9F-A85B10A8F7D1" );
            // Attrib for BlockType: Pie Chart:Column Width
            RockMigrationHelper.DeleteAttribute( "70718E08-5A47-4F67-9AE7-72A9708EDE77" );
            // Attrib for BlockType: Pie Chart:Subtitle
            RockMigrationHelper.DeleteAttribute( "68B88202-0D9D-4DE3-A2F5-6037515D0AB0" );
            // Attrib for BlockType: Pie Chart:Title
            RockMigrationHelper.DeleteAttribute( "68084C17-0C7D-4490-8D09-5E723583AFF8" );
            // Attrib for BlockType: Line Chart DashboardWidget:Chart Style
            RockMigrationHelper.DeleteAttribute( "03F83BCD-EE3D-4A80-9B3D-78CD5665A0AD" );
            // Attrib for BlockType: Bar Chart:Chart Style
            RockMigrationHelper.DeleteAttribute( "E95C9843-84DF-4898-BE9B-E66012598638" );
            // Attrib for BlockType: Bar Chart:Metric
            RockMigrationHelper.DeleteAttribute( "BD319138-D634-4B95-8115-598E6168AC3A" );
            // Attrib for BlockType: Bar Chart:Metric Value Types
            RockMigrationHelper.DeleteAttribute( "816620DE-6724-4D6A-B948-6144032F49E6" );
            // Attrib for BlockType: Bar Chart:Column Width
            RockMigrationHelper.DeleteAttribute( "74E65479-7EE3-495A-A817-0A0413CDAFC0" );
            // Attrib for BlockType: Bar Chart:Subtitle
            RockMigrationHelper.DeleteAttribute( "848373AA-60DA-4CEC-9FB8-F4ED46FE1E2F" );
            // Attrib for BlockType: Bar Chart:Title
            RockMigrationHelper.DeleteAttribute( "6A38DFA2-4C77-4BA1-9331-8BF805FE4483" );
            // Attrib for BlockType: Bar Chart:Detail Page
            RockMigrationHelper.DeleteAttribute( "65E1CDAB-3F0D-4E33-A240-B481E38A392C" );
            // Attrib for BlockType: Device Detail:Map Style
            RockMigrationHelper.DeleteAttribute( "EF6E64B3-5D6F-4D1F-B49F-21E23D0213FD" );
            // Attrib for BlockType: Line Chart DashboardWidget:Metric
            RockMigrationHelper.DeleteAttribute( "68E1C377-D487-4B56-AB00-7B50D5734A5C" );
            // Attrib for BlockType: Line Chart DashboardWidget:Detail Page
            RockMigrationHelper.DeleteAttribute( "CC59F4BD-E7E0-4F99-861E-E51021941352" );
            // Attrib for BlockType: Line Chart DashboardWidget:Metric Value Types
            RockMigrationHelper.DeleteAttribute( "FD81255C-F9B5-40F8-A6D0-DBD766100491" );
            // Attrib for BlockType: Line Chart DashboardWidget:Subtitle
            RockMigrationHelper.DeleteAttribute( "E60082CC-E014-45D5-9C51-515302EF4F62" );
            // Attrib for BlockType: Line Chart DashboardWidget:Column Width
            RockMigrationHelper.DeleteAttribute( "E2BE16B9-DEF7-4910-B24E-601F8D4D6C65" );
            // Attrib for BlockType: Line Chart DashboardWidget:Title
            RockMigrationHelper.DeleteAttribute( "88FF63E2-827B-4E81-B090-BD9B4D3CBD2E" );
            // Attrib for BlockType: Group List:Display Active Status Column
            RockMigrationHelper.DeleteAttribute( "FCB5F8B3-9C0E-46A8-974A-15353447FCD7" );
            // Attrib for BlockType: Rock Control Gallery:Map Style
            RockMigrationHelper.DeleteAttribute( "33368551-CE9F-4174-AB19-0F7D089398E5" );
            // Remove Block: Attendance Reporting, from Page: Attendance, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3EF007F1-6B46-4BCD-A435-345C03EBCA17" );
            // Remove Block: Pie Chart, from Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4CAF651C-322A-4F5E-9A87-D560A56C858C" );
            // Remove Block: Line Chart DashboardWidget, from Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4274AB1E-D8CB-4F57-903C-BB18F316956E" );
            
            RockMigrationHelper.DeleteBlockType( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D" ); // Attendance Reporting
            RockMigrationHelper.DeleteBlockType( "62F749F7-67DF-4A84-B7DD-84CA8E10E205" ); // Groups Context Setter
            RockMigrationHelper.DeleteBlockType( "C4FBF612-C1F6-428B-97FD-8AB0B8EA31FC" ); // Rest Key List
            RockMigrationHelper.DeleteBlockType( "A2C41730-BF79-4F8C-8368-2C4D5F76129D" ); // Rest Key Detail
            RockMigrationHelper.DeleteBlockType( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85" ); // Campus Context Setter
            RockMigrationHelper.DeleteBlockType( "AC19A4F3-2E88-487E-8E88-377C1C20DBD5" ); // Liquid Dashboard Widget
            RockMigrationHelper.DeleteBlockType( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D" ); // Pie Chart
            RockMigrationHelper.DeleteBlockType( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD" ); // Bar Chart
            RockMigrationHelper.DeleteBlockType( "1ACCF349-73A5-4568-B801-2A6A620791D9" ); // Business List
            RockMigrationHelper.DeleteBlockType( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C" ); // Business Detail
            RockMigrationHelper.DeleteBlockType( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1" ); // Line Chart DashboardWidget
            RockMigrationHelper.DeletePage( "7A3CF259-1090-403C-83B7-2DB3A53DEE26" ); // Page: AttendanceLayout: Full Width, Site: Rock RMS
        }
    }
}
