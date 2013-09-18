using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using XDrawing.TestLab;

namespace XDrawing.TestLab.FormPages
{
  /// <summary>
  /// 
  /// </summary>
  public class GeneralPage : System.Windows.Forms.UserControl
  {
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnBackground;
    private System.Windows.Forms.Label lblFillMode;
    private System.Windows.Forms.Panel pnlBackground;
    private System.Windows.Forms.ComboBox cbxFillMode;
    private System.Windows.Forms.NumericUpDown udTension;
    private System.Windows.Forms.Label lblTension;
    private System.Windows.Forms.Button btnClear;
    private System.Windows.Forms.Label lblPageUnit;
    private System.Windows.Forms.ComboBox cbxPageUnit;
    private System.Windows.Forms.Label lblPageDirection;
    private System.Windows.Forms.ComboBox cbxPageDirection;
    private System.Windows.Forms.Label lblDirectionInfo;
    private ComboBox cbxColorMode;
    private Label lblColorMode;
    private System.ComponentModel.Container components = null;

    public GeneralPage()
    {
      InitializeComponent();
      UITools.SetTabPageColor(this);
    }

    void ModelToView()
    {
      if (this.inModelToView || DesignMode)
        return;

      this.inModelToView = true;

      this.pnlBackground.BackColor = this.generalProperties.BackColor.Color.ToGdiColor();
      this.cbxFillMode.SelectedIndex = (int)this.generalProperties.FillMode;
      this.udTension.Value = (decimal)this.generalProperties.Tension;
      this.cbxPageUnit.SelectedIndex = (int)this.generalProperties.PageUnit;
      this.cbxPageDirection.SelectedIndex = (int)this.generalProperties.PageDirection;
      this.cbxColorMode.SelectedIndex = (int)this.generalProperties.ColorMode;
      this.inModelToView = false;
    }
    bool inModelToView;

    public event UpdateDrawing UpdateDrawing;

    void OnUpdateDrawing()
    {
      if (UpdateDrawing != null)
      {
        ModelToView();
        UpdateDrawing();
      }
    }

    public GeneralProperties GeneralProperties
    {
      get {return this.generalProperties;}
      set {this.generalProperties = value; ModelToView();}
    }
    GeneralProperties generalProperties;

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
      this.label1 = new System.Windows.Forms.Label();
      this.pnlBackground = new System.Windows.Forms.Panel();
      this.btnBackground = new System.Windows.Forms.Button();
      this.lblFillMode = new System.Windows.Forms.Label();
      this.cbxFillMode = new System.Windows.Forms.ComboBox();
      this.udTension = new System.Windows.Forms.NumericUpDown();
      this.lblTension = new System.Windows.Forms.Label();
      this.btnClear = new System.Windows.Forms.Button();
      this.lblPageUnit = new System.Windows.Forms.Label();
      this.cbxPageUnit = new System.Windows.Forms.ComboBox();
      this.lblPageDirection = new System.Windows.Forms.Label();
      this.cbxPageDirection = new System.Windows.Forms.ComboBox();
      this.lblDirectionInfo = new System.Windows.Forms.Label();
      this.cbxColorMode = new System.Windows.Forms.ComboBox();
      this.lblColorMode = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.udTension)).BeginInit();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 24);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(68, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Background:";
      // 
      // pnlBackground
      // 
      this.pnlBackground.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pnlBackground.Location = new System.Drawing.Point(90, 12);
      this.pnlBackground.Name = "pnlBackground";
      this.pnlBackground.Size = new System.Drawing.Size(44, 40);
      this.pnlBackground.TabIndex = 1;
      // 
      // btnBackground
      // 
      this.btnBackground.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnBackground.Location = new System.Drawing.Point(144, 19);
      this.btnBackground.Name = "btnBackground";
      this.btnBackground.Size = new System.Drawing.Size(24, 23);
      this.btnBackground.TabIndex = 2;
      this.btnBackground.Text = "...";
      this.btnBackground.Click += new System.EventHandler(this.btnBackground_Click);
      // 
      // lblFillMode
      // 
      this.lblFillMode.Location = new System.Drawing.Point(16, 61);
      this.lblFillMode.Name = "lblFillMode";
      this.lblFillMode.Size = new System.Drawing.Size(64, 16);
      this.lblFillMode.TabIndex = 3;
      this.lblFillMode.Text = "FillMode:";
      // 
      // cbxFillMode
      // 
      this.cbxFillMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxFillMode.Items.AddRange(new object[] {
            "Alternate (0)",
            "Winding (1)"});
      this.cbxFillMode.Location = new System.Drawing.Point(90, 58);
      this.cbxFillMode.Name = "cbxFillMode";
      this.cbxFillMode.Size = new System.Drawing.Size(121, 21);
      this.cbxFillMode.TabIndex = 4;
      this.cbxFillMode.SelectedIndexChanged += new System.EventHandler(this.cbxFillMode_SelectedIndexChanged);
      // 
      // udTension
      // 
      this.udTension.DecimalPlaces = 2;
      this.udTension.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
      this.udTension.Location = new System.Drawing.Point(90, 85);
      this.udTension.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.udTension.Name = "udTension";
      this.udTension.Size = new System.Drawing.Size(48, 20);
      this.udTension.TabIndex = 9;
      this.udTension.ValueChanged += new System.EventHandler(this.udTension_ValueChanged);
      // 
      // lblTension
      // 
      this.lblTension.Location = new System.Drawing.Point(16, 87);
      this.lblTension.Name = "lblTension";
      this.lblTension.Size = new System.Drawing.Size(64, 16);
      this.lblTension.TabIndex = 8;
      this.lblTension.Text = "Tension:";
      // 
      // btnClear
      // 
      this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.btnClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnClear.ForeColor = System.Drawing.Color.Firebrick;
      this.btnClear.Location = new System.Drawing.Point(144, 84);
      this.btnClear.Name = "btnClear";
      this.btnClear.Size = new System.Drawing.Size(19, 19);
      this.btnClear.TabIndex = 10;
      this.btnClear.TabStop = false;
      this.btnClear.Text = "X";
      this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
      // 
      // lblPageUnit
      // 
      this.lblPageUnit.Location = new System.Drawing.Point(16, 114);
      this.lblPageUnit.Name = "lblPageUnit";
      this.lblPageUnit.Size = new System.Drawing.Size(60, 16);
      this.lblPageUnit.TabIndex = 11;
      this.lblPageUnit.Text = "PageUnit:";
      // 
      // cbxPageUnit
      // 
      this.cbxPageUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxPageUnit.Items.AddRange(new object[] {
            "Point (0)",
            "Inch (1)",
            "Millimeter (2)",
            "Centimeter (3)"});
      this.cbxPageUnit.Location = new System.Drawing.Point(90, 111);
      this.cbxPageUnit.Name = "cbxPageUnit";
      this.cbxPageUnit.Size = new System.Drawing.Size(121, 21);
      this.cbxPageUnit.TabIndex = 4;
      this.cbxPageUnit.SelectedIndexChanged += new System.EventHandler(this.cbxPageUnit_SelectedIndexChanged);
      // 
      // lblPageDirection
      // 
      this.lblPageDirection.Location = new System.Drawing.Point(16, 141);
      this.lblPageDirection.Name = "lblPageDirection";
      this.lblPageDirection.Size = new System.Drawing.Size(60, 16);
      this.lblPageDirection.TabIndex = 11;
      this.lblPageDirection.Text = "Direction:";
      // 
      // cbxPageDirection
      // 
      this.cbxPageDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxPageDirection.Items.AddRange(new object[] {
            "Downwards (0)",
            "Upwards (1)"});
      this.cbxPageDirection.Location = new System.Drawing.Point(90, 138);
      this.cbxPageDirection.Name = "cbxPageDirection";
      this.cbxPageDirection.Size = new System.Drawing.Size(121, 21);
      this.cbxPageDirection.TabIndex = 4;
      this.cbxPageDirection.SelectedIndexChanged += new System.EventHandler(this.cbxPageDirection_SelectedIndexChanged);
      // 
      // lblDirectionInfo
      // 
      this.lblDirectionInfo.Location = new System.Drawing.Point(217, 141);
      this.lblDirectionInfo.Name = "lblDirectionInfo";
      this.lblDirectionInfo.Size = new System.Drawing.Size(148, 16);
      this.lblDirectionInfo.TabIndex = 12;
      this.lblDirectionInfo.Text = "(applies to PDF pages only)";
      // 
      // cbxColorMode
      // 
      this.cbxColorMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxColorMode.Items.AddRange(new object[] {
            "Do not care",
            "RGB",
            "CMYK"});
      this.cbxColorMode.Location = new System.Drawing.Point(90, 165);
      this.cbxColorMode.Name = "cbxColorMode";
      this.cbxColorMode.Size = new System.Drawing.Size(121, 21);
      this.cbxColorMode.TabIndex = 4;
      this.cbxColorMode.SelectedIndexChanged += new System.EventHandler(this.cbxColorMode_SelectedIndexChanged);
      // 
      // lblColorMode
      // 
      this.lblColorMode.Location = new System.Drawing.Point(16, 168);
      this.lblColorMode.Name = "lblColorMode";
      this.lblColorMode.Size = new System.Drawing.Size(60, 16);
      this.lblColorMode.TabIndex = 11;
      this.lblColorMode.Text = "Direction:";
      // 
      // GeneralPage
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.lblDirectionInfo);
      this.Controls.Add(this.lblPageUnit);
      this.Controls.Add(this.btnClear);
      this.Controls.Add(this.udTension);
      this.Controls.Add(this.lblTension);
      this.Controls.Add(this.cbxFillMode);
      this.Controls.Add(this.lblFillMode);
      this.Controls.Add(this.btnBackground);
      this.Controls.Add(this.pnlBackground);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.cbxPageUnit);
      this.Controls.Add(this.lblColorMode);
      this.Controls.Add(this.lblPageDirection);
      this.Controls.Add(this.cbxColorMode);
      this.Controls.Add(this.cbxPageDirection);
      this.Name = "GeneralPage";
      this.Size = new System.Drawing.Size(380, 240);
      ((System.ComponentModel.ISupportInitialize)(this.udTension)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private void btnBackground_Click(object sender, System.EventArgs e)
    {
      try
      {
        using (ChooseColorDialog dialog = new ChooseColorDialog())
        {
          dialog.ColorProperty = this.generalProperties.BackColor;
          dialog.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
        
          XColor oldColor = this.generalProperties.BackColor.Color;
          if (dialog.ShowDialog(XGraphicsLab.mainForm) != DialogResult.OK)
          {
            this.generalProperties.BackColor.Color = oldColor;
            OnUpdateDrawing();
          }
        }
      }
      catch
      {}
    }

    private void cbxFillMode_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      if (this.cbxFillMode.SelectedIndex != -1)
      {
        this.generalProperties.FillMode = (XFillMode)this.cbxFillMode.SelectedIndex;
        OnUpdateDrawing();
      }
    }

    private void udTension_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.generalProperties.Tension = Convert.ToDouble(this.udTension.Value);
      OnUpdateDrawing();
    }

    private void btnClear_Click(object sender, System.EventArgs e)
    {
      this.udTension.Value = 0.5M;
    }

    private void cbxPageUnit_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      if (this.cbxPageUnit.SelectedIndex != -1)
      {
        this.generalProperties.PageUnit = (XGraphicsUnit)this.cbxPageUnit.SelectedIndex;
        OnUpdateDrawing();
      }
    }

    private void cbxPageDirection_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      if (this.cbxPageDirection.SelectedIndex != -1)
      {
        this.generalProperties.PageDirection = (XPageDirection)this.cbxPageDirection.SelectedIndex;
        OnUpdateDrawing();
      }
    }

    private void cbxColorMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (this.inModelToView)
        return;
      if (this.cbxColorMode.SelectedIndex != -1)
      {
        this.generalProperties.ColorMode = (PdfColorMode)this.cbxColorMode.SelectedIndex;
        OnUpdateDrawing();
      }
    }
  }
}
