using Microsoft.EntityFrameworkCore;
using RXNT.API.Data;
using RXNT.API.Models;
using RXNT.API.Repositories;
using RXNT.API.Services;
using RXNT.API.Middleware;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Hangfire (using SQL Server storage)
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        QueuePollInterval = TimeSpan.FromSeconds(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        PrepareSchemaIfNecessary = true
    }));

builder.Services.AddHangfireServer();

// Bind BulkProcessing options
builder.Services.Configure<BulkProcessingOptions>(builder.Configuration.GetSection("BulkProcessing"));

// Add repositories
builder.Services.AddScoped<IRepository<Patient>, Repository<Patient>>();
builder.Services.AddScoped<IRepository<Doctor>, Repository<Doctor>>();
builder.Services.AddScoped<IRepository<Appointment>, Repository<Appointment>>();
builder.Services.AddScoped<IRepository<Invoice>, Repository<Invoice>>();

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// Add services
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IAppointmentBookingService, AppointmentBookingService>();
builder.Services.AddScoped<IBulkAppointmentService, BulkAppointmentService>();
builder.Services.AddSingleton<ICsvAppointmentParser, CsvAppointmentParser>();
builder.Services.AddTransient<BulkAppointmentProcessor>();
builder.Services.AddTransient<BulkCleanupService>();
builder.Services.AddSingleton<ICsvUnifiedParser, CsvUnifiedParser>();
builder.Services.AddScoped<IBulkUnifiedImportService, BulkUnifiedImportService>();
builder.Services.AddTransient<BulkUnifiedImportProcessor>();

// Add validation services
builder.Services.AddScoped<IPatientValidationService, PatientValidationService>();
builder.Services.AddScoped<IDoctorValidationService, DoctorValidationService>();
builder.Services.AddScoped<IAppointmentValidationService, AppointmentValidationService>();

var app = builder.Build();

// Ensure database exists and apply EF Core migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Hangfire dashboard (allow anonymous in Development)
var dashboardOptions = new DashboardOptions();
if (app.Environment.IsDevelopment())
{
    dashboardOptions.Authorization = new[] { new AllowAllDashboardAuthorizationFilter() };
}
app.MapHangfireDashboard("/hangfire", dashboardOptions);

app.UseAuthorization();

app.MapControllers();

// Recurring cleanup job (daily at 2 AM)
RecurringJob.AddOrUpdate<BulkCleanupService>(
    "bulk-cleanup",
    svc => svc.CleanupAsync(),
    Cron.Daily(2));

app.Run();

// Internal authorization filter to allow all (Development only)
internal class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}