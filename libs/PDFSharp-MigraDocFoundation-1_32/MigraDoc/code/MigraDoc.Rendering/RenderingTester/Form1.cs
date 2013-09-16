using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using MigraDoc.Rendering;
using MigraDoc.Rendering.UnitTest;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;


namespace XGraphicsRendererTester
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class Form1 : System.Windows.Forms.Form
  {
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Button button4;
    private System.Windows.Forms.Button button5;
    private System.Windows.Forms.Button button6;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button button7;
    private System.Windows.Forms.Button button8;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Button button9;
    private System.Windows.Forms.Button button10;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Button Borders;
    private System.Windows.Forms.Button button11;
    private System.Windows.Forms.Button btnFile;
    private System.Windows.Forms.TextBox tbxDdlFile;
    private System.Windows.Forms.Button button12;
    private System.Windows.Forms.GroupBox groupBox4;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public Form1()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool _disposing)
    {
      if(_disposing)
      {
        if (components != null) 
        {
          components.Dispose();
        }
      }
      base.Dispose(_disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.button1 = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.button5 = new System.Windows.Forms.Button();
      this.button6 = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.button8 = new System.Windows.Forms.Button();
      this.button7 = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.button10 = new System.Windows.Forms.Button();
      this.button9 = new System.Windows.Forms.Button();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.button11 = new System.Windows.Forms.Button();
      this.Borders = new System.Windows.Forms.Button();
      this.btnFile = new System.Windows.Forms.Button();
      this.tbxDdlFile = new System.Windows.Forms.TextBox();
      this.button12 = new System.Windows.Forms.Button();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(20, 24);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(116, 24);
      this.button1.TabIndex = 0;
      this.button1.Text = "Iterator";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(464, 144);
      this.textBox1.Multiline = true;
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(252, 364);
      this.textBox1.TabIndex = 1;
      this.textBox1.Text = "";
      this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(20, 68);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(116, 24);
      this.button2.TabIndex = 2;
      this.button2.Text = "Text And Blanks";
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(24, 108);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(112, 24);
      this.button3.TabIndex = 3;
      this.button3.Text = "Formatted Text";
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(160, 24);
      this.button4.Name = "button4";
      this.button4.Size = new System.Drawing.Size(108, 24);
      this.button4.TabIndex = 4;
      this.button4.Text = "Alignment";
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // button5
      // 
      this.button5.Location = new System.Drawing.Point(160, 68);
      this.button5.Name = "button5";
      this.button5.Size = new System.Drawing.Size(108, 24);
      this.button5.TabIndex = 5;
      this.button5.Text = "Tabs";
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // button6
      // 
      this.button6.Location = new System.Drawing.Point(12, 24);
      this.button6.Name = "button6";
      this.button6.Size = new System.Drawing.Size(100, 24);
      this.button6.TabIndex = 6;
      this.button6.Text = "Dump Paragraph";
      this.button6.Click += new System.EventHandler(this.button6_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.button8);
      this.groupBox1.Controls.Add(this.button7);
      this.groupBox1.Controls.Add(this.button6);
      this.groupBox1.Location = new System.Drawing.Point(464, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(252, 132);
      this.groupBox1.TabIndex = 7;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Layout";
      // 
      // button8
      // 
      this.button8.Location = new System.Drawing.Point(136, 24);
      this.button8.Name = "button8";
      this.button8.Size = new System.Drawing.Size(104, 24);
      this.button8.TabIndex = 8;
      this.button8.Text = "1000 Paragraphs";
      this.button8.Click += new System.EventHandler(this.button8_Click);
      // 
      // button7
      // 
      this.button7.Location = new System.Drawing.Point(12, 64);
      this.button7.Name = "button7";
      this.button7.Size = new System.Drawing.Size(100, 28);
      this.button7.TabIndex = 7;
      this.button7.Text = "2 Paragraphs";
      this.button7.Click += new System.EventHandler(this.button7_Click);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.button10);
      this.groupBox2.Controls.Add(this.button9);
      this.groupBox2.Location = new System.Drawing.Point(12, 8);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(432, 132);
      this.groupBox2.TabIndex = 8;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Paragraphs";
      // 
      // button10
      // 
      this.button10.Location = new System.Drawing.Point(292, 20);
      this.button10.Name = "button10";
      this.button10.Size = new System.Drawing.Size(120, 24);
      this.button10.TabIndex = 1;
      this.button10.Text = "Fields";
      this.button10.Click += new System.EventHandler(this.button10_Click);
      // 
      // button9
      // 
      this.button9.Location = new System.Drawing.Point(148, 100);
      this.button9.Name = "button9";
      this.button9.Size = new System.Drawing.Size(108, 24);
      this.button9.TabIndex = 0;
      this.button9.Text = "Borders";
      this.button9.Click += new System.EventHandler(this.button9_Click);
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.button11);
      this.groupBox3.Controls.Add(this.Borders);
      this.groupBox3.Location = new System.Drawing.Point(16, 168);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(240, 180);
      this.groupBox3.TabIndex = 9;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Tables";
      // 
      // button11
      // 
      this.button11.Location = new System.Drawing.Point(148, 32);
      this.button11.Name = "button11";
      this.button11.TabIndex = 1;
      this.button11.Text = "Cell Merge";
      this.button11.Click += new System.EventHandler(this.button11_Click);
      // 
      // Borders
      // 
      this.Borders.Location = new System.Drawing.Point(12, 32);
      this.Borders.Name = "Borders";
      this.Borders.Size = new System.Drawing.Size(108, 24);
      this.Borders.TabIndex = 0;
      this.Borders.Text = "Borders, Shading";
      this.Borders.Click += new System.EventHandler(this.Borders_Click);
      // 
      // btnFile
      // 
      this.btnFile.Location = new System.Drawing.Point(216, 376);
      this.btnFile.Name = "btnFile";
      this.btnFile.Size = new System.Drawing.Size(28, 20);
      this.btnFile.TabIndex = 10;
      this.btnFile.Text = "...";
      this.btnFile.Click += new System.EventHandler(this.btnFile_Click);
      // 
      // tbxDdlFile
      // 
      this.tbxDdlFile.Location = new System.Drawing.Point(16, 376);
      this.tbxDdlFile.Name = "tbxDdlFile";
      this.tbxDdlFile.Size = new System.Drawing.Size(188, 20);
      this.tbxDdlFile.TabIndex = 11;
      this.tbxDdlFile.Text = "";
      // 
      // button12
      // 
      this.button12.Location = new System.Drawing.Point(20, 412);
      this.button12.Name = "button12";
      this.button12.Size = new System.Drawing.Size(60, 20);
      this.button12.TabIndex = 12;
      this.button12.Text = "PDF";
      this.button12.Click += new System.EventHandler(this.button12_Click);
      // 
      // groupBox4
      // 
      this.groupBox4.Location = new System.Drawing.Point(12, 356);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(248, 140);
      this.groupBox4.TabIndex = 13;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "DDL-Rendering";
      // 
      // Form1
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(740, 522);
      this.Controls.Add(this.button12);
      this.Controls.Add(this.tbxDdlFile);
      this.Controls.Add(this.btnFile);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.groupBox4);
      this.Name = "Form1";
      this.Text = "Form1";
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() 
    {
      Application.Run(new Form1());
//      Document document = new Document();
//      Section  section = new Section();
//      Paragraph paragraph = new Paragraph();
//      paragraph.AddText("This is a Text");
//      section.Add(paragraph);
//      document.Add(section);
//
//      PdfPrinter printer = new PdfPrinter();
//      printer.Document = document;
//      printer.PrintDocument();
    }

    private void button1_Click(object sender, System.EventArgs e)
    {
      Document document = new Document();
      Section  section = document.AddSection();
      Paragraph paragraph = section.AddParagraph();
      paragraph.AddText("Hallo");
      FormattedText formText = paragraph.AddFormattedText("formattedText", TextFormat.Bold);
      formText.AddFormattedText("formattedTextNested", TextFormat.Italic);
      formText.AddDateField();
      this.textBox1.Text = TestParagraphIterator.GetBackIterators(paragraph);
    }

    private void textBox1_TextChanged(object sender, System.EventArgs e)
    {
    
    }

    private void button2_Click(object sender, System.EventArgs e)
    {
      TestParagraphRenderer.TextAndBlanks("egal.pdf");
      System.Diagnostics.Process.Start("egal.pdf");
    }

    private void button3_Click(object sender, System.EventArgs e)
    {
      TestParagraphRenderer.Formatted("egal.pdf");
      System.Diagnostics.Process.Start("egal.pdf");
    }

    private void button4_Click(object sender, System.EventArgs e)
    {
      TestParagraphRenderer.Alignment("egal.pdf");
      System.Diagnostics.Process.Start("egal.pdf");
    }

    private void button5_Click(object sender, System.EventArgs e)
    {
      TestParagraphRenderer.Tabs("egal.pdf");
      System.Diagnostics.Process.Start("egal.pdf");
    }

    private void button6_Click(object sender, System.EventArgs e)
    {
      this.textBox1.Text = TestLayout.DumpParagraph();
    }

    private void button7_Click(object sender, System.EventArgs e)
    {
      TestLayout.TwoParagraphs("egal.pdf");
      System.Diagnostics.Process.Start("egal.pdf");
    }

    private void button8_Click(object sender, System.EventArgs e)
    {
      TestLayout.A1000Paragraphs("egal.pdf");
      System.Diagnostics.Process.Start("egal.pdf");
    }

    private void button9_Click(object sender, System.EventArgs e)
    {
      TestParagraphRenderer.Borders("egal.pdf");
      System.Diagnostics.Process.Start("egal.pdf");
    }

    private void button10_Click(object sender, System.EventArgs e)
    {
      TestParagraphRenderer.Fields("egal.pdf");
      System.Diagnostics.Process.Start("egal.pdf");
    }

    private void Borders_Click(object sender, System.EventArgs e)
    {
      TestTable.Borders("egal.pdf");
      System.Diagnostics.Process.Start("egal.pdf");
    }

    private void button11_Click(object sender, System.EventArgs e)
    {
      TestTable.CellMerge("egal.pdf");
      System.Diagnostics.Process.Start("egal.pdf");
    }

    private void btnFile_Click(object sender, System.EventArgs e)
    {
      OpenFileDialog fileDialog = new OpenFileDialog();
      if (fileDialog.ShowDialog() == DialogResult.OK)
      {
        this.tbxDdlFile.Text = fileDialog.FileName;
      }
    }

    private void button12_Click(object sender, System.EventArgs e)
    {
      if (this.tbxDdlFile.Text != "")
      {
        try
        {
          Document doc = DdlReader.DocumentFromFile(tbxDdlFile.Text);
          PdfPrinter pdfPrinter = new PdfPrinter();
          pdfPrinter.Document = doc;
          pdfPrinter.PrintDocument();
          pdfPrinter.PdfDocument.Save("egal.pdf");
          System.Diagnostics.Process.Start("egal.pdf");
        }
        catch(Exception exc)
        {
          MessageBox.Show(exc.Message);
        }
      }
    }
  }
}
