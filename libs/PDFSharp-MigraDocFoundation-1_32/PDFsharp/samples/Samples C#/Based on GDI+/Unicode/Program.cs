#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   PDFsharp Team (mailto:PDFsharpSupport@pdfsharp.de)
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace Unicode
{
  /// <summary>
  /// This sample shows how to use Unicode text in PDFsharp.
  /// </summary>
  class Program
  {
    [STAThread]
    static void Main()
    {
      // Create new document
      PdfDocument document = new PdfDocument();

      // Set font encoding to unicode
      XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);

      XFont font = new XFont("Times New Roman", 12, XFontStyle.Regular, options);

      // Draw text in different languages
      for (int idx = 0; idx < texts.Length; idx++)
      {
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        XTextFormatter tf = new XTextFormatter(gfx);
        tf.Alignment = XParagraphAlignment.Left;

        tf.DrawString(texts[idx], font, XBrushes.Black,
          new XRect(100, 100, page.Width - 200, 600), XStringFormats.TopLeft);
      }

      const string filename = "Unicode_tempfile.pdf";
      // Save the document...
      document.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }

    static readonly string[] texts = new string[]
    {
      // International version of the text in English
      "English\n" +
      "PDFsharp is a .NET library for creating and processing PDF documents 'on the fly'. " +
      "The library is completely written in C# and based exclusively on safe, managed code. " +
      "PDFsharp offers two powerful abstraction levels to create and process PDF documents.\n" +
      "For drawing text, graphics, and images there is a set of classes which are modeled similar to the classes " +
      "of the name space System.Drawing of the .NET framework. With these classes it is not only possible to create " +
      "the content of PDF pages in an easy way, but they can also be used to draw in a window or on a printer.\n" +
      "Additionally PDFsharp completely models the structure elements PDF is based on. With them existing PDF documents " +
      "can be modified, merged, or split with ease.\n" +
      "The source code of PDFsharp is Open Source under the MIT license (http://en.wikipedia.org/wiki/MIT_License). " +
      "Therefore it is possible to use PDFsharp without limitations in non open source or commercial projects/products.",

      // PDFsharp is 'Made in Germany'
      "German (deutsch)\n" +
      "PDFsharp ist eine .NET-Bibliothek zum Erzeugen und Verarbeiten von PDF-Dokumenten 'On the Fly'. " +
      "Die Bibliothek ist vollständig in C# geschrieben und basiert ausschließlich auf sicherem, verwaltetem Code. " +
      "PDFsharp bietet zwei leistungsstarke Abstraktionsebenen zur Erstellung und Verarbeitung von PDF-Dokumenten.\n" +
      "Zum Zeichnen von Text, Grafik und Bildern gibt es einen Satz von Klassen, die sehr stark an die Klassen " +
      "des Namensraums System.Drawing des .NET Frameworks angelehnt sind. Mit diesen Klassen ist es nicht " +
      "nur auf einfache Weise möglich, den Inhalt von PDF-Seiten zu gestalten, sondern sie können auch zum " +
      "Zeichnen in einem Fenster oder auf einem Drucker verwendet werden.\n" +
      "Zusätzlich modelliert PDFsharp vollständig die Stukturelemente, auf denen PDF basiert. Dadurch können existierende " +
      "PDF-Dokumente mit Leichtigkeit zerlegt, ergänzt oder umgebaut werden.\n" +
      "Der Quellcode von PDFsharp ist Open-Source unter der MIT-Lizenz (http://de.wikipedia.org/wiki/MIT-Lizenz). " +
      "Damit kann PDFsharp auch uneingeschränkt in Nicht-Open-Source- oder kommerziellen Projekten/Produkten eingesetzt werden.",

      // Greek version
      // The text was translated by Babel Fish. We here in Germany have no idea what it means.
      // If you are a native speaker please correct it and mail it to mailto:PDFsharpSupport@pdfsharp.de
      "Greek (Translated with Babel Fish)\n" +
      "Το PDFsharp είναι βιβλιοθήκη δικτύου α. για τη δημιουργία και την επεξεργασία των εγγράφων PDF 'σχετικά με τη μύγα'. Η βιβλιοθήκη γράφεται εντελώς γ # και βασίζεται αποκλειστικά εκτός από, διοικούμενος κώδικας. Το PDFsharp προσφέρει δύο ισχυρά επίπεδα αφαίρεσης για να δημιουργήσει και να επεξεργαστεί τα έγγραφα PDF. Για το κείμενο, τη γραφική παράσταση, και τις εικόνες σχεδίων υπάρχει ένα σύνολο κατηγοριών που διαμορφώνονται παρόμοιος με τις κατηγορίες του διαστημικού σχεδίου συστημάτων ονόματος του. πλαισίου δικτύου. Με αυτές τις κατηγορίες που είναι όχι μόνο δυνατό να δημιουργηθεί το περιεχόμενο των σελίδων PDF με έναν εύκολο τρόπο, αλλά αυτοί μπορεί επίσης να χρησιμοποιηθεί για να επισύρει την προσοχή σε ένα παράθυρο ή σε έναν εκτυπωτή. Επιπλέον PDFsharp διαμορφώνει εντελώς τα στοιχεία PDF δομών είναι βασισμένο. Με τους τα υπάρχοντα έγγραφα PDF μπορούν να τροποποιηθούν, συγχωνευμένος, ή να χωρίσουν με την ευκολία. Ο κώδικας πηγής PDFsharp είναι ανοικτή πηγή με άδεια MIT (http://en.wikipedia.org/wiki/MIT_License). Επομένως είναι δυνατό να χρησιμοποιηθεί PDFsharp χωρίς προβλήματα στη μη ανοικτή πηγή ή τα εμπορικά προγράμματα/τα προϊόντα.",

      // Russian version (by courtesy of Alexey Kuznetsov)
      "Russian\n" +
      "PDFsharp это .NET библиотека для создания и обработки PDF документов 'налету'. " +
      "Библиотека полностью написана на языке C# и базируется исключительно на безопасном, управляемом коде. " +
      "PDFsharp использует два мощных абстрактных уровня для создания и обработки PDF документов.\n" + 
      "Для рисования текста, графики, и изображений в ней используется набор классов, которые разработаны аналогично с" +
      "пакетом System.Drawing, библиотеки .NET framework. С помощью этих классов возможно не только создавать" + 
      "содержимое PDF страниц очень легко, но они так же позволяют рисовать напрямую в окне приложения или на принтере.\n" +
      "Дополнительно PDFsharp имеет полноценные модели структурированных базовых элементов PDF. Они позволяют работать с существующим PDF документами " + 
      "для изменения их содержимого, склеивания документов, или разделения на части.\n" +
      "Исходный код PDFsharp библиотеки это Open Source распространяемый под лицензией MIT (http://ru.wikipedia.org/wiki/MIT_License). " +
      "Теоретически она позволяет использовать PDFsharp без ограничений в не open source проектах или коммерческих проектах/продуктах.",

      // French version (by courtesy of Olivier Dalet)
      "French (Français)\n" +
      "PDFSharp est une librairie .NET permettant de créer et de traiter des documents PDF 'à la volée'. " +
      "La librairie est entièrement écrite en C# et exclusivement basée sur du code sûr et géré. " +
      "PDFSharp fournit deux puissants niveaux d'abstraction pour la création et le traitement des documents PDF.\n" +
      "Un jeu de classes, modélisées afin de ressembler aux classes du namespace System.Drawing du framework .NET, " +
      "permet de dessiner du texte, des graphiques et des images. Non seulement ces classes permettent la création du " +
      "contenu des pages PDF de manière aisée, mais elles peuvent aussi être utilisées pour dessiner dans une fenêtre ou pour l'imprimante.\n" +
      "De plus, PDFSharp modélise complètement les éléments structurels de PDF. Ainsi, des documents PDF existants peuvent être " +
      "facilement modifiés, fusionnés ou éclatés.\n"+
      "Le code source de PDFSharp est Open Source sous licence MIT (http://fr.wikipedia.org/wiki/Licence_MIT). " +
      "Il est donc possible d'utiliser PDFSharp sans limitation aucune dans des projets ou produits non Open Source ou commerciaux.",

      // Dutch version (translated from English with Google, ĳ was added manually)
      "Dutch (translated with Google)\n" +
      "PDFsharp is een. NET bibliotheek voor het maken en verwerken van PDF-documenten 'on the fly'. " +
      "De bibliotheek is volledig geschreven in C# en uitsluitend op basis van veilige, beheerde code. " +
      "PDFsharp biedt twee krachtige abstractie niveaus te creëren en verwerken van PDF-documenten.\n" +
      "Voor het tekenen van tekst, afbeeldingen en beelden is er een set van klassen die vergelĳkbaar zĳn " +
      "gemodelleerd naar de klassen van de naam van de ruimte System.Drawing. NET framework. Met deze klassen " +
      "is het niet alleen mogelĳk te maken de inhoud van de PDF-pagina's op een eenvoudige manier, maar ze kunnen " +
      "ook gebruikt worden op te stellen in een venster of op een printer.\n" +
      "Daarnaast PDFsharp volledig modellen van de structuur elementen PDF is gebaseerd. Met hen bestaande PDF-documenten " +
      "kan worden gewĳzigd, samengevoegd of gesplitst met gemak.\n" +
      "De broncode van PDFsharp is open source onder de MIT-licentie (http://nl.wikipedia.org/wiki/MIT-licentie). " +
      "Daarom is het mogelĳk om PDFsharp gebruiken zonder beperkingen in niet open source of commerciële projecten / producten.",

      // Your language may come here
      "Invitation\n" +
      "If you use PDFsharp and haven't found your native language in this document, we will be pleased to get your translation of the text above and include it here.\n" +
      "Mail to PDFsharpSupport@pdfsharp.de"


      // The current implementation of PDFsharp is limited to left-to-right languages.
      // Languages like Arabic cannot yet be created even with Unicode fonts. Also the so called
      // CJK (Chinese, Japanese, Korean) support in PDF can also not be addressed with PDF sharp.
      // However, we plan to support as much as possible languages with PDFsharp. If you are a 
      // programmer and a native speaker of one of these languages and you like to create PDF
      // documents in your language, you can help us to implement it in PDFsharp. You don't have
      // to do the programming, but just help us to verify our implementation.
    };
  }
}
/*
PDFsharp is a .NET library for creating and processing PDF documents 'on the fly'.
The library is completely written in C# and based exclusively on safe, managed code.
PDFsharp offers two powerful abstraction levels to create and process PDF documents.
For drawing text, graphics, and images there is a set of classes which are modeled similar to the classes
of the name space System.Drawing of the .NET framework. With these classes it is not only possible to create
the content of PDF pages in an easy way, but they can also be used to draw in a window or on a printer.
Additionally PDFsharp completely models the structure elements PDF is based on. With them existing PDF documents
can be modified, merged, or split with ease.
The source code of PDFsharp is Open Source under the MIT license (http://en.wikipedia.org/wiki/MIT_License).
Therefore it is possible to use PDFsharp without limitations in non open source or commercial projects/products.
*/
