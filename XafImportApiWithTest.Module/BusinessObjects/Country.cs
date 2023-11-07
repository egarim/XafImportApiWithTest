using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Linq;

namespace XafImportApiWithTest.Module.BusinessObjects
{
    [DefaultClassOptions]
    public class Country : BaseObject
    {
        public Country(Session session) : base(session)
        { }

        
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Code
        {
            get => code;
            set => SetPropertyValue(nameof(Code), ref code, value);
        }
        string code;
        string name;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }
    }
}