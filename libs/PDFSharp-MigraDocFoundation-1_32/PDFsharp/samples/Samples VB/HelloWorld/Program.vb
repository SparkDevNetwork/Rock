#Region "PDFsharp - A .NET library for processing PDF"
'
' Authors:
'   PDFsharp team (mailto:PDFsharpSupport@pdfsharp.com)
'
' Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
'
' http://www.pdfsharp.com
' http://sourceforge.net/projects/pdfsharp
'
' Permission is hereby granted, free of charge, to any person obtaining a
' copy of this software and associated documentation files (the "Software"),
' to deal in the Software without restriction, including without limitation
' the rights to use, copy, modify, merge, publish, distribute, sublicense,
' and/or sell copies of the Software, and to permit persons to whom the
' Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included
' in all copies or substantial portions of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
' FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
#End Region

Imports PdfSharp
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

Module Program

  ' VB.NET version of 'Hello World'
  Sub Main()
    ' Create a new PDF document
    Dim document As PdfDocument = New PdfDocument
    document.Info.Title = "Created with PDFsharp"

    ' Create an empty page
    Dim page As PdfPage = document.AddPage

    ' Get an XGraphics object for drawing
    Dim gfx As XGraphics = XGraphics.FromPdfPage(page)

    ' Draw crossing lines
    Dim pen As XPen = New XPen(XColor.FromArgb(255, 0, 0))
    gfx.DrawLine(pen, New XPoint(0, 0), New XPoint(page.Width.Point, page.Height.Point))
    gfx.DrawLine(pen, New XPoint(page.Width.Point, 0), New XPoint(0, page.Height.Point))

    ' Draw an ellipse
    gfx.DrawEllipse(pen, 3 * page.Width.Point / 10, 3 * page.Height.Point / 10, 2 * page.Width.Point / 5, 2 * page.Height.Point / 5)

    ' Create a font
    Dim font As XFont = New XFont("Verdana", 20, XFontStyle.Bold)

    ' Draw the text
    gfx.DrawString("Hello, World!", font, XBrushes.Black, _
    New XRect(0, 0, page.Width.Point, page.Height.Point), XStringFormats.Center)

    ' Save the document...
    Dim filename As String = "HelloWorld.pdf"
    document.Save(filename)

    ' ...and start a viewer.
    Process.Start(filename)

  End Sub

End Module
