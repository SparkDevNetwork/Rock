#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Klaus Potzesny (mailto:Klaus.Potzesny@pdfsharp.com)
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
using System.Resources;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MigraDoc.RtfRendering.Resources
{
  /// <summary>
  /// Provides diagnostic messages taken from the resources.
  /// </summary>
  internal class Messages
  {
    internal static string TextframeContentsNotTurned
    {
      get
      {
        return FormatMessage(IDs.TextframeContentsNotTurned);
      }
    }

    internal static string ImageFreelyPlacedInWrongContext(string imageName)
    {
      return FormatMessage(IDs.ImageFreelyPlacedInWrongContext, imageName);
    }

    internal static string ChartFreelyPlacedInWrongContext
    {
      get
      {
        return FormatMessage(IDs.ChartFreelyPlacedInWrongContext);
      }
    }

    internal static string ImageNotFound(string imageName)
    {
      return FormatMessage(IDs.ImageNotFound, imageName);
    }

    internal static string ImageNotReadable(string imageName, string innerException)
    {
      return FormatMessage(IDs.ImageNotReadable, imageName, innerException);
    }

    internal static string ImageTypeNotSupported(string imageName)
    {
      return FormatMessage(IDs.ImageTypeNotSupported, imageName);
    }

    internal static string InvalidNumericFieldFormat(string format)
    {
      return FormatMessage(IDs.InvalidNumericFieldFormat, format);
    }

    internal static string CharacterNotAllowedInDateFormat(char character)
    {
      string charString = "";
      charString += character;
      return FormatMessage(IDs.CharacterNotAllowedInDateFormat, charString);
    }

    internal static string UpdateField
    {
      get
      {
        return FormatMessage(IDs.UpdateField);
      }
    }

    private enum IDs
    {
      UpdateField,
      TextframeContentsNotTurned,
      InvalidNumericFieldFormat,
      ImageFreelyPlacedInWrongContext,
      ChartFreelyPlacedInWrongContext,
      ImageNotFound,
      ImageNotReadable,
      ImageTypeNotSupported,
      CharacterNotAllowedInDateFormat
    }

    private static ResourceManager ResourceManager
    {
      get
      {
        if (resourceManager == null)
          resourceManager = new ResourceManager("MigraDoc.RtfRendering.Resources.Messages", Assembly.GetExecutingAssembly());

        return resourceManager;
      }
    }
    private static ResourceManager resourceManager;


    private static string FormatMessage(IDs id, params object[] args)
    {
      string message;
      try
      {
        message = ResourceManager.GetString(id.ToString());
        if (message != null)
        {
#if DEBUG
          if (Regex.Matches(message, @"\{[0-9]\}").Count > args.Length)
          {
            //TODO too many placeholders or too few args...
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
  }
}
