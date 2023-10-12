﻿// <copyright>
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
using System.Diagnostics;
using Fluid.Parser;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// Defines a set of parsers used to process Lava Tags in Fluid that can be chained together with other standard Fluid parsers.
    /// </summary>
    public static class LavaFluidTagParsers
    {
        private static Parser<TagResult> _lavaBlockCommentStart = null;
        public static Parser<TagResult> LavaBlockCommentStart
        {
            get
            {
                if ( _lavaBlockCommentStart == null )
                {
                    var parser = new LavaTagParsers.LavaTagStartParser( LavaTagFormatSpecifier.BlockComment );
                    _lavaBlockCommentStart = parser.AsFluidTagResultParser();
                }

                return _lavaBlockCommentStart;
            }
        }

        private static Parser<TagResult> _lavaBlockCommentEnd = null;
        public static Parser<TagResult> LavaBlockCommentEnd
        {
            get
            {
                if ( _lavaBlockCommentEnd == null )
                {
                    var parser = new LavaTagParsers.LavaTagEndParser( LavaTagFormatSpecifier.BlockComment );
                    _lavaBlockCommentEnd = parser.AsFluidTagResultParser();
                }

                return _lavaBlockCommentEnd;
            }
        }

        private static Parser<TagResult> _lavaInlineCommentStart = null;
        public static Parser<TagResult> LavaInlineCommentStart
        {
            get
            {
                if ( _lavaInlineCommentStart == null )
                {
                    var parser = new LavaTagParsers.LavaTagStartParser( LavaTagFormatSpecifier.InlineComment );
                    _lavaInlineCommentStart = parser.AsFluidTagResultParser();
                }

                return _lavaInlineCommentStart;
            }
        }

        private static Parser<TagResult> _lavaInlineCommentEnd = null;
        public static Parser<TagResult> LavaInlineCommentEnd
        {
            get
            {
                if ( _lavaInlineCommentEnd == null )
                {
                    var parser = new LavaTagParsers.LavaTagEndParser( LavaTagFormatSpecifier.InlineComment );
                    _lavaInlineCommentEnd = parser.AsFluidTagResultParser();
                }

                return _lavaInlineCommentEnd;
            }
        }
    }

    /// <summary>
    /// Defines a set of parsers used to process Lava Tags in Fluid.
    /// Parsers defined in this list can be combined with one another to produce a final result,
    /// but cannot be chained together with standard Fluid parsers.
    /// For implementations that can be combined with standard Fluid parsers, <see cref="LavaFluidTagParsers" /> .
    /// </summary>
    public static class LavaTagParsers
    {
        public static Parser<LavaTagResult> LavaTagWithAttributes( string tagName, LavaTagFormatSpecifier format )
        {
            return new LavaTagWithAttributesParser( tagName, format );
        }

        public static Parser<LavaTagResult> LavaTagStart => _lavaTagStart;
        private static Parser<LavaTagResult> _lavaTagStart = new LavaTagStartParser( LavaTagFormatSpecifier.LiquidTag );

        public static Parser<LavaTagResult> LavaTagEnd => _lavaTagEnd;
        private static Parser<LavaTagResult> _lavaTagEnd = new LavaTagEndParser( LavaTagFormatSpecifier.LiquidTag );

        public static Parser<LavaTagResult> LavaShortcodeStart => _lavaShortcodeStart;
        private static Parser<LavaTagResult> _lavaShortcodeStart = new LavaTagStartParser( LavaTagFormatSpecifier.LavaShortcode );

        public static Parser<LavaTagResult> LavaShortcodeEnd => _lavaShortcodeEnd;
        private static Parser<LavaTagResult> _lavaShortcodeEnd = new LavaTagEndParser( LavaTagFormatSpecifier.LavaShortcode );

        public static Parser<LavaTagResult> LavaBlockCommentStart => _lavaBlockCommentStart;
        private static Parser<LavaTagResult> _lavaBlockCommentStart = new LavaTagStartParser( LavaTagFormatSpecifier.BlockComment );

        public static Parser<LavaTagResult> LavaBlockCommentEnd => _lavaBlockCommentEnd;
        private static Parser<LavaTagResult> _lavaBlockCommentEnd = new LavaTagEndParser( LavaTagFormatSpecifier.BlockComment );

        public static Parser<LavaTagResult> LavaInlineCommentStart => _lavaInlineCommentStart;
        private static Parser<LavaTagResult> _lavaInlineCommentStart = new LavaTagStartParser( LavaTagFormatSpecifier.InlineComment );

        public static Parser<LavaTagResult> LavaInlineCommentEnd => _lavaInlineCommentEnd;
        private static Parser<LavaTagResult> _lavaInlineCommentEnd = new LavaTagEndParser( LavaTagFormatSpecifier.InlineComment );

        /// <summary>
        /// Wraps a custom Lava Parser so that is can be included in a standard Fluid parser chain.
        /// </summary>
        /// <param name="lavaTagParser"></param>
        /// <returns></returns>
        public static Parser<TagResult> AsFluidTagResultParser( this Parser<LavaTagResult> lavaTagParser )
        {
            return new LavaTagResultParser( lavaTagParser );
        }

        /// <summary>
        /// An extension method that wraps a custom Lava Parser so that is can be included in a standard Fluid parser chain.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Parser<TagResult> LavaTagResultToFluidTagResultParser( Parser<LavaTagResult> lavaTagParser )
        {
            return new LavaTagResultParser( lavaTagParser );
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
                    tagParser = LavaShortcodeStart
                        .And( Terms.Text( _tagName ) )
                        .And( AnyCharBefore( LavaShortcodeEnd, canBeEmpty: true ) )
                        .And( LavaShortcodeEnd );
                }
                else
                {
                    tagParser = LavaTagStart
                        .And( Terms.Text( _tagName ) ).
                        And( AnyCharBefore( LavaFluidParser.LavaTokenEndParser, canBeEmpty: true ) )
                        .And( LavaFluidParser.LavaTokenEndParser );
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
            private readonly LavaTagFormatSpecifier _format;
            private readonly char _tagChar1;
            private readonly char _tagChar2;
            private readonly char _tagChar3;

            public LavaTagStartParser( LavaTagFormatSpecifier format )
            {
                if ( format == LavaTagFormatSpecifier.LavaShortcode )
                {
                    _tagChar1 = '{';
                    _tagChar2 = '[';
                    _tagChar3 = '\0';
                }
                else if ( format == LavaTagFormatSpecifier.BlockComment )
                {
                    _tagChar1 = '/';
                    _tagChar2 = '-';
                    _tagChar3 = '\0';
                }
                else if ( format == LavaTagFormatSpecifier.InlineComment )
                {
                    _tagChar1 = '/';
                    _tagChar2 = '/';
                    _tagChar3 = '-';
                }
                else
                {
                    // Standard Liquid tag format.
                    _tagChar1 = '{';
                    _tagChar2 = '%';
                    _tagChar3 = '\0';
                }

                _format = format;
            }

            public override bool Parse( ParseContext context, ref ParseResult<LavaTagResult> result )
            {
                var start = context.Scanner.Cursor.Position;
                var p = ( FluidParseContext ) context;

                LavaTagResult lavaTagResult;

                // If we are processing the content of a {% liquid %} tag and scanning for a standard Liquid open tag or a Lava shortcode,
                // the existence of these tokens is implied so return a matched tag result.
                if ( p.InsideLiquidTag
                     && ( _format == LavaTagFormatSpecifier.LiquidTag || _format == LavaTagFormatSpecifier.LavaShortcode ) )
                {
                    lavaTagResult = new LavaTagResult()
                    {
                        TagResult = TagResult.TagOpen,
                        Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset ),
                        TagFormat = _format
                    };

                    result.Set( start.Offset, context.Scanner.Cursor.Offset, lavaTagResult );
                    return true;
                }

                // Find the tag start token.
                var startTagFound = context.Scanner.ReadChar( _tagChar1 )
                    && context.Scanner.ReadChar( _tagChar2 )
                    && ( _tagChar3 == '\0' || context.Scanner.ReadChar( _tagChar3 ) );
                if ( !startTagFound )
                {
                    // Return the scanner to the start position.
                    context.Scanner.Cursor.ResetPosition( start );
                    return false;
                }

                //  If this is a comment tag, ignore it if it is preceded by an unmatched quote.
                if ( _format == LavaTagFormatSpecifier.BlockComment
                    || _format == LavaTagFormatSpecifier.InlineComment )
                {
                    if ( p.PreviousTextSpanStatement != null )
                    {
                        var inQuote = TextContainsUnpairedQuote( p.PreviousTextSpanStatement.Text );
                        if ( inQuote )
                        {
                            // This comment tag is enclosed in an open quote, and should therefore be ignored.
                            // Return the scanner to the start position and exit.
                            context.Scanner.Cursor.ResetPosition( start );
                            return false;
                        }
                    }
                }

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

                lavaTagResult = new LavaTagResult()
                {
                    TagResult = trim ? TagResult.TagOpenTrim : TagResult.TagOpen,
                    Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset ),
                    TagFormat = _format
                };

                result.Set( start.Offset, context.Scanner.Cursor.Offset, lavaTagResult );
                return true;
            }
        }

        internal static bool TextContainsUnpairedQuote( TextSpan text )
        {
            int singleQuoteCount = 0;
            int doubleQuoteCount = 0;

            for ( var i = text.Offset; i < text.Length; i++ )
            {
                var c = text.Buffer[i];

                if ( c == '\'' )
                {
                    singleQuoteCount++;
                }
                else if ( c == '\"' )
                {
                    doubleQuoteCount++;
                }
            }

            var isQuoted = ( singleQuoteCount % 2 == 1 || doubleQuoteCount % 2 == 1 );
            return isQuoted;
        }

        /// <summary>
        /// A re-implementation of Fluid.Parser.TagEndParser to capture additional information about the position of the tag in the source text.
        /// </summary>
        internal class LavaTagEndParser : Parser<LavaTagResult>
        {
            private readonly bool _skipWhiteSpace;
            private readonly LavaTagFormatSpecifier _format;
            private char _tagChar1;
            private char _tagChar2;
            private bool _autoTrim = false;

            public LavaTagEndParser( LavaTagFormatSpecifier format )
            {
                _skipWhiteSpace = true;
                _format = format;

                if ( _format == LavaTagFormatSpecifier.LavaShortcode )
                {
                    _tagChar1 = ']';
                    _tagChar2 = '}';
                }

                else if ( _format == LavaTagFormatSpecifier.BlockComment )
                {
                    _tagChar1 = '-';
                    _tagChar2 = '/';

                    // Always trim whitespace around comments.
                    _autoTrim = true;
                }
                else if ( _format == LavaTagFormatSpecifier.InlineComment )
                {
                    _tagChar1 = '\r';
                    _tagChar2 = '\n';

                    // The EOL characters form the tag, but they should remain in the output.
                    _skipWhiteSpace = false;
                }
                else
                {
                    // Standard Liquid tag format.
                    _tagChar1 = '%';
                    _tagChar2 = '}';
                }
            }

            public override bool Parse( ParseContext context, ref ParseResult<LavaTagResult> result )
            {
                var p = ( FluidParseContext ) context;
                var newLineIsPresent = false;

                if ( _skipWhiteSpace )
                {
                    if ( p.InsideLiquidTag
                         && ( _format == LavaTagFormatSpecifier.LiquidTag || _format == LavaTagFormatSpecifier.LavaShortcode ) )
                    {
                        // If we are processing the content of a {% liquid %} tag and searching for the closing tag of
                        // a standard Liquid keyword, new lines should also be interpreted as a close tag token.
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

                LavaTagResult lavaTagResult;
                if ( p.InsideLiquidTag
                     && ( _format == LavaTagFormatSpecifier.LiquidTag || _format == LavaTagFormatSpecifier.LavaShortcode ) )
                {
                    // If processing the content of a {% liquid %} tag, either a newline or an explicit tag close token can signify the end of a tag.
                    if ( newLineIsPresent )
                    {
                        // If we encountered a newline sequence, imply the existence of a tag close token.
                        lavaTagResult = new LavaTagResult()
                        {
                            TagResult = TagResult.TagClose,
                            Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset ),
                            TagFormat = _format
                        };

                        result.Set( start.Offset, context.Scanner.Cursor.Offset, lavaTagResult );
                        return true;
                    }

                    // Find an explicit tag close token.
                    trim = _autoTrim || context.Scanner.ReadChar( '-' );

                    var endTagFound = context.Scanner.ReadChar( _tagChar1 ) && context.Scanner.ReadChar( _tagChar2 );
                    if ( !endTagFound )
                    {
                        // Not found, so return the scanner to the start position.
                        context.Scanner.Cursor.ResetPosition( start );
                        return false;

                    }

                    //  If this is a comment tag, ignore it if it is preceded by an unmatched quote.
                    if ( _format == LavaTagFormatSpecifier.BlockComment
                            || _format == LavaTagFormatSpecifier.InlineComment )
                    {
                        if ( p.PreviousTextSpanStatement != null )
                        {
                            var inQuote = TextContainsUnpairedQuote( p.PreviousTextSpanStatement.Text );
                            if ( inQuote )
                            {
                                // This comment tag is enclosed in an open quote, and should therefore be ignored.
                                // Return the scanner to the start position and exit.
                                context.Scanner.Cursor.ResetPosition( start );
                                return false;
                            }
                        }
                    }

                    p.StripNextTextSpanStatement = trim;
                    p.PreviousTextSpanStatement = null;
                    p.PreviousIsTag = true;
                    p.PreviousIsOutput = false;

                    context.Scanner.Cursor.ResetPosition( start );

                    lavaTagResult = new LavaTagResult()
                    {
                        TagResult = TagResult.TagClose,
                        Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset ),
                        TagFormat = _format
                    };
                    result.Set( start.Offset, start.Offset, lavaTagResult );
                    return true;
                }
                else
                {
                    // Find the tag close token.
                    trim = _autoTrim || context.Scanner.ReadChar( '-' );

                    var endTagFound = context.Scanner.ReadChar( _tagChar1 ) && context.Scanner.ReadChar( _tagChar2 );
                    if ( !endTagFound )
                    {
                        // Not found, so return the scanner to the start position.
                        context.Scanner.Cursor.ResetPosition( start );
                        return false;
                    }

                    p.StripNextTextSpanStatement = trim;
                    p.PreviousTextSpanStatement = null;
                    p.PreviousIsTag = true;
                    p.PreviousIsOutput = false;

                    lavaTagResult = new LavaTagResult()
                    {
                        TagResult = trim ? TagResult.TagCloseTrim : TagResult.TagClose,
                        Text = new TextSpan( context.Scanner.Buffer, start.Offset, context.Scanner.Cursor.Offset - start.Offset ),
                        TagFormat = _format
                    };

                    result.Set( start.Offset, context.Scanner.Cursor.Offset, lavaTagResult );
                    return true;

                }
            }
        }
    }
}
