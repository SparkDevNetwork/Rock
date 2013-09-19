#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@pdfsharp.com)
//   Klaus Potzesny (mailto:Klaus.Potzesny@pdfsharp.com)
//   David Stephensen (mailto:David.Stephensen@pdfsharp.com)
//
// Copyright (c) 2001-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://www.migradoc.com
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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using MigraDoc.DocumentObjectModel.Internals;

namespace MigraDoc.DocumentObjectModel
{
  /// <summary>
  /// String resources of MigraDoc.DocumentObjectModel. Provides all localized text strings
  /// for this assembly.
  /// </summary>
  static class DomSR
  {
    /// <summary>
    /// Loads the message from the resource associated with the enum type and formats it
    /// using 'String.Format'. Because this function is intended to be used during error
    /// handling it never raises an exception.
    /// </summary>
    /// <param name="id">The type of the parameter identifies the resource
    /// and the name of the enum identifies the message in the resource.</param>
    /// <param name="args">Parameters passed through 'String.Format'.</param>
    /// <returns>The formatted message.</returns>
    public static string FormatMessage(DomMsgID id, params object[] args)
    {
      string message;
      try
      {
        message = DomSR.GetString(id);
        if (message != null)
        {
#if DEBUG
          if (Regex.Matches(message, @"\{[0-9]\}").Count > args.Length)
          {
            //TODO too many placeholders or too less args...
          }
#endif
          message = String.Format(message, args);
        }
        else
          message = "<<<error: message not found>>>";
        return message;
      }
      catch (Exception ex)
      {
        message = "INTERNAL ERROR while formatting error message: " + ex.ToString();
      }
      return message;
    }

    public static string CompareJustCells
    {
      get
      {
        return "Only cells can be compared by this Comparer.";
      }
    }

    /// <summary>
    /// Gets the localized message identified by the specified DomMsgID.
    /// </summary>
    public static string GetString(DomMsgID id)
    {
      return DomSR.ResMngr.GetString(id.ToString());
    }
    #region How to use
#if true_
    // Message with no parameter is property.
    public static string SampleMessage1
    {
      // In the first place English only
      get { return "This is sample message 1."; }
    }

    // Message with no parameter is property.
    public static string SampleMessage2
    {
      // Then localized:
      get { return DomSR.GetString(DomMsgID.SampleMessage1); }
    }

    // Message with parameters is function.
    public static string SampleMessage3(string parm)
    {
      // In the first place English only
      //return String.Format("This is sample message 2: {0}.", parm);
    }
    public static string SampleMessage4(string parm)
    {
      // Then localized:
      return String.Format(GetString(DomMsgID.SampleMessage2), parm);
    }
#endif
    #endregion

    #region General Messages

    public static string StyleExpected
    {
      get { return DomSR.GetString(DomMsgID.StyleExpected); }
    }

    public static string BaseStyleRequired
    {
      get { return DomSR.GetString(DomMsgID.BaseStyleRequired); }
    }

    public static string EmptyBaseStyle
    {
      get { return DomSR.GetString(DomMsgID.EmptyBaseStyle); }
    }

    public static string InvalidFieldFormat(string format)
    {
      return DomSR.FormatMessage(DomMsgID.InvalidFieldFormat, format);
    }

    public static string InvalidInfoFieldName(string name)
    {
      return DomSR.FormatMessage(DomMsgID.InvalidInfoFieldName, name);
    }

    public static string UndefinedBaseStyle(string baseStyle)
    {
      return DomSR.FormatMessage(DomMsgID.UndefinedBaseStyle, baseStyle);
    }

    public static string InvalidValueName(string name)
    {
      return DomSR.FormatMessage(DomMsgID.InvalidValueName, name);
    }

    public static string InvalidUnitValue(string unitValue)
    {
      return DomSR.FormatMessage(DomMsgID.InvalidUnitValue, unitValue);
    }

    public static string InvalidUnitType(string unitType)
    {
      return DomSR.FormatMessage(DomMsgID.InvalidUnitType, unitType);
    }

    public static string InvalidEnumForLeftPosition
    {
      get { return DomSR.GetString(DomMsgID.InvalidEnumForLeftPosition); }
    }

    public static string InvalidEnumForTopPosition
    {
      get { return DomSR.GetString(DomMsgID.InvalidEnumForTopPosition); }
    }

    public static string InvalidColorString(string colorString)
    {
      return DomSR.FormatMessage(DomMsgID.InvalidColorString, colorString);
    }

    public static string InvalidFontSize(double value)
    {
      return DomSR.FormatMessage(DomMsgID.InvalidFontSize, value);
    }

    public static string InsertNullNotAllowed()
    {
      return "Insert null not allowed.";
    }

    public static string ParentAlreadySet(DocumentObject value, DocumentObject docObject)
    {
      return String.Format("Value of type '{0}' must be cloned before set into '{1}'.",
        value.GetType().ToString(), docObject.GetType().ToString());
    }

    public static string MissingObligatoryProperty(string propertyName, string className)
    {
      return String.Format("ObigatoryProperty '{0}' not set in '{1}'.", propertyName, className);
    }

    public static string InvalidDocumentObjectType
    {
      get
      {
        return "The given document object is not valid in this context.";
      }
    }
    #endregion

    #region DdlReader Messages

    #endregion

    #region Resource Manager

    public static ResourceManager ResMngr
    {
      get
      {
        if (DomSR.resmngr == null)
          DomSR.resmngr = new ResourceManager("MigraDoc.DocumentObjectModel.Resources.Messages",
            Assembly.GetExecutingAssembly());
        return DomSR.resmngr;
      }
    }

    /// <summary>
    /// Writes all messages defined by DomMsgID.
    /// </summary>
    [Conditional("DEBUG")]
    public static void TestResourceMessages()
    {
      string[] names = Enum.GetNames(typeof(DomMsgID));
      foreach (string name in names)
      {
        string message = String.Format("{0}: '{1}'", name, ResMngr.GetString(name));
        Debug.WriteLine(message);
      }
    }
    static ResourceManager resmngr;

    #endregion
  }
}