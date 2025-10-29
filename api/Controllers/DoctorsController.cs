using Microsoft.AspNetCore.Mvc;
using RXNT.API.Data;
using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Services;

namespace RXNT.API.Controllers
{
    [Route("api/[controller]")]
    public class DoctorsController : BaseController
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(
            IDoctorService doctorService,
            ApplicationDbContext context,
            ILogger<BaseController> logger)
            : base(context, logger)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _doctorService.GetAllDoctorsAsync();
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor(DoctorDto doctor)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var createdDoctor = await _doctorService.CreateDoctorAsync(doctor);
                return CreatedAtAction(nameof(GetDoctor), new { id = createdDoctor.Id }, createdDoctor);
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, DoctorDto doctor)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var updatedDoctor = await _doctorService.UpdateDoctorAsync(id, doctor);
                if (updatedDoctor == null)
                    throw new KeyNotFoundException($"Doctor with ID {id} not found");

                return updatedDoctor;
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var result = await _doctorService.DeleteDoctorAsync(id);
                if (!result)
                    throw new KeyNotFoundException($"Doctor with ID {id} not found");
            });
        }

        [HttpGet("{id}/schedule")]
        public async Task<IActionResult> GetDoctorSchedule(int id)
        {
            var schedule = await _doctorService.GetDoctorScheduleAsync(id);
            return Ok(schedule);
        }
    }
}
