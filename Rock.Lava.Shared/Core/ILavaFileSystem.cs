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
namespace Rock.Lava
{
    /// <summary>
    /// A Lava file system provides a means of accessing external files from a template.
    /// A file system implementation is required for the Liquid "{% include %}" command to load partial templates during the rendering process.
    /// </summary>
    public interface ILavaFileSystem
    {
        /// <summary>
        /// Returns a flag indicating if the specified file exists.
        /// </summary>
        /// <param name="filePath">A relative file path.</param>
        /// <returns></returns>
        bool FileExists( string filePath );

        /// <summary>
        /// Called by the Lava Engine to read the contents of a template file.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        string ReadTemplateFile( ILavaRenderContext context, string templateName );
    }
}