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
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;

// This namespace is an exception due to a conflict
// between the Rock.Model.Group domain and Rock.Model.Group Type.
namespace Rock.Model.Groups.Group.Options
{
    /// <summary>
    /// Options and fields related to a copy group operation.
    /// </summary>
    [RockInternal( "1.17" )]
    public class CopyGroupOptions
    {
        /// <summary>
        /// The group identifier of the Group to use as a template for the copy.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// If true copy child groups in addition to the Group identified by the GroupId.
        /// </summary>
        public bool IncludeChildGroups { get; set; }

        /// <summary>
        /// The <seealso cref="Rock.Model.PersonAlias"/> PersonAliasId of the person performing the copy.
        /// </summary>
        public int? CreatedByPersonAliasId { get; set; }
    }
}
