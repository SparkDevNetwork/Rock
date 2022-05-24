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
namespace Rock.SystemGuid
{
    /// <summary>
    /// Rock Guid attribute to be used to specify the <see cref="Rock.Model.RestController">RestController.Guid</see> for Rock.Rest controllers.
    /// <para>
    /// Note that the Code Generated files for Rock.Rest.ApiController&lt;Entity&gt; will automatically have the RestControllerGuid applied to it. So, any partials of these code-generated ones don't need the RestControllerGuid added. 
    /// </para>
    /// <para>
    /// Any other controllers that aren't code generated should have the RestControllerGuidAttribute added manually by the developer. However, the CodeGenerator will catch any missings ones and add it.
    /// This includes things that inherit from
    /// <list type="bullet">
    /// <item>Rock.Rest.ApiControllerBase</item>
    /// <item>Rock.Rest.ApiController</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <seealso cref="RockGuidAttribute" />
    [System.AttributeUsage( System.AttributeTargets.Class, Inherited = true, AllowMultiple = false )]
    public class RestControllerGuidAttribute : RockGuidAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestControllerGuidAttribute"/> class.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public RestControllerGuidAttribute( string guid )
            : base( guid )
        {
        }
    }
}