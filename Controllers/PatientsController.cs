using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ContosoHealthcare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(IConfiguration config, ILogger<PatientsController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetPatients()
        {
            // VULNERABILITY: No authentication required to access patient data
            // VULNERABILITY: Returns all patient data including SSN
            var connectionString = _config.GetConnectionString("DefaultConnection");
            
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            connection.Open();
            
            var command = new Microsoft.Data.SqlClient.SqlCommand(
                "SELECT Id, FirstName, LastName, SSN, DateOfBirth, Email, Phone, MedicalRecordNumber, InsuranceId FROM Patients", 
                connection
            );
            
            var patients = new List<object>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                patients.Add(new
                {
                    Id = reader["Id"],
                    FirstName = reader["FirstName"],
                    LastName = reader["LastName"],
                    SSN = reader["SSN"],  // VULNERABILITY: Exposing SSN in API response
                    DateOfBirth = reader["DateOfBirth"],
                    Email = reader["Email"],
                    Phone = reader["Phone"],
                    MedicalRecordNumber = reader["MedicalRecordNumber"],
                    InsuranceId = reader["InsuranceId"]
                });
            }
            
            // VULNERABILITY: Logging sensitive patient data
            _logger.LogDebug("Retrieved {Count} patients: {Data}", patients.Count, 
                System.Text.Json.JsonSerializer.Serialize(patients));
            
            return Ok(patients);
        }

        [HttpGet("{id}")]
        public IActionResult GetPatient(int id)
        {
            // VULNERABILITY: SQL Injection
            var connectionString = _config.GetConnectionString("DefaultConnection");
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            connection.Open();
            
            // VULNERABILITY: String interpolation in SQL query
            var command = new Microsoft.Data.SqlClient.SqlCommand(
                $"SELECT * FROM Patients WHERE Id = {id}", connection
            );
            
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return Ok(new
                {
                    Id = reader["Id"],
                    FirstName = reader["FirstName"],
                    LastName = reader["LastName"],
                    SSN = reader["SSN"],
                    DateOfBirth = reader["DateOfBirth"],
                    Diagnosis = reader["Diagnosis"],
                    Medications = reader["Medications"],
                    // VULNERABILITY: Exposing all PHI data
                    Notes = reader["Notes"]
                });
            }
            
            return NotFound();
        }

        [HttpPost]
        public IActionResult CreatePatient([FromBody] dynamic patient)
        {
            // VULNERABILITY: No input validation
            var connectionString = _config.GetConnectionString("DefaultConnection");
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            connection.Open();
            
            string firstName = patient.FirstName;
            string lastName = patient.LastName;
            string ssn = patient.SSN;
            
            // VULNERABILITY: Weak encryption for SSN storage
            var encryptedSsn = EncryptWithDES(ssn);
            
            // VULNERABILITY: SQL Injection
            var sql = $"INSERT INTO Patients (FirstName, LastName, SSN) VALUES ('{firstName}', '{lastName}', '{encryptedSsn}')";
            var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection);
            command.ExecuteNonQuery();
            
            // VULNERABILITY: Logging SSN in plaintext
            _logger.LogInformation("Created patient: {FirstName} {LastName}, SSN: {SSN}", firstName, lastName, ssn);
            
            return Ok(new { status = "created" });
        }

        // VULNERABILITY: Using DES encryption (deprecated, weak)
        private string EncryptWithDES(string plainText)
        {
            byte[] key = Encoding.UTF8.GetBytes("12345678"); // 8-byte key for DES
            byte[] iv = Encoding.UTF8.GetBytes("12345678");
            
            using var des = DES.Create();
            des.Key = key;
            des.IV = iv;
            
            using var encryptor = des.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            
            return Convert.ToBase64String(encryptedBytes);
        }

        [HttpGet("export")]
        public IActionResult ExportAllPatients()
        {
            // VULNERABILITY: No auth, no audit logging, exports all PHI
            var connectionString = _config.GetConnectionString("DefaultConnection");
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            connection.Open();
            
            var command = new Microsoft.Data.SqlClient.SqlCommand(
                "SELECT * FROM Patients", connection
            );
            
            var patients = new List<Dictionary<string, object>>();
            using var reader = command.ExecuteReader();
            
            for (int i = 0; i < reader.FieldCount; i++)
            {
                // Export everything including PHI
            }
            
            return Ok(patients);
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            // VULNERABILITY: Exposes connection strings and secrets
            return Ok(new
            {
                ConnectionString = _config.GetConnectionString("DefaultConnection"),
                JwtSecret = _config["Jwt:Secret"],
                BlobStorage = _config["AzureBlobStorage:ConnectionString"]
            });
        }
    }
}
