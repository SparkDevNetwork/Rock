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
using System.ComponentModel;
using Rock.Attribute;

namespace Rock.Jobs
{
    /// <summary>
    /// This job downloads CBR information for the specified PBX component.
    /// </summary>
    [DisplayName( "PBX CDR Download" )]
    [Description( "This job downloads CBR information for the specified PBX component." )]

    [ComponentField( "Rock.Pbx.PbxContainer, Rock", "PBX Component", "The PBX type to process.", true, key:"PbxComponent" )]
    public class PbxCdrDownload : RockJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public PbxCdrDownload()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the selected provider
            var componentType = GetAttributeValue( "PbxComponent" );
            var provider = Pbx.PbxContainer.GetComponent( componentType );

            if (provider == null )
            {
                this.Result = "Could not find Component Type";
                return;
            }

            var lastProcessedKey = string.Format( "pbx-cdr-download-{0}", provider.TypeName.ToLower().Replace( ' ', '-' ) );
            var lastProcessedDate = Rock.Web.SystemSettings.GetValue( lastProcessedKey ).AsDateTime();

            if ( !lastProcessedDate.HasValue )
            {
                lastProcessedDate = new DateTime(2000, 1, 1); // if first run use 1/1/2000
            }

            bool downloadSuccessful = false;
            this.Result = provider.DownloadCdr( out downloadSuccessful, lastProcessedDate );

            if ( downloadSuccessful )
            {
                Rock.Web.SystemSettings.SetValue( lastProcessedKey, RockDateTime.Now.ToString() );
            }

        }

    }
}
