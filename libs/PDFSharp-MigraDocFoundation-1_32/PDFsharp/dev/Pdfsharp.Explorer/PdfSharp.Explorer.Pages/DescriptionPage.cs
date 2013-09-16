using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Explorer.Pages
{
  /// <summary>
  /// 
  /// </summary>
  public class DescriptionPage : PageBase
  {
    private System.Windows.Forms.Label lblTitel;
    private System.Windows.Forms.TextBox tbxTitel;
    private System.Windows.Forms.Label lblAuthor;
    private System.Windows.Forms.Label lblSubject;
    private System.Windows.Forms.Label lblKeywords;
    private System.Windows.Forms.TextBox tbxAuthor;
    private System.Windows.Forms.TextBox tbxSubject;
    private System.Windows.Forms.TextBox tbxKeywords;
    private System.Windows.Forms.Label lblCreated;
    private System.Windows.Forms.Label lblModified;
    private System.Windows.Forms.Label lblApplication;
    private System.Windows.Forms.Label txtCreated;
    private System.Windows.Forms.Label txtModified;
    private System.Windows.Forms.Label txtApplication;
    private System.Windows.Forms.Label lblProducer;
    private System.Windows.Forms.Label lblVersion;
    private System.Windows.Forms.Label lblFileSize;
    private System.Windows.Forms.Label lblPageSize;
    private System.Windows.Forms.Label lblPages;
    private System.Windows.Forms.Label txtProcuder;
    private System.Windows.Forms.Label txtVersion;
    private System.Windows.Forms.Label txtFileSize;
    private System.Windows.Forms.Label txtPageSize;
    private System.Windows.Forms.Label txtPages;
    private System.Windows.Forms.Label lblObjects;
    private System.Windows.Forms.Label txtObjects;
    private System.ComponentModel.Container components = null;

    public DescriptionPage(ExplorerPanel explorer)
    {
      this.explorer = explorer;
      InitializeComponent();
    }
    ExplorerPanel explorer;

    internal override void UpdateDocument()
    {
      base.UpdateDocument ();

      PdfDocument document = this.explorer.MainForm.Process.Document;
      if (document == null)
        return;

      this.tbxTitel.Text = document.Info.Title;
      this.tbxAuthor.Text = document.Info.Author;
      this.tbxSubject.Text = document.Info.Subject;
      this.tbxKeywords.Text = document.Info.Keywords;

      this.txtCreated.Text = document.Info.CreationDate.ToShortDateString() + " " + 
        document.Info.CreationDate.ToLongTimeString();
      this.txtModified.Text = document.Info.ModificationDate.ToShortDateString() + " " + 
        document.Info.ModificationDate.ToLongTimeString();
      this.txtApplication.Text = document.Info.Creator;
      this.txtProcuder.Text = document.Info.Producer;
      this.txtVersion.Text = 
        String.Format("{0}.{1} (Acrobat {2}.x)", document.Version / 10, document.Version % 10, document.Version % 10 + 1);
      this.txtFileSize.Text = 
        String.Format("{0:0,000} Byte", document.FileSize.ToString());
      int pageCount = document.PageCount;
      if (pageCount > 0)
        this.txtPageSize.Text = ExplorerHelper.PageSize(document.Pages[0], this.explorer.Process.IsMetric);
      this.txtPages.Text = pageCount.ToString();
      this.txtObjects.Text = document.Internals.GetAllObjects().Length.ToString();
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
      this.lblTitel = new System.Windows.Forms.Label();
      this.tbxTitel = new System.Windows.Forms.TextBox();
      this.lblAuthor = new System.Windows.Forms.Label();
      this.lblSubject = new System.Windows.Forms.Label();
      this.lblKeywords = new System.Windows.Forms.Label();
      this.tbxAuthor = new System.Windows.Forms.TextBox();
      this.tbxSubject = new System.Windows.Forms.TextBox();
      this.tbxKeywords = new System.Windows.Forms.TextBox();
      this.lblCreated = new System.Windows.Forms.Label();
      this.lblModified = new System.Windows.Forms.Label();
      this.lblApplication = new System.Windows.Forms.Label();
      this.txtCreated = new System.Windows.Forms.Label();
      this.txtModified = new System.Windows.Forms.Label();
      this.txtApplication = new System.Windows.Forms.Label();
      this.lblProducer = new System.Windows.Forms.Label();
      this.lblVersion = new System.Windows.Forms.Label();
      this.lblFileSize = new System.Windows.Forms.Label();
      this.lblPageSize = new System.Windows.Forms.Label();
      this.lblPages = new System.Windows.Forms.Label();
      this.txtProcuder = new System.Windows.Forms.Label();
      this.txtVersion = new System.Windows.Forms.Label();
      this.txtFileSize = new System.Windows.Forms.Label();
      this.txtPageSize = new System.Windows.Forms.Label();
      this.txtPages = new System.Windows.Forms.Label();
      this.lblObjects = new System.Windows.Forms.Label();
      this.txtObjects = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lblTitel
      // 
      this.lblTitel.Location = new System.Drawing.Point(16, 16);
      this.lblTitel.Name = "lblTitel";
      this.lblTitel.Size = new System.Drawing.Size(100, 16);
      this.lblTitel.TabIndex = 0;
      this.lblTitel.Text = "&Title:";
      // 
      // tbxTitel
      // 
      this.tbxTitel.Location = new System.Drawing.Point(128, 12);
      this.tbxTitel.Name = "tbxTitel";
      this.tbxTitel.ReadOnly = true;
      this.tbxTitel.Size = new System.Drawing.Size(304, 20);
      this.tbxTitel.TabIndex = 1;
      // 
      // lblAuthor
      // 
      this.lblAuthor.Location = new System.Drawing.Point(16, 44);
      this.lblAuthor.Name = "lblAuthor";
      this.lblAuthor.Size = new System.Drawing.Size(100, 16);
      this.lblAuthor.TabIndex = 0;
      this.lblAuthor.Text = "&Author:";
      // 
      // lblSubject
      // 
      this.lblSubject.Location = new System.Drawing.Point(16, 72);
      this.lblSubject.Name = "lblSubject";
      this.lblSubject.Size = new System.Drawing.Size(100, 16);
      this.lblSubject.TabIndex = 0;
      this.lblSubject.Text = "&Subject:";
      // 
      // lblKeywords
      // 
      this.lblKeywords.Location = new System.Drawing.Point(16, 104);
      this.lblKeywords.Name = "lblKeywords";
      this.lblKeywords.Size = new System.Drawing.Size(100, 16);
      this.lblKeywords.TabIndex = 0;
      this.lblKeywords.Text = "&Keywords:";
      // 
      // tbxAuthor
      // 
      this.tbxAuthor.Location = new System.Drawing.Point(128, 40);
      this.tbxAuthor.Name = "tbxAuthor";
      this.tbxAuthor.ReadOnly = true;
      this.tbxAuthor.Size = new System.Drawing.Size(304, 20);
      this.tbxAuthor.TabIndex = 1;
      // 
      // tbxSubject
      // 
      this.tbxSubject.Location = new System.Drawing.Point(128, 68);
      this.tbxSubject.Name = "tbxSubject";
      this.tbxSubject.ReadOnly = true;
      this.tbxSubject.Size = new System.Drawing.Size(304, 20);
      this.tbxSubject.TabIndex = 1;
      // 
      // tbxKeywords
      // 
      this.tbxKeywords.Location = new System.Drawing.Point(128, 100);
      this.tbxKeywords.Multiline = true;
      this.tbxKeywords.Name = "tbxKeywords";
      this.tbxKeywords.ReadOnly = true;
      this.tbxKeywords.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbxKeywords.Size = new System.Drawing.Size(304, 76);
      this.tbxKeywords.TabIndex = 1;
      // 
      // lblCreated
      // 
      this.lblCreated.Location = new System.Drawing.Point(16, 184);
      this.lblCreated.Name = "lblCreated";
      this.lblCreated.Size = new System.Drawing.Size(100, 16);
      this.lblCreated.TabIndex = 0;
      this.lblCreated.Text = "Created:";
      // 
      // lblModified
      // 
      this.lblModified.Location = new System.Drawing.Point(16, 208);
      this.lblModified.Name = "lblModified";
      this.lblModified.Size = new System.Drawing.Size(100, 16);
      this.lblModified.TabIndex = 0;
      this.lblModified.Text = "Modified:";
      // 
      // lblApplication
      // 
      this.lblApplication.Location = new System.Drawing.Point(16, 232);
      this.lblApplication.Name = "lblApplication";
      this.lblApplication.Size = new System.Drawing.Size(100, 16);
      this.lblApplication.TabIndex = 0;
      this.lblApplication.Text = "Application:";
      // 
      // txtCreated
      // 
      this.txtCreated.Location = new System.Drawing.Point(128, 184);
      this.txtCreated.Name = "txtCreated";
      this.txtCreated.Size = new System.Drawing.Size(348, 16);
      this.txtCreated.TabIndex = 0;
      // 
      // txtModified
      // 
      this.txtModified.Location = new System.Drawing.Point(128, 208);
      this.txtModified.Name = "txtModified";
      this.txtModified.Size = new System.Drawing.Size(348, 16);
      this.txtModified.TabIndex = 0;
      // 
      // txtApplication
      // 
      this.txtApplication.Location = new System.Drawing.Point(128, 232);
      this.txtApplication.Name = "txtApplication";
      this.txtApplication.Size = new System.Drawing.Size(348, 16);
      this.txtApplication.TabIndex = 0;
      // 
      // lblProducer
      // 
      this.lblProducer.Location = new System.Drawing.Point(16, 264);
      this.lblProducer.Name = "lblProducer";
      this.lblProducer.Size = new System.Drawing.Size(100, 16);
      this.lblProducer.TabIndex = 0;
      this.lblProducer.Text = "PDF Producer:";
      // 
      // lblVersion
      // 
      this.lblVersion.Location = new System.Drawing.Point(16, 288);
      this.lblVersion.Name = "lblVersion";
      this.lblVersion.Size = new System.Drawing.Size(100, 16);
      this.lblVersion.TabIndex = 0;
      this.lblVersion.Text = "PDF Version:";
      // 
      // lblFileSize
      // 
      this.lblFileSize.Location = new System.Drawing.Point(16, 320);
      this.lblFileSize.Name = "lblFileSize";
      this.lblFileSize.Size = new System.Drawing.Size(100, 16);
      this.lblFileSize.TabIndex = 0;
      this.lblFileSize.Text = "File Size:";
      // 
      // lblPageSize
      // 
      this.lblPageSize.Location = new System.Drawing.Point(16, 344);
      this.lblPageSize.Name = "lblPageSize";
      this.lblPageSize.Size = new System.Drawing.Size(100, 16);
      this.lblPageSize.TabIndex = 0;
      this.lblPageSize.Text = "Page Size:";
      // 
      // lblPages
      // 
      this.lblPages.Location = new System.Drawing.Point(260, 344);
      this.lblPages.Name = "lblPages";
      this.lblPages.Size = new System.Drawing.Size(108, 16);
      this.lblPages.TabIndex = 0;
      this.lblPages.Text = "Number of Pages:";
      // 
      // txtProcuder
      // 
      this.txtProcuder.Location = new System.Drawing.Point(128, 264);
      this.txtProcuder.Name = "txtProcuder";
      this.txtProcuder.Size = new System.Drawing.Size(348, 16);
      this.txtProcuder.TabIndex = 0;
      // 
      // txtVersion
      // 
      this.txtVersion.Location = new System.Drawing.Point(128, 288);
      this.txtVersion.Name = "txtVersion";
      this.txtVersion.Size = new System.Drawing.Size(348, 16);
      this.txtVersion.TabIndex = 0;
      // 
      // txtFileSize
      // 
      this.txtFileSize.Location = new System.Drawing.Point(128, 320);
      this.txtFileSize.Name = "txtFileSize";
      this.txtFileSize.Size = new System.Drawing.Size(116, 16);
      this.txtFileSize.TabIndex = 0;
      // 
      // txtPageSize
      // 
      this.txtPageSize.Location = new System.Drawing.Point(128, 344);
      this.txtPageSize.Name = "txtPageSize";
      this.txtPageSize.Size = new System.Drawing.Size(116, 16);
      this.txtPageSize.TabIndex = 0;
      // 
      // txtPages
      // 
      this.txtPages.Location = new System.Drawing.Point(380, 344);
      this.txtPages.Name = "txtPages";
      this.txtPages.Size = new System.Drawing.Size(96, 16);
      this.txtPages.TabIndex = 0;
      // 
      // lblObjects
      // 
      this.lblObjects.Location = new System.Drawing.Point(260, 320);
      this.lblObjects.Name = "lblObjects";
      this.lblObjects.Size = new System.Drawing.Size(108, 16);
      this.lblObjects.TabIndex = 2;
      this.lblObjects.Text = "Number of Objects:";
      // 
      // txtObjects
      // 
      this.txtObjects.Location = new System.Drawing.Point(380, 320);
      this.txtObjects.Name = "txtObjects";
      this.txtObjects.Size = new System.Drawing.Size(96, 16);
      this.txtObjects.TabIndex = 0;
      // 
      // DescriptionPage
      // 
      this.Controls.Add(this.lblObjects);
      this.Controls.Add(this.tbxTitel);
      this.Controls.Add(this.lblTitel);
      this.Controls.Add(this.lblAuthor);
      this.Controls.Add(this.lblSubject);
      this.Controls.Add(this.lblKeywords);
      this.Controls.Add(this.tbxAuthor);
      this.Controls.Add(this.tbxSubject);
      this.Controls.Add(this.tbxKeywords);
      this.Controls.Add(this.lblCreated);
      this.Controls.Add(this.lblModified);
      this.Controls.Add(this.lblApplication);
      this.Controls.Add(this.txtCreated);
      this.Controls.Add(this.txtModified);
      this.Controls.Add(this.txtApplication);
      this.Controls.Add(this.lblProducer);
      this.Controls.Add(this.lblVersion);
      this.Controls.Add(this.lblFileSize);
      this.Controls.Add(this.lblPageSize);
      this.Controls.Add(this.lblPages);
      this.Controls.Add(this.txtProcuder);
      this.Controls.Add(this.txtVersion);
      this.Controls.Add(this.txtFileSize);
      this.Controls.Add(this.txtPageSize);
      this.Controls.Add(this.txtPages);
      this.Controls.Add(this.txtObjects);
      this.Name = "DescriptionPage";
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion
  }
}
