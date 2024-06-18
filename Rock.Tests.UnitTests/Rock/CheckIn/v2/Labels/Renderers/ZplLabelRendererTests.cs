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
using Rock.Tests.Shared;
using Rock.ViewModels.CheckIn.Labels;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Labels.Renderers
{
    [TestClass]
    public class ZplLabelRendererTests
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

            renderer.Setup( f => f.GetImage( It.IsAny<byte[]>(), It.IsAny<ZplImageOptions>(), It.IsAny<bool>() ) )
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

            renderer.Setup( f => f.GetImage( It.IsAny<byte[]>(), It.IsAny<ZplImageOptions>(), It.IsAny<bool>() ) )
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

            renderer.Setup( f => f.GetImage( It.IsAny<byte[]>(), It.IsAny<ZplImageOptions>(), It.IsAny<bool>() ) )
                .Returns( new ZplImageCache( imageData, 0, 0 ) );

            var field = new Mock<LabelField>( MockBehavior.Strict, new LabelFieldBag
            {
                FieldType = LabelFieldType.Image
            } );

            field.Setup( f => f.GetConfiguration<ImageFieldConfiguration>() )
                .Returns( new ImageFieldConfiguration
                {
                     IsInverted = true
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

            renderer.Setup( f => f.GetImage( It.IsAny<byte[]>(), It.IsAny<ZplImageOptions>(), It.IsAny<bool>() ) )
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
