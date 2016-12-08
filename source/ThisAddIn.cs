﻿// Copyright (c) 2012, 2016, Oracle and/or its affiliates. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Core;
using MySQL.ForExcel.Classes;
using MySQL.ForExcel.Controls;
using MySQL.ForExcel.Forms;
using MySQL.ForExcel.Properties;
using MySql.Utility.Classes;
using MySql.Utility.Classes.MySql;
using MySql.Utility.Classes.MySqlInstaller;
using MySql.Utility.Classes.MySqlWorkbench;
using MySql.Utility.Forms;
using ExcelInterop = Microsoft.Office.Interop.Excel;
using ExcelTools = Microsoft.Office.Tools.Excel;
using OfficeTools = Microsoft.Office.Tools;
using OfficeCore = Microsoft.Office.Core;

namespace MySQL.ForExcel
{
  /// <summary>
  /// Represents the main MySQL for Excel Office add-in.
  /// </summary>
  public partial class ThisAddIn
  {
    #region Constants

    /// <summary>
    /// The Add-In's maximum pane width in pixels.
    /// </summary>
    public const int ADD_IN_MAX_PANE_WIDTH = 460;

    /// <summary>
    /// The Add-In's minimum pane width in pixels.
    /// </summary>
    public const int ADD_IN_MIN_PANE_WIDTH = 266;

    /// <summary>
    /// The relative path of the stored connections file under the application data directory.
    /// </summary>
    public const string CONNECTIONS_FILE_RELATIVE_PATH = SETTINGS_DIRECTORY_RELATIVE_PATH + @"\connections.xml";

    /// <summary>
    /// The relative path of the log file under the application data directory.
    /// </summary>
    public const string ERROR_LOG_FILE_RELATIVE_PATH = SETTINGS_DIRECTORY_RELATIVE_PATH + @"\MySQLForExcelInterop.log";

    /// <summary>
    /// The string representation of the Escape key.
    /// </summary>
    public const string ESCAPE_KEY = "{ESC}";

    /// <summary>
    /// The Excel major version number corresponding to Excel 2007.
    /// </summary>
    public const int EXCEL_2007_VERSION_NUMBER = 12;

    /// <summary>
    /// The Excel major version number corresponding to Excel 2010.
    /// </summary>
    public const int EXCEL_2010_VERSION_NUMBER = 14;

    /// <summary>
    /// The Excel major version number corresponding to Excel 2013.
    /// </summary>
    public const int EXCEL_2013_VERSION_NUMBER = 15;

    /// <summary>
    /// The Excel major version number corresponding to Excel 2016.
    /// </summary>
    public const int EXCEL_2016_VERSION_NUMBER = 16;

    /// <summary>
    /// The number of seconds in 1 hour.
    /// </summary>
    public const int MILLISECONDS_IN_HOUR = 3600000;

    /// <summary>
    /// The relative path of the passwords vault file under the application data directory.
    /// </summary>
    public const string PASSWORDS_VAULT_FILE_RELATIVE_PATH = SETTINGS_DIRECTORY_RELATIVE_PATH + @"\user_data.dat";

    /// <summary>
    /// The relative path of the settings directory under the application data directory.
    /// </summary>
    public const string SETTINGS_DIRECTORY_RELATIVE_PATH = @"\Oracle\MySQL for Excel";

    /// <summary>
    /// The relative path of the settings file under the application data directory.
    /// </summary>
    public const string SETTINGS_FILE_RELATIVE_PATH = SETTINGS_DIRECTORY_RELATIVE_PATH + @"\settings.config";

    #endregion Constants

    #region Fields

    /// <summary>
    /// The timer that checks for automatic connetions migration.
    /// </summary>
    private Timer _connectionsMigrationTimer;

    /// <summary>
    /// A dictionary containing subsets of the <see cref="EditConnectionInfo"/> list filtered by <see cref="ExcelInterop.Workbook"/>.
    /// </summary>
    private Dictionary<string, List<EditConnectionInfo>> _editConnectionInfosByWorkbook;

    /// <summary>
    /// The <see cref="GlobalOptionsDialog"/>.
    /// </summary>
    private GlobalOptionsDialog _globalOptionsDialog;

    /// <summary>
    /// The name of the last deactivated Excel <see cref="ExcelInterop.Worksheet"/>.
    /// </summary>
    private string _lastDeactivatedSheetName;

    /// <summary>
    /// Flag indicating whether the code that migrates connections is in progress.
    /// </summary>
    private bool _migratingStoredConnections;

    /// <summary>
    /// True while restoring existing <see cref="EditConnectionInfo"/> objects for the current workbook, avoiding unwanted actions to be raised during the process.
    /// </summary>
    private bool _restoringExistingConnectionInfo;

    /// <summary>
    /// Flag indicating whether the detection of contents for a cell selection should be skipped.
    /// </summary>
    private bool _skipSelectedDataContentsDetection;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the environment's application data directory.
    /// </summary>
    public static string EnvironmentApplicationDataDirectory
    {
      get
      {
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      }
    }

    /// <summary>
    /// Gets the <see cref="CustomTaskPane"/> contained in the active Excel window.
    /// </summary>
    public OfficeTools.CustomTaskPane ActiveCustomPane
    {
      get
      {
        OfficeTools.CustomTaskPane addInPane = CustomTaskPanes.FirstOrDefault(ctp =>
        {
          bool isParentWindowActiveExcelWindow;
          if (ExcelVersionNumber >= EXCEL_2013_VERSION_NUMBER)
          {
            // If running on Excel 2013 or later a MDI is used for the windows so the active custom pane is matched with its window and the application active window.
            ExcelInterop.Window paneWindow = null;
            try
            {
              // This assignment is intentionally inside a try block because when an Excel window has been previously closed this property (ActiveCustomPane)
              // is called before the CustomTaskPane linked to the closed Excel window is removed from the collection, so the ctp.Window can throw an Exception.
              // A null check is not enough.
              paneWindow = ctp.Window as ExcelInterop.Window;
            }
            catch
            {
            }

            isParentWindowActiveExcelWindow = paneWindow != null && Application.ActiveWindow != null && paneWindow.Hwnd == Application.ActiveWindow.Hwnd;
          }
          else
          {
            // If running on Excel 2007 or 2010 a SDI is used so the active custom pane is the first one of an Excel Add-In.
            isParentWindowActiveExcelWindow = true;
          }

          return isParentWindowActiveExcelWindow && ctp.Control is ExcelAddInPane;
        });

        return addInPane;
      }
    }

    /// <summary>
    /// Gets the pane containing the MySQL for Excel add-in contained in the custom task pane shown in the active window.
    /// </summary>
    public ExcelAddInPane ActiveExcelPane
    {
      get
      {
        return ActiveCustomPane != null ? ActiveCustomPane.Control as ExcelAddInPane : null;
      }
    }

    /// <summary>
    /// Gets the active <see cref="ExcelInterop.Workbook"/> or creates one if there is no active one.
    /// </summary>
    public ExcelInterop.Workbook ActiveWorkbook
    {
      get
      {
        return Application.ActiveWorkbook ?? Application.Workbooks.Add(1);
      }
    }

    /// <summary>
    /// Gets a subset of the <see cref="EditConnectionInfos"/> objects listing only those associated to the active <see cref="ExcelInterop.Workbook"/>.
    /// </summary>
    public List<EditConnectionInfo> ActiveWorkbookEditConnectionInfos
    {
      get
      {
        return GetWorkbookEditConnectionInfos(ActiveWorkbook);
      }
    }

    /// <summary>
    /// Gets a subset of the <see cref="StoredImportConnectionInfos"/> objects listing only those assocaiated to the active <see cref="ExcelInterop.Workbook"/>.
    /// </summary>
    public List<ImportConnectionInfo> ActiveWorkbookImportConnectionInfos
    {
      get
      {
        var workbookId = ActiveWorkbook.GetOrCreateId();
        return GetWorkbookImportConnectionInfos(workbookId);
      }
    }

    /// <summary>
    /// Gets a subset of the <see cref="StoredImportConnectionInfos"/> listing only those assocaiated to the active <see cref="ExcelInterop.Worksheet"/>.
    /// </summary>
    public List<ImportConnectionInfo> ActiveWorksheetImportConnectionInfos
    {
      get
      {
        var workbookId = ActiveWorkbook.GetOrCreateId();
        ExcelInterop.Worksheet worksheet = ActiveWorkbook.ActiveSheet;
        return GetWorkSheetImportConnectionInfos(workbookId, worksheet.Name);
      }
    }

    /// <summary>
    /// Gets the title given to the assembly of the Add-In.
    /// </summary>
    public string AssemblyTitle { get; private set; }

    /// <summary>
    /// Gets the custom ribbon defined by this add-in.
    /// </summary>
    public MySqlRibbon CustomMySqlRibbon { get; private set; }

    /// <summary>
    /// Gets a list of <see cref="EditConnectionInfo"/> objects saved to disk.
    /// </summary>
    public List<EditConnectionInfo> EditConnectionInfos
    {
      get
      {
        return Settings.Default.EditConnectionInfosList ?? (Settings.Default.EditConnectionInfosList = new List<EditConnectionInfo>());
      }
    }

    /// <summary>
    /// Gets a list with all the Excel panes instantiated in the Excel window, stored it to dispose of them when needed.
    /// </summary>
    public List<ExcelAddInPane> ExcelPanesList { get; private set; }

    /// <summary>
    /// Gets the version for <see cref="ExcelInterop.PivotTable"/> objects creation.
    /// </summary>
    public ExcelInterop.XlPivotTableVersionList ExcelPivotTableVersion
    {
      get
      {
        switch (ExcelVersionNumber)
        {
          case EXCEL_2013_VERSION_NUMBER:
          case EXCEL_2016_VERSION_NUMBER:
            return ExcelInterop.XlPivotTableVersionList.xlPivotTableVersion15;

          case EXCEL_2010_VERSION_NUMBER:
            return ExcelInterop.XlPivotTableVersionList.xlPivotTableVersion14;

          default:
            return ExcelInterop.XlPivotTableVersionList.xlPivotTableVersion12;
        }
      }
    }

    /// <summary>
    /// Gets the MS Excel major version number.
    /// </summary>
    public int ExcelVersionNumber { get; private set; }

    /// <summary>
    /// Gets a <see cref="DateTime"/> value for when the next automatic connections migration will occur.
    /// </summary>
    public DateTime NextAutomaticConnectionsMigration
    {
      get
      {
        var alreadyMigrated = Settings.Default.WorkbenchMigrationSucceeded;
        var delay = Settings.Default.WorkbenchMigrationRetryDelay;
        var lastAttempt = Settings.Default.WorkbenchMigrationLastAttempt;
        return alreadyMigrated || (lastAttempt.Equals(DateTime.MinValue) && delay == 0)
          ? DateTime.MinValue
          : (delay == -1 ? DateTime.MaxValue : lastAttempt.AddHours(delay));
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the detection of contents for a cell selection should be skipped.
    /// </summary>
    /// <remarks>Used when a cell selection is being done programatically and not by the user.</remarks>
    public bool SkipSelectedDataContentsDetection
    {
      get
      {
        return _skipSelectedDataContentsDetection;
      }

      set
      {
        _skipSelectedDataContentsDetection = value;
        if (!_skipSelectedDataContentsDetection)
        {
          ActiveExcelPane.UpdateExcelSelectedDataStatus(Application.ActiveCell);
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="ExcelInterop.Worksheet.Change"/> event should be skipped.
    /// </summary>
    /// <remarks>Used when a cell's value is being set programatically and not by the user.</remarks>
    public bool SkipWorksheetChangeEvent { get; set; }

    /// <summary>
    /// Gets a list of <see cref="ImportConnectionInfo"/> objects saved to disk.
    /// </summary>
    public List<ImportConnectionInfo> StoredImportConnectionInfos
    {
      get
      {
        return Settings.Default.ImportConnectionInfosList ?? (Settings.Default.ImportConnectionInfosList = new List<ImportConnectionInfo>());
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a temporary hidden <see cref="ExcelInterop.Worksheet"/> is being used by a <see cref="TempRange"/> instance.
    /// </summary>
    public bool UsingTempWorksheet { get; set; }

    #endregion Properties

    /// <summary>
    /// Closes the Excel Add-In pane and its related custom task pane.
    /// </summary>
    /// <param name="excelPane">The Excel pane to close.</param>
    public void CloseExcelPane(ExcelAddInPane excelPane)
    {
      if (excelPane == null)
      {
        return;
      }

      try
      {
        ExcelPanesList.Remove(excelPane);
        if (ExcelPanesList.Count == 0)
        {
          ExcelAddInPanesClosed();
        }

        excelPane.Dispose();
        OfficeTools.CustomTaskPane customPane = CustomTaskPanes.FirstOrDefault(ctp => ctp.Control is ExcelAddInPane && ctp.Control == excelPane);
        if (customPane != null)
        {
          CustomTaskPanes.Remove(customPane);
          customPane.Dispose();
        }
      }
      catch (Exception ex)
      {
        MySqlSourceTrace.WriteAppErrorToLog(ex, false);
      }
    }

    /// <summary>
    /// Closes and removes all <see cref="EditConnectionInfo" /> associated to the given <see cref="ExcelInterop.Workbook"/>.
    /// </summary>
    /// <param name="workbook">The <see cref="ExcelInterop.Workbook"/> associated to the <see cref="EditConnectionInfo" /> objects to close.</param>
    public void CloseWorkbookEditConnectionInfos(ExcelInterop.Workbook workbook)
    {
      if (workbook == null)
      {
        return;
      }

      var workbookConnectionInfos = GetWorkbookEditConnectionInfos(workbook);
      var connectionInfosToFreeResources = workbookConnectionInfos.FindAll(connectionInfo => connectionInfo.EditDialog != null && connectionInfo.EditDialog.WorkbookName == workbook.Name);
      foreach (var connectionInfo in connectionInfosToFreeResources)
      {
        // The Close method is both closing the dialog and removing itself from the collection of <see cref="EditConnectionInfo" /> objects.
        connectionInfo.EditDialog.Close();
      }
    }

    /// <summary>
    /// Gets the custom task pane in the active window, if not found creates it.
    /// </summary>
    /// <returns>the active or newly created <see cref="CustomTaskPane"/> object.</returns>
    public OfficeTools.CustomTaskPane GetOrCreateActiveCustomPane()
    {
      OfficeTools.CustomTaskPane activeCustomPane = ActiveCustomPane;

      // If there is no custom pane associated to the Excel Add-In in the active window, create one.
      if (activeCustomPane != null)
      {
        return activeCustomPane;
      }

      Application.Cursor = ExcelInterop.XlMousePointer.xlWait;
      if (ExcelPanesList == null)
      {
        ExcelPanesList = new List<ExcelAddInPane>();
      }

      // Determine if this is the first run of the Add-In by checking if there are no Excel panes in the collection.
      // This must be done at this point of the code, before the lines below that create an Excel pane.
      bool firstRun = ExcelPanesList.Count == 0;
      if (firstRun)
      {
        // Attemtp to migrate all locally stored connections to the MySQL Workbench connections file.
        CheckForNextAutomaticConnectionsMigration(false);
      }

      // Instantiate the Excel Add-In pane to attach it to the Excel's custom task pane.
      // Note that in Excel 2007 and 2010 a MDI model is used so only a single Excel pane is instantiated, whereas in Excel 2013 and greater
      //  a SDI model is used instead, so an Excel pane is instantiated for each custom task pane appearing in each Excel window.
      var excelPane = new ExcelAddInPane { Dock = DockStyle.Fill };
      excelPane.SizeChanged += ExcelPane_SizeChanged;
      ExcelPanesList.Add(excelPane);

      // Create a new custom task pane and initialize it.
      activeCustomPane = CustomTaskPanes.Add(excelPane, AssemblyTitle);
      activeCustomPane.VisibleChanged += CustomTaskPaneVisibleChanged;
      activeCustomPane.DockPosition = MsoCTPDockPosition.msoCTPDockPositionRight;
      activeCustomPane.DockPositionRestrict = MsoCTPDockPositionRestrict.msoCTPDockPositionRestrictNoHorizontal;
      activeCustomPane.Width = ADD_IN_MIN_PANE_WIDTH;

      // First run if no Excel panes have been opened yet.
      if (firstRun)
      {
        ExcelAddInPaneFirstRun();
      }

      Application.Cursor = ExcelInterop.XlMousePointer.xlDefault;
      return activeCustomPane;
    }

    /// <summary>
    /// Attempts to migrate connections created in the MySQL for Excel's connections file to the Workbench's one.
    /// </summary>
    /// <param name="showDelayOptions">Flag indicating whether options to delay the migration are shown in case the user chooses not to migrate connections now.</param>
    public void MigrateExternalConnectionsToWorkbench(bool showDelayOptions)
    {
      _migratingStoredConnections = true;

      // If the method is not being called from the glbal options dialog itself, then force close the dialog.
      // This is necessary since when this code is executed from another thread the dispatch is posted to the main thread, so we don't have control over when the code
      // starts and when finishes in order to prevent the users from doing a manual migration in the options dialog, and we can't update the automatic migration date either.
      if (showDelayOptions && _globalOptionsDialog != null)
      {
        _globalOptionsDialog.Close();
        _globalOptionsDialog.Dispose();
        _globalOptionsDialog = null;
      }

      // Attempt to perform the migration
      MySqlWorkbench.MigrateExternalConnectionsToWorkbench(showDelayOptions);

      // Update settings depending on the migration outcome.
      Settings.Default.WorkbenchMigrationSucceeded = MySqlWorkbench.ConnectionsMigrationStatus == MySqlWorkbench.ConnectionsMigrationStatusType.MigrationNeededAlreadyMigrated;
      if (MySqlWorkbench.ConnectionsMigrationStatus == MySqlWorkbench.ConnectionsMigrationStatusType.MigrationNeededButNotMigrated)
      {
        Settings.Default.WorkbenchMigrationLastAttempt = DateTime.Now;
        if (showDelayOptions)
        {
          Settings.Default.WorkbenchMigrationRetryDelay = MySqlWorkbench.ConnectionsMigrationDelay.ToHours();
        }
      }
      else
      {
        Settings.Default.WorkbenchMigrationLastAttempt = DateTime.MinValue;
        Settings.Default.WorkbenchMigrationRetryDelay = 0;
      }

      Settings.Default.Save();

      // If the migration was done successfully, no need to keep the timer running.
      if (Settings.Default.WorkbenchMigrationSucceeded && _connectionsMigrationTimer != null)
      {
        _connectionsMigrationTimer.Enabled = false;
      }

      _migratingStoredConnections = false;
    }

    /// <summary>
    /// Refreshes the data in all <see cref="ExcelInterop.ListObject"/> and <see cref="ExcelInterop.PivotTable"/> objects in every <see cref="ExcelInterop.Worksheet"/> of the active <see cref="ExcelInterop.Workbook"/>.
    /// </summary>
    public void RefreshAllCustomFunctionality()
    {
      foreach (ExcelInterop.WorkbookConnection wbConnection in ActiveWorkbook.Connections)
      {
        var excelTable = wbConnection.GetExcelTable();
        if (excelTable != null && excelTable.RefreshMySqlData())
        {
          continue;
        }

        // The try-catch block must be INSIDE the foreach loop since we may want to continue refreshing the next WorkbookConnection even if an Exception is thrown.
        try
        {
          wbConnection.Refresh();
        }
        catch (Exception ex)
        {
          MySqlSourceTrace.WriteAppErrorToLog(ex, false);
          var infoProperties = InfoDialogProperties.GetYesNoDialogProperties(
            InfoDialog.InfoType.Error,
            Resources.OperationErrorTitle,
            string.Format(Resources.StandardWorkbookConnectionRefreshError, wbConnection.Name),
            Resources.ContinueRefreshingWorkbookConnectionsText,
            ex.GetFormattedMessage());
          infoProperties.WordWrapMoreInfo = true;
          if (InfoDialog.ShowDialog(infoProperties).DialogResult != DialogResult.Yes)
          {
            break;
          }
        }
      }
    }

    /// <summary>
    /// Attempts to refresh the MySQL data tied to the <see cref="ExcelInterop.ListObject"/> of the active Excell cell.
    /// </summary>
    /// <returns><c>true</c> if the active <see cref="ExcelInterop.ListObject"/> has a related <see cref="ImportConnectionInfo"/>, <c>false</c> otherwise.</returns>
    public bool RefreshDataCustomFunctionality()
    {
      var listObject = Application.ActiveCell.ListObject;
      return listObject.RefreshMySqlData();
    }

    /// <summary>
    /// Shows the <see cref="GlobalOptionsDialog"/>.
    /// </summary>
    public void ShowGlobalOptionsDialog()
    {
      using (_globalOptionsDialog = new GlobalOptionsDialog())
      {
        if (_globalOptionsDialog.ShowDialog() != DialogResult.OK)
        {
          return;
        }

        var excelAddInPane = ActiveExcelPane;
        if (excelAddInPane != null)
        {
          excelAddInPane.RefreshWbConnectionTimeouts();
        }
      }
    }

    /// <summary>
    /// Creates and returns a new instance of the <see cref="MySqlRibbon"/> class.
    /// </summary>
    /// <returns>A new instance of the <see cref="MySqlRibbon"/> class.</returns>
    protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
    {
      CustomMySqlRibbon = new MySqlRibbon();
      return CustomMySqlRibbon;
    }

    /// <summary>
    /// Adjusts the settings related to bulk inserts.
    /// </summary>
    private void AdjustSettingsForBulkInserts()
    {
      if (Settings.Default.AdjustedMultipleInsertFlags)
      {
        return;
      }

      if (!Settings.Default.ExportGenerateMultipleInserts && Settings.Default.ExportSqlQueriesCreateIndexesLast)
      {
        Settings.Default.ExportSqlQueriesCreateIndexesLast = false;
      }

      if (!Settings.Default.AppendGenerateMultipleInserts && Settings.Default.AppendSqlQueriesDisableIndexes)
      {
        Settings.Default.AppendSqlQueriesDisableIndexes = false;
      }

      Settings.Default.AdjustedMultipleInsertFlags = true;
      MiscUtilities.SaveSettings();
    }

    /// <summary>
    /// Event delegate method fired when an Excel <see cref="ExcelInterop.Worksheet"/> is activated.
    /// </summary>
    /// <param name="workSheet">A <see cref="ExcelInterop.Worksheet"/> object.</param>
    private void Application_SheetActivate(object workSheet)
    {
      if (ActiveExcelPane == null || UsingTempWorksheet)
      {
        return;
      }

      ExcelInterop.Worksheet activeSheet = workSheet as ExcelInterop.Worksheet;
      if (!activeSheet.IsVisible())
      {
        return;
      }

      if (_lastDeactivatedSheetName.Length > 0 && !ActiveWorkbook.WorksheetExists(_lastDeactivatedSheetName))
      {
        // Worksheet was deleted and the Application_SheetBeforeDelete did not run, user is running Excel 2010 or earlier.
        CloseMissingWorksheetEditConnectionInfo(ActiveWorkbook, _lastDeactivatedSheetName);
      }

      ChangeEditDialogVisibility(activeSheet, true);
    }

    /// <summary>
    /// Event delegate method fired before an Excel <see cref="ExcelInterop.Worksheet"/> is deleted.
    /// </summary>
    /// <param name="workSheet">A <see cref="ExcelInterop.Worksheet"/> object.</param>
    private void Application_SheetBeforeDelete(object workSheet)
    {
      if (ActiveExcelPane == null || UsingTempWorksheet)
      {
        return;
      }

      ExcelInterop.Worksheet activeSheet = workSheet as ExcelInterop.Worksheet;
      if (!activeSheet.IsVisible())
      {
        return;
      }

      CloseWorksheetEditConnectionInfo(activeSheet);

      // If the _lastDeactivatedSheetName is not empty it means a deactivated sheet may have been deleted, if this method ran it means the user is running
      // Excel 2013 or later where this method is supported, so we need to clean the _lastDeactivatedSheetName.
      if (_lastDeactivatedSheetName.Length > 0)
      {
        _lastDeactivatedSheetName = string.Empty;
      }
    }

    /// <summary>
    /// Event delegate method fired when the contents of the current selection of Excel cells in a given <see cref="ExcelInterop.Worksheet"/> change.
    /// </summary>
    /// <param name="workSheet">A <see cref="ExcelInterop.Worksheet"/> object.</param>
    /// <param name="targetRange">A selection of Excel cells.</param>
    private void Application_SheetChange(object workSheet, ExcelInterop.Range targetRange)
    {
      if (ActiveExcelPane == null || SkipWorksheetChangeEvent || UsingTempWorksheet)
      {
        return;
      }

      ExcelInterop.Worksheet activeSheet = workSheet as ExcelInterop.Worksheet;
      if (!activeSheet.IsVisible())
      {
        return;
      }

      if (!SkipSelectedDataContentsDetection)
      {
        ActiveExcelPane.UpdateExcelSelectedDataStatus(targetRange);
      }
    }

    /// <summary>
    /// Event delegate method fired when an Excel <see cref="ExcelInterop.Worksheet"/> is deactivated.
    /// </summary>
    /// <param name="workSheet">A <see cref="ExcelInterop.Worksheet"/> object.</param>
    private void Application_SheetDeactivate(object workSheet)
    {
      if (ActiveExcelPane == null || UsingTempWorksheet)
      {
        return;
      }

      ExcelInterop.Worksheet deactivatedSheet = workSheet as ExcelInterop.Worksheet;
      if (!deactivatedSheet.IsVisible())
      {
        return;
      }

      _lastDeactivatedSheetName = deactivatedSheet != null ? deactivatedSheet.Name : string.Empty;
      ChangeEditDialogVisibility(deactivatedSheet, false);
    }

    /// <summary>
    /// Event delegate method fired when the selection of Excel cells in a given <see cref="ExcelInterop.Worksheet"/> changes.
    /// </summary>
    /// <param name="workSheet">A <see cref="ExcelInterop.Worksheet"/> object.</param>
    /// <param name="targetRange">The new selection of Excel cells.</param>
    private void Application_SheetSelectionChange(object workSheet, ExcelInterop.Range targetRange)
    {
      if (ActiveExcelPane == null || UsingTempWorksheet)
      {
        return;
      }

      var activeSheet = workSheet as ExcelInterop.Worksheet;
      if (!activeSheet.IsVisible())
      {
        return;
      }

      if (!SkipSelectedDataContentsDetection)
      {
        ActiveExcelPane.UpdateExcelSelectedDataStatus(targetRange);
      }
    }

    /// <summary>
    /// Event delegate method fired when an Excel window is activated.
    /// </summary>
    /// <param name="workbook">The Excel workbook tied to the activated window.</param>
    /// <param name="window">The activated Excel window.</param>
    private void Application_WindowActivate(ExcelInterop.Workbook workbook, ExcelInterop.Window window)
    {
      // Verify the collection of custom task panes to dispose of custom task panes pointing to closed (invalid) windows.
      bool disposePane = false;
      foreach (OfficeTools.CustomTaskPane customPane in CustomTaskPanes.Where(customPane => customPane.Control is ExcelAddInPane))
      {
        try
        {
          // Do NOT remove the following line although the customPaneWindow variable is not used in the method the casting
          // of the customPane.Window is needed to determine if the window is still valid and has not been disposed of.
          var customPaneWindow = customPane.Window as ExcelInterop.Window;
        }
        catch
        {
          // If an error ocurred trying to access the custom task pane window, it means its window is no longer valid
          //  or in other words, it has been closed. There is no other way to find out if a windows was closed
          //  (similar to the way we find out if a Worksheet has been closed as there are no events for that).
          disposePane = true;
        }

        if (!disposePane)
        {
          continue;
        }

        ExcelAddInPane excelPane = customPane.Control as ExcelAddInPane;
        CloseExcelPane(excelPane);
        break;
      }

      // Synchronize the MySQL for Excel toggle button state of the currently activated window.
      CustomMySqlRibbon.ChangeShowMySqlForExcelPaneToggleState(ActiveCustomPane != null && ActiveCustomPane.Visible);
    }

    /// <summary>
    /// Event delegate method fired when a <see cref="ExcelInterop.Workbook"/> is activated.
    /// </summary>
    /// <param name="workBook">A <see cref="ExcelInterop.Workbook"/> object.</param>
    private void Application_WorkbookActivate(object workBook)
    {
      if (ActiveExcelPane == null)
      {
        return;
      }

      ExcelInterop.Workbook activeWorkbook = workBook as ExcelInterop.Workbook;
      ExcelInterop.Worksheet activeSheet = activeWorkbook != null ? activeWorkbook.ActiveSheet as ExcelInterop.Worksheet : null;
      ChangeEditDialogVisibility(activeSheet, true);
      ActiveExcelPane.RefreshDbObjectPanelActionLabelsEnabledStatus();
    }

    /// <summary>
    /// Event delegate method fired after an Excel <see cref="ExcelInterop.Workbook"/> is saved to disk.
    /// </summary>
    /// <param name="workbook">A <see cref="ExcelInterop.Workbook"/> object.</param>
    /// <param name="success">Flag indicating whether the save operation was successful.</param>
    private void Application_WorkbookAfterSave(ExcelInterop.Workbook workbook, bool success)
    {
      var workbookEditConnectionInfos = GetWorkbookEditConnectionInfos(workbook);
      ProtectEditingWorksheets(workbook, success, workbookEditConnectionInfos);
      if (!success)
      {
        workbook.Saved = false;
        return;
      }

      // Scrubbing of invalid ImportConnectionInfo and setting last access time.
      RemoveInvalidImportConnectionInformation();
      foreach (var activeImportConnectionInfo in ActiveWorkbookImportConnectionInfos)
      {
        activeImportConnectionInfo.LastAccess = DateTime.Now;
        activeImportConnectionInfo.WorkbookName = workbook.Name;
        activeImportConnectionInfo.WorkbookFilePath = workbook.FullName;
      }

      // Remove deleted EditConnectionInfo objects from memory collection also from serialized collection
      var workbookId = workbook.GetOrCreateId();
      foreach (var storedConnectionInfo in EditConnectionInfos.FindAll(storedConnectionInfo => string.Equals(storedConnectionInfo.WorkbookGuid, workbookId, StringComparison.InvariantCulture) && !workbookEditConnectionInfos.Exists(wbConnectionInfo => wbConnectionInfo.HasSameWorkbookAndTable(storedConnectionInfo))))
      {
        EditConnectionInfos.Remove(storedConnectionInfo);
      }

      MiscUtilities.SaveSettings();
      workbook.Saved = true;
    }

    /// <summary>
    /// Method that overrides the default program flow on Excel 2007 since it doesn't exist an WorkbookAfterSave event in this version compared to 2010 and superior versions of Excel.
    /// More info about this topic can be found at http://msdn.microsoft.com/en-us/library/office/ff836466(v=office.15).aspx" />
    /// </summary>
    /// <param name="workbook">A <see cref="ExcelInterop.Workbook"/> object.</param>
    /// <param name="saveAsUi">Flag indicating whether the Save As dialog was displayed.</param>
    private void Application_WorkbookAfterSave2007(ExcelInterop.Workbook workbook, bool saveAsUi)
    {
      Application.EnableEvents = false; //Stops beforesave event from re-running
      bool triggerAfterSave = true;

      try
      {
        if (saveAsUi)
        {
          var saveAsDialog = Application.Dialogs[ExcelInterop.XlBuiltInDialog.xlDialogSaveAs];
          triggerAfterSave = saveAsDialog.Show(workbook.Name, Application.DefaultSaveFormat, null, true, null, false);
        }
        else
        {
          workbook.Save();
        }
      }
      catch (Exception ex)
      {
        MySqlSourceTrace.WriteAppErrorToLog(ex, false);
        triggerAfterSave = false;
      }

      Application.EnableEvents = true;
      if (triggerAfterSave)
      {
        Application_WorkbookAfterSave(workbook, saveAsUi);
      }
    }

    /// <summary>
    /// Event delegate method fired before a <see cref="ExcelInterop.Workbook"/> is closed.
    /// </summary>
    /// <param name="workbook">A <see cref="ExcelInterop.Workbook"/> object.</param>
    /// <param name="cancel">Flag indicating whether the user cancelled the closing event.</param>
    private void Application_WorkbookBeforeClose(ExcelInterop.Workbook workbook, ref bool cancel)
    {
      if (ActiveExcelPane == null)
      {
        return;
      }

      bool wasAlreadySaved = workbook.Saved;
      if (!wasAlreadySaved)
      {
        switch (MessageBox.Show(string.Format(Resources.WorkbookSavingDetailText, workbook.Name), Application.Name, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1))
        {
          case DialogResult.Yes:
            UnprotectEditingWorksheets(workbook);
            for (int retry = 1; retry <= 3 && !wasAlreadySaved && !cancel; retry++)
            {
              try
              {
                if (workbook.IsNew())
                {
                  // The workbook is being saved for the very first time, so show the Save As dialog to users which will save the Workbook where the user wants to.
                  if (ExcelVersionNumber <= EXCEL_2007_VERSION_NUMBER)
                  {
                    Application.EnableEvents = false; //Stops beforesave event from re-running
                    var saveAsDialog = Application.Dialogs[ExcelInterop.XlBuiltInDialog.xlDialogSaveAs];
                    wasAlreadySaved = saveAsDialog.Show(workbook.Name, Application.DefaultSaveFormat, null, true, null, false);
                    Application.EnableEvents = true;
                  }
                  else
                  {
                    var saveAsDialog = Application.Dialogs[ExcelInterop.XlBuiltInDialog.xlDialogSaveAs];
                    wasAlreadySaved = saveAsDialog.Show(workbook.Name);
                  }

                  cancel = !wasAlreadySaved;
                }
                else
                {
                  // The workbook has been saved before, so just overwrite it.
                  workbook.Save();
                  wasAlreadySaved = true;
                }
              }
              catch (Exception ex)
              {
                var errorTitle = retry <= 3 ? Resources.WorkbookSaveErrorText : Resources.WorkbookSaveErrorFinalText;
                MySqlSourceTrace.WriteAppErrorToLog(ex, null, errorTitle, true);
              }
            }

            ProtectEditingWorksheets(workbook, false);
            break;

          case DialogResult.No:
            wasAlreadySaved = true;
            break;

          case DialogResult.Cancel:
            cancel = true;
            break;
        }
      }

      if (cancel)
      {
        return;
      }

      // Cleanup and close EditConnectionInfo and ImportConnectionInfo objects from the closing workbook.
      CloseWorkbookEditConnectionInfos(workbook);
      RemoveInvalidImportConnectionInformation();
      foreach (var importConnectionInfo in ActiveWorkbookImportConnectionInfos)
      {
        importConnectionInfo.Dispose();
      }

      // Remove the EditConnectionInfo objects for the workbook being closed from the dictionary.
      _editConnectionInfosByWorkbook.Remove(workbook.GetOrCreateId());

      if (wasAlreadySaved)
      {
        workbook.Saved = true;
      }
    }

    /// <summary>
    /// Event delegate method fired before an Excel <see cref="ExcelInterop.Workbook"/> is saved to disk.
    /// </summary>
    /// <param name="workbook">A <see cref="ExcelInterop.Workbook"/> object.</param>
    /// <param name="saveAsUi">Flag indicating whether the Save As dialog was displayed.</param>
    /// <param name="cancel">Flag indicating whether the user cancelled the saving event.</param>
    private void Application_WorkbookBeforeSave(ExcelInterop.Workbook workbook, bool saveAsUi, ref bool cancel)
    {
      UnprotectEditingWorksheets(workbook);

      //The WorkbookAfterSave event in Excel 2007 does not exist so we need to sligthly alter the program flow to overcome this limitation.
      if (ExcelVersionNumber <= EXCEL_2007_VERSION_NUMBER)
      {
        cancel = true; //Cancels the users original save command request in order to execute the following code override.
        Application_WorkbookAfterSave2007(workbook, saveAsUi);
      }
    }

    /// <summary>
    /// Event delegate method fired when an Excel <see cref="ExcelInterop.Workbook"/> is deactivated.
    /// </summary>
    /// <param name="workbook">A <see cref="ExcelInterop.Workbook"/> object.</param>
    private void Application_WorkbookDeactivate(object workbook)
    {
      if (ActiveExcelPane == null)
      {
        return;
      }

      // Hide editDialogs from deactivated Workbook
      ExcelInterop.Workbook deactivatedWorkbook = workbook as ExcelInterop.Workbook;
      if (deactivatedWorkbook == null)
      {
        return;
      }

      foreach (ExcelInterop.Worksheet wSheet in deactivatedWorkbook.Worksheets)
      {
        ChangeEditDialogVisibility(wSheet, false);
      }
    }

    /// <summary>
    /// Event delegate method fired when a new <see cref="ExcelInterop.Workbook"/> is created.
    /// </summary>
    /// <param name="workbook">The <see cref="ExcelInterop.Workbook"/> being created.</param>
    private void Application_WorkbookNewWorkbook(Microsoft.Office.Interop.Excel.Workbook workbook)
    {
      InitializeWorkbook(workbook);
    }

    /// <summary>
    /// Event delegate method fired when a <see cref="ExcelInterop.Workbook"/> is opened.
    /// </summary>
    /// <param name="workbook">The <see cref="ExcelInterop.Workbook"/> being opened.</param>
    private void Application_WorkbookOpen(ExcelInterop.Workbook workbook)
    {
      InitializeWorkbook(workbook);
    }

    /// <summary>
    /// Shows or hides an Edit dialog associated to the given <see cref="ExcelInterop.Worksheet"/>.
    /// </summary>
    /// <param name="workSheet">A <see cref="ExcelInterop.Worksheet"/> object.</param>
    /// <param name="show">Flag indicating if the dialog will be shown or hidden.</param>
    private void ChangeEditDialogVisibility(ExcelInterop.Worksheet workSheet, bool show)
    {
      if (workSheet == null)
      {
        return;
      }

      var parentWorkbook = workSheet.Parent as ExcelInterop.Workbook;
      if (parentWorkbook == null)
      {
        return;
      }

      var workbookConnectionInfos = GetWorkbookEditConnectionInfos(parentWorkbook);
      if (workbookConnectionInfos.Count == 0 || _restoringExistingConnectionInfo)
      {
        return;
      }

      var activeEditConnectionInfo = workbookConnectionInfos.GetActiveEditConnectionInfo(workSheet);
      if (activeEditConnectionInfo == null)
      {
        return;
      }

      if (show)
      {
        activeEditConnectionInfo.EditDialog.ShowDialog();
      }
      else
      {
        activeEditConnectionInfo.EditDialog.Hide();
      }
    }

    /// <summary>
    /// Event delegate that checks if it's time to display the dialog for connections migration.
    /// </summary>
    /// <param name="fromTimer">Flag indicating whether this method is called from a timer.</param>
    private void CheckForNextAutomaticConnectionsMigration(bool fromTimer)
    {
      // If the execution of the code that migrates connections is sitll executing, then exit.
      if (_migratingStoredConnections)
      {
        return;
      }

      // Temporarily disable the timer.
      if (fromTimer)
      {
        _connectionsMigrationTimer.Enabled = false;
      }

      // Check if the next connections migration is due now.
      bool doMigration = true;
      var nextMigrationAttempt = NextAutomaticConnectionsMigration;
      if (!fromTimer && !nextMigrationAttempt.Equals(DateTime.MinValue) && (nextMigrationAttempt.Equals(DateTime.MaxValue) || DateTime.Now.CompareTo(nextMigrationAttempt) < 0))
      {
        doMigration = false;
      }
      else if (fromTimer && nextMigrationAttempt.Equals(DateTime.MinValue) || nextMigrationAttempt.Equals(DateTime.MaxValue) || DateTime.Now.CompareTo(nextMigrationAttempt) < 0)
      {
        doMigration = false;
      }

      if (doMigration)
      {
        MigrateExternalConnectionsToWorkbench(true);
      }

      // Re-enable the timer.
      if (fromTimer)
      {
        _connectionsMigrationTimer.Enabled = true;
      }
    }

    /// <summary>
    /// Closes and removes the <see cref="EditConnectionInfo"/> associated to the given <see cref="ExcelInterop.Worksheet"/>.
    /// </summary>
    /// <param name="workbook">An <see cref="ExcelInterop.Workbook"/>.</param>
    /// <param name="missingWorksheetName">The name of the <see cref="ExcelInterop.Worksheet"/> that no longer exists and that is associated to the <see cref="EditConnectionInfo"/> to close.</param>
    private void CloseMissingWorksheetEditConnectionInfo(ExcelInterop.Workbook workbook, string missingWorksheetName)
    {
      if (workbook == null || missingWorksheetName.Length == 0)
      {
        return;
      }

      var workbookConnectionInfos = GetWorkbookEditConnectionInfos(workbook);
      var wsConnectionInfo = workbookConnectionInfos.FirstOrDefault(connectionInfo => !connectionInfo.EditDialog.EditingWorksheetExists);
      if (wsConnectionInfo == null)
      {
        return;
      }

      wsConnectionInfo.EditDialog.Close();
    }

    /// <summary>
    /// Closes and removes the <see cref="EditConnectionInfo"/> associated to the given <see cref="ExcelInterop.Worksheet"/>.
    /// </summary>
    /// <param name="worksheet">The <see cref="ExcelInterop.Worksheet"/> associated to the <see cref="EditConnectionInfo"/> to close.</param>
    private void CloseWorksheetEditConnectionInfo(ExcelInterop.Worksheet worksheet)
    {
      if (worksheet == null)
      {
        return;
      }

      ExcelInterop.Workbook parentWorkbook = worksheet.Parent as ExcelInterop.Workbook;
      if (parentWorkbook == null)
      {
        return;
      }

      var workbookConnectionInfos = GetWorkbookEditConnectionInfos(parentWorkbook);
      var wsConnectionInfo = workbookConnectionInfos.FirstOrDefault(connectionInfo => string.Equals(connectionInfo.EditDialog.WorkbookName, parentWorkbook.Name, StringComparison.InvariantCulture) && string.Equals(connectionInfo.EditDialog.WorksheetName, worksheet.Name, StringComparison.InvariantCulture));
      if (wsConnectionInfo == null)
      {
        return;
      }

      wsConnectionInfo.EditDialog.Close();
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="_connectionsMigrationTimer"/> ticks.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void ConnectionsMigrationTimer_Tick(object sender, EventArgs e)
    {
      CheckForNextAutomaticConnectionsMigration(true);
    }

    /// <summary>
    /// Converts the settings stored mappings property to the renamed MySqlColumnMapping class.
    /// </summary>
    private void ConvertSettingsStoredMappingsCasing()
    {
      if (Settings.Default.ConvertedSettingsStoredMappingsCasing)
      {
        return;
      }

      // Check if settings file exists, if it does not flag the conversion as done since it was not needed.
      MySqlForExcelSettings settings = new MySqlForExcelSettings();
      if (!File.Exists(settings.SettingsPath))
      {
        Settings.Default.ConvertedSettingsStoredMappingsCasing = true;
        MiscUtilities.SaveSettings();
        return;
      }

      // Open the settings.config file for writing and convert the MySQLColumnMapping class to MySqlColumnMapping.
      try
      {
        bool converted = false;
        string settingsConfigText = File.ReadAllText(settings.SettingsPath, Encoding.Unicode);
        if (settingsConfigText.Contains("MySQLColumnMapping"))
        {
          settingsConfigText = settingsConfigText.Replace("MySQLColumnMapping", "MySqlColumnMapping");
          converted = true;
        }

        if (!converted)
        {
          return;
        }

        File.WriteAllText(settings.SettingsPath, settingsConfigText, Encoding.Unicode);
        Settings.Default.Reload();
        Settings.Default.ConvertedSettingsStoredMappingsCasing = true;
        MiscUtilities.SaveSettings();
      }
      catch (Exception ex)
      {
        MySqlSourceTrace.WriteAppErrorToLog(ex, false);
      }
    }

    /// <summary>
    /// Customizes the looks of some dialogs found in the MySQL.Utility for ExcelInterop.
    /// </summary>
    private void CustomizeUtilityDialogs()
    {
      InfoDialog.ApplicationName = AssemblyTitle;
      InfoDialog.SuccessLogo = Resources.MySQLforExcel_InfoDlg_Success_64x64;
      InfoDialog.ErrorLogo = Resources.MySQLforExcel_InfoDlg_Error_64x64;
      InfoDialog.WarningLogo = Resources.MySQLforExcel_InfoDlg_Warning_64x64;
      InfoDialog.InformationLogo = Resources.MySQLforExcel_Logo_64x64;
      PasswordDialog.ApplicationIcon = Resources.mysql_for_excel;
      PasswordDialog.SecurityLogo = Resources.MySQLforExcel_Security;
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="CustomTaskPane"/> visible property value changes.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Sender object.</param>
    private void CustomTaskPaneVisibleChanged(object sender, EventArgs e)
    {
      OfficeTools.CustomTaskPane customTaskPane = sender as OfficeTools.CustomTaskPane;
      CustomMySqlRibbon.ChangeShowMySqlForExcelPaneToggleState(customTaskPane != null && customTaskPane.Visible);
    }

    /// <summary>
    /// Deletes automatically saved connection information entries with non-existent Excel Workbooks.
    /// </summary>
    /// <param name="logOperation">Flag indicating whether this operation is written in the application log.</param>
    private void DeleteConnectionInfosWithNonExistentWorkbook(bool logOperation)
    {
      if (!Settings.Default.DeleteAutomaticallyOrphanedConnectionInfos)
      {
        return;
      }

      var orphanedConnectionInfos = ManageConnectionInfosDialog.GetConnectionInfosWithNonExistentWorkbook();
      if (orphanedConnectionInfos == null)
      {
        return;
      }

      if (logOperation)
      {
        MySqlSourceTrace.WriteToLog(Resources.DeletingConnectionInfosWithNonExistentWorkbook, false, SourceLevels.Information);
      }

      foreach (var connectionInfo in orphanedConnectionInfos)
      {
        if (connectionInfo.GetType() == typeof(EditConnectionInfo))
        {
          Globals.ThisAddIn.EditConnectionInfos.Remove(connectionInfo as EditConnectionInfo);
        }
        else
        {
          Globals.ThisAddIn.StoredImportConnectionInfos.Remove(connectionInfo as ImportConnectionInfo);
        }
      }

      MiscUtilities.SaveSettings();
    }

    /// <summary>
    /// Delete the closed workbook's <see cref="EditConnectionInfo"/> objects from the settings file.
    /// </summary>
    private void DeleteCurrentWorkbookEditConnectionInfos(ExcelInterop.Workbook workbook)
    {
      if (workbook == null || string.IsNullOrEmpty(workbook.GetOrCreateId()))
      {
        return;
      }

      // Remove all EditConnectionInfo objects from the current workbook.
      var workbookConnectionInfos = GetWorkbookEditConnectionInfos(workbook);
      foreach (var connectionInfo in workbookConnectionInfos)
      {
        EditConnectionInfos.Remove(connectionInfo);
      }

      Settings.Default.Save();
      if (workbookConnectionInfos.Count > 0)
      {
        _editConnectionInfosByWorkbook.Remove(workbook.GetOrCreateId());
      }
    }

    /// <summary>
    /// Performs initializations that must occur when the first Excel pane is opened by the user and not at the Add-In startup.
    /// </summary>
    private void ExcelAddInPaneFirstRun()
    {
      _editConnectionInfosByWorkbook = new Dictionary<string, List<EditConnectionInfo>>(EditConnectionInfos.Count);
      _lastDeactivatedSheetName = string.Empty;
      _restoringExistingConnectionInfo = false;
      _skipSelectedDataContentsDetection = false;
      SkipWorksheetChangeEvent = false;

      // Subscribe to Excel events
      SetupExcelEvents(true);

      // Create custom MySQL Excel table style in the active workbook(s).
      if (ExcelVersionNumber <= EXCEL_2007_VERSION_NUMBER)
      {
        // Note that in Excel 2007 and 2010 a MDI model is used so only a single Excel pane is instantiated containing all Workbooks,
        // in which case all workbooks need to be initialized, not only the active one.
        foreach (ExcelInterop.Workbook workbook in Application.Workbooks)
        {
          InitializeWorkbook(workbook);
        }
      }
      else
      {
        InitializeWorkbook(ActiveWorkbook);
      }

      // Automatically delete ConnectionInfos that have a non-existent Excel Workbook.
      DeleteConnectionInfosWithNonExistentWorkbook(true);

      // Restore EditConnectionInfos
      ShowOpenEditConnectionInfosDialog(ActiveWorkbook);
    }

    /// <summary>
    /// Performs clean-up code that must be done after all Excel panes have been closed by the user.
    /// </summary>
    private void ExcelAddInPanesClosed()
    {
      // Unsubscribe from Excel events
      SetupExcelEvents(false);
      if (_editConnectionInfosByWorkbook != null)
      {
        _editConnectionInfosByWorkbook.Clear();

      }
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="ExcelAddInPane"/> size changes.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void ExcelPane_SizeChanged(object sender, EventArgs e)
    {
      ExcelAddInPane excelPane = sender as ExcelAddInPane;

      // Find the parent Custom Task Pane
      OfficeTools.CustomTaskPane customTaskPane = CustomTaskPanes.FirstOrDefault(ctp => ctp.Control == excelPane);
      if (customTaskPane == null || !customTaskPane.Visible)
      {
        return;
      }

      // Since there is no way to restrict the resizing of a custom task pane, cancel the resizing as soon as a
      //  user attempts to resize the pane.
      bool shouldResetWidth = false;
      int resetToWidth = customTaskPane.Width;
      if (resetToWidth < ADD_IN_MIN_PANE_WIDTH)
      {
        shouldResetWidth = true;
        resetToWidth = ADD_IN_MIN_PANE_WIDTH;
      }
      else if (resetToWidth > ADD_IN_MAX_PANE_WIDTH)
      {
        shouldResetWidth = true;
        resetToWidth = ADD_IN_MAX_PANE_WIDTH;
      }

      if (!shouldResetWidth)
      {
        return;
      }

      try
      {
        SendKeys.Send(ESCAPE_KEY);
        customTaskPane.Width = resetToWidth;
      }
      catch (Exception ex)
      {
        MySqlSourceTrace.WriteAppErrorToLog(ex, false);
      }
    }

    /// <summary>
    /// Gets a subset of the <see cref="EditConnectionInfos"/> listing only those assocaiated to the given <see cref="ExcelInterop.Workbook"/>.
    /// </summary>
    /// <param name="workbook">A <see cref="ExcelInterop.Workbook"/> with active <see cref="EditConnectionInfo"/> objects.</param>
    /// <returns>A subset of the <see cref="EditConnectionInfos"/> listing only those assocaiated to the given <see cref="ExcelInterop.Workbook"/></returns>
    private List<EditConnectionInfo> GetWorkbookEditConnectionInfos(ExcelInterop.Workbook workbook)
    {
      List<EditConnectionInfo> workbookConnectionInfos = null;
      string workbookId = workbook.GetOrCreateId();
      if (_editConnectionInfosByWorkbook != null && !string.IsNullOrEmpty(workbookId))
      {
        if (_editConnectionInfosByWorkbook.ContainsKey(workbookId))
        {
          workbookConnectionInfos = _editConnectionInfosByWorkbook[workbookId];
        }
        else
        {
          workbookConnectionInfos = EditConnectionInfos.FindAll(connectionInfo => string.Equals(connectionInfo.WorkbookGuid, workbookId, StringComparison.InvariantCulture));
          _editConnectionInfosByWorkbook.Add(workbookId, workbookConnectionInfos);
        }
      }

      return workbookConnectionInfos ?? new List<EditConnectionInfo>();
    }

    /// <summary>
    /// Gets a subset of the <see cref="StoredImportConnectionInfos" /> listing only those assocaiated to the given <see cref="ExcelInterop.Workbook" />.
    /// </summary>
    /// <param name="workbookId">Workbook Id to match the sub set of ImportConnectionInfos to.</param>
    /// <returns> A subset of the <see cref="StoredImportConnectionInfos" /> listing only those assocaiated to the given <see cref="ExcelInterop.Workbook" /></returns>
    private List<ImportConnectionInfo> GetWorkbookImportConnectionInfos(string workbookId)
    {
      return StoredImportConnectionInfos.FindAll(connectionInfo => string.Equals(connectionInfo.WorkbookGuid, workbookId, StringComparison.InvariantCulture));
    }

    /// <summary>
    /// Gets a subset of the <see cref="StoredImportConnectionInfos" /> listing only those assocaiated to the given <see cref="ExcelInterop.Worksheet" />.
    /// </summary>
    /// <param name="workbookId">Workbook Id to match the sub set of <see cref="ImportConnectionInfo" /> to.</param>
    /// <param name="worksheetName">Worksheet Name to match the sub set of <see cref="ImportConnectionInfo" /> to.</param>
    /// <returns>A subset of the <see cref="StoredImportConnectionInfos" /> listing only those assocaiated to the given <see cref="ExcelInterop.Worksheet" /></returns>
    private List<ImportConnectionInfo> GetWorkSheetImportConnectionInfos(string workbookId, string worksheetName)
    {
      List<ImportConnectionInfo> worksheetConnectionInfos = GetWorkbookImportConnectionInfos(workbookId);
      return worksheetConnectionInfos.FindAll(connectionInfo => string.Equals(connectionInfo.WorksheetName, worksheetName, StringComparison.InvariantCulture));
    }

    /// <summary>
    /// Initializes settings for the <see cref="MySqlWorkbench"/> and <see cref="MySqlWorkbenchPasswordVault"/> classes.
    /// </summary>
    private void InitializeMySqlWorkbenchStaticSettings()
    {
      string applicationDataFolderPath = EnvironmentApplicationDataDirectory;
      MySqlSourceTrace.LogFilePath = applicationDataFolderPath + ERROR_LOG_FILE_RELATIVE_PATH;
      MySqlSourceTrace.SourceTraceClass = "MySQLForExcel";
      MySqlWorkbench.ExternalApplicationName = AssemblyTitle;
      MySqlWorkbenchPasswordVault.ApplicationPasswordVaultFilePath = applicationDataFolderPath + PASSWORDS_VAULT_FILE_RELATIVE_PATH;
      MySqlWorkbench.ExternalConnections.CreateDefaultConnections = !MySqlWorkbench.ConnectionsFileExists && MySqlWorkbench.Connections.Count == 0;
      MySqlWorkbench.ExternalApplicationConnectionsFilePath = applicationDataFolderPath + CONNECTIONS_FILE_RELATIVE_PATH;
      MySqlWorkbench.ChangeCurrentCursor = delegate (Cursor cursor)
      {
        if (cursor == Cursors.WaitCursor)
        {
          Globals.ThisAddIn.Application.Cursor = ExcelInterop.XlMousePointer.xlWait;
        }
        else if (cursor == Cursors.Default)
        {
          Globals.ThisAddIn.Application.Cursor = ExcelInterop.XlMousePointer.xlDefault;
        }
      };
    }

    /// <summary>
    /// Method used to initialize a <see cref="ExcelInterop.Workbook" /> when it is opened or created.
    /// </summary>
    /// <param name="workbook">The <see cref="ExcelInterop.Workbook" /> being opened.</param>
    private void InitializeWorkbook(ExcelInterop.Workbook workbook)
    {
      if (workbook == null)
      {
        return;
      }

      // Add the custom MySQL table style (for Excel tables) to this workbook.
      workbook.CreateMySqlTableStyle();

      // When it is a new workbook it won't have any IConnectionInfo object related to it, so we could skip the rest of the method altogether.
      if (workbook.IsNew())
      {
        return;
      }

      RestoreImportConnectionInfos(workbook);
      if (ActiveExcelPane == null)
      {
        return;
      }

      ShowOpenEditConnectionInfosDialog(workbook);
    }

    /// <summary>
    /// Attempts to open a <see cref="MySqlWorkbenchConnection"/> from an Editing table.
    /// </summary>
    /// <param name="connectionInfoConnection">The <see cref="MySqlWorkbenchConnection"/> the <see cref="EditConnectionInfo" /> uses.</param>
    /// <returns><c>true</c> if the connection was successfully opened, <c>false</c> otherwise.</returns>
    private bool OpenConnectionForSavedEditConnectionInfo(MySqlWorkbenchConnection connectionInfoConnection)
    {
      var connectionResult = ActiveExcelPane.OpenConnection(connectionInfoConnection, false);
      if (connectionResult.Cancelled)
      {
        return false;
      }

      if (connectionResult.ConnectionSuccess)
      {
        return true;
      }

      InfoDialog.ShowDialog(InfoDialogProperties.GetWarningDialogProperties(Resources.RestoreConnectionInfosOpenConnectionErrorTitle, Resources.RestoreConnectionInfosOpenConnectionErrorDetail));
      return false;
    }

    /// <summary>
    /// Attempts to open a <see cref="MySqlWorkbenchConnection"/> from an Editing table.
    /// </summary>
    /// <param name="connectionInfo">A saved <see cref="EditConnectionInfo"/> object.</param>
    /// <param name="workbook">A <see cref="ExcelInterop.Workbook"/> object related to the <see cref="EditConnectionInfo" />.</param>
    /// <returns>The opened <see cref="MySqlWorkbenchConnection"/>.</returns>
    private MySqlWorkbenchConnection OpenConnectionForSavedEditConnectionInfo(EditConnectionInfo connectionInfo, ExcelInterop.Workbook workbook)
    {
      if (connectionInfo == null || workbook == null)
      {
        return null;
      }

      // Check if connection in stored the <see cref="EditConnectionInfo" /> still exists in the collection of Workbench connections.
      var wbConnectionInfoConnection = MySqlWorkbench.Connections.GetConnectionForId(connectionInfo.ConnectionId);
      DialogResult dialogResult;
      if (wbConnectionInfoConnection == null)
      {
        dialogResult = MiscUtilities.ShowCustomizedWarningDialog(Resources.RestoreConnectionInfosOpenConnectionErrorTitle, Resources.RestoreConnectionInfosWBConnectionNoLongerExistsFailedDetail);
        if (dialogResult == DialogResult.Yes)
        {
          DeleteCurrentWorkbookEditConnectionInfos(workbook);
        }

        return null;
      }

      wbConnectionInfoConnection.AllowZeroDateTimeValues = true;
      if (ActiveExcelPane.WbConnection == null)
      {
        // If the connection in the active pane is null it means an active connection does not exist, so open a connection.
        if (!OpenConnectionForSavedEditConnectionInfo(wbConnectionInfoConnection))
        {
          return null;
        }
      }
      else if (!string.Equals(wbConnectionInfoConnection.HostIdentifier, ActiveExcelPane.WbConnection.HostIdentifier, StringComparison.InvariantCulture))
      {
        // If the stored connection points to a different host as the current connection, ask the user if he wants to open a new connection only if there are active Edit dialogs.
        if (_editConnectionInfosByWorkbook.Count > 1)
        {
          var dialogProperties = InfoDialogProperties.GetYesNoDialogProperties(
            InfoDialog.InfoType.Warning,
            Resources.RestoreConnectionInfosTitle,
            Resources.RestoreConnectionInfosOpenConnectionCloseEditDialogsDetail,
            null,
            Resources.RestoreConnectionInfosOpenConnectionCloseEditDialogsMoreInfo);
          if (InfoDialog.ShowDialog(dialogProperties).DialogResult == DialogResult.No)
          {
            return null;
          }

          ActiveExcelPane.CloseSchema(false, false);
          ActiveExcelPane.CloseConnection(false);
        }

        if (!OpenConnectionForSavedEditConnectionInfo(wbConnectionInfoConnection))
        {
          return null;
        }
      }

      return ActiveExcelPane.WbConnection;
    }

    /// <summary>
    /// Opens an <see cref="EditDataDialog"/> for each <see cref="EditConnectionInfo" />.
    /// </summary>
    /// <param name="workbook">The workbook.</param>
    private void OpenEditConnectionInfosOfTables(ExcelInterop.Workbook workbook)
    {
      if (workbook == null)
      {
        return;
      }

      var workbookConnectionInfos = GetWorkbookEditConnectionInfos(workbook);
      if (workbookConnectionInfos.Count == 0)
      {
        return;
      }

      var missingTables = new List<string>();
      _restoringExistingConnectionInfo = true;
      foreach (var connectionInfos in workbookConnectionInfos)
      {
        var editTableObject = ActiveExcelPane.LoadedTables.FirstOrDefault(dbo => string.Equals(dbo.Name, connectionInfos.TableName, StringComparison.InvariantCulture));
        if (editTableObject == null)
        {
          missingTables.Add(connectionInfos.TableName);
          continue;
        }

        ActiveExcelPane.EditTableData(editTableObject, true, workbook);
      }

      if (workbookConnectionInfos.Count - missingTables.Count > 0)
      {
        ActiveExcelPane.ActiveEditDialog.ShowDialog();
      }

      _restoringExistingConnectionInfo = false;

      // If no errors were found at the opening process do not display the warning dialog at the end.
      if (missingTables.Count <= 0)
      {
        return;
      }

      var errorMessage = new StringBuilder();
      if (missingTables.Count > 0)
      {
        errorMessage.AppendLine(Resources.RestoreConnectionInfosMissingTablesMessage);
        foreach (var table in missingTables)
        {
          errorMessage.AppendLine(table);
        }
      }

      MiscUtilities.ShowCustomizedInfoDialog(InfoDialog.InfoType.Warning, Resources.RestoreConnectionInfosWarningMessage, errorMessage.ToString());
    }

    /// <summary>
    /// Adjusts values in the settings.config file that have changed and must be adjusted or transformed.
    /// </summary>
    private void PerformSettingsAdjustments()
    {
      ConvertSettingsStoredMappingsCasing();
      AdjustSettingsForBulkInserts();
    }

    /// <summary>
    /// Processes the missing connections to either create and assign them a new connection or disconnect their excel tables.
    /// </summary>
    /// <param name="missingConnectionInfoConnections">A list of <see cref="ImportConnectionInfo" /> objects which connection is not found.</param>
    /// <param name="workbook">The <see cref="Microsoft.Office.Interop.Excel.Workbook" /> the list of <see cref="ImportConnectionInfo" /> belong to.</param>
    private void ProcessMissingConnectionInfoWorkbenchConnections(List<ImportConnectionInfo> missingConnectionInfoConnections, ExcelInterop.Workbook workbook)
    {
      if (missingConnectionInfoConnections.Count <= 0)
      {
        return;
      }

      var moreInfoText = MySqlWorkbench.IsRunning
        ? Resources.UnableToAddConnectionsWhenWBRunning + Environment.NewLine + Resources.ImportConnectionInfosMissingConnectionsMoreInfo
        : Resources.ImportConnectionInfosMissingConnectionsMoreInfo;
      var stringBuilder = new StringBuilder(moreInfoText);
      List<string> missingHostIds = missingConnectionInfoConnections.Select(i => i.HostIdentifier).Distinct().ToList();
      foreach (var missingHostId in missingHostIds)
      {
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append(missingHostId);
      }

      var dialogProperties = InfoDialogProperties.GetWarningDialogProperties(
        Resources.ImportConnectionInfosMissingConnectionsTitle,
        Resources.ImportConnectionInfosMissingConnectionsDetail,
        null,
        stringBuilder.ToString());
      dialogProperties.CommandAreaProperties = new CommandAreaProperties(CommandAreaProperties.ButtonsLayoutType.Generic3Buttons)
      {
        Button1Text = Resources.CreateButtonText,
        Button1DialogResult = DialogResult.OK,
        Button2Text = Resources.DeleteAllButtonText,
        Button2DialogResult = DialogResult.Cancel,
        Button3Text = Resources.WorkOfflineButtonText,
        Button3DialogResult = DialogResult.Abort
      };
      dialogProperties.WordWrapMoreInfo = false;
      switch (InfoDialog.ShowDialog(dialogProperties).DialogResult)
      {
        case DialogResult.OK:
          // If Workbench is running we can't add new connections, so we ask the user to close it. if he still decides not to do so we disconnect all excel tables to work offline.
          var workbenchWarningDialogResult = DialogResult.None;
          while (MySqlWorkbench.IsRunning && workbenchWarningDialogResult != DialogResult.Cancel)
          {
            workbenchWarningDialogResult = InfoDialog.ShowDialog(InfoDialogProperties.GetErrorDialogProperties(
              Resources.OperationErrorTitle,
              Resources.UnableToAddConnectionsWhenWBRunning,
              Resources.CloseWBAdviceToAdd,
              Resources.CloseWBAdviceToAdd)).DialogResult;
          }

          if (workbenchWarningDialogResult == DialogResult.Cancel)
          {
            missingConnectionInfoConnections.ForEach(connectionInfo => connectionInfo.ExcelTable.Unlink());
            break;
          }

          List<string> missingConnectionIds = missingConnectionInfoConnections.Select(i => i.ConnectionId).Distinct().ToList();
          foreach (var missingConnectionId in missingConnectionIds)
          {
            //Fill the new connection with the old HostIdentifier information for the New Connection Dialog if available;
            var missingConnectionconnectionInfo = missingConnectionInfoConnections.FirstOrDefault(s => s.ConnectionId == missingConnectionId);
            //Create the new connection and assign it to all corresponding connectionInfos.
            using (var newConnectionDialog = new MySqlWorkbenchConnectionDialog(null, false))
            {
              //If the HostIdentifier is set, we use it to fill in the blanks for the new connection in the dialog.
              if (missingConnectionconnectionInfo != null && !string.IsNullOrEmpty(missingConnectionconnectionInfo.HostIdentifier))
              {
                var hostIdArray = missingConnectionconnectionInfo.HostIdentifier.ToLower().Replace("mysql@", string.Empty).Split(':').ToArray();
                var host = hostIdArray.Length > 0 ? hostIdArray[0] : string.Empty;
                var portString = hostIdArray.Length > 1 ? hostIdArray[1] : string.Empty;
                uint port;
                uint.TryParse(portString, out port);
                newConnectionDialog.WorkbenchConnection.Host = host;
                newConnectionDialog.WorkbenchConnection.Port = port;
              }

              var result = newConnectionDialog.ShowDialog();
              //For each connectionInfo that is pointing to the same connection
              foreach (var connectionInfo in missingConnectionInfoConnections.Where(connectionInfo => connectionInfo.ConnectionId == missingConnectionId).ToList())
              {
                if (result == DialogResult.OK)
                {
                  //If the connection was created we reassign every corresponding connectionInfo of this set to it.
                  connectionInfo.ConnectionId = newConnectionDialog.WorkbenchConnection.Id;
                  connectionInfo.Restore(workbook);
                  MiscUtilities.SaveSettings();
                }
                else
                {
                  //If the user cancels the creation of a new connection for this set of connectionInfos, we just need to disconnect their Excel Tables.
                  connectionInfo.ExcelTable.Unlink();
                }
              }
            }
          }
          break;
        case DialogResult.Cancel:
          foreach (var connectionInfo in missingConnectionInfoConnections)
          {
            connectionInfo.ExcelTable.Unlink();
            StoredImportConnectionInfos.Remove(connectionInfo);
          }
          break;
        case DialogResult.Abort: //The user selected Work offline so we will disconnect every invalid connectionInfo.
          missingConnectionInfoConnections.ForEach(connectionInfo => connectionInfo.ExcelTable.Unlink());
          break;
      }
    }

    /// <summary>
    /// Protects all Worksheets that have an active Edit Data session.
    /// </summary>
    /// <param name="workbook">A <see cref="ExcelInterop.Workbook"/> object.</param>
    /// <param name="addEditingWorksheetToPersistedList">Flag indicating whether the <see cref="EditConnectionInfo"/>s are added to the collection persisted to disk.</param>
    /// <param name="workbookEditConnectionInfos"></param>
    private void ProtectEditingWorksheets(ExcelInterop.Workbook workbook, bool addEditingWorksheetToPersistedList, List<EditConnectionInfo> workbookEditConnectionInfos = null)
    {
      if (workbook == null)
      {
        return;
      }

      if (workbookEditConnectionInfos == null)
      {
        workbookEditConnectionInfos = GetWorkbookEditConnectionInfos(workbook);
      }

      foreach (var activeEditConnectionInfo in workbookEditConnectionInfos)
      {
        if (activeEditConnectionInfo.EditDialog != null && !activeEditConnectionInfo.EditDialog.EditingWorksheet.ProtectContents)
        {
          activeEditConnectionInfo.EditDialog.ProtectWorksheet();
        }

        if (!addEditingWorksheetToPersistedList)
        {
          continue;
        }

        // Add new EditConnectionInfo in memory collection to serialized collection
        activeEditConnectionInfo.LastAccess = DateTime.Now;
        activeEditConnectionInfo.WorkbookFilePath = workbook.FullName;
        if (!EditConnectionInfos.Contains(activeEditConnectionInfo))
        {
          EditConnectionInfos.Add(activeEditConnectionInfo);
        }
      }
    }

    /// <summary>
    /// Removes invalid import connection information from the collection.
    /// </summary>
    private void RemoveInvalidImportConnectionInformation()
    {
      var invalidConnectionInfos = new List<ImportConnectionInfo>();
      foreach (var importConnectionInfo in ActiveWorkbookImportConnectionInfos)
      {
        try
        {
          // DO NOT REMOVE this line. If the excel table is invalid, accessing it will throw an exception.
          var excelTableComment = importConnectionInfo.ExcelTable.Comment;
        }
        catch
        {
          // The importConnectionInfo's list object was moved to another worksheet or when its columns had been deleted or the reference to it no longer exists.
          invalidConnectionInfos.Add(importConnectionInfo);
        }
      }

      // Dispose of ImportConnectionInfo objects that are no longer valid for the current workbook.
      if (invalidConnectionInfos.Count > 0)
      {
        invalidConnectionInfos.ForEach(invalidSession => invalidSession.ExcelTable.DeleteSafely(false));
        invalidConnectionInfos.ForEach(invalidSession => StoredImportConnectionInfos.Remove(invalidSession));
      }
    }

    ///  <summary>
    /// Restores saved <see cref="EditConnectionInfo"/> objects from the given <see cref="ExcelInterop.Workbook"/>.
    /// </summary>
    /// <param name="workbook">An <see cref="ExcelInterop.Workbook"/> with saved <see cref="EditConnectionInfo"/> objects.</param>
    private void RestoreEditConnectionInfos(ExcelInterop.Workbook workbook)
    {
      if (workbook == null || ActiveExcelPane == null || _editConnectionInfosByWorkbook.ContainsKey(workbook.GetOrCreateId()))
      {
        return;
      }

      // Add the EditConnectionInfo objects for the workbook being opened to the dictionary.
      // The GetWorkbookEditConnectionInfos method will add the EditConnectionInfo objects related to the workbook it if they haven't been added.
      var workbookEditConnectionInfos = GetWorkbookEditConnectionInfos(workbook);
      if (!Settings.Default.EditSessionsRestoreWhenOpeningWorkbook || workbookEditConnectionInfos.Count == 0)
      {
        return;
      }

      // Open the connection from the EditConnectionInfo, check also if the current connection can be used to avoid opening a new one.
      var currenConnection = ActiveExcelPane.WbConnection;
      var firstConnectionInfo = workbookEditConnectionInfos[0];
      var currentSchema = currenConnection != null ? currenConnection.Schema : string.Empty;
      var connectionInfoConnection = OpenConnectionForSavedEditConnectionInfo(firstConnectionInfo, workbook);
      if (connectionInfoConnection == null)
      {
        return;
      }

      // Close the current schema if the current connection is being reused but the EditConnectionInfo's schema is different
      bool connectionReused = connectionInfoConnection.Equals(currenConnection);
      bool openSchema = !connectionReused;
      if (connectionReused && !string.Equals(currentSchema, firstConnectionInfo.SchemaName, StringComparison.InvariantCulture))
      {
        if (!ActiveExcelPane.CloseSchema(true, false))
        {
          return;
        }

        openSchema = true;
      }

      if (openSchema)
      {
        // Verify if the EditConnectionInfo's schema to be opened still exists in the connected MySQL server
        if (!ActiveExcelPane.LoadedSchemas.Exists(schemaObj => schemaObj.Name == firstConnectionInfo.SchemaName))
        {
          var errorMessage = string.Format(Resources.RestoreConnectionInfosSchemaNoLongerExistsFailed, connectionInfoConnection.HostIdentifier, connectionInfoConnection.Schema);
          MiscUtilities.ShowCustomizedInfoDialog(InfoDialog.InfoType.Error, errorMessage);
          return;
        }

        // Open the EditConnectionInfo's schema
        ActiveExcelPane.OpenSchema(firstConnectionInfo.SchemaName, true);
      }

      // Open the EditConnectionInfo for each of the tables being edited
      OpenEditConnectionInfosOfTables(workbook);
    }

    /// <summary>
    /// Restores the <see cref="ImportConnectionInfo"/>s that are tied to the given <see cref="ExcelInterop.Workbook"/>.
    /// </summary>
    /// <param name="workbook">A <see cref="ExcelInterop.Workbook"/> object.</param>
    private void RestoreImportConnectionInfos(ExcelInterop.Workbook workbook)
    {
      if (workbook == null)
      {
        return;
      }

      var workbookId = workbook.GetOrCreateId();
      var importConnectionInfos = GetWorkbookImportConnectionInfos(workbookId);
      if (importConnectionInfos == null)
      {
        return;
      }

      foreach (ImportConnectionInfo connectionInfo in importConnectionInfos)
      {
        connectionInfo.Restore(workbook);
      }

      // Verify missing connections and ask the user for action to take?
      ProcessMissingConnectionInfoWorkbenchConnections(importConnectionInfos.Where(connectionInfo => connectionInfo.ConnectionInfoError == ImportConnectionInfo.ConnectionInfoErrorType.WorkbenchConnectionDoesNotExist).ToList(), workbook);
    }

    /// <summary>
    /// Setups the excel events.
    /// </summary>
    /// <param name="subscribe">if set to <c>true</c> [subscribe].</param>
    private void SetupExcelEvents(bool subscribe)
    {
      if (subscribe)
      {
        // Excel version corresponds to Excel 2013 or greater.
        if (ExcelVersionNumber >= EXCEL_2013_VERSION_NUMBER)
        {
          //  Monitor the Excel windows activation and deactivation in order to synchronize the Add-In's toggle button state and dispose custom task panes when its parent window closes.
          Application.WindowActivate += Application_WindowActivate;
        }

        ((ExcelInterop.AppEvents_Event)Application).NewWorkbook += Application_WorkbookNewWorkbook;
        Application.SheetActivate += Application_SheetActivate;
        Application.SheetBeforeDelete += Application_SheetBeforeDelete;
        Application.SheetChange += Application_SheetChange;
        Application.SheetDeactivate += Application_SheetDeactivate;
        Application.SheetSelectionChange += Application_SheetSelectionChange;
        Application.WorkbookActivate += Application_WorkbookActivate;
        Application.WorkbookAfterSave += Application_WorkbookAfterSave;
        Application.WorkbookBeforeClose += Application_WorkbookBeforeClose;
        Application.WorkbookBeforeSave += Application_WorkbookBeforeSave;
        Application.WorkbookDeactivate += Application_WorkbookDeactivate;
      }
      else
      {
        if (ExcelVersionNumber >= EXCEL_2013_VERSION_NUMBER)
        {
          Application.WindowActivate -= Application_WindowActivate;
        }

        ((ExcelInterop.AppEvents_Event)Application).NewWorkbook -= Application_WorkbookNewWorkbook;
        Application.SheetActivate -= Application_SheetActivate;
        Application.SheetBeforeDelete -= Application_SheetBeforeDelete;
        Application.SheetChange -= Application_SheetChange;
        Application.SheetDeactivate -= Application_SheetDeactivate;
        Application.SheetSelectionChange -= Application_SheetSelectionChange;
        Application.WorkbookActivate -= Application_WorkbookActivate;
        Application.WorkbookAfterSave -= Application_WorkbookAfterSave;
        Application.WorkbookBeforeClose -= Application_WorkbookBeforeClose;
        Application.WorkbookBeforeSave -= Application_WorkbookBeforeSave;
        Application.WorkbookDeactivate -= Application_WorkbookDeactivate;
      }
    }

    /// <summary>
    /// Shows a dialog to the users to decide what to do with saved <see cref="EditConnectionInfo"/> objects.
    /// </summary>
    /// <param name="workbook">The <see cref="ExcelInterop.Workbook"/> that may contain saved <see cref="EditConnectionInfo"/> objects.</param>
    private void ShowOpenEditConnectionInfosDialog(ExcelInterop.Workbook workbook)
    {
      if (workbook == null)
      {
        return;
      }

      var workbookId = workbook.GetOrCreateId();
      if (!EditConnectionInfos.Exists(connectionInfo => connectionInfo.WorkbookGuid == workbookId) || _editConnectionInfosByWorkbook.ContainsKey(workbookId))
      {
        return;
      }

      var infoProperties = InfoDialogProperties.GetWarningDialogProperties(Resources.RestoreEditConnectionInfoTitle, Resources.RestoreEditConnectionInfoDetail);
      infoProperties.CommandAreaProperties.ButtonsLayout = CommandAreaProperties.ButtonsLayoutType.Generic3Buttons;
      infoProperties.CommandAreaProperties.Button1Text = Resources.RestoreButtonText;
      infoProperties.CommandAreaProperties.Button1DialogResult = DialogResult.Yes;
      infoProperties.CommandAreaProperties.Button2Text = Resources.WorkOfflineButtonText;
      infoProperties.CommandAreaProperties.Button2DialogResult = DialogResult.Cancel;
      infoProperties.CommandAreaProperties.Button3Text = Resources.DeleteButtonText;
      infoProperties.CommandAreaProperties.Button3DialogResult = DialogResult.Abort;
      infoProperties.WordWrapMoreInfo = false;
      var infoResult = InfoDialog.ShowDialog(infoProperties);
      switch (infoResult.DialogResult)
      {
        case DialogResult.Abort:
          // Discard: Do not open any and delete all saved EditConnectionInfo objects for the current workbook.
          DeleteCurrentWorkbookEditConnectionInfos(workbook);
          break;

        case DialogResult.Yes:
          // Attempt to restore EditConnectionInfo objects for the workbook being opened
          RestoreEditConnectionInfos(workbook);
          break;
      }
    }

    /// <summary>
    /// Starts the global timer that fires connections migration checks.
    /// </summary>
    private void StartConnectionsMigrationTimer()
    {
      _connectionsMigrationTimer = null;
      _migratingStoredConnections = false;

      // Determine if the timer is needed
      if (Settings.Default.WorkbenchMigrationSucceeded && !MySqlWorkbench.ExternalApplicationConnectionsFileExists)
      {
        return;
      }

      _connectionsMigrationTimer = new Timer();
      _connectionsMigrationTimer.Tick += ConnectionsMigrationTimer_Tick;
      _connectionsMigrationTimer.Interval = MILLISECONDS_IN_HOUR;
      _connectionsMigrationTimer.Start();
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="ThisAddIn"/> is closed.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void ThisAddIn_Shutdown(object sender, EventArgs e)
    {
      // Stop global timer and dispose of it
      if (_connectionsMigrationTimer != null)
      {
        if (_connectionsMigrationTimer.Enabled)
        {
          _connectionsMigrationTimer.Enabled = false;
        }

        _connectionsMigrationTimer.Dispose();
      }

      // Close all Excel panes created
      if (ExcelPanesList != null)
      {
        foreach (var excelPane in ExcelPanesList)
        {
          excelPane.Dispose();
        }
      }

      ExcelAddInPanesClosed();
      MySqlSourceTrace.WriteToLog(Resources.ShutdownMessage, false, SourceLevels.Information);

      // Unsubscribe events tracked even when no Excel panes are open.
      Application.WorkbookOpen -= Application_WorkbookOpen;

      // Dispose (close) all ImportConnectionInfo object
      if (ActiveWorkbookImportConnectionInfos != null && ActiveWorkbookImportConnectionInfos.Count > 0)
      {
        foreach (var importConnectionInfo in ActiveWorkbookImportConnectionInfos)
        {
          importConnectionInfo.Dispose();
        }
      }
    }

    /// <summary>
    /// Event delegate method fired when the <see cref="ThisAddIn"/> is started.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void ThisAddIn_Startup(object sender, EventArgs e)
    {
      try
      {
        // Static initializations.
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        CustomizeUtilityDialogs();
        InitializeMySqlWorkbenchStaticSettings();
        MySqlInstaller.LoadData();
        AssemblyTitle = AssemblyInfo.AssemblyTitle;
        UsingTempWorksheet = false;

        // Make sure the settings directory exists
        Directory.CreateDirectory(EnvironmentApplicationDataDirectory + SETTINGS_DIRECTORY_RELATIVE_PATH);

        // Log the Add-In's Startup
        MySqlSourceTrace.WriteToLog(Resources.StartupMessage, false, SourceLevels.Information);

        // Detect Excel version.
        int pointPos = Application.Version.IndexOf('.');
        string majorVersionText = pointPos >= 0 ? Application.Version.Substring(0, pointPos) : Application.Version;
        ExcelVersionNumber = Int32.Parse(majorVersionText, CultureInfo.InvariantCulture);

        // Adjust values in the settings.config file that have changed and must be adjusted or transformed
        PerformSettingsAdjustments();

        // Subscribe events tracked even when no Excel panes are open.
        Application.WorkbookOpen += Application_WorkbookOpen;

        // Start timer that checks for automatic connections migration.
        StartConnectionsMigrationTimer();
      }
      catch (Exception ex)
      {
        MySqlSourceTrace.WriteAppErrorToLog(ex, false);
      }
    }

    /// <summary>
    /// Unprotects all Worksheets that have an active Edit Data session.
    /// </summary>
    /// <param name="workbook">A <see cref="ExcelInterop.Workbook"/> object.</param>
    private void UnprotectEditingWorksheets(ExcelInterop.Workbook workbook)
    {
      var workbookEditConnectionInfos = GetWorkbookEditConnectionInfos(workbook);
      foreach (var activeEditConnectionInfo in workbookEditConnectionInfos.Where(activeEditConnectionInfo => activeEditConnectionInfo.EditDialog != null && activeEditConnectionInfo.EditDialog.EditingWorksheet.ProtectContents))
      {
        activeEditConnectionInfo.EditDialog.UnprotectWorksheet();
      }
    }

    #region VSTO generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InternalStartup()
    {
      Startup += ThisAddIn_Startup;
      Shutdown += ThisAddIn_Shutdown;
    }

    #endregion VSTO generated code
  }
}