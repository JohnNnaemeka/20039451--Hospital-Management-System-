using System;

namespace HospitalManager.Models
{
    public class Doctor : Employee
    {
        public string Specialty { get; set; }
        public string? DepartmentID { get; set; }


        public Doctor() : base()
        {
            Specialty = string.Empty;
        }

        public Doctor(string id, string name, string specialty, double salary)
            : base(id, name, specialty, salary)
        {
            Specialty = specialty;
        }
        public override void ShowEmployeeInfo()
        {
            Console.WriteLine($"-------------------------------------");
            Console.WriteLine($"Doctor ID : {ID}");
            Console.WriteLine($"Name      : {Name}");
            Console.WriteLine($"Specialty : {Specialty}");
            Console.WriteLine($"Department: {DepartmentName}");
            Console.WriteLine($"Salary    : €{Salary:F2}");
            Console.WriteLine($"-------------------------------------");
            Console.WriteLine("Press Any Key To Return");
        }
    }
}

