using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Explorer
{
  /// <summary>
  /// 
  /// </summary>
  public class ExplorerProcess
  {
    public ExplorerProcess()
    {
    }

    public void OpenDocument(string path)
    {
      this.path = path;
      try
      {
        this.filename = Path.GetFullPath(path);
        this.document = PdfReader.Open(this.filename, PdfDocumentOpenMode.Modify, new PdfPasswordProvider(ProvidePassword));
      }
      finally
      {
      }
    }
    string path;

    // HACK: should not create a form in a process object
    void ProvidePassword(PdfPasswordProviderArgs args)
    {
      using (PasswordForm form = new PasswordForm(this.path))
      {
        switch (form.ShowDialog())
        {
          case DialogResult.OK:
            args.Password = form.Password;
            break;

          case DialogResult.Cancel:
            args.Abort = true;
            break;
        }
      }
    }

    /// <summary>
    /// Converts a PDF file in a more readeable text layout.
    /// </summary>
    public void FormatDocument(string path)
    {
      this.path = path;
      try
      {
        PdfDocument pdfDocument = PdfReader.Open(path, PdfDocumentOpenMode.Modify, ProvidePassword);
#if true
        PdfObject[] objs = pdfDocument.Internals.GetAllObjects();
        for (int idx = 0; idx < objs.Length; idx++)
        {
          PdfDictionary dict = objs[idx] as PdfDictionary;
          if (dict != null && dict.Stream != null)
          {
            dict.Stream.TryUnfilter();
            // PdfSharp.Toolbox.PdfLib.ZzzPsgiolePfrp(dict.Stream);
          }
        }
#else
        PdfPages pages = document.Pages;
        int count = pages.Count;
        for (int idx = 0; idx < count; idx++)
        {
          //pages[0].Contents.Content.Deflate();
        }
#endif
        string name = Path.GetFileName(path);
        path = Path.GetDirectoryName(path);
        name = Path.GetFileNameWithoutExtension(name) + "_" + ".pdf";
        path = Path.Combine(path, name);

        pdfDocument.Save(path);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString(), VersionInfo.Title);
      }
    }

    //public bool IsMetric = false; //RegionInfo.CurrentRegion.IsMetric;
    public bool IsMetric = RegionInfo.CurrentRegion.IsMetric;

    public string Filename
    {
      get {return this.filename;}
    }
    string filename;

    public PdfDocument Document
    {
      get {return this.document;}
    }
    PdfDocument document;


    public static string GetTypeName(PdfObject obj)
    {
      if (obj is PdfDictionary)
        return "dictionary";
      if (obj is PdfArray)
        return "array";
      if (obj is PdfBooleanObject)
        return "boolean";
      if (obj is PdfIntegerObject)
        return "integer";
      if (obj is PdfRealObject)
        return "real";
      if (obj is PdfStringObject)
        return "string";
      if (obj is PdfNameObject)
        return "name";
      if (obj is PdfNullObject)
        return "null";

      throw new NotImplementedException("TODO: " + obj.GetType().FullName);
    }

    public ItemNavigator Navigator
    {
      get
      {
        if (this.navigator == null)
          this.navigator = new ItemNavigator(this);
        return this.navigator;
      }
    }
    ItemNavigator navigator;

    public class ItemNavigator
    {
      internal ItemNavigator(ExplorerProcess explorer)
      {
        this.explorer = explorer;
      }
      ExplorerProcess explorer;

      public void Reset(PdfItem value)
      {
        this.items.Clear();
        this.cursor = 0;
        this.top = 0;
        SetNext(value);
      }

      public PdfItem Current
      {
        get
        {
          if (this.cursor == 0)
            throw new InvalidOperationException("No current item.");
          return (PdfItem)this.items[this.cursor - 1];
        }
      }

      public void SetNext(PdfItem value)
      {
        if (this.items.Count <= this.cursor)
          this.items.Add(null);
        this.items[this.cursor++] = value;
        this.top = cursor;
      }

      public bool CanMoveForward
      {
        get { return this.cursor < this.top; }
      }

      public PdfItem MoveForward()
      {
        if (this.cursor < this.top)
          throw new InvalidOperationException("No next item.");
        return (PdfItem)this.items[this.cursor++];
      }

      public bool CanMoveBack
      {
        get { return this.cursor > 1; }
      }

      public PdfItem MoveBack()
      {
        if (this.cursor > 1)
          throw new InvalidOperationException("No previous item.");
        return (PdfItem)this.items[--this.cursor - 1];
      }

      ArrayList items = new ArrayList();
      int cursor;
      int top;
    }
  }
}
