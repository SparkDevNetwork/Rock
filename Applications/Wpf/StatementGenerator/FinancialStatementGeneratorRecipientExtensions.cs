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
using System.IO;
using System.Text.RegularExpressions;

using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

using Rock.Client;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    internal static class FinancialStatementGeneratorRecipientExtensions
    {
        /// <summary>
        /// Gets the PDF document.
        /// </summary>
        /// <param name="financialStatementGeneratorRecipientResult">The financial statement generator recipient result.</param>
        /// <param name="reportRockStatementGeneratorStatementsTemporaryDirectory">The report rock statement generator statements temporary directory.</param>
        /// <returns></returns>
        internal static PdfDocument GetPdfDocument( this FinancialStatementGeneratorRecipient recipient, string reportRockStatementGeneratorStatementsTemporaryDirectory )
        {
            var filePath = recipient.GetPdfDocumentFilePath( reportRockStatementGeneratorStatementsTemporaryDirectory );
            if ( File.Exists( filePath ) )
            {
                return PdfReader.Open( filePath, PdfDocumentOpenMode.Import );
            }

            return null;
        }

        /// <summary>
        /// PDFs the file exists.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="reportRockStatementGeneratorStatementsTemporaryDirectory">The report rock statement generator statements temporary directory.</param>
        /// <returns></returns>
        internal static bool PdfFileExists( this FinancialStatementGeneratorRecipient recipient, string reportRockStatementGeneratorStatementsTemporaryDirectory )
        {
            var filePath = recipient.GetPdfDocumentFilePath( reportRockStatementGeneratorStatementsTemporaryDirectory );
            if ( File.Exists( filePath ) && PdfReader.TestPdfFile( filePath ) != 0 )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the recipient key.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        internal static string GetRecipientKey( this FinancialStatementGeneratorRecipient recipient )
        {
            return $"GroupId_{recipient.GroupId}_PersonID_{recipient.PersonId}";
        }

        /// <summary>
        /// Gets the PDF document file path.
        /// </summary>
        /// <param name="financialStatementGeneratorRecipientResult">The financial statement generator recipient result.</param>
        /// <param name="currentDayTemporaryDirectory">The current day temporary directory.</param>
        /// <returns></returns>
        internal static string GetPdfDocumentFilePath( this FinancialStatementGeneratorRecipient recipient, string currentDayTemporaryDirectory )
        {
            string pdfTempFileName = $"{GetRecipientKey( recipient )}.pdf";
            return Path.Combine( currentDayTemporaryDirectory, "Statements", pdfTempFileName );
        }

        /// <summary>
        /// Return just the first part of the zip code
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        internal static string GetFiveDigitPostalCode( this FinancialStatementGeneratorRecipient recipient )
        {
            // return just the first part of the zip code
            // 85013-4567 => 85013
            // 85013-1234 => 85013
            // 85083 => 85083
            // 60540654-1234 => 60540654 (unexpected length, so just grab the first number)
            // null => "00000"

            var firstFive = Regex.Match( recipient?.PostalCode ?? "00000", @"\d+" ).Value;
            return firstFive;
        }
    }
}
