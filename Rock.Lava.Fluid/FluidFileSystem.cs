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
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Rock.Lava
{
    /// <summary>
    /// A proxy for a LavaFileSystem component that is compatible with the Fluid framework.
    /// </summary>
    public class FluidFileSystem : IFileProvider, ILavaFileSystem
    {
        private ILavaFileSystem _fileSystem = null;

        #region Constructors

        /// <summary>
        /// Create a new proxy for a LavaFileSystem instance.
        /// </summary>
        /// <param name="lavaFileSystem"></param>
        public FluidFileSystem( ILavaFileSystem fileSystem )
        {
            _fileSystem = fileSystem;
        }

        #endregion

        #region ILavaFileSystem implementation

        bool ILavaFileSystem.FileExists( string filePath )
        {
            filePath = GetCorrectedFilePath( filePath );

            var exists = _fileSystem.FileExists( filePath );

            return exists;
        }

        string ILavaFileSystem.ReadTemplateFile( ILavaRenderContext context, string templateName )
        {
            var content = _fileSystem.ReadTemplateFile( context, templateName );

            return content;
        }

        #endregion

        #region IFileProvider implementation

        private static LavaToLiquidTemplateConverter _lavaConverter = new LavaToLiquidTemplateConverter();

        IDirectoryContents IFileProvider.GetDirectoryContents( string subpath )
        {
            // Directory listing is not supported.
            return null;
        }

        IFileInfo IFileProvider.GetFileInfo( string subpath )
        {
            var filePath = GetCorrectedFilePath( subpath );

            // The Fluid framework forces a ".liquid" extension in the file path.
            // If the file is not found using the corrected file path, retry with the original filepath in case the correction was not needed.
            var exists = _fileSystem.FileExists( filePath );

            if ( !exists
                 && subpath.EndsWith( ".liquid" ) )
            {
                filePath = subpath.Substring( 0, subpath.Length - 7 );

                exists = _fileSystem.FileExists( filePath );
            }

            // This method is called directly by the Fluid framework.
            // Therefore, we need to load the Lava template from the file and convert it to Liquid-compatible syntax before returning it to the Fluid engine.
            var lavaText = exists ? _fileSystem.ReadTemplateFile( null, filePath ) : string.Empty;

            var liquidText = _lavaConverter.ConvertToLiquid( lavaText );

            var fileInfo = new LavaFileInfo( filePath, liquidText, exists );

            if ( !exists )
            {
                throw new LavaException( "File Load Failed. File \"{0}\" could not be accessed.", filePath );
            }

            return fileInfo;
        }

        IChangeToken IFileProvider.Watch( string filter )
        {
            // File system monitoring is not supported.
            return null;
        }

        #endregion

        private string GetCorrectedFilePath( string filePath )
        {
            // The Fluid framework forces a ".liquid" extension in the file path.
            // Most Lava template files use a ".lava" file type, so if the file is not found remove the ".liquid" extension and retry.
            if ( !string.IsNullOrEmpty( filePath ) )
            {
                filePath = filePath.Trim();

                if ( filePath.EndsWith( ".lava.liquid" ) )
                {
                    filePath = filePath.Substring( 0, filePath.Length - 7 );
                }
            }

            return filePath;
        }
    }

    #region Support Classes

    /// <summary>
    /// A FileInfo implementation for the FluidFileSystem.
    /// </summary>
    internal class LavaFileInfo : IFileInfo
    {
        public LavaFileInfo( string name, string content, bool exists = true )
        {
            Name = name;
            Content = content;
            Exists = exists;
        }

        public string Content { get; set; }

        public bool Exists { get; }

        public bool IsDirectory
        {
            get
            {
                return false;
            }
        }

        public DateTimeOffset LastModified
        {
            get
            {
                return DateTimeOffset.MinValue;
            }
        }

        public long Length
        {
            get
            {
                return -1;
            }
        }

        public string Name { get; }

        public string PhysicalPath
        {
            get
            {
                return null;
            }
        }

        public Stream CreateReadStream()
        {
            var data = Encoding.UTF8.GetBytes( Content );
            return new MemoryStream( data );
        }
    }

    #endregion
}