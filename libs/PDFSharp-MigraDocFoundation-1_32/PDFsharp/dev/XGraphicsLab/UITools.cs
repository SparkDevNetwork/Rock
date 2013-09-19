/*
   This is a part of the empira Application Framework.

   Copyright (c) 2002-2008 empira Software GmbH
   All rights reserved


 Abstract

   Class Efw.UserInterface.UITools. A collection of functions that calls Windows 
   API directly to do some stuff not possible with WinForms.
*/

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace XDrawing.TestLab
{
  /// <summary>
  /// Excerpt from the empira FrameWork of some useful functions to make WinForm windows more Windows conform.
  /// </summary>
  public sealed class UITools
  {
    UITools() {}

    /// <summary>
    /// Gives a dialog window the style WS_THICKFRAME. This makes a dialog sizeable
    /// in combination with WS_EX_DLGMODALFRAME. In effect the window is sizeabe, has
    /// a dialog frame, a system menu, a close box, but no system menu button.
    /// </summary>
    public static void MakeDialogSizable(Form form)
    {
      // LexiROM says it is 'sizeable'??
      // MSDN search hits: sizeable 19, sizable 90 
      // From MSDN documentation of AccessibleStates:
      //   Sizeable   A sizable object. 

      // TODO use create params instead of this hack...
      if (form != null)
      {
        int ws = GetWindowLong(form.Handle, GWL_STYLE);
        ws = ws | 0x00040000;
        SetWindowLong(form.Handle, GWL_STYLE, ws);
        // Force a WM_NCCALCSIZE
        //User32.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED);
        form.Invalidate();
      }
    }

    /// <summary>
    /// Removes unused items from the system menu of a dialog window. In a dialog
    /// window (that is not sizeable) the items Resore, Move, Size, Maximize and
    /// the separator have to be removed.
    /// </summary>
    public static void MakeUpDialogSysMenu(Control control)
    {
      //const int MF_BYCOMMAND  = 0x00000000;
      const int MF_BYPOSITION = 0x00000400;

      IntPtr menu = GetSystemMenu(control.Handle, false);
      if (menu != IntPtr.Zero)
      {
        DeleteMenu(menu, 5, MF_BYPOSITION);
        DeleteMenu(menu, 4, MF_BYPOSITION);
        DeleteMenu(menu, 3, MF_BYPOSITION);
        DeleteMenu(menu, 2, MF_BYPOSITION);
        DeleteMenu(menu, 0, MF_BYPOSITION);
      }
    }

    public static void SetTabPageColor(Control control)
    {
      if (control.Site != null && control.Site.DesignMode)
        return;
      if (XGraphicsLab.visualStyles)
        control.BackColor = UITools.tabPageColor;
    }
    static Color tabPageColor = Color.FromArgb(252, 252, 254);

    const int GWL_STYLE = -16;

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hwnd, int index, int val);

    [DllImport("user32.dll")]
    public static extern IntPtr GetSystemMenu(IntPtr hwnd, bool revert);

    [DllImport("user32.dll")]
    public static extern bool DeleteMenu(IntPtr hmenu, int position, int flags);
  }
}
