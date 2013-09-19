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
  /// Font properties.
  /// </summary>
  public class FontPage : System.Windows.Forms.UserControl
  {
    private XDrawing.TestLab.FormPages.ColorPage colorPage;
    private System.Windows.Forms.Label lblFamily;
    private System.Windows.Forms.CheckBox btnBold;
    private System.Windows.Forms.CheckBox btnItalic;
    private System.Windows.Forms.ComboBox cbxFamily;
    private System.Windows.Forms.NumericUpDown udSize;
    private System.Windows.Forms.Label lblSize;
    private System.Windows.Forms.TextBox tbxText;
    private System.Windows.Forms.Label lblSupportedStyles;
    private System.Windows.Forms.CheckBox btnUnderline;
    private System.Windows.Forms.CheckBox btnStrikeout;
    private System.Windows.Forms.CheckBox btnUnicode;
    private CheckBox btnEmbed;
    private System.ComponentModel.Container components = null;

    public FontPage()
    {
      InitializeComponent();
      UITools.SetTabPageColor(this);
      this.colorPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);

      FontFamily[] family1 = FontFamily.Families;
      //FontFamily[] family2 = FontFamily.GetFamilies(Graphics.FromHwnd(IntPtr.Zero));
      FontFamily[] family2 = FontFamily.Families;

      FontFamily[] families = FontFamily.Families;
      foreach (FontFamily family in families)
        this.cbxFamily.Items.Add(family.Name);
    }

    void ModelToView()
    {
      if (this.inModelToView || DesignMode)
        return;

      this.cbxFamily.SelectedItem = this.fontProperty.FamilyName;
      this.udSize.Value = (decimal)this.fontProperty.Size;
      this.btnBold.Checked = this.fontProperty.Bold;
      this.btnItalic.Checked = this.fontProperty.Italic;
      this.btnUnderline.Checked = this.fontProperty.Underline;
      this.btnStrikeout.Checked = this.fontProperty.Strikeout;
      this.btnUnicode.Checked = this.fontProperty.Unicode;
      this.btnEmbed.Checked = this.fontProperty.Embed;
      this.tbxText.Text = this.fontProperty.Text;

      this.inModelToView = false;
    }
    bool inModelToView;

    public event UpdateDrawing UpdateDrawing;

    void OnUpdateDrawing()
    {
      if (UpdateDrawing != null)
        UpdateDrawing();
    }

    public FontProperty FontProperty
    {
      get {return this.fontProperty;}
      set 
      {
        this.fontProperty = value;
        this.colorPage.ColorProperty = value.Color;
      }
    }
    FontProperty fontProperty;

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
      this.lblFamily = new System.Windows.Forms.Label();
      this.cbxFamily = new System.Windows.Forms.ComboBox();
      this.btnBold = new System.Windows.Forms.CheckBox();
      this.btnItalic = new System.Windows.Forms.CheckBox();
      this.udSize = new System.Windows.Forms.NumericUpDown();
      this.lblSize = new System.Windows.Forms.Label();
      this.tbxText = new System.Windows.Forms.TextBox();
      this.lblSupportedStyles = new System.Windows.Forms.Label();
      this.btnUnderline = new System.Windows.Forms.CheckBox();
      this.btnStrikeout = new System.Windows.Forms.CheckBox();
      this.btnUnicode = new System.Windows.Forms.CheckBox();
      this.btnEmbed = new System.Windows.Forms.CheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.udSize)).BeginInit();
      this.SuspendLayout();
      // 
      // colorPage
      // 
      this.colorPage.ColorProperty = null;
      this.colorPage.Location = new System.Drawing.Point(0, 0);
      this.colorPage.Name = "colorPage";
      this.colorPage.Size = new System.Drawing.Size(380, 148);
      this.colorPage.TabIndex = 4;
      // 
      // lblFamily
      // 
      this.lblFamily.Location = new System.Drawing.Point(8, 156);
      this.lblFamily.Name = "lblFamily";
      this.lblFamily.Size = new System.Drawing.Size(48, 16);
      this.lblFamily.TabIndex = 1;
      this.lblFamily.Text = "Family";
      // 
      // cbxFamily
      // 
      this.cbxFamily.Location = new System.Drawing.Point(60, 152);
      this.cbxFamily.Name = "cbxFamily";
      this.cbxFamily.Size = new System.Drawing.Size(192, 21);
      this.cbxFamily.TabIndex = 2;
      this.cbxFamily.SelectedIndexChanged += new System.EventHandler(this.cbxFamily_SelectedIndexChanged);
      // 
      // btnBold
      // 
      this.btnBold.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnBold.Location = new System.Drawing.Point(260, 152);
      this.btnBold.Name = "btnBold";
      this.btnBold.Size = new System.Drawing.Size(52, 20);
      this.btnBold.TabIndex = 3;
      this.btnBold.Text = "Bold";
      this.btnBold.CheckedChanged += new System.EventHandler(this.btnBold_CheckedChanged);
      // 
      // btnItalic
      // 
      this.btnItalic.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnItalic.Location = new System.Drawing.Point(320, 152);
      this.btnItalic.Name = "btnItalic";
      this.btnItalic.Size = new System.Drawing.Size(48, 20);
      this.btnItalic.TabIndex = 3;
      this.btnItalic.Text = "Italic";
      this.btnItalic.CheckedChanged += new System.EventHandler(this.btnItalic_CheckedChanged);
      // 
      // udSize
      // 
      this.udSize.ContextMenu = null;
      this.udSize.DecimalPlaces = 1;
      this.udSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
      this.udSize.Location = new System.Drawing.Point(60, 200);
      this.udSize.Name = "udSize";
      this.udSize.Size = new System.Drawing.Size(52, 20);
      this.udSize.TabIndex = 5;
      this.udSize.ValueChanged += new System.EventHandler(this.udSize_ValueChanged);
      // 
      // lblSize
      // 
      this.lblSize.Location = new System.Drawing.Point(8, 204);
      this.lblSize.Name = "lblSize";
      this.lblSize.Size = new System.Drawing.Size(40, 16);
      this.lblSize.TabIndex = 1;
      this.lblSize.Text = "Size";
      // 
      // tbxText
      // 
      this.tbxText.Location = new System.Drawing.Point(8, 220);
      this.tbxText.Name = "tbxText";
      this.tbxText.Size = new System.Drawing.Size(364, 20);
      this.tbxText.TabIndex = 6;
      this.tbxText.Text = "AaBbCcfg‚¬ƒ÷‹";
      this.tbxText.TextChanged += new System.EventHandler(this.tbxText_TextChanged);
      // 
      // lblSupportedStyles
      // 
      this.lblSupportedStyles.Location = new System.Drawing.Point(4, 180);
      this.lblSupportedStyles.Name = "lblSupportedStyles";
      this.lblSupportedStyles.Size = new System.Drawing.Size(364, 16);
      this.lblSupportedStyles.TabIndex = 7;
      this.lblSupportedStyles.Text = "<SupportedStyles>";
      // 
      // btnUnderline
      // 
      this.btnUnderline.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnUnderline.Location = new System.Drawing.Point(116, 200);
      this.btnUnderline.Name = "btnUnderline";
      this.btnUnderline.Size = new System.Drawing.Size(72, 20);
      this.btnUnderline.TabIndex = 3;
      this.btnUnderline.Text = "Underline";
      this.btnUnderline.CheckedChanged += new System.EventHandler(this.btnUnderline_CheckedChanged);
      // 
      // btnStrikeout
      // 
      this.btnStrikeout.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnStrikeout.Location = new System.Drawing.Point(184, 200);
      this.btnStrikeout.Name = "btnStrikeout";
      this.btnStrikeout.Size = new System.Drawing.Size(68, 20);
      this.btnStrikeout.TabIndex = 3;
      this.btnStrikeout.Text = "Strikeout";
      this.btnStrikeout.CheckedChanged += new System.EventHandler(this.btnStrikeout_CheckedChanged);
      // 
      // btnUnicode
      // 
      this.btnUnicode.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnUnicode.Location = new System.Drawing.Point(258, 200);
      this.btnUnicode.Name = "btnUnicode";
      this.btnUnicode.Size = new System.Drawing.Size(68, 20);
      this.btnUnicode.TabIndex = 3;
      this.btnUnicode.Text = "Unicode";
      this.btnUnicode.CheckedChanged += new System.EventHandler(this.btnUnicode_CheckedChanged);
      // 
      // btnEmbed
      // 
      this.btnEmbed.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnEmbed.Location = new System.Drawing.Point(332, 200);
      this.btnEmbed.Name = "btnEmbed";
      this.btnEmbed.Size = new System.Drawing.Size(68, 20);
      this.btnEmbed.TabIndex = 3;
      this.btnEmbed.Text = "Embed";
      this.btnEmbed.CheckedChanged += new System.EventHandler(this.btnEmbed_CheckedChanged);
      // 
      // FontPage
      // 
      this.Controls.Add(this.lblSupportedStyles);
      this.Controls.Add(this.tbxText);
      this.Controls.Add(this.udSize);
      this.Controls.Add(this.colorPage);
      this.Controls.Add(this.btnBold);
      this.Controls.Add(this.cbxFamily);
      this.Controls.Add(this.lblFamily);
      this.Controls.Add(this.btnItalic);
      this.Controls.Add(this.lblSize);
      this.Controls.Add(this.btnUnderline);
      this.Controls.Add(this.btnStrikeout);
      this.Controls.Add(this.btnEmbed);
      this.Controls.Add(this.btnUnicode);
      this.Name = "FontPage";
      this.Size = new System.Drawing.Size(380, 248);
      ((System.ComponentModel.ISupportInitialize)(this.udSize)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad (e);
      ModelToView();
    }

    private void cbxFamily_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      int idx = this.cbxFamily.SelectedIndex;
      if (idx >= 0)
      {
        this.fontProperty.FamilyName = this.cbxFamily.SelectedItem.ToString();
        FontFamily family = new FontFamily(this.fontProperty.FamilyName);
        
        string supportedSyles = "";
        if (family.IsStyleAvailable(FontStyle.Regular))
          supportedSyles += "Regualr, ";
        if (family.IsStyleAvailable(FontStyle.Bold))
          supportedSyles += "Bold, ";
        if (family.IsStyleAvailable(FontStyle.Italic))
          supportedSyles += "Italic, ";
        if (family.IsStyleAvailable(FontStyle.Bold | FontStyle.Italic))
          supportedSyles += "BoldItalic, ";
        if (family.IsStyleAvailable(FontStyle.Strikeout))
          supportedSyles += "Strikeout, ";
        if (family.IsStyleAvailable(FontStyle.Underline))
          supportedSyles += "Underline, ";
        this.lblSupportedStyles.Text = supportedSyles;

        OnUpdateDrawing();
      }
    }

    private void udSize_ValueChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.fontProperty.Size = Convert.ToSingle(this.udSize.Value);
      OnUpdateDrawing();
    }

    private void btnBold_CheckedChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.fontProperty.Bold = this.btnBold.Checked;
      OnUpdateDrawing();
    }

    private void btnItalic_CheckedChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.fontProperty.Italic = this.btnItalic.Checked;
      OnUpdateDrawing();
    }

    private void btnUnderline_CheckedChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.fontProperty.Underline = this.btnUnderline.Checked;
      OnUpdateDrawing();
    }

    private void btnStrikeout_CheckedChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.fontProperty.Strikeout = this.btnStrikeout.Checked;
      OnUpdateDrawing();
    }

    private void btnUnicode_CheckedChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.fontProperty.Unicode = this.btnUnicode.Checked;
      OnUpdateDrawing();
    }

    private void btnEmbed_CheckedChanged(object sender, EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.fontProperty.Embed = this.btnEmbed.Checked;
      OnUpdateDrawing();
    }

    private void tbxText_TextChanged(object sender, System.EventArgs e)
    {
      if (this.inModelToView)
        return;
      this.fontProperty.Text = this.tbxText.Text;
      OnUpdateDrawing();
    }

  }
}
