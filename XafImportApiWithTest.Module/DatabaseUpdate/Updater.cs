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

        if(this.ObjectSpace.GetObjectsCount(typeof(BusinessObjects.Country), null) == 0)
        {
            var Italy = this.ObjectSpace.CreateObject<BusinessObjects.Country>();
            Italy.Name = "Italy";
            Italy.Code = "C003";

            var Russia = this.ObjectSpace.CreateObject<BusinessObjects.Country>();
            Russia.Name = "Russia";
            Russia.Code = "C001";

            var Belarus = this.ObjectSpace.CreateObject<BusinessObjects.Country>();
            Belarus.Name = "Belarus";
            Belarus.Code = "C002";

            var Minsk = this.ObjectSpace.CreateObject<BusinessObjects.City>();
            Minsk.Name = "Minsk";
            Minsk.Code = "C001";
            Minsk.Country= Belarus;

            var Moscow = this.ObjectSpace.CreateObject<BusinessObjects.City>();
            Moscow.Name = "Moscow";
            Moscow.Code = "C002";
            Moscow.Country = Russia;

            var Rome = this.ObjectSpace.CreateObject<BusinessObjects.City>();
            Rome.Name = "Rome";
            Rome.Code = "C003";
            Rome.Country = Italy;

            var Pizzeria= this.ObjectSpace.CreateObject<BusinessObjects.Location>();
            Pizzeria.Name = "Pizzeria";
            Pizzeria.Code = "C001";
            Pizzeria.City = Rome;

            var Restaurant = this.ObjectSpace.CreateObject<BusinessObjects.Location>();
            Restaurant.Name = "Restaurant";
            Restaurant.Code = "C002";
            Restaurant.City = Moscow;

            var Bar = this.ObjectSpace.CreateObject<BusinessObjects.Location>();
            Bar.Name = "Bar";
            Bar.Code = "C003";
            Bar.City = Minsk;

            var Jose= this.ObjectSpace.CreateObject<BusinessObjects.Person>();
            Jose.Name = "Jose";
            Jose.LastName = "Ojeda";

            var Alina= this.ObjectSpace.CreateObject<BusinessObjects.Person>();
            Alina.Name = "Alina";
            Alina.LastName = "Lukashova";

            var Daniel = this.ObjectSpace.CreateObject<BusinessObjects.Person>();
            Daniel.Name = "Daniel";
            Daniel.LastName = "Correa";

            var Lidia= this.ObjectSpace.CreateObject<BusinessObjects.Person>();
            Lidia.Name = "Lidia";
            Lidia.LastName = "Norinsk";

            var Jaime= this.ObjectSpace.CreateObject<BusinessObjects.Person>();
            Jaime.Name = "Jaime";
            Jaime.LastName = "Macias";

            var Ceci= this.ObjectSpace.CreateObject<BusinessObjects.Person>();
            Ceci.Name = "Ceci";
            Ceci.LastName = "Flores";



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
