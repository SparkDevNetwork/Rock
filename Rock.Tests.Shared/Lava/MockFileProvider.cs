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
using System.Text;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Rock.Lava;

namespace Rock.Tests.Shared.Lava
{
    public class MockFileProvider : IFileProvider, ILavaFileSystem
    {
        private Dictionary<string, MockFileInfo> _files = new Dictionary<string, MockFileInfo>();

        public MockFileProvider()
        {
        }

        #region IFileProvider implementation

        public IDirectoryContents GetDirectoryContents( string subpath )
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFileInfo( string path )
        {
            if ( _files.ContainsKey( path ) )
            {
                return _files[path];
            }
            else
            {
                return null;
            }
        }

        public MockFileProvider Add( string path, string content )
        {
            _files[path] = new MockFileInfo( path, content );
            return this;
        }

        public IChangeToken Watch( string filter )
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ILavaFileSystem implementation

        public string ReadTemplateFile( ILavaRenderContext context, string templateName )
        {
            var fi = GetFileInfo( templateName );

            if ( fi == null )
            {
                throw new Exception( $"File Load Failed. The file \"{templateName}\" could not be found." );
            }

            var sb = new StringBuilder();

            using ( var fs = fi.CreateReadStream() )
            {
                byte[] byteArray = new byte[1024];
                var fileContent = new UTF8Encoding( true );

                while ( fs.Read( byteArray, 0, byteArray.Length ) > 0 )
                {
                    sb.Append( fileContent.GetString( byteArray ) );
                }
            }

            return sb.ToString().Trim( '\x0' );
        }

        public bool FileExists( string filePath )
        {
            return _files.ContainsKey( filePath );
        }

        IChangeToken IFileProvider.Watch( string filter )
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
