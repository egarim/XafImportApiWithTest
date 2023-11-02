using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using XafImportApiWithTest.Module.BusinessObjects;

namespace XafImportApiWithTest.Module.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();
        //string name = "MyName";
        //DomainObject1 theObject = ObjectSpace.FirstOrDefault<DomainObject1>(u => u.Name == name);
        //if(theObject == null) {
        //    theObject = ObjectSpace.CreateObject<DomainObject1>();
        //    theObject.Name = name;
        //}

        if (this.ObjectSpace.GetObjectsCount(typeof(RefObject1), null) == 0)
        {
            for (int i = 0; i < 30000; i++)
            {

                CreateRefObject(typeof(RefObject1), i.ToString());
                CreateRefObject(typeof(RefObject2), i.ToString());
                CreateRefObject(typeof(RefObject3), i.ToString());
                CreateRefObject(typeof(RefObject4), i.ToString());
                CreateRefObject(typeof(RefObject5), i.ToString());
            }
        }

        ObjectSpace.CommitChanges(); //Uncomment this line to persist created object(s).
    }
    void CreateRefObject(Type type, string Code)
    {
        var Instace = this.ObjectSpace.CreateObject(type) as BaseObject;
        Instace.SetMemberValue("Code", Code);
    }
    public override void UpdateDatabaseBeforeUpdateSchema() {
        base.UpdateDatabaseBeforeUpdateSchema();
        //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
        //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
        //}
    }
}
