using System;
using System.Collections.Generic;
using System.Linq;

namespace HospitalManager.Models
{
    public class Department
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string HeadID { get; set; }
        public List<Doctor> Doctors { get; set; }
        public List<Nurse> Nurses { get; set; }
        //public List<Patient> Patients { get; set; }
        public List<AppointmentPatient> Patients { get; set; } = new();

        public Department()
        {
            ID = Guid.NewGuid().ToString();
            Name = string.Empty;
            HeadID = string.Empty;
            Doctors = new List<Doctor>();
            Nurses = new List<Nurse>();
            Patients = new List<AppointmentPatient>();
        }

        public void AddDoctor(Doctor doctor)
        {
            if (Doctors.Any(d => d.ID == doctor.ID))
            {
                Console.WriteLine($" Doctor {doctor.Name} already exists.");
                return;
            }
            Doctors.Add(doctor);
            Console.WriteLine($" Doctor {doctor.Name} added to {Name}.");
        }

        public void AddPatient(AppointmentPatient patient)
        {
            if (Patients.Any(p => p.ID == patient.ID))
            {
                Console.WriteLine($" Patient {patient.Name} already exists.");
                return;
            }
            Patients.Add(patient);
            Console.WriteLine($" Patient {patient.Name} added.");
        }
    }
}
