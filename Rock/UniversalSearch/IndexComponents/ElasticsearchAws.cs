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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch.IndexModels;
using Rock.UniversalSearch.IndexModels.Attributes;
using Rock.Web.Cache;

namespace Rock.UniversalSearch.IndexComponents
{
    /// <summary>
    /// Elastic Search Index Provider
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexComponent" />
    [Description( "Amazon Elasticsearch Universal Search Index" )]
    [Export( typeof( IndexComponent ) )]
    [ExportMetadata( "ComponentName", "Elasticsearch AWS" )]

    [TextField( "Node URL", "The URL of the ElasticSearch node (http://myserver:9200)", true, key: "NodeUrl", Order = 10 )]
    [TextField( "Region", "The AWS region to connect to.", true, "us-east-1", Order = 11 )]
    [TextField( "Access Key", "Access key for the account.", true, Order = 12 )]
    [TextField( "Secret Key", "Secret key for the account.", true, Order = 13 )]
    public class ElasticSearchAws : Elasticsearch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Elasticsearch" /> class.
        /// </summary>
        public ElasticSearchAws()
        {
            ConnectToServer();
        }

        /// <summary>
        /// Connects to server.
        /// </summary>
        protected override void ConnectToServer()
        {
            if ( GetAttributeValue( "NodeUrl" ).IsNotNullOrWhitespace() )
            {
                try {
                    var node = new Uri( GetAttributeValue( "NodeUrl" ) );
                    var region = GetAttributeValue( "Region" );
                    var accessKey = GetAttributeValue( "AccessKey" );
                    var secretKey = GetAttributeValue( "SecretKey" );

                    var httpConnection = new AwsHttpConnection( new AwsSettings
                    {
                        AccessKey = accessKey,
                        SecretKey = secretKey,
                        Region = region,
                    } );

                    var pool = new SingleNodeConnectionPool( node );
                    var config = new ConnectionSettings( pool, httpConnection );
                    _client = new ElasticClient( config );
                }
                catch { }
            }
        }
    }
}