namespace RXNT.API.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public PatientDto? Patient { get; set; }
        public DoctorDto? Doctor { get; set; }
    }
}
