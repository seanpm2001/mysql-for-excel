﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MySQL.ForExcel
{
  public partial class WarningDialog : AutoStyleableBaseDialog
  {
    public enum WarningButtons { Yes_No, OK_Cancel, OK };

    public WarningButtons DisplayButtons { get; set; }
    public string WarningTitle
    {
      get { return lblWarningTitle.Text; }
      set { lblWarningTitle.Text = value; }
    }
    public string WarningText
    {
      get { return lblWarningText.Text; }
      set { lblWarningText.Text = value; }
    }

    public WarningDialog(WarningButtons displayButtons, string warningTitle, string warningText)
    {
      InitializeComponent();
      WarningTitle = warningTitle;
      WarningText = warningText;
      switch (displayButtons)
      {
        case WarningButtons.Yes_No:
          btnYes.DialogResult = DialogResult.Yes;
          btnYes.Text = "Yes";
          btnNo.DialogResult = DialogResult.No;
          btnNo.Text = "No";
          break;
        case WarningButtons.OK_Cancel:
          btnYes.DialogResult = DialogResult.OK;
          btnYes.Text = "OK";
          btnNo.DialogResult = DialogResult.Cancel;
          btnNo.Text = "Cancel";
          break;
        case WarningButtons.OK:
          btnYes.DialogResult = DialogResult.OK;
          btnYes.Text = "OK";
          btnNo.DialogResult = DialogResult.None;
          btnNo.Text = "Cancel";
          btnNo.Visible = false;
          btnYes.Location = btnNo.Location;
          break;
      }
    }

    public WarningDialog(string warningTitle, string warningText)
      : this(WarningButtons.Yes_No, warningTitle, warningText)
    {
    }

    public WarningDialog()
      : this("Warning Title", "Warning Details Text")
    {
    }
  }
}
