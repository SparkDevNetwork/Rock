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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using Microsoft.Extensions.FileProviders;

using Rock.Lava;

namespace Rock.Web.v2
{
    internal class LavaPageLayoutBuilder
    {
        private readonly ConcurrentDictionary<string, LavaPageLayout> _layoutCache = new ConcurrentDictionary<string, LavaPageLayout>();

        private readonly IFileProvider _fileProvider;

        public LavaPageLayoutBuilder( IFileProvider fileProvider )
        {
            _fileProvider = fileProvider;
        }

        public LavaPageLayout GetLayout( string layoutPath, ILavaEngine lavaEngine )
        {
            return _layoutCache.GetOrAdd( layoutPath, CreateLayout, lavaEngine );
        }

        private LavaPageLayout CreateLayout( string layoutPath, ILavaEngine lavaEngine )
        {
            var parser = new HtmlParser();
            var fileInfo = _fileProvider.GetFileInfo( layoutPath );
            string fileContent;

            using ( var stream = fileInfo.CreateReadStream() )
            {
                fileContent = Encoding.UTF8.GetString( stream.ReadBytesToEnd() );
            }

            // Special tags:
            //
            // <Rock:ParentLayout src="" />
            // Layout specified is loaded and used as a parent template with injection points.
            //
            // <Rock:RenderBody />
            // Renders the body of a child template, no default/inner content allowed.
            //
            // <Rock:Section name="">...</Rock:Section>
            // Defines a section that will made available to a parent template. If the section
            // has already been defined by a child template, it will be replaced, though nesting
            // would be allowed as <Rock:RenderSection> tags are processed in the current template
            // before <Rock:Section> tags are processed.
            //
            // <Rock:RenderSection name="">...</Rock:RenderSection>
            // Renders a named section, with optional default content if the section was not
            // defined.

            // If fileContent has 0 instances of '<Rock:ParentLayout' then parse as full document.
            // If fileContent has 1 instance of '<Rock:ParentLayout' then parse as fragment.
            // If fileContent has multiple instances of `<Rock:ParentLayout' then fail with error.
            var doc = parser.ParseDocument( "<html></html>" );
            var document = parser.ParseFragment( fileContent, doc.Body );
            var doc2 = parser.ParseFragment( "<html><body id=\"abc\"></body></html>", doc.Body );

            var result = lavaEngine.ParseTemplate( fileContent );
            var dependencies = new List<string>
            {
                layoutPath
            };

            //template.GetTemplateDependencies( dependencies );

            return new LavaPageLayout( result.Template, fileContent, dependencies );
        }

        private void ProcessRootLayout( string filePath, string content, RenderContext context )
        {

        }

        private class RenderContext
        {
            public List<string> Dependencies { get; } = new List<string>();

            public Dictionary<string, INodeList> Sections { get; } = new Dictionary<string, INodeList>();
        }
    }

    internal class LavaPageLayout
    {
        public ILavaTemplate Template { get; }

        public string Source { get; }

        public IReadOnlyList<string> Dependencies { get; }

        public LavaPageLayout( ILavaTemplate template, string source, IReadOnlyList<string> dependencies )
        {
            Template = template;
            Source = source;
            Dependencies = dependencies;
        }
    }
}
