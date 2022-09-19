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
using Fluid.Parser;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Rock.Lava.Fluid
{
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
        /// A parser that captures the literal content of a Lava Block.
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
                TextPosition nextEndTagPos;

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
                var p = ( FluidParseContext ) context;

                // If we are processing the content of a {% liquid %} tag, and scanning for a standard liquid open tag token,
                // the existence of the open tag token is implied so return a match.
                if ( p.InsideLiquidTag )
                {
                    var lavaTagResult = new LavaTagResult()
                    {
                        TagResult = TagResult.TagOpen,
                        Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset ),
                        TagFormat = _format
                    };

                    result.Set( start.Offset, context.Scanner.Cursor.Offset, lavaTagResult );
                    return true;
                }

                // Find the tag start token.
                if ( context.Scanner.ReadChar( _firstTagChar ) && context.Scanner.ReadChar( _secondTagChar ) )
                {
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
                var p = ( FluidParseContext ) context;
                var newLineIsPresent = false;

                // Process the whitespace preceding the end tag token.
                if ( _skipWhiteSpace )
                {
                    if ( p.InsideLiquidTag )
                    {
                        // If processing the content of a {% liquid %} tag, new lines should also be interpreted as a close tag token.
                        var cursor = context.Scanner.Cursor;
                        while ( Character.IsWhiteSpace( cursor.Current ) )
                        {
                            cursor.Advance();
                        }

                        if ( Character.IsNewLine( cursor.Current ) )
                        {
                            newLineIsPresent = true;
                            while ( Character.IsNewLine( cursor.Current ) )
                            {
                                cursor.Advance();
                            }
                        }
                    }
                    else
                    {
                        context.SkipWhiteSpace();
                    }
                }

                // Process the end tag token.
                var start = context.Scanner.Cursor.Position;
                bool trim;

                if ( p.InsideLiquidTag )
                {
                    // If processing the content of a {% liquid %} tag, either a newline or an explicit tag close token can signify the end of a tag.
                    if ( newLineIsPresent )
                    {
                        // If we encountered a newline sequence, imply the existence of a tag close token.
                        var lavaTagResult = new LavaTagResult()
                        {
                            TagResult = TagResult.TagClose,
                            Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset ),
                            TagFormat = _format
                        };

                        result.Set( start.Offset, context.Scanner.Cursor.Offset, lavaTagResult );
                        return true;
                    }
                    else
                    {
                        // Find an explicit tag close token.
                        trim = context.Scanner.ReadChar( '-' );

                        if ( context.Scanner.ReadChar( _firstTagChar ) && context.Scanner.ReadChar( _secondTagChar ) )
                        {
                            p.StripNextTextSpanStatement = trim;
                            p.PreviousTextSpanStatement = null;
                            p.PreviousIsTag = true;
                            p.PreviousIsOutput = false;

                            context.Scanner.Cursor.ResetPosition( start );

                            var lavaTagResult = new LavaTagResult()
                            {
                                TagResult = TagResult.TagClose,
                                Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset ),
                                TagFormat = _format
                            };
                            result.Set( start.Offset, start.Offset, lavaTagResult );
                            return true;
                        }

                        // Not found, so return the scanner to the start position.
                        context.Scanner.Cursor.ResetPosition( start );
                        return false;
                    }
                }
                else
                {
                    // Find the tag close token.
                    trim = context.Scanner.ReadChar( '-' );
                    if ( context.Scanner.ReadChar( _firstTagChar ) && context.Scanner.ReadChar( _secondTagChar ) )
                    {
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

                    // Not found, so return the scanner to the start position.
                    context.Scanner.Cursor.ResetPosition( start );
                    return false;
                }
            }
        }
    }
}
