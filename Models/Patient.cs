using System;

namespace HospitalManager.Models
{
    public class Patient : Person
    {
        public string Diagnosis { get; set; }
        public string? DepartmentID { get; set; }


        public Patient() : base()
        {
            Diagnosis = string.Empty;
        }

        public Patient(string id, string name, DateTime birthDate, string address, string diagnosis)
            : base(id, name, birthDate, address)
        {
            Diagnosis = diagnosis;
        }

        public override void ShowInfo()
        {
            base.ShowInfo();
            Console.WriteLine($"Diagnosis: {Diagnosis}");
        }

        // Overrideable billing total
        public virtual double GetBill() => 0;
    }
}
