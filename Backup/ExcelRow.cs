/* 
 * You may amend and distribute as you like, but don't remove this header!
 * 
 * ExcelPackage provides server-side generation of Excel 2007 spreadsheets.
 * See http://www.codeplex.com/ExcelPackage for details.
 * 
 * Copyright 2007 © Dr John Tunnicliffe 
 * mailto:dr.john.tunnicliffe@btinternet.com
 * All rights reserved.
 * 
 * ExcelPackage is an Open Source project provided under the 
 * GNU General Public License (GPL) as published by the 
 * Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 * 
 * The GNU General Public License can be viewed at http://www.opensource.org/licenses/gpl-license.php
 * If you unfamiliar with this license or have questions about it, here is an http://www.gnu.org/licenses/gpl-faq.html
 * 
 * The code for this project may be used and redistributed by any means PROVIDING it is 
 * not sold for profit without the author's written consent, and providing that this notice 
 * and the author's name and all copyright notices remain intact.
 * 
 * All code and executables are provided "as is" with no warranty either express or implied. 
 * The author accepts no liability for any damage or loss of business that this product may cause.
 */

/*
 * Code change notes:
 * 
 * Author							Change						Date
 * ******************************************************************************
 * John Tunnicliffe		Initial Release		01-Jan-2007
 * ******************************************************************************
 */
using System;
using System.Xml;

namespace OfficeOpenXml
{
	/// <summary>
	/// Represents an individual row in the spreadsheet.
	/// </summary>
	public class ExcelRow
	{
		private ExcelWorksheet _xlWorksheet;
		private XmlElement _rowElement = null;

		#region ExcelRow Constructor
		/// <summary>
		/// Creates a new instance of the ExcelRow class. 
		/// For internal use only!
		/// </summary>
		/// <param name="Worksheet">The parent worksheet</param>
		/// <param name="row">The row number</param>
		protected internal ExcelRow(ExcelWorksheet Worksheet, int row)
		{
			_xlWorksheet = Worksheet;

			//  Search for the existing row
			_rowElement = (XmlElement) Worksheet.WorksheetXml.SelectSingleNode(string.Format("//d:sheetData/d:row[@r='{0}']", row), _xlWorksheet.NameSpaceManager);
			if (_rowElement == null)
			{
				// We didn't find the row, so add a new row element.
				// HOWEVER we MUST insert new row in the correct position - otherwise Excel 2007 will complain!!!
				_rowElement = Worksheet.WorksheetXml.CreateElement("row", ExcelPackage.schemaMain);
				_rowElement.SetAttribute("r", row.ToString());

				// now work out where to insert the new row
				XmlNode sheetDataNode = Worksheet.WorksheetXml.SelectSingleNode("//d:sheetData", _xlWorksheet.NameSpaceManager);
				if (sheetDataNode != null)
				{
					XmlNode followingRow = null;
					foreach (XmlNode currentRow in Worksheet.WorksheetXml.SelectNodes("//d:sheetData/d:row", _xlWorksheet.NameSpaceManager))
					{
						int rowFound = Convert.ToInt32(currentRow.Attributes.GetNamedItem("r").Value);
						if (rowFound > row)
						{
							followingRow = currentRow;
							break;
						}
					}
					if (followingRow == null)
						// no data rows exist, so just add row
						sheetDataNode.AppendChild(_rowElement);
					else
						sheetDataNode.InsertBefore(_rowElement, followingRow);
				}
			}
		}
		#endregion

		/// <summary>
		/// Provides access to the node representing the row.
		/// For internal use only!
		/// </summary>
		protected internal XmlNode Node { get { return (_rowElement); } }

		#region ExcelRow Hidden
		/// <summary>
		/// Allows the row to be hidden in the worksheet
		/// </summary>
		public bool Hidden
		{
			get
			{
				bool retValue = false;
				string hidden = _rowElement.GetAttribute("hidden", "1");
				if (hidden == "1") retValue = true;
				return (retValue);
			}
			set
			{
				if (value)
					_rowElement.SetAttribute("hidden", "1");
				else
					_rowElement.SetAttribute("hidden", "0");
			}
		}
		#endregion

		#region ExcelRow Height
		/// <summary>
		/// Sets the height of the row
		/// </summary>
		public double Height
		{
			get
			{
				double retValue = 10;  // default row height
				string ht = _rowElement.GetAttribute("ht");
				if (ht != "")
				{
					retValue = double.Parse(ht);
				}
				return (retValue);
			}
			set
			{
				_rowElement.SetAttribute("ht", value.ToString());
				// we must set customHeight="1" for the height setting to take effect
				_rowElement.SetAttribute("customHeight", "1");
			}
		}
		#endregion

		#region ExcelRow Style
		/// <summary>
		/// Gets/sets the style name based on the StyleID
		/// </summary>
		public string Style
		{
			get { return _xlWorksheet.GetStyleName(StyleID); }
			set { StyleID = _xlWorksheet.GetStyleID(value); }
		}

		/// <summary>
		/// Sets the style for the entire row using the style ID.  
		/// </summary>
		public int StyleID
		{
			get
			{
				int retValue = 0;
				string sid = _rowElement.GetAttribute("s");
				if (sid != "") retValue = int.Parse(sid);
				return retValue; 
			}
			set	
			{
				_rowElement.SetAttribute("s", value.ToString());
				// to get Excel to apply this style we need to set customFormat="1"
				_rowElement.SetAttribute("customFormat", "1");
			}
		}
		#endregion

	}
}
