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
using System.Linq;
using System.Runtime.Serialization;

using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;

namespace Rock.BulkExport
{
    /// <summary>
    /// Base class for BulkExport of Entities
    /// </summary>
    public abstract class ModelExport
    {
        private Rock.Data.IModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelExport"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        public ModelExport( Rock.Data.IModel model )
        {
            _model = model;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty(Order = -2)]
        [DataMember]
        public virtual int Id => _model.Id;

        /// <summary>
        /// Gets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [DataMember]
        public virtual DateTime? CreatedDateTime => _model.CreatedDateTime;

        /// <summary>
        /// Gets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        [DataMember]
        public virtual DateTime? ModifiedDateTime => _model.ModifiedDateTime;

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        [DataMember]
        public virtual Guid Guid => _model.Guid;

        /// <summary>
        /// Gets the foreign key.
        /// </summary>
        /// <value>
        /// The foreign key.
        /// </value>
        [DataMember]
        public virtual string ForeignKey => _model.ForeignKey;

        /// <summary>
        /// Gets the foreign identifier.
        /// </summary>
        /// <value>
        /// The foreign identifier.
        /// </value>
        [DataMember]
        public virtual int? ForeignId => _model.ForeignId;

        /// <summary>
        /// Gets the foreign unique identifier.
        /// </summary>
        /// <value>
        /// The foreign unique identifier.
        /// </value>
        [DataMember]
        public virtual Guid? ForeignGuid => _model.ForeignGuid;

        /// <summary>
        /// Gets or sets the attributes export.
        /// </summary>
        /// <value>
        /// The attributes export.
        /// </value>
        [DataMember]
        public virtual AttributesExport AttributesExport { get; set; } = null;

        /// <summary>
        /// Returns a IQueryable of T based on the dataViewId
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Invalid DataView EntityType</exception>
        /// <exception cref="AggregateException"></exception>
        /// <exception cref="System.ArgumentException">Invalid DataViewId: {this.DataViewId}</exception>
        internal static IQueryable<T> QueryFromDataView<T>( RockContext rockContext, int dataViewId ) where T : Rock.Data.IEntity
        {
            IQueryable<T> entityQuery;
            var dataView = new DataViewService( rockContext ).GetNoTracking( dataViewId );
            if ( dataView != null )
            {
                List<string> errorMessages = null;
                entityQuery = dataView.GetQuery( null, rockContext, null, out errorMessages ) as IQueryable<T>;
                if ( entityQuery == null )
                {
                    throw new Exception( "Invalid DataView EntityType" );
                }
                else if ( errorMessages.Any() )
                {
                    throw new AggregateException( errorMessages.Select( a => new Exception( a ) ) );
                }
            }
            else
            {
                throw new ArgumentException( $"Invalid DataViewId: {dataViewId} " );
            }

            return entityQuery;

        }
    }
}
