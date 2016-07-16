/*
 *  TVServerXBMC plugin for Team MediaPortal's TV-Server
 *  Copyright (C) 2010-2012 Marcel Groothuis
 *  http://www.github.com/margro
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * This CreateShareForm is based on the CreateShareForm from ARGUS TV
 * http://www.argus-tv.com
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TVServerKodi.Forms
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
