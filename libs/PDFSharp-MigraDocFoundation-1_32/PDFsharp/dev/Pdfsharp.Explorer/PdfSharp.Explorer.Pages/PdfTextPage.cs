using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using PdfSharp.Pdf;

namespace PdfSharp.Explorer.Pages
{
  /// <summary>
  /// Base class for MainObjectsPage and MainPagesPage
  /// </summary>
  public class PdfTextPage : PageBase
  {
    private System.ComponentModel.Container components = null;

    public PdfTextPage(ExplorerPanel explorer)
    {
      this.explorer = explorer;
      InitializeComponent();
      //explorer.lvPages.SelectedIndexChanged += new System.EventHandler(this.lvPages_SelectedIndexChanged);
    }

    private System.Windows.Forms.CheckBox btnClosure;
    private System.Windows.Forms.RichTextBox tbxPdf;
    private System.Windows.Forms.Label txtClosure;
    protected ExplorerPanel explorer;

//    internal virtual void UpdateDocument()
//    {
//    }
//
    internal override void SetObject(PdfItem item)
    {
      if (item != null)
      {
        MemoryStream memoryStream = new MemoryStream();
        PdfItem[] items;
        if (item is PdfObject && this.btnClosure.Checked)
        {
          PdfObject[] objects = this.explorer.Process.Document.Internals.GetClosure((PdfObject)item);
          items = new PdfItem[objects.Length];
          objects.CopyTo(items, 0);
          this.txtClosure.Text = String.Format("Closure contains {0} elements.", objects.Length);
        }
        else
        {
          this.txtClosure.Text = "";
          items = new PdfItem[1]{item};
        }
      
        for (int idx = 0; idx < items.Length; idx++)
          this.explorer.Process.Document.Internals.WriteObject(memoryStream, items[idx]);
      
        int count = (int)memoryStream.Length;
        byte[] bytes = new byte[count];
        memoryStream.Position = 0;
        memoryStream.Read(bytes, 0, count);
        char[] chars = new char[count];
      
        // Keep in mind that a PDF stream has no intrinsic encoding.
        for (int idx = 0; idx < count; idx++)
        {
          byte b = bytes[idx];
          if (b == 0)
            chars[idx] = '?';
          else
            chars[idx] = (char)bytes[idx];
        }
        string pdf = new string(chars);
        this.tbxPdf.Text = pdf;
      }
    }

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
          components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btnClosure = new System.Windows.Forms.CheckBox();
      this.tbxPdf = new System.Windows.Forms.RichTextBox();
      this.txtClosure = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // btnClosure
      // 
      this.btnClosure.Location = new System.Drawing.Point(8, 8);
      this.btnClosure.Name = "btnClosure";
      this.btnClosure.Size = new System.Drawing.Size(104, 16);
      this.btnClosure.TabIndex = 5;
      this.btnClosure.Text = "Include &Closure";
      this.btnClosure.CheckedChanged += new System.EventHandler(this.btnClosure_CheckedChanged);
      // 
      // tbxPdf
      // 
      this.tbxPdf.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbxPdf.DetectUrls = false;
      this.tbxPdf.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.tbxPdf.Location = new System.Drawing.Point(8, 28);
      this.tbxPdf.Name = "tbxPdf";
      this.tbxPdf.ReadOnly = true;
      this.tbxPdf.Size = new System.Drawing.Size(664, 464);
      this.tbxPdf.TabIndex = 6;
      this.tbxPdf.Text = "";
      // 
      // txtClosure
      // 
      this.txtClosure.Location = new System.Drawing.Point(120, 9);
      this.txtClosure.Name = "txtClosure";
      this.txtClosure.Size = new System.Drawing.Size(424, 16);
      this.txtClosure.TabIndex = 7;
      this.txtClosure.Text = "{Closure}";
      // 
      // PdfTextPage
      // 
      this.Controls.Add(this.txtClosure);
      this.Controls.Add(this.tbxPdf);
      this.Controls.Add(this.btnClosure);
      this.Name = "PdfTextPage";
      this.ResumeLayout(false);

    }
    #endregion

    private void btnUpdate_Click(object sender, System.EventArgs e)
    {
//      PdfItem item = this.explorer.Process.Navigator.Current;
//      if (item != null)
//      {
//        MemoryStream memoryStream = new MemoryStream();
//        PdfItem[] items;
//        if (item is PdfObject && this.btnClosure.Checked)
//        {
//          PdfObject[] objects = this.process.Document.Internals.GetClosure((PdfObject)item);
//          items = new PdfItem[objects.Length];
//          objects.CopyTo(items, 0);
//          string message = String.Format("Closure contains {0} elements.\n\n", objects.Length);
//          memoryStream.Write(Encoding.Default.GetBytes(message), 0, message.Length);
//        }
//        else
//          items = new PdfItem[1]{item};;
//
//        for (int idx = 0; idx < items.Length; idx++)
//          this.process.Document.Internals.WriteObject(memoryStream, items[idx]);
//
//        int count = (int)memoryStream.Length;
//        byte[] bytes = new byte[count];
//        memoryStream.Position = 0;
//        memoryStream.Read(bytes, 0, count);
//        char[] chars = new char[count];
//
//        // Keep in mind that a PDF stream has no intrinsic encoding.
//        for (int idx = 0; idx < count; idx++)
//        {
//          byte b = bytes[idx];
//          if (b == 0)
//            chars[idx] = '?';
//          else
//            chars[idx] = (char)bytes[idx];
//        }
//        string pdf = new string(chars);
//        this.tbxPdf.Text = pdf;
//      }
    }

    private void btnClosure_CheckedChanged(object sender, System.EventArgs e)
    {
      SetObject(this.explorer.Process.Navigator.Current);
    }
  }
}
