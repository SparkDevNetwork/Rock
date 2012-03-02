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
	/// Represents the different view states of the worksheet
	/// </summary>
	public class ExcelWorksheetView
	{
		// TODO: implement the different view states of the worksheet
		private ExcelWorksheet _xlWorksheet;
		private XmlElement _sheetView;

		#region ExcelWorksheetView Constructor
		/// <summary>
		/// Creates a new ExcelWorksheetView which provides access to all the 
		/// view states of the worksheet.
		/// </summary>
		/// <param name="xlWorksheet"></param>
		protected internal ExcelWorksheetView(ExcelWorksheet xlWorksheet)
		{
			_xlWorksheet = xlWorksheet;
		}
		#endregion

		#region SheetViewElement
		/// <summary>
		/// Returns a reference to the sheetView element
		/// </summary>
		protected internal XmlElement SheetViewElement
		{
			get 
			{
				if (_sheetView == null)
				{
					_sheetView = (XmlElement)_xlWorksheet.WorksheetXml.SelectSingleNode("//d:sheetView", _xlWorksheet.NameSpaceManager);
				}
				return _sheetView;
			}
		}
		#endregion

		#region TabSelected
		/// <summary>
		/// Indicates if the worksheet is selected within the workbook
		/// </summary>
		public bool TabSelected
		{
			get
			{
				bool retValue = false;
				string ret = SheetViewElement.GetAttribute("tabSelected");
				if (ret == "1") retValue = true;
				return retValue;
			}
			set
			{
				// the sheetView node should always exist, so no need to create
				if (value)
				{
					// ensure no other worksheet has its tabSelected attribute set to 1
					foreach (ExcelWorksheet sheet in _xlWorksheet.xlPackage.Workbook.Worksheets)
						sheet.View.TabSelected = false;

					SheetViewElement.SetAttribute("tabSelected", "1");
				}
				else
					SheetViewElement.SetAttribute("tabSelected", "0");
			}
		}
		#endregion

		#region PageLayoutView
		/// <summary>
		/// Sets the view mode of the worksheet to pageLayout
		/// </summary>
		public bool PageLayoutView
		{
			get
			{
				bool retValue = false;
				string ret = SheetViewElement.GetAttribute("view");
				if (ret == "pageLayout") retValue = true;
				return retValue;
			}
			set
			{
				if (value)
					SheetViewElement.SetAttribute("view", "pageLayout");
				else
					SheetViewElement.RemoveAttribute("view");
			}
		}
		#endregion
	}
}
