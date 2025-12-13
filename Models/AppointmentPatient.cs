
using System;
using System.Collections.Generic;
using System.Linq;
using HospitalManager.DataAccess;

namespace HospitalManager.Models
{
    public class AppointmentPatient : Patient
    {
        public List<Appointment> Appointments { get; set; }
        //public string? DepartmentID { get; set; }


        public AppointmentPatient() : base()
        {
            Appointments = new List<Appointment>();
        }

        // Overloaded constructor used in HospitalService
        public AppointmentPatient(string name, DateTime birthDate, string address, string diagnosis)
       : base()
        {
            ID = SqliteDatabase.GeneratePatientID();  // Auto-generate ID
            Name = name;
            BirthDate = birthDate;
            Address = address;
            Diagnosis = diagnosis;
            Appointments = new List<Appointment>();
        }

        public void AddAppointment(Appointment appointment)
        {
            if (!Appointments.Any(a => a.ID == appointment.ID))
            {
                Appointments.Add(appointment);
                Console.WriteLine($" Appointment added successfully (ID: {appointment.ID})");
            }
            else
            {
                Console.WriteLine(" Appointment already exists!");
            }
        }

        public void RemoveAppointment(string id)
        {
            int removedCount = Appointments.RemoveAll(a =>
                a.ID.Equals(id, StringComparison.OrdinalIgnoreCase));

            Console.WriteLine(removedCount > 0
                ? $" Appointment with ID '{id}' removed successfully."
                : $" No appointment found with ID '{id}'.");
        }

        public double CalculateBill(BillingOptions options)
        {
            double baseTotal = Appointments.Sum(a => a.Fee);

            double discount = baseTotal * options.DiscountRate;
            double afterDiscount = baseTotal - discount;

            double insurance = afterDiscount * options.InsuranceCoverageRate;
            double afterInsurance = afterDiscount - insurance;

            double vat = afterInsurance * options.VAT;

            double finalTotal = afterInsurance + vat + options.ServiceFee;

            return Math.Round(finalTotal, 2);
        }

        public override double GetBill() => Appointments.Sum(a => a.Fee);

        public void ShowBillingDetails(BillingOptions options)
        {
            double baseAmount = Appointments.Sum(a => a.Fee);

            Console.WriteLine("========  Billing Breakdown ========");
            Console.WriteLine($"Base Appointment Fees: €{baseAmount:F2}");

            double discount = baseAmount * options.DiscountRate;
            Console.WriteLine($"Discount ({options.DiscountRate * 100}%): -€{discount:F2}");

            double afterDiscount = baseAmount - discount;

            double insurance = afterDiscount * options.InsuranceCoverageRate;
            Console.WriteLine($"Insurance Coverage ({options.InsuranceCoverageRate * 100}%): -€{insurance:F2}");

            double afterInsurance = afterDiscount - insurance;

            double vat = afterInsurance * options.VAT;
            Console.WriteLine($"VAT ({options.VAT * 100}%): +€{vat:F2}");

            Console.WriteLine($"Service Fee: +€{options.ServiceFee:F2}");

            double final = afterInsurance + vat + options.ServiceFee;
            Console.WriteLine("-------------------------------------");
            Console.WriteLine($"TOTAL BILL: €{final:F2}");
            Console.WriteLine("=====================================\n");
        }
        public void ShowAppointments()
        {
            Console.WriteLine("\n================== APPOINTMENT SUMMARY ==================");
            Console.WriteLine($"Patient Name : {Name}");
            Console.WriteLine($"Patient ID   : {ID}");
            Console.WriteLine("--------------------------------------------------------");

            if (Appointments == null || Appointments.Count == 0)
            {
                Console.WriteLine("No appointments found.");
                return;
            }

            foreach (var appt in Appointments)
            {
                // Load doctor info from SQLite
                var doc = HospitalManager.DataAccess.SqliteDatabase.GetDoctorByID(appt.DoctorID);

                Console.WriteLine($" Appointment ID: {appt.ID}");
                Console.WriteLine($"Doctor ID: {appt.DoctorID}");

                if (doc != null)
                {
                    Console.WriteLine($"Doctor Name: {doc.Name}");
                    Console.WriteLine($"Specialty  : {doc.Specialty}");
                }
                else
                {
                    Console.WriteLine("Doctor Name: (not found)");
                }

                Console.WriteLine($"Reason     : {appt.Reason}");
                Console.WriteLine($"Fee        : €{appt.Fee:F2}");
                Console.WriteLine($"Status     : {appt.Status}");
                Console.WriteLine($"Date       : {appt.Date:yyyy-MM-dd HH:mm}");
                Console.WriteLine("--------------------------------------------------------");
            }

            Console.WriteLine($" TOTAL BILL: €{GetBill():F2}");
            Console.WriteLine("=========================================================\n");
        }
    }
}

