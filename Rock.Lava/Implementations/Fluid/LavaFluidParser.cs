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
using System.Runtime.CompilerServices;
using System.Text;
using Fluid;
using Fluid.Ast;
using Fluid.Ast.BinaryExpressions;
using Fluid.Tags;
using Fluid.Values;
using Irony.Parsing;
using Microsoft.Extensions.Primitives;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// An extended implementation of the Fluid Liquid parser.
    /// </summary>

    /* [2021-02-02] DJL
     * This code is largely identical to the FluidParser.cs file in release 1.0.0-beta-9660.
     * This implementation extends the parser to capture the source text of tokens as they are processed,
     * which is necessary for Lava to support a framework-independent implementation of custom tags and blocks.
     * This modification should be suggested as a pull request to the Fluid project when it is proved to work correctly.
     */
    internal class FluidParserEx : IFluidParser, IFluidParserEx
    {
        protected bool _isComment; // true when the current block is a comment
        protected bool _isRaw; // true when the current block is raw
        private readonly LanguageData _languageData;
        private readonly Dictionary<string, ITag> _tags;
        private readonly Dictionary<string, ITag> _blocks;
        protected LavaFluidParserContext _context;

        private static IList<AssignStatement> _assignStatements;

        public event EventHandler<FluidElementParseEventArgs> ElementParsed;

        public FluidParserEx( LanguageData languageData, Dictionary<string, ITag> tags, Dictionary<string, ITag> blocks )
        {
            _languageData = languageData;
            _tags = tags;
            _blocks = blocks;
        }

        #region IFluidParser implementation

        public bool TryParse( string template, bool stripEmptyLines, out List<Statement> result, out IEnumerable<string> errors )
        {
            errors = Array.Empty<string>();
            var segment = new StringSegment( template );
            Parser parser = null;
            _context = new LavaFluidParserContext();

            result = _context.CurrentBlock.Statements;

            var contextBlocksPropertyInfo = _context.GetType().GetProperty( "_blocks", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
            var contextBlocksInstance = contextBlocksPropertyInfo.GetValue( _context );
            var contextBlocksCountPropertyInfo = contextBlocksInstance.GetType().GetProperty( "Count" );

            try
            {
                bool trimBefore = false;
                bool trimAfter = false;

                int previous = 0, index = 0;
                Statement s;

                while ( true )
                {
                    previous = index;

                    if ( !MatchTag( segment, index, out var start, out var end ) )
                    {
                        index = segment.Length;

                        if ( index != previous )
                        {
                            // Consume last Text statement
                            ConsumeTextStatement( segment, previous, index, trimAfter, false, stripEmptyLines );
                        }

                        break;
                    }
                    else
                    {
                        trimBefore = segment.Index( start + 2 ) == '-';

                        // Only create a parser if there are tags in the template
                        if ( parser == null )
                        {
                            parser = new Parser( _languageData );
                        }

                        if ( start != previous )
                        {
                            // Consume current Text statement
                            if ( _context.CurrentBlock?.Tag?.Term?.Name == "case" )
                            {
                                // Ignore whitespace inside a case block.
                                ConsumeTextStatement( segment, previous, start, trimAfter, trimBefore, stripEmptyLines: true );
                            }
                            else
                            {
                                ConsumeTextStatement( segment, previous, start, trimAfter, trimBefore, stripEmptyLines );
                            }
                        }

                        trimAfter = segment.Index( end - 2 ) == '-';

                        var tag = segment.Subsegment( start, end - start + 1 ).ToString();

                        if ( trimAfter || trimBefore )
                        {
                            // Remove the dashes for the parser

                            StringBuilder sb = new StringBuilder( tag );

                            if ( trimBefore )
                            {
                                sb[2] = ' ';
                            }

                            if ( trimAfter )
                            {
                                sb[end - start - 2] = ' ';
                            }

                            tag = sb.ToString();
                        }

                        var tree = parser.Parse( tag );

                        if ( tree.HasErrors() )
                        {
                            int line = 1, col = 1;

                            for ( var i = segment.Offset; i < start; i++ )
                            {
                                var ch = segment.Index( i );

                                switch ( ch )
                                {
                                    case '\n':
                                        line++;
                                        col = 1;
                                        break;
                                    case '\r':
                                        // Ignore
                                        break;
                                    default:
                                        col++;
                                        break;
                                }
                            }
                            errors = tree
                                .ParserMessages
                                .Select( x => $"{x.Message} at line:{line + x.Location.Line}, col:{col + x.Location.Column}" )
                                .ToArray();

                            return false;
                        }

                        switch ( tree.Root.Term.Name )
                        {
                            case "output":
                                s = BuildOutputStatement( tree.Root );
                                break;

                            case "tag":
                                s = BuildTagStatement( tree.Root, segment, start, end );
                                break;

                            default:
                                s = null;
                                break;
                        }

                        index = end + 1;

                        // Entered a comment block?
                        if ( _isComment )
                        {
                            s = new CommentStatement( ConsumeTag( segment, end + 1, "endcomment", out end ) );
                            index = end;
                        }

                        // Entered a raw block?
                        if ( _isRaw )
                        {
                            s = new TextStatement( ConsumeTag( segment, end + 1, "endraw", out end ) );
                            index = end;
                        }

                        if ( s != null )
                        {
                            _context.CurrentBlock.AddStatement( s );
                        }

                        if ( ElementParsed != null )
                        {
                            var elementId = GetOrAssignNodeId( tree?.Root );

                            ElementParsed.Invoke( this,
                                new FluidElementParseEventArgs { ElementId = elementId, ElementText = tag, StartIndex = start, EndIndex = end, Statement = s } );
                        }
                    }
                }

                // Make sure we aren't still in a block
                var contextBlocksCount = (int)contextBlocksCountPropertyInfo.GetValue( contextBlocksInstance );

                if ( contextBlocksCount > 0 )
                {
                    throw ( new ParseException( $"Expected end of block: {_context.CurrentBlock.Tag}" ) );
                }
                return true;
            }
            catch ( ParseException e )
            {
                errors = new[] { e.Message };
            }

            return false;
        }

        #endregion

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void ConsumeTextStatement( StringSegment segment, int start, int end, bool trimStart, bool trimEnd, bool stripEmptyLines )
        {
            var textStatement = CreateTextStatement( segment, start, end, trimStart, trimEnd, stripEmptyLines );

            if ( textStatement != null )
            {
                _context.CurrentBlock.AddStatement( textStatement );
            }

            if ( ElementParsed != null )
            {
                var tag = _context?.CurrentBlock?.Tag;

                var elementId = GetOrAssignNodeId( tag );

                ElementParsed.Invoke( this,
                    new FluidElementParseEventArgs { ElementId = elementId, ElementText = segment.Substring( start, end - start ), StartIndex = start, EndIndex = end, Statement = textStatement } );
            }
        }

        /// <summary>
        /// Creates a <see cref="TextStatement"/> by reading the text until the specific end tag is found,
        /// or the end of the segment reached.
        /// </summary>
        private StringSegment ConsumeTag( StringSegment segment, int start, string endTag, out int end )
        {
            int index = start;

            while ( index < segment.Length - 1 )
            {
                var pos = index;

                if ( segment.Index( index ) == '{' && segment.Index( index + 1 ) == '%' )
                {
                    var tagStart = index;

                    index = index + 2;

                    while ( index < segment.Length && ( Char.IsWhiteSpace( segment.Index( index ) ) || segment.Index( index ) == '-' ) )
                    {
                        index++;
                    }

                    if ( index + endTag.Length < segment.Length && segment.Substring( index, endTag.Length ) == endTag )
                    {
                        end = pos;
                        return segment.Subsegment( start, tagStart - start );
                    }
                    else
                    {
                        index++;
                    }
                }
                else
                {
                    index++;
                }
            }

            // We reached the end of the segment without finding the matched tag.
            // Ideally we could return a parsing error, right now we just return the text.
            throw new ParseException( $"Expected '{endTag}'" );
        }

        /// <summary>
        /// Returns a <see cref="TextStatement"/> where the extra whitespace is stripped 
        /// for a Tag that is the only content on a line
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private TextStatement CreateTextStatement( StringSegment segment, int start, int end, bool trimStart, bool trimEnd, bool stripEmptyLines )
        {
            int index;

            var endIsPercent = end < segment.Length - 1 && segment.Index( end + 1 ) == '%';
            var startIsPercent = start > 2 && segment.Index( start - 2 ) == '%';

            if ( trimEnd )
            {
                index = end - 1;

                // There is a tag after, we can try to strip the end of the section
                while ( true )
                {
                    // We strip the text if all chars down to the begining of the line
                    // are white spaces.
                    if ( index == start - 1 )
                    {
                        if ( index >= 0 && segment.Index( index ) == '\n' || index == -1 )
                        {
                            end = start;
                        }

                        break;
                    }

                    var c = segment.Index( index );

                    if ( c == '\n' )
                    {
                        // Beginning of line, we can strip
                        end = index + 1;
                        break;
                    }

                    if ( !Char.IsWhiteSpace( c ) )
                    {
                        end = index + 1;

                        // This is not just whitespace
                        break;
                    }

                    index--;
                }
            }

            if ( trimStart )
            {
                index = start;

                // There is a tag before, we can try to strip the beginning of the section
                while ( true )
                {
                    // Reach end of section?
                    if ( index == end )
                    {
                        start = end;
                        break;
                    }

                    var c = segment.Index( index );

                    if ( c == '\n' && index + 1 <= end )
                    {
                        // End of line, we can strip
                        start = index + 1;
                        break;
                    }

                    if ( c == '\r' && index + 2 < end && segment.Index( index + 1 ) == '\n' )
                    {
                        start = index + 2;
                        break;
                    }

                    if ( !Char.IsWhiteSpace( c ) )
                    {
                        start = index;

                        // This is not just whitespace
                        break;
                    }

                    index++;
                }
            }

            // Did the text get completely removed?
            if ( end == start )
            {
                return null;
            }

            if ( stripEmptyLines )
            {
                if ( startIsPercent && endIsPercent )
                {
                    // Remove all whitespace between two statements

                    bool hasNonWhitespace = false;
                    for ( var i = start; i < end; i++ )
                    {
                        var c = segment.Index( i );

                        if ( !Char.IsWhiteSpace( c ) )
                        {
                            hasNonWhitespace = true;
                            break;
                        }
                    }

                    if ( !hasNonWhitespace )
                    {
                        end = start;
                    }
                }

                if ( startIsPercent )
                {
                    for ( var i = start; i < end; i++ )
                    {
                        var c = segment.Index( i );

                        if ( !Char.IsWhiteSpace( c ) )
                        {
                            break;
                        }

                        if ( c == '\n' || i == segment.Length - 1 )
                        {
                            start = i + 1;
                            break;
                        }
                    }
                }

                if ( endIsPercent )
                {
                    for ( var i = end - 1; i >= start; i-- )
                    {
                        var c = segment.Index( i );

                        if ( !Char.IsWhiteSpace( c ) )
                        {
                            break;
                        }

                        if ( c == '\n' )
                        {
                            end = i + 1;
                            break;
                        }

                        if ( i == 0 )
                        {
                            end = i;
                            break;
                        }
                    }
                }
            }

            // Did the text get completely removed?
            if ( end == start )
            {
                return null;
            }

            return new TextStatement( segment.Subsegment( start, end - start ) );
        }

        private bool MatchTag( StringSegment template, int startIndex, out int start, out int end )
        {
            start = -1;
            end = -1;

            while ( startIndex < template.Length )
            {
                start = template.IndexOf( '{', startIndex );

                // No match
                if ( start == -1 )
                {
                    end = -1;
                    return false;
                }

                if ( start < template.Length - 1 )
                {
                    var c = template.Index( start + 1 );

                    if ( ( c == '{' && !( _isComment || _isRaw ) ) || c == '%' )
                    {
                        // Start tag found
                        var endTag = c == '{' ? '}' : '%';

                        var from = start + 2;
                        do
                        {
                            end = template.IndexOf( endTag, from );
                            from = end + 1;

                        } while ( end != -1 && end < template.Length - 1 && template.Index( end + 1 ) != '}' );

                        if ( end == -1 || end >= template.Length - 1 )
                        {
                            // No end tag
                            return false;
                        }
                        else
                        {
                            // Found a match
                            end = end + 1;
                            return true;
                        }
                    }
                    else
                    {
                        // Was not a start tag, look further
                        startIndex = start + 1;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        #region Build methods

        private string GetOrAssignNodeId( ParseTreeNode node )
        {
            if ( node != null )
            {
                if ( node.Tag == null )
                {
                    node.Tag = Guid.NewGuid().ToString();
                }

                return node.Tag as string;
            }

            return null;
        }

        public virtual Statement BuildTagStatement( ParseTreeNode node, StringSegment segment, int start, int end )
        {
            var tag = node.ChildNodes[0];

            GetOrAssignNodeId( tag );

            var tagText = segment.Substring( start, end - start + 1 );

            switch ( tag.Term.Name )
            {
                case "for":
                    _context.EnterBlock( tag, segment, start, end );
                    break;

                case "endfor":
                    var forStatement = BuildForStatement( _context.CurrentBlock );
                    _context.ExitBlock();
                    return forStatement;

                case "case":
                    _context.EnterBlock( tag, segment, start, end );
                    break;

                case "when":
                    EnterWhenSection( tag );
                    break;

                case "endcase":
                    var caseStatement = BuildCaseStatement( _context.CurrentBlock );
                    _context.ExitBlock();
                    return caseStatement;

                case "if":
                    _context.EnterBlock( tag, segment, start, end );
                    break;

                case "unless":
                    _context.EnterBlock( tag, segment, start, end );
                    break;

                case "endif":
                    var ifStatement = BuildIfStatement( _context.CurrentBlock );
                    _context.ExitBlock();
                    return ifStatement;

                case "endunless":
                    var unlessStatement = BuildUnlessStatement( _context.CurrentBlock );
                    _context.ExitBlock();
                    return unlessStatement;

                case "else":
                    EnterElseSection();
                    break;

                case "elsif":
                    EnterElsifSection( tag );
                    break;

                case "break":
                    return new BreakStatement();

                case "continue":
                    return new ContinueStatement();

                case "comment":
                    _isComment = true;
                    break;

                case "endcomment":

                    if ( !_isComment )
                    {
                        throw new ParseException( $"Expected tag 'comment' before 'endcomment'" );
                    }

                    _isComment = false;
                    break;

                case "raw":
                    _isRaw = true;
                    break;

                case "endraw":

                    if ( !_isRaw )
                    {
                        throw new ParseException( $"Expected tag 'raw' before 'endraw'" );
                    }

                    _isRaw = false;
                    break;

                case "cycle":
                    return BuildCycleStatement( tag );

                case "assign":
                    return BuildAssignStatement( tag );

                case "increment":
                    return BuildIncrementStatement( tag );

                case "decrement":
                    return BuildDecrementStatement( tag );

                case "capture":
                    _context.EnterBlock( tag, segment, start, end );
                    break;

                case "endcapture":
                    var captureStatement = BuildCaptureStatement( _context.CurrentBlock );
                    _context.ExitBlock();
                    return captureStatement;

                case "include":
                    return BuildIncludeStatement( tag );

                default:

                    if ( tag.Term.Name.StartsWith( "end", StringComparison.Ordinal ) )
                    {
                        var tagName = tag.Term.Name.Substring( 3 );

                        if ( _blocks.TryGetValue( tagName, out ITag customEndBlock ) )
                        {
                            if ( _context?.CurrentBlock?.Tag?.Term?.Name != tagName )
                            {
                                throw new ParseException( $"Unexpected tag: '{tag.Term.Name}' not matching '{_context?.CurrentBlock?.Tag?.Term?.Name}' tag." );
                            }

                            var statement = CreateStatementForCustomBlock( _context.CurrentBlock.Tag, customEndBlock, segment, start, end );

                            _context.ExitBlock();
                            return statement;
                        }
                    }

                    if ( _tags.TryGetValue( tag.Term.Name, out ITag customTag ) )
                    {
                        return CreateStatementForCustomTag( tag, customTag, segment, start, end );
                    }

                    if ( _blocks.TryGetValue( tag.Term.Name, out ITag customBlock ) )
                    {
                        _context.EnterBlock( tag, segment, start, end );
                    }

                    break;
            }

            return null;
        }

        private List<FluidParsedTemplateElement> _tokens = new List<FluidParsedTemplateElement>();

        private Statement CreateStatementForCustomTag( ParseTreeNode node, ITag customTag, StringSegment segment, int tagStartPosition, int tagEndPosition )
        {
            if ( customTag is ITagEx blockEx )
            {
                _context.CurrentBlock.AdditionalData = new BlockInfo();

                var blockData = _context.CurrentBlock.AdditionalData;

                blockData.StartPosition = tagStartPosition;

                blockData.OpenTag = segment.Substring( tagStartPosition, tagEndPosition - tagStartPosition + 1 );

                blockData.CloseTag = blockData.OpenTag;

                blockData.EndPosition = tagEndPosition;

                return blockEx.Parse( node, _context );
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        private Statement CreateStatementForCustomBlock( ParseTreeNode node, ITag customEndBlock, StringSegment segment, int closingTagStartPosition, int closingTagEndPosition )
        {
            //Statement statement = null;

            if ( customEndBlock is ITagEx blockEx )
            {
                //if ( _context.CurrentBlock.AdditionalData == null )
                //{
                //    // This is an inline tag.
                //    _context.CurrentBlock.AdditionalData = new BlockInfo();
                //}

                var blockData = _context.CurrentBlock.AdditionalData;

                blockData.EndPosition = closingTagStartPosition - 1;

                blockData.CloseTag = segment.Substring( closingTagStartPosition, closingTagEndPosition - closingTagStartPosition + 1 );

                var startInnerText = blockData.StartPosition + blockData.OpenTag.Length;

                blockData.InnerText = segment.Substring( startInnerText, closingTagStartPosition - startInnerText );

                return blockEx.Parse( node, _context );
            }
            else
            {
                throw new NotImplementedException();
                //statement = customEndBlock.Parse( _context.CurrentBlock.Tag, _context as ParserContext );
            }

        }
        public static CaptureStatement BuildCaptureStatement( BlockContext context )
        {
            if ( context.Tag == null )
            {
                throw new ParseException( $"Expected tag 'capture' before 'endcapture'" );
            }

            if ( context.Tag.Term.Name != "capture" )
            {
                throw new ParseException( $"Unexpected tag: '{context.Tag.Term.Name}' not matching 'capture' tag." );
            }

            var identifier = context.Tag.ChildNodes[0].Token.ValueString;

            var captureStatement = new CaptureStatement( identifier, context.Statements );

            return captureStatement;
        }

        public static AssignStatement BuildAssignStatement( ParseTreeNode tag )
        {
            var identifier = tag.ChildNodes[0].Token.ValueString;
            var value = BuildExpression( tag.ChildNodes[1] );

            return new AssignStatement( identifier, value );
        }

        public static IncrementStatement BuildIncrementStatement( ParseTreeNode tag )
        {
            var identifier = tag.ChildNodes[0].Token.ValueString;

            return new IncrementStatement( identifier );
        }

        public static DecrementStatement BuildDecrementStatement( ParseTreeNode tag )
        {
            var identifier = tag.ChildNodes[0].Token.ValueString;

            return new DecrementStatement( identifier );
        }

        public static IncludeStatement BuildIncludeStatement( ParseTreeNode tag )
        {
            var pathExpression = BuildTermExpression( tag.ChildNodes[0] );
            Expression withExpression = null;

            if ( tag.ChildNodes.Count == 2 )
            {
                withExpression = BuildTermExpression( tag.ChildNodes[1] );
                return new IncludeStatement( pathExpression, with: withExpression );
            }

            if ( tag.ChildNodes.Count >= 3 )
            {
                _assignStatements = new List<AssignStatement>();
                Traverse( tag.ChildNodes[2] );
                return new IncludeStatement( pathExpression, assignStatements: _assignStatements );
            }

            return new IncludeStatement( pathExpression );
        }

        public static CycleStatement BuildCycleStatement( ParseTreeNode tag )
        {
            Expression group = null;
            IList<Expression> values;

            if ( tag.ChildNodes[0].Term.Name == "cycleArguments" )
            {
                // No group name
                values = tag.ChildNodes[0].ChildNodes.Select( BuildTermExpression ).ToArray();
            }
            else
            {
                group = BuildTermExpression( tag.ChildNodes[0] );
                values = tag.ChildNodes[1].ChildNodes.Select( BuildTermExpression ).ToArray();
            }

            return new CycleStatement( group, values );
        }

        public void EnterWhenSection( ParseTreeNode tag )
        {
            var options = tag.ChildNodes[0].ChildNodes.Select( BuildTermExpression ).ToList();
            _context.EnterBlockSection( "when", new WhenStatement( options, new List<Statement>() ) );
        }

        public void EnterElseSection()
        {
            _context.EnterBlockSection( "else", new ElseStatement( new List<Statement>() ) );
        }

        public void EnterElsifSection( ParseTreeNode tag )
        {
            _context.EnterBlockSection( "elsif", new ElseIfStatement( BuildExpression( tag.ChildNodes[0] ), new List<Statement>() ) );
        }

        public virtual OutputStatement BuildOutputStatement( ParseTreeNode node )
        {
            var expressionNode = node.ChildNodes[0];

            var expression = BuildExpression( expressionNode );

            return new OutputStatement( expression );
        }

        public static IfStatement BuildIfStatement( BlockContext context )
        {
            if ( context.Tag == null )
            {
                throw new ParseException( $"Expected tag 'if' before 'endif'" );
            }

            if ( context.Tag.Term.Name != "if" )
            {
                throw new ParseException( $"Unexpected tag: '{context.Tag.Term.Name}' not matching 'if' tag." );
            }

            var elseStatements = context.GetBlockStatements<ElseStatement>( "else" );
            var elseIfStatements = context.GetBlockStatements<ElseIfStatement>( "elsif" );

            var ifStatement = new IfStatement(
                BuildExpression( context.Tag.ChildNodes[0] ),
                context.Statements,
                elseStatements.FirstOrDefault(),
                elseIfStatements
                );

            return ifStatement;
        }

        public static CaseStatement BuildCaseStatement( BlockContext context )
        {
            if ( context.Tag == null )
            {
                throw new ParseException( $"Expected tag 'case' before 'endcase'" );
            }

            if ( context.Tag.Term.Name != "case" )
            {
                throw new ParseException( $"Unexpected tag: '{context.Tag.Term.Name}' not matching 'case' tag." );
            }

            if ( context.Statements.Count > 0 && context.Statements.Any( x => x is TextStatement text ) )
            {
                throw new ParseException( $"Unexpected content in 'case' tag. Only 'when' and 'else' are allowed." );
            }

            var elseStatements = context.GetBlockStatements<ElseStatement>( "else" );
            var whenStatements = context.GetBlockStatements<WhenStatement>( "when" );

            var caseStatement = new CaseStatement(
                BuildExpression( context.Tag.ChildNodes[0] ),
                elseStatements.FirstOrDefault(),
                whenStatements
                );

            return caseStatement;
        }

        public virtual UnlessStatement BuildUnlessStatement( BlockContext context )
        {
            if ( context.Tag == null )
            {
                throw new ParseException( $"Expected tag 'unless' before 'endunless'" );
            }

            if ( context.Tag.Term.Name != "unless" )
            {
                throw new ParseException( $"Unexpected tag: '{context.Tag.Term.Name}' not matching 'unless' tag." );
            }

            var elseStatements = context.GetBlockStatements<ElseStatement>( "else" );
            var elseIfStatements = context.GetBlockStatements<ElseIfStatement>( "elsif" );

            if ( elseStatements.Count > 0 )
            {
                throw new ParseException( $"Unexpected tag 'else' in 'unless'." );
            }

            if ( elseIfStatements.Count > 0 )
            {
                throw new ParseException( $"Unexpected tag 'elsif' in 'unless'." );
            }

            var unlessStatement = new UnlessStatement(
                BuildExpression( _context.CurrentBlock.Tag.ChildNodes[0] ),
                _context.CurrentBlock.Statements
                );

            return unlessStatement;
        }

        public static Statement BuildForStatement( BlockContext context )
        {
            if ( context.Tag == null )
            {
                throw new ParseException( $"Expected tag 'for' before 'endfor'" );
            }

            if ( context.Tag.Term.Name != "for" )
            {
                throw new ParseException( $"Unexpected tag: '{context.Tag.Term.Name}' not matching 'for' tag." );
            }

            var identifier = context.Tag.ChildNodes[0].Token.Text;
            var source = context.Tag.ChildNodes[1];

            Expression limit = null;
            Expression offset = null;
            var reversed = false;

            var elseStatements = context.GetBlockStatements<ElseStatement>( "else" );


            // Options?
            if ( context.Tag.ChildNodes.Count > 2 )
            {
                foreach ( var option in context.Tag.ChildNodes[2].ChildNodes )
                {
                    switch ( option.Term.Name )
                    {
                        case "limit":
                            limit = BuildExpression( option );
                            break;
                        case "offset":
                            offset = BuildExpression( option );
                            break;
                        case "reversed":
                            reversed = true;
                            break;
                    }
                }
            }

            ForStatement forStatement;

            switch ( source.Term.Name )
            {
                case "memberAccess":
                    forStatement = new ForStatement(
                        context.Statements,
                        identifier,
                        BuildMemberExpression( source ),
                        limit,
                        offset,
                        reversed,
                        elseStatements.FirstOrDefault() );
                    break;

                case "range":
                    forStatement = new ForStatement(
                        context.Statements,
                        identifier,
                        BuildRangeExpression( source ),
                        limit,
                        offset,
                        reversed,
                        elseStatements.FirstOrDefault() );
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return forStatement;
        }

        public static RangeExpression BuildRangeExpression( ParseTreeNode node )
        {
            var from = BuildRangePart( node.ChildNodes[0] );
            var to = BuildRangePart( node.ChildNodes[1] );

            return new RangeExpression( from, to );
        }

        /// <summary>
        /// Parses either a Number or a MemberAccess
        /// </summary>
        public static Expression BuildRangePart( ParseTreeNode node )
        {
            switch ( node.Term.Name )
            {
                case "number":
                    return BuildLiteralExpression( node );

                case "memberAccess":
                    return BuildMemberExpression( node );

                default:
                    throw new ParseException( "Expected either a number or a member at: " + node.Token.Location );
            }
        }

        public static Expression BuildExpression( ParseTreeNode node )
        {
            var child = node.ChildNodes[0];

            switch ( child.Term.Name )
            {
                case "binaryExpression":
                    return BuildBinaryExpression( child );

                default:
                    var term = BuildTermExpression( node.ChildNodes[0] );

                    // Filters ?
                    if ( node.ChildNodes.Count > 1 )
                    {
                        return BuildFilterExpression( term, node.ChildNodes[1] );
                    }
                    else
                    {
                        return term;
                    }
            }
        }

        public static Expression BuildTermExpression( ParseTreeNode node )
        {
            if ( node.Term.Name == "memberAccess" )
            {
                return BuildMemberExpression( node );
            }
            else
            {
                return BuildLiteralExpression( node );
            }
        }

        public static Expression BuildBinaryExpression( ParseTreeNode node )
        {
            var left = BuildExpression( node.ChildNodes[0] );
            var op = node.ChildNodes[1].Term.Name;
            var right = BuildExpression( node.ChildNodes[2] );

            switch ( op )
            {
                case "==":
                    return new EqualBinaryExpression( left, right );
                case "<>":
                case "!=":
                    return new NotEqualBinaryExpression( left, right );
                case "+":
                    return new AddBinaryExpression( left, right );
                case "-":
                    return new SubstractBinaryExpression( left, right );
                case "*":
                    return new MultiplyBinaryExpression( left, right );
                case "/":
                    return new DivideBinaryExpression( left, right );
                case "%":
                    return new ModuloBinaryExpression( left, right );
                case ">":
                    return new GreaterThanBinaryExpression( left, right, true );
                case "<":
                    return new LowerThanExpression( left, right, true );
                case ">=":
                    return new GreaterThanBinaryExpression( left, right, false );
                case "<=":
                    return new LowerThanExpression( left, right, false );
                case "contains":
                    return new ContainsBinaryExpression( left, right );
                case "and":
                    return new AndBinaryExpression( left, right );
                case "or":
                    return new OrBinaryExpression( left, right );
            }

            return null;
        }

        public static MemberExpression BuildMemberExpression( ParseTreeNode node )
        {
            var identifierNode = node.ChildNodes[0];
            var segmentNodes = node.ChildNodes[1].ChildNodes;

            var segments = new MemberSegment[segmentNodes.Count + 1];
            segments[0] = new IdentifierSegment( identifierNode.Token.Text );

            for ( var i = 0; i < segmentNodes.Count; i++ )
            {
                var segmentNode = segmentNodes[i];
                segments[i + 1] = BuildMemberSegment( segmentNode );
            }

            return new MemberExpression( segments );
        }

        public static MemberSegment BuildMemberSegment( ParseTreeNode node )
        {
            var child = node.ChildNodes[0];

            switch ( child.Term.Name )
            {
                case "memberAccessSegmentIdentifier":
                    return new IdentifierSegment( child.ChildNodes[0].Token.Text );
                case "memberAccessSegmentIndexer":
                    return new IndexerSegment( BuildExpression( child.ChildNodes[0] ) );
                default:
                    throw new ParseException( "Unknown expression type: " + node.Term.Name );
            }
        }

        public static LiteralExpression BuildLiteralExpression( ParseTreeNode node )
        {
            switch ( node.Term.Name )
            {
                case "string1":
                case "string2":
                    return new LiteralExpression( new StringValue( node.Token.ValueString ) );

                case "number":
                    // We know it's a decimal as it's configured in the grammar
                    return new LiteralExpression( NumberValue.Create( (decimal)node.Token.Value ) );

                case "boolean":
                    if ( !bool.TryParse( node.ChildNodes[0].Token.Text, out var boolean ) )
                    {
                        throw new ParseException( "Invalid boolean: " + node.Token.Text );
                    }
                    return new LiteralExpression( BooleanValue.Create( boolean ) );

                default:
                    throw new ParseException( "Unknown literal expression: " + node.Term.Name );
            }
        }

        public static Expression BuildFilterExpression( Expression input, ParseTreeNode node )
        {
            Expression outer = input;

            // From last to first filter 
            var length = node.ChildNodes.Count;

            for ( var i = 0; i < length; i++ )
            {
                var filterNode = node.ChildNodes[i];
                var identifier = filterNode.ChildNodes[0].Token.Text;

                var arguments = Array.Empty<FilterArgument>();

                if ( filterNode.ChildNodes.Count > 1 )
                {
                    var nodes = filterNode.ChildNodes[1].ChildNodes;
                    var nodesLength = nodes.Count;
                    var argumentsList = new List<FilterArgument>( nodesLength );

                    for ( var k = 0; k < nodesLength; k++ )
                    {
                        argumentsList.Add( BuildFilterArgument( nodes[k] ) );
                    }

                    arguments = argumentsList.ToArray();
                }

                outer = new FilterExpression( outer, identifier, arguments );
            }

            return outer;
        }

        public static FilterArgument BuildFilterArgument( ParseTreeNode node )
        {
            string identifier = null;
            Expression term = null;

            if ( String.Equals( node.ChildNodes[0].Term.Name, "identifier", StringComparison.Ordinal ) )
            {
                identifier = node.ChildNodes[0].Token.ValueString;
                term = BuildTermExpression( node.ChildNodes[1] );
            }
            else
            {
                term = BuildTermExpression( node.ChildNodes[0] );
            }

            return new FilterArgument( identifier, term );
        }

        private static AssignStatement BuildIncludeAssignStatement( ParseTreeNode tag )
        {
            var identifier = tag.ChildNodes[0].Token.ValueString;
            var value = BuildTermExpression( tag.ChildNodes[1] );

            return new AssignStatement( identifier, value );
        }

        #endregion

        private static void Traverse( ParseTreeNode tag )
        {
            if ( tag.ChildNodes.Count == 1 )
            {
                _assignStatements.Add( BuildIncludeAssignStatement( tag.ChildNodes[0] ) );
            }
            else
            {
                Traverse( tag.ChildNodes[0] );
                _assignStatements.Add( BuildIncludeAssignStatement( tag.ChildNodes[2] ) );
            }
        }
    }


    internal static class ExceptionHelper
    {
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static void ThrowArgumentNullException( string paramName )
        {
            ThrowArgumentNullException<object>( paramName );
        }

        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T ThrowArgumentNullException<T>( string paramName )
        {
            throw new ArgumentNullException( paramName );
        }

        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T ThrowParseException<T>( string message )
        {
            throw new ParseException( message );
        }
    }

    #region Support Classes

    /* [2021-02-02] DJL
     * These classes have been added to support Lava-specific changes to the Fluid Parser.
     * 
     */

    /// <summary>
    /// An extension of Fluid.BlockContext that allows storing additional context information.
    /// </summary>
    internal class BlockContextEx : BlockContext
    {
        public BlockContextEx( ParseTreeNode tag )
            : base( tag )
        {
            //
        }

        /// <summary>
        /// A property to store extended information about the block context.
        /// </summary>
        public BlockInfo AdditionalData { get; set; }
    }

    /// <summary>
    /// An element from a Fluid template that has been parsed.
    /// </summary>
    internal class FluidParsedTemplateElement
    {
        public string ElementId { get; set; }
        public Statement Statement { get; set; }
        public string Node { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

    /// <summary>
    /// Information about a parsing event generated by the Fluid Parser.
    /// </summary>
    internal class FluidElementParseEventArgs : EventArgs
    {
        public string ElementId { get; set; }
        public string ElementText { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public Statement Statement { get; set; }
    }

    /// <summary>
    /// An extension of the IFluisParser interface to add parsing events.
    /// </summary>
    internal interface IFluidParserEx : IFluidParser
    {
        event EventHandler<FluidElementParseEventArgs> ElementParsed;
    }

    /// <summary>
    /// Stores metadata for a block element that is parsed from a source document.
    /// </summary>
    internal class BlockInfo
    {
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string SourceText { get; set; }
        public string OpenTag { get; set; }
        public string InnerText { get; set; }
        public string CloseTag { get; set; }
    }

    /// <summary>
    /// An extension of the Fluid.ITag interface to add additional parsing using the Lava parser for Fluid.
    /// </summary>
    internal interface ITagEx : ITag
    {
        Statement Parse( ParseTreeNode node, LavaFluidParserContext context );
    }

    #endregion
}
