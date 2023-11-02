using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XafImportApiWithTest.Module.BusinessObjects
{
    public class Customer
    {
        
        public Customer()
        {
            
        }
        public Address Address { get; set; }
        public Person Contact { get; set; }
    }
    public class Address
    {
        
        public Address()
        {
            
        }
        public Street Street { get; set; }
    }
    public class Street
    {
        public string Name { get; set; }
        public Street()
        {
            
        }
    }
    public class Person
    {
        public Address Address { get; set; }
        public Person()
        {
            
        }
    }
    //how can I get all the properties and types if what i have is the property path? something link Customer.Person.Address.Street.Name

}
