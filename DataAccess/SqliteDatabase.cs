using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using HospitalManager.Models;

namespace HospitalManager.DataAccess
{
    public static class SqliteDatabase
    {
        private static readonly string DbFile = "hospital.sqlite";
        private static string ConnectionString => $"Data Source={DbFile}";

        // ===========================================================
        // INITIALIZE
        // ===========================================================
        public static void InitializeDatabase()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Sequences (
                    Name TEXT PRIMARY KEY,
                    CurrentValue INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Patients (
                    ID TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    BirthDate TEXT NOT NULL,
                    Address TEXT NOT NULL,
                    Diagnosis TEXT,
                    DepartmentID TEXT
                );
                CREATE TABLE IF NOT EXISTS Departments (
                    ID TEXT PRIMARY KEY,
                    Name TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Doctors (
                    ID TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    DepartmentName TEXT,
                    DepartmentID TEXT,
                    Salary REAL,
                    Specialty TEXT
                );


                CREATE TABLE IF NOT EXISTS Nurses (
                    ID TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    DepartmentName TEXT,
                    DepartmentID TEXT,
                    Salary REAL,
                    ShiftHours INTEGER
                );

                CREATE TABLE IF NOT EXISTS Appointments (
                    ID TEXT PRIMARY KEY,
                    DoctorID TEXT NOT NULL,
                    PatientID TEXT NOT NULL,
                    Reason TEXT,
                    Fee REAL,
                    Date TEXT,
                    Status TEXT
                );

                CREATE TABLE IF NOT EXISTS Rooms (
                    RoomID TEXT PRIMARY KEY,
                    Type TEXT,
                    Capacity INTEGER,
                    DailyRate REAL
                );

                CREATE TABLE IF NOT EXISTS Inpatients (
                    PatientID TEXT PRIMARY KEY,
                    RoomID TEXT NOT NULL,
                    AdmissionDate TEXT NOT NULL,
                    DischargeDate TEXT NULL,
                    DailyRate REAL NOT NULL
                );
                ";
                cmd.ExecuteNonQuery();
            }

            EnsureDefaultRooms();
        }

        // ===========================================================
        // SEQUENCES (P001 / D001 / A001 / R001)
        // ===========================================================
        private static string GenerateNextID(string sequenceKey, string prefix)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT OR IGNORE INTO Sequences (Name, CurrentValue) VALUES ($n, 0)";
                cmd.Parameters.AddWithValue("$n", sequenceKey);
                cmd.ExecuteNonQuery();
            }

            int current;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT CurrentValue FROM Sequences WHERE Name = $n";
                cmd.Parameters.AddWithValue("$n", sequenceKey);
                current = Convert.ToInt32(cmd.ExecuteScalar());
            }

            current++;

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Sequences SET CurrentValue = $v WHERE Name = $n";
                cmd.Parameters.AddWithValue("$v", current);
                cmd.Parameters.AddWithValue("$n", sequenceKey);
                cmd.ExecuteNonQuery();
            }

            return $"{prefix}{current:D3}";
        }

        public static string GeneratePatientID() => GenerateNextID("PATIENT", "P");
        public static string GenerateDoctorID() => GenerateNextID("DOCTOR", "D");
        public static string GenerateAppointmentID() => GenerateNextID("APPOINTMENT", "A");
        public static string GenerateRoomID() => GenerateNextID("ROOM", "R");
        public static string GenerateNurseID()
        {
            return GenerateNextID("NURSE", "N");
        }


        // ===========================================================
        // PATIENTS (AppointmentPatient)
        // ===========================================================
        public static void SavePatient(Patient p)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT OR REPLACE INTO Patients (ID, Name, BirthDate, Address, Diagnosis, DepartmentID)
                VALUES ($id, $name, $birth, $addr, $diag, $deptId)
            ";

            cmd.Parameters.AddWithValue("$id", p.ID);
            cmd.Parameters.AddWithValue("$name", p.Name);
            cmd.Parameters.AddWithValue("$birth", p.BirthDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$addr", p.Address);
            cmd.Parameters.AddWithValue("$diag", p.Diagnosis ?? "");
            cmd.Parameters.AddWithValue("$deptId", p.DepartmentID ?? "");

            cmd.ExecuteNonQuery();
        }

        public static void AddOrUpdatePatient(Patient p) => SavePatient(p);
        //======================================================
        //UPDATE PATIENT
        //======================================================
        public static void UpdatePatient(Patient p)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =

            @"UPDATE Patients SET 
                Name=$name, 
                BirthDate=$birth, 
                Address=$addr, 
                Diagnosis=$diag,
                DepartmentID=$deptId
            WHERE ID=$id
            ";
            cmd.Parameters.AddWithValue("$id", p.ID);
            cmd.Parameters.AddWithValue("$name", p.Name);
            cmd.Parameters.AddWithValue("$birth", p.BirthDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$addr", p.Address);
            cmd.Parameters.AddWithValue("$diag", p.Diagnosis);
            cmd.Parameters.AddWithValue("$deptId", p.DepartmentID ?? "");

            cmd.ExecuteNonQuery();
        }
        //=========================================================
        //DELETE PATIENT
        //========================================================
        public static void DeletePatient(string id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Patients WHERE ID=$id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
        //===============================================================
        //ALL PATIENTS WITH APPOINTMENTS
        //==============================================================
        public static List<AppointmentPatient> GetAllPatients()
        {
            List<AppointmentPatient> list = new();

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ID, Name, BirthDate, Address, Diagnosis, DepartmentID FROM Patients";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var p = new AppointmentPatient
                {
                    ID = reader.GetString(0),
                    Name = reader.GetString(1),
                    BirthDate = DateTime.Parse(reader.GetString(2)),
                    Address = reader.GetString(3),
                    Diagnosis = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    DepartmentID = reader.IsDBNull(5) ? "" : reader.GetString(5)

                };

                p.Appointments = LoadAppointmentsForPatient(p.ID);

                list.Add(p);
            }

            return list;
        }

        // ===========================================================
        // DOCTORS
        // ===========================================================
        public static void SaveDoctor(Doctor d)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT OR REPLACE INTO Doctors (ID, Name, DepartmentName, Salary, Specialty, DepartmentID)
                VALUES ($id, $name, $dept, $sal, $spec, $deptId)
            ";

            cmd.Parameters.AddWithValue("$id", d.ID);
            cmd.Parameters.AddWithValue("$name", d.Name);
            cmd.Parameters.AddWithValue("$dept", d.DepartmentName ?? "");
            cmd.Parameters.AddWithValue("$sal", d.Salary);
            cmd.Parameters.AddWithValue("$spec", d.Specialty ?? "");
            cmd.Parameters.AddWithValue("$deptId", d.DepartmentID ?? "");////

            cmd.ExecuteNonQuery();
        }
        //============================================================
        //LIST ALL DOCTORS
        //===========================================================
        public static List<Doctor> GetAllDoctorsWithIDs()
        {
            List<Doctor> list = new();

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ID, Name, DepartmentName, Salary, Specialty, DepartmentID FROM Doctors";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Doctor
                {
                    ID = reader.GetString(0),
                    Name = reader.GetString(1),
                    DepartmentName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Salary = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                    Specialty = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    DepartmentID = reader.IsDBNull(5) ? "" : reader.GetString(5)

                });
            }

            return list;
        }
        //===============================================================
        //UPDATE DOCTOR
        //=============================================================
        public static void UpdateDoctor(Doctor d)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText =
            @"UPDATE Doctors SET 
            Name=$name, DepartmentName=$dept, DepartmentID=$deptId, Salary=$salary, Specialty=$spec
            WHERE ID=$id";

            cmd.Parameters.AddWithValue("$id", d.ID);
            cmd.Parameters.AddWithValue("$name", d.Name);
            cmd.Parameters.AddWithValue("$dept", d.DepartmentName);
            cmd.Parameters.AddWithValue("$salary", d.Salary);
            cmd.Parameters.AddWithValue("$spec", d.Specialty);
            cmd.Parameters.AddWithValue("$deptId", d.DepartmentID ?? "");

            cmd.ExecuteNonQuery();
        }
        //==========================================================
        //DELETE DOCTOR
        //=========================================================
        public static void DeleteDoctor(string id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Doctors WHERE ID=$id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
        //===============================================================
        //SEARCH DOCTOR BY ID
        //==============================================================
        public static Doctor? GetDoctorByID(string id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ID, Name, DepartmentName, Salary, Specialty FROM Doctors WHERE ID = $id";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Doctor
                {
                    ID = reader.GetString(0),
                    Name = reader.GetString(1),
                    DepartmentName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Salary = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                    Specialty = reader.IsDBNull(4) ? "" : reader.GetString(4)
                };
            }

            return null;
        }
        // ==========================================================
        // NURSES
        // ==========================================================
        public static void SaveNurse(Nurse n)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            INSERT OR REPLACE INTO Nurses (ID, Name, DepartmentName, DepartmentID, Salary, ShiftHours)
            VALUES ($id, $name, $dept, $deptId, $salary, $hours);
            ";

            cmd.Parameters.AddWithValue("$id", n.ID);
            cmd.Parameters.AddWithValue("$name", n.Name);
            cmd.Parameters.AddWithValue("$dept", n.DepartmentName ?? "");
            cmd.Parameters.AddWithValue("$salary", n.Salary);
            cmd.Parameters.AddWithValue("$hours", n.ShiftHours);
            cmd.Parameters.AddWithValue("$deptId", n.DepartmentID ?? "");

            cmd.ExecuteNonQuery();
        }

        public static void UpdateNurse(Nurse n)
        {
            SaveNurse(n);
        }

        public static void DeleteNurse(string id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Nurses WHERE ID = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public static List<Nurse> GetAllNursesWithIDs()
        {
            List<Nurse> list = new();

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ID, Name, DepartmentName, Salary, ShiftHours, DepartmentID FROM Nurses";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var n = new Nurse
                {
                    ID = reader.GetString(0),
                    Name = reader.GetString(1),
                    DepartmentName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Salary = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                    ShiftHours = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    DepartmentID = reader.IsDBNull(5) ? "" : reader.GetString(5)

                };

                list.Add(n);
            }

            return list;
        }

        // ===========================================================
        // APPOINTMENTS
        // ===========================================================
        public static void SaveAppointment(Appointment a)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT OR REPLACE INTO Appointments
                    (ID, DoctorID, PatientID, Reason, Fee, Date, Status)
                VALUES ($id, $doc, $pat, $reason, $fee, $date, $status)
            ";

            cmd.Parameters.AddWithValue("$id", a.ID);
            cmd.Parameters.AddWithValue("$doc", a.DoctorID);
            cmd.Parameters.AddWithValue("$pat", a.PatientID);
            cmd.Parameters.AddWithValue("$reason", a.Reason ?? "");
            cmd.Parameters.AddWithValue("$fee", a.Fee);
            cmd.Parameters.AddWithValue("$date", a.Date.ToString("yyyy-MM-dd HH:mm"));
            cmd.Parameters.AddWithValue("$status", a.Status ?? "Scheduled");

            cmd.ExecuteNonQuery();
        }

        public static void AddAppointment(Appointment a) => SaveAppointment(a);

        public static List<Appointment> LoadAppointmentsForPatient(string patientId)
        {
            List<Appointment> list = new();

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT ID, DoctorID, PatientID, Reason, Fee, Date, Status
                FROM Appointments
                WHERE PatientID = $pid
            ";
            cmd.Parameters.AddWithValue("$pid", patientId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Appointment
                {
                    ID = reader.GetString(0),
                    DoctorID = reader.GetString(1),
                    PatientID = reader.GetString(2),
                    Reason = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Fee = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                    Date = DateTime.Parse(reader.GetString(5)),
                    Status = reader.IsDBNull(6) ? "Scheduled" : reader.GetString(6)
                });
            }

            return list;
        }
        //------------------------------------------//
        //VIEW ALL APPOINTMENTS
        //-----------------------------------------//
        public static List<dynamic> GetAllAppointmentsWithPatientNamesAndDoctorNames()
        {
            List<dynamic> result = new();

            using var conn = new SqliteConnection($"Data Source={DbFile}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT 
            a.ID,
            a.DoctorID,
            d.Name AS DoctorName,
            a.PatientID,
            p.Name AS PatientName,
            a.Reason,
            a.Fee,
            a.Date,
            a.Status
        FROM Appointments a
        LEFT JOIN Doctors d ON a.DoctorID = d.ID
        LEFT JOIN Patients p ON a.PatientID = p.ID
        ORDER BY a.Date DESC;
        ";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new
                {
                    ID = reader.GetString(0),
                    DoctorID = reader.GetString(1),
                    DoctorName = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),
                    PatientID = reader.GetString(3),
                    PatientName = reader.IsDBNull(4) ? "Unknown" : reader.GetString(4),
                    Reason = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Fee = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                    Date = reader.GetString(7),
                    Status = reader.IsDBNull(8) ? "Scheduled" : reader.GetString(8)
                });
            }

            return result;
        }


        public static void DeleteAppointment(string id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Appointments WHERE ID = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // ===========================================================
        // ROOMS
        // ===========================================================
        public static void SaveRoom(Room r)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT OR REPLACE INTO Rooms (RoomID, Type, Capacity, DailyRate)
                VALUES ($id, $type, $cap, $rate)
            ";

            cmd.Parameters.AddWithValue("$id", r.RoomID);
            cmd.Parameters.AddWithValue("$type", r.Type);
            cmd.Parameters.AddWithValue("$cap", r.Capacity);
            cmd.Parameters.AddWithValue("$rate", r.DailyRate);

            cmd.ExecuteNonQuery();
        }

        public static List<Room> GetAllRooms()
        {
            List<Room> list = new();

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT RoomID, Type, Capacity, DailyRate FROM Rooms";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Room
                {
                    RoomID = reader.GetString(0),
                    Type = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Capacity = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    DailyRate = reader.IsDBNull(3) ? 0 : reader.GetDouble(3)
                });
            }

            return list;
        }
        //---------------------------------------//
        //ROOM CAPACITY
        //---------------------------------------//
        public static void EnsureDefaultRooms()
        {
            UpsertRoom("R001", "General", 10, 150);
            UpsertRoom("R002", "Private", 10, 300);
            UpsertRoom("R003", "ICU", 6, 800);
        }

        private static void UpsertRoom(string id, string type, int cap, double rate)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        INSERT INTO Rooms (RoomID, Type, Capacity, DailyRate)
        VALUES ($id, $type, $cap, $rate)
        ON CONFLICT(RoomID) DO UPDATE SET
            Type = excluded.Type,
            Capacity = excluded.Capacity,
            DailyRate = excluded.DailyRate;
    ";

            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$type", type);
            cmd.Parameters.AddWithValue("$cap", cap);
            cmd.Parameters.AddWithValue("$rate", rate);

            cmd.ExecuteNonQuery();
        }

        // ===========================================================
        // INPATIENTS
        // ===========================================================
        public static void AdmitInpatient(Inpatient inp)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT OR REPLACE INTO Inpatients
                    (PatientID, RoomID, AdmissionDate, DischargeDate, DailyRate)
                VALUES ($pid, $room, $admit, $discharge, $rate)
            ";

            cmd.Parameters.AddWithValue("$pid", inp.ID);
            cmd.Parameters.AddWithValue("$room", inp.RoomID);
            cmd.Parameters.AddWithValue("$admit", inp.AdmissionDate.ToString("yyyy-MM-dd HH:mm"));

            // IMPORTANT: use DBNull.Value if DischargeDate is null
            if (inp.DischargeDate == null)
                cmd.Parameters.AddWithValue("$discharge", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("$discharge", inp.DischargeDate.Value.ToString("yyyy-MM-dd HH:mm"));

            cmd.Parameters.AddWithValue("$rate", inp.DailyRate);

            cmd.ExecuteNonQuery();
        }

        public static void DischargeInpatient(string patientId, DateTime dischargeDate)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Inpatients
                SET DischargeDate = $date
                WHERE PatientID = $pid AND DischargeDate IS NULL
            ";
            cmd.Parameters.AddWithValue("$date", dischargeDate.ToString("yyyy-MM-dd HH:mm"));
            cmd.Parameters.AddWithValue("$pid", patientId);
            cmd.ExecuteNonQuery();
        }

        public static List<Inpatient> GetActiveInpatients()
        {
            List<Inpatient> list = new();

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT  p.ID, p.Name, p.BirthDate, p.Address, p.Diagnosis,
                        i.RoomID, i.AdmissionDate, i.DailyRate
                FROM Inpatients i
                JOIN Patients p ON p.ID = i.PatientID
                WHERE i.DischargeDate IS NULL
            ";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var inp = new Inpatient(
                    id: reader.GetString(0),
                    name: reader.GetString(1),
                    birthDate: DateTime.Parse(reader.GetString(2)),
                    address: reader.GetString(3),
                    diagnosis: reader.IsDBNull(4) ? "" : reader.GetString(4),
                    roomId: reader.GetString(5),
                    dailyRate: reader.GetDouble(7)
                )
                {
                    AdmissionDate = DateTime.Parse(reader.GetString(6)),
                    DischargeDate = null
                };

                list.Add(inp);
            }

            return list;
        }

        public static Inpatient? GetInpatientByPatientId(string patientId)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT  p.ID, p.Name, p.BirthDate, p.Address, p.Diagnosis,
                        i.RoomID, i.AdmissionDate, i.DischargeDate, i.DailyRate
                FROM Inpatients i
                JOIN Patients p ON p.ID = i.PatientID
                WHERE i.PatientID = $pid
            ";
            cmd.Parameters.AddWithValue("$pid", patientId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var inp = new Inpatient(
                    id: reader.GetString(0),
                    name: reader.GetString(1),
                    birthDate: DateTime.Parse(reader.GetString(2)),
                    address: reader.GetString(3),
                    diagnosis: reader.IsDBNull(4) ? "" : reader.GetString(4),
                    roomId: reader.GetString(5),
                    dailyRate: reader.GetDouble(8)
                )
                {
                    AdmissionDate = DateTime.Parse(reader.GetString(6)),
                    DischargeDate = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7))
                };

                return inp;
            }

            return null;
        }

        public static List<Department> GetAllDepartments()
        {
            List<Department> list = new();

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ID, Name FROM Departments";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Department
                {
                    ID = reader.GetString(0),
                    Name = reader.GetString(1),
                    Doctors = new List<Doctor>(),
                    Nurses = new List<Nurse>(),
                    //public List<AppointmentPatient> Patients { get; set; }

                    Patients = new List<AppointmentPatient>()
                });
            }

            return list;
        }
        public static string GenerateDepartmentID()
        {
            return GenerateNextID("DEPARTMENT", "DEP");
        }
        public static void AddDepartment(Department dept)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        INSERT OR REPLACE INTO Departments (ID, Name)
        VALUES ($id, $name)
        ";

            cmd.Parameters.AddWithValue("$id", dept.ID);
            cmd.Parameters.AddWithValue("$name", dept.Name);

            cmd.ExecuteNonQuery();
        }

    }
}