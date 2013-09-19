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

namespace MigraDoc.Rendering.Resources
{
  /// <summary>
  /// Provides diagnostic messages taken from the resources.
  /// </summary>
  internal static class Messages
  {
    internal static string NumberTooLargeForRoman(int number)
    {
      return FormatMessage(IDs.NumberTooLargeForRoman, number);
    }

    internal static string NumberTooLargeForLetters(int number)
    {
      return FormatMessage(IDs.NumberTooLargeForLetters, number);
    }

    internal static string DisplayEmptyImageSize
    {
      get { return FormatMessage(IDs.DisplayEmptyImageSize); }
    }

    internal static string DisplayImageFileNotFound
    {
      get { return FormatMessage(IDs.DisplayImageFileNotFound); }
    }

    internal static string DisplayInvalidImageType
    {
      get { return FormatMessage(IDs.DisplayInvalidImageType); }
    }

    internal static string DisplayImageNotRead
    {
      get { return FormatMessage(IDs.DisplayImageNotRead); }
    }

    internal static string PropertyNotSetBefore(string propertyName, string functionName)
    {
      return FormatMessage(IDs.PropertyNotSetBefore, propertyName, functionName);
    }

    internal static string BookmarkNotDefined(string bookmarkName)
    {
      return FormatMessage(IDs.BookmarkNotDefined, bookmarkName);
    }

    internal static string ImageNotFound(string imageName)
    {
      return FormatMessage(IDs.ImageNotFound, imageName);
    }

    internal static string InvalidImageType(string type)
    {
      return FormatMessage(IDs.InvalidImageType, type);
    }

    internal static string ImageNotReadable(string imageName, string innerException)
    {
      return FormatMessage(IDs.ImageNotReadable, imageName, innerException);
    }

    internal static string EmptyImageSize
    {
      get
      {
        return FormatMessage(IDs.EmptyImageSize);
      }
    }

    internal static string ObjectNotRenderable
    {
      get
      {
        return FormatMessage(IDs.ObjectNotRenderable);
      }
    }


    private enum IDs
    {
      PropertyNotSetBefore,
      BookmarkNotDefined,
      ImageNotFound,
      InvalidImageType,
      ImageNotReadable,
      EmptyImageSize,
      ObjectNotRenderable,
      NumberTooLargeForRoman,
      NumberTooLargeForLetters,
      DisplayEmptyImageSize,
      DisplayImageFileNotFound,
      DisplayInvalidImageType,
      DisplayImageNotRead
    }

    private static ResourceManager ResourceManager
    {
      get
      {
        if (resourceManager == null)
          resourceManager = new ResourceManager("MigraDoc.Rendering.Resources.Messages", Assembly.GetExecutingAssembly());
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
