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
using System.Collections.Generic;
using System.IO;

namespace Rock.Lava
{
    public interface ILavaElement
    {
        string ElementName
        {
            get;
        }

        /// <summary>
        /// The name by which the element is processed internally.
        /// This may be a decorated version of the element name that is known by the user to avoid collisions with other elements.
        /// </summary>
        string InternalName
        {
            get;
        }

        void OnInitialize( string tagName, string markup, List<string> tokens );

        void OnRender( ILavaRenderContext context, TextWriter result );


    }
}
