﻿// Copyright (c) 2014, Oracle and/or its affiliates. All rights reserved.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation; version 2 of the
// License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
// 02110-1301  USA

using MySQL.ForExcel.Controls;

namespace MySQL.ForExcel.Forms
{
  partial class ImportMultipleDialog
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing"><c>true</c> if managed resources should be disposed; otherwise, <c>false</c>.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }

        // Set variables to null so this object does not hold references to them and the GC disposes of them sooner.
        _wbConnection = null;
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportMultipleDialog));
      this.DialogCancelButton = new System.Windows.Forms.Button();
      this.SelectTablesViewsSubLabel = new System.Windows.Forms.Label();
      this.ImportRelationshipsFromDbCheckBox = new System.Windows.Forms.CheckBox();
      this.PickRelatedSubLabel = new System.Windows.Forms.Label();
      this.PickRelatedMainLabel = new System.Windows.Forms.Label();
      this.DefineRelationshipsPictureBox = new System.Windows.Forms.PictureBox();
      this.SelectTablesViewsMainLabel = new System.Windows.Forms.Label();
      this.SelectTablesViewsPictureBox = new System.Windows.Forms.PictureBox();
      this.ImportDataLabel = new System.Windows.Forms.Label();
      this.TablesViewsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.SelectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.SelectNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.PreviewDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.WorkbookInCompatibilityModeWarningLabel = new System.Windows.Forms.Label();
      this.RelatedTablesViewsListView = new System.Windows.Forms.ListView();
      this.RelatedTableViewColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.RelatedToColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.DbObjectsImageList = new System.Windows.Forms.ImageList(this.components);
      this.TablesViewsListView = new System.Windows.Forms.ListView();
      this.TableViewColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.WorkbookInCompatibilityModeWarningPictureBox = new System.Windows.Forms.PictureBox();
      this.TotalTablesViewsLabel = new System.Windows.Forms.Label();
      this.AdvancedOptionsButton = new System.Windows.Forms.Button();
      this.RelationshipsNotSupportedLabel = new System.Windows.Forms.Label();
      this.RelationshipsNotSupportedPictureBox = new System.Windows.Forms.PictureBox();
      this.ImportButton = new System.Windows.Forms.Button();
      this.CreatePivotTableCheckBox = new System.Windows.Forms.CheckBox();
      this.OptionsGroupBox = new System.Windows.Forms.GroupBox();
      this.ContentAreaPanel.SuspendLayout();
      this.CommandAreaPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.DefineRelationshipsPictureBox)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.SelectTablesViewsPictureBox)).BeginInit();
      this.TablesViewsContextMenuStrip.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.WorkbookInCompatibilityModeWarningPictureBox)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.RelationshipsNotSupportedPictureBox)).BeginInit();
      this.OptionsGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // FootnoteAreaPanel
      // 
      this.FootnoteAreaPanel.Location = new System.Drawing.Point(0, 292);
      this.FootnoteAreaPanel.Size = new System.Drawing.Size(634, 0);
      // 
      // ContentAreaPanel
      // 
      this.ContentAreaPanel.Controls.Add(this.WorkbookInCompatibilityModeWarningLabel);
      this.ContentAreaPanel.Controls.Add(this.WorkbookInCompatibilityModeWarningPictureBox);
      this.ContentAreaPanel.Controls.Add(this.TotalTablesViewsLabel);
      this.ContentAreaPanel.Controls.Add(this.TablesViewsListView);
      this.ContentAreaPanel.Controls.Add(this.RelatedTablesViewsListView);
      this.ContentAreaPanel.Controls.Add(this.ImportDataLabel);
      this.ContentAreaPanel.Controls.Add(this.ImportRelationshipsFromDbCheckBox);
      this.ContentAreaPanel.Controls.Add(this.SelectTablesViewsSubLabel);
      this.ContentAreaPanel.Controls.Add(this.PickRelatedSubLabel);
      this.ContentAreaPanel.Controls.Add(this.PickRelatedMainLabel);
      this.ContentAreaPanel.Controls.Add(this.DefineRelationshipsPictureBox);
      this.ContentAreaPanel.Controls.Add(this.SelectTablesViewsMainLabel);
      this.ContentAreaPanel.Controls.Add(this.SelectTablesViewsPictureBox);
      this.ContentAreaPanel.Controls.Add(this.OptionsGroupBox);
      this.ContentAreaPanel.Controls.Add(this.RelationshipsNotSupportedLabel);
      this.ContentAreaPanel.Controls.Add(this.RelationshipsNotSupportedPictureBox);
      this.ContentAreaPanel.Size = new System.Drawing.Size(704, 541);
      // 
      // CommandAreaPanel
      // 
      this.CommandAreaPanel.Controls.Add(this.ImportButton);
      this.CommandAreaPanel.Controls.Add(this.AdvancedOptionsButton);
      this.CommandAreaPanel.Controls.Add(this.DialogCancelButton);
      this.CommandAreaPanel.Location = new System.Drawing.Point(0, 496);
      this.CommandAreaPanel.Size = new System.Drawing.Size(704, 45);
      // 
      // DialogCancelButton
      // 
      this.DialogCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.DialogCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.DialogCancelButton.Location = new System.Drawing.Point(617, 11);
      this.DialogCancelButton.Name = "DialogCancelButton";
      this.DialogCancelButton.Size = new System.Drawing.Size(75, 23);
      this.DialogCancelButton.TabIndex = 2;
      this.DialogCancelButton.Text = "Cancel";
      this.DialogCancelButton.UseVisualStyleBackColor = true;
      // 
      // SelectTablesViewsSubLabel
      // 
      this.SelectTablesViewsSubLabel.BackColor = System.Drawing.Color.Transparent;
      this.SelectTablesViewsSubLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.SelectTablesViewsSubLabel.ForeColor = System.Drawing.SystemColors.ControlText;
      this.SelectTablesViewsSubLabel.Location = new System.Drawing.Point(79, 73);
      this.SelectTablesViewsSubLabel.Name = "SelectTablesViewsSubLabel";
      this.SelectTablesViewsSubLabel.Size = new System.Drawing.Size(220, 33);
      this.SelectTablesViewsSubLabel.TabIndex = 2;
      this.SelectTablesViewsSubLabel.Text = "Tables and Views selected below will be imported to individual Excel worksheets.";
      // 
      // ImportRelationshipsFromDbCheckBox
      // 
      this.ImportRelationshipsFromDbCheckBox.AutoSize = true;
      this.ImportRelationshipsFromDbCheckBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.ImportRelationshipsFromDbCheckBox.Location = new System.Drawing.Point(372, 121);
      this.ImportRelationshipsFromDbCheckBox.Name = "ImportRelationshipsFromDbCheckBox";
      this.ImportRelationshipsFromDbCheckBox.Size = new System.Drawing.Size(211, 19);
      this.ImportRelationshipsFromDbCheckBox.TabIndex = 7;
      this.ImportRelationshipsFromDbCheckBox.Text = "Import relationships from database";
      this.ImportRelationshipsFromDbCheckBox.UseVisualStyleBackColor = true;
      this.ImportRelationshipsFromDbCheckBox.CheckedChanged += new System.EventHandler(this.ImportRelationshipsFromDbCheckBox_CheckedChanged);
      // 
      // PickRelatedSubLabel
      // 
      this.PickRelatedSubLabel.BackColor = System.Drawing.Color.Transparent;
      this.PickRelatedSubLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.PickRelatedSubLabel.ForeColor = System.Drawing.SystemColors.ControlText;
      this.PickRelatedSubLabel.Location = new System.Drawing.Point(369, 71);
      this.PickRelatedSubLabel.Name = "PickRelatedSubLabel";
      this.PickRelatedSubLabel.Size = new System.Drawing.Size(290, 33);
      this.PickRelatedSubLabel.TabIndex = 6;
      this.PickRelatedSubLabel.Text = "Related tables and views, not in the original selection, can be selected based on" +
    " their relationships.";
      // 
      // PickRelatedMainLabel
      // 
      this.PickRelatedMainLabel.AutoSize = true;
      this.PickRelatedMainLabel.BackColor = System.Drawing.Color.Transparent;
      this.PickRelatedMainLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.PickRelatedMainLabel.ForeColor = System.Drawing.SystemColors.ControlText;
      this.PickRelatedMainLabel.Location = new System.Drawing.Point(369, 54);
      this.PickRelatedMainLabel.Name = "PickRelatedMainLabel";
      this.PickRelatedMainLabel.Size = new System.Drawing.Size(174, 17);
      this.PickRelatedMainLabel.TabIndex = 5;
      this.PickRelatedMainLabel.Text = "Pick Related Tables or Views";
      // 
      // DefineRelationshipsPictureBox
      // 
      this.DefineRelationshipsPictureBox.BackColor = System.Drawing.Color.Transparent;
      this.DefineRelationshipsPictureBox.Image = global::MySQL.ForExcel.Properties.Resources.MySQLforExcel_ObjectPanel_RelatedObjects_32x32;
      this.DefineRelationshipsPictureBox.Location = new System.Drawing.Point(331, 60);
      this.DefineRelationshipsPictureBox.Name = "DefineRelationshipsPictureBox";
      this.DefineRelationshipsPictureBox.Size = new System.Drawing.Size(32, 32);
      this.DefineRelationshipsPictureBox.TabIndex = 31;
      this.DefineRelationshipsPictureBox.TabStop = false;
      // 
      // SelectTablesViewsMainLabel
      // 
      this.SelectTablesViewsMainLabel.AutoSize = true;
      this.SelectTablesViewsMainLabel.BackColor = System.Drawing.Color.Transparent;
      this.SelectTablesViewsMainLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.SelectTablesViewsMainLabel.ForeColor = System.Drawing.SystemColors.ControlText;
      this.SelectTablesViewsMainLabel.Location = new System.Drawing.Point(79, 54);
      this.SelectTablesViewsMainLabel.Name = "SelectTablesViewsMainLabel";
      this.SelectTablesViewsMainLabel.Size = new System.Drawing.Size(162, 17);
      this.SelectTablesViewsMainLabel.TabIndex = 1;
      this.SelectTablesViewsMainLabel.Text = "Selected Tables and Views";
      // 
      // SelectTablesViewsPictureBox
      // 
      this.SelectTablesViewsPictureBox.BackColor = System.Drawing.Color.Transparent;
      this.SelectTablesViewsPictureBox.Image = global::MySQL.ForExcel.Properties.Resources.MySQLforExcel_ObjectPanel_MultiSelectObject_32x32;
      this.SelectTablesViewsPictureBox.Location = new System.Drawing.Point(41, 60);
      this.SelectTablesViewsPictureBox.Name = "SelectTablesViewsPictureBox";
      this.SelectTablesViewsPictureBox.Size = new System.Drawing.Size(32, 32);
      this.SelectTablesViewsPictureBox.TabIndex = 20;
      this.SelectTablesViewsPictureBox.TabStop = false;
      // 
      // ImportDataLabel
      // 
      this.ImportDataLabel.AutoSize = true;
      this.ImportDataLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.ImportDataLabel.ForeColor = System.Drawing.Color.Navy;
      this.ImportDataLabel.Location = new System.Drawing.Point(17, 17);
      this.ImportDataLabel.Name = "ImportDataLabel";
      this.ImportDataLabel.Size = new System.Drawing.Size(176, 20);
      this.ImportDataLabel.TabIndex = 0;
      this.ImportDataLabel.Text = "Import Data from MySQL";
      // 
      // TablesViewsContextMenuStrip
      // 
      this.TablesViewsContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SelectAllToolStripMenuItem,
            this.SelectNoneToolStripMenuItem,
            this.PreviewDataToolStripMenuItem});
      this.TablesViewsContextMenuStrip.Name = "TablesViewsContextMenuStrip";
      this.TablesViewsContextMenuStrip.Size = new System.Drawing.Size(143, 70);
      this.TablesViewsContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.TablesViewsContextMenuStrip_Opening);
      // 
      // SelectAllToolStripMenuItem
      // 
      this.SelectAllToolStripMenuItem.Image = global::MySQL.ForExcel.Properties.Resources.select_all;
      this.SelectAllToolStripMenuItem.Name = "SelectAllToolStripMenuItem";
      this.SelectAllToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
      this.SelectAllToolStripMenuItem.Text = "Select All";
      this.SelectAllToolStripMenuItem.Click += new System.EventHandler(this.SelectAllToolStripMenuItem_Click);
      // 
      // SelectNoneToolStripMenuItem
      // 
      this.SelectNoneToolStripMenuItem.Image = global::MySQL.ForExcel.Properties.Resources.select_none;
      this.SelectNoneToolStripMenuItem.Name = "SelectNoneToolStripMenuItem";
      this.SelectNoneToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
      this.SelectNoneToolStripMenuItem.Text = "Select None";
      this.SelectNoneToolStripMenuItem.Click += new System.EventHandler(this.SelectNoneToolStripMenuItem_Click);
      // 
      // PreviewDataToolStripMenuItem
      // 
      this.PreviewDataToolStripMenuItem.Image = global::MySQL.ForExcel.Properties.Resources.MySQLforExcel_ExportDlg_ColumnOptions_32x32;
      this.PreviewDataToolStripMenuItem.Name = "PreviewDataToolStripMenuItem";
      this.PreviewDataToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
      this.PreviewDataToolStripMenuItem.Text = "Preview Data";
      this.PreviewDataToolStripMenuItem.Click += new System.EventHandler(this.PreviewDataToolStripMenuItem_Click);
      // 
      // WorkbookInCompatibilityModeWarningLabel
      // 
      this.WorkbookInCompatibilityModeWarningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.WorkbookInCompatibilityModeWarningLabel.AutoSize = true;
      this.WorkbookInCompatibilityModeWarningLabel.BackColor = System.Drawing.SystemColors.Window;
      this.WorkbookInCompatibilityModeWarningLabel.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.WorkbookInCompatibilityModeWarningLabel.ForeColor = System.Drawing.Color.Red;
      this.WorkbookInCompatibilityModeWarningLabel.Location = new System.Drawing.Point(157, 409);
      this.WorkbookInCompatibilityModeWarningLabel.Name = "WorkbookInCompatibilityModeWarningLabel";
      this.WorkbookInCompatibilityModeWarningLabel.Size = new System.Drawing.Size(57, 12);
      this.WorkbookInCompatibilityModeWarningLabel.TabIndex = 11;
      this.WorkbookInCompatibilityModeWarningLabel.Text = "Warning Text";
      this.WorkbookInCompatibilityModeWarningLabel.Visible = false;
      // 
      // RelatedTablesViewsListView
      // 
      this.RelatedTablesViewsListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.RelatedTablesViewsListView.CheckBoxes = true;
      this.RelatedTablesViewsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.RelatedTableViewColumnHeader,
            this.RelatedToColumnHeader});
      this.RelatedTablesViewsListView.ContextMenuStrip = this.TablesViewsContextMenuStrip;
      this.RelatedTablesViewsListView.FullRowSelect = true;
      this.RelatedTablesViewsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.RelatedTablesViewsListView.HideSelection = false;
      this.RelatedTablesViewsListView.Location = new System.Drawing.Point(372, 146);
      this.RelatedTablesViewsListView.MultiSelect = false;
      this.RelatedTablesViewsListView.Name = "RelatedTablesViewsListView";
      this.RelatedTablesViewsListView.Size = new System.Drawing.Size(287, 248);
      this.RelatedTablesViewsListView.SmallImageList = this.DbObjectsImageList;
      this.RelatedTablesViewsListView.TabIndex = 9;
      this.RelatedTablesViewsListView.UseCompatibleStateImageBehavior = false;
      this.RelatedTablesViewsListView.View = System.Windows.Forms.View.Details;
      this.RelatedTablesViewsListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.RelatedTablesViewsListView_ItemChecked);
      // 
      // RelatedTableViewColumnHeader
      // 
      this.RelatedTableViewColumnHeader.Text = "Table / View";
      this.RelatedTableViewColumnHeader.Width = 140;
      // 
      // RelatedToColumnHeader
      // 
      this.RelatedToColumnHeader.Text = "Related to";
      this.RelatedToColumnHeader.Width = 140;
      // 
      // DbObjectsImageList
      // 
      this.DbObjectsImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("DbObjectsImageList.ImageStream")));
      this.DbObjectsImageList.TransparentColor = System.Drawing.Color.Transparent;
      this.DbObjectsImageList.Images.SetKeyName(0, "MySQLforExcel-ObjectPanel-ListItem-Table-24x24.png");
      this.DbObjectsImageList.Images.SetKeyName(1, "MySQLforExcel-ObjectPanel-ListItem-View-24x24.png");
      // 
      // TablesViewsListView
      // 
      this.TablesViewsListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.TablesViewsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TableViewColumnHeader});
      this.TablesViewsListView.ContextMenuStrip = this.TablesViewsContextMenuStrip;
      this.TablesViewsListView.FullRowSelect = true;
      this.TablesViewsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.TablesViewsListView.HideSelection = false;
      this.TablesViewsListView.Location = new System.Drawing.Point(82, 146);
      this.TablesViewsListView.MultiSelect = false;
      this.TablesViewsListView.Name = "TablesViewsListView";
      this.TablesViewsListView.Size = new System.Drawing.Size(217, 248);
      this.TablesViewsListView.SmallImageList = this.DbObjectsImageList;
      this.TablesViewsListView.TabIndex = 4;
      this.TablesViewsListView.UseCompatibleStateImageBehavior = false;
      this.TablesViewsListView.View = System.Windows.Forms.View.Details;
      // 
      // TableViewColumnHeader
      // 
      this.TableViewColumnHeader.Text = "Table / View";
      this.TableViewColumnHeader.Width = 200;
      // 
      // WorkbookInCompatibilityModeWarningPictureBox
      // 
      this.WorkbookInCompatibilityModeWarningPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.WorkbookInCompatibilityModeWarningPictureBox.BackColor = System.Drawing.SystemColors.Window;
      this.WorkbookInCompatibilityModeWarningPictureBox.Image = global::MySQL.ForExcel.Properties.Resources.Warning;
      this.WorkbookInCompatibilityModeWarningPictureBox.Location = new System.Drawing.Point(135, 405);
      this.WorkbookInCompatibilityModeWarningPictureBox.Name = "WorkbookInCompatibilityModeWarningPictureBox";
      this.WorkbookInCompatibilityModeWarningPictureBox.Size = new System.Drawing.Size(20, 20);
      this.WorkbookInCompatibilityModeWarningPictureBox.TabIndex = 37;
      this.WorkbookInCompatibilityModeWarningPictureBox.TabStop = false;
      this.WorkbookInCompatibilityModeWarningPictureBox.Visible = false;
      // 
      // TotalTablesViewsLabel
      // 
      this.TotalTablesViewsLabel.AutoSize = true;
      this.TotalTablesViewsLabel.BackColor = System.Drawing.Color.Transparent;
      this.TotalTablesViewsLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.TotalTablesViewsLabel.Location = new System.Drawing.Point(79, 125);
      this.TotalTablesViewsLabel.Name = "TotalTablesViewsLabel";
      this.TotalTablesViewsLabel.Size = new System.Drawing.Size(133, 15);
      this.TotalTablesViewsLabel.TabIndex = 3;
      this.TotalTablesViewsLabel.Text = "Total Tables and Views: ";
      // 
      // AdvancedOptionsButton
      // 
      this.AdvancedOptionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.AdvancedOptionsButton.Location = new System.Drawing.Point(12, 11);
      this.AdvancedOptionsButton.Name = "AdvancedOptionsButton";
      this.AdvancedOptionsButton.Size = new System.Drawing.Size(131, 23);
      this.AdvancedOptionsButton.TabIndex = 0;
      this.AdvancedOptionsButton.Text = "Advanced Options...";
      this.AdvancedOptionsButton.UseVisualStyleBackColor = true;
      this.AdvancedOptionsButton.Click += new System.EventHandler(this.AdvancedOptionsButton_Click);
      // 
      // RelationshipsNotSupportedLabel
      // 
      this.RelationshipsNotSupportedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.RelationshipsNotSupportedLabel.BackColor = System.Drawing.SystemColors.Window;
      this.RelationshipsNotSupportedLabel.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.RelationshipsNotSupportedLabel.ForeColor = System.Drawing.Color.Red;
      this.RelationshipsNotSupportedLabel.Location = new System.Drawing.Point(397, 117);
      this.RelationshipsNotSupportedLabel.Name = "RelationshipsNotSupportedLabel";
      this.RelationshipsNotSupportedLabel.Size = new System.Drawing.Size(262, 26);
      this.RelationshipsNotSupportedLabel.TabIndex = 8;
      this.RelationshipsNotSupportedLabel.Text = "Warning Text";
      this.RelationshipsNotSupportedLabel.Visible = false;
      // 
      // RelationshipsNotSupportedPictureBox
      // 
      this.RelationshipsNotSupportedPictureBox.BackColor = System.Drawing.SystemColors.Window;
      this.RelationshipsNotSupportedPictureBox.Image = global::MySQL.ForExcel.Properties.Resources.Warning;
      this.RelationshipsNotSupportedPictureBox.Location = new System.Drawing.Point(372, 117);
      this.RelationshipsNotSupportedPictureBox.Name = "RelationshipsNotSupportedPictureBox";
      this.RelationshipsNotSupportedPictureBox.Size = new System.Drawing.Size(20, 20);
      this.RelationshipsNotSupportedPictureBox.TabIndex = 42;
      this.RelationshipsNotSupportedPictureBox.TabStop = false;
      this.RelationshipsNotSupportedPictureBox.Visible = false;
      // 
      // ImportButton
      // 
      this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.ImportButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.ImportButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.ImportButton.Location = new System.Drawing.Point(536, 11);
      this.ImportButton.Name = "ImportButton";
      this.ImportButton.Size = new System.Drawing.Size(75, 23);
      this.ImportButton.TabIndex = 1;
      this.ImportButton.Text = "Import";
      this.ImportButton.UseVisualStyleBackColor = true;
      // 
      // CreatePivotTableCheckBox
      // 
      this.CreatePivotTableCheckBox.AutoSize = true;
      this.CreatePivotTableCheckBox.Location = new System.Drawing.Point(10, 23);
      this.CreatePivotTableCheckBox.Name = "CreatePivotTableCheckBox";
      this.CreatePivotTableCheckBox.Size = new System.Drawing.Size(271, 17);
      this.CreatePivotTableCheckBox.TabIndex = 0;
      this.CreatePivotTableCheckBox.Text = "Create a PivotTable for each imported table or view.";
      this.CreatePivotTableCheckBox.UseVisualStyleBackColor = true;
      // 
      // OptionsGroupBox
      // 
      this.OptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.OptionsGroupBox.Controls.Add(this.CreatePivotTableCheckBox);
      this.OptionsGroupBox.Location = new System.Drawing.Point(82, 408);
      this.OptionsGroupBox.Name = "OptionsGroupBox";
      this.OptionsGroupBox.Size = new System.Drawing.Size(577, 55);
      this.OptionsGroupBox.TabIndex = 10;
      this.OptionsGroupBox.TabStop = false;
      this.OptionsGroupBox.Text = "Options";
      // 
      // ImportMultipleDialog
      // 
      this.AcceptButton = this.ImportButton;
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CancelButton = this.DialogCancelButton;
      this.ClientSize = new System.Drawing.Size(704, 541);
      this.CommandAreaVisible = true;
      this.FootnoteAreaHeight = 0;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
      this.MainInstructionLocation = new System.Drawing.Point(11, 15);
      this.MinimumSize = new System.Drawing.Size(720, 580);
      this.Name = "ImportMultipleDialog";
      this.Text = "Import Data";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImportMultipleDialog_FormClosing);
      this.Controls.SetChildIndex(this.FootnoteAreaPanel, 0);
      this.Controls.SetChildIndex(this.ContentAreaPanel, 0);
      this.Controls.SetChildIndex(this.CommandAreaPanel, 0);
      this.ContentAreaPanel.ResumeLayout(false);
      this.ContentAreaPanel.PerformLayout();
      this.CommandAreaPanel.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.DefineRelationshipsPictureBox)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.SelectTablesViewsPictureBox)).EndInit();
      this.TablesViewsContextMenuStrip.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.WorkbookInCompatibilityModeWarningPictureBox)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.RelationshipsNotSupportedPictureBox)).EndInit();
      this.OptionsGroupBox.ResumeLayout(false);
      this.OptionsGroupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button DialogCancelButton;
    private System.Windows.Forms.Label SelectTablesViewsSubLabel;
    private System.Windows.Forms.CheckBox ImportRelationshipsFromDbCheckBox;
    private System.Windows.Forms.Label PickRelatedSubLabel;
    private System.Windows.Forms.Label PickRelatedMainLabel;
    private System.Windows.Forms.PictureBox DefineRelationshipsPictureBox;
    private System.Windows.Forms.Label SelectTablesViewsMainLabel;
    private System.Windows.Forms.PictureBox SelectTablesViewsPictureBox;
    private System.Windows.Forms.Label ImportDataLabel;
    private System.Windows.Forms.Label WorkbookInCompatibilityModeWarningLabel;
    private System.Windows.Forms.ContextMenuStrip TablesViewsContextMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem PreviewDataToolStripMenuItem;
    private System.Windows.Forms.ListView RelatedTablesViewsListView;
    private System.Windows.Forms.ListView TablesViewsListView;
    private System.Windows.Forms.ColumnHeader RelatedTableViewColumnHeader;
    private System.Windows.Forms.ColumnHeader RelatedToColumnHeader;
    private System.Windows.Forms.PictureBox WorkbookInCompatibilityModeWarningPictureBox;
    private System.Windows.Forms.ImageList DbObjectsImageList;
    private System.Windows.Forms.Label TotalTablesViewsLabel;
    private System.Windows.Forms.ColumnHeader TableViewColumnHeader;
    private System.Windows.Forms.ToolStripMenuItem SelectAllToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem SelectNoneToolStripMenuItem;
    private System.Windows.Forms.Button AdvancedOptionsButton;
    private System.Windows.Forms.Label RelationshipsNotSupportedLabel;
    private System.Windows.Forms.PictureBox RelationshipsNotSupportedPictureBox;
    private System.Windows.Forms.Button ImportButton;
    private System.Windows.Forms.CheckBox CreatePivotTableCheckBox;
    private System.Windows.Forms.GroupBox OptionsGroupBox;
  }
}