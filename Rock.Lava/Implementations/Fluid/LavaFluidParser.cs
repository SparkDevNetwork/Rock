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
using System.Linq;
using Fluid;
using Fluid.Ast;
using Fluid.Parser;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// An extended implementation of the Fluid Liquid parser for Lava.
    /// </summary>

    /* [2021-04-29] DJL
     * This implementation extends the Fluid parser to capture the source text of Lava blocks as they are processed,
     * which is necessary to support a framework-independent implementation of custom tags and blocks by
     * working directly with the source text of the template.
     */

    internal class LavaFluidParser : FluidParser
    {
        #region Lava Template Element Parsers

        static readonly Parser<LavaDocumentToken> Output = OutputStart.SkipAnd( AnyCharBefore( OutputEnd, canBeEmpty: true ) )
            .AndSkip( OutputEnd )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Output, x.ToString() ) );

        static readonly Parser<LavaDocumentToken> Tag = TagStart.SkipAnd( AnyCharBefore( TagEnd, canBeEmpty: true ) )
            .AndSkip( TagEnd )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Tag, x.ToString() ) );

        static readonly Parser<LavaDocumentToken> Text = AnyCharBefore( OutputStart.Or( TagStart ) )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Literal, x.ToString() ) );

        static readonly Parser<List<LavaDocumentToken>> TagsList = ZeroOrMany( Output.Or( Tag ).Or( Text ) );

        #endregion

        public Parser<List<FilterArgument>> ArgumentsListParser
        {
            get
            {
                return ArgumentsList;
            }
        }
        protected static readonly Parser<char> Space = Terms.Char( ' ' );

        protected Parser<List<FilterArgument>> LavaArgumentsList;

        public LavaFluidParser()
            : base()
        {
            // Define a parser for a named argument list separated by spaces in the form:
            // [name1:]value1 [name2:]value2 ...
            LavaArgumentsList = OneOf( TagEnd.Then( x => new List<FilterArgument>() ),
                Separated( Space,
                            OneOf(
                                Identifier.AndSkip( Colon ).And( Primary ).Then( x => new FilterArgument( x.Item1, x.Item2 ) ),
                                Primary.Then( x => new FilterArgument( null, x ) )
                            ) ) );
        }

        public void RegisterLavaBlock( string tagName )
        {
            // Create a parser for the Lava block that does the following:
            // 1. Captures any optional attributes that are contained in the open tag.
            // 2. Creates a new Statement that will execute when the block is rendered. The Statement captures the block content
            //    as literal text, so that it can be scanned and tokenized by the Lava library before being passed back to Fluid
            //    for final rendering.
            // 3. Throw an exception if the Block is malformed.
            var endTagName = "end" + tagName;

            var endTag = CreateTag( endTagName );

            var lavaBlock = AnyCharBefore( TagEnd, canBeEmpty: true )
                .AndSkip( TagEnd )
                .And( AnyCharBefore( endTag, canBeEmpty: true ) )
                .AndSkip( endTag.ElseError( $"'{{% {endTagName} %}}' was expected" ) )
                .Then<Statement>( x => new FluidLavaBlockStatement( this, tagName, x.Item1, x.Item2 ) )
                .ElseError( $"Invalid '{{% {tagName} %}}' tag" );

            this.RegisteredTags[tagName] = lavaBlock;
        }

        public void RegisterLavaTag( string tagName )
        {
            // Create a parser for the Lava tag that does the following:
            // 1. Captures any optional attributes that are contained in the open tag.
            // 2. Create a new Statement that will execute when the tag is rendered.
            // 3. Throw an exception if the Tag is malformed.
            var lavaTag = AnyCharBefore( TagEnd, canBeEmpty: true )
                .AndSkip( TagEnd )
                .Then<Statement>( x => new FluidLavaTagStatement( this, tagName, x ) )
                .ElseError( $"Invalid '{{% {tagName} %}}' tag" );

            this.RegisteredTags[tagName] = lavaTag;
        }

        public List<string> ParseToTokens( string template )
        {
            var tokens = new List<string>();

            var context = new FluidParseContext( template );

            var lavaTokens = TagsList.Parse( template, context ).Select( x => x.ToString() ).ToList();

            // If the template contains only literal text, add the entire content as a single text token.
            if ( !lavaTokens.Any()
                 && !string.IsNullOrEmpty( template ) )
            {
                lavaTokens.Add( template );
            }

            return lavaTokens;
        }

    }

    #region Support Classes

    /* [2021-02-02] DJL
     * These classes have been added to support Lava-specific changes to the Fluid Parser.
     * 
     */

    public enum LavaDocumentTokenTypeSpecifier
    {
        Output,
        Tag,
        Literal
    }

    /// <summary>
    /// An element from a Fluid template that has been parsed.
    /// </summary>
    internal class LavaDocumentToken
    {
        public LavaDocumentToken( LavaDocumentTokenTypeSpecifier elementType, string content )
        {
            ElementType = elementType;
            Content = content;
        }

        public LavaDocumentTokenTypeSpecifier ElementType { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            if ( ElementType == LavaDocumentTokenTypeSpecifier.Output )
            {
                return "{{ " + Content + " }}";
            }
            else if ( ElementType == LavaDocumentTokenTypeSpecifier.Tag )
            {
                return "{% " + Content + " %}";
            }

            return Content;
        }
    }

    #endregion
}
