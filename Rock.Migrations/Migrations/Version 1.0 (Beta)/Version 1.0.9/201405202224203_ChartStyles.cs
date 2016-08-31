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
    public partial class ChartStyles : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType_pre201409101843015( "Global", "Chart Styles", "Defines a listing of various chart styles to be used as configuration for blocks that render charts", "FC684FD7-FE68-493F-AF38-1656FBF67E6B", @"Chart styles are defined with a JSON object. Click the ''Show/Hide Fields'' button below to view an example with all the available chart style fields.

<p>
    <a data-toggle=""collapse""  href=""#collapseFields"" class=''btn btn-action btn-xs''>Show/Hide  Fields</a>
</p>

<div id=""collapseFields"" class=""panel-collapse collapse"">
<pre>
{
  ""SeriesColors"": [
    ""#00CC00"",
    ""#007A00"",
    ""#005500"",
    ""#4DB84D""
  ],
  ""GoalSeriesColor"": ""blue"",
  ""Grid"": {  
    ""ColorGradient"": null,
    ""Color"": ""black"",
    ""BackgroundColorGradient"": [""RGBA(0, 125, 0, .2)"", ""RGBA(0,255,0,.1)"", ""RGBA(0,125,0,.01)""],
    ""BackgroundColor"": null,
    ""BorderWidth"": {
      ""top"": 0,
      ""right"": 0,
      ""bottom"": 1,
      ""left"": 1
    },
    ""BorderColor"": {
      ""top"": null,
      ""right"": null,
      ""bottom"": ""green"",
      ""left"": ""black""
    }
  },
  ""XAxis"": {
    ""Color"": ""#99C2FF"",
    ""Font"": {
      ""Size"": null,
      ""Family"": null,
      ""Color"": ""#336600""
    }
  },
  ""YAxis"": {
    ""Color"": ""#99C2FF"",
    ""Font"": {
      ""Size"": null,
      ""Family"": null,
      ""Color"": ""#336600""
    }
  },
  ""FillOpacity"": 0.2,
  ""FillColor"": null,
  ""Legend"": {
    ""BackgroundColor"": ""transparent"",
    ""BackgroundOpacity"": null,
    ""LabelBoxBorderColor"": null
  },
  ""Title"": {
    ""Font"": {
      ""Size"": 18,
      ""Family"": ""Terminal"",
      ""Color"": ""#296629""
    },
    ""Align"": ""left""
  },
  ""Subtitle"": {
    ""Font"": {
      ""Size"": 12,
      ""Family"": ""Terminal"",
      ""Color"": ""#00CC66""
    },
    ""Align"": ""right""
  }
}
</pre>
</div>" );
            RockMigrationHelper.AddDefinedTypeAttribute( "FC684FD7-FE68-493F-AF38-1656FBF67E6B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Chart Style", "ChartStyle", "", 26, "{}", "173E1A89-A84E-48EC-BFD5-0C8D65A84276" );
            RockMigrationHelper.AddAttributeQualifier( "173E1A89-A84E-48EC-BFD5-0C8D65A84276", "editorHeight", "", "AB54A928-73C8-4172-97A5-1A1C0C7F4439" );
            RockMigrationHelper.AddAttributeQualifier( "173E1A89-A84E-48EC-BFD5-0C8D65A84276", "editorMode", "4", "8DCD60DF-C48C-41FA-A9B0-978855A3FB47" );
            RockMigrationHelper.AddAttributeQualifier( "173E1A89-A84E-48EC-BFD5-0C8D65A84276", "editorTheme", "0", "7C894CCE-5BC6-4A12-A9F4-BBBC27672A23" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "FC684FD7-FE68-493F-AF38-1656FBF67E6B", "Flot", "The standard Flot chart style", "B45DA8E1-B9A6-46FD-9A2B-E8440D7D6AAC", false );
            RockMigrationHelper.AddDefinedValue_pre20140819( "FC684FD7-FE68-493F-AF38-1656FBF67E6B", "Rock", "The default styling for Rock charts.", "2ABB2EA0-B551-476C-8F6B-478CD08C2227", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2ABB2EA0-B551-476C-8F6B-478CD08C2227", "173E1A89-A84E-48EC-BFD5-0C8D65A84276", @"{
  ""SeriesColors"": [
    ""#8498ab"",
    ""#a4b4c4"",
    ""#b9c7d5"",
    ""#c6d2df"",
    ""#d8e1ea""
  ],
  ""GoalSeriesColor"": ""red"",
  ""Grid"": {
    ""ColorGradient"": null,
    ""Color"": null,
    ""BackgroundColorGradient"": null,
    ""BackgroundColor"": ""transparent"",
    ""BorderWidth"": {
      ""top"": 0,
      ""right"": 0,
      ""bottom"": 1,
      ""left"": 1
    },
    ""BorderColor"": null
  },
  ""XAxis"": {
    ""Color"": ""rgba(81, 81, 81, 0.2)"",
    ""Font"": {
      ""Size"": 10,
      ""Family"": null,
      ""Color"": ""#515151""
    },
    ""DateTimeFormat"": ""%b %e,<br />%Y""
  },
  ""YAxis"": {
    ""Color"": ""rgba(81, 81, 81, 0.2)"",
    ""Font"": {
      ""Size"": null,
      ""Family"": null,
      ""Color"": ""#515151""
    },
    ""DateTimeFormat"": null
  },
  ""FillOpacity"": 0.2,
  ""FillColor"": null,
  ""Legend"": {
    ""BackgroundColor"": ""transparent"",
    ""BackgroundOpacity"": null,
    ""LabelBoxBorderColor"": null
  },
  ""Title"": {
    ""Font"": {
      ""Size"": 16,
      ""Family"": null,
      ""Color"": null
    },
    ""Align"": ""left""
  },
  ""Subtitle"": {
    ""Font"": {
      ""Size"": 12,
      ""Family"": null,
      ""Color"": null
    },
    ""Align"": ""left""
  }
}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B45DA8E1-B9A6-46FD-9A2B-E8440D7D6AAC", "173E1A89-A84E-48EC-BFD5-0C8D65A84276", @"{
  ""Title"": {
    ""Font"": {
      ""Size"": 16,
      ""Family"": null,
      ""Color"": null
    },
    ""Align"": ""left""
  },
  ""Subtitle"": {
    ""Font"": {
      ""Size"": 12,
      ""Family"": null,
      ""Color"": null
    },
    ""Align"": ""left""
  }
}" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "173E1A89-A84E-48EC-BFD5-0C8D65A84276" );
            RockMigrationHelper.DeleteDefinedType( "FC684FD7-FE68-493F-AF38-1656FBF67E6B" );
            RockMigrationHelper.DeleteDefinedValue( "2ABB2EA0-B551-476C-8F6B-478CD08C2227" );
            RockMigrationHelper.DeleteDefinedValue( "B45DA8E1-B9A6-46FD-9A2B-E8440D7D6AAC" );
        }
    }
}
