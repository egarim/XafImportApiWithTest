using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.Office.Win;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Spreadsheet;
using DevExpress.Utils.Extensions;
using DevExpress.Xpo;
using DevExpress.XtraRichEdit.Model;
using DevExpress.XtraSpreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using XafImportApiWithTest.Module.BusinessObjects;
using XafImportApiWithTest.Module.Import;

namespace XafImportApiWithTest.Win.Controllers
{
	// For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
	public partial class SpreadSheet_ViewController1 : ViewController
	{
		// Use CodeRush to create Controllers and Actions with a few keystrokes.
		// https://docs.devexpress.com/CodeRushForRoslyn/403133/
		public SpreadSheet_ViewController1()
		{
			InitializeComponent();
			TargetObjectType = typeof(SpreadSheet);
			TargetViewType = ViewType.DetailView;
			SimpleAction Analyze = new SimpleAction(this, "Analyze", PredefinedCategory.View)
			{
				Caption = "Analyze",
				TargetViewType = ViewType.DetailView
			};
			Analyze.Execute += Analyze_Execute;


		}

		SpreadsheetPropertyEditor spreadsheetPropertyEditor = null;

		private void Analyze_Execute(object sender, SimpleActionExecuteEventArgs e)
		{
			string connectionString = this.Application.ConnectionString;
			var connectionProvider = XpoDefault.GetConnectionProvider(connectionString, DevExpress.Xpo.DB.AutoCreateOption.SchemaAlreadyExists);
			SimpleDataLayer simpleDataLayer = new SimpleDataLayer(connectionProvider);
			UnitOfWork unitOfWork = new UnitOfWork(simpleDataLayer);

			SpreadSheet currentObj = (SpreadSheet)e.CurrentObject;

            DevExpress.Spreadsheet.Workbook workbook = currentObj.GetSpreadSheet();

			SpreadsheetService spreadsheetService = new SpreadsheetService();
            var spreadSheetType = this.ObjectSpace.TypesInfo.FindTypeInfo(currentObj.TypeFullName);
            DevExpress.Spreadsheet.Worksheet analyzedSheets = spreadsheetService.AnalyzeSheet(View.ObjectSpace, workbook.Worksheets[0], unitOfWork, spreadSheetType.Type);
            //DevExpress.Spreadsheet.Worksheet analyzedSheets = spreadsheetService.AnalyzeSheet(View.ObjectSpace, workbook.Worksheets[0], unitOfWork);

			using (MemoryStream ms = new MemoryStream())
			{
				analyzedSheets.Workbook.SaveDocument(ms, DevExpress.Spreadsheet.DocumentFormat.OpenXml);

				ms.Position = 0;

				byte[] byte_array = new byte[ms.Length];
				ms.Read(byte_array, 0, byte_array.Length);

				currentObj.Data = byte_array;
				View.Refresh();

				//View.ObjectSpace.CommitChanges();
			}
			//RowDef.ObjectType = typeof(MainObject).FullName;
		}
		protected override void OnActivated()
		{
			base.OnActivated();
			// Perform various tasks depending on the target View.
		}
		protected override void OnViewControlsCreated()
		{
			base.OnViewControlsCreated();
			spreadsheetPropertyEditor = ((DetailView)View).FindItem("Data") as SpreadsheetPropertyEditor;
			// Access and customize the target View control.
		}
		protected override void OnDeactivated()
		{
			// Unsubscribe from previously subscribed events and release other references and resources.
			base.OnDeactivated();
		}
	}
}
