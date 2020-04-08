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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents the data/content of a <see cref="Rock.Model.BinaryFile"/> this entity can either be used to temporary store the 
    /// file content in memory or can be persisted to the database. 
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "BinaryFileData" )]
    [DataContract]
    public partial class BinaryFileData : Model<BinaryFileData>
    {
        #region Entity Properties

        //// ** NOTE:  We need [DataMember] on Content so that REST can GET and POST BinaryFileData. 
        //// ** However, we don't have to worry about Liquid serializing this since BinaryFile.Data is not marked with [DataMember]
        //// ** So the only way you would get serialized Content if you intentionally requested to serialize BinaryFileData
        
        /// <summary>
        /// Gets or sets the data/content of a <see cref="Rock.Model.BinaryFile"/>
        /// NOTE: Use ContentStream instead of Content whenever possible
        /// </summary>
        /// <value>
        /// A <see cref="System.Byte"/> array that contains the data/content of a <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        [DataMember]
        [HideFromReporting]
        public byte[] Content { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class BinaryFileDataConfiguration : EntityTypeConfiguration<BinaryFileData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryFileDataConfiguration"/> class.
        /// </summary>
        public BinaryFileDataConfiguration()
        {
        }
    }

    #endregion

}
