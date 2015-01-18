namespace TVServerXBMC.Forms
{
  partial class SetupForm
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.portNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this._tcpPortLabel = new System.Windows.Forms.Label();
      this.uncPathsTabPage = new System.Windows.Forms.TabPage();
      this.refreshUncButton = new System.Windows.Forms.Button();
      this.uncTimeshiftGroupBox = new System.Windows.Forms.GroupBox();
      this.createTimeshiftShareButton = new System.Windows.Forms.Button();
      this.uncTimeshiftPathsDataGrid = new System.Windows.Forms.DataGridView();
      this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.uncTimeshiftPathsBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.uncRecordingGroupBox = new System.Windows.Forms.GroupBox();
      this.createRecordingsShareButton = new System.Windows.Forms.Button();
      this.uncRecordingPathsDataGrid = new System.Windows.Forms.DataGridView();
      this.Card = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.uncRecordingPathsBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.portNumericUpDown)).BeginInit();
      this.uncPathsTabPage.SuspendLayout();
      this.uncTimeshiftGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.uncTimeshiftPathsDataGrid)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.uncTimeshiftPathsBindingSource)).BeginInit();
      this.uncRecordingGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.uncRecordingPathsDataGrid)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.uncRecordingPathsBindingSource)).BeginInit();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Controls.Add(this.uncPathsTabPage);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(559, 400);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.groupBox1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(551, 374);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Configuration";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.portNumericUpDown);
      this.groupBox1.Controls.Add(this._tcpPortLabel);
      this.groupBox1.Location = new System.Drawing.Point(6, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(539, 117);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "TVServer XBMC proxy";
      // 
      // portNumericUpDown
      // 
      this.portNumericUpDown.Location = new System.Drawing.Point(64, 18);
      this.portNumericUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
      this.portNumericUpDown.Name = "portNumericUpDown";
      this.portNumericUpDown.Size = new System.Drawing.Size(60, 20);
      this.portNumericUpDown.TabIndex = 15;
      this.portNumericUpDown.Value = new decimal(new int[] {
            9556,
            0,
            0,
            0});
      // 
      // _tcpPortLabel
      // 
      this._tcpPortLabel.AutoSize = true;
      this._tcpPortLabel.Location = new System.Drawing.Point(6, 21);
      this._tcpPortLabel.Name = "_tcpPortLabel";
      this._tcpPortLabel.Size = new System.Drawing.Size(52, 13);
      this._tcpPortLabel.TabIndex = 14;
      this._tcpPortLabel.Text = "TCP port:";
      // 
      // uncPathsTabPage
      // 
      this.uncPathsTabPage.Controls.Add(this.refreshUncButton);
      this.uncPathsTabPage.Controls.Add(this.uncTimeshiftGroupBox);
      this.uncPathsTabPage.Controls.Add(this.uncRecordingGroupBox);
      this.uncPathsTabPage.Location = new System.Drawing.Point(4, 22);
      this.uncPathsTabPage.Name = "uncPathsTabPage";
      this.uncPathsTabPage.Padding = new System.Windows.Forms.Padding(3);
      this.uncPathsTabPage.Size = new System.Drawing.Size(551, 374);
      this.uncPathsTabPage.TabIndex = 1;
      this.uncPathsTabPage.Text = "Shares";
      this.uncPathsTabPage.UseVisualStyleBackColor = true;
      // 
      // refreshUncButton
      // 
      this.refreshUncButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.refreshUncButton.Location = new System.Drawing.Point(470, 321);
      this.refreshUncButton.Name = "refreshUncButton";
      this.refreshUncButton.Size = new System.Drawing.Size(75, 23);
      this.refreshUncButton.TabIndex = 13;
      this.refreshUncButton.Text = "Refresh";
      this.refreshUncButton.UseVisualStyleBackColor = true;
      this.refreshUncButton.Click += new System.EventHandler(this.refreshUncButton_Click);
      // 
      // uncTimeshiftGroupBox
      // 
      this.uncTimeshiftGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.uncTimeshiftGroupBox.Controls.Add(this.createTimeshiftShareButton);
      this.uncTimeshiftGroupBox.Controls.Add(this.uncTimeshiftPathsDataGrid);
      this.uncTimeshiftGroupBox.Location = new System.Drawing.Point(6, 163);
      this.uncTimeshiftGroupBox.Name = "uncTimeshiftGroupBox";
      this.uncTimeshiftGroupBox.Size = new System.Drawing.Size(538, 152);
      this.uncTimeshiftGroupBox.TabIndex = 1;
      this.uncTimeshiftGroupBox.TabStop = false;
      this.uncTimeshiftGroupBox.Text = "UNC Timeshift Paths";
      // 
      // createTimeshiftShareButton
      // 
      this.createTimeshiftShareButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.createTimeshiftShareButton.Location = new System.Drawing.Point(8, 123);
      this.createTimeshiftShareButton.Name = "createTimeshiftShareButton";
      this.createTimeshiftShareButton.Size = new System.Drawing.Size(100, 23);
      this.createTimeshiftShareButton.TabIndex = 13;
      this.createTimeshiftShareButton.Text = "Create Share";
      this.createTimeshiftShareButton.UseVisualStyleBackColor = true;
      this.createTimeshiftShareButton.Click += new System.EventHandler(this.createTimeshiftShareButton_Click);
      // 
      // uncTimeshiftPathsDataGrid
      // 
      this.uncTimeshiftPathsDataGrid.AllowUserToAddRows = false;
      this.uncTimeshiftPathsDataGrid.AllowUserToDeleteRows = false;
      this.uncTimeshiftPathsDataGrid.AllowUserToResizeRows = false;
      this.uncTimeshiftPathsDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.uncTimeshiftPathsDataGrid.AutoGenerateColumns = false;
      this.uncTimeshiftPathsDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
      this.uncTimeshiftPathsDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.uncTimeshiftPathsDataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
      this.uncTimeshiftPathsDataGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.uncTimeshiftPathsDataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      this.uncTimeshiftPathsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.uncTimeshiftPathsDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2});
      this.uncTimeshiftPathsDataGrid.DataSource = this.uncTimeshiftPathsBindingSource;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.uncTimeshiftPathsDataGrid.DefaultCellStyle = dataGridViewCellStyle2;
      this.uncTimeshiftPathsDataGrid.GridColor = System.Drawing.SystemColors.Control;
      this.uncTimeshiftPathsDataGrid.Location = new System.Drawing.Point(7, 20);
      this.uncTimeshiftPathsDataGrid.MultiSelect = false;
      this.uncTimeshiftPathsDataGrid.Name = "uncTimeshiftPathsDataGrid";
      this.uncTimeshiftPathsDataGrid.ReadOnly = true;
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.uncTimeshiftPathsDataGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
      this.uncTimeshiftPathsDataGrid.RowHeadersVisible = false;
      this.uncTimeshiftPathsDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.uncTimeshiftPathsDataGrid.Size = new System.Drawing.Size(525, 100);
      this.uncTimeshiftPathsDataGrid.StandardTab = true;
      this.uncTimeshiftPathsDataGrid.TabIndex = 0;
      this.uncTimeshiftPathsDataGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.uncTimeshiftPathsDataGrid_CellFormatting);
      this.uncTimeshiftPathsDataGrid.SelectionChanged += new System.EventHandler(this.uncTimeshiftPathsDataGrid_SelectionChanged);
      // 
      // dataGridViewTextBoxColumn1
      // 
      this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.dataGridViewTextBoxColumn1.DataPropertyName = "CardName";
      this.dataGridViewTextBoxColumn1.FillWeight = 30F;
      this.dataGridViewTextBoxColumn1.HeaderText = "Card";
      this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
      this.dataGridViewTextBoxColumn1.ReadOnly = true;
      // 
      // dataGridViewTextBoxColumn2
      // 
      this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.dataGridViewTextBoxColumn2.DataPropertyName = "Message";
      this.dataGridViewTextBoxColumn2.FillWeight = 70F;
      this.dataGridViewTextBoxColumn2.HeaderText = "Path";
      this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
      this.dataGridViewTextBoxColumn2.ReadOnly = true;
      // 
      // uncRecordingGroupBox
      // 
      this.uncRecordingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.uncRecordingGroupBox.Controls.Add(this.createRecordingsShareButton);
      this.uncRecordingGroupBox.Controls.Add(this.uncRecordingPathsDataGrid);
      this.uncRecordingGroupBox.Location = new System.Drawing.Point(7, 7);
      this.uncRecordingGroupBox.Name = "uncRecordingGroupBox";
      this.uncRecordingGroupBox.Size = new System.Drawing.Size(538, 152);
      this.uncRecordingGroupBox.TabIndex = 0;
      this.uncRecordingGroupBox.TabStop = false;
      this.uncRecordingGroupBox.Text = "UNC Recording Paths";
      // 
      // createRecordingsShareButton
      // 
      this.createRecordingsShareButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.createRecordingsShareButton.Location = new System.Drawing.Point(7, 123);
      this.createRecordingsShareButton.Name = "createRecordingsShareButton";
      this.createRecordingsShareButton.Size = new System.Drawing.Size(100, 23);
      this.createRecordingsShareButton.TabIndex = 12;
      this.createRecordingsShareButton.Text = "Create Share";
      this.createRecordingsShareButton.UseVisualStyleBackColor = true;
      this.createRecordingsShareButton.Click += new System.EventHandler(this.createRecordingsShareButton_Click);
      // 
      // uncRecordingPathsDataGrid
      // 
      this.uncRecordingPathsDataGrid.AllowUserToAddRows = false;
      this.uncRecordingPathsDataGrid.AllowUserToDeleteRows = false;
      this.uncRecordingPathsDataGrid.AllowUserToResizeRows = false;
      this.uncRecordingPathsDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.uncRecordingPathsDataGrid.AutoGenerateColumns = false;
      this.uncRecordingPathsDataGrid.BackgroundColor = System.Drawing.SystemColors.Window;
      this.uncRecordingPathsDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.uncRecordingPathsDataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
      this.uncRecordingPathsDataGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
      dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.uncRecordingPathsDataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
      this.uncRecordingPathsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.uncRecordingPathsDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Card,
            this.Path});
      this.uncRecordingPathsDataGrid.DataSource = this.uncRecordingPathsBindingSource;
      dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.uncRecordingPathsDataGrid.DefaultCellStyle = dataGridViewCellStyle5;
      this.uncRecordingPathsDataGrid.GridColor = System.Drawing.SystemColors.Control;
      this.uncRecordingPathsDataGrid.Location = new System.Drawing.Point(7, 20);
      this.uncRecordingPathsDataGrid.MultiSelect = false;
      this.uncRecordingPathsDataGrid.Name = "uncRecordingPathsDataGrid";
      this.uncRecordingPathsDataGrid.ReadOnly = true;
      dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.uncRecordingPathsDataGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
      this.uncRecordingPathsDataGrid.RowHeadersVisible = false;
      this.uncRecordingPathsDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.uncRecordingPathsDataGrid.Size = new System.Drawing.Size(525, 100);
      this.uncRecordingPathsDataGrid.StandardTab = true;
      this.uncRecordingPathsDataGrid.TabIndex = 0;
      this.uncRecordingPathsDataGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.uncRecordingPathsDataGrid_CellFormatting);
      this.uncRecordingPathsDataGrid.SelectionChanged += new System.EventHandler(this.uncRecordingPathsDataGrid_SelectionChanged);
      // 
      // Card
      // 
      this.Card.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.Card.DataPropertyName = "CardName";
      this.Card.FillWeight = 30F;
      this.Card.HeaderText = "Card";
      this.Card.Name = "Card";
      this.Card.ReadOnly = true;
      // 
      // Path
      // 
      this.Path.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.Path.DataPropertyName = "Message";
      this.Path.FillWeight = 70F;
      this.Path.HeaderText = "Path";
      this.Path.Name = "Path";
      this.Path.ReadOnly = true;
      // 
      // SetupForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "SetupForm";
      this.Size = new System.Drawing.Size(559, 400);
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.portNumericUpDown)).EndInit();
      this.uncPathsTabPage.ResumeLayout(false);
      this.uncTimeshiftGroupBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.uncTimeshiftPathsDataGrid)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.uncTimeshiftPathsBindingSource)).EndInit();
      this.uncRecordingGroupBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.uncRecordingPathsDataGrid)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.uncRecordingPathsBindingSource)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage uncPathsTabPage;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.NumericUpDown portNumericUpDown;
    private System.Windows.Forms.Label _tcpPortLabel;
    private System.Windows.Forms.GroupBox uncRecordingGroupBox;
    private System.Windows.Forms.DataGridView uncRecordingPathsDataGrid;
    private System.Windows.Forms.DataGridViewTextBoxColumn Card;
    private System.Windows.Forms.DataGridViewTextBoxColumn Path;
    private System.Windows.Forms.GroupBox uncTimeshiftGroupBox;
    private System.Windows.Forms.DataGridView uncTimeshiftPathsDataGrid;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private System.Windows.Forms.Button createTimeshiftShareButton;
    private System.Windows.Forms.Button createRecordingsShareButton;
    private System.Windows.Forms.Button refreshUncButton;
    private System.Windows.Forms.BindingSource uncRecordingPathsBindingSource;
    private System.Windows.Forms.BindingSource uncTimeshiftPathsBindingSource;
  }
}
