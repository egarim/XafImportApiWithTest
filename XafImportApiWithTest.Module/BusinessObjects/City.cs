using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Linq;

namespace XafImportApiWithTest.Module.BusinessObjects
{
    [DefaultClassOptions]
    public class Location : BaseObject
    {
        public Location(Session session) : base(session)
        { }


        City city;
        string name;
        string code;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value);
        }

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }
        
        public City City
        {
            get => city;
            set => SetPropertyValue(nameof(City), ref city, value);
        }

    }
    [DefaultClassOptions]
    public class City : BaseObject
    {
        public City(Session session) : base(session)
        { }



        Country country;
        string code;
        string name;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value);
        }

        public Country Country
        {
            get => country;
            set => SetPropertyValue(nameof(Country), ref country, value);
        }
    }
}