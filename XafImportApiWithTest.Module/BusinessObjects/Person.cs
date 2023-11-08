using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Linq;

namespace XafImportApiWithTest.Module.BusinessObjects
{
    [DefaultClassOptions]
    public class Person : BaseObject
    {
        public Person(Session session) : base(session)
        { }

        string code;
        Country country;
        string lastName;
        string name;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string LastName
        {
            get => lastName;
            set => SetPropertyValue(nameof(LastName), ref lastName, value);
        }

        public Country Country
        {
            get => country;
            set => SetPropertyValue(nameof(Country), ref country, value);
        }
        
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value);
        }

    }
}