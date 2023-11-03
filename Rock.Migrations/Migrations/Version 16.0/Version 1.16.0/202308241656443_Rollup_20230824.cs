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
    public partial class Rollup_20230824 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixLavaWebhookDefinedTypeDescriptionUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// DL: Fix Lava Webhook Defined Type Description
        /// </summary>
        private void FixLavaWebhookDefinedTypeDescriptionUp()
        {
            var description = @"The properties of the incoming request are available as merge fields to resolve the Lava in the 'Template' attribute.
<p>
    <a data-toggle=`collapse` href=`#collapsefields` class=`btn btn-action btn-xs`>show/hide fields</a>
</p>

<div id=`collapsefields` class=`panel-collapse collapse`>
<pre>
{
  `Url`: `/test`,
  `RawUrl`: `http://rock.rocksolidchurchdemo.com/Webhooks/Lava.ashx/test?queryParameter=true`,
  `Method`: `POST`,
  `QueryString`: {
    `queryParameter`: `true`
  },
  `RemoteAddress`: `100.101.102.103`,
  `RemoteName`: `100.101.102.103`,
  `ServerName`: `rock.rocksolidchurchdemo.com`,
  `RawBody`: `{`key1`:`value1`, `key2`:`value2`}`,
  `Body`: {
    `key1`: `value1`,
    `key2`: `value2`
  },
  `Headers`: {
    `Content-Length`: `34`,
    `Content-Type`: `application/json`,
    `Accept`: `*/*`,
    `Host`: `rock.rocksolidchurchdemo.com`,
    `User-Agent`: `curl/7.35.0`
  },
  `Cookies`: {
    `sessionToken`: `abc123`
  }
}
</pre>
</div>";

            description = description.Replace( "`", @"""" ).Replace( "'", "''" );
            Sql( $@"UPDATE [DefinedType] SET  [HelpText] = '{description}' WHERE [Guid] = '{Rock.SystemGuid.DefinedType.WEBHOOK_TO_LAVA}'" );
        }
    }
}
