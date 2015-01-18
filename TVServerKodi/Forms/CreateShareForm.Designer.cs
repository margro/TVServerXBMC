namespace TVServerXBMC.Forms
{
  partial class CreateShareForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
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
      this.cancelButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.shareNameTextBox = new System.Windows.Forms.TextBox();
      this._shareLabel = new System.Windows.Forms.Label();
      this.localPathLabel = new System.Windows.Forms.Label();
      this._pathLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // cancelButton
      // 
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(302, 65);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 11;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Location = new System.Drawing.Point(221, 65);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 10;
      this.okButton.Text = "OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // shareNameTextBox
      // 
      this.shareNameTextBox.Location = new System.Drawing.Point(80, 31);
      this.shareNameTextBox.Name = "shareNameTextBox";
      this.shareNameTextBox.Size = new System.Drawing.Size(297, 20);
      this.shareNameTextBox.TabIndex = 9;
      // 
      // _shareLabel
      // 
      this._shareLabel.AutoSize = true;
      this._shareLabel.Location = new System.Drawing.Point(7, 34);
      this._shareLabel.Name = "_shareLabel";
      this._shareLabel.Size = new System.Drawing.Size(67, 13);
      this._shareLabel.TabIndex = 8;
      this._shareLabel.Text = "Share name:";
      // 
      // localPathLabel
      // 
      this.localPathLabel.AutoEllipsis = true;
      this.localPathLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.localPathLabel.Location = new System.Drawing.Point(80, 8);
      this.localPathLabel.Name = "localPathLabel";
      this.localPathLabel.Size = new System.Drawing.Size(297, 20);
      this.localPathLabel.TabIndex = 7;
      // 
      // _pathLabel
      // 
      this._pathLabel.AutoSize = true;
      this._pathLabel.Location = new System.Drawing.Point(7, 8);
      this._pathLabel.Name = "_pathLabel";
      this._pathLabel.Size = new System.Drawing.Size(60, 13);
      this._pathLabel.TabIndex = 6;
      this._pathLabel.Text = "Local path:";
      // 
      // CreateShareForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(394, 104);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.shareNameTextBox);
      this.Controls.Add(this._shareLabel);
      this.Controls.Add(this.localPathLabel);
      this.Controls.Add(this._pathLabel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Name = "CreateShareForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Create UNC Share";
      this.Load += new System.EventHandler(this.CreateShareForm_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.TextBox shareNameTextBox;
    private System.Windows.Forms.Label _shareLabel;
    private System.Windows.Forms.Label localPathLabel;
    private System.Windows.Forms.Label _pathLabel;
  }
}