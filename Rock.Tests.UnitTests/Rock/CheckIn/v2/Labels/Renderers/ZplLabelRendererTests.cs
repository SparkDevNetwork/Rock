using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2.Labels;
using Rock.CheckIn.v2.Labels.Renderers;
using Rock.Enums.CheckIn.Labels;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.ViewModels.CheckIn.Labels;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Labels.Renderers
{
    [TestClass]
    public class ZplLabelRendererTests : MockDatabaseTestsBase
    {
        #region BeginLabel

        [TestMethod]
        public void BeginLabel_WithDefaults_IncludesStartFormatToken()
        {
            var expectedToken = "^XA";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedToken );
        }

        [TestMethod]
        public void BeginLabel_WithLabelSize_SetsWidthToken()
        {
            var labelWidth = 2.5;
            var dpi = 300;
            var expectedToken = $"^PW{labelWidth * dpi}";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag
                {
                    Width = labelWidth
                },
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedToken );
        }

        [TestMethod]
        public void BeginLabel_WithLabelSize_SetsLengthToken()
        {
            var labelHeight = 1.5;
            var dpi = 300;
            var expectedToken = $"^LL{labelHeight * dpi}";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag
                {
                    Height = labelHeight
                },
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedToken );
        }

        #endregion

        #region EndLabel

        [TestMethod]
        public void EndLabel_WithDefaults_IncludesEndFormatToken()
        {
            var expectedToken = "^XZ";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.EndLabel();
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedToken );
        }

        #endregion

        #region WriteTextField

        [TestMethod]
        public void WriteTextField_WithFirstItemOnly_IncludesFirstValueOnly()
        {
            var expectedValue = "FirstValue";
            var unexpectedValue = "SecondValue";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    CollectionFormat = TextCollectionFormat.FirstItemOnly
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { expectedValue, unexpectedValue } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedValue );
            Assert.That.DoesNotContain( zpl, unexpectedValue );
        }

        [TestMethod]
        public void WriteTextField_WithCommaDelimited_IncludesAllValues()
        {
            var firstValue = "FirstValue";
            var secondValue = "SecondValue";
            var expectedValue = $"{firstValue}, {secondValue}";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    CollectionFormat = TextCollectionFormat.CommaDelimited
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "FirstValue", "SecondValue" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedValue );
        }

        [TestMethod]
        public void WriteTextField_WithOnePerLine_IncludesAllValues()
        {
            var firstValue = "FirstValue";
            var secondValue = "SecondValue";
            var expectedValue = $"{firstValue}\\&{secondValue}";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    CollectionFormat = TextCollectionFormat.OnePerLine
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "FirstValue", "SecondValue" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedValue );
        }

        [TestMethod]
        public void WriteTextField_WithColorInverted_IncludesFieldReversePrintToken()
        {
            var expectedToken = "^FR";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    IsColorInverted = true
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedToken );
        }

        [TestMethod]
        public void WriteTextField_WithFieldWidth_IncludesFieldBlockToken()
        {
            var fieldWidth = 2.5;
            var dpi = 300;
            var expectedToken = "^FB750,";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                Width = fieldWidth
            } );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration() );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedToken );
        }

        [TestMethod]
        public void WriteTextField_WithFieldHeight_IncludesLineCount()
        {
            var fieldHeight = 2.25;
            var dpi = 300;
            var expectedLineCount = 2;
            var expectedPattern = new Regex( $@"\^FB[0-9]+,{expectedLineCount}" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    FontSize = 72
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteTextField_WithTwoColumn_IncludesAllValues()
        {
            var expectedFirstValue = "FirstValue";
            var expectedSecondValue = "SecondValue";
            var expectedThirdValue = "ThirdValue";
            var expectedFourthValue = "FourthValue";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                Width = 4,
                Height = 1
            } );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    CollectionFormat = TextCollectionFormat.TwoColumn
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { expectedFirstValue, expectedSecondValue, expectedThirdValue, expectedFourthValue } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedFirstValue );
            Assert.That.Contains( zpl, expectedSecondValue );
            Assert.That.Contains( zpl, expectedThirdValue );
            Assert.That.Contains( zpl, expectedFourthValue );
        }

        [TestMethod]
        public void WriteTextField_WithLeftAlignment_IncludesLeftAlignmentToken()
        {
            var expectedAlignment = "L";
            var expectedPattern = new Regex( $@"\^FB[0-9]+,[0-9]+,[0-9]+,{expectedAlignment}" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );
            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    HorizontalAlignment = HorizontalTextAlignment.Left
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteTextField_WithCenterAlignment_IncludesCenterAlignmentToken()
        {
            var expectedAlignment = "C";
            var expectedPattern = new Regex( $@"\^FB[0-9]+,[0-9]+,[0-9]+,{expectedAlignment}" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );
            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    HorizontalAlignment = HorizontalTextAlignment.Center
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteTextField_WithCenterAlignment_EndsTextWithZebraNewline()
        {
            var expectedPattern = new Regex( $@"\^FD.*\\&\^FS" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );
            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    HorizontalAlignment = HorizontalTextAlignment.Center
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteTextField_WithRightAlignment_IncludesRightAlignmentToken()
        {
            var expectedAlignment = "R";
            var expectedPattern = new Regex( $@"\^FB[0-9]+,[0-9]+,[0-9]+,{expectedAlignment}" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );
            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    HorizontalAlignment = HorizontalTextAlignment.Right
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteTextField_WithRegularText_MakesTextSquare()
        {
            var expectedPattern = new Regex( $@"\^A0,([0-9]+),([0-9]+)" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );
            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    IsBold = false,
                    IsCondensed = false
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            var match = expectedPattern.Match( zpl );

            Assert.That.IsTrue( match.Success );

            var isSquare = match.Groups[1].Value.AsInteger() == match.Groups[2].Value.AsInteger();

            Assert.That.IsTrue( isSquare, $"Expected '{match.Groups[1].Value.AsInteger()}' to be equal to '{match.Groups[2].Value.AsInteger()}'." );
        }

        [TestMethod]
        public void WriteTextField_WithBoldText_MakesTextWider()
        {
            var expectedPattern = new Regex( $@"\^A0,([0-9]+),([0-9]+)" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );
            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    IsBold = true
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            var match = expectedPattern.Match( zpl );

            Assert.That.IsTrue( match.Success );

            var isWider = match.Groups[1].Value.AsInteger() > match.Groups[2].Value.AsInteger();

            Assert.That.IsTrue( isWider, $"Expected '{match.Groups[1].Value.AsInteger()}' to be greater than '{match.Groups[1].Value.AsInteger()}'." );
        }

        [TestMethod]
        public void WriteTextField_WithCondensedText_MakesTextNarrower()
        {
            var expectedPattern = new Regex( $@"\^A0,([0-9]+),([0-9]+)" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );
            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    IsCondensed = true
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            var match = expectedPattern.Match( zpl );

            Assert.That.IsTrue( match.Success );

            var isNarrower = match.Groups[1].Value.AsInteger() < match.Groups[2].Value.AsInteger();

            Assert.That.IsTrue( isNarrower, $"Expected '{match.Groups[1].Value.AsInteger()}' to be less than '{match.Groups[1].Value.AsInteger()}'." );
        }

        [TestMethod]
        public void WriteTextField_WithBoldAndCondensedText_MakesTextWider()
        {
            var expectedPattern = new Regex( $@"\^A0,([0-9]+),([0-9]+)" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag() );
            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
                .Returns( new TextFieldConfiguration
                {
                    IsBold = true,
                    IsCondensed = true
                } );
            field.Setup( f => f.GetFormattedValues( request ) )
                .Returns( () => new List<string> { "Test Value" } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            var match = expectedPattern.Match( zpl );

            Assert.That.IsTrue( match.Success );

            var isWider = match.Groups[1].Value.AsInteger() > match.Groups[2].Value.AsInteger();

            Assert.That.IsTrue( isWider, $"Expected '{match.Groups[1].Value.AsInteger()}' to be greater than '{match.Groups[1].Value.AsInteger()}'." );
        }

        #endregion

        #region WriteLineField

        [TestMethod]
        public void WriteLineField_WithPositiveWidthAndPositiveHeight_SetsOriginCorrectly()
        {
            var fieldLeft = 1;
            var fieldTop = 1;
            var fieldWidth = 0.5;
            var fieldHeight = 0.5;
            var dpi = 300;
            var expectedLeft = 1;
            var expectedTop = 1;
            var expectedOrigin = $"^FO{expectedLeft * dpi},{expectedTop * dpi}";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Left = fieldLeft,
                Top = fieldTop,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedOrigin );
        }

        [TestMethod]
        public void WriteLineField_WithPositiveWidthAndPositiveHeight_SetsOrientationToLeftLeaning()
        {
            var fieldLeft = 1;
            var fieldTop = 1;
            var fieldWidth = 0.5;
            var fieldHeight = 0.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GD[0-9]+,[0-9]+,[0-9]+,[BW],L" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Left = fieldLeft,
                Top = fieldTop,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteLineField_WithPositiveWidthAndNegativeHeight_SetsOriginCorrectly()
        {
            var fieldLeft = 1;
            var fieldTop = 1;
            var fieldWidth = 0.5;
            var fieldHeight = -0.5;
            var dpi = 300;
            var expectedLeft = 1;
            var expectedTop = 0.5;
            var expectedOrigin = $"^FO{expectedLeft * dpi},{expectedTop * dpi}";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Left = fieldLeft,
                Top = fieldTop,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedOrigin );
        }

        [TestMethod]
        public void WriteLineField_WithPositiveWidthAndNegativeHeight_SetsOrientationToRightLeaning()
        {
            var fieldLeft = 1;
            var fieldTop = 1;
            var fieldWidth = 0.5;
            var fieldHeight = -0.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GD[0-9]+,[0-9]+,[0-9]+,[BW],R" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Left = fieldLeft,
                Top = fieldTop,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteLineField_WithNegativeWidthAndPositiveHeight_SetsOriginCorrectly()
        {
            var fieldLeft = 1;
            var fieldTop = 1;
            var fieldWidth = -0.5;
            var fieldHeight = 0.5;
            var dpi = 300;
            var expectedLeft = 0.5;
            var expectedTop = 1;
            var expectedOrigin = $"^FO{expectedLeft * dpi},{expectedTop * dpi}";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Left = fieldLeft,
                Top = fieldTop,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedOrigin );
        }

        [TestMethod]
        public void WriteLineField_WithNegativeWidthAndPositiveHeight_SetsOrientationToRightLeaning()
        {
            var fieldLeft = 1;
            var fieldTop = 1;
            var fieldWidth = -0.5;
            var fieldHeight = 0.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GD[0-9]+,[0-9]+,[0-9]+,[BW],R" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Left = fieldLeft,
                Top = fieldTop,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteLineField_WithNegativeWidthAndNegativeHeight_SetsOriginCorrectly()
        {
            var fieldLeft = 1;
            var fieldTop = 1;
            var fieldWidth = -0.5;
            var fieldHeight = -0.5;
            var dpi = 300;
            var expectedLeft = 0.5;
            var expectedTop = 0.5;
            var expectedOrigin = $"^FO{expectedLeft * dpi},{expectedTop * dpi}";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Left = fieldLeft,
                Top = fieldTop,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedOrigin );
        }

        [TestMethod]
        public void WriteLineField_WithNegativeWidthAndNegativeHeight_SetsOrientationToLeftLeaning()
        {
            var fieldLeft = 1;
            var fieldTop = 1;
            var fieldWidth = -0.5;
            var fieldHeight = -0.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GD[0-9]+,[0-9]+,[0-9]+,[BW],L" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Left = fieldLeft,
                Top = fieldTop,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteLineField_WithFieldWidthAndHeight_IncludesGraphicLine()
        {
            var fieldWidth = 4;
            var fieldHeight = 3;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GD1200,900," );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteLineField_WithThickness_UsesThickness()
        {
            var expectedThickness = 12;
            var expectedPattern = new Regex( @"\^GD[0-9]+,[0-9]+,12" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                Width = 0.5,
                Height = 0.5,
                FieldType = LabelFieldType.Line
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration
                {
                    Thickness = expectedThickness
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteLineField_WithBlack_SetsColorToBlack()
        {
            var expectedPattern = new Regex( @"\^GD[0-9]+,[0-9]+,[0-9]+,B" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                Width = 0.5,
                Height = 0.5,
                FieldType = LabelFieldType.Line
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration
                {
                    IsBlack = true
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteLineField_WithoutBlack_SetsColorToWhite()
        {
            var expectedPattern = new Regex( @"\^GD[0-9]+,[0-9]+,[0-9]+,W" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                Width = 0.5,
                Height = 0.5,
                FieldType = LabelFieldType.Line
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration
                {
                    IsBlack = false
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteLineField_WithHorizontalLine_DrawsWithBox()
        {
            var fieldWidth = 1;
            var fieldHeight = 0;
            var thickness = 4;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GB300,4,4," );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration
                {
                    Thickness = thickness
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteLineField_WithVerticalLine_DrawsWithBox()
        {
            var fieldWidth = 0;
            var fieldHeight = 1;
            var thickness = 4;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GB4,300,4," );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Line,
                Width = fieldWidth,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<LineFieldConfiguration>() )
                .Returns( new LineFieldConfiguration
                {
                    Thickness = thickness
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        #endregion

        #region WriteRectangleField

        [TestMethod]
        public void WriteRectangleField_WithFieldWidth_IncludesGraphicBoxToken()
        {
            var fieldWidth = 2.5;
            var dpi = 300;
            var expectedToken = "^GB750,";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Rectangle,
                Width = fieldWidth
            } );

            field.Setup( f => f.GetConfiguration<RectangleFieldConfiguration>() )
                .Returns( new RectangleFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedToken );
        }

        [TestMethod]
        public void WriteRectangleField_WithFieldHeight_IncludesGraphicBoxToken()
        {
            var fieldHeight = 2.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GB[0-9]+,750" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Rectangle,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<RectangleFieldConfiguration>() )
                .Returns( new RectangleFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteRectangleField_WithFilled_SetsBorderToHeight()
        {
            var fieldHeight = 2.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GB[0-9]+,[0-9]+,750" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Rectangle,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<RectangleFieldConfiguration>() )
                .Returns( new RectangleFieldConfiguration
                {
                    IsFilled = true
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteRectangleField_WithoutFilled_UsesBorderThickness()
        {
            var fieldHeight = 2.25;
            var dpi = 300;
            var borderThickness = 12;
            var expectedPattern = new Regex( @"\^GB[0-9]+,[0-9]+,12" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Rectangle,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<RectangleFieldConfiguration>() )
                .Returns( new RectangleFieldConfiguration
                {
                    BorderThickness = borderThickness
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteRectangleField_WithBlack_SetsColorToBlack()
        {
            var fieldHeight = 2.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GB[0-9]+,[0-9]+,[0-9]+,B" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Rectangle,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<RectangleFieldConfiguration>() )
                .Returns( new RectangleFieldConfiguration
                {
                    IsBlack = true
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteRectangleField_WithoutBlack_SetsColorToWhite()
        {
            var fieldHeight = 2.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GB[0-9]+,[0-9]+,[0-9]+,W" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Rectangle,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<RectangleFieldConfiguration>() )
                .Returns( new RectangleFieldConfiguration
                {
                    IsBlack = false
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteRectangleField_WithRoundingValue_SetsRoundingParameter()
        {
            var fieldHeight = 2.5;
            var dpi = 300;
            var cornerRadius = 4;
            var expectedPattern = new Regex( @"\^GB[0-9]+,[0-9]+,[0-9]+,[BW],4" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Rectangle,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<RectangleFieldConfiguration>() )
                .Returns( new RectangleFieldConfiguration
                {
                    CornerRadius = cornerRadius
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        #endregion

        #region WriteEllipseField

        [TestMethod]
        public void WriteEllipseField_WithFieldWidth_IncludesGraphicEllipseToken()
        {
            var fieldWidth = 2.5;
            var dpi = 300;
            var expectedToken = "^GE750,";

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Ellipse,
                Width = fieldWidth
            } );

            field.Setup( f => f.GetConfiguration<EllipseFieldConfiguration>() )
                .Returns( new EllipseFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Contains( zpl, expectedToken );
        }

        [TestMethod]
        public void WriteEllipseField_WithFieldHeight_IncludesGraphicEllipseToken()
        {
            var fieldHeight = 2.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GE[0-9]+,750" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Ellipse,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<EllipseFieldConfiguration>() )
                .Returns( new EllipseFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteEllipseField_WithFilled_SetsBorderToHeight()
        {
            var fieldHeight = 2.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GE[0-9]+,[0-9]+,750" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Ellipse,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<EllipseFieldConfiguration>() )
                .Returns( new EllipseFieldConfiguration
                {
                    IsFilled = true
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteEllipseField_WithoutFilled_UsesBorderThickness()
        {
            var fieldHeight = 2.25;
            var dpi = 300;
            var borderThickness = 12;
            var expectedPattern = new Regex( @"\^GE[0-9]+,[0-9]+,12" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Ellipse,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<EllipseFieldConfiguration>() )
                .Returns( new EllipseFieldConfiguration
                {
                    BorderThickness = borderThickness
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteEllipseField_WithBlack_SetsColorToBlack()
        {
            var fieldHeight = 2.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GE[0-9]+,[0-9]+,[0-9]+,B" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Ellipse,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<EllipseFieldConfiguration>() )
                .Returns( new EllipseFieldConfiguration
                {
                    IsBlack = true
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteEllipseField_WithoutBlack_SetsColorToWhite()
        {
            var fieldHeight = 2.5;
            var dpi = 300;
            var expectedPattern = new Regex( @"\^GE[0-9]+,[0-9]+,[0-9]+,W" );

            var renderer = new ZplLabelRenderer();
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Ellipse,
                Height = fieldHeight
            } );

            field.Setup( f => f.GetConfiguration<EllipseFieldConfiguration>() )
                .Returns( new EllipseFieldConfiguration
                {
                    IsBlack = false
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        #endregion

        #region WriteImageField

        [TestMethod]
        public void WriteImageField_WithImageData_SetsCorrectLength()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 19 ).Select( i => ( byte ) i ).ToArray();
            var expectedPattern = new Regex( @"\^GFA,19,19," );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetImage( It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<ZplImageOptions>() ) )
                .Returns( new ZplImageCache( imageData, 0, 0 ) );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Image
            } );

            field.Setup( f => f.GetConfiguration<ImageFieldConfiguration>() )
                .Returns( new ImageFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteImageField_WithImageData_EncodesDataAsHex()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 8, 4 ).Select( i => ( byte ) i ).ToArray();
            var expectedPattern = new Regex( @"\^GFA,[0-9],[0-9],[0-9],08090A0B" );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetImage( It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<ZplImageOptions>() ) )
                .Returns( new ZplImageCache( imageData, 0, 0 ) );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Image
            } );

            field.Setup( f => f.GetConfiguration<ImageFieldConfiguration>() )
                .Returns( new ImageFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteImageField_WithInvertedColor_IncludesFieldReverseCommand()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 4 ).Select( i => ( byte ) i ).ToArray();
            var expectedPattern = new Regex( @"\^FR" );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetImage( It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<ZplImageOptions>() ) )
                .Returns( new ZplImageCache( imageData, 0, 0 ) );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Image
            } );

            field.Setup( f => f.GetConfiguration<ImageFieldConfiguration>() )
                .Returns( new ImageFieldConfiguration
                {
                    IsColorInverted = true
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteImageField_WithFailedConversion_DoesNotEmitField()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 4 ).Select( i => ( byte ) i ).ToArray();

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetImage( It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<ZplImageOptions>() ) )
                .Throws<InvalidOperationException>();

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Image
            } );

            field.Setup( f => f.GetConfiguration<ImageFieldConfiguration>() )
                .Returns( new ImageFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.IsEmpty( zpl );
        }

        #endregion

        #region WriteIconField

        [TestMethod]
        public void WriteIconField_WithImageData_IncludesGraphicField()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 19 ).Select( i => ( byte ) i ).ToArray();
            var expectedPattern = new Regex( @"\^GFA,19,19," );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetIcon( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<LabelIcon>() ) )
                .Returns( new ZplImageCache( imageData, 0, 0 ) );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Icon
            } );

            field.Setup( f => f.GetConfiguration<IconFieldConfiguration>() )
                .Returns( new IconFieldConfiguration
                {
                    Icon = new LabelIcon( "undefined", "undefined", "", false )
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteIconField_WithImageData_EncodesDataAsHex()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 8, 4 ).Select( i => ( byte ) i ).ToArray();
            var expectedPattern = new Regex( @"\^GFA,[0-9],[0-9],[0-9],08090A0B" );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetIcon( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<LabelIcon>() ) )
                .Returns( new ZplImageCache( imageData, 0, 0 ) );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Icon
            } );

            field.Setup( f => f.GetConfiguration<IconFieldConfiguration>() )
                .Returns( new IconFieldConfiguration
                {
                    Icon = new LabelIcon( "undefined", "undefined", "", false )
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteIconField_WithInvertedColor_IncludesFieldReverseCommand()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 4 ).Select( i => ( byte ) i ).ToArray();
            var expectedPattern = new Regex( @"\^FR" );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetIcon( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<LabelIcon>() ) )
                .Returns( new ZplImageCache( imageData, 0, 0 ) );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Icon
            } );

            field.Setup( f => f.GetConfiguration<IconFieldConfiguration>() )
                .Returns( new IconFieldConfiguration
                {
                    Icon = new LabelIcon( "undefined", "undefined", "", false ),
                    IsColorInverted = true
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteIconField_WithFailedConversion_DoesNotEmitField()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 4 ).Select( i => ( byte ) i ).ToArray();

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetIcon( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<LabelIcon>() ) )
                .Throws<InvalidOperationException>();

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Icon
            } );

            field.Setup( f => f.GetConfiguration<IconFieldConfiguration>() )
                .Returns( new IconFieldConfiguration
                {
                    Icon = new LabelIcon( "undefined", "undefined", "", false )
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.IsEmpty( zpl );
        }

        [TestMethod]
        public void WriteIconField_WithInvalidIcon_DoesNotEmitField()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 4 ).Select( i => ( byte ) i ).ToArray();

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new ZplLabelRenderer();

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Icon
            } );

            field.Setup( f => f.GetConfiguration<IconFieldConfiguration>() )
                .Returns( new IconFieldConfiguration
                {
                    Icon = null
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.IsEmpty( zpl );
        }

        #endregion

        #region WriteAttendeePhotoField

        [TestMethod]
        public void WriteAttendeePhotoField_WithPhotoData_SetsCorrectLength()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 19 ).Select( i => ( byte ) i ).ToArray();
            var expectedPattern = new Regex( @"\^GFA,19,19," );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetPersonPhoto( It.IsAny<ZplImageOptions>() ) )
                .Returns( new ZplImageCache( imageData, 0, 0 ) );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.AttendeePhoto
            } );

            field.Setup( f => f.GetConfiguration<AttendeePhotoFieldConfiguration>() )
                .Returns( new AttendeePhotoFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteAttendeePhotoField_WithPhotoData_EncodesDataAsHex()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 8, 4 ).Select( i => ( byte ) i ).ToArray();
            var expectedPattern = new Regex( @"\^GFA,[0-9],[0-9],[0-9],08090A0B" );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetPersonPhoto( It.IsAny<ZplImageOptions>() ) )
                .Returns( new ZplImageCache( imageData, 0, 0 ) );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.AttendeePhoto
            } );

            field.Setup( f => f.GetConfiguration<AttendeePhotoFieldConfiguration>() )
                .Returns( new AttendeePhotoFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteAttendeePhotoField_WithInvertedColor_IncludesFieldReverseCommand()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 4 ).Select( i => ( byte ) i ).ToArray();
            var expectedPattern = new Regex( @"\^FR" );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetPersonPhoto( It.IsAny<ZplImageOptions>() ) )
                .Returns( new ZplImageCache( imageData, 0, 0 ) );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.AttendeePhoto
            } );

            field.Setup( f => f.GetConfiguration<AttendeePhotoFieldConfiguration>() )
                .Returns( new AttendeePhotoFieldConfiguration
                {
                    IsColorInverted = true
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteAttendeePhotoField_WithFailedConversion_DoesNotEmitField()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 4 ).Select( i => ( byte ) i ).ToArray();

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetPersonPhoto( It.IsAny<ZplImageOptions>() ) )
                .Throws<InvalidOperationException>();

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.AttendeePhoto
            } );

            field.Setup( f => f.GetConfiguration<AttendeePhotoFieldConfiguration>() )
                .Returns( new AttendeePhotoFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.IsEmpty( zpl );
        }

        [TestMethod]
        public void WriteAttendeePhotoField_WithoutPhoto_DoesNotEmitField()
        {
            var dpi = 300;
            var imageData = Enumerable.Range( 0, 4 ).Select( i => ( byte ) i ).ToArray();

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = dpi
                }
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetPersonPhoto( It.IsAny<ZplImageOptions>() ) )
                .Returns<ZplImageCache>( null );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.AttendeePhoto
            } );

            field.Setup( f => f.GetConfiguration<AttendeePhotoFieldConfiguration>() )
                .Returns( new AttendeePhotoFieldConfiguration() );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.IsEmpty( zpl );
        }

        [TestMethod]
        public void WriteAttendeePhotoField_WithoutHighQuality_SetsDitheringToFast()
        {
            var expectedDithering = DitherMode.Fast;
            var actualDithering = DitherMode.None;

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetPersonPhoto( It.IsAny<ZplImageOptions>() ) )
                .Returns<ZplImageOptions>( o =>
                {
                    actualDithering = o.Dithering;

                    return new ZplImageCache( new byte[0], 0, 0 );
                } );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.AttendeePhoto
            } );

            field.Setup( f => f.GetConfiguration<AttendeePhotoFieldConfiguration>() )
                .Returns( new AttendeePhotoFieldConfiguration
                {
                    IsHighQuality = false
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.AreEqual( expectedDithering, actualDithering );
        }

        [TestMethod]
        public void WriteAttendeePhotoField_WithHighQuality_SetsDitheringToQuality()
        {
            var expectedDithering = DitherMode.Quality;
            var actualDithering = DitherMode.None;

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( f => f.GetPersonPhoto( It.IsAny<ZplImageOptions>() ) )
                .Returns<ZplImageOptions>( o =>
                {
                    actualDithering = o.Dithering;

                    return new ZplImageCache( new byte[0], 0, 0 );
                } );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.AttendeePhoto
            } );

            field.Setup( f => f.GetConfiguration<AttendeePhotoFieldConfiguration>() )
                .Returns( new AttendeePhotoFieldConfiguration
                {
                    IsHighQuality = true
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.AreEqual( expectedDithering, actualDithering );
        }

        #endregion

        #region WriteBarcodeField

        [TestMethod]
        public void WriteBarcodeField_WithEmptyCustomText_DoesNotEmitField()
        {
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var renderer = new ZplLabelRenderer();

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Barcode
            } );

            field.Setup( f => f.GetConfiguration<BarcodeFieldConfiguration>() )
                .Returns( new BarcodeFieldConfiguration
                {
                    IsDynamic = true,
                    DynamicTextTemplate = string.Empty
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.IsEmpty( zpl );
        }

        [TestMethod]
        public void WriteBarcodeField_WithCode128_SetsModuleWidthTo3()
        {
            var expectedPattern = new Regex( @"\^BY3" );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var renderer = new ZplLabelRenderer();

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Barcode
            } );

            field.Setup( f => f.GetConfiguration<BarcodeFieldConfiguration>() )
                .Returns( new BarcodeFieldConfiguration
                {
                    Format = BarcodeFormat.Code128,
                    IsDynamic = true,
                    DynamicTextTemplate = "1234"
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteBarcodeField_WithCode128AndDynamicContent_IncludesTextValue()
        {
            var expectedPattern = new Regex( @"\^FD1234\^FS" );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities()
            };

            var renderer = new ZplLabelRenderer();

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Barcode
            } );

            field.Setup( f => f.GetConfiguration<BarcodeFieldConfiguration>() )
                .Returns( new BarcodeFieldConfiguration
                {
                    Format = BarcodeFormat.Code128,
                    IsDynamic = true,
                    DynamicTextTemplate = "1234"
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteBarcodeField_WithAlternateId_IncludesValue()
        {
            var expectedPattern = new Regex( @"\^FD1234\^FS" );

            var rockContext = MockDatabaseHelper.GetRockContextMock();
            var sampleSearchKey = new PersonSearchKey
            {
                PersonAlias = new PersonAlias
                {
                    PersonId = 2
                },
                SearchTypeValueId = 1,
                SearchValue = "1234"
            };
            var person = new Person
            {
                Id = 2
            };

            rockContext.SetupDbSet( sampleSearchKey );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities(),
                LabelData = new PersonLabelData( person, null, new List<LabelAttendanceDetail>(), rockContext.Object ),
                RockContext = rockContext.Object
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( a => a.GetSearchTypeAlternateIdValueId() ).Returns( 1 );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Barcode
            } );

            field.Setup( f => f.GetConfiguration<BarcodeFieldConfiguration>() )
                .Returns( new BarcodeFieldConfiguration
                {
                    Format = BarcodeFormat.Code128,
                    IsDynamic = false
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteBarcodeField_WithAlternateIdAndNullSearchValue_DoesNotEmitField()
        {
            var rockContext = MockDatabaseHelper.GetRockContextMock();
            var sampleSearchKey = new PersonSearchKey
            {
                PersonAlias = new PersonAlias
                {
                    PersonId = 2
                },
                SearchTypeValueId = 1,
                SearchValue = null
            };
            var person = new Person
            {
                Id = 2
            };

            rockContext.SetupDbSet( sampleSearchKey );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities(),
                LabelData = new PersonLabelData( person, null, new List<LabelAttendanceDetail>(), rockContext.Object ),
                RockContext = rockContext.Object
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( a => a.GetSearchTypeAlternateIdValueId() ).Returns( 1 );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Barcode
            } );

            field.Setup( f => f.GetConfiguration<BarcodeFieldConfiguration>() )
                .Returns( new BarcodeFieldConfiguration
                {
                    Format = BarcodeFormat.Code128,
                    IsDynamic = false
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.IsEmpty( zpl );
        }

        [TestMethod]
        public void WriteBarcodeField_WithMissingAlternateId_DoesNotEmitField()
        {
            var rockContext = MockDatabaseHelper.GetRockContextMock();
            var person = new Person
            {
                Id = 2
            };

            rockContext.SetupDbSet<PersonSearchKey>();

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities(),
                LabelData = new PersonLabelData( person, null, new List<LabelAttendanceDetail>(), rockContext.Object ),
                RockContext = rockContext.Object
            };

            var renderer = new Mock<ZplLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            renderer.Setup( a => a.GetSearchTypeAlternateIdValueId() ).Returns( 1 );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Barcode
            } );

            field.Setup( f => f.GetConfiguration<BarcodeFieldConfiguration>() )
                .Returns( new BarcodeFieldConfiguration
                {
                    Format = BarcodeFormat.Code128,
                    IsDynamic = false
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.Object.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.Object.WriteField( field.Object );
                renderer.Object.Dispose();
            } );

            Assert.That.IsEmpty( zpl );
        }

        [TestMethod]
        public void WriteBarcodeField_WithQRCodeAndDynamicContent_EmitsCodeData()
        {
            var expectedPattern = new Regex( @"\^GFA,[0-9]+,[0-9]+,4,000000000000000000000000000000000FEEBF000828A0000BADAE000BA8AE000BA92E000823A0000FEABF00000F000006B72F00080C500006E772000E95DC0007E4F300000E23000FED08000824A2000BAAAA000BA355000BAD76000829DC000FE5760000000000000000000000000000000000\^FS" );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = 300
                }
            };

            var renderer = new ZplLabelRenderer();

            // Use a size of 0.2x0.2 so there is enough room for 2 dots per module.
            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Barcode,
                Width = 0.1,
                Height = 0.1
            } );

            field.Setup( f => f.GetConfiguration<BarcodeFieldConfiguration>() )
                .Returns( new BarcodeFieldConfiguration
                {
                    Format = BarcodeFormat.QRCode,
                    IsDynamic = true,
                    DynamicTextTemplate = "1234"
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        [TestMethod]
        public void WriteBarcodeField_WithQRCodeAndLargerBox_Emits2DotsPerModule()
        {
            var expectedPattern = new Regex( @"\^GFA,[0-9]+,[0-9]+,8," );

            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = 300
                }
            };

            var renderer = new ZplLabelRenderer();

            // Use a size of 0.2x0.2 so there is enough room for 2 dots per module.
            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Barcode,
                Width = 0.2,
                Height = 0.2
            } );

            field.Setup( f => f.GetConfiguration<BarcodeFieldConfiguration>() )
                .Returns( new BarcodeFieldConfiguration
                {
                    Format = BarcodeFormat.QRCode,
                    IsDynamic = true,
                    DynamicTextTemplate = "1234"
                } );

            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.That.Matches( zpl, expectedPattern );
        }

        #endregion

        #region Font Size Test

        [TestMethod]
        public void WriteTextField_InBaseFontSize_NoAdaptiveFontSizesProvided()
        {
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = 72 // keep dpi for testing as 72 to have the dot font size evaluate to same value as the font size.
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Loose, new LabelFieldBag() );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
               .Returns( new TextFieldConfiguration
               {
                   FontSize = 24
               } );

            field.Setup( f => f.GetFormattedValues( request ) )
               .Returns( new List<string> { "Text" } );

            var renderer = new ZplLabelRenderer();
            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.AreEqual( @"^FO0,0^FB0,1,0,L^A0,24,24^FDText^FS
", zpl );
        }


        [TestMethod]
        public void WriteTextField_InProvidedFontSize_TextLengthGreaterThanKey()
        {
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = 72 // keep dpi for testing as 72 to have the dot font size evaluate to same value as the font size.
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Loose, new LabelFieldBag() );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
               .Returns( new TextFieldConfiguration
               {
                   FontSize = 24,
                   AdaptiveFontSize = new Dictionary<int, double>
                   {
                       { 30, 12 }
                   }
               } );

            field.Setup( f => f.GetFormattedValues( request ) )
               .Returns( new List<string> { "Text With Length Greater Than Thirty" } );

            var renderer = new ZplLabelRenderer();
            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.AreEqual( @"^FO0,0^FB0,1,0,L^A0,12,12^FDText With Length Greater Than Thirty^FS
", zpl );
        }

        [TestMethod]
        public void WriteTextField_InProvidedFontSize_TextLengthLesserThanKey()
        {
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = 72 // keep dpi for testing as 72 to have the dot font size evaluate to same value as the font size.
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Loose, new LabelFieldBag() );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
               .Returns( new TextFieldConfiguration
               {
                   FontSize = 24,
                   AdaptiveFontSize = new Dictionary<int, double>
                   {
                       { 30, 12 }
                   }
               } );

            field.Setup( f => f.GetFormattedValues( request ) )
               .Returns( new List<string> { "Text" } );

            var renderer = new ZplLabelRenderer();
            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.AreEqual( @"^FO0,0^FB0,1,0,L^A0,24,24^FDText^FS
", zpl );
        }



        [TestMethod]
        public void WriteTextField_InBaseFontSize_TextLengthEqualToKey()
        {
            var request = new PrintLabelRequest
            {
                Label = new DesignedLabelBag(),
                Capabilities = new PrinterCapabilities
                {
                    Dpi = 72 // keep dpi for testing as 72 to have the dot font size evaluate to same value as the font size.
                }
            };

            var field = new Mock<LabelField>( MockBehavior.Loose, new LabelFieldBag() );

            field.Setup( f => f.GetConfiguration<TextFieldConfiguration>() )
               .Returns( new TextFieldConfiguration
               {
                   FontSize = 24,
                   AdaptiveFontSize = new Dictionary<int, double>
                   {
                       { 15, 18 },
                       { 30, 12 },
                       { 45, 6 }
                   }
               } );

            field.Setup( f => f.GetFormattedValues( request ) )
               .Returns( new List<string> { "Text With Length Greater Than Thirty" } );

            var renderer = new ZplLabelRenderer();
            var zpl = GetTextFromStream( stream =>
            {
                renderer.BeginLabel( stream, request );
                stream.SetLength( 0 );

                renderer.WriteField( field.Object );
                renderer.Dispose();
            } );

            Assert.AreEqual( @"^FO0,0^FB0,1,0,L^A0,12,12^FDText With Length Greater Than Thirty^FS
", zpl );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a memory stream and passes it to the action. After the action
        /// is done executing the stream contents will be read as a UTF8 string
        /// and returned.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>The UTF-8 string from the stream.</returns>
        private string GetTextFromStream( Action<MemoryStream> action )
        {
            using ( var stream = new MemoryStream() )
            {
                action( stream );

                stream.Position = 0;
                var bytes = stream.ReadBytesToEnd();
                return Encoding.UTF8.GetString( bytes );
            }
        }

        #endregion
    }
}
