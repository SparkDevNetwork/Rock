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
using Rock.Data;
using Rock.Model;
using System.Linq;

namespace Rock.Tasks
{
    /// <summary>
    /// Updates <see cref="Rock.Model.PersistedDataset"/> 
    /// </summary>
    public sealed class UpdatePersistedDataset : BusStartedTask<UpdatePersistedDataset.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var persistedDatasetService = new PersistedDatasetService( rockContext );
                var persistedDataset = persistedDatasetService.Queryable().FirstOrDefault( d => d.AccessKey == message.AccessKey );

                if ( persistedDataset == null )
                {
                    return;
                }

                persistedDataset.UpdateResultData();

                /*
                    2/10/2026 - NA
                    We are calling the SaveChanges( true ) overload that disables pre/post processing hooks
                    because we only want to change the properties changed in UpdateResultData(). If we don't disable
                    these hooks, the [ModifiedDateTime] value will also be updated every time a DataView is
                    run, which is not what we want here.

                    Reason: See Asana task "Persisted Datasets Don't Have CreatedBy/ModifiedBy Values"
                    https://app.asana.com/1/20866866924293/task/1213202694111290
                */
                rockContext.SaveChanges( true );
            }
        }

        /// <summary>
        /// Message class for <see cref="UpdatePersistedDataset"/>
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// The AccessKey of the dataset to be updated
            /// </summary>
            public string AccessKey { get; set; }
        }
    }
}
