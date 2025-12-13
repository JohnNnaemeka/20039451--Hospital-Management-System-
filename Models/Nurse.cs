using System;

namespace HospitalManager.Models
{
    public class Nurse : Employee
    {
        public int ShiftHours { get; set; }
        public string? DepartmentID { get; set; }


        public Nurse() : base()
        {
            ShiftHours = 0;
        }

        public Nurse(string id, string name, string dept, double salary, int hours)
            : base(id, name, dept, salary)
        {
            ShiftHours = hours;
        }

        public override void ShowEmployeeInfo()
        {
            Console.WriteLine("===========================");
            Console.WriteLine($"ID: {ID}");
            Console.WriteLine($"Nurse: {Name}");
            Console.WriteLine($"Department: {DepartmentName}");
            Console.WriteLine($"Shift Hours: {ShiftHours}");
            Console.WriteLine($"Salary: €{Salary:F2}");
            Console.WriteLine("===========================");
        }
    }
}
