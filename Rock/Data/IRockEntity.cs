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
    /// <para>Apply this to your plugin entities so that they can be loaded using RockContext</para>
    /// <para>Example:</para>
    /// <para>    var rockContext = new RockContext();</para>
    /// <para>    var potlockQry = new Service&lt;Potlock&gt;( rockContext ).Queryable();</para>
    /// <para> NOTE: This should <b>not</b> be used in Rock core, only plugins.</para>
    /// </summary>
    public interface IRockEntity : IEntity
    {
        // intentionally empty
    }
}
