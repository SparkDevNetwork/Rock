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
using System.Configuration;

namespace Rock.Store
{
    /// <summary>
    /// Represents a package in the store.
    /// </summary>
    public class StoreImage : StoreModel
    {
        /// <summary>
        /// Gets or sets the Id of the package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Package.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the Guid of the Package.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the image URL. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the image URL.
        /// </value>
        public string ImageUrl {
            get
            {
                string storeServer = ConfigurationManager.AppSettings["RockStoreUrl"];
                return string.Format("{0}GetImage.ashx?guid={1}", storeServer, this.Guid);
            }
        }
    }
}
