using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PdfSharp.Explorer
{
  /// <summary>
  /// The password dialog.
  /// </summary>
  public class PasswordForm : System.Windows.Forms.Form
  {
    private System.Windows.Forms.Label lblMessage;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox tbxPassword;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.ComponentModel.Container components = null;

    public PasswordForm(string path)
    {
      InitializeComponent();
      this.lblMessage.Text = String.Format(this.lblMessage.Text, path);
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

    public string Password = "";

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.lblMessage = new System.Windows.Forms.Label();
      this.lblPassword = new System.Windows.Forms.Label();
      this.tbxPassword = new System.Windows.Forms.TextBox();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblMessage
      // 
      this.lblMessage.Location = new System.Drawing.Point(8, 8);
      this.lblMessage.Name = "lblMessage";
      this.lblMessage.Size = new System.Drawing.Size(388, 44);
      this.lblMessage.TabIndex = 0;
      this.lblMessage.Text = "\"{0}\" is protected. Enter a password to open the file.";
      // 
      // lblPassword
      // 
      this.lblPassword.Location = new System.Drawing.Point(88, 68);
      this.lblPassword.Name = "lblPassword";
      this.lblPassword.Size = new System.Drawing.Size(60, 16);
      this.lblPassword.TabIndex = 1;
      this.lblPassword.Text = "Password:";
      // 
      // tbxPassword
      // 
      this.tbxPassword.Location = new System.Drawing.Point(152, 64);
      this.tbxPassword.Name = "tbxPassword";
      this.tbxPassword.PasswordChar = '•';
      this.tbxPassword.Size = new System.Drawing.Size(168, 20);
      this.tbxPassword.TabIndex = 2;
      this.tbxPassword.Text = "";
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(236, 104);
      this.btnOK.Name = "btnOK";
      this.btnOK.TabIndex = 3;
      this.btnOK.Text = "OK";
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(320, 104);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.TabIndex = 3;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // PasswordForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(404, 133);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.tbxPassword);
      this.Controls.Add(this.lblPassword);
      this.Controls.Add(this.lblMessage);
      this.Controls.Add(this.btnCancel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "PasswordForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Password";
      this.ResumeLayout(false);

    }
    #endregion

    private void btnOK_Click(object sender, System.EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Password = this.tbxPassword.Text;
      this.Close();
    }

    private void btnCancel_Click(object sender, System.EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      this.Close();
    }
  }
}
