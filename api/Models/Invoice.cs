namespace RXNT.API.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled
        public string? PaymentMethod { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        
        // Navigation property
        public Appointment Appointment { get; set; } = null!;
    }
}
