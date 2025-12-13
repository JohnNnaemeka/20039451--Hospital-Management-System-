using System;

namespace HospitalManager.Models
{
    public class Inpatient : Patient
    {
        public string RoomID { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }
        public double DailyRate { get; set; }

        public Inpatient() : base()
        {
            AdmissionDate = DateTime.Now;
        }

        public Inpatient(
            string id,
            string name,
            DateTime birthDate,
            string address,
            string diagnosis,
            string roomId,
            double dailyRate)
            : base()  // I set all inherited fields manually
        {
            ID = id;
            Name = name;
            BirthDate = birthDate;
            Address = address;
            Diagnosis = diagnosis;
            RoomID = roomId;
            AdmissionDate = DateTime.Now;
            DailyRate = dailyRate;
        }
        public int GetDaysStayed()
        {
            // SAFETY: If AdmissionDate is uninitialized or invalid, force a safe fallback.
            if (AdmissionDate == default || AdmissionDate.Year < 2020)
                AdmissionDate = DateTime.Now.Date;

            // SAFETY: DischargeDate cannot be before AdmissionDate.
            if (DischargeDate.HasValue && DischargeDate.Value.Date < AdmissionDate.Date)
                DischargeDate = AdmissionDate.Date;

            // Determine final day of stay
            DateTime end = DischargeDate?.Date ?? DateTime.Now.Date;

            // SAFETY: Ensure end is NOT before admission
            if (end < AdmissionDate.Date)
                end = AdmissionDate.Date;

            // Count exact days (inclusive)
            int days = (end - AdmissionDate.Date).Days + 1;

            // SAFETY: Stay must be at least 1 day as m
            return Math.Max(days, 1);
        }
        public override double GetBill()
        {
            int days = GetDaysStayed();

            // SAFETY: Daily rate must be valid
            if (DailyRate < 0)
                DailyRate = 0;

            double total = days * DailyRate;

            // SAFETY: bill should never be negative
            return Math.Max(total, 0);
        }

        //public override double GetBill() => GetDaysStayed() * DailyRate;
        public void Discharge()
        {
            if (DischargeDate.HasValue)
                return; // Already discharged — no double processing

            // SAFETY: Discharge cannot be before admission
            DateTime now = DateTime.Now;
            DischargeDate = (now.Date < AdmissionDate.Date)
                ? AdmissionDate.Date
                : now;
        }

        public override void ShowInfo()
        {
            base.ShowInfo();
            Console.WriteLine($"Room: {RoomID}");
            Console.WriteLine($"Admission Date: {AdmissionDate:yyyy-MM-dd}");
            Console.WriteLine($"Discharge Date: {(DischargeDate.HasValue ? DischargeDate.Value.ToString("yyyy-MM-dd") : "Still Admitted")}");
            Console.WriteLine($"Daily Rate: €{DailyRate:F2}");
            Console.WriteLine($"Days Stayed: {GetDaysStayed()}");
            Console.WriteLine($"Stay Cost: €{GetBill():F2}");
        }
    }
}
