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
    public enum LavaTagFormatSpecifier
    {
        LiquidTag = 0,
        LavaShortcode = 1
    }

    /// <summary>
    /// An implementation of the Fluid Liquid parser for Lava.
    /// </summary>

    /* [2021-04-29] DJL
     * The Lava parser works by executing a set of parsers across the input template text.
     * Parsers are executed sequentially to process the text at the current scanner position until the first match is found.
     * The ultimate output of each parser is either a Statement for execution when the element is rendered, or a result that
     * can be consumed by another parser in the sequence.
     * 
     * This implementation extends the Fluid parser by adding the following Lava-specific behaviours:
     * 1. Capture the source text of Lava blocks as they are processed, which is needed to support our
     *    implementation of custom tags and blocks that offers direct access to the source text of the template.
     * 2. Adds support for Lava shortcode tags using the "{[ shortcode ]}" syntax.
     */
    internal class LavaFluidParser : FluidParser
    {
        #region Lava Template Element Text Parsers

        /*
         * A Lava Element is any recognized portion of a Lava document, including tags, shortcodes, output and literal text.
         * A Lava Token is a Lava Element that is delimited by a recognized string of characters, such as a tag or shortcode.
         * 
         * Elements must be defined before they are referenced by other elements.
         * Take care when altering the sequence of these definitions, or runtime errors may occur.
         */

        public static Parser<LavaTagResult> LavaTokenStart => OneOf( LavaTagParsers.LavaTagStart(), LavaTagParsers.LavaShortcodeStart() );
        public static Parser<LavaTagResult> LavaTokenEnd => OneOf( LavaTagParsers.LavaTagEnd(), LavaTagParsers.LavaShortcodeEnd() );

        private static readonly Parser<char> Space = Terms.Char( ' ' );

        private static readonly Parser<LavaDocumentToken> LavaOutputTokenParser = OutputStart.SkipAnd( AnyCharBefore( OutputEnd, canBeEmpty: true ) )
            .AndSkip( OutputEnd )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Output, x.ToString() ) );

        private static readonly Parser<LavaDocumentToken> LavaTagTokenParser = LavaTokenStart.SkipAnd( AnyCharBefore( LavaTokenEnd, canBeEmpty: true ) )
            .AndSkip( LavaTokenEnd )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Tag, x.ToString() ) );

        private static readonly Parser<LavaDocumentToken> LavaTextTokenParser = AnyCharBefore( OutputStart.Or( LavaTokenStart.AsFluidTagResultParser() ) )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Literal, x.ToString() ) );

        private static Parser<LavaTagResult> ShortcodeTagStart = LavaTagParsers.LavaShortcodeStart();
        private static Parser<LavaTagResult> ShortcodeTagEnd = LavaTagParsers.LavaShortcodeEnd();

        private static readonly Parser<LavaDocumentToken> LavaShortcodeTokenParser = ShortcodeTagStart.SkipAnd( AnyCharBefore( ShortcodeTagEnd, canBeEmpty: true ) )
            .AndSkip( ShortcodeTagEnd )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Shortcode, x.ToString() ) );

        // The complete list of valid tokens in a Lava document.
        public static readonly Parser<List<LavaDocumentToken>> LavaTokensListParser = ZeroOrMany( LavaOutputTokenParser.Or( LavaShortcodeTokenParser ).Or( LavaTagTokenParser ).Or( LavaTextTokenParser ) );

        public Parser<List<FilterArgument>> ArgumentsListParser => ArgumentsList;
        public Parser<List<FilterArgument>> LavaArgumentsListParser;

        #endregion

        #region Constructors
        public LavaFluidParser()
            : base()
        {
            RegisterLavaCommentTag();
            RegisterLavaCaptureTag();
            RegisterLavaElseIfTag();

            RegisterLavaOperators();

            DefineLavaElementParsers();
            DefineLavaDocumentParsers();
        }

        private void DefineLavaElementParsers()
        {
            // Define a parser for a named argument list separated by spaces in the form:
            // [name1:]value1 [name2:]value2 ...
            LavaArgumentsListParser = OneOf( LavaTokenEnd.Then( x => new List<FilterArgument>() ),
                Separated( Space,
                            OneOf(
                                Identifier.AndSkip( Colon ).And( Primary ).Then( x => new FilterArgument( x.Item1, x.Item2 ) ),
                                Primary.Then( x => new FilterArgument( null, x ) )
                            ) ) );
        }

        private void DefineLavaDocumentParsers()
        {
            // Define the top-level parsers.
            var anyTags = KnownTagsParser( throwOnUnknownTag: false );

            var knownTags = KnownTagsParser( throwOnUnknownTag: true );

            var outputElement = OutputStart.SkipAnd( FilterExpression.And( OutputEnd.ElseError( ErrorMessages.ExpectedOutputEnd ) )
                .Then<Statement>( x => new OutputStatement( x.Item1 ) ) );

            var textElement = AnyCharBefore( OutputStart.Or( LavaTagParsers.LavaTagStart().AsFluidTagResultParser() ).Or( LavaTagParsers.LavaShortcodeStart().AsFluidTagResultParser() ) )
                .Then<Statement>( ( ctx, x ) =>
                {
                    // Keep track of each text span such that whitespace trimming can be applied
                    var p = ( FluidParseContext ) ctx;

                    var result = new TextSpanStatement( x );

                    p.PreviousTextSpanStatement = result;

                    if ( p.StripNextTextSpanStatement )
                    {
                        result.StripLeft = true;
                        p.StripNextTextSpanStatement = false;
                    }

                    result.PreviousIsTag = p.PreviousIsTag;
                    result.PreviousIsOutput = p.PreviousIsOutput;

                    return result;
                } );

            var blockCommentElement = Terms.Text( "/-" ).SkipAnd( AnyCharBefore( Terms.Text( "-/" ) ) );
            var lineCommentElement = Terms.Text( "/-" ).SkipAnd( AnyCharBefore( Terms.Char( '\r' ).SkipAnd( Terms.Char( '\n' ) ) ) );

            var commentElement = blockCommentElement.Or( lineCommentElement );

            // Set the parser to be used for a block element.
            // This parser returns an empty result when an unknown tag is found.
            AnyTagsList.Parser = ZeroOrMany( outputElement.Or( anyTags ).Or( textElement ) );

            // Set the parser to be used for the entire template.
            // This parser raises an exception when an unknown tag is found.
            KnownTagsList.Parser = ZeroOrMany( outputElement.Or( knownTags ).Or( textElement ) );

            // Set the Grammer parser, which represents the top-level document elements.
            Grammar = KnownTagsList;
        }

        private void RegisterLavaOperators()
        {
            // Replace the default Fluid comparison operators to add support for
            // automatic operand type conversions, date comparisons, and empty string comparisons.
            this.RegisteredOperators["=="] = ( a, b ) =>
            {
                return new LavaEqualBinaryExpression( a, b, failIfEqual: false );
            };
            this.RegisteredOperators["!="] = ( a, b ) =>
            {
                return new LavaEqualBinaryExpression( a, b, failIfEqual: true );
            };
            this.RegisteredOperators["<"] = ( a, b ) =>
            {
                return new LavaLessThanExpression( a, b, failIfEqual: true );
            };
            this.RegisteredOperators["<="] = ( a, b ) =>
            {
                return new LavaLessThanExpression( a, b, failIfEqual: false );
            };
            this.RegisteredOperators[">"] = ( a, b ) =>
            {
                return new LavaGreaterThanExpression( a, b, failIfEqual: true );
            };
            this.RegisteredOperators[">="] = ( a, b ) =>
            {
                return new LavaGreaterThanExpression( a, b, failIfEqual: false );
            };
        }

        /// <summary>
        /// Replace the default Fluid comment block to allow empty content.
        /// </summary>
        private void RegisterLavaCommentTag()
        {
            var commentTag = LavaTagParsers.LavaTagEnd()
                .SkipAnd( AnyCharBefore( CreateTag( "endcomment" ), canBeEmpty: true ) )
                .AndSkip( CreateTag( "endcomment" ).ElseError( $"'{{% endcomment %}}' was expected" ) )
                .Then<Statement>( x => new CommentStatement( x ) )
                .ElseError( "Invalid 'comment' tag" );

            RegisteredTags["comment"] = commentTag;
        }

        /// <summary>
        /// Replace the default Fluid comment block to allow embedded tags.
        /// </summary>
        private void RegisterLavaCaptureTag()
        {
            var captureTag = Identifier
                .AndSkip( TagEnd )
                .And( AnyTagsList )
                .AndSkip( CreateTag( "endcapture" ).ElseError( $"'{{% endcapture %}}' was expected" ) )
                .Then<Statement>( x => new CaptureStatement( x.Item1, x.Item2 ) )
                .ElseError( "Invalid 'capture' tag" );

            RegisteredTags["capture"] = captureTag;
        }

        /// <summary>
        /// Redefine the standard Liquid {% if %} tag to allow "{% elsif %}" or "{% elseif %}".
        /// </summary>
        private void RegisterLavaElseIfTag()
        {
            var ifTag = LogicalExpression
                .AndSkip( TagEnd )
                .And( AnyTagsList )
                .And( ZeroOrMany(
                    TagStart.SkipAnd( Terms.Text( "elsif" ).Or( Terms.Text( "elseif" ) ) ).SkipAnd( LogicalExpression ).AndSkip( TagEnd ).And( AnyTagsList ) )
                    .Then( x => x.Select( e => new ElseIfStatement( e.Item1, e.Item2 ) ).ToList() ) )
                .And( ZeroOrOne(
                    CreateTag( "else" ).SkipAnd( AnyTagsList ) )
                    .Then( x => x != null ? new ElseStatement( x ) : null ) )
                .AndSkip( CreateTag( "endif" ).ElseError( $"'{{% endif %}}' was expected" ) )
                .Then<Statement>( x => new IfStatement( x.Item1, x.Item2, x.Item4, x.Item3 ) )
                .ElseError( "Invalid 'if' tag" );

            RegisteredTags["if"] = ifTag;
        }

        public Parser<Statement> KnownTagsParser( bool throwOnUnknownTag )
        {
            var parser = OneOf(
                LavaTagParsers.LavaTagStart()
                    .SkipAnd( Identifier.ElseError( ErrorMessages.IdentifierAfterTagStart )
                        .Switch( ( context, tagName ) =>
                        {
                            if ( RegisteredTags.TryGetValue( tagName, out var tag ) )
                            {
                                return tag;
                            }
                            else if ( throwOnUnknownTag )
                            {
                                throw new global::Fluid.ParseException( $"Unknown tag '{tagName}' at {context.Scanner.Cursor.Position}" );
                            }

                            return null;
                        } )
                    ),
                LavaTagParsers.LavaShortcodeStart()
                    .SkipAnd( Identifier.ElseError( ErrorMessages.IdentifierAfterTagStart )
                        .Switch( ( context, tagName ) =>
                        {
                            var shortcodeTagName = tagName + "_";

                            if ( RegisteredTags.TryGetValue( shortcodeTagName, out var shortcode ) )
                            {
                                return shortcode;
                            }
                            else if ( throwOnUnknownTag )
                            {
                                throw new global::Fluid.ParseException( $"Unknown shortcode '{tagName}' at {context.Scanner.Cursor.Position}" );
                            }

                            return null;
                        } )
                    ) );

            return parser;
        }

        #endregion

        /// <summary>
        /// Register a Lava Block with the Fluid Parser.
        /// </summary>
        /// <param name="tagName"></param>
        public void RegisterLavaBlock( string tagName, LavaTagFormatSpecifier format = LavaTagFormatSpecifier.LiquidTag )
        {
            // Create a parser for the Lava block that does the following:
            // 1. Captures any optional attributes that are contained in the open tag.
            // 2. Creates a new Statement that will execute when the block is rendered. The Statement captures the block content
            //    as literal text, so that it can be scanned and tokenized by the Lava library before being passed back to Fluid
            //    for final rendering.
            // 3. Throw an exception if the Block is malformed.
            Parser<LavaTagResult> tokenEndParser;

            var registerTagName = tagName;

            if ( format == LavaTagFormatSpecifier.LavaShortcode )
            {
                tokenEndParser = LavaTagParsers.LavaShortcodeEnd();

                tagName = tagName.Substring( 0, tagName.Length - "_".Length );
            }
            else
            {
                tokenEndParser = LavaTokenEnd;
            }

            var lavaBlock = AnyCharBefore( tokenEndParser, canBeEmpty: true )
                .AndSkip( tokenEndParser )
                .And( new LavaTagParsers.LavaBlockContentParser( tagName, format ) )
                .Then<Statement>( x => new FluidLavaBlockStatement( this, tagName, format, x.Item1, x.Item2 ) )
                .ElseError( $"Invalid '{{% {tagName} %}}' tag" );

            RegisteredTags[registerTagName] = lavaBlock;
        }

        /// <summary>
        /// Register a Lava Tag with the Fluid Parser.
        /// </summary>
        /// <param name="tagName"></param>
        public void RegisterLavaTag( string tagName, LavaTagFormatSpecifier format = LavaTagFormatSpecifier.LiquidTag )
        {
            // Create a parser for the Lava tag that does the following:
            // 1. Processes all of the content from the end of the open tag until the end of the closing tag.
            // 2. Captures any optional attributes that are contained in the open tag.
            // 3. Create a new Statement that will execute when the tag is rendered.
            // 4. Throw an exception if the Tag is malformed.
            Parser<LavaTagResult> tokenEndParser;

            var registerTagName = tagName;

            if ( format == LavaTagFormatSpecifier.LavaShortcode )
            {
                tokenEndParser = LavaTagParsers.LavaShortcodeEnd();

                tagName = tagName.Substring( 0, tagName.Length - "_".Length );
            }
            else
            {
                tokenEndParser = LavaTokenEnd;
            }

            var lavaTag = AnyCharBefore( tokenEndParser, canBeEmpty: true )
                           .AndSkip( tokenEndParser )
                           .Then<Statement>( x => new FluidLavaTagStatement( tagName, format, x ) )
                           .ElseError( $"Invalid '{{% {tagName} %}}' tag" );

            this.RegisteredTags[registerTagName] = lavaTag;
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

            var lavaTokens = LavaTokensListParser.Parse( template, context ).Select( x => x.ToString() ).ToList();

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
        Literal,
        Shortcode
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
            else if ( ElementType == LavaDocumentTokenTypeSpecifier.Shortcode )
            {
                return "{[ " + Content + " ]}";
            }

            return Content;
        }
    }

    /// <summary>
    /// Extends the Fluid TagResult to include the content and position of the source text.
    /// </summary>
    /// <remarks>
    /// Although it would be preferrable to derive from Fluid.TagResult, that Type is sealed.
    /// </remarks>
    public class LavaTagResult
    {
        public LavaTagResult()
        {
            //
        }
        public LavaTagResult( TagResult result, TextSpan text, LavaTagFormatSpecifier format )
        {
            TagResult = result;
            Text = text;
        }

        public TagResult TagResult;
        public TextSpan Text;
        public LavaTagFormatSpecifier TagFormat;
    }

    /// <summary>
    /// Defines a set of parsers used to process Lava Tags in Fluid.
    /// </summary>
    public static class LavaTagParsers
    {
        public static Parser<TagResult> AsFluidTagResultParser( this Parser<LavaTagResult> lavaTagParser )
        {
            return new LavaTagResultParser( lavaTagParser );
        }

        public static Parser<LavaTagResult> LavaTagWithAttributes( string tagName, LavaTagFormatSpecifier format )
        {
            return new LavaTagWithAttributesParser( tagName, format );
        }

        public static Parser<LavaTagResult> LavaTagStart( bool skipWhiteSpace = false )
        {
            return new LavaTagStartParser( LavaTagFormatSpecifier.LiquidTag, skipWhiteSpace );
        }

        public static Parser<LavaTagResult> LavaTagEnd()
        {
            return new LavaTagEndParser( LavaTagFormatSpecifier.LiquidTag, skipWhiteSpace: true );
        }

        public static Parser<LavaTagResult> LavaShortcodeStart()
        {
            return new LavaTagStartParser( LavaTagFormatSpecifier.LavaShortcode, skipWhiteSpace: false );
        }

        public static Parser<TagResult> LavaTagResultToFluidTagResultParser( Parser<LavaTagResult> result )
        {
            return new LavaTagResultParser( result );
        }

        public static Parser<LavaTagResult> LavaShortcodeEnd()
        {
            return new LavaTagEndParser( LavaTagFormatSpecifier.LavaShortcode, skipWhiteSpace: true );
        }

        /// <summary>
        /// Converts a LavaTagResult to a Fluid TagResult for use with a Fluid Parser chain.
        /// </summary>
        internal class LavaTagResultParser : Parser<TagResult>
        {
            private Parser<LavaTagResult> _tagParser;

            public LavaTagResultParser( Parser<LavaTagResult> tagParser )
            {
                _tagParser = tagParser;
            }

            public override bool Parse( ParseContext context, ref ParseResult<TagResult> result )
            {
                if ( _tagParser == null )
                {
                    return false;
                }

                var lavaParseResult = new ParseResult<LavaTagResult>();

                var isValid = _tagParser.Parse( context, ref lavaParseResult );

                if ( !isValid )
                {
                    return false;
                }

                result = new ParseResult<TagResult>( lavaParseResult.Start, lavaParseResult.End, lavaParseResult.Value.TagResult );

                return true;
            }
        }

        /// <summary>
        /// A parser that captures the literal content a Lava Block.
        /// This parser is invoked after the opening tag has been parsed, and is intended to capture all content until the closing tag,
        /// including nested blocks.
        /// </summary>
        internal class LavaBlockContentParser : Parser<TextSpan>
        {
            private readonly bool _skipWhiteSpace = false;
            private string _tagName;
            private LavaTagFormatSpecifier _tagFormat;

            public LavaBlockContentParser( string tagName, LavaTagFormatSpecifier format )
            {
                _tagName = tagName;
                _tagFormat = format;
            }

            public override bool Parse( ParseContext context, ref ParseResult<TextSpan> result )
            {
                if ( _skipWhiteSpace )
                {
                    context.SkipWhiteSpace();
                }

                // This block is invoked when the scanner is positioned at the start of the block content, after the open tag.
                var openTags = 1;
                var start = context.Scanner.Cursor.Position;

                // Define parsers that can identify opening and closing tags for this block type, to track nested blocks of the same type if necessary.
                var openTagOnlyParser = LavaTagWithAttributes( _tagName, _tagFormat );

                var openTagParser = AnyCharBefore( openTagOnlyParser, canBeEmpty: true )
                    .SkipAnd( openTagOnlyParser );

                var closeTagOnlyParser = LavaTagWithAttributes( "end" + _tagName, _tagFormat );

                var closeTagParser = AnyCharBefore( closeTagOnlyParser, canBeEmpty: true )
                    .SkipAnd( closeTagOnlyParser );

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
            private LavaTagFormatSpecifier _tagFormat;

            public LavaTagWithAttributesParser( string tagName, LavaTagFormatSpecifier format )
            {
                _tagName = tagName;
                _tagFormat = format;
            }

            public override bool Parse( ParseContext context, ref ParseResult<LavaTagResult> result )
            {
                var start = context.Scanner.Cursor.Position;

                if ( _skipWhiteSpace )
                {
                    context.SkipWhiteSpace();
                }

                Parser<(LavaTagResult, string, TextSpan, LavaTagResult)> tagParser;

                if ( _tagFormat == LavaTagFormatSpecifier.LavaShortcode )
                {
                    tagParser = LavaTagParsers.LavaShortcodeStart()
                        .And( Terms.Text( _tagName ) )
                        .And( AnyCharBefore( LavaTagParsers.LavaShortcodeEnd(), canBeEmpty: true ) )
                        .And( LavaTagParsers.LavaShortcodeEnd() );
                }
                else
                {
                    tagParser = LavaTagParsers.LavaTagStart()
                        .And( Terms.Text( _tagName ) ).
                        And( AnyCharBefore( LavaFluidParser.LavaTokenEnd, canBeEmpty: true ) )
                        .And( LavaFluidParser.LavaTokenEnd );
                }

                var tagResult = new ParseResult<(LavaTagResult, string, TextSpan, LavaTagResult)>();

                var parseSucceeded = tagParser.Parse( context, ref tagResult );

                if ( !parseSucceeded )
                {
                    context.Scanner.Cursor.ResetPosition( start );
                    return false;
                }

                var lavaResult = new LavaTagResult( tagResult.Value.Item1.TagResult,
                    new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Position.Offset - start.Offset ),
                    _tagFormat );

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
            private readonly LavaTagFormatSpecifier _format;
            private char _firstTagChar;
            private char _secondTagChar;

            public LavaTagStartParser( LavaTagFormatSpecifier format, bool skipWhiteSpace = false )
            {
                _skipWhiteSpace = skipWhiteSpace;

                _firstTagChar = '{';

                if ( format == LavaTagFormatSpecifier.LavaShortcode )
                {
                    _secondTagChar = '[';
                }
                else
                {
                    _secondTagChar = '%';
                }

                _format = format;

            }
            public override bool Parse( ParseContext context, ref ParseResult<LavaTagResult> result )
            {
                if ( _skipWhiteSpace )
                {
                    context.SkipWhiteSpace();
                }

                var start = context.Scanner.Cursor.Position;

                if ( context.Scanner.ReadChar( _firstTagChar ) && context.Scanner.ReadChar( _secondTagChar ) )
                {
                    var p = ( FluidParseContext ) context;

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
                        Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset ),
                        TagFormat = _format
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
            private readonly LavaTagFormatSpecifier _format;
            private char _firstTagChar;
            private char _secondTagChar;

            public LavaTagEndParser( LavaTagFormatSpecifier format, bool skipWhiteSpace = false )
            {
                _skipWhiteSpace = skipWhiteSpace;

                _format = format;

                if ( _format == LavaTagFormatSpecifier.LavaShortcode )
                {
                    _firstTagChar = ']';
                }
                else
                {
                    _firstTagChar = '%';
                }

                _secondTagChar = '}';
            }

            public override bool Parse( ParseContext context, ref ParseResult<LavaTagResult> result )
            {
                if ( _skipWhiteSpace )
                {
                    context.SkipWhiteSpace();
                }

                var start = context.Scanner.Cursor.Position;

                bool trim = context.Scanner.ReadChar( '-' );

                if ( context.Scanner.ReadChar( _firstTagChar ) && context.Scanner.ReadChar( _secondTagChar ) )
                {
                    var p = ( FluidParseContext ) context;

                    p.StripNextTextSpanStatement = trim;
                    p.PreviousTextSpanStatement = null;
                    p.PreviousIsTag = true;
                    p.PreviousIsOutput = false;

                    var lavaTagResult = new LavaTagResult()
                    {
                        TagResult = trim ? TagResult.TagCloseTrim : TagResult.TagClose,
                        Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset ),
                        TagFormat = _format
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
