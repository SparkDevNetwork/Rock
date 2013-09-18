using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace XDrawing.TestLab
{
  /// <summary>
  /// ChooseColorDialog.
  /// </summary>
  public class ChooseColorDialog : System.Windows.Forms.Form
  {
    private XDrawing.TestLab.FormPages.ColorPage colorPage;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.ComponentModel.Container components = null;

    public ChooseColorDialog()
    { 
      InitializeComponent();
      this.colorPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
    }

    void ModelToView()
    {
      if (this.inModelToView || DesignMode)
        return;

      this.inModelToView = false;
    }
    bool inModelToView;

    public event UpdateDrawing UpdateDrawing;

    void OnUpdateDrawing()
    {
      if (UpdateDrawing != null)
        UpdateDrawing();
    }

    public ColorProperty ColorProperty
    {
      get {return this.colorProperty;}
      set 
      {
        this.colorProperty = value;
        this.colorPage.ColorProperty = value;
      }
    }
    ColorProperty colorProperty;

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

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.colorPage = new XDrawing.TestLab.FormPages.ColorPage();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // colorPage
      // 
      this.colorPage.BackColor = System.Drawing.SystemColors.Control;
      this.colorPage.ColorProperty = null;
      this.colorPage.Location = new System.Drawing.Point(0, 0);
      this.colorPage.Name = "colorPage";
      this.colorPage.Size = new System.Drawing.Size(380, 148);
      this.colorPage.TabIndex = 0;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnOK.Location = new System.Drawing.Point(208, 156);
      this.btnOK.Name = "btnOK";
      this.btnOK.TabIndex = 1;
      this.btnOK.Text = "OK";
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnCancel.Location = new System.Drawing.Point(292, 156);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      // 
      // ChooseColorDialog
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(374, 188);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.colorPage);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ChooseColorDialog";
      this.ShowInTaskbar = false;
      this.Text = "Choose Color";
      this.ResumeLayout(false);

    }
    #endregion

    private void btnOK_Click(object sender, System.EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      Close();
    }
  }
}
