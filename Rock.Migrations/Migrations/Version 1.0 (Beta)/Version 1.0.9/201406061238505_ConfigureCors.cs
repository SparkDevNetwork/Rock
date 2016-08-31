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
    public partial class ConfigureCors : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType_pre201409101843015( "Global", "REST CORS Domains (Advanced)", @"
Lists the external domains that are authorized to access the REST API through ""cross-origin resource sharing"" (CORS).", "DF7C8DF7-49F9-4858-9E5D-20842AF65AD8", @"
When a browser encounters a script that originated from another domain trying to access the Rock REST API, the browser will query Rock (using CORS) to check 
if that request should be allowed. By default Rock will deny access to the API through this type of cross-site request. To override this behavior, add the domains that
should be allowed to make this type of request to the list of values below. This will enable CORS for just those domains. Note: This only applies to REST calls made 
through scripts downloaded from another domain to a browser. It does not apply to REST calls made directly from another external server or application. That type of
request is allowed by default (but still controlled through security).");

            RockMigrationHelper.AddPage("91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F","D65F783D-87A9-4CC9-8110-E83466A0EADB","REST CORS Domains","","B03A8C4E-E394-44B0-B7CC-89B74C79C325","fa fa-sign-in"); // Site:Rock RMS
            // Add Block to Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.AddBlock("B03A8C4E-E394-44B0-B7CC-89B74C79C325","","0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE","Defined Value List","Main","","",1,"BC6CE880-382B-4DF2-8C10-B7F7C1DEC9FB"); 
            // Add Block to Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.AddBlock("B03A8C4E-E394-44B0-B7CC-89B74C79C325","","08C35F15-9AF7-468F-9D50-CDFD3D21220C","Defined Type Detail","Main","","",0,"EF27C0E7-9D1A-41AB-970B-C854299CE667"); 
            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BC6CE880-382B-4DF2-8C10-B7F7C1DEC9FB","9280D61F-C4F3-4A3E-A9BB-BCD67FF78637",@"df7c8df7-49f9-4858-9e5d-20842af65ad8");
            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("EF27C0E7-9D1A-41AB-970B-C854299CE667","0305EF98-C791-4626-9996-F189B9BB674C",@"df7c8df7-49f9-4858-9e5d-20842af65ad8");


            Sql( @"
/* Move Communications Under People */
UPDATE [Page]
SET [ParentPageId] = (SELECT [Id] FROM [Page] WHERE [Guid] = '97ECDC48-6DF6-492E-8C72-161F76AE111B')
WHERE 
	[Id] = (SELECT [Id] FROM [Page] WHERE [Guid] = '7F79E512-B9DB-4780-9887-AD6D63A39050')

/* Move Prayer Under People */
UPDATE [Page]
SET [ParentPageId] = (SELECT [Id] FROM [Page] WHERE [Guid] = 'B0F4B33D-DD11-4CCC-B79D-9342831B8701')
WHERE 
	[Id] = (SELECT [Id] FROM [Page] WHERE [Guid] = '1A3437C8-D4CB-4329-A366-8D6A4CBF79BF')

UPDATE [DefinedType] SET [HelpText] = 'chart styles are defined with a json object. click the ''show/hide fields'' button below to view an example with all the available chart style fields.
<p>
    <a data-toggle=""collapse""  href=""#collapsefields"" class=''btn btn-action btn-xs''>show/hide  fields</a>
</p>

<div id=""collapsefields"" class=""panel-collapse collapse"">
<pre>
{
  ""seriescolors"": [
    ""#00cc00"",
    ""#007a00"",
    ""#005500"",
    ""#4db84d""
  ],
  ""goalseriescolor"": ""blue"",
  ""grid"": {  
    ""colorgradient"": null,
    ""color"": ""black"",
    ""backgroundcolorgradient"": [""rgba(0, 125, 0, .2)"", ""rgba(0,255,0,.1)"", ""rgba(0,125,0,.01)""],
    ""backgroundcolor"": null,
    ""borderwidth"": {
      ""top"": 0,
      ""right"": 0,
      ""bottom"": 1,
      ""left"": 1
    },
    ""bordercolor"": {
      ""top"": null,
      ""right"": null,
      ""bottom"": ""green"",
      ""left"": ""black""
    }
  },
  ""xaxis"": {
    ""color"": ""#99c2ff"",
    ""font"": {
      ""size"": null,
      ""family"": null,
      ""color"": ""#336600""
    },
    ""datetimeformat"": ""%b %d, %y""
  },
  ""yaxis"": {
    ""color"": ""#99c2ff"",
    ""font"": {
      ""size"": null,
      ""family"": null,
      ""color"": ""#336600""
    },
    ""datetimeformat"": null
  },
  ""fillopacity"": 0.2,
  ""fillcolor"": null,
  ""legend"": {
    ""backgroundcolor"": ""transparent"",
    ""backgroundopacity"": null,
    ""labelboxbordercolor"": null
  },
  ""title"": {
    ""font"": {
      ""size"": 18,
      ""family"": ""terminal"",
      ""color"": ""#296629""
    },
    ""align"": ""left""
  },
  ""subtitle"": {
    ""font"": {
      ""size"": 12,
      ""family"": ""terminal"",
      ""color"": ""#00cc66""
    },
    ""align"": ""right""
  }
}
</pre>

datetimeformat of the xaxis and yaxis supports the following specifiers.  for example ""%b %d, %y"" will output something like ''december 25, 2014''
<pre>
%a: weekday name (customizable)
%b: month name (customizable)
%d: day of month, zero-padded (01-31)
%e: day of month, space-padded ( 1-31)
%h: hours, 24-hour time, zero-padded (00-23)
%i: hours, 12-hour time, zero-padded (01-12)
%m: month, zero-padded (01-12)
%m: minutes, zero-padded (00-59)
%q: quarter (1-4)
%s: seconds, zero-padded (00-59)
%y: year (two digits)
%y: year (four digits)
%p: am/pm
%p: am/pm (uppercase version of %p)
%w: weekday as number (0-6, 0 being sunday)
</pre>
</div>' WHERE [Guid] = 'FC684FD7-FE68-493F-AF38-1656FBF67E6B'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Defined Type Detail, from Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("EF27C0E7-9D1A-41AB-970B-C854299CE667");
            // Remove Block: Defined Value List, from Page: REST CORS Domains, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("BC6CE880-382B-4DF2-8C10-B7F7C1DEC9FB");

            RockMigrationHelper.DeletePage("B03A8C4E-E394-44B0-B7CC-89B74C79C325"); // Page: REST CORS DomainsLayout: Full Width, Site: Rock RMS        
        }
    }
}
