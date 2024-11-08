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

namespace Rock.Tests.Shared.Lava
{
    public class MockFileInfo : IFileInfo
    {
        public MockFileInfo( string name, string content )
        {
            Name = name;
            Content = content;
        }

        public string Content { get; set; }
        public bool Exists => true;

        public bool IsDirectory => false;

        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        public long Length => -1;

        public string Name { get; }

        public string PhysicalPath => null;

        public Stream CreateReadStream()
        {
            var data = Encoding.UTF8.GetBytes( Content );
            return new MemoryStream( data );
        }
    }
}
