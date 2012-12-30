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
 * This SetupForm is based on the SetupForm from ARGUS TV
 * http://www.argus-tv.com
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using TvLibrary.Log;
using TvControl;

using TVServerXBMC.Common;

namespace TVServerXBMC.Forms
{
  public partial class SetupForm : SetupTv.SectionSettings
  {
    public SetupForm()
    {
      InitializeComponent();
      uncRecordingPathsBindingSource.DataSource = typeof(List<UncPathItem>);
    }

    #region Properties
    private TVServerXBMCPlugin m_plugin;

    public TVServerXBMCPlugin Plugin
    {
      get { return m_plugin; }
      set { m_plugin = value; }
    }
    #endregion

    #region SetupTv.SectionSettings

    public override void OnSectionActivated()
    {
      Log.Info("TVServerXBMC: Configuration activated");

      m_plugin.LoadSettings();
      portNumericUpDown.Value = m_plugin.Port;

      LoadUncPaths();

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      Log.Info("TVServerXBMC: Configuration deactivated");

      m_plugin.Port = (int) portNumericUpDown.Value;
      m_plugin.SaveSettings();

      base.OnSectionDeActivated();
    }

    #endregion

    private void LoadUncPaths()
    {
      Log.Info("TVServerXBMC: Loading UNC paths");
      try
      {
        bool anyError = false;

        List<UncPathItem> pathItems = new List<UncPathItem>();
        List<UncPathItem> tsPathItems = new List<UncPathItem>();

        List<TvDatabase.Card> mediaPortalCards = Utility.GetAllCards();
        foreach (TvDatabase.Card card in mediaPortalCards)
        {
          Log.Info("Card: " + card.Name + " " + card.RecordingFolder + " " + card.TimeShiftFolder);
          anyError = anyError | AddUncPathItem(pathItems, card.Name, card.RecordingFolder);
          anyError = anyError | AddUncPathItem(tsPathItems, card.Name, card.TimeShiftFolder);
        }

        uncRecordingPathsBindingSource.DataSource = pathItems;
        uncTimeshiftPathsBindingSource.DataSource = tsPathItems;
        EnableUncButtons();

        if (anyError)
        {
          MessageBox.Show(this, "You must set up at least 2 shares with full permissions to access the recordings and timeshift folders on remote XBMC clients!", null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private static bool AddUncPathItem(List<UncPathItem> pathItems, string name, string path)
    {
      bool hasError = true;
      string message;

      string uncPath = ShareExplorer.GetUncPathForLocalPath(path);
      if (!String.IsNullOrEmpty(uncPath))
      {
        Log.Info("TVServerXBMC: " + path + " => " + uncPath);
        message = uncPath;
        path = uncPath;
        hasError = false;
      }
      else
      {
        message = path;
        hasError = true;
      }

      pathItems.Add(new UncPathItem(name, path, message, hasError));

      return hasError;
    }

    private void EnableUncButtons()
    {
      if (uncRecordingPathsDataGrid.SelectedRows.Count > 0)
      {
        UncPathItem linkItem = uncRecordingPathsDataGrid.SelectedRows[0].DataBoundItem as UncPathItem;
        createRecordingsShareButton.Enabled = true;
      }
      else
      {
        createRecordingsShareButton.Enabled = false;
      }
      if (uncTimeshiftPathsDataGrid.SelectedRows.Count > 0)
      {
        UncPathItem linkItem = uncTimeshiftPathsDataGrid.SelectedRows[0].DataBoundItem as UncPathItem;
        createTimeshiftShareButton.Enabled = true;
      }
      else
      {
        createTimeshiftShareButton.Enabled = false;
      }
    }

    private class UncPathItem
    {
      private string _cardName;
      private string _recordingPath;
      private string _message;
      private bool _hasError;

      public UncPathItem(string cardName, string recordingPath, string message, bool hasError)
      {
        _cardName = cardName;
        _recordingPath = recordingPath;
        _message = message;
        _hasError = hasError;
      }

      public string CardName
      {
        get { return _cardName; }
      }

      public string RecordingPath
      {
        get { return _recordingPath; }
      }

      public string Message
      {
        get { return _message; }
      }

      public bool HasError
      {
        get { return _hasError; }
      }
    }

    private void createRecordingsShareButton_Click(object sender, EventArgs e)
    {
      if (uncRecordingPathsDataGrid.SelectedRows.Count > 0)
      {
        ShowCreateShareForm(uncRecordingPathsDataGrid.SelectedRows[0].DataBoundItem as UncPathItem);
      }
    }

    private void createTimeshiftShareButton_Click(object sender, EventArgs e)
    {
      if (uncTimeshiftPathsDataGrid.SelectedRows.Count > 0)
      {
        ShowCreateShareForm(uncTimeshiftPathsDataGrid.SelectedRows[0].DataBoundItem as UncPathItem);
      }
    }

    private void refreshUncButton_Click(object sender, EventArgs e)
    {
      LoadUncPaths();
    }

    private void uncRecordingPathsDataGrid_SelectionChanged(object sender, EventArgs e)
    {
      EnableUncButtons();
    }

    private void uncRecordingPathsDataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      DataGridView dataGridView = sender as DataGridView;
      if (e.ColumnIndex == 1
          && e.RowIndex >= 0
          && e.RowIndex < uncRecordingPathsBindingSource.Count)
      {
        UncPathItem linkItem = dataGridView.Rows[e.RowIndex].DataBoundItem as UncPathItem;
        if (linkItem != null
            && linkItem.HasError)
        {
          e.CellStyle.ForeColor = Color.Red;
          e.CellStyle.SelectionForeColor = Color.Red;
        }
        else
        {
          e.CellStyle.ForeColor = Color.DarkGreen;
          e.CellStyle.SelectionForeColor = Color.DarkGreen;
        }
      }
    }

    private void uncTimeshiftPathsDataGrid_SelectionChanged(object sender, EventArgs e)
    {
      EnableUncButtons();
    }

    private void uncTimeshiftPathsDataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      DataGridView dataGridView = sender as DataGridView;
      if (e.ColumnIndex == 1
          && e.RowIndex >= 0
          && e.RowIndex < uncTimeshiftPathsBindingSource.Count)
      {
        UncPathItem linkItem = dataGridView.Rows[e.RowIndex].DataBoundItem as UncPathItem;
        if (linkItem != null
            && linkItem.HasError)
        {
          e.CellStyle.ForeColor = Color.Red;
          e.CellStyle.SelectionForeColor = Color.Red;
        }
        else
        {
          e.CellStyle.ForeColor = Color.DarkGreen;
          e.CellStyle.SelectionForeColor = Color.DarkGreen;
        }
      }

    }

    private void ShowCreateShareForm(UncPathItem linkItem)
    {
      CreateShareForm form = new CreateShareForm();
      form.LocalPath = linkItem.RecordingPath;
      if (form.ShowDialog(this) == DialogResult.OK)
      {
        LoadUncPaths();
      }
    }
  }
}
