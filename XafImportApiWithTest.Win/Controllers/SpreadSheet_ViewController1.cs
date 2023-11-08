using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.Office.Win;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.PivotGrid.OLAP.SchemaEntities;
using DevExpress.Spreadsheet;
using DevExpress.Utils.Extensions;
using DevExpress.Xpo;
using DevExpress.XtraRichEdit.Model;
using DevExpress.XtraSpreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
			IObjectSpace objectSpace = View.ObjectSpace;
			DateTime startTime = DateTime.Now;
			SpreadSheet spreedSheetObj = (SpreadSheet)e.CurrentObject;
            DevExpress.Spreadsheet.Worksheet sheet = spreadsheetPropertyEditor.SpreadsheetControl.ActiveWorksheet.Workbook.Worksheets[0];

			DevExpress.Spreadsheet.Worksheet initialSheet = sheet.Workbook.Worksheets[0];

			SpreadsheetService spreadsheetService = new SpreadsheetService();
			var RowDef = spreadsheetService.GetData(initialSheet, objectSpace.TypesInfo, typeof(MainObject));

			foreach(var rowProperty in RowDef.Properties)
			{
				if(rowProperty.Value.PropertyKind == PropertyKind.Reference)
				{
					string propertyName = rowProperty.Value.PropertyName;
					if (!sheet.Workbook.Worksheets.Contains(propertyName))
					{
						sheet.Workbook.Worksheets.Add().Name = propertyName;
					}

					DevExpress.Spreadsheet.Worksheet destinationWorksheet = sheet.Workbook.Worksheets.FirstOrDefault(w => w.Name == propertyName);
					
					destinationWorksheet.Cells.FillColor = System.Drawing.Color.White;
					destinationWorksheet.Cells.Clear();

					Type mainObjectType = typeof(MainObject);


					System.Reflection.PropertyInfo propertyInfo = mainObjectType.GetProperty(propertyName);
					Type propertyType = propertyInfo.PropertyType;
					List<object> values = new List<object>();

					foreach (var row in RowDef.Rows)
					{
						values.Add(row[rowProperty.Key]);
					};
					CriteriaOperator criteria = new InOperator(rowProperty.Value.ReferencePropertyLookup, values);

					string connectionString = this.Application.ConnectionString;
					var connectionProvider = XpoDefault.GetConnectionProvider(connectionString, DevExpress.Xpo.DB.AutoCreateOption.SchemaAlreadyExists);
					SimpleDataLayer simpleDataLayer = new SimpleDataLayer(connectionProvider);
					UnitOfWork unitOfWork = new UnitOfWork(simpleDataLayer);
					var View = propertyType.CreateViewWithProperties(unitOfWork, criteria); // change criteria for null for more performance
					
					List<object> objectData = new List<object>();

					var defaultPropertyAttribute = View.ObjectClassInfo.FindAttributeInfo(typeof(DefaultPropertyAttribute)) as DefaultPropertyAttribute;
					string defaultProperty = defaultPropertyAttribute != null ? defaultPropertyAttribute.Name : rowProperty.Value.ReferencePropertyLookup;

					foreach (ViewRecord record in View)
					{
						objectData.Add(record[defaultProperty]);
					}

					int startIndex = 0;
					List<object> objectsToAdd = new List<object>();

					foreach (var row in RowDef.Rows)
					{
						startIndex++;
						if (!objectData.Contains(row[rowProperty.Key].ToString()))
						{
							objectsToAdd.Add(row[rowProperty.Key]);
						}
					};

					destinationWorksheet.Cells[0, 0].Value = defaultProperty;


					destinationWorksheet.Import(objectData, 1, 0, true);


			
					foreach (var obj in objectsToAdd)
					{

						destinationWorksheet.Rows.Insert(1);
						destinationWorksheet.Cells[1, 0].Value = obj.ToString();
						destinationWorksheet.Cells[1, 0].FillColor = System.Drawing.Color.Red;
					}


				}
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
