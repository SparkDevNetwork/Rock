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
using System.IO;
using System.IO.Packaging;
using System.Configuration;

namespace OfficeOpenXml
{
	/// <summary>
	/// Represents an Excel worksheet and provides access to its properties and methods
	/// </summary>
	public class ExcelWorksheet
	{
		#region Worksheet Private Properties
		/// <summary>
		/// Temporary tag for all column numbers in the worksheet XML
		/// For internal use only!
		/// </summary>
		protected internal const string tempColumnNumberTag = "colNumber"; 
		/// <summary>
		/// Reference to the parent package
		/// For internal use only!
		/// </summary>
		protected internal ExcelPackage xlPackage;
		private Uri _worksheetUri;
		private string _name;
		private int _sheetID;
		private bool _hidden;
		private string _relationshipID;
		private XmlDocument _worksheetXml;
		private ExcelWorksheetView _sheetView;
		private ExcelHeaderFooter _headerFooter;
		private XmlNamespaceManager _nsManager;
		#endregion  // END Worksheet Private Properties

		#region ExcelWorksheet Constructor
		/// <summary>
		/// Creates a new instance of ExcelWorksheet class. 
		/// For internal use only!
		/// </summary>
		/// <param name="ParentXlPackage">Parent ExcelPackage object</param>
		/// <param name="RelationshipID">Package relationship ID</param>
		/// <param name="sheetName">Name of the new worksheet</param>
		/// <param name="uriWorksheet">Uri of the worksheet in the package</param>
		/// <param name="SheetID">The worksheet's ID in the workbook XML</param>
		/// <param name="Hide">Indicates if the worksheet is hidden</param>
		protected internal ExcelWorksheet(
			ExcelPackage ParentXlPackage,
			string RelationshipID,
			string sheetName,
			Uri uriWorksheet, 
			int SheetID, 
			bool Hide)
		{
			xlPackage = ParentXlPackage;
			_relationshipID = RelationshipID;
			_worksheetUri = uriWorksheet;
			_name = sheetName;
			//_type = Type;
			_sheetID = SheetID;
			Hidden = Hide;
		}
		#endregion

		#region Worksheet Public Properties
		/// <summary>
		/// Read-only: the Uri to the worksheet within the package
		/// </summary>
		protected internal Uri WorksheetUri { get { return (_worksheetUri); } }
		/// <summary>
		/// Read-only: a reference to the PackagePart for the worksheet within the package
		/// </summary>
		protected internal PackagePart Part { get { return (xlPackage.Package.GetPart(WorksheetUri)); } }
		/// <summary>
		/// Read-only: the ID for the worksheet's relationship with the workbook in the package
		/// </summary>
		protected internal string RelationshipID { get { return (_relationshipID); } }
		/// <summary>
		/// The unique identifier for the worksheet.  Note that these can be random, so not
		/// too useful in code!
		/// </summary>
		protected internal int SheetID { get { return (_sheetID); } }
		/// <summary>
		/// Provides access to a namespace manager instance to allow XPath searching
		/// </summary>
		public XmlNamespaceManager NameSpaceManager 
		{ 
			get 
			{
				if (_nsManager == null)
				{
					NameTable nt = new NameTable();
					_nsManager = new XmlNamespaceManager(nt);
					_nsManager.AddNamespace("d", ExcelPackage.schemaMain);
				}
				return (_nsManager);
			}
		}
		/// <summary>
		/// Returns a ExcelWorksheetView object that allows you to
		/// set the view state properties of the worksheet
		/// </summary>
		public ExcelWorksheetView View
		{
			get
			{
				if (_sheetView == null)
				{
					_sheetView = new ExcelWorksheetView(this);
				}
				return (_sheetView);
			}
		}

		#region Name // Worksheet Name
		/// <summary>
		/// The worksheet's name as it appears on the tab
		/// </summary>
		public string Name
		{
			get { return (_name); }
			set
			{
				XmlNode sheetNode = xlPackage.Workbook.WorkbookXml.SelectSingleNode(string.Format("//d:sheet[@sheetId={0}]", _sheetID), NameSpaceManager);
				if (sheetNode != null)
				{
					XmlAttribute nameAttr = (XmlAttribute)sheetNode.Attributes.GetNamedItem("name");
					if (nameAttr != null)
					{
						nameAttr.Value = value;
					}
				}
				_name = value;
			}
		}
		#endregion // END Worksheet Name

		#region Hidden
		/// <summary>
		/// Indicates if the worksheet is hidden in the workbook
		/// </summary>
		public bool Hidden
		{
			get { return (_hidden); }
			set
			{
				XmlNode sheetNode = xlPackage.Workbook.WorkbookXml.SelectSingleNode(string.Format("//d:sheet[@sheetId={0}]", _sheetID), NameSpaceManager);
				if (sheetNode != null)
				{
					XmlAttribute nameAttr = (XmlAttribute)sheetNode.Attributes.GetNamedItem("hidden");
					if (nameAttr != null)
					{
						nameAttr.Value = value.ToString();
					}
				}
				_hidden = value;
			}
		}
		#endregion

		#region defaultRowHeight
		/// <summary>
		/// Allows you to get/set the default height of all rows in the worksheet
		/// </summary>
		public int defaultRowHeight
		{
			get 
			{ 
				int retValue = 15; // Excel's default height
				XmlElement sheetFormat = (XmlElement) WorksheetXml.SelectSingleNode("//d:sheetFormatPr", NameSpaceManager);
				if (sheetFormat != null)
				{
					string ret = sheetFormat.GetAttribute("defaultRowHeight");
					if (ret != "")
						retValue = int.Parse(ret);
				}
				return retValue;
			}
			set
			{
				XmlElement sheetFormat = (XmlElement) WorksheetXml.SelectSingleNode("//d:sheetFormatPr", NameSpaceManager);
				if (sheetFormat == null)
				{
					// create the node as it does not exist
					sheetFormat = WorksheetXml.CreateElement("sheetFormatPr", ExcelPackage.schemaMain);
					// find location to insert new element
					XmlNode sheetViews = WorksheetXml.SelectSingleNode("//d:sheetViews", NameSpaceManager);
					// insert the new node
					WorksheetXml.DocumentElement.InsertAfter(sheetFormat, sheetViews);
				}
				sheetFormat.SetAttribute("defaultRowHeight", value.ToString());
			}
		}
		#endregion

		#region WorksheetXml
		/// <summary>
		/// The XML document holding all the worksheet data.
		/// </summary>
		public XmlDocument WorksheetXml
		{
			get
			{
				if (_worksheetXml == null)
				{
					_worksheetXml = new XmlDocument();
					PackagePart packPart = xlPackage.Package.GetPart(WorksheetUri);
					_worksheetXml.Load(packPart.GetStream());
					// convert worksheet into the type of XML we like dealing with
					AddNumericCellIDs();
				}
				return (_worksheetXml);
			}
		}
		#endregion

		#region HeaderFooter
		/// <summary>
		/// A reference to the header and footer class which allows you to 
		/// set the header and footer for all odd, even and first pages of the worksheet
		/// </summary>
		public ExcelHeaderFooter HeaderFooter
		{
			get
			{
				if (_headerFooter == null)
				{
					XmlNode headerFooterNode = WorksheetXml.SelectSingleNode("//d:headerFooter", NameSpaceManager);
					if (headerFooterNode == null)
						headerFooterNode = WorksheetXml.DocumentElement.AppendChild(WorksheetXml.CreateElement("headerFooter", ExcelPackage.schemaMain));
					_headerFooter = new ExcelHeaderFooter((XmlElement)headerFooterNode);
				}
				return (_headerFooter);
			}
		}
		#endregion

		// TODO: implement freeze pane. 
		// TODO: implement page margin properties

		#endregion // END Worksheet Public Properties

		#region Worksheet Public Methods
		/// <summary>
		/// Provides access to an individual cell within the worksheet.
		/// </summary>
		/// <param name="row">The row number in the worksheet</param>
		/// <param name="col">The column number in the worksheet</param>
		/// <returns></returns>
		public ExcelCell Cell(int row, int col)
		{
			return (new ExcelCell(this, row, col));
		}

		/// <summary>
		/// Provides access to an individual row within the worksheet so you can set its properties.
		/// </summary>
		/// <param name="row">The row number in the worksheet</param>
		/// <returns></returns>
		public ExcelRow Row(int row)
		{
		  return (new ExcelRow(this, row));
		}

		/// <summary>
		/// Provides access to an individual column within the worksheet so you can set its properties.
		/// </summary>
		/// <param name="col">The column number in the worksheet</param>
		/// <returns></returns>
		public ExcelColumn Column(int col)
		{
			return (new ExcelColumn(this, col));
		}

		#region CreateSharedFormula
		/// <summary>
		/// Creates a shared formula based on the formula already in startCell
		/// Essentially this supports the formula attributes such as t="shared" ref="B2:B4" si="0"
		/// as per Brian Jones: Open XML Formats blog. See
		/// http://blogs.msdn.com/brian_jones/archive/2006/11/15/simple-spreadsheetml-file-part-2-of-3.aspx
		/// </summary>
		/// <param name="startCell">The cell containing the formula</param>
		/// <param name="endCell">The end cell (i.e. end of the range)</param>
		public void CreateSharedFormula(ExcelCell startCell, ExcelCell endCell)
		{
			XmlElement formulaElement;
			string formula = startCell.Formula;
			if (formula == "") throw new Exception("CreateSharedFormula Error: startCell does not contain a formula!");

			// find or create a shared formula ID
			int sharedID = -1;
			foreach (XmlNode node in _worksheetXml.SelectNodes("//d:sheetData/d:row/d:c/d:f/@si", NameSpaceManager))
			{
				int curID = int.Parse(node.Value);
				if (curID > sharedID)	sharedID = curID;
			}
			sharedID++;  // first value must be zero

			for (int row = startCell.Row; row <= endCell.Row; row++)
			{
				for (int col = startCell.Column; col <= endCell.Column; col++)
				{
					ExcelCell cell = Cell(row, col);
					
					// to force Excel to re-calculate the formula, we must remove the value
					cell.RemoveValue();

					formulaElement = (XmlElement)cell.Element.SelectSingleNode("./d:f", NameSpaceManager);
					if (formulaElement == null)
					{
						formulaElement = cell.AddFormulaElement();
					}
					formulaElement.SetAttribute("t", "shared");
					formulaElement.SetAttribute("si", sharedID.ToString());
				}
			}

			// finally add the shared cell range to the startCell
			formulaElement = (XmlElement) startCell.Element.SelectSingleNode("./d:f", NameSpaceManager);
			formulaElement.SetAttribute("ref", string.Format("{0}:{1}", startCell.CellAddress, endCell.CellAddress));
		}
		#endregion

		/// <summary>
		/// Inserts conditional formatting for the cell range.
		/// Currently only supports the dataBar style.
		/// </summary>
		/// <param name="startCell"></param>
		/// <param name="endCell"></param>
		/// <param name="color"></param>
		public void CreateConditionalFormatting(ExcelCell startCell, ExcelCell endCell, string color)
		{
			XmlNode formatNode = WorksheetXml.SelectSingleNode("//d:conditionalFormatting", NameSpaceManager);
			if (formatNode == null)
			{
				formatNode = WorksheetXml.CreateElement("conditionalFormatting", ExcelPackage.schemaMain);
				XmlNode prevNode = WorksheetXml.SelectSingleNode("//d:mergeCells", NameSpaceManager);
				if (prevNode == null)
					prevNode = WorksheetXml.SelectSingleNode("//d:sheetData", NameSpaceManager);
				WorksheetXml.DocumentElement.InsertAfter(formatNode, prevNode);
			}
			XmlAttribute attr = formatNode.Attributes["sqref"];
			if (attr == null)
			{
				attr = WorksheetXml.CreateAttribute("sqref");
				formatNode.Attributes.Append(attr);
			}
			attr.Value = string.Format("{0}:{1}", startCell.CellAddress, endCell.CellAddress);
			
			XmlNode node = formatNode.SelectSingleNode("./d:cfRule", NameSpaceManager);
			if (node == null)
			{
				node = WorksheetXml.CreateElement("cfRule", ExcelPackage.schemaMain);
				formatNode.AppendChild(node);
			}
			
			attr = node.Attributes["type"];
			if (attr == null)
			{
				attr = WorksheetXml.CreateAttribute("type");
				node.Attributes.Append(attr);
			}
			attr.Value = "dataBar";
			
			attr = node.Attributes["priority"];
			if (attr == null)
			{
				attr = WorksheetXml.CreateAttribute("priority");
				node.Attributes.Append(attr);
			}
			attr.Value = "1";

			// the following is poor code, but just an example!!!
			XmlNode databar = WorksheetXml.CreateElement("databar", ExcelPackage.schemaMain);
			node.AppendChild(databar);

			XmlNode child = WorksheetXml.CreateElement("cfvo", ExcelPackage.schemaMain);
			databar.AppendChild(child);
			attr = WorksheetXml.CreateAttribute("type");
			child.Attributes.Append(attr);
			attr.Value = "min";
			attr = WorksheetXml.CreateAttribute("val");
			child.Attributes.Append(attr);
			attr.Value = "0";

			child = WorksheetXml.CreateElement("cfvo", ExcelPackage.schemaMain);
			databar.AppendChild(child);
			attr = WorksheetXml.CreateAttribute("type");
			child.Attributes.Append(attr);
			attr.Value = "max";
			attr = WorksheetXml.CreateAttribute("val");
			child.Attributes.Append(attr);
			attr.Value = "0";

			child = WorksheetXml.CreateElement("color", ExcelPackage.schemaMain);
			databar.AppendChild(child);
			attr = WorksheetXml.CreateAttribute("rgb");
			child.Attributes.Append(attr);
			attr.Value = color;
		}

		#region InsertRow
		/// <summary>
		/// Inserts a new row into the spreadsheet.  Existing rows below the insersion position are 
		/// shifted down.  All formula are updated to take account of the new row.
		/// </summary>
		/// <param name="position">The position of the new row</param>
		public void InsertRow(int position)
		{
			XmlNode rowNode = null;
			// create the new row element
			XmlElement rowElement = WorksheetXml.CreateElement("row", ExcelPackage.schemaMain);
			rowElement.Attributes.Append(WorksheetXml.CreateAttribute("r"));
			rowElement.Attributes["r"].Value = position.ToString();

			XmlNode sheetDataNode = WorksheetXml.SelectSingleNode("//d:sheetData", NameSpaceManager);
			if (sheetDataNode != null)
			{
				int renumberFrom = 1;
				XmlNodeList nodes = sheetDataNode.ChildNodes;
				int nodeCount = nodes.Count;
				XmlNode insertAfterRowNode = null;
				int insertAfterRowNodeID = 0;
				for (int i = 0; i < nodeCount; i++)
				{
					int currentRowID = int.Parse(nodes[i].Attributes["r"].Value);
					if (currentRowID < position)
					{
						insertAfterRowNode = nodes[i];
						insertAfterRowNodeID = i;
					}
					if (currentRowID >= position)
					{
						renumberFrom = currentRowID;
						break;
					}
				}

				// update the existing row ids
				for (int i = insertAfterRowNodeID + 1; i < nodeCount; i++)
				{
					int currentRowID = int.Parse(nodes[i].Attributes["r"].Value);
					if (currentRowID >= renumberFrom)
					{
						nodes[i].Attributes["r"].Value = Convert.ToString(currentRowID + 1);

						// now update any formula that are in the row 
						XmlNodeList formulaNodes = nodes[i].SelectNodes("./d:c/d:f", NameSpaceManager);
						foreach (XmlNode formulaNode in formulaNodes)
						{
							formulaNode.InnerText = ExcelCell.UpdateFormulaReferences(formulaNode.InnerText, 1, 0, position, 0);
						}
					}
				}

				// now insert the new row
				if (insertAfterRowNode != null)
					rowNode = sheetDataNode.InsertAfter(rowElement, insertAfterRowNode);

			}
		}
		#endregion

		#region DeleteRow
		/// <summary>
		/// Deletes the specified row from the worksheet.
		/// If shiftOtherRowsUp=true then all formula are updated to take account of the deleted row.
		/// </summary>
		/// <param name="rowToDelete">The number of the row to be deleted</param>
		/// <param name="shiftOtherRowsUp">Set to true if you want the other rows renumbered so they all move up</param>
		public void DeleteRow(int rowToDelete, bool shiftOtherRowsUp)
		{
			XmlNode sheetDataNode = WorksheetXml.SelectSingleNode("//d:sheetData", NameSpaceManager);
			if (sheetDataNode != null)
			{
				XmlNodeList nodes = sheetDataNode.ChildNodes;
				int nodeCount = nodes.Count;
				int rowNodeID = 0;
				XmlNode rowNode = null;
				for (int i = 0; i < nodeCount; i++)
				{
					int currentRowID = int.Parse(nodes[i].Attributes["r"].Value);
					if (currentRowID == rowToDelete)
					{
						rowNodeID = i;
						rowNode = nodes[i];
					}
				}

				if (shiftOtherRowsUp)
				{
					// update the existing row ids
					for (int i = rowNodeID + 1; i < nodeCount; i++)
					{
						int currentRowID = int.Parse(nodes[i].Attributes["r"].Value);
						if (currentRowID > rowToDelete)
						{
							nodes[i].Attributes["r"].Value = Convert.ToString(currentRowID - 1);

							// now update any formula that are in the row 
							XmlNodeList formulaNodes = nodes[i].SelectNodes("./d:c/d:f", NameSpaceManager);
							foreach (XmlNode formulaNode in formulaNodes)
								formulaNode.InnerText = ExcelCell.UpdateFormulaReferences(formulaNode.InnerText, -1, 0, rowToDelete, 0);
						}
					}
				}
				// delete the row
				if (rowNode != null)
				{
					sheetDataNode.RemoveChild(rowNode);
				}
			}
		}
		#endregion

		#endregion // END Worksheet Public Methods

		#region Worksheet Private Methods

		#region Worksheet Save
		/// <summary>
		/// Saves the worksheet to the package.  For internal use only.
		/// </summary>
		protected internal void Save()  // Worksheet Save
		{
			#region Delete the printer settings component (if it exists)
			// we also need to delete the relationship from the pageSetup tag
			XmlNode pageSetup = _worksheetXml.SelectSingleNode("//d:pageSetup", NameSpaceManager);
			if (pageSetup != null)
			{
				XmlAttribute attr = (XmlAttribute)pageSetup.Attributes.GetNamedItem("id", ExcelPackage.schemaRelationships);
				if (attr != null)
				{
					string relID = attr.Value;
					// first delete the attribute from the XML
					pageSetup.Attributes.Remove(attr);

					// get the URI
					PackageRelationship relPrinterSettings = Part.GetRelationship(relID);
					Uri printerSettingsUri = new Uri("/xl" + relPrinterSettings.TargetUri.ToString().Replace("..", ""), UriKind.Relative);

					// now delete the relationship
					Part.DeleteRelationship(relPrinterSettings.Id);

					// now delete the part from the package
					xlPackage.Package.DeletePart(printerSettingsUri);
				}
			}
			#endregion

			if (_worksheetXml != null)
			{
				// save the header & footer (if defined)
				if (_headerFooter != null)
					HeaderFooter.Save();
				// replace the numeric Cell IDs we inserted with AddNumericCellIDs()
				ReplaceNumericCellIDs();

				// save worksheet to package
				PackagePart partPack = xlPackage.Package.GetPart(WorksheetUri);
				WorksheetXml.Save(partPack.GetStream(FileMode.Create, FileAccess.Write));
				xlPackage.WriteDebugFile(WorksheetXml, @"xl\worksheets", "sheet" + SheetID + ".xml");
			}
		}
		#endregion

		#region AddNumericCellIDs
		/// <summary>
		/// Adds numeric cell identifiers so that it is easier to work out position of cells
		/// Private method, for internal use only!
		/// </summary>
		private void AddNumericCellIDs()
		{
			// process each row
			foreach (XmlNode rowNode in WorksheetXml.SelectNodes("//d:sheetData/d:row", NameSpaceManager))
			{
				// remove the spans attribute.  Excel simply recreates it when the file is opened.
				XmlAttribute attr = (XmlAttribute)rowNode.Attributes.GetNamedItem("spans");
				if (attr != null)
					rowNode.Attributes.Remove(attr);

				int row = Convert.ToInt32(rowNode.Attributes.GetNamedItem("r").Value);
				// process each cell in current row
				foreach (XmlNode colNode in rowNode.SelectNodes("./d:c", NameSpaceManager))
				{
					XmlAttribute cellAddressAttr = (XmlAttribute)colNode.Attributes.GetNamedItem("r");
					if (cellAddressAttr != null)
					{
						string cellAddress = cellAddressAttr.Value;

						int col = ExcelCell.GetColumnNumber(cellAddress);
						attr = WorksheetXml.CreateAttribute(tempColumnNumberTag);
						if (attr != null)
						{
							attr.Value = col.ToString();
							colNode.Attributes.Append(attr);
							// remove all cell Addresses like A1, A2, A3 etc.
							colNode.Attributes.Remove(cellAddressAttr);
						}
					}
				}
			}
		}
		#endregion

		#region ReplaceNumericCellIDs
		/// <summary>
		/// Replaces the numeric cell identifiers we inserted with AddNumericCellIDs with the traditional 
		/// A1, A2 cell identifiers that Excel understands.
		/// Private method, for internal use only!
		/// </summary>
		private void ReplaceNumericCellIDs()
		{
			int maxColumn = 0;
			// process each row
			foreach (XmlNode rowNode in WorksheetXml.SelectNodes("//d:sheetData/d:row", NameSpaceManager))
			{
				int row = Convert.ToInt32(rowNode.Attributes.GetNamedItem("r").Value);
				int count = 0;
				// process each cell in current row
				foreach (XmlNode colNode in rowNode.SelectNodes("./d:c", NameSpaceManager))
				{
					XmlAttribute colNumber = (XmlAttribute)colNode.Attributes.GetNamedItem(tempColumnNumberTag);
					if (colNumber != null)
					{
						count++;
						if (count > maxColumn) maxColumn = count;
						int col = Convert.ToInt32(colNumber.Value);
						string cellAddress = ExcelCell.GetColumnLetter(col) + row.ToString();
						XmlAttribute attr = WorksheetXml.CreateAttribute("r");
						if (attr != null)
						{
							attr.Value = cellAddress;
							// the cellAddress needs to be the first attribute, otherwise Excel complains
							if (colNode.Attributes.Count == 0)
								colNode.Attributes.Append(attr);
							else
							{
								colNode.Attributes.InsertBefore(attr, (XmlAttribute)colNode.Attributes.Item(0));
							}
						}
						// remove all numeric cell addresses added by AddNumericCellIDs
						colNode.Attributes.Remove(colNumber);
					}
				}
			}

			// process each row and add the spans attribute
			// TODO: Need to add proper spans handling.
			//foreach (XmlNode rowNode in XmlDoc.SelectNodes("//d:sheetData/d:row", NameSpaceManager))
			//{
			//  // we must add or update the "spans" attribute of each row
			//  XmlAttribute spans = (XmlAttribute)rowNode.Attributes.GetNamedItem("spans");
			//  if (spans == null)
			//  {
			//    spans = XmlDoc.CreateAttribute("spans");
			//    rowNode.Attributes.Append(spans);
			//  }
			//  spans.Value = "1:" + maxColumn.ToString();
			//}
		}
		#endregion

		#region Get Style Information
		/// <summary>
		/// Returns the name of the style using its xfId
		/// </summary>
		/// <param name="StyleID">The xfId of the style</param>
		/// <returns>The name of the style</returns>
		protected internal string GetStyleName(int StyleID)
		{
			string retValue = null;
			XmlNode styleNode = null;
			int count = 0;
			foreach (XmlNode node in xlPackage.Workbook.StylesXml.SelectNodes("//d:cellXfs/d:xf", NameSpaceManager))
			{
				if (count == StyleID)
				{
					styleNode = node;
					break;
				}
				count++;
			}

			if (styleNode != null)
			{
				string searchString = string.Format("//d:cellStyle[@xfId = '{0}']", styleNode.Attributes["xfId"].Value);
				XmlNode styleNameNode = xlPackage.Workbook.StylesXml.SelectSingleNode(searchString, NameSpaceManager);
				if (styleNameNode != null)
				{
					retValue = styleNameNode.Attributes["name"].Value;
				}
			}

			return retValue;
		}

		/// <summary>
		/// Returns the style ID given a style name.  
		/// The style ID will be created if not found, but only if the style name exists!
		/// </summary>
		/// <param name="StyleName"></param>
		/// <returns></returns>
		protected internal int GetStyleID(string StyleName)
		{
			int styleID = 0;
			// find the named style in the style sheet
			string searchString = string.Format("//d:cellStyle[@name = '{0}']", StyleName);
			XmlNode styleNameNode = xlPackage.Workbook.StylesXml.SelectSingleNode(searchString, NameSpaceManager);
			if (styleNameNode != null)
			{
				string xfId = styleNameNode.Attributes["xfId"].Value;
				// look up position of style in the cellXfs 
				searchString = string.Format("//d:cellXfs/d:xf[@xfId = '{0}']", xfId);
				XmlNode styleNode = xlPackage.Workbook.StylesXml.SelectSingleNode(searchString, NameSpaceManager);
				if (styleNode != null)
				{
					XmlNodeList nodes = styleNode.SelectNodes("preceding-sibling::d:xf", NameSpaceManager);
					if (nodes != null)
						styleID = nodes.Count;
				}
			}
			return styleID;
		}
		#endregion
		
		#endregion  // END Worksheet Private Methods
	}  // END class Worksheet
}
