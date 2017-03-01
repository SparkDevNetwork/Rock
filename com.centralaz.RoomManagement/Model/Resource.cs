// <copyright>
// Copyright by the Central Christian Church
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using Rock;
using Rock.Data;
using Rock.Model;
using DDay.iCal;
namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// A Resource
    /// </summary>
    [Table( "_com_centralaz_RoomManagement_Resource" )]
    [DataContract]
    public class Resource : Rock.Data.Model<Resource>, Rock.Data.IRockEntity, Rock.Data.ICategorized
    {

        #region Entity Properties

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int? CategoryId { get; set; }

        [DataMember]
        public int? CampusId { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        [DataMember]
        public string Note { get; set; }

        #endregion

        #region Virtual Properties

        public virtual Category Category { get; set; }

        public virtual Campus Campus { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class ResourceConfiguration : EntityTypeConfiguration<Resource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceConfiguration"/> class.
        /// </summary>
        public ResourceConfiguration()
        {
            this.HasRequired( r => r.Category ).WithMany().HasForeignKey( r => r.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Campus ).WithMany().HasForeignKey( r => r.CampusId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
