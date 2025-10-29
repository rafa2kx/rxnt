using Microsoft.AspNetCore.Mvc;
using RXNT.API.Data;
using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Services;

namespace RXNT.API.Controllers
{
    [Route("api/[controller]")]
    public class PatientsController : BaseController
    {
        private readonly IPatientService _patientService;

        public PatientsController(
            IPatientService patientService,
            ApplicationDbContext context,
            ILogger<BaseController> logger)
            : base(context, logger)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _patientService.GetAllPatientsAsync();
            return Ok(patients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(int id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
                return NotFound();

            return Ok(patient);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient(PatientDto patient)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var createdPatient = await _patientService.CreatePatientAsync(patient);
                return CreatedAtAction(nameof(GetPatient), new { id = createdPatient.Id }, createdPatient);
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, PatientDto patient)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var updatedPatient = await _patientService.UpdatePatientAsync(id, patient);
                if (updatedPatient == null)
                    throw new KeyNotFoundException($"Patient with ID {id} not found");

                return updatedPatient;
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var result = await _patientService.DeletePatientAsync(id);
                if (!result)
                    throw new KeyNotFoundException($"Patient with ID {id} not found");
            });
        }
    }
}
