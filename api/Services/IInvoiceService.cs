using RXNT.API.Models;
using RXNT.API.DTOs;

namespace RXNT.API.Services
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync();
        Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
        Task<InvoiceDto> CreateInvoiceAsync(InvoiceDto invoice);
        Task<InvoiceDto?> UpdateInvoiceAsync(int id, InvoiceDto invoice);
        Task<bool> MarkInvoiceAsPaidAsync(int id, string paymentMethod);
        Task<(bool IsValid, string ErrorMessage)> ValidateInvoiceAsync(Invoice invoice);
    }
}
