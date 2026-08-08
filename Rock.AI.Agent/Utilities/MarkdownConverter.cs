using System;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;

using CommonMark;
using CommonMark.Syntax;

namespace Rock.AI.Agent.Utilities;

/// <summary>
/// Handles converting Markdown to the different structured formats supported
/// by Rock.
/// </summary>
internal class MarkdownConverter
{
    #region Fields

    /// <summary>
    /// The random number generator used by this converter. This is used anytime
    /// we need to generate a random Id, such as block identifiers.
    /// </summary>
    private readonly Random _random = new();

    #endregion

    #region Methods

    /// <summary>
    /// Converts the markdown string into the Editor.js JSON format.
    /// </summary>
    /// <param name="markdown">The markdown string to convert.</param>
    /// <returns>The Editor.js JSON representation of the markdown.</returns>
    public string ConvertToEditorJs( string markdown )
    {
        var document = ParseDocument( markdown );
        var jsonBlocks = new JsonArray();
        var jsonRoot = new JsonObject
        {
            ["time"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ["version"] = "2.31.0-rc.7",
            ["blocks"] = jsonBlocks,
        };

        if ( markdown.IsNullOrWhiteSpace() )
        {
            return jsonRoot.ToString();
        }

        for ( var block = document.FirstChild; block != null; block = block.NextSibling )
        {
            var editorBlock = ConvertToEditorJsBlock( block );

            jsonBlocks.Add( editorBlock );
        }

        return jsonRoot.ToJsonString();
    }

    /// <summary>
    /// Parses the specified Markdown text and returns the corresponding
    /// CommonMark document block.
    /// </summary>
    /// <param name="markdown">The Markdown-formatted text to parse.</param>
    /// <returns>A Block representing the root of the parsed CommonMark document tree.</returns>
    private Block ParseDocument( string markdown )
    {
        var settings = CommonMarkSettings.Default.Clone();

        // Match the default markdown conversion in Rock.
        settings.RenderSoftLineBreaksAsLineBreaks = false;

        using var markdownReader = new StringReader( markdown );

        var document = CommonMarkConverter.ProcessStage1( markdownReader, settings );

        CommonMarkConverter.ProcessStage2( document, settings );

        return document;
    }

    /// <summary>
    /// Converts a Markdown block to its corresponding Editor.js block
    /// representation as a JSON object.
    /// </summary>
    /// <param name="block">The Markdown block to convert.</param>
    /// <returns>A JsonObject representing the Editor.js block equivalent of the specified Markdown block.</returns>
    private JsonObject ConvertToEditorJsBlock( Block block )
    {
        var blockId = _random.Next().AsIdKey();

        if ( block.Tag == BlockTag.AtxHeading )
        {
            return new JsonObject
            {
                ["id"] = blockId,
                ["type"] = "header",
                ["data"] = new JsonObject
                {
                    ["text"] = ConvertToEditorJsInline( block.InlineContent ),
                    ["level"] = block.Heading.Level,
                },
            };
        }
        else if ( block.Tag == BlockTag.Paragraph )
        {
            return new JsonObject
            {
                ["id"] = blockId,
                ["type"] = "paragraph",
                ["data"] = new JsonObject
                {
                    ["text"] = ConvertToEditorJsInline( block.InlineContent ),
                },
            };
        }
        else if ( block.Tag == BlockTag.List )
        {
            var items = new JsonArray();

            for ( var listItem = block.FirstChild; listItem != null; listItem = listItem.NextSibling )
            {
                items.Add( ConvertToEditorJsListItem( listItem ) );
            }

            return new JsonObject
            {
                ["id"] = blockId,
                ["type"] = "list",
                ["data"] = new JsonObject
                {
                    ["style"] = block.ListData.ListType == ListType.Bullet ? "unordered" : "ordered",
                    ["items"] = items,
                },
            };
        }
        else
        {
            throw new InvalidMarkdownTagException( false, block.Tag.ToString() );
        }
    }

    /// <summary>
    /// Converts the specified inline element to its Editor.js string
    /// representation.
    /// </summary>
    /// <param name="inline">The inline element to convert.</param>
    /// <returns>A string containing the Editor.js representation of the specified inline element.</returns>
    private string ConvertToEditorJsInline( Inline inline )
    {
        var sb = new StringBuilder();

        ConvertToEditorJsInline( inline, sb );

        return sb.ToString();
    }

    /// <summary>
    /// Converts a linked list of inline Markdown elements to their Editor.js
    /// HTML representation and appends the result to the specified StringBuilder.
    /// </summary>
    /// <param name="inline">The first inline element in the linked list to convert.</param>
    /// <param name="sb">The StringBuilder to which the converted HTML content is appended.</param>
    private void ConvertToEditorJsInline( Inline inline, StringBuilder sb )
    {
        for ( var current = inline; current != null; current = current.NextSibling )
        {
            if ( current.Tag == InlineTag.Strong )
            {
                sb.Append( "<b>" );
                ConvertToEditorJsInline( current.FirstChild, sb );
                sb.Append( "</b>" );
            }
            else if ( current.Tag == InlineTag.Emphasis )
            {
                sb.Append( "<i>" );
                ConvertToEditorJsInline( current.FirstChild, sb );
                sb.Append( "</i>" );
            }
            else if ( current.Tag == InlineTag.String )
            {
                sb.Append( current.LiteralContent );
            }
            else
            {
                throw new InvalidMarkdownTagException( true, current.Tag.ToString() );
            }
        }
    }

    /// <summary>
    /// Converts a Markdown list item block to a JSON object compatible with
    /// the Editor.js List Tool format.
    /// </summary>
    /// <param name="block">The block representing a Markdown list item to convert.</param>
    /// <returns>A JsonObject containing the converted list item, formatted for Editor.js.</returns>
    private JsonObject ConvertToEditorJsListItem( Block block )
    {
        if ( block.Tag != BlockTag.ListItem )
        {
            throw new InvalidMarkdownException( $"Expected list item block, but got: {block.Tag}" );
        }

        var items = new JsonArray();
        var item = new JsonObject
        {
            ["items"] = items,
        };

        for ( var child = block.FirstChild; child != null; child = child.NextSibling )
        {
            if ( child.Tag == BlockTag.Paragraph )
            {
                item["content"] = ConvertToEditorJsInline( child.InlineContent );
            }
            else if ( child.Tag == BlockTag.List )
            {
                for ( var listItem = child.FirstChild; listItem != null; listItem = listItem.NextSibling )
                {
                    items.Add( ConvertToEditorJsListItem( listItem ) );
                }
            }
            else
            {
                throw new InvalidMarkdownException( $"Unsupported block type in list item: {child.Tag}" );
            }
        }

        return item;
    }

    #endregion
}
