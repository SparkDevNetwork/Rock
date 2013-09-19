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
  public class PenPage : System.Windows.Forms.UserControl
  {
    private System.Windows.Forms.Label lblWidth;
    private XDrawing.TestLab.FormPages.ColorPage colorPage;
    private System.Windows.Forms.NumericUpDown udWidth;
    private System.Windows.Forms.ComboBox cbxStyle;
    private System.Windows.Forms.ComboBox cbxLineCap;
    private System.Windows.Forms.ComboBox cbxLineJoin;
    private System.Windows.Forms.Label lblStyle;
    private System.Windows.Forms.Label lblLineCap;
    private System.Windows.Forms.Label lblLineJoin;
    private System.ComponentModel.Container components = null;

    public PenPage()
    {
      InitializeComponent();
      UITools.SetTabPageColor(this);
      this.colorPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
    }

    void ModelToView()
    {
      if (this.inModelToView || DesignMode)
        return;

      this.udWidth.Value = (decimal)this.penProperty.Width;
      this.cbxStyle.SelectedIndex = (int)this.penProperty.DashStyle;
      this.cbxLineCap.SelectedIndex = (int)this.penProperty.LineCap;
      this.cbxLineJoin.SelectedIndex = (int)this.penProperty.LineJoin;
      //this.cbxStyle.SelectedIndex = (int)this.penProperty.l

      this.inModelToView = false;
    }
    bool inModelToView;

    public event UpdateDrawing UpdateDrawing;

    void OnUpdateDrawing()
    {
      if (UpdateDrawing != null)
        UpdateDrawing();
    }

    public PenProperty PenProperty
    {
      get {return this.penProperty;}
      set 
      {
        this.penProperty = value;
        this.colorPage.ColorProperty = value.Color;
        ModelToView();
      }
    }
    PenProperty penProperty;

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
      this.lblWidth = new System.Windows.Forms.Label();
      this.udWidth = new System.Windows.Forms.NumericUpDown();
      this.lblStyle = new System.Windows.Forms.Label();
      this.cbxStyle = new System.Windows.Forms.ComboBox();
      this.lblLineCap = new System.Windows.Forms.Label();
      this.cbxLineCap = new System.Windows.Forms.ComboBox();
      this.lblLineJoin = new System.Windows.Forms.Label();
      this.cbxLineJoin = new System.Windows.Forms.ComboBox();
      ((System.ComponentModel.ISupportInitialize)(this.udWidth)).BeginInit();
      this.SuspendLayout();
      // 
      // colorPage
      // 
      this.colorPage.ColorProperty = null;
      this.colorPage.Location = new System.Drawing.Point(0, 0);
      this.colorPage.Name = "colorPage";
      this.colorPage.Size = new System.Drawing.Size(380, 148);
      this.colorPage.TabIndex = 0;
      // 
      // lblWidth
      // 
      this.lblWidth.Location = new System.Drawing.Point(12, 164);
      this.lblWidth.Name = "lblWidth";
      this.lblWidth.Size = new System.Drawing.Size(52, 16);
      this.lblWidth.TabIndex = 1;
      this.lblWidth.Text = "&Width:";
      // 
      // udWidth
      // 
      this.udWidth.DecimalPlaces = 1;
      this.udWidth.Increment = new System.Decimal(new int[] {
                                                              1,
                                                              0,
                                                              0,
                                                              65536});
      this.udWidth.Location = new System.Drawing.Point(72, 160);
      this.udWidth.Name = "udWidth";
      this.udWidth.Size = new System.Drawing.Size(56, 20);
      this.udWidth.TabIndex = 2;
      this.udWidth.ValueChanged += new System.EventHandler(this.udWidth_ValueChanged);
      // 
      // lblStyle
      // 
      this.lblStyle.Location = new System.Drawing.Point(12, 192);
      this.lblStyle.Name = "lblStyle";
      this.lblStyle.Size = new System.Drawing.Size(52, 16);
      this.lblStyle.TabIndex = 1;
      this.lblStyle.Text = "&Style:";
      // 
      // cbxStyle
      // 
      this.cbxStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxStyle.Items.AddRange(new object[] {
                                                  "Solid (0)",
                                                  "Dash (1)",
                                                  "Dot (2)",
                                                  "DashDot (3)",
                                                  "DashDotDot (4)"});
      this.cbxStyle.Location = new System.Drawing.Point(72, 188);
      this.cbxStyle.Name = "cbxStyle";
      this.cbxStyle.Size = new System.Drawing.Size(92, 21);
      this.cbxStyle.TabIndex = 3;
      this.cbxStyle.SelectedIndexChanged += new System.EventHandler(this.cbxStyle_SelectedIndexChanged);
      // 
      // lblLineCap
      // 
      this.lblLineCap.Location = new System.Drawing.Point(200, 160);
      this.lblLineCap.Name = "lblLineCap";
      this.lblLineCap.Size = new System.Drawing.Size(52, 16);
      this.lblLineCap.TabIndex = 1;
      this.lblLineCap.Text = "LineCap:";
      // 
      // cbxLineCap
      // 
      this.cbxLineCap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxLineCap.Items.AddRange(new object[] {
                                                    "Flat (0)",
                                                    "Round (1)",
                                                    "Square (2)"});
      this.cbxLineCap.Location = new System.Drawing.Point(260, 156);
      this.cbxLineCap.Name = "cbxLineCap";
      this.cbxLineCap.Size = new System.Drawing.Size(92, 21);
      this.cbxLineCap.TabIndex = 3;
      this.cbxLineCap.SelectedIndexChanged += new System.EventHandler(this.cbxLineCap_SelectedIndexChanged);
      // 
      // lblLineJoin
      // 
      this.lblLineJoin.Location = new System.Drawing.Point(200, 188);
      this.lblLineJoin.Name = "lblLineJoin";
      this.lblLineJoin.Size = new System.Drawing.Size(52, 16);
      this.lblLineJoin.TabIndex = 1;
      this.lblLineJoin.Text = "&LineJoin:";
      // 
      // cbxLineJoin
      // 
      this.cbxLineJoin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxLineJoin.Items.AddRange(new object[] {
                                                     "Miter (0)",
                                                     "Round (1)",
                                                     "Bevel (2)"});
      this.cbxLineJoin.Location = new System.Drawing.Point(260, 184);
      this.cbxLineJoin.Name = "cbxLineJoin";
      this.cbxLineJoin.Size = new System.Drawing.Size(92, 21);
      this.cbxLineJoin.TabIndex = 3;
      this.cbxLineJoin.SelectedIndexChanged += new System.EventHandler(this.cbxLineJoin_SelectedIndexChanged);
      // 
      // PenPage
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.cbxStyle);
      this.Controls.Add(this.udWidth);
      this.Controls.Add(this.lblWidth);
      this.Controls.Add(this.colorPage);
      this.Controls.Add(this.lblStyle);
      this.Controls.Add(this.lblLineCap);
      this.Controls.Add(this.cbxLineCap);
      this.Controls.Add(this.lblLineJoin);
      this.Controls.Add(this.cbxLineJoin);
      this.Name = "PenPage";
      this.Size = new System.Drawing.Size(380, 240);
      ((System.ComponentModel.ISupportInitialize)(this.udWidth)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad (e);
      ModelToView();
    }

    private void udWidth_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.penProperty.Width = Convert.ToSingle(this.udWidth.Value);
      OnUpdateDrawing();
    }

    private void cbxStyle_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.penProperty.DashStyle = (XDashStyle)this.cbxStyle.SelectedIndex;
      OnUpdateDrawing();
    }

    private void cbxLineCap_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.penProperty.LineCap = (XLineCap)this.cbxLineCap.SelectedIndex;
      OnUpdateDrawing();
    }

    private void cbxLineJoin_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.penProperty.LineJoin = (XLineJoin)this.cbxLineJoin.SelectedIndex;
      OnUpdateDrawing();
    }
  }
}
