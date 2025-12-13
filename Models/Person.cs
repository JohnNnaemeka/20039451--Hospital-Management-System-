
using System;

namespace HospitalManager.Models
{
    public class Person
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public string Address { get; set; }

        public Person()
        {
            // ID is NOT auto-generated here (SQLite will provide it)
            ID = string.Empty;
            Name = string.Empty;
            Address = string.Empty;
            BirthDate = DateTime.Now;
        }

        public Person(string id, string name, DateTime birthDate, string address)
        {
            ID = id;
            Name = name;
            BirthDate = birthDate;
            Address = address;
        }

        public virtual void ShowInfo()
        {
            Console.WriteLine($"ID: {ID}");
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Birth Date: {BirthDate:yyyy-MM-dd}");
            Console.WriteLine($"Address: {Address}");
        }
    }
}
