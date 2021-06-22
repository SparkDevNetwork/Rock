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
    /// An implementation of the Fluid Liquid parser for Lava.
    /// </summary>

    /* [2021-04-29] DJL
     * This implementation extends the Fluid parser to capture the source text of Lava blocks as they are processed,
     * which is necessary to support a framework-independent implementation of custom tags and blocks by
     * working directly with the source text of the template.
     */
    internal class LavaFluidParser : FluidParser
    {
        #region Lava Template Element Parsers

        private static readonly Parser<LavaDocumentToken> Output = OutputStart.SkipAnd( AnyCharBefore( OutputEnd, canBeEmpty: true ) )
            .AndSkip( OutputEnd )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Output, x.ToString() ) );

        private static readonly Parser<LavaDocumentToken> Tag = TagStart.SkipAnd( AnyCharBefore( TagEnd, canBeEmpty: true ) )
            .AndSkip( TagEnd )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Tag, x.ToString() ) );

        private static readonly Parser<LavaDocumentToken> Text = AnyCharBefore( OutputStart.Or( TagStart ) )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Literal, x.ToString() ) );

        public static readonly Parser<List<LavaDocumentToken>> TagsList = ZeroOrMany( Output.Or( Tag ).Or( Text ) );

        public new static Parser<TagResult> TagStart => FluidParser.TagStart;
        public new static Parser<TagResult> TagEnd => FluidParser.TagEnd;

        protected static readonly Parser<char> Space = Terms.Char( ' ' );

        public Parser<List<FilterArgument>> ArgumentsListParser => ArgumentsList;
        public Parser<List<FilterArgument>> LavaArgumentsListParser;

        #endregion

        #region Constructors
        public LavaFluidParser()
            : base()
        {
            InitializeLavaElements();
        }
        private void InitializeLavaElements()
        {
            // Define a parser for a named argument list separated by spaces in the form:
            // [name1:]value1 [name2:]value2 ...
            LavaArgumentsListParser = OneOf( TagEnd.Then( x => new List<FilterArgument>() ),
                Separated( Space,
                            OneOf(
                                Identifier.AndSkip( Colon ).And( Primary ).Then( x => new FilterArgument( x.Item1, x.Item2 ) ),
                                Primary.Then( x => new FilterArgument( null, x ) )
                            ) ) );

            // Replace the default Fluid comment block to allow empty content.
            var commentTag = TagEnd
                       .SkipAnd( AnyCharBefore( CreateTag( "endcomment" ), canBeEmpty: true ) )
                       .AndSkip( CreateTag( "endcomment" ).ElseError( $"'{{% endcomment %}}' was expected" ) )
                       .Then<Statement>( x => new CommentStatement( x ) )
                       .ElseError( "Invalid 'comment' tag" );

            this.RegisteredTags["comment"] = commentTag;
        }

        #endregion

        /// <summary>
        /// Register a Lava Block with the Fluid Parser.
        /// </summary>
        /// <param name="tagName"></param>
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
                .And( new LavaTagParsers.LavaBlockContentParser( tagName ) )
                .Then<Statement>( x => new FluidLavaBlockStatement( this, tagName, x.Item1, x.Item2 ) )
                .ElseError( $"Invalid '{{% {tagName} %}}' tag" );

            this.RegisteredTags[tagName] = lavaBlock;
        }

        /// <summary>
        /// Register a Lava Tag with the Fluid Parser.
        /// </summary>
        /// <param name="tagName"></param>
        public void RegisterLavaTag( string tagName )
        {
            // Create a parser for the Lava tag that does the following:
            // 1. Captures any optional attributes that are contained in the open tag.
            // 2. Create a new Statement that will execute when the tag is rendered.
            // 3. Throw an exception if the Tag is malformed.
            var lavaTag = AnyCharBefore( TagEnd, canBeEmpty: true )
                           .AndSkip( TagEnd )
                           .Then<Statement>( x => new FluidLavaTagStatement( tagName, x ) )
                           .ElseError( $"Invalid '{{% {tagName} %}}' tag" );

            this.RegisteredTags[tagName] = lavaTag;
        }

        /// <summary>
        /// Parse the supplied template into a collection of tokens that are recognized by the Fluid parser.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static List<string> ParseToTokens( string template )
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

    /// <summary>
    /// Extends the Fluid TagResult to include the content and position of the source text.
    /// </summary>
    public class LavaTagResult
    {
        public LavaTagResult()
        {
            //
        }
        public LavaTagResult( TagResult result, TextSpan text )
        {
            TagResult = result;
            Text = text;
        }

        public TagResult TagResult;
        public TextSpan Text;
    }

    /// <summary>
    /// Defines a set of parsers used to process Lava Tags in Fluid.
    /// </summary>
    public static class LavaTagParsers
    {
        public static Parser<LavaTagResult> LavaTagStart( bool skipWhiteSpace = false ) => new LavaTagStartParser( skipWhiteSpace );
        public static Parser<LavaTagResult> LavaTagWithAttributes( string tagName ) => new LavaTagWithAttributesParser( tagName );

        /// <summary>
        /// A parser that captures the source text of a Lava Block.
        /// </summary>
        internal class LavaBlockContentParser : Parser<TextSpan>
        {
            private readonly bool _skipWhiteSpace = false;
            private string _tagName;
            public LavaBlockContentParser( string tagName )
            {
                _tagName = tagName;
            }
            public override bool Parse( ParseContext context, ref ParseResult<TextSpan> result )
            {
                if ( _skipWhiteSpace )
                {
                    context.SkipWhiteSpace();
                }

                var start = context.Scanner.Cursor.Position;

                var endTagText = "end" + _tagName;

                var openTagParser = AnyCharBefore( LavaTagWithAttributes( _tagName ), canBeEmpty: true )
                    .SkipAnd( LavaTagWithAttributes( _tagName ) );
                var closeTagParser = AnyCharBefore( LavaTagWithAttributes( endTagText ), canBeEmpty: true )
                    .SkipAnd( LavaTagWithAttributes( endTagText ) );

                int openTags = 1;
                TextPosition nextStartTagPos = TextPosition.Start;
                TextPosition nextEndTagPos = TextPosition.Start;

                TextPosition currentSearchStart = start;
                var resultStartTag = new ParseResult<LavaTagResult>();
                var resultEndTag = new ParseResult<LavaTagResult>();
                var startTagFound = true;
                var endTagFound = false;

                var cursor = context.Scanner.Cursor;

                int startIndex = start.Offset;
                int endTagStartIndex = -1;
                int endTagEndIndex = -1;

                while ( openTags > 0 && ( startTagFound || endTagFound ) )
                {
                    // Find the next instances of start and end tags.
                    startTagFound = openTagParser.Parse( context, ref resultStartTag );
                    if ( startTagFound )
                    {
                        nextStartTagPos = startTagFound ? new TextPosition( resultStartTag.Value.Text.Offset, cursor.Position.Line, cursor.Position.Column ) : TextPosition.Start;
                    }

                    cursor.ResetPosition( currentSearchStart );
                    endTagFound = closeTagParser.Parse( context, ref resultEndTag );

                    if ( endTagFound )
                    {
                        endTagStartIndex = resultEndTag.Value.Text.Offset;
                        endTagEndIndex = resultEndTag.End;

                        nextEndTagPos = endTagFound ? new TextPosition( endTagStartIndex, cursor.Position.Line, cursor.Position.Column ) : TextPosition.Start;

                        if ( startTagFound
                             && nextStartTagPos.Offset < nextEndTagPos.Offset )
                        {
                            // A start tag was found that precedes the next end tag.
                            // Increment the open tag counter and continue searching.
                            openTags++;
                            context.Scanner.Cursor.ResetPosition( nextStartTagPos );
                        }
                        else
                        {
                            openTags--;
                            context.Scanner.Cursor.ResetPosition( nextEndTagPos );
                        }
                    }
                    else
                    {
                        if ( startTagFound )
                        {
                            openTags++;
                            context.Scanner.Cursor.ResetPosition( nextStartTagPos );
                        }
                    }

                    // Move to the next search position.
                    context.Scanner.Cursor.Advance();

                    currentSearchStart = context.Scanner.Cursor.Position;
                }

                // If the number of start and end tags do not match, the block is invalid.
                if ( openTags != 0 )
                {
                    return false;
                }

                // Store the text content of the block in the parser result.
                if ( startIndex >= 0 && endTagStartIndex >= 0 )
                {
                    result.Set( start.Offset, context.Scanner.Cursor.Offset,
                        new TextSpan( context.Scanner.Buffer, startIndex, endTagStartIndex - startIndex ) );
                }

                // Position the cursor at the end of the closing tag.
                if ( endTagEndIndex >= 0 )
                {
                    context.Scanner.Cursor.ResetPosition( new TextPosition( endTagEndIndex, 0, 0 ) );
                }

                return true;
            }
        }

        /// <summary>
        /// A parser that captures a complete Lava Tag, including optional tag attributes.
        /// </summary>
        internal class LavaTagWithAttributesParser : Parser<LavaTagResult>
        {
            private readonly bool _skipWhiteSpace = false;
            private string _tagName;
            public LavaTagWithAttributesParser( string tagName )
            {
                _tagName = tagName;
            }
            public override bool Parse( ParseContext context, ref ParseResult<LavaTagResult> result )
            {
                var start = context.Scanner.Cursor.Position;

                if ( _skipWhiteSpace )
                {
                    context.SkipWhiteSpace();
                }

                var tagParser = LavaTagParsers.LavaTagStart().And( Terms.Text( _tagName ) ).And( AnyCharBefore( LavaFluidParser.TagEnd, canBeEmpty: true ) ).And( LavaFluidParser.TagEnd );

                var tagResult = new ParseResult<(LavaTagResult, string, TextSpan, TagResult)>();

                var parseSucceeded = tagParser.Parse( context, ref tagResult );

                if ( !parseSucceeded )
                {
                    context.Scanner.Cursor.ResetPosition( start );
                    return false;
                }

                var lavaResult = new LavaTagResult( tagResult.Value.Item1.TagResult, new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Position.Offset - start.Offset ) );

                result.Set( start.Offset, context.Scanner.Cursor.Offset, lavaResult );

                return true;
            }
        }

        /// <summary>
        /// A re-implementation of Fluid.Parser.TagStartParser to capture additional information about the position of the tag in the source text.
        /// </summary>
        internal class LavaTagStartParser : Parser<LavaTagResult>
        {
            private readonly bool _skipWhiteSpace;

            public LavaTagStartParser( bool skipWhiteSpace = false )
            {
                _skipWhiteSpace = skipWhiteSpace;
            }
            public override bool Parse( ParseContext context, ref ParseResult<LavaTagResult> result )
            {
                if ( _skipWhiteSpace )
                {
                    context.SkipWhiteSpace();
                }

                var start = context.Scanner.Cursor.Position;

                if ( context.Scanner.ReadChar( '{' ) && context.Scanner.ReadChar( '%' ) )
                {
                    var p = (FluidParseContext)context;

                    var trim = context.Scanner.ReadChar( '-' );

                    if ( p.PreviousTextSpanStatement != null )
                    {
                        if ( trim )
                        {
                            p.PreviousTextSpanStatement.StripRight = true;
                        }

                        p.PreviousTextSpanStatement.NextIsTag = true;

                        p.PreviousTextSpanStatement = null;
                    }

                    var lavaTagResult = new LavaTagResult()
                    {
                        TagResult = trim ? TagResult.TagOpenTrim : TagResult.TagOpen,
                        Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset )
                    };

                    result.Set( start.Offset, context.Scanner.Cursor.Offset, lavaTagResult );

                    return true;
                }
                else
                {
                    // Return the scanner to the start position.
                    context.Scanner.Cursor.ResetPosition( start );
                    return false;
                }
            }
        }

        /// <summary>
        /// A re-implementation of Fluid.Parser.TagEndParser to capture additional information about the position of the tag in the source text.
        /// </summary>
        internal class LavaTagEndParser : Parser<LavaTagResult>
        {
            private readonly bool _skipWhiteSpace;

            public LavaTagEndParser( bool skipWhiteSpace = false )
            {
                _skipWhiteSpace = skipWhiteSpace;
            }

            public override bool Parse( ParseContext context, ref ParseResult<LavaTagResult> result )
            {
                if ( _skipWhiteSpace )
                {
                    context.SkipWhiteSpace();
                }

                var start = context.Scanner.Cursor.Position;

                bool trim = context.Scanner.ReadChar( '-' );

                if ( context.Scanner.ReadChar( '%' ) && context.Scanner.ReadChar( '}' ) )
                {
                    var p = (FluidParseContext)context;

                    p.StripNextTextSpanStatement = trim;
                    p.PreviousTextSpanStatement = null;
                    p.PreviousIsTag = true;
                    p.PreviousIsOutput = false;

                    var lavaTagResult = new LavaTagResult()
                    {
                        TagResult = trim ? TagResult.TagCloseTrim : TagResult.TagClose,
                        Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset )
                    };

                    result.Set( start.Offset, context.Scanner.Cursor.Offset, lavaTagResult );

                    return true;
                }
                else
                {
                    // Return the scanner to the start position.
                    context.Scanner.Cursor.ResetPosition( start );
                    return false;
                }
            }
        }
    }

    #endregion
}
