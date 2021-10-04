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
namespace Rock.Data
{
    /// <summary>
    /// <para>
    /// <c>WARNING</c>: Experimental. For internal use only. This is intended only for use in environments with
    /// a read only replica of the production DB.
    /// </para>
    /// <para>
    /// This is special RockContext that can be used to connect to a readonly copy of the RockContext database
    /// as specified as a connection named <see cref="RockContextAnalytics"/> in web.connectionstrings.config.
    /// Operations that write to the database are not allowed.
    /// See additional notes in <seealso cref="RockContextReadOnly"/>
    /// </para>
    /// </summary>
    public class RockContextAnalytics : RockContextReadOnly
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockContextAnalytics"/> class.
        /// </summary>
        public RockContextAnalytics()
            : base( new RockContext( GetConnectionString<RockContextAnalytics>() ) )
        {
            //
        }
    }
}
