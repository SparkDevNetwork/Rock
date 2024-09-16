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
using Microsoft.Web.XmlTransform;

namespace Rock.Configuration
{
    /// <summary>
    /// Helper class for working with Xml.
    /// </summary>
    internal class XmlHelper
    {
        /// <summary>
        /// Transforms the XML document found at the specified file path using the supplied transform string.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="transformString">The transform string.</param>
        /// <returns></returns>
        public static bool TransformXmlDocument( string filePath, string transformString )
        {
            bool isSuccess = false;

            using ( var document = new XmlTransformableDocument() )
            {
                document.PreserveWhitespace = true;
                document.Load( filePath );

                using ( var transformation = new XmlTransformation( transformString, false, null ) )
                {
                    isSuccess = transformation.Apply( document );
                    document.Save( filePath );
                }
            }

            return isSuccess;
        }
    }
}
