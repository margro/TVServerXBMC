using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TVServerXBMC.Forms
{
  public partial class CreateShareForm : Form
  {
    public CreateShareForm()
    {
      InitializeComponent();
    }

    private string m_localPath;

    public string LocalPath
    {
      get { return m_localPath; }
      set { m_localPath = value; }
    }

    private void CreateShareForm_Load(object sender, EventArgs e)
    {
      localPathLabel.Text = m_localPath;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      if (shareNameTextBox.Text.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
      {
        MessageBox.Show(this, "Invalid share name, don't use special characters.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      try
      {
        if (Utility.CreateUncShare(shareNameTextBox.Text.Trim(), m_localPath))
        {
          this.DialogResult = DialogResult.OK;
          Close();
        }
        else
        {
          MessageBox.Show(this, "Failed to create share, name not unique?", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
      Close();
    }
  }
}
