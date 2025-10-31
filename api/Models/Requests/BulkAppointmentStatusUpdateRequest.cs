namespace RXNT.API.Models.Requests
{
    public class BulkAppointmentStatusUpdateRequest
    {
        public string Status { get; set; } = string.Empty;
        public List<int> Ids { get; set; } = new List<int>();
    }
}


