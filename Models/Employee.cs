using System;

namespace HospitalManager.Models
{
    public abstract class Employee : Person
    {
        public string DepartmentName { get; set; }
        public double Salary { get; set; }

        public Employee() : base()
        {
            DepartmentName = string.Empty;
            Salary = 0;
        }

        public Employee(string id, string name, string dept, double salary)
        {
            ID = id;
            Name = name;
            DepartmentName = dept;
            Salary = salary;
        }

        public abstract void ShowEmployeeInfo();
    }
}