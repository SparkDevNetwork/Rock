// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.IO;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents the data/content of a <see cref="Rock.Model.BinaryFile"/> this entity can either be used to temporary store the 
    /// file content in memory or can be persisted to the database. 
    /// </summary>
    [NotAudited]
    [Table( "BinaryFileData" )]
    [DataContract]
    public partial class BinaryFileData : Model<BinaryFileData>
    {
        #region Entity Properties

        //// ** NOTE:  We need [DataMember] on Content so that REST can GET and POST BinaryFileData. 
        //// ** However, we don't have to worry about Liquid serializing this since BinaryFile.Data is not marked with [DataMember]
        //// ** So the only way you would get serialized Content if you intentionally requested to serialise BinaryFileData
        
        /// <summary>
        /// Gets or sets the data/content of a <see cref="Rock.Model.BinaryFile"/>
        /// Private so that it will be read/write using ContentStream, but needs to be there for EF
        /// </summary>
        /// <value>
        /// A <see cref="System.Byte"/> array that contains the data/content of a <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        [DataMember]
        [HideFromReporting]
        public byte[] Content {
            get
            {
                if ( this.ContentStream != null )
                {
                    var result = new byte[this.ContentStream.Length];
                    this.ContentStream.Seek( 0, SeekOrigin.Begin );
                    this.ContentStream.Read( result, 0, result.Length );
                    return result;
                }
                else
                {
                    return null;
                }
            }

            private set
            {
                this.ContentStream = new MemoryStream( value );
            }
        }

        /// <summary>
        /// Gets or sets the content stream.
        /// </summary>
        /// <value>
        /// The content stream.
        /// </value>
        [HideFromReporting]
        [NotMapped]
        public virtual Stream ContentStream { get; set; }

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
