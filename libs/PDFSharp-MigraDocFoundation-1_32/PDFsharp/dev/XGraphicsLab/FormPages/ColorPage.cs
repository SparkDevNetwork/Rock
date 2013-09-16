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
  public class ColorPage : System.Windows.Forms.UserControl
  {
    private System.Windows.Forms.RadioButton btnRGB;
    private System.Windows.Forms.Label lblR;
    private System.Windows.Forms.Label lblG;
    private System.Windows.Forms.Label lblB;
    private System.Windows.Forms.Label lblGS;
    private System.Windows.Forms.NumericUpDown udGS;
    private System.Windows.Forms.Panel pnlColor;
    private PdfSharp.Forms.ColorComboBox cbxColor;
    private System.Windows.Forms.RadioButton btnCMYK;
    private System.Windows.Forms.RadioButton btnGS;
    private System.Windows.Forms.NumericUpDown udR;
    private System.Windows.Forms.Panel pnlRGB;
    private System.Windows.Forms.NumericUpDown udG;
    private System.Windows.Forms.NumericUpDown udB;
    private System.Windows.Forms.Panel pnlCMYK;
    private System.Windows.Forms.Label lblC;
    private System.Windows.Forms.NumericUpDown udC;
    private System.Windows.Forms.Label lblM;
    private System.Windows.Forms.Label lblY;
    private System.Windows.Forms.NumericUpDown udM;
    private System.Windows.Forms.NumericUpDown udY;
    private System.Windows.Forms.Label lblK;
    private System.Windows.Forms.NumericUpDown udK;
    private System.Windows.Forms.Panel pnlGS;
    private System.Windows.Forms.Label lblAlpha;
    private System.Windows.Forms.NumericUpDown udAlpha;
    private System.ComponentModel.Container components = null;

    public ColorPage()
    {
      InitializeComponent();
      UITools.SetTabPageColor(this);
      //this.cbxColor.Items.AddRange(Enum.GetNames(typeof(PdfSharp.Drawing.XKnownColor)));
    }

    void ModelToView()
    {
      if (this.inModelToView || DesignMode)
        return;

      this.inModelToView = true;

      this.udR.Value = this.colorProperty.R;
      this.udG.Value = this.colorProperty.G;
      this.udB.Value = this.colorProperty.B;

      this.udC.Value = (decimal)this.colorProperty.C;
      this.udM.Value = (decimal)this.colorProperty.M;
      this.udY.Value = (decimal)this.colorProperty.Y;
      this.udK.Value = (decimal)this.colorProperty.K;

      this.udGS.Value = (decimal)this.colorProperty.GS;

      this.udAlpha.Value = (decimal)this.colorProperty.A;

      this.pnlColor.BackColor = this.colorProperty.Color.ToGdiColor();

      this.cbxColor.Color = this.colorProperty.Color;

      this.inModelToView = false;
    }
    bool inModelToView;

    public event UpdateDrawing UpdateDrawing;

    void OnUpdateDrawing()
    {
      ModelToView();
      if (UpdateDrawing != null)
        UpdateDrawing();
    }

    public ColorProperty ColorProperty
    {
      get {return this.colorProperty;}
      set {this.colorProperty = value;}
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

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.lblR = new System.Windows.Forms.Label();
      this.lblG = new System.Windows.Forms.Label();
      this.lblB = new System.Windows.Forms.Label();
      this.lblGS = new System.Windows.Forms.Label();
      this.udGS = new System.Windows.Forms.NumericUpDown();
      this.pnlColor = new System.Windows.Forms.Panel();
      this.btnRGB = new System.Windows.Forms.RadioButton();
      this.btnCMYK = new System.Windows.Forms.RadioButton();
      this.btnGS = new System.Windows.Forms.RadioButton();
      this.udR = new System.Windows.Forms.NumericUpDown();
      this.pnlRGB = new System.Windows.Forms.Panel();
      this.udG = new System.Windows.Forms.NumericUpDown();
      this.udB = new System.Windows.Forms.NumericUpDown();
      this.pnlCMYK = new System.Windows.Forms.Panel();
      this.lblC = new System.Windows.Forms.Label();
      this.udC = new System.Windows.Forms.NumericUpDown();
      this.lblM = new System.Windows.Forms.Label();
      this.lblY = new System.Windows.Forms.Label();
      this.udM = new System.Windows.Forms.NumericUpDown();
      this.udY = new System.Windows.Forms.NumericUpDown();
      this.lblK = new System.Windows.Forms.Label();
      this.udK = new System.Windows.Forms.NumericUpDown();
      this.pnlGS = new System.Windows.Forms.Panel();
      this.udAlpha = new System.Windows.Forms.NumericUpDown();
      this.lblAlpha = new System.Windows.Forms.Label();
      this.cbxColor = new PdfSharp.Forms.ColorComboBox();
      ((System.ComponentModel.ISupportInitialize)(this.udGS)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.udR)).BeginInit();
      this.pnlRGB.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.udG)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.udB)).BeginInit();
      this.pnlCMYK.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.udC)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.udM)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.udY)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.udK)).BeginInit();
      this.pnlGS.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.udAlpha)).BeginInit();
      this.SuspendLayout();
      // 
      // lblR
      // 
      this.lblR.Location = new System.Drawing.Point(4, 4);
      this.lblR.Name = "lblR";
      this.lblR.Size = new System.Drawing.Size(16, 16);
      this.lblR.TabIndex = 0;
      this.lblR.Text = "R";
      // 
      // lblG
      // 
      this.lblG.Location = new System.Drawing.Point(84, 4);
      this.lblG.Name = "lblG";
      this.lblG.Size = new System.Drawing.Size(16, 16);
      this.lblG.TabIndex = 0;
      this.lblG.Text = "G";
      // 
      // lblB
      // 
      this.lblB.Location = new System.Drawing.Point(164, 4);
      this.lblB.Name = "lblB";
      this.lblB.Size = new System.Drawing.Size(16, 16);
      this.lblB.TabIndex = 0;
      this.lblB.Text = "B";
      // 
      // lblGS
      // 
      this.lblGS.Location = new System.Drawing.Point(4, 4);
      this.lblGS.Name = "lblGS";
      this.lblGS.Size = new System.Drawing.Size(22, 16);
      this.lblGS.TabIndex = 0;
      this.lblGS.Text = "GS";
      // 
      // udGS
      // 
      this.udGS.DecimalPlaces = 3;
      this.udGS.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.udGS.Location = new System.Drawing.Point(44, 2);
      this.udGS.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.udGS.Name = "udGS";
      this.udGS.Size = new System.Drawing.Size(52, 20);
      this.udGS.TabIndex = 2;
      this.udGS.ValueChanged += new System.EventHandler(this.udGS_ValueChanged);
      // 
      // pnlColor
      // 
      this.pnlColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pnlColor.Location = new System.Drawing.Point(160, 60);
      this.pnlColor.Name = "pnlColor";
      this.pnlColor.Size = new System.Drawing.Size(200, 56);
      this.pnlColor.TabIndex = 3;
      // 
      // btnRGB
      // 
      this.btnRGB.Enabled = false;
      this.btnRGB.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnRGB.Location = new System.Drawing.Point(8, 8);
      this.btnRGB.Name = "btnRGB";
      this.btnRGB.Size = new System.Drawing.Size(16, 16);
      this.btnRGB.TabIndex = 5;
      this.btnRGB.CheckedChanged += new System.EventHandler(this.ColorMode_CheckedChanged);
      // 
      // btnCMYK
      // 
      this.btnCMYK.Enabled = false;
      this.btnCMYK.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnCMYK.Location = new System.Drawing.Point(8, 36);
      this.btnCMYK.Name = "btnCMYK";
      this.btnCMYK.Size = new System.Drawing.Size(16, 16);
      this.btnCMYK.TabIndex = 5;
      this.btnCMYK.CheckedChanged += new System.EventHandler(this.ColorMode_CheckedChanged);
      // 
      // btnGS
      // 
      this.btnGS.Enabled = false;
      this.btnGS.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnGS.Location = new System.Drawing.Point(8, 64);
      this.btnGS.Name = "btnGS";
      this.btnGS.Size = new System.Drawing.Size(16, 16);
      this.btnGS.TabIndex = 5;
      this.btnGS.CheckedChanged += new System.EventHandler(this.ColorMode_CheckedChanged);
      // 
      // udR
      // 
      this.udR.Location = new System.Drawing.Point(28, 2);
      this.udR.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
      this.udR.Name = "udR";
      this.udR.Size = new System.Drawing.Size(48, 20);
      this.udR.TabIndex = 7;
      this.udR.ValueChanged += new System.EventHandler(this.udR_ValueChanged);
      // 
      // pnlRGB
      // 
      this.pnlRGB.Controls.Add(this.lblR);
      this.pnlRGB.Controls.Add(this.udR);
      this.pnlRGB.Controls.Add(this.lblG);
      this.pnlRGB.Controls.Add(this.lblB);
      this.pnlRGB.Controls.Add(this.udG);
      this.pnlRGB.Controls.Add(this.udB);
      this.pnlRGB.Location = new System.Drawing.Point(32, 4);
      this.pnlRGB.Name = "pnlRGB";
      this.pnlRGB.Size = new System.Drawing.Size(330, 24);
      this.pnlRGB.TabIndex = 8;
      // 
      // udG
      // 
      this.udG.Location = new System.Drawing.Point(104, 3);
      this.udG.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
      this.udG.Name = "udG";
      this.udG.Size = new System.Drawing.Size(48, 20);
      this.udG.TabIndex = 7;
      this.udG.ValueChanged += new System.EventHandler(this.udG_ValueChanged);
      // 
      // udB
      // 
      this.udB.Location = new System.Drawing.Point(184, 3);
      this.udB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
      this.udB.Name = "udB";
      this.udB.Size = new System.Drawing.Size(48, 20);
      this.udB.TabIndex = 7;
      this.udB.ValueChanged += new System.EventHandler(this.udB_ValueChanged);
      // 
      // pnlCMYK
      // 
      this.pnlCMYK.Controls.Add(this.lblC);
      this.pnlCMYK.Controls.Add(this.udC);
      this.pnlCMYK.Controls.Add(this.lblM);
      this.pnlCMYK.Controls.Add(this.lblY);
      this.pnlCMYK.Controls.Add(this.udM);
      this.pnlCMYK.Controls.Add(this.udY);
      this.pnlCMYK.Controls.Add(this.lblK);
      this.pnlCMYK.Controls.Add(this.udK);
      this.pnlCMYK.Location = new System.Drawing.Point(32, 32);
      this.pnlCMYK.Name = "pnlCMYK";
      this.pnlCMYK.Size = new System.Drawing.Size(330, 24);
      this.pnlCMYK.TabIndex = 8;
      // 
      // lblC
      // 
      this.lblC.Location = new System.Drawing.Point(4, 4);
      this.lblC.Name = "lblC";
      this.lblC.Size = new System.Drawing.Size(16, 16);
      this.lblC.TabIndex = 0;
      this.lblC.Text = "C";
      // 
      // udC
      // 
      this.udC.DecimalPlaces = 2;
      this.udC.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.udC.Location = new System.Drawing.Point(28, 2);
      this.udC.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.udC.Name = "udC";
      this.udC.Size = new System.Drawing.Size(48, 20);
      this.udC.TabIndex = 7;
      this.udC.ValueChanged += new System.EventHandler(this.udC_ValueChanged);
      // 
      // lblM
      // 
      this.lblM.Location = new System.Drawing.Point(84, 4);
      this.lblM.Name = "lblM";
      this.lblM.Size = new System.Drawing.Size(16, 16);
      this.lblM.TabIndex = 0;
      this.lblM.Text = "M";
      // 
      // lblY
      // 
      this.lblY.Location = new System.Drawing.Point(164, 4);
      this.lblY.Name = "lblY";
      this.lblY.Size = new System.Drawing.Size(16, 16);
      this.lblY.TabIndex = 0;
      this.lblY.Text = "Y";
      // 
      // udM
      // 
      this.udM.DecimalPlaces = 2;
      this.udM.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.udM.Location = new System.Drawing.Point(104, 3);
      this.udM.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.udM.Name = "udM";
      this.udM.Size = new System.Drawing.Size(48, 20);
      this.udM.TabIndex = 7;
      this.udM.ValueChanged += new System.EventHandler(this.udM_ValueChanged);
      // 
      // udY
      // 
      this.udY.DecimalPlaces = 2;
      this.udY.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.udY.Location = new System.Drawing.Point(184, 3);
      this.udY.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.udY.Name = "udY";
      this.udY.Size = new System.Drawing.Size(48, 20);
      this.udY.TabIndex = 7;
      this.udY.ValueChanged += new System.EventHandler(this.udY_ValueChanged);
      // 
      // lblK
      // 
      this.lblK.Location = new System.Drawing.Point(244, 4);
      this.lblK.Name = "lblK";
      this.lblK.Size = new System.Drawing.Size(16, 16);
      this.lblK.TabIndex = 0;
      this.lblK.Text = "K";
      // 
      // udK
      // 
      this.udK.DecimalPlaces = 2;
      this.udK.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.udK.Location = new System.Drawing.Point(264, 2);
      this.udK.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.udK.Name = "udK";
      this.udK.Size = new System.Drawing.Size(48, 20);
      this.udK.TabIndex = 7;
      this.udK.ValueChanged += new System.EventHandler(this.udK_ValueChanged);
      // 
      // pnlGS
      // 
      this.pnlGS.Controls.Add(this.lblGS);
      this.pnlGS.Controls.Add(this.udGS);
      this.pnlGS.Location = new System.Drawing.Point(32, 60);
      this.pnlGS.Name = "pnlGS";
      this.pnlGS.Size = new System.Drawing.Size(120, 24);
      this.pnlGS.TabIndex = 8;
      // 
      // udAlpha
      // 
      this.udAlpha.DecimalPlaces = 2;
      this.udAlpha.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
      this.udAlpha.Location = new System.Drawing.Point(76, 92);
      this.udAlpha.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.udAlpha.Name = "udAlpha";
      this.udAlpha.Size = new System.Drawing.Size(48, 20);
      this.udAlpha.TabIndex = 7;
      this.udAlpha.ValueChanged += new System.EventHandler(this.upAlpha_ValueChanged);
      // 
      // lblAlpha
      // 
      this.lblAlpha.Location = new System.Drawing.Point(32, 92);
      this.lblAlpha.Name = "lblAlpha";
      this.lblAlpha.Size = new System.Drawing.Size(36, 16);
      this.lblAlpha.TabIndex = 0;
      this.lblAlpha.Text = "Alpha";
      // 
      // cbxColor
      // 
      this.cbxColor.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.cbxColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxColor.Location = new System.Drawing.Point(160, 122);
      this.cbxColor.MaxDropDownItems = 20;
      this.cbxColor.Name = "cbxColor";
      this.cbxColor.Size = new System.Drawing.Size(202, 21);
      this.cbxColor.TabIndex = 4;
      this.cbxColor.SelectedIndexChanged += new System.EventHandler(this.cbxColor_SelectedIndexChanged);
      // 
      // ColorPage
      // 
      this.Controls.Add(this.pnlRGB);
      this.Controls.Add(this.btnRGB);
      this.Controls.Add(this.cbxColor);
      this.Controls.Add(this.pnlColor);
      this.Controls.Add(this.btnCMYK);
      this.Controls.Add(this.btnGS);
      this.Controls.Add(this.pnlCMYK);
      this.Controls.Add(this.pnlGS);
      this.Controls.Add(this.udAlpha);
      this.Controls.Add(this.lblAlpha);
      this.Name = "ColorPage";
      this.Size = new System.Drawing.Size(380, 150);
      ((System.ComponentModel.ISupportInitialize)(this.udGS)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.udR)).EndInit();
      this.pnlRGB.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.udG)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.udB)).EndInit();
      this.pnlCMYK.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.udC)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.udM)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.udY)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.udK)).EndInit();
      this.pnlGS.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.udAlpha)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad (e);
      ModelToView();
    }

    private void udR_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.colorProperty.R = Convert.ToByte(this.udR.Value);
      //this.cbxColor.SelectedIndex = -1;
      this.cbxColor.Color = this.colorProperty.Color;
      OnUpdateDrawing();
    }

    private void udG_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.colorProperty.G = Convert.ToByte(this.udG.Value);
      //this.cbxColor.SelectedIndex = -1;
      this.cbxColor.Color = this.colorProperty.Color;
      OnUpdateDrawing();
    }

    private void udB_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.colorProperty.B = Convert.ToByte(this.udB.Value);
      //this.cbxColor.SelectedIndex = -1;
      this.cbxColor.Color = this.colorProperty.Color;
      OnUpdateDrawing();
    }

    private void udC_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.colorProperty.C = Convert.ToDouble(this.udC.Value);
      //this.cbxColor.SelectedIndex = -1;
      this.cbxColor.Color = this.colorProperty.Color;
      OnUpdateDrawing();
    }

    private void udM_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.colorProperty.M = Convert.ToDouble(this.udM.Value);
      //this.cbxColor.SelectedIndex = -1;
      this.cbxColor.Color = this.colorProperty.Color;
      OnUpdateDrawing();
    }

    private void udY_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.colorProperty.Y = Convert.ToDouble(this.udY.Value);
      //this.cbxColor.SelectedIndex = -1;
      this.cbxColor.Color = this.colorProperty.Color;
      OnUpdateDrawing();
    }

    private void udK_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.colorProperty.K = Convert.ToDouble(this.udK.Value);
      //this.cbxColor.SelectedIndex = -1;
      this.cbxColor.Color = this.colorProperty.Color;
      OnUpdateDrawing();
    }

    private void udGS_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.colorProperty.GS = Convert.ToDouble(this.udGS.Value);
      //this.cbxColor.SelectedIndex = -1;
      this.cbxColor.Color = this.colorProperty.Color;
      OnUpdateDrawing();
    }

    private void upAlpha_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.colorProperty.A = Convert.ToDouble(this.udAlpha.Value);
      //this.cbxColor.SelectedIndex = -1;
      this.cbxColor.Color = this.colorProperty.Color;
      OnUpdateDrawing();
    }

    private void cbxColor_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      if (this.cbxColor.SelectedIndex != -1)
      {
        //this.colorProperty.Color = XColor.FromName((string)this.cbxColor.SelectedItem);
        this.colorProperty.Color = this.cbxColor.Color;
        OnUpdateDrawing();
      }
    }

    private void ColorMode_CheckedChanged(object sender, EventArgs e)
    {

    }

  }
}
