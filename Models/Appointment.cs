using System;

namespace HospitalManager.Models
{
    public class Appointment
    {
        public string ID { get; set; }
        public string DoctorID { get; set; } = string.Empty;
        public string PatientID { get; set; } = string.Empty;
        public string Reason { get; set; }
        public double Fee { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }

        public Appointment()
        {
            ID = Guid.NewGuid().ToString();
            Reason = string.Empty;
            Fee = 0;
            Status = "Scheduled";
            Date = DateTime.Now;
        }
    }
}
