using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp;
using XafImportApiWithTest.Module.BusinessObjects;
using DevExpress.Spreadsheet;
using System.Security.AccessControl;
using XafImportApiWithTest.Module.Import;
using DevExpress.Xpo.DB;
using System.Diagnostics;
using System.Reflection;

namespace Tests
{
    public class Tests
    {
        private static void EnableXpoDebugLog()
        {
            FieldInfo xpoSwitchF = typeof(ConnectionProviderSql).GetField("xpoSwitch", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            TraceSwitch xpoSwitch = (TraceSwitch)xpoSwitchF.GetValue(null);
            xpoSwitch.Level = TraceLevel.Info;
        }
        IObjectSpace objectSpace;
        [SetUp]
        public void Setup()
        {
            XpoTypesInfoHelper.GetXpoTypeInfoSource();
            XafTypesInfo.Instance.RegisterEntity(typeof(MainObject));
            XafTypesInfo.Instance.RegisterEntity(typeof(RefObject1));
            XafTypesInfo.Instance.RegisterEntity(typeof(RefObject2));
            XafTypesInfo.Instance.RegisterEntity(typeof(RefObject3));
            XafTypesInfo.Instance.RegisterEntity(typeof(RefObject4));
            XafTypesInfo.Instance.RegisterEntity(typeof(RefObject5));
            XafTypesInfo.Instance.RegisterEntity(typeof(SpreadSheet));
            XPObjectSpaceProvider osProvider = new XPObjectSpaceProvider(
            @"Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\mssqllocaldb;Initial Catalog=XafImportApiWithTest", null);
            objectSpace = osProvider.CreateObjectSpace();
          
        }

        [Test]
        public void Test1()
        {
            EnableXpoDebugLog();
            DateTime startTime = DateTime.Now;
            var spreedSheet= objectSpace.GetObjectsQuery<SpreadSheet>().FirstOrDefault(s=>s.Name == "Test").GetSpreadSheet();
            SpreadsheetService spreadsheetService= new SpreadsheetService();
            var RowDef=spreadsheetService.GetData(spreedSheet.Worksheets[0], objectSpace.TypesInfo, typeof(MainObject));
            RowDef.ObjectType=typeof(MainObject).FullName;
            ImportService importService = new ImportService();
            importService.Import(objectSpace, RowDef, ImportMode.Insert);
            objectSpace.CommitChanges();
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            Debug.WriteLine("Time to import 1000 records with 5 references objects per record:"+duration.TotalSeconds);
            Assert.Pass();
        }

        [Test]
        public void GetHeadersAndNestedTypes()
        {
            EnableXpoDebugLog();
            DateTime startTime = DateTime.Now;
            var spreedSheet = objectSpace.GetObjectsQuery<SpreadSheet>().FirstOrDefault(s => s.Name == "Test").GetSpreadSheet();
            SpreadsheetService spreadsheetService = new SpreadsheetService();
            var RowDef = spreadsheetService.GetData(spreedSheet.Worksheets[0], objectSpace.TypesInfo, typeof(MainObject));
            RowDef.ObjectType = typeof(MainObject).FullName;
            ImportService importService = new ImportService();
            importService.Import(objectSpace, RowDef, ImportMode.Insert);
            objectSpace.CommitChanges();
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            Debug.WriteLine("Time to import 1000 records with 5 references objects per record:" + duration.TotalSeconds);
            Assert.Pass();
        }
    }
}