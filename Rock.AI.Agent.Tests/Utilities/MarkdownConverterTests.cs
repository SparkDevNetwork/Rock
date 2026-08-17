using System.Text.Json.Nodes;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Utilities;

namespace Rock.AI.Agent.Tests.Utilities;

[TestClass]
public class MarkdownConverterTests
{
    [TestMethod]
    public void UnsupportedInlineTag_ThrowsInvalidMarkdownTagException()
    {
        var markdown = "This is [Link](http://a.com) text.";

        var converter = new MarkdownConverter();

        var exception = Assert.ThrowsExactly<InvalidMarkdownTagException>( () => converter.ConvertToEditorJs( markdown ) );

        Assert.IsTrue( exception.IsInlineTag );
    }

    [TestMethod]
    public void UnsupportedBlockTag_ThrowsInvalidMarkdownTagException()
    {
        var markdown = "```\nCode\n```";

        var converter = new MarkdownConverter();

        var exception = Assert.ThrowsExactly<InvalidMarkdownTagException>( () => converter.ConvertToEditorJs( markdown ) );

        Assert.IsTrue( exception.IsBlockTag );
    }

    [TestMethod]
    public void EmptyString_ReturnsEmptyBlockSet()
    {
        var markdown = string.Empty;

        var converter = new MarkdownConverter();

        var json = converter.ConvertToEditorJs( markdown );

        var actualBlocks = JsonNode.Parse( json )["blocks"].AsArray();

        Assert.IsEmpty( actualBlocks );
    }

    [TestMethod]
    public void InlineBoldText_ReturnsMarkupInParagraph()
    {
        var markdown = "This is **bold** text.";
        var expectedBlocksJson = """
            [
                {
                  "id": "XRL-mgPOlP",
                  "type": "paragraph",
                  "data": {
                    "text": "This is <b>bold</b> text."
                  }
                }
            ]
            """;

        var converter = new MarkdownConverter();
        var json = converter.ConvertToEditorJs( markdown );

        var expectedBlocks = JsonNode.Parse( expectedBlocksJson ).AsArray();
        var actualBlocks = JsonNode.Parse( json )["blocks"].AsArray();

        AssertEqualArrays( expectedBlocks, actualBlocks );
    }

    [TestMethod]
    public void InlineItalicText_ReturnsMarkupInParagraph()
    {
        var markdown = "This is *italic* text.";
        var expectedBlocksJson = """
            [
                {
                  "id": "XRL-mgPOlP",
                  "type": "paragraph",
                  "data": {
                    "text": "This is <i>italic</i> text."
                  }
                }
            ]
            """;

        var converter = new MarkdownConverter();
        var json = converter.ConvertToEditorJs( markdown );

        var expectedBlocks = JsonNode.Parse( expectedBlocksJson ).AsArray();
        var actualBlocks = JsonNode.Parse( json )["blocks"].AsArray();

        AssertEqualArrays( expectedBlocks, actualBlocks );
    }

    [TestMethod]
    public void HeadingLevel1_ReturnsHeadingBlock()
    {
        var markdown = "# Heading 1";
        var expectedBlocksJson = """
            [
                {
                    "id": "XRL-mgPOlP",
                    "type": "header",
                    "data": {
                        "text": "Heading 1",
                        "level": 1
                    }
                }
            ]
            """;

        var converter = new MarkdownConverter();
        var json = converter.ConvertToEditorJs( markdown );


        var expectedBlocks = JsonNode.Parse( expectedBlocksJson ).AsArray();
        var actualBlocks = JsonNode.Parse( json )["blocks"].AsArray();

        AssertEqualArrays( expectedBlocks, actualBlocks );
    }

    [TestMethod]
    public void HeadingLevel2_ReturnsHeadingBlock()
    {
        var markdown = "## Heading 2";
        var expectedBlocksJson = """
            [
                {
                    "id": "XRL-mgPOlP",
                    "type": "header",
                    "data": {
                        "text": "Heading 2",
                        "level": 2
                    }
                }
            ]
            """;

        var converter = new MarkdownConverter();
        var json = converter.ConvertToEditorJs( markdown );


        var expectedBlocks = JsonNode.Parse( expectedBlocksJson ).AsArray();
        var actualBlocks = JsonNode.Parse( json )["blocks"].AsArray();

        AssertEqualArrays( expectedBlocks, actualBlocks );
    }

    [TestMethod]
    public void UnorderedListBlock_ReturnsListBlock()
    {
        var markdown = "- Item A\n- Item B\n    - Item B.2\n- Item C";
        var expectedBlocksJson = """
            [
                {
                    "id": "XRL-mgPOlP",
                    "type": "list",
                    "data": {
                        "style": "unordered",
                        "items": [
                            {
                                "content": "Item A",
                                "items": []
                            },
                            {
                                "content": "Item B",
                                "items": [
                                    {
                                        "content": "Item B.2",
                                        "items": []
                                    }
                                ]
                            },
                            {
                                "content": "Item C",
                                "items": []
                            }
                        ]
                    }
                }
            ]
            """;

        var converter = new MarkdownConverter();
        var json = converter.ConvertToEditorJs( markdown );


        var expectedBlocks = JsonNode.Parse( expectedBlocksJson ).AsArray();
        var actualBlocks = JsonNode.Parse( json )["blocks"].AsArray();

        AssertEqualArrays( expectedBlocks, actualBlocks );
    }

    [TestMethod]
    public void OrderedListBlock_ReturnsListBlock()
    {
        var markdown = "1. Item 1\n2. Item 2\n    1. Item 2.1\n3. Item 3";
        var expectedBlocksJson = """
            [
                {
                    "id": "XRL-mgPOlP",
                    "type": "list",
                    "data": {
                        "style": "ordered",
                        "items": [
                            {
                                "content": "Item 1",
                                "items": []
                            },
                            {
                                "content": "Item 2",
                                "items": [
                                    {
                                        "content": "Item 2.1",
                                        "items": []
                                    }
                                ]
                            },
                            {
                                "content": "Item 3",
                                "items": []
                            }
                        ]
                    }
                }
            ]
            """;

        var converter = new MarkdownConverter();
        var json = converter.ConvertToEditorJs( markdown );


        var expectedBlocks = JsonNode.Parse( expectedBlocksJson ).AsArray();
        var actualBlocks = JsonNode.Parse( json )["blocks"].AsArray();

        AssertEqualArrays( expectedBlocks, actualBlocks );
    }

    #region Assert Methods

    private static void AssertEqualArrays( JsonArray expected, JsonArray actual, string jsonPath = "$" )
    {
        Assert.AreEqual( expected.Count, actual.Count );

        for ( var i = 0; i < expected.Count; i++ )
        {
            var expectedValue = expected[i];
            var actualValue = actual[i];

            if ( expectedValue is JsonObject expectedObj && actualValue is JsonObject actualObj )
            {
                AssertEqualObjects( expectedObj, actualObj, $"{jsonPath}[{i}]" );
            }
            else if ( expectedValue is JsonArray expectedArr && actualValue is JsonArray actualArr )
            {
                AssertEqualArrays( expectedArr, actualArr, $"{jsonPath}[{i}]" );
            }
            else
            {
                Assert.AreEqual( expectedValue.ToString(), actualValue.ToString(), $"Value mismatch at {jsonPath}[{i}]" );
            }
        }
    }

    private static void AssertEqualObjects( JsonObject expected, JsonObject actual, string jsonPath = "$")
    {
        Assert.AreEqual( expected.Count, actual.Count );

        foreach ( var kvp in expected )
        {
            Assert.IsTrue( actual.ContainsKey( kvp.Key ) );

            if ( kvp.Key == "id" )
            {
                // Skip "id" property since it's randomly generated
                continue;
            }

            var expectedValue = kvp.Value;
            var actualValue = actual[kvp.Key];

            if ( expectedValue is JsonObject expectedObj && actualValue is JsonObject actualObj )
            {
                AssertEqualObjects( expectedObj, actualObj, $"{jsonPath}.{kvp.Key}" );
            }
            else if ( expectedValue is JsonArray expectedArr && actualValue is JsonArray actualArr )
            {
                AssertEqualArrays( expectedArr, actualArr, $"{jsonPath}.{kvp.Key}" );
            }
            else
            {
                Assert.AreEqual( expectedValue.ToString(), actualValue.ToString(), $"Value mismatch at {jsonPath}.{kvp.Key}" );
            }
        }
    }

    #endregion
}
