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
using System.Collections.Generic;
using System.Linq;
using Fluid;
using Fluid.Ast;
using Fluid.Parser;
using Fluid.Values;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Rock.Lava.Fluid
{
    public enum LavaTagFormatSpecifier
    {
        LiquidTag = 0,
        LavaShortcode = 1,
        BlockComment = 2,
        InlineComment = 3
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
     * 3. Adds support for Lava comment syntax.
     * 4. Adds support for basic date and string comparisons using standard operators.
     */
    internal class LavaFluidParser : FluidParser
    {
        #region Lava Template Element Text Parsers

        /*
         * A Lava Element is any recognized portion of a Lava document, including tags, shortcodes, output and literal text.
         * A Lava Token is a Lava Element that is delimited by a recognized string of characters, such as a tag, a shortcode, or a shorthand comment.
         * 
         * Elements must be defined before they are referenced by other elements.
         * Take care when altering the sequence of these definitions, or runtime errors may occur.
         */

        #region Internal Parsers

        private static readonly Parser<char> SpaceParser = Terms.Char( ' ' );

        private static readonly Parser<LavaDocumentToken> LavaOutputTokenParser =
            OutputStart
            .SkipAnd( AnyCharBefore( OutputEnd, canBeEmpty: true ) )
            .AndSkip( OutputEnd )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Output, x.ToString() ) );

        private static readonly Parser<LavaDocumentToken> LavaTagTokenParser =
            LavaTokenStartParser.SkipAnd( AnyCharBefore( LavaTokenEndParser, canBeEmpty: true ) )
            .AndSkip( LavaTokenEndParser )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Tag, x.ToString() ) );

        private static readonly Parser<LavaDocumentToken> LavaInlineCommentTokenParser =
            LavaTagParsers.LavaInlineCommentStart.SkipAnd( AnyCharBefore( LavaTagParsers.LavaInlineCommentEnd, canBeEmpty: true ) )
            .AndSkip( LavaTagParsers.LavaInlineCommentEnd )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.InlineComment, x.ToString() ) );
        private static readonly Parser<LavaDocumentToken> LavaBlockCommentTokenParser =
            LavaTagParsers.LavaBlockCommentStart.SkipAnd( AnyCharBefore( LavaTagParsers.LavaBlockCommentEnd, canBeEmpty: true ) )
            .AndSkip( LavaTagParsers.LavaBlockCommentEnd )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.BlockComment, x.ToString() ) );

        private static readonly Parser<LavaDocumentToken> LavaTextTokenParser =
            AnyCharBefore( OutputStart.Or( LavaTokenStartParser.AsFluidTagResultParser() ).Or( LavaCommentTokenStartParser.AsFluidTagResultParser() ) )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Literal, x.ToString() ) );

        private static Parser<LavaTagResult> ShortcodeTagStartParser = LavaTagParsers.LavaShortcodeStart;
        private static Parser<LavaTagResult> ShortcodeTagEndParser = LavaTagParsers.LavaShortcodeEnd;

        private static readonly Parser<LavaDocumentToken> LavaShortcodeTokenParser =
            ShortcodeTagStartParser.SkipAnd( AnyCharBefore( ShortcodeTagEndParser, canBeEmpty: true ) )
            .AndSkip( ShortcodeTagEndParser )
            .Then( x => new LavaDocumentToken( LavaDocumentTokenTypeSpecifier.Shortcode, x.ToString() ) );

        private Parser<List<Statement>> _anyTagsListParser;
        private Parser<List<Statement>> _knownTagsListParser;

        #endregion

        public static Parser<LavaTagResult> LavaCommentTokenStartParser =>
            OneOf( LavaTagParsers.LavaBlockCommentStart,
                LavaTagParsers.LavaInlineCommentStart );
        public static Parser<LavaTagResult> LavaCommentTokenEndParser =>
            OneOf( LavaTagParsers.LavaBlockCommentEnd,
                LavaTagParsers.LavaInlineCommentEnd );

        public static Parser<LavaTagResult> LavaTokenStartParser =>
            OneOf( LavaTagParsers.LavaTagStart,
                LavaTagParsers.LavaShortcodeStart );
        public static Parser<LavaTagResult> LavaTokenEndParser =>
            OneOf( LavaTagParsers.LavaTagEnd,
                LavaTagParsers.LavaShortcodeEnd );

        // The complete list of valid tokens in a Lava document.
        public static readonly Parser<List<LavaDocumentToken>> LavaTokensListParser =
            ZeroOrMany(
                LavaInlineCommentTokenParser
                .Or( LavaBlockCommentTokenParser )
                .Or( LavaOutputTokenParser )
                .Or( LavaShortcodeTokenParser )
                .Or( LavaTagTokenParser )
                .Or( LavaTextTokenParser ) );

        public Parser<List<FilterArgument>> ArgumentsListParser => ArgumentsList;
        public Parser<List<FilterArgument>> LavaArgumentsListParser;

        #endregion

        #region Constructors
        public LavaFluidParser()
            : base()
        {
            CreateLavaDocumentParsers();

            RegisterLavaCommentTag();
            RegisterLavaCaptureTag();
            RegisterLavaElseIfTag();
            RegisterLavaTag();

            RegisterLavaOperators();

            DefineLavaElementParsers();
            DefineLavaTrueFalseAsCaseInsensitive();
            DefineLavaDocumentParsers();
        }

        private void DefineLavaElementParsers()
        {
            // Define a parser for a named argument list separated by spaces in the form:
            // [name1:]value1 [name2:]value2 ...
            // This parser can also return an empty argument list.
            LavaArgumentsListParser = OneOf( LavaTokenEndParser.Then( x => new List<FilterArgument>() ),
                Separated( SpaceParser,
                            OneOf(
                                Identifier.AndSkip( Colon ).And( Primary ).Then( x => new FilterArgument( x.Item1, x.Item2 ) ),
                                Primary.Then( x => new FilterArgument( null, x ) )
                            ) ) );
        }

        /// <summary>
        /// Redefines the True/False keywords to be case-insensitive.
        /// </summary>
        private void DefineLavaTrueFalseAsCaseInsensitive()
        {
            // To redefine the True and False parsers, we need to rebuild the Fluid Primary expression parser.
            // Fluid defines a Primary expression as: primary => STRING | BOOLEAN | EMPTY | MEMBER | NUMBER.

            // Reproduce the standard Fluid parsers that are internally defined by the default parser.
            var integer = Terms.Integer().Then<Expression>( x => new LiteralExpression( NumberValue.Create( x ) ) );

            var indexer = Between( LBracket, Primary, RBracket ).Then<MemberSegment>( x => new IndexerSegment( x ) );

            var member = Identifier.Then<MemberSegment>( x => new IdentifierSegment( x ) ).And(
                ZeroOrMany(
                    Dot.SkipAnd( Identifier.Then<MemberSegment>( x => new IdentifierSegment( x ) ) )
                    .Or( indexer ) ) )
                .Then( x =>
                {
                    x.Item2.Insert( 0, x.Item1 );
                    return new MemberExpression( x.Item2 );
                } );

            var range = LParen
                .SkipAnd( OneOf( integer, member.Then<Expression>( x => x ) ) )
                .AndSkip( Terms.Text( ".." ) )
                .And( OneOf( integer, member.Then<Expression>( x => x ) ) )
                .AndSkip( RParen )
                .Then<Expression>( x => new RangeExpression( x.Item1, x.Item2 ) );

            Primary.Parser =
                String.Then<Expression>( x => new LiteralExpression( StringValue.Create( x.ToString() ) ) )
                .Or( member.Then<Expression>( x =>
                {
                    if ( x.Segments.Count == 1 )
                    {
                        // Redefine these Liquid keywords as case-insensitive for compatibility with Lava.
                        switch ( ( x.Segments[0] as IdentifierSegment ).Identifier.ToLower() )
                        {
                            case "empty":
                                return EmptyKeyword;
                            case "blank":
                                return BlankKeyword;
                            case "true":
                                return TrueKeyword;
                            case "false":
                                return FalseKeyword;
                        }
                    }

                    return x;
                } ) )
                .Or( Number.Then<Expression>( x => new LiteralExpression( NumberValue.Create( x ) ) ) )
                .Or( range );
        }

        /// <summary>
        /// Defines the root-level parsers for a Lava document.
        /// </summary>
        private void CreateLavaDocumentParsers()
        {
            // Tag Elements: {% tag %} or {[ shortcode ]}
            var anyTags = CreateKnownTagsParser( throwOnUnknownTag: false );
            var knownTags = CreateKnownTagsParser( throwOnUnknownTag: true );

            // Output Element: {{ output }}
            var outputElement = OutputStart
                .SkipAnd( FilterExpression.And( OutputEnd.ElseError( ErrorMessages.ExpectedOutputEnd ) )
                .Then<Statement>( x => new OutputStatement( x.Item1 ) ) );

            // Block Comment Element: /- ... -/
            var blockCommentElement = LavaTagParsers.LavaBlockCommentStart
                .SkipAnd( AnyCharBefore( LavaTagParsers.LavaBlockCommentEnd ).And( LavaTagParsers.LavaBlockCommentEnd )
                    .Then<Statement>( x => new CommentStatement( x.Item1 ) ) );

            // Inline Comment Element: /- ... <eol>
            var inlineCommentElement = LavaTagParsers.LavaInlineCommentStart
                .SkipAnd( AnyCharBefore( LavaTagParsers.LavaInlineCommentEnd ) )
                .Then<Statement>( x => new CommentStatement( x ) );

            var commentElement = blockCommentElement.Or( inlineCommentElement );

            // Text Element: any literal text not contained in one of the preceding elements.
            var startTags = OutputStart
                .Or( LavaTagParsers.LavaTagStart.AsFluidTagResultParser() )
                .Or( LavaTagParsers.LavaShortcodeStart.AsFluidTagResultParser() )
                .Or( LavaFluidTagParsers.LavaBlockCommentStart )
                .Or( LavaFluidTagParsers.LavaInlineCommentStart );

            var textElement = AnyCharBefore( startTags )
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

            // Set the parser to be used for a block element.
            // This parser returns an empty result when an unknown tag is found, so the tag is ignored.
            _anyTagsListParser = ZeroOrMany( commentElement.Or( outputElement ).Or( anyTags ).Or( textElement ) );

            // Set the parser to be used for the entire template.
            // This parser raises an exception when an unknown tag is found.
            _knownTagsListParser = ZeroOrMany( commentElement.Or( outputElement ).Or( knownTags ).Or( textElement ) );
        }

        private void DefineLavaDocumentParsers()
        {
            // Set the parser to be used for a block element.
            // This parser returns an empty result when an unknown tag is found.
            AnyTagsList.Parser = _anyTagsListParser;

            // Set the parser to be used for the entire template.
            // This parser raises an exception when an unknown tag is found.
            KnownTagsList.Parser = _knownTagsListParser;

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
            var commentTag = LavaTagParsers.LavaTagEnd
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
                .And( _anyTagsListParser )
                .And( ZeroOrMany(
                    TagStart.SkipAnd( Terms.Text( "elsif" ).Or( Terms.Text( "elseif" ) ) ).SkipAnd( LogicalExpression ).AndSkip( TagEnd ).And( _anyTagsListParser ) )
                    .Then( x => x.Select( e => new ElseIfStatement( e.Item1, e.Item2 ) ).ToList() ) )
                .And( ZeroOrOne(
                    CreateTag( "else" ).SkipAnd( _anyTagsListParser ) )
                    .Then( x => x != null ? new ElseStatement( x ) : null ) )
                .AndSkip( CreateTag( "endif" ).ElseError( $"'{{% endif %}}' was expected" ) )
                .Then<Statement>( x => new IfStatement( x.Item1, x.Item2, x.Item4, x.Item3 ) )
                .ElseError( "Invalid 'if' tag" );

            RegisteredTags["if"] = ifTag;
        }

        /// <summary>
        /// Re-implement the {% liquid %} tag to be Lava-friendly.
        /// </summary>
        private void RegisterLavaTag()
        {
            var LiquidTag = Literals.WhiteSpace( true ) // {% liquid %} can start with new lines
                .Then( ( context, x ) => { ( ( FluidParseContext ) context ).InsideLiquidTag = true; return x; } )
                .SkipAnd( OneOrMany( Identifier.Switch( ( context, previous ) =>
                {
                    // Because tags like 'else' are not listed, they won't count in TagsList, and will stop being processed
                    // as inner tags in blocks like {% if %} TagsList {% endif $}
                    var tagName = previous;

                    if ( RegisteredTags.TryGetValue( tagName, out var tag ) )
                    {
                        return tag;
                    }
                    // If the tag name matches a shortcode, return it.
                    // Note that there is some potential for collision between built-in tags and shortcodes,
                    // which may require this parser to be refined further.
                    else if ( RegisteredTags.TryGetValue( tagName + "_", out var shortcodeTag ) )
                    {
                        return shortcodeTag;
                    }
                    else
                    {
                        throw new ParseException( $"Unknown tag '{tagName}' at {context.Scanner.Cursor.Position}" );
                    }
                } ) ) )
                .Then( ( context, x ) => { ( ( FluidParseContext ) context ).InsideLiquidTag = false; return x; } )
                .AndSkip( TagEnd ).Then<Statement>( x => new LiquidStatement( x ) );

            // Register the new tag, and add an alias for the "{% lava %}" tag.
            RegisteredTags["liquid"] = LiquidTag;
            RegisteredTags["lava"] = LiquidTag;
        }

        /// <summary>
        /// Create a parser for the set of tags and shortcodes that have been defined.
        /// </summary>
        /// <param name="throwOnUnknownTag">If true, undefined tags return null rather than throwing an exception.</param>
        /// <returns></returns>
        /// <remarks>The option to ignore unknown tags can be used to ignore output tags that are not defined until the render process.</remarks>
        private Parser<Statement> CreateKnownTagsParser( bool throwOnUnknownTag )
        {
            var parser = OneOf(
                LavaTagParsers.LavaTagStart
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
                LavaTagParsers.LavaShortcodeStart
                    .SkipAnd( Identifier.ElseError( ErrorMessages.IdentifierAfterTagStart )
                        .Switch( ( context, tagName ) =>
                        {
                            var shortcodeTagName = tagName + "_";

                            if ( RegisteredTags.TryGetValue( shortcodeTagName, out var shortcode ) )
                            {
                                return shortcode;
                            }

                            // If we encounter an invalid shortcode, always throw an error - if we don't, the parser will report
                            // a different (and misleading) error caused by the unexpected syntax.
                            // However, we can safely ignore the error inside a {% liquid %} tag because it will be reported
                            // later when the content of the tag is processed.
                            var p = ( FluidParseContext ) context;
                            if ( !p.InsideLiquidTag )
                            {
                                throw new global::Fluid.ParseException( $"Unknown shortcode '{tagName}' at {context.Scanner.Cursor.Position}" );
                            }
                            return null;
                        } )
                    )
                );

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
            string errorMessage;

            if ( format == LavaTagFormatSpecifier.LavaShortcode )
            {
                tokenEndParser = LavaTagParsers.LavaShortcodeEnd;
                tagName = tagName.Substring( 0, tagName.Length - "_".Length );
                errorMessage = $"Invalid '{{[ {tagName} ]}}' shortcode block";
            }
            else
            {
                tokenEndParser = LavaTokenEndParser;
                errorMessage = $"Invalid '{{% {tagName} %}}' block";
            }

            var lavaBlock = AnyCharBefore( tokenEndParser, canBeEmpty: true )
                .AndSkip( tokenEndParser )
                .And( new LavaTagParsers.LavaBlockContentParser( tagName, format ) )
                .Then<Statement>( x => new FluidLavaBlockStatement( this, tagName, format, x.Item1, x.Item2 ) )
                .ElseError( errorMessage );

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
            string errorMessage;

            if ( format == LavaTagFormatSpecifier.LavaShortcode )
            {
                tokenEndParser = LavaTagParsers.LavaShortcodeEnd;
                tagName = tagName.Substring( 0, tagName.Length - "_".Length );
                errorMessage = $"Invalid '{{[ {tagName} ]}}' shortcode tag";
            }
            else
            {
                tokenEndParser = LavaTokenEndParser;
                errorMessage = $"Invalid '{{% {tagName} %}}' tag";
            }

            var lavaTag = AnyCharBefore( tokenEndParser, canBeEmpty: true )
                           .AndSkip( tokenEndParser )
                           .Then<Statement>( x => new FluidLavaTagStatement( tagName, format, x ) )
                           .ElseError( errorMessage );

            this.RegisteredTags[registerTagName] = lavaTag;
        }

        /// <summary>
        /// Parse the supplied template into a collection of tokens that are recognized by the Fluid parser.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static List<string> ParseToTokens( string template )
        {
            return ParseToTokens( template, includeComments: false );
        }

        /// <summary>
        /// Parse the supplied template into a collection of tokens that are recognized by the Fluid parser.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="includeComments">If set to true, comment tokens are included in the output.</param>
        /// <returns></returns>
        public static List<string> ParseToTokens( string template, bool includeComments )
        {
            var tokens = new List<string>();

            var context = new FluidParseContext( template );

            var lavaTokens = LavaTokensListParser.Parse( context );

            if ( !includeComments )
            {
                lavaTokens = lavaTokens.Where( t => t.ElementType != LavaDocumentTokenTypeSpecifier.InlineComment && t.ElementType != LavaDocumentTokenTypeSpecifier.BlockComment ).ToList();
            }

            var lavaTokenStrings = lavaTokens.Select( x => x.ToString() ).ToList();

            // If the template contains only literal text, add the entire content as a single text token.
            if ( !lavaTokenStrings.Any()
                 && !string.IsNullOrEmpty( template ) )
            {
                lavaTokenStrings.Add( template );
            }

            return lavaTokenStrings;
        }

    }

    #region Support Classes

    /* [2021-02-02] DJL
     * These classes have been added to support Lava-specific changes to the Fluid Parser.
     */
    public enum LavaDocumentTokenTypeSpecifier
    {
        // {{ output }}
        Output,
        // {% tag %}, {% endtag %}
        Tag,
        // literal_text
        Literal,
        // {[ shortcode ]}
        Shortcode,
        // Comment in the form: //- comment<eol>
        InlineComment,
        // Comment in the form: /- comment -/
        BlockComment

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
                return "{{" + Content + "}}";
            }
            else if ( ElementType == LavaDocumentTokenTypeSpecifier.Tag )
            {
                return "{%" + Content + "%}";
            }
            else if ( ElementType == LavaDocumentTokenTypeSpecifier.Shortcode )
            {
                return "{[" + Content + "]}";
            }
            else if ( ElementType == LavaDocumentTokenTypeSpecifier.InlineComment )
            {
                return "//-" + Content;
            }
            else if ( ElementType == LavaDocumentTokenTypeSpecifier.BlockComment )
            {
                return "/-" + Content + "-/";
            }

            return Content;
        }
    }

    #endregion
}
