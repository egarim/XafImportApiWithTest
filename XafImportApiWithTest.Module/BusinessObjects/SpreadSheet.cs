using DevExpress.Spreadsheet;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Spreadsheet;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;

namespace XafImportApiWithTest.Module.BusinessObjects
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (https://documentation.devexpress.com/#eXpressAppFramework/CustomDocument112701).
    public class SpreadSheet : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public SpreadSheet(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }
        
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }
        string typeFullName;
        string name;
        private byte[] data;
        [EditorAlias("DevExpress.ExpressApp.Office.Win.SpreadsheetPropertyEditor")]
        public byte[] Data
        {
            get { return data; }
            set { SetPropertyValue(nameof(Data), ref data, value); }
        }
        
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string TypeFullName
        {
            get => typeFullName;
            set => SetPropertyValue(nameof(TypeFullName), ref typeFullName, value);
        }

        public Workbook GetSpreadSheet()
        {
            Workbook workbook = new Workbook();

            workbook.LoadDocument(new MemoryStream(this.Data), DocumentFormat.Xlsx);
            return workbook;
        }
    }
}