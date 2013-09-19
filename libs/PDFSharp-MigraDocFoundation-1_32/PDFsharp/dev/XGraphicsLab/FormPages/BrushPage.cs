using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.FormPages
{
  /// <summary>
  /// 
  /// </summary>
  public class BrushPage : System.Windows.Forms.UserControl
  {
    private System.ComponentModel.Container components = null;

    public BrushPage()
    {
      InitializeComponent();
      UITools.SetTabPageColor(this);
      this.colorPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
    }

    void ModelToView()
    {
      if (this.inModelToView || DesignMode)
        return;

      // No further attributes than color so far...

      this.inModelToView = false;
    }

    private XDrawing.TestLab.FormPages.ColorPage colorPage;
    bool inModelToView;

    public event UpdateDrawing UpdateDrawing;

    void OnUpdateDrawing()
    {
      if (UpdateDrawing != null)
        UpdateDrawing();
    }

    public BrushProperty BrushProperty
    {
      get {return this.brushProperty;}
      set 
      {
        this.brushProperty = value;
        this.colorPage.ColorProperty = value.Color;
      }
    }
    BrushProperty brushProperty;

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
      this.colorPage = new XDrawing.TestLab.FormPages.ColorPage();
      this.SuspendLayout();
      // 
      // colorPage
      // 
      this.colorPage.ColorProperty = null;
      this.colorPage.Location = new System.Drawing.Point(0, 0);
      this.colorPage.Name = "colorPage";
      this.colorPage.Size = new System.Drawing.Size(380, 150);
      this.colorPage.TabIndex = 0;
      // 
      // BrushPage
      // 
      this.Controls.Add(this.colorPage);
      this.Name = "BrushPage";
      this.Size = new System.Drawing.Size(380, 240);
      this.ResumeLayout(false);

    }
    #endregion

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad (e);
      ModelToView();
    }

  }
}
