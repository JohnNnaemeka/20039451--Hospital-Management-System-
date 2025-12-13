namespace HospitalManager.Models
{
    public class BillingOptions
    {
        public double InsuranceCoverageRate { get; set; } = 0; // e.g., 0.20 = 20%
        public double DiscountRate { get; set; } = 0;          // e.g., 0.10 = 10%
        public double VAT { get; set; } = 0.075;               // 7.5% VAT
        public double ServiceFee { get; set; } = 0;             // flat fee added
    }
}
