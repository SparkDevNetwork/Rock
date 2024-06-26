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
using System.Data.Entity;
using System.Runtime.Serialization;

using Rock.Lava;
using Rock.Observability;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PersistedDataset
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return PersistedDatasetCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Data.DbContext dbContext )
        {
            PersistedDatasetCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion ICacheable

        #region methods

        /// <summary>
        /// Runs the <see cref="BuildScript" /> and sets <see cref="ResultData"/> if it is valid.
        /// </summary>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">Is thrown if the resulting data deserialized.</exception>
        /// <exception cref="Rock.Model.PersistedDataset.UnsupportedBuildScriptTypeException">Is thrown if the BuildScriptType is not known/supported.</exception>
        public void UpdateResultData()
        {
            using ( var activity = ObservabilityHelper.StartActivity( $"PERSISTED DATASET: Update Result Data (Id: {Id})" ) )
            {
                ObservabilityHelper.EnableDbQueryCountTracking( activity );

                activity?.AddTag( "rock.persisted_dataset.id", Id );
                activity?.AddTag( "rock.persisted_dataset.name", Name );
                activity?.AddTag( "rock.persisted_dataset.type", BuildScriptType.ToString() );

                var timeToBuildStopwatch = System.Diagnostics.Stopwatch.StartNew();
                switch ( this.BuildScriptType )
                {
                    case PersistedDatasetScriptType.Lava:
                        {
                            var mergeFields = LavaHelper.GetCommonMergeFields( null, null, CommonMergeFieldsOptions.CommonMergeFieldsOptionsEmpty );
                            var output = this.BuildScript.ResolveMergeFields( mergeFields, null, this.EnabledLavaCommands );

                            // Ensure resulting output is valid for its defined format,
                            // otherwise log the problem and throw an exception.
                            if ( this.ResultFormat == PersistedDatasetDataFormat.JSON )
                            {
                                var outputAsDynamic = output.FromJsonDynamicOrNull();

                                // If the 
                                if ( outputAsDynamic == null )
                                {
                                    throw new InvalidDataContractException( $"PersistedDataset (Id: {this.Id}) build script created invalid result data: {output}" );
                                }

                                // Save a nicely formatted version of the result
                                this.ResultData = outputAsDynamic.ToJson( true );
                            }
                            break;
                        }

                    default:
                        {
                            throw new UnsupportedBuildScriptTypeException( this.BuildScriptType );
                        }
                }

                timeToBuildStopwatch.Stop();
                this.TimeToBuildMS = timeToBuildStopwatch.Elapsed.TotalMilliseconds;

                this.LastRefreshDateTime = RockDateTime.Now;

                activity?.AddTag( "rock.persisted_dataset.build_duration", ( int ) Math.Floor( timeToBuildStopwatch.Elapsed.TotalMilliseconds ) );
                activity?.AddTag( "rock.persisted_dataset.result_size", ResultData.Length );
            }
        }

        /// <summary>
        /// Exception to throw if the <see cref="PersistedDatasetScriptType"/> is not supported
        /// </summary>
        /// <seealso cref="System.Exception" />
        [Serializable]
        private class UnsupportedBuildScriptTypeException : Exception
        {
            private readonly PersistedDatasetScriptType buildScriptType;

            /// <summary>
            /// Initializes a new instance of the <see cref="UnsupportedBuildScriptTypeException"/> class.
            /// </summary>
            public UnsupportedBuildScriptTypeException()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="UnsupportedBuildScriptTypeException"/> class.
            /// </summary>
            /// <param name="buildScriptType">Type of the build script.</param>
            public UnsupportedBuildScriptTypeException( PersistedDatasetScriptType buildScriptType )
                : base( $"Unsupported PersistedDatasetScriptType: {buildScriptType.ConvertToString()}" )
            {
                this.buildScriptType = buildScriptType;
            }
        }

        #endregion

        #region Helper Classes
        /// <summary>
        /// Result of the <see cref="PersistedDataset.UpdateResultData"/> method.
        /// </summary>
        public class UpdateResult
        {
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="UpdateResult"/> is a success.
            /// </summary>
            public bool IsSuccess { get; set; }
            /// <summary>
            /// Gets or sets the warning message.
            /// </summary>
            public string WarningMessage { get; set; }
        }

        #endregion
    }
}
