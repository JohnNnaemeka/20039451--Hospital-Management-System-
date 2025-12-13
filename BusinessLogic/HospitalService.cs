using System;
using System.Collections.Generic;
using System.Linq;
using HospitalManager.Models;
using HospitalManager.DataAccess;

namespace HospitalManager.BusinessLogic
{
    public class HospitalService
    {
        private const string HospitalName = "ZenithMedix Specialist Hospital";

        public List<AppointmentPatient> Patients { get; private set; }
        public List<Doctor> Doctors { get; private set; }
        public List<Room> Rooms { get; private set; }
        public List<Inpatient> Inpatients { get; private set; }
        public List<Nurse> Nurses { get; private set; }
        public List<Department> Departments { get; private set; } = new();
        //======================================================//
        private void LoadDepartments()
        {
            Departments = SqliteDatabase.GetAllDepartments();

            // Link doctors, nurses, patients based on DepartmentID
            foreach (var dept in Departments)
            {
                dept.Doctors = Doctors.Where(d => d.DepartmentID == dept.ID).ToList();
                dept.Nurses = Nurses.Where(n => n.DepartmentID == dept.ID).ToList();
                dept.Patients = Patients.Where(p => p.DepartmentID == dept.ID).ToList();
            }
        }
        //=======================================================//
        // ============================================================
        // CONSTRUCTOR
        // ============================================================
        public HospitalService()
        {
            // Load all data
            Patients = SqliteDatabase.GetAllPatients();
            Doctors = SqliteDatabase.GetAllDoctorsWithIDs();
            Nurses = SqliteDatabase.GetAllNursesWithIDs();
            SqliteDatabase.EnsureDefaultRooms();
            Rooms = SqliteDatabase.GetAllRooms();
            Inpatients = SqliteDatabase.GetActiveInpatients();
            LoadDepartments();

            // Seed doctors if none exist
            if (!Doctors.Any())
            {
                var d1 = new Doctor(SqliteDatabase.GenerateDoctorID(), "Dr. James Okon", "Cardiology", 300000);
                var d2 = new Doctor(SqliteDatabase.GenerateDoctorID(), "Dr. Adaobi Nwosu", "Neurology", 280000);

                SqliteDatabase.SaveDoctor(d1);
                SqliteDatabase.SaveDoctor(d2);

                Doctors = SqliteDatabase.GetAllDoctorsWithIDs();
            }
            // Seed some nurses if table is empty
            if (!Nurses.Any())
            {
                var n1 = new Nurse(SqliteDatabase.GenerateNurseID(), "Nurse Jane Doe", "General Ward", 1500, 8);
                var n2 = new Nurse(SqliteDatabase.GenerateNurseID(), "Nurse John Smith", "ICU", 1800, 12);

                SqliteDatabase.SaveNurse(n1);
                SqliteDatabase.SaveNurse(n2);

                Nurses = SqliteDatabase.GetAllNursesWithIDs();
            }

        }
        //=================LOAD DEPT RELATIONSHIPS========================
        private void LoadDepartmentRelations()
        {
            Departments = SqliteDatabase.GetAllDepartments();
            Doctors = SqliteDatabase.GetAllDoctorsWithIDs();
            Nurses = SqliteDatabase.GetAllNursesWithIDs();
            Patients = SqliteDatabase.GetAllPatients();

            foreach (var dept in Departments)
            {
                dept.Doctors = Doctors.Where(d => d.DepartmentID == dept.ID).ToList();
                dept.Nurses = Nurses.Where(n => n.DepartmentID == dept.ID).ToList();
                dept.Patients = Patients.Where(p => p.DepartmentID == dept.ID).ToList();
            }
        }


        // ============================================================
        // MAIN LOOP
        // ============================================================
        public void Start()
        {
            bool running = true;

            LoadDepartmentRelations();
            while (running)
            {
                Console.Clear();
                Console.WriteLine($" {HospitalName}");
                Console.WriteLine("=======================================");
                Console.WriteLine("1. Patient Management");
                Console.WriteLine("2. Doctor Management");
                Console.WriteLine("3. Nurse Management");
                Console.WriteLine("4. Appointment Management");
                Console.WriteLine("5. Inpatient & Room Management");
                Console.WriteLine("6. Search");
                Console.WriteLine("7. Sorting");
                Console.WriteLine("8. Billing");
                Console.WriteLine("9. Dashboard (Who is in hospital)");
                Console.WriteLine("10. Department Management");
                Console.WriteLine("0. Exit");
                Console.WriteLine("=======================================");
                Console.Write("Select: ");

                switch (Console.ReadLine())
                {
                    case "1": PatientMenu(); break;
                    case "2": DoctorMenu(); break;
                    case "3": NurseMenu(); break;
                    case "4": AppointmentMenu(); break;
                    case "5": InpatientRoomMenu(); break;
                    case "6": SearchMenu(); break;
                    case "7": SortMenu(); break;
                    case "8": BillingMenu(); break;
                    case "9": ShowDashboard(); break;
                    case "10": DepartmentMenu(); break;//=========================
                    case "0": running = false; break;
                }
            }
        }

        // ============================================================
        // DOCTOR MANAGEMENT
        // ============================================================
        private void DoctorMenu()
        {
            bool submenu = true;

            while (submenu)
            {
                Console.Clear();
                Console.WriteLine(" DOCTOR MANAGEMENT");
                Console.WriteLine("----------------------------");
                Console.WriteLine("1. View All Doctors");
                Console.WriteLine("2. Add Doctor");
                Console.WriteLine("3. Update Doctor");
                Console.WriteLine("4. Delete Doctor");
                Console.WriteLine("0. Back");
                Console.Write("Select: ");

                switch (Console.ReadLine())
                {
                    case "1": ShowAllDoctors(); break;
                    case "2": AddDoctor(); break;
                    case "3": UpdateDoctor(); break;
                    case "4": DeleteDoctor(); break;
                    case "0": submenu = false; break;
                }
            }
        }
        //===============================================
        //DISPLAY ALL DOCTORS
        //===============================================
        private void ShowAllDoctors()
        {
            Console.Clear();
            Doctors = SqliteDatabase.GetAllDoctorsWithIDs();

            foreach (var d in Doctors)
                d.ShowEmployeeInfo();

            Console.ReadKey();
        }
        //=================================================
        //ADD DOCTOR
        //=================================================
        private void AddDoctor()
        {
            Console.Clear();
            Console.WriteLine("\n REGISTER DOCTOR");
            Console.WriteLine("----------------------------");

            // ---------------------------
            // NAME (Required, string-only)
            // ---------------------------
            Console.Write("Name: ");
            string name = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name is required. Please enter a valid name.");
                Console.ReadKey();
                return;
            }

            // Validate no digits in name
            if (name.Any(char.IsDigit))
            {
                Console.WriteLine("Name cannot contain numbers.");
                Console.ReadKey();
                return;
            }
            // ---------------------------
            // SPECIALTY (Required, string-only)
            // ---------------------------
            Console.Write("Specialty: ");
            string spec = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(spec))
            {
                Console.WriteLine("Specialty is required.");
                Console.ReadKey();
                return;
            }

            if (spec.Any(char.IsDigit))
            {
                Console.WriteLine("Specialty cannot contain numbers.");
                Console.ReadKey();
                return;
            }

            // Load departments
            var depts = SqliteDatabase.GetAllDepartments();
            if (!depts.Any())
            {
                Console.WriteLine("No departments available. Create a department first.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nSelect Department:");
            for (int i = 0; i < depts.Count; i++)
                Console.WriteLine($"{i + 1}. {depts[i].Name}");

            int dIndex = ReadSelection(depts.Count);
            if (dIndex == -1) return;

            var dept = depts[dIndex];

            // Salary
            double salary = ReadSafeDouble("Salary", 0);
            if (salary <= 0)
            {
                Console.WriteLine("Invalid salary.");
                Console.ReadKey();
                return;
            }

            string id = SqliteDatabase.GenerateDoctorID();

            var doctor = new Doctor(id, name, spec, salary)
            {
                DepartmentName = dept.Name,
                DepartmentID = dept.ID
            };

            SqliteDatabase.SaveDoctor(doctor);

            Console.WriteLine($"Doctor '{name}' added and assigned to {dept.Name}.");
            Console.ReadKey();
        }

        //==================================================
        //UPDATE DOCTOR DETAILS
        //==================================================
        private void UpdateDoctor()
        {
            Console.Clear();
            Doctors = SqliteDatabase.GetAllDoctorsWithIDs();

            if (!Doctors.Any())
            {
                Console.WriteLine(" No doctors found.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" UPDATE DOCTOR");
            Console.WriteLine("-----------------------");
            Console.WriteLine("Select a doctor to update:\n");

            for (int i = 0; i < Doctors.Count; i++)
            {
                var d = Doctors[i];
                Console.WriteLine($"{i + 1}. {d.Name} ({d.ID}) — {d.Specialty}");
            }

            Console.Write("\nSelect doctor (Enter to cancel): ");
            int index = ReadSelection(Doctors.Count);

            if (index == -1)
            {
                Console.WriteLine(" Cancelled.");
                Console.ReadKey();
                return;
            }

            var doc = Doctors[index];

            Console.WriteLine($"\nEditing Doctor: {doc.Name} ({doc.ID})");
            Console.WriteLine("Leave field empty to keep current value.\n");

            Console.Write($"Name ({doc.Name}): ");
            string newName = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(newName))
                doc.Name = newName;

            Console.Write($"Specialty ({doc.Specialty}): ");
            string newSpec = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(newSpec))
                doc.Specialty = newSpec;

            Console.Write($"Department ({doc.DepartmentName}): ");
            string newDept = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(newDept))
                doc.DepartmentName = newDept;

            double newSalary = ReadSafeDouble($"Salary (current {doc.Salary:F2}, Enter to keep)", doc.Salary);
            doc.Salary = newSalary;

            SqliteDatabase.UpdateDoctor(doc);

            Console.WriteLine("\n Doctor updated successfully.");
            Console.ReadKey();
        }
        //=================================================
        //DELETE A DOCTOR
        //=================================================
        private void DeleteDoctor()
        {
            Console.Clear();

            // Load all doctors
            Doctors = SqliteDatabase.GetAllDoctorsWithIDs();

            if (!Doctors.Any())
            {
                Console.WriteLine(" No doctors found.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" DELETE DOCTOR");
            Console.WriteLine("-----------------------");
            Console.WriteLine("Select a doctor to delete:\n");

            // Show list of doctors
            for (int i = 0; i < Doctors.Count; i++)
            {
                var d = Doctors[i];
                Console.WriteLine($"{i + 1}. {d.ID} — {d.Name} ({d.Specialty})");
            }

            Console.Write("\nSelect doctor number (Enter to cancel): ");
            int index = ReadSelection(Doctors.Count);

            if (index == -1)
            {
                Console.WriteLine(" Cancelled.");
                Console.ReadKey();
                return;
            }

            var doctor = Doctors[index];

            // Confirm delete
            Console.WriteLine($"\nAre you sure you want to delete Dr. {doctor.Name}? (y/n): ");
            string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";

            if (confirm != "y")
            {
                Console.WriteLine(" Deletion cancelled.");
                Console.ReadKey();
                return;
            }

            SqliteDatabase.DeleteDoctor(doctor.ID);

            Console.WriteLine($" Doctor {doctor.Name} deleted.");
            Console.ReadKey();
        }
        // ============================================================
        // PATIENT MANAGEMENT
        // ============================================================
        private void PatientMenu()
        {
            bool submenu = true;

            while (submenu)
            {
                Console.Clear();
                Console.WriteLine(" PATIENT MANAGEMENT");
                Console.WriteLine("----------------------------");
                Console.WriteLine("1. View All Patients");
                Console.WriteLine("2. Add New Patient");
                Console.WriteLine("3. Update Patient");
                Console.WriteLine("4. Delete Patient");
                Console.WriteLine("0. Back");
                Console.Write("Select: ");

                switch (Console.ReadLine())
                {
                    case "1": ShowAllPatients(); break;
                    case "2": AddNewPatient(); break;
                    case "3": UpdatePatient(); break;
                    case "4": DeletePatient(); break;
                    case "0": submenu = false; break;
                }
            }
        }

        //===================================================
        //SHOW ALL PATIENTS
        //===================================================
        private void ShowAllPatients()
        {
            Console.Clear();
            Patients = SqliteDatabase.GetAllPatients();

            if (!Patients.Any())
            {
                Console.WriteLine(" No patients found.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("\n=========================================");
            Console.WriteLine("List of All Registere Patients:");
            Console.WriteLine("PID   /Pat Name     /Diagnosis   /Bill:");
            Console.WriteLine("---------------------------------------------");
            foreach (var p in Patients)
            {
                Console.WriteLine($"{p.ID} - {p.Name} ({p.Diagnosis}) |Bill €{p.GetBill():F2}");
            }
            Console.WriteLine("---------------------------------------------");
            Console.ReadKey();
        }
        //===============================================
        //ADD NEW PATIENT
        //==============================================
        private void AddNewPatient()
        {
            Console.Clear();
            Console.WriteLine("\nREGISTER NEW PATIENT");
            Console.WriteLine("----------------------------");

            // ============================
            // NAME (Required, letters only)
            // ============================
            Console.Write("Name: ");
            string name = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name is required.");
                Console.ReadKey();
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z\s\-']+$"))
            {
                Console.WriteLine("Name contains invalid characters. Only letters, spaces, hyphens allowed.");
                Console.ReadKey();
                return;
            }
            DateTime birthDate = ReadSafeDate("Birthdate");

            // ADDRESS (Required)
            Console.Write("Address: ");
            string address = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(address))
            {
                Console.WriteLine("Address is required.");
                Console.ReadKey();
                return;
            }

            // DIAGNOSIS (Full validation)
            string diagnosis = "";
            while (true)
            {
                Console.Write("Diagnosis: ");
                diagnosis = Console.ReadLine()?.Trim() ?? "";

                // Ensure no Empty input
                if (string.IsNullOrWhiteSpace(diagnosis))
                {
                    Console.WriteLine("Diagnosis is required. Please enter a valid description.");
                    continue;
                }

                // Letters only (no numbers)
                if (!System.Text.RegularExpressions.Regex.IsMatch(diagnosis, @"^[A-Za-z\s,\-]+$"))
                {
                    Console.WriteLine("Diagnosis must contain letters only. Example: 'Pneumonia', 'Chronic Back Pain'.");
                    continue;
                }

                // Minimum length
                if (diagnosis.Length < 3)
                {
                    Console.WriteLine("Diagnosis is too short. Please provide more detail.");
                    continue;
                }

                break;
            }

            // ============================
            // SELECT DEPARTMENT
            // ============================
            var depts = SqliteDatabase.GetAllDepartments();
            if (!depts.Any())
            {
                Console.WriteLine("No departments available. Please create a department first.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nSelect Department:");
            for (int i = 0; i < depts.Count; i++)
                Console.WriteLine($"{i + 1}. {depts[i].Name}");

            int dIndex = ReadSelection(depts.Count);
            if (dIndex == -1)
            {
                Console.WriteLine("Cancelled.");
                Console.ReadKey();
                return;
            }

            var dept = depts[dIndex];

            // ============================
            // CREATE PATIENT RECORD
            // ============================
            string id = SqliteDatabase.GeneratePatientID();

            var patient = new AppointmentPatient
            {
                ID = id,
                Name = name,
                BirthDate = birthDate,
                Address = address,
                Diagnosis = diagnosis,
                DepartmentID = dept.ID
            };

            SqliteDatabase.SavePatient(patient);

            Console.WriteLine($"\nPatient '{name}' successfully registered under {dept.Name}.");
            Console.ReadKey();
        }

        //-----------Safe Date Reader------------------//
        private DateTime ReadSafeDate(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (yyyy-MM-dd): ");
                string input = Console.ReadLine()?.Trim() ?? "";

                // --- Enforce EXACT format yyyy-MM-dd ---
                if (!DateTime.TryParseExact(
                        input,
                        "yyyy-MM-dd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime date))
                {
                    Console.WriteLine("Invalid date. The ONLY accepted format is: yyyy-MM-dd (e.g., 1998-04-12).");
                    continue;
                }

                // --- Prevent future dates (optional rule) ---
                if (date > DateTime.Now.Date)
                {
                    Console.WriteLine("Date cannot be in the future. Enter a current or past date.");
                    continue;
                }

                // --- Prevent unrealistic dates (e.g., year < 1900) ---
                if (date.Year < 1900)
                {
                    Console.WriteLine("Year is too old to be valid. Please enter a date from 1900 onward.");
                    continue;
                }

                // --- Valid result ---
                return date;
            }
        }

        //===========================================
        //UPDATE PATIENT DETAIL
        //===========================================
        private void UpdatePatient()
        {
            Console.Clear();
            Patients = SqliteDatabase.GetAllPatients();

            if (!Patients.Any())
            {
                Console.WriteLine(" No patients found.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" UPDATE PATIENT");
            Console.WriteLine("------------------------");
            Console.WriteLine("Select a patient to update:\n");

            for (int i = 0; i < Patients.Count; i++)
            {
                var p = Patients[i];
                Console.WriteLine($"{i + 1}. {p.Name} ({p.ID}) — {p.Diagnosis}");
            }

            Console.Write("\nSelect patient (Enter to cancel): ");
            int index = ReadSelection(Patients.Count);

            if (index == -1)
            {
                Console.WriteLine(" Cancelled.");
                Console.ReadKey();
                return;
            }

            var patient = Patients[index];

            Console.WriteLine($"\nEditing Patient: {patient.Name} ({patient.ID})");
            Console.WriteLine("Leave field empty to keep current value.\n");

            Console.Write($"Name ({patient.Name}): ");
            string newName = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(newName))
                patient.Name = newName;

            Console.Write($"Address ({patient.Address}): ");
            string newAddr = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(newAddr))
                patient.Address = newAddr;

            Console.Write($"Diagnosis ({patient.Diagnosis}): ");
            string newDiag = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(newDiag))
                patient.Diagnosis = newDiag;

            // Optional: safe birthdate update
            Console.Write($"Birthdate ({patient.BirthDate:yyyy-MM-dd}) - press Enter to keep, or type new (yyyy-MM-dd): ");
            string newBirthText = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(newBirthText) &&
                DateTime.TryParse(newBirthText, out DateTime newBirth) &&
                newBirth <= DateTime.Now)
            {
                patient.BirthDate = newBirth;
            }

            SqliteDatabase.UpdatePatient(patient);

            Console.WriteLine("\n Patient updated successfully.");
            Console.ReadKey();
        }

        //===============================================
        //DELETE A PATIENT
        //===============================================
        private void DeletePatient()
        {
            Console.Clear();
            Patients = SqliteDatabase.GetAllPatients();

            if (!Patients.Any())
            {
                Console.WriteLine(" No patients found.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" DELETE PATIENT");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Select a patient to delete:\n");

            // Show patient list
            for (int i = 0; i < Patients.Count; i++)
            {
                var p = Patients[i];
                Console.WriteLine($"{i + 1}. {p.ID} — {p.Name} ({p.Diagnosis})");
            }

            Console.WriteLine("\nEnter patient number OR type an ID manually.");
            Console.Write("Selection (Enter to cancel): ");

            string input = Console.ReadLine()?.Trim() ?? "";

            // Cancel if empty
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine(" Cancelled.");
                Console.ReadKey();
                return;
            }

            AppointmentPatient? patientToDelete = null;

            // Try selection by number (1-N)
            if (int.TryParse(input, out int num) && num >= 1 && num <= Patients.Count)
            {
                patientToDelete = Patients[num - 1];
            }
            else
            {
                // Try match by ID
                patientToDelete = Patients.FirstOrDefault(p => p.ID.Equals(input, StringComparison.OrdinalIgnoreCase));
            }

            if (patientToDelete == null)
            {
                Console.WriteLine(" Invalid selection or ID not found.");
                Console.ReadKey();
                return;
            }

            // Confirm delete
            Console.Write($"\nAre you sure you want to delete patient '{patientToDelete.Name}' ({patientToDelete.ID})? (y/n): ");
            string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";

            if (confirm != "y")
            {
                Console.WriteLine(" Deletion cancelled.");
                Console.ReadKey();
                return;
            }

            SqliteDatabase.DeletePatient(patientToDelete.ID);

            Console.WriteLine($" Patient '{patientToDelete.Name}' deleted.");
            Console.ReadKey();
        }

        // ============================================================
        // NURSE MANAGEMENT
        // ============================================================
        private void NurseMenu()
        {
            bool submenu = true;

            while (submenu)
            {
                Console.Clear();
                Console.WriteLine(" NURSE MANAGEMENT");
                Console.WriteLine("----------------------------");
                Console.WriteLine("1. View All Nurses");
                Console.WriteLine("2. Add Nurse");
                Console.WriteLine("3. Update Nurse");
                Console.WriteLine("4. Delete Nurse");
                Console.WriteLine("0. Back");
                Console.Write("Select: ");

                var choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1": ShowAllNurses(); break;
                    case "2": AddNurse(); break;
                    case "3": UpdateNurse(); break;
                    case "4": DeleteNurse(); break;
                    case "0": submenu = false; break;
                    default:
                        Console.WriteLine(" Invalid selection. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void ShowAllNurses()
        {
            Console.Clear();
            Nurses = SqliteDatabase.GetAllNursesWithIDs();

            if (!Nurses.Any())
            {
                Console.WriteLine(" No nurses found.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" ALL NURSES");
            Console.WriteLine("----------------------------");

            foreach (var n in Nurses)
                n.ShowEmployeeInfo();

            Console.WriteLine("Press any key to return...");
            Console.ReadKey();
        }
        private void AddNurse()
        {
            Console.Clear();
            Console.WriteLine("\nREGISTER NURSE");
            Console.WriteLine("----------------------------");

            // Name
            Console.Write("Name: ");
            string name = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name is required.");
                Console.ReadKey();
                return;
            }

            // Load departments
            var depts = SqliteDatabase.GetAllDepartments();
            if (!depts.Any())
            {
                Console.WriteLine("No departments available. Create a department first.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nSelect Department:");
            for (int i = 0; i < depts.Count; i++)
                Console.WriteLine($"{i + 1}. {depts[i].Name}");

            int dIndex = ReadSelection(depts.Count);
            if (dIndex == -1) return;

            var dept = depts[dIndex];

            // Salary
            double salary = ReadSafeDouble("Salary", 0);

            // Shift Hours
            Console.Write("Shift hours (number): ");
            int hours = (int)ReadSafeDouble("Shift hours", 8);

            string id = SqliteDatabase.GenerateNurseID();

            var nurse = new Nurse
            {
                ID = id,
                Name = name,
                DepartmentName = dept.Name,
                DepartmentID = dept.ID,
                Salary = salary,
                ShiftHours = hours
            };

            SqliteDatabase.SaveNurse(nurse);

            Console.WriteLine($"Nurse '{name}' added and assigned to {dept.Name}.");
            Console.ReadKey();
        }
        //===================================================
        // UPDATE NURSE
        //===================================================
        private void UpdateNurse()
        {
            Console.Clear();
            Nurses = SqliteDatabase.GetAllNursesWithIDs();

            if (!Nurses.Any())
            {
                Console.WriteLine("No nurses found.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Select nurse to update:");
            for (int i = 0; i < Nurses.Count; i++)
                Console.WriteLine($"{i + 1}. {Nurses[i].Name} ({Nurses[i].ID})");

            Console.Write("Select (Enter to cancel): ");

            int index;
            try
            {
                index = ReadSelection(Nurses.Count);
            }
            catch
            {
                Console.WriteLine("Invalid selection.");
                Console.ReadKey();
                return;
            }

            if (index == -1)
            {
                Console.WriteLine("Cancelled.");
                Console.ReadKey();
                return;
            }

            var n = Nurses[index];

            Console.WriteLine($"\nEditing Nurse {n.Name} ({n.ID})");

            try
            {
                Console.Write($"Name ({n.Name}): ");
                string name = Console.ReadLine() ?? "";
                if (!string.IsNullOrWhiteSpace(name))
                    n.Name = name.Trim();
            }
            catch
            {
                Console.WriteLine("Invalid name input.");
            }

            try
            {
                Console.Write($"Department ({n.DepartmentName}): ");
                string dept = Console.ReadLine() ?? "";
                if (!string.IsNullOrWhiteSpace(dept))
                    n.DepartmentName = dept.Trim();
            }
            catch
            {
                Console.WriteLine("Invalid department input.");
            }

            try
            {
                Console.Write($"Salary ({n.Salary}): ");
                string salaryText = Console.ReadLine() ?? "";

                if (double.TryParse(salaryText, out double newSalary) && newSalary >= 0)
                    n.Salary = newSalary;
                else if (!string.IsNullOrWhiteSpace(salaryText))
                    Console.WriteLine("Invalid salary - change skipped.");
            }
            catch
            {
                Console.WriteLine("Invalid salary format.");
            }

            try
            {
                Console.Write($"Shift Hours ({n.ShiftHours}): ");
                string hoursText = Console.ReadLine() ?? "";

                if (int.TryParse(hoursText, out int newHours) && newHours >= 0)
                    n.ShiftHours = newHours;
                else if (!string.IsNullOrWhiteSpace(hoursText))
                    Console.WriteLine("Invalid shift hours - change skipped.");
            }
            catch
            {
                Console.WriteLine("Invalid hours format.");
            }

            try
            {
                SqliteDatabase.UpdateNurse(n);
                Console.WriteLine(" Nurse updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error saving nurse: {ex.Message}");
            }

            Console.ReadKey();
        }


        private void DeleteNurse()
        {
            Console.Clear();
            Nurses = SqliteDatabase.GetAllNursesWithIDs();

            if (!Nurses.Any())
            {
                Console.WriteLine("No nurses to delete.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Select nurse to delete:");
            for (int i = 0; i < Nurses.Count; i++)
                Console.WriteLine($"{i + 1}. {Nurses[i].Name} ({Nurses[i].ID})");

            Console.Write("Select (Enter to cancel): ");
            int index = ReadSelection(Nurses.Count);
            if (index == -1)
            {
                Console.WriteLine("Cancelled.");
                Console.ReadKey();
                return;
            }

            var n = Nurses[index];

            Console.Write($"Are you sure you want to delete {n.Name} (y/N)? ");
            string confirm = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            if (confirm != "y" && confirm != "yes")
            {
                Console.WriteLine("Cancelled.");
                Console.ReadKey();
                return;
            }

            SqliteDatabase.DeleteNurse(n.ID);
            Nurses.RemoveAt(index);

            Console.WriteLine(" Nurse deleted.");
            Console.ReadKey();
        }


        // ============================================================
        // APPOINTMENT MANAGEMENT
        // ============================================================
        private void AppointmentMenu()
        {
            bool submenu = true;

            while (submenu)
            {
                Console.Clear();
                Console.WriteLine("APPOINTMENT MANAGEMENT");
                Console.WriteLine("1. Book Appointment");
                Console.WriteLine("2. View Patient Appointments");
                Console.WriteLine("3. Update Appointment");
                Console.WriteLine("4. Delete Appointment");
                Console.WriteLine("5. View ALL Appointments");
                Console.WriteLine("0. Back");

                switch (Console.ReadLine())
                {
                    case "1": BookAppointment(); break;
                    case "2": ViewAppointmentsForPatient(); break;
                    case "3": UpdateAppointmentFlow(); break;
                    case "4": DeleteAppointmentFlow(); break;
                    case "5": ShowAllAppointments(); break;
                    case "0": submenu = false; break;
                }
            }
        }
        //=================================================
        //BOOK APPOINTMENT
        //=================================================
        private void BookAppointment()
        {
            Console.Clear();

            // Reload to be safe
            Patients = SqliteDatabase.GetAllPatients();
            Doctors = SqliteDatabase.GetAllDoctorsWithIDs();

            if (!Patients.Any())
            {
                Console.WriteLine(" No patients registered yet.");
                Console.WriteLine("Press Any Key to Return");
                Console.ReadKey();
                return;
            }

            if (!Doctors.Any())
            {
                Console.WriteLine(" No doctors available. Please add doctors first.");
                Console.WriteLine("Press Any Key to Return");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" BOOK APPOINTMENT");
            Console.WriteLine("---------------------------\n");

            // 1. Select patient
            Console.WriteLine("Select patient:");
            for (int i = 0; i < Patients.Count; i++)
                Console.WriteLine($"{i + 1}. {Patients[i].Name} ({Patients[i].ID})");

            Console.Write("\nSelect patient (Enter to cancel): ");
            int pIndex = ReadSelection(Patients.Count);

            if (pIndex == -1)
            {
                Console.WriteLine(" Cancelled.");
                Console.WriteLine("Press Any Key to Return");
                Console.ReadKey();
                return;
            }

            var patient = Patients[pIndex];

            // 2. Select doctor
            Console.Clear();
            Console.WriteLine(" BOOK APPOINTMENT");
            Console.WriteLine("---------------------------\n");
            Console.WriteLine($"Patient: {patient.Name} ({patient.ID})\n");

            Console.WriteLine("Select doctor:");
            for (int i = 0; i < Doctors.Count; i++)
                Console.WriteLine($"{i + 1}. {Doctors[i].Name} - {Doctors[i].Specialty} ({Doctors[i].ID})");

            Console.Write("\nSelect doctor (Enter to cancel): ");
            int dIndex = ReadSelection(Doctors.Count);

            if (dIndex == -1)
            {
                Console.WriteLine(" Cancelled.");
                Console.ReadKey();
                return;
            }

            var doctor = Doctors[dIndex];

            // 3. Reason + Fee
            Console.Write("\nReason (default: Check-up): ");
            string reason = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(reason))
                reason = "Check-up";

            double fee = ReadSafeDouble("Fee (€)", defaultValue: 0);

            var appt = new Appointment
            {
                ID = SqliteDatabase.GenerateAppointmentID(),
                DoctorID = doctor.ID,
                PatientID = patient.ID,
                Reason = reason,
                Fee = fee,
                Date = DateTime.Now,
                Status = "Scheduled"
            };

            // Update in-memory object and DB
            patient.AddAppointment(appt);
            SqliteDatabase.AddAppointment(appt);

            Console.WriteLine("\n Appointment booked successfully!");
            Console.WriteLine($"   Patient : {patient.Name} ({patient.ID})");
            Console.WriteLine($"   Doctor  : {doctor.Name} ({doctor.ID})");
            Console.WriteLine($"   Reason  : {appt.Reason}");
            Console.WriteLine($"   Fee     : €{appt.Fee:F2}");
            Console.WriteLine($"   Date    : {appt.Date:yyyy-MM-dd HH:mm}");
            Console.ReadKey();
        }

        //-----------------------------------------------//
        //UPDATE PATIENT APPOINTMENT
        //------------------------------------------------//
        private void UpdateAppointmentFlow()
        {
            Console.Clear();

            Patients = SqliteDatabase.GetAllPatients();

            var withAppointments = Patients
                .Where(p => p.Appointments.Any())
                .ToList();

            if (!withAppointments.Any())
            {
                Console.WriteLine(" No patients have appointments.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" UPDATE APPOINTMENT");
            Console.WriteLine("------------------------------");

            // 1. Select patient
            for (int i = 0; i < withAppointments.Count; i++)
                Console.WriteLine($"{i + 1}. {withAppointments[i].Name} ({withAppointments[i].ID}) — {withAppointments[i].Appointments.Count} appointment(s)");

            Console.Write("\nSelect patient: ");
            if (!int.TryParse(Console.ReadLine(), out int pSel) || pSel < 1 || pSel > withAppointments.Count)
            {
                Console.WriteLine(" Invalid input.");
                Console.ReadKey();
                return;
            }

            var patient = withAppointments[pSel - 1];

            // 2. Select appointment
            Console.Clear();
            Console.WriteLine($"Appointments for {patient.Name}");
            Console.WriteLine("----------------------------------");

            var appts = patient.Appointments;

            for (int i = 0; i < appts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. [{appts[i].ID}] {appts[i].Date:yyyy-MM-dd HH:mm} — {appts[i].Reason} — €{appts[i].Fee:F2}");
            }

            Console.Write("\nSelect appointment to update: ");
            if (!int.TryParse(Console.ReadLine(), out int aSel) || aSel < 1 || aSel > appts.Count)
            {
                Console.WriteLine("Invalid input.");
                Console.ReadKey();
                return;
            }

            var appt = appts[aSel - 1];
            //=======================================================
            //APPOINTMENT UPDATE MENU
            //-----------------------------------------
            Console.Clear();
            Console.WriteLine($"Updating Appointment {appt.ID}");
            Console.WriteLine("----------------------------------");

            Console.Write($"New Reason (leave empty = no change): ");
            string? reason = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(reason))
                appt.Reason = reason;

            double newFee = ReadSafeDouble("New Fee (leave empty = no change)", appt.Fee);
            appt.Fee = newFee;

            Console.Write($"New Status (Scheduled, Completed, Cancelled): ");
            string? status = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(status))
                appt.Status = status;

            // SAVE UPDATE
            SqliteDatabase.SaveAppointment(appt);

            Console.WriteLine("\n Appointment updated successfully!");
            Console.ReadKey();
        }

        //====================================================
        //DELETE APPOINTMENT
        //====================================================
        private void DeleteAppointmentFlow()
        {
            Console.Clear();

            Patients = SqliteDatabase.GetAllPatients();

            var withAppointments = Patients
                .Where(p => p.Appointments.Any())
                .ToList();

            if (!withAppointments.Any())
            {
                Console.WriteLine("No patients have appointments.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" DELETE APPOINTMENT");
            Console.WriteLine("------------------------------");

            // 1. Select patient
            for (int i = 0; i < withAppointments.Count; i++)
                Console.WriteLine($"{i + 1}. {withAppointments[i].Name} ({withAppointments[i].ID}) — {withAppointments[i].Appointments.Count} appointment(s)");

            Console.Write("\nSelect patient: ");
            if (!int.TryParse(Console.ReadLine(), out int pSel) || pSel < 1 || pSel > withAppointments.Count)
            {
                Console.WriteLine(" Invalid input.");
                Console.ReadKey();
                return;
            }

            var patient = withAppointments[pSel - 1];

            // 2. Select appointment
            Console.Clear();
            Console.WriteLine($"Appointments for {patient.Name}");
            Console.WriteLine("----------------------------------");

            var appts = patient.Appointments;

            for (int i = 0; i < appts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. [{appts[i].ID}] {appts[i].Date:yyyy-MM-dd HH:mm} — {appts[i].Reason} — €{appts[i].Fee:F2}");
            }

            Console.Write("\nSelect appointment to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int aSel) || aSel < 1 || aSel > appts.Count)
            {
                Console.WriteLine(" Invalid input.");
                Console.ReadKey();
                return;
            }

            var appt = appts[aSel - 1];

            // DELETE from patient list
            patient.RemoveAppointment(appt.ID);

            // DELETE from DB
            SqliteDatabase.DeleteAppointment(appt.ID);

            Console.WriteLine("\n Appointment deleted successfully!");
            Console.ReadKey();
        }

        //==================================================
        //SHOW ALL APPOINTMENTS
        //===================================================
        private void ShowAllAppointments()
        {
            Console.Clear();
            Console.WriteLine(" ALL APPOINTMENTS");
            Console.WriteLine("--------------------------");

            var allAppointments = SqliteDatabase.GetAllAppointmentsWithPatientNamesAndDoctorNames();

            if (!allAppointments.Any())
            {
                Console.WriteLine("No appointments found.");
                Console.ReadKey();
                return;
            }

            foreach (var a in allAppointments)
            {
                Console.WriteLine($"\n Appointment ID: {a.ID}");
                Console.WriteLine($" Doctor: {a.DoctorName} ({a.DoctorID})");
                Console.WriteLine($" Patient: {a.PatientName} ({a.PatientID})");
                Console.WriteLine($" Date: {a.Date}");
                Console.WriteLine($" Reason: {a.Reason}");
                Console.WriteLine($" Fee: €{a.Fee:F2}");
                Console.WriteLine($" Status: {a.Status}");
                Console.WriteLine("------------------------------------");
            }

            Console.ReadKey();
        }
        //=================================================
        //VIEW A SINGLE PATIENT APPOINTMENT
        //================================================
        private void ViewAppointmentsForPatient()
        {
            Console.Clear();

            // Reload patients to ensure appointment lists are loaded fresh
            Patients = SqliteDatabase.GetAllPatients();

            // Filter only patients with appointments
            var withAppointments = Patients
                .Where(p => p.Appointments != null && p.Appointments.Count > 0)
                .ToList();

            if (!withAppointments.Any())
            {
                Console.WriteLine(" No patients have appointments yet.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" PATIENTS WITH APPOINTMENTS");
            Console.WriteLine("-----------------------------------");

            // List all patients with appointment count
            for (int i = 0; i < withAppointments.Count; i++)
            {
                var p = withAppointments[i];
                Console.WriteLine($"{i + 1}. {p.Name} ({p.ID}) — {p.Appointments.Count} appointment(s)");
            }

            Console.Write("\nSelect a patient number: ");
            if (!int.TryParse(Console.ReadLine(), out int choice) ||
                choice < 1 || choice > withAppointments.Count)
            {
                Console.WriteLine(" Invalid selection.");
                Console.ReadKey();
                return;
            }

            var selected = withAppointments[choice - 1];

            Console.Clear();
            Console.WriteLine($" APPOINTMENTS FOR {selected.Name}");
            Console.WriteLine("-----------------------------------");

            selected.ShowAppointments();

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        // ============================================================
        // INPATIENT & ROOM MANAGEMENT
        // ============================================================
        private void InpatientRoomMenu()
        {
            bool submenu = true;

            while (submenu)
            {
                Console.Clear();
                Console.WriteLine(" INPATIENT & ROOM MANAGEMENT");
                Console.WriteLine("1. View Rooms");
                Console.WriteLine("2. Admit Inpatient");
                Console.WriteLine("3. Discharge Inpatient");
                Console.WriteLine("0. Back");
                Console.WriteLine("Select Option:");
                switch (Console.ReadLine())
                {
                    case "1": ShowRooms(); break;
                    case "2": AdmitInpatientFlow(); break;
                    case "3": DischargeInpatientFlow(); break;
                    case "0": submenu = false; break;
                }
            }
        }
        //===============================================
        //DISPLAY ROOMS
        //==============================================
        private void ShowRooms()
        {
            Console.Clear();
            Rooms = SqliteDatabase.GetAllRooms();
            Inpatients = SqliteDatabase.GetActiveInpatients();

            Console.WriteLine("\n---------------------------");
            Console.WriteLine("         ROOMS");
            Console.WriteLine("----------------------------");

            foreach (var room in Rooms)
            {
                int occ = Inpatients.Count(i => i.RoomID == room.RoomID && !i.DischargeDate.HasValue);

                Console.WriteLine($"Room {room.RoomID} — {room.Type} — {occ}/{room.Capacity} — €{room.DailyRate:F2}");
            }
            Console.WriteLine("Press Any Key to Return");
            Console.ReadKey();
        }
        //===================================================
        //ADMIT NEW PATIENT
        //===================================================
        private void AdmitInpatientFlow()
        {
            Console.Clear();
            Patients = SqliteDatabase.GetAllPatients();
            Inpatients = SqliteDatabase.GetActiveInpatients();

            var already = Inpatients.Select(i => i.ID).ToHashSet();
            var eligible = Patients.Where(p => !already.Contains(p.ID)).ToList();

            if (!eligible.Any())
            {
                Console.WriteLine("No eligible patients.");
                Console.WriteLine("Press Any Key to Return");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Select patient:");
            for (int i = 0; i < eligible.Count; i++)
                Console.WriteLine($"{i + 1}. {eligible[i].Name}");

            Console.Write("Select patient (Enter to cancel): ");
            int pIndex = ReadSelection(eligible.Count);

            if (pIndex == -1)
            {
                Console.WriteLine(" Cancelled.");
                Console.ReadKey();
                return;
            }

            var patient = eligible[pIndex];
            // SAFETY CHECK: Patient must have a valid name
            if (string.IsNullOrWhiteSpace(patient.Name))
            {
                Console.WriteLine(" Invalid patient record: missing name.");
                Console.ReadKey();
                return;
            }

            // SAFETY CHECK: Diagnosis exists
            if (string.IsNullOrWhiteSpace(patient.Diagnosis))
                patient.Diagnosis = "Unknown";

            // SAFETY CHECK: Birthdate cannot be in the future
            if (patient.BirthDate > DateTime.Now)
                patient.BirthDate = DateTime.Now;

            Rooms = SqliteDatabase.GetAllRooms();
            Inpatients = SqliteDatabase.GetActiveInpatients();

            var available = Rooms.Where(r =>
                Inpatients.Count(i => i.RoomID == r.RoomID && !i.DischargeDate.HasValue) < r.Capacity
            ).ToList();

            if (!available.Any())
            {
                Console.WriteLine("No rooms available.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nSelect room:");
            for (int i = 0; i < available.Count; i++)
                Console.WriteLine($"{i + 1}. Room {available[i].RoomID} ({available[i].Type})");

            // SAFE INPUT HERE 
            Console.Write("Select room (Enter to cancel): ");
            int roomIndex = ReadSelection(available.Count);

            if (roomIndex == -1)
            {
                Console.WriteLine(" Cancelled.");
                Console.ReadKey();
                return;
            }

            var room = available[roomIndex];
            // SAFETY: Room capacity corrupted?
            if (room.Capacity <= 0)
            {
                Console.WriteLine(" Room capacity invalid. Cannot admit.");
                Console.ReadKey();
                return;
            }

            var inp = new Inpatient(
                id: patient.ID,
                name: patient.Name,
                birthDate: patient.BirthDate,
                address: patient.Address,
                diagnosis: patient.Diagnosis,
                roomId: room.RoomID,
                dailyRate: room.DailyRate
            );

            SqliteDatabase.AdmitInpatient(inp);

            Console.WriteLine($" {patient.Name} admitted to Room {room.RoomID}");
            Console.ReadKey();
        }
        //================================================
        //SAFE INPUT HELPER
        //================================================
        private int ReadSelection(int max)
        {
            string input = Console.ReadLine()?.Trim() ?? "";

            // If user presses ENTER — cancel
            if (string.IsNullOrWhiteSpace(input))
                return -1;

            // Try convert
            if (!int.TryParse(input, out int num))
                return -1;

            // Validate range
            if (num < 1 || num > max)
                return -1;

            return num - 1;
        }
        //===================================================
        //SAFE DOUBLE INPUT HELPER for salary
        //===================================================
        private double ReadSafeDouble(string label, double defaultValue = 0)
        {
            while (true)
            {
                Console.Write($"{label}: ");
                string input = Console.ReadLine()?.Trim() ?? "";

                // Prevent empty input
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Input cannot be empty. Please enter a valid number.");
                    continue;
                }

                // Validate numeric entry
                if (!double.TryParse(input, out double value))
                {
                    Console.WriteLine("Invalid number format. Please enter digits only (e.g., 1200 or 1200.50).");
                    continue;
                }

                // Validate positive amount
                if (value < 0)
                {
                    Console.WriteLine(" Value cannot be negative. Enter a positive amount.");
                    continue;
                }

                return value;
            }
        }

        private int ReadSafeInt(string label, int defaultValue = 0)
        {
            Console.Write($"{label}: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (int.TryParse(input, out int value) && value >= 0)
                return value;

            return defaultValue;
        }

        private string ReadRequiredString(string label)
        {
            while (true)
            {
                try
                {
                    Console.Write($"{label}: ");
                    string input = Console.ReadLine() ?? "";

                    // Basic validation
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine("Input cannot be empty. Please enter a value.");
                        continue;
                    }

                    // Extra safety: prevent numbers-only for names or departments
                    if (input.All(char.IsDigit))
                    {
                        Console.WriteLine("Input cannot contain only numbers. Please enter valid text.");
                        continue;
                    }

                    // Optional: Prevent extremely short entries
                    if (input.Length < 2)
                    {
                        Console.WriteLine("Input too short. Must be at least 2 characters.");
                        continue;
                    }

                    // Optional: Prevent special characters except spaces, hyphens, and apostrophes
                    if (!input.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '\''))
                    {
                        Console.WriteLine("Invalid characters detected. Use only letters, numbers, spaces, hyphens (-), and apostrophes (').");
                        continue;
                    }

                    return input.Trim();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}. Please try again.");
                }
            }
        }

        //========================================================
        //DISCHARGE INPATIENT 
        //==========================================================
        private void DischargeInpatientFlow()
        {
            Console.Clear();
            Inpatients = SqliteDatabase.GetActiveInpatients();

            if (!Inpatients.Any())
            {
                Console.WriteLine(" No active inpatients to discharge.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" DISCHARGE INPATIENT");
            Console.WriteLine("---------------------------");
            Console.WriteLine("Select inpatient:\n");

            for (int i = 0; i < Inpatients.Count; i++)
            {
                var inp = Inpatients[i];
                Console.WriteLine($"{i + 1}. {inp.Name} — Room {inp.RoomID} — Admitted {inp.AdmissionDate:yyyy-MM-dd}");
            }

            Console.Write("\nSelect inpatient (Enter to cancel): ");
            int index = ReadSelection(Inpatients.Count);

            if (index == -1)
            {
                Console.WriteLine(" Cancelled.");
                Console.ReadKey();
                return;
            }

            var selected = Inpatients[index];

            if (selected == null)
            {
                Console.WriteLine(" Error: Inpatient record missing.");
                Console.ReadKey();
                return;
            }

            // Confirm
            Console.Write($"\n Are you sure you want to discharge {selected.Name}? (Y/N): ");
            string confirm = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (confirm != "Y")
            {
                Console.WriteLine(" Discharge cancelled.");
                Console.ReadKey();
                return;
            }

            // Discharge now
            SqliteDatabase.DischargeInpatient(selected.ID, DateTime.Now);

            Console.WriteLine($"\n {selected.Name} discharged successfully.");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey();
        }
        // ============================================================
        // DEPARTMENT MENU
        // ============================================================
        private void DepartmentMenu()
        {
            LoadDepartmentRelations();//NEW
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine(" DEPARTMENT MANAGEMENT");
                Console.WriteLine("1. View All Departments");
                Console.WriteLine("2. Add Department");
                Console.WriteLine("3. Explore Department");
                Console.WriteLine("4. Assign Staff to Department");
                Console.WriteLine("5. Department Dashboard");
                Console.WriteLine("0. Back");
                Console.Write("Select: ");

                switch (Console.ReadLine())
                {
                    case "1": ShowDepartments(); break;
                    case "2": AddDepartment(); break;
                    case "3": ExploreDepartment(); break;
                    case "4": AssignToDepartment(); break;
                    case "5": DepartmentDashboard(); break;
                    case "0": running = false; break;
                }
            }
        }
        //====SHOW ALL DEPARTMENTS====//
        private void ShowDepartments()
        {
            Console.Clear();
            LoadDepartments();

            if (!Departments.Any())
            {
                Console.WriteLine("No departments available.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" ALL DEPARTMENTS");
            Console.WriteLine("---------------------------");

            foreach (var d in Departments)
            {
                Console.WriteLine($"{d.ID} — {d.Name}");
            }

            Console.ReadKey();
        }
        //=========ADD DEPT=============//
        private void AddDepartment()
        {
            Console.Clear();
            Console.Write("Enter Department Name: ");
            string name = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Invalid name.");
                Console.ReadKey();
                return;
            }

            string id = SqliteDatabase.GenerateDepartmentID();
            var dept = new Department { ID = id, Name = name };

            SqliteDatabase.AddDepartment(dept);

            Console.WriteLine($" Department '{name}' added (ID: {id})");
            Console.ReadKey();
        }
        //===============EXPLORE DEPT===============//
        private void ExploreDepartment()
        {
            Console.Clear();
            LoadDepartmentRelations(); //NEW
            LoadDepartments();

            if (!Departments.Any())
            {
                Console.WriteLine("No departments available.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("SELECT DEPARTMENT:");
            for (int i = 0; i < Departments.Count; i++)
                Console.WriteLine($"{i + 1}. {Departments[i].Name}");

            Console.WriteLine("Enter Dept No. to Explore:");
            int index = ReadSelection(Departments.Count);
            if (index == -1) return;

            var dept = Departments[index];

            Console.Clear();
            Console.WriteLine($" DEPARTMENT: {dept.Name}");
            Console.WriteLine("----------------------------------");

            Console.WriteLine("\nDOCTORS:");
            if (!dept.Doctors.Any()) Console.WriteLine(" No doctors assigned.");
            foreach (var doc in dept.Doctors)
                Console.WriteLine($" - {doc.Name} ({doc.Specialty})");

            Console.WriteLine("\nNURSES:");
            if (!dept.Nurses.Any()) Console.WriteLine(" No nurses assigned.");
            foreach (var nurse in dept.Nurses)
                Console.WriteLine($" - {nurse.Name}");

            Console.WriteLine("\nPATIENTS:");
            if (!dept.Patients.Any()) Console.WriteLine(" No patients assigned.");
            foreach (var pat in dept.Patients)
                Console.WriteLine($" - {pat.Name} ({pat.Diagnosis})");

            Console.ReadKey();
        }
        //==========ASSIGN STAFF TO DEPARTMENT=========//
        private void AssignToDepartment()
        {
            Console.Clear();
            LoadDepartments();

            if (!Departments.Any())
            {
                Console.WriteLine("No departments available.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("SELECT DEPARTMENT:");
            for (int i = 0; i < Departments.Count; i++)
                Console.WriteLine($"{i + 1}. {Departments[i].Name}");

            int dIndex = ReadSelection(Departments.Count);
            if (dIndex == -1) return;

            var dept = Departments[dIndex];

            Console.Clear();
            Console.WriteLine($" Assign to {dept.Name}");
            Console.WriteLine("1. Doctor");
            Console.WriteLine("2. Nurse");
            Console.WriteLine("3. Patient");
            Console.WriteLine("0. Cancel");

            switch (Console.ReadLine())
            {
                case "1": AssignDoctor(dept); break;
                case "2": AssignNurse(dept); break;
                case "3": AssignPatient(dept); break;
            }
        }
        private void AssignDoctor(Department dept)
        {
            Doctors = SqliteDatabase.GetAllDoctorsWithIDs();

            Console.WriteLine("Select doctor:");
            for (int i = 0; i < Doctors.Count; i++)
                Console.WriteLine($"{i + 1}. {Doctors[i].Name}");

            int index = ReadSelection(Doctors.Count);
            if (index == -1) return;

            var doc = Doctors[index];
            doc.DepartmentID = dept.ID;

            SqliteDatabase.UpdateDoctor(doc);
            Console.WriteLine($" Doctor {doc.Name} assigned to {dept.Name}.");
            Console.ReadKey();
        }
        private void AssignNurse(Department dept)
        {
            Nurses = SqliteDatabase.GetAllNursesWithIDs();

            Console.WriteLine("Select nurse:");
            for (int i = 0; i < Nurses.Count; i++)
                Console.WriteLine($"{i + 1}. {Nurses[i].Name}");

            int index = ReadSelection(Nurses.Count);
            if (index == -1) return;

            var nurse = Nurses[index];
            nurse.DepartmentID = dept.ID;

            SqliteDatabase.UpdateNurse(nurse);
            Console.WriteLine($" Nurse {nurse.Name} assigned to {dept.Name}.");
            Console.ReadKey();
        }
        private void AssignPatient(Department dept)
        {
            Patients = SqliteDatabase.GetAllPatients();

            Console.WriteLine("Select patient:");
            for (int i = 0; i < Patients.Count; i++)
                Console.WriteLine($"{i + 1}. {Patients[i].Name}");

            int index = ReadSelection(Patients.Count);
            if (index == -1) return;

            var pat = Patients[index];
            pat.DepartmentID = dept.ID;

            SqliteDatabase.UpdatePatient(pat);
            Console.WriteLine($" Patient {pat.Name} assigned to {dept.Name}.");
            Console.ReadKey();
        }
        //===========DEPARTMENT DashBoard=========================//
        private void DepartmentDashboard()
        {
            Console.Clear();
            LoadDepartmentRelations();

            if (!Departments.Any())
            {
                Console.WriteLine("No departments available.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" HOSPITAL DEPARTMENT DASHBOARD");
            Console.WriteLine("=====================================\n");

            foreach (var dept in Departments)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{dept.Name.ToUpper()}");
                Console.ResetColor();

                // Head of department (optional)
                var head = Doctors.FirstOrDefault(d => d.ID == dept.HeadID);

                Console.WriteLine($"Head: {(head != null ? head.Name : "Not assigned")}");
                Console.WriteLine($"Doctors: {dept.Doctors.Count}");
                Console.WriteLine($"Nurses: {dept.Nurses.Count}");
                Console.WriteLine($"Patients: {dept.Patients.Count}");

                // Optional: inpatient count
                int inpatientCount = Inpatients.Count(i => dept.Patients.Any(p => p.ID == i.ID));
                Console.WriteLine($"Inpatients: {inpatientCount}");

                Console.WriteLine("--------------------------------------\n");
            }

            Console.WriteLine("Press any key to return...");
            Console.ReadKey();
        }


        // ============================================================
        // SEARCH
        // ============================================================
        private void SearchMenu()
        {
            Console.Clear();
            Console.WriteLine(" SEARCH");
            Console.WriteLine("-------------------------");
            Console.WriteLine("1. Patient by Name");
            Console.WriteLine("2. Patient by ID");
            Console.WriteLine("3. Patient by Diagnosis");
            Console.WriteLine("4. Doctor by Name");
            Console.WriteLine("5. Doctor by ID");
            Console.WriteLine("6. Doctor by Specialty");
            Console.WriteLine("7. Nurse by Name");
            Console.WriteLine("8. Nurse by ID");
            Console.WriteLine("9. Nurse by Department");
            Console.WriteLine("0. Back");
            Console.Write("\nSelect option (Enter = cancel): ");

            string choice = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(choice) || choice == "0")
                return; // Safe exit

            switch (choice)
            {
                case "1": SearchPatientByName_Binary(); break;
                case "2": SearchPatientByID_Binary(); break;
                case "3": SearchPatientByDiagnosis(); break;
                case "4": SearchDoctorByName_Binary(); break;
                case "5": SearchDoctorByID_Binary(); break;
                case "6": SearchDoctorBySpecialty(); break;
                case "7": SearchNurseByName_Binary(); break;
                case "8": SearchNurseByID_Binary(); break;
                case "9": SearchNurseByDepartment(); break;
                default:
                    Console.WriteLine("\n Invalid option.");
                    Console.WriteLine("Press any key...");
                    Console.ReadKey();
                    break;
            }
        }
        // ============================================================
        // BINARY SEARCH - NURSE BY NAME
        // ============================================================
        private int BinarySearchNurseByName(List<Nurse> list, string target)
        {
            int left = 0;
            int right = list.Count - 1;
            target = target.ToLower();

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                string midName = list[mid].Name.ToLower();

                int cmp = string.Compare(midName, target, StringComparison.Ordinal);

                if (cmp == 0)
                    return mid;

                if (cmp < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1; // Not found
        }
        private void SearchNurseByName_Binary()
        {
            Console.Clear();
            Nurses = SqliteDatabase.GetAllNursesWithIDs();

            if (!Nurses.Any())
            {
                Console.WriteLine("No nurses found.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter exact Nurse Name: ");
            string name = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name))
                return;

            // Sort before binary search
            Nurses = Nurses.OrderBy(n => n.Name).ToList();

            int index = BinarySearchNurseByName(Nurses, name);

            if (index == -1)
            {
                Console.WriteLine("Nurse not found.");
            }
            else
            {
                var n = Nurses[index];
                Console.WriteLine("\nFOUND NURSE:");
                n.ShowEmployeeInfo();
            }

            Console.ReadKey();
        }

        // ============================================================
        // BINARY SEARCH - NURSE BY ID
        // ============================================================
        private int BinarySearchNurseByID(List<Nurse> list, string targetID)
        {
            int left = 0;
            int right = list.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                string midID = list[mid].ID;

                int cmp = string.Compare(midID, targetID, StringComparison.Ordinal);

                if (cmp == 0)
                    return mid;

                if (cmp < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1; // Not found
        }
        private void SearchNurseByID_Binary()
        {
            Console.Clear();
            Nurses = SqliteDatabase.GetAllNursesWithIDs();

            if (!Nurses.Any())
            {
                Console.WriteLine("No nurses found.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter Nurse ID: ");
            string id = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(id))
                return;

            // Sort before binary search
            Nurses = Nurses.OrderBy(n => n.ID).ToList();

            int index = BinarySearchNurseByID(Nurses, id);

            if (index == -1)
            {
                Console.WriteLine("Nurse not found.");
            }
            else
            {
                var n = Nurses[index];
                Console.WriteLine("\nFOUND NURSE:");
                n.ShowEmployeeInfo();
            }

            Console.ReadKey();
        }
        // ============================================================
        // BINARY SEARCH - PATIENT BY ID
        // ============================================================
        private int BinarySearchPatientByID(List<AppointmentPatient> list, string targetID)
        {
            int left = 0;
            int right = list.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                string midID = list[mid].ID;

                int cmp = string.Compare(midID, targetID, StringComparison.Ordinal);

                if (cmp == 0)
                    return mid;

                if (cmp < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1;
        }
        private void SearchPatientByID_Binary()
        {
            Console.Clear();
            Patients = SqliteDatabase.GetAllPatients();

            if (!Patients.Any())
            {
                Console.WriteLine("No patients found.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter Patient ID: ");
            string id = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(id))
                return;

            // Sort before binary search
            Patients = Patients.OrderBy(p => p.ID).ToList();

            int index = BinarySearchPatientByID(Patients, id);

            if (index == -1)
            {
                Console.WriteLine("Patient not found.");
            }
            else
            {
                var p = Patients[index];
                Console.WriteLine("\nFOUND PATIENT:");
                Console.WriteLine($"{p.ID} — {p.Name} — {p.Diagnosis}");
            }

            Console.ReadKey();
        }

        private void SearchNurseByDepartment()
        {
            Console.Clear();
            Nurses = SqliteDatabase.GetAllNursesWithIDs();

            if (!Nurses.Any())
            {
                Console.WriteLine("No nurses found.");
                Console.ReadKey();
                return;
            }

            string dept = ReadRequiredString("Enter department (part)");
            var results = Nurses
                .Where(n => (n.DepartmentName ?? "")
                    .Contains(dept, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!results.Any())
            {
                Console.WriteLine("No nurses match that department.");
            }
            else
            {
                Console.WriteLine($"\nFound {results.Count} nurse(s):");
                foreach (var n in results)
                    n.ShowEmployeeInfo();
            }

            Console.ReadKey();
        }
        // ============================================================
        // BINARY SEARCH - PATIENT BY NAME
        // ============================================================
        private int BinarySearchPatientByName(List<AppointmentPatient> list, string target)
        {
            int left = 0;
            int right = list.Count - 1;
            target = target.ToLower();

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                string midName = list[mid].Name.ToLower();

                int cmp = string.Compare(midName, target, StringComparison.Ordinal);

                if (cmp == 0)
                    return mid;    // Found exact match

                if (cmp < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1; // Not found
        }
        //============================================
        private void SearchPatientByName_Binary()
        {
            Console.Clear();
            Patients = SqliteDatabase.GetAllPatients();

            if (!Patients.Any())
            {
                Console.WriteLine("No patients found.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter exact patient name: ");
            string name = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name))
                return;

            // MUST sort before binary search
            Patients = Patients.OrderBy(p => p.Name).ToList();

            int index = BinarySearchPatientByName(Patients, name);

            if (index == -1)
            {
                Console.WriteLine("Patient not found.");
            }
            else
            {
                var p = Patients[index];
                Console.WriteLine($"\nFOUND!");
                Console.WriteLine($"{p.ID} — {p.Name} — {p.Diagnosis}");
            }

            Console.ReadKey();
        }
        // ============================================================
        // BINARY SEARCH - DOCTOR BY ID
        // ============================================================
        private int BinarySearchDoctorByID(List<Doctor> list, string targetID)
        {
            int left = 0;
            int right = list.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                string midID = list[mid].ID;

                int cmp = string.Compare(midID, targetID, StringComparison.Ordinal);

                if (cmp == 0)
                    return mid;

                if (cmp < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1;
        }

        private void SearchDoctorByID_Binary()
        {
            Console.Clear();
            Doctors = SqliteDatabase.GetAllDoctorsWithIDs();

            if (!Doctors.Any())
            {
                Console.WriteLine("No doctors found.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter Doctor ID: ");
            string id = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(id))
                return;

            // MUST sort by ID before binary search
            Doctors = Doctors.OrderBy(d => d.ID).ToList();

            int index = BinarySearchDoctorByID(Doctors, id);

            if (index == -1)
            {
                Console.WriteLine("Doctor not found.");
            }
            else
            {
                var d = Doctors[index];
                Console.WriteLine("\nFOUND DOCTOR:");
                d.ShowEmployeeInfo();
            }

            Console.ReadKey();
        }

        //=======================================================================================

        private void SearchPatientByDiagnosis()
        {
            Console.Clear();
            Console.Write("Enter diagnosis: ");
            string diag = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(diag) || diag == "0")
                return; // Safe exit

            var results = Patients.Where(p => p.Diagnosis.Contains(diag, StringComparison.OrdinalIgnoreCase));

            foreach (var p in results)
                Console.WriteLine($"{p.Name} — {p.Diagnosis}");

            Console.ReadKey();
        }
        // ============================================================
        // BINARY SEARCH - DOCTOR BY NAME
        // ============================================================
        private int BinarySearchDoctorByName(List<Doctor> list, string target)
        {
            int left = 0;
            int right = list.Count - 1;
            target = target.ToLower();

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                string midName = list[mid].Name.ToLower();

                int cmp = string.Compare(midName, target, StringComparison.Ordinal);

                if (cmp == 0)
                    return mid;

                if (cmp < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1;
        }
        private void SearchDoctorByName_Binary()
        {
            Console.Clear();
            Doctors = SqliteDatabase.GetAllDoctorsWithIDs();

            if (!Doctors.Any())
            {
                Console.WriteLine("No doctors found.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter exact doctor name: ");
            string name = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name))
                return;

            // MUST sort first
            Doctors = Doctors.OrderBy(d => d.Name).ToList();

            int index = BinarySearchDoctorByName(Doctors, name);

            if (index == -1)
            {
                Console.WriteLine("Doctor not found.");
            }
            else
            {
                var d = Doctors[index];
                Console.WriteLine("\nFOUND DOCTOR:");
                d.ShowEmployeeInfo();
            }

            Console.ReadKey();
        }

        private void SearchDoctorBySpecialty()
        {
            Console.Clear();
            Console.Write("Specialty: ");
            string spec = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(spec) || spec == "0")
                return; // Safe exit
            var results = Doctors.Where(d => d.Specialty.Contains(spec, StringComparison.OrdinalIgnoreCase));

            foreach (var d in results)
                d.ShowEmployeeInfo();

            Console.ReadKey();
        }

        // ============================================================
        // SORTING
        // ============================================================
        private void SortMenu()
        {
            Console.Clear();
            Console.WriteLine(" SORTING MENU");
            Console.WriteLine("1. Sort Patients");
            Console.WriteLine("2. Sort Doctors");
            Console.WriteLine("3. Sort Inpatients");
            Console.WriteLine("4. Sort Nurses");
            Console.WriteLine("0. Back");

            switch (Console.ReadLine())
            {
                case "1": SortPatientsMenu(); break;
                case "2": SortDoctorsMenu(); break;
                case "3": SortInpatientsMenu(); break;
                case "4": SortNursesMenu(); break;
            }
        }

        private void SortPatientsMenu()
        {
            Console.Clear();
            Console.WriteLine("Sort Patients:");
            Console.WriteLine("1. Bubble Sort by Name");
            Console.WriteLine("2. Selection Sort by Diagnosis");
            Console.WriteLine("3. Merge Sort by Bill (High → Low)");

            string c = Console.ReadLine() ?? "";

            if (c == "1") BubbleSortByName(Patients);
            else if (c == "2") SelectionSortByDiagnosis(Patients);
            else if (c == "3") Patients = MergeSortByBill(Patients);

            foreach (var p in Patients)
                Console.WriteLine($"{p.Name} — €{p.GetBill():F2}");

            Console.WriteLine("Press Any Key to Return:");
            Console.ReadKey();
        }

        private void SortDoctorsMenu()
        {
            Console.Clear();
            Console.WriteLine("Sort Doctors:");
            Console.WriteLine("1. By Name");
            Console.WriteLine("2. By Specialty");

            string c = Console.ReadLine() ?? "";

            var sorted = c switch
            {
                "1" => Doctors.OrderBy(d => d.Name).ToList(),
                "2" => Doctors.OrderBy(d => d.Specialty).ToList(),
                _ => Doctors
            };

            foreach (var d in sorted)
                d.ShowEmployeeInfo();

            Console.WriteLine("Press Any Key to Return:");
            Console.ReadKey();
        }

        private void SortInpatientsMenu()
        {
            Console.Clear();
            Console.WriteLine("Sort Inpatients:");
            Console.WriteLine("1. By Name");
            Console.WriteLine("2. By Room");
            Console.WriteLine("3. By Bill");

            string c = Console.ReadLine() ?? "";

            var sorted = c switch
            {
                "1" => Inpatients.OrderBy(p => p.Name).ToList(),
                "2" => Inpatients.OrderBy(p => p.RoomID).ToList(),
                "3" => Inpatients.OrderByDescending(p => p.GetBill()).ToList(),
                _ => Inpatients
            };

            foreach (var p in sorted)
                p.ShowInfo();

            Console.WriteLine("Press Any Key to Return:");
            Console.ReadKey();
        }
        private void SortNursesMenu()
        {
            Console.Clear();
            Nurses = SqliteDatabase.GetAllNursesWithIDs();

            if (!Nurses.Any())
            {
                Console.WriteLine("No nurses to sort.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Sort Nurses:");
            Console.WriteLine("1. By Name");
            Console.WriteLine("2. By Department");
            Console.WriteLine("3. By Shift Hours");
            Console.Write("Select: ");

            string c = Console.ReadLine() ?? "";
            List<Nurse> sorted = Nurses;

            switch (c)
            {
                case "1":
                    sorted = Nurses.OrderBy(n => n.Name).ToList();
                    break;
                case "2":
                    sorted = Nurses.OrderBy(n => n.DepartmentName).ToList();
                    break;
                case "3":
                    sorted = Nurses.OrderByDescending(n => n.ShiftHours).ToList();
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    Console.ReadKey();
                    return;
            }

            Console.WriteLine("\nSORTED NURSES:");
            Console.WriteLine("---------------------------");
            foreach (var n in sorted)
                n.ShowEmployeeInfo();

            Console.WriteLine("Press any key to return...");
            Console.ReadKey();
        }

        //-----------------------------------------------------------------//
        //---------------------SORTING HELPERS-----------------------------//
        //-----------------------------------------------------------------//
        private void BubbleSortByName(List<AppointmentPatient> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = 0; j < list.Count - i - 1; j++)
                {
                    if (string.Compare(list[j].Name, list[j + 1].Name, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        (list[j], list[j + 1]) = (list[j + 1], list[j]);
                    }
                }
            }
        }
        private void SelectionSortByDiagnosis(List<AppointmentPatient> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                int min = i;
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (string.Compare(list[j].Diagnosis, list[min].Diagnosis, StringComparison.OrdinalIgnoreCase) < 0)
                        min = j;
                }
                (list[i], list[min]) = (list[min], list[i]);
            }
        }
        private List<AppointmentPatient> MergeSortByBill(List<AppointmentPatient> list)
        {
            if (list.Count <= 1) return list;

            int mid = list.Count / 2;
            var left = MergeSortByBill(list.GetRange(0, mid));
            var right = MergeSortByBill(list.GetRange(mid, list.Count - mid));

            return Merge(left, right);
        }
        private List<AppointmentPatient> Merge(List<AppointmentPatient> left, List<AppointmentPatient> right)
        {
            List<AppointmentPatient> result = new();
            int i = 0, j = 0;

            while (i < left.Count && j < right.Count)
            {
                if (left[i].GetBill() >= right[j].GetBill())
                    result.Add(left[i++]);
                else
                    result.Add(right[j++]);
            }

            result.AddRange(left.Skip(i));
            result.AddRange(right.Skip(j));

            return result;
        }

        // ============================================================
        // BILLING
        // ============================================================
        private void BillingMenu()
        {
            Console.Clear();
            Console.WriteLine(" BILLING");
            Console.WriteLine("1. Appointment Bill");
            Console.WriteLine("2. Inpatient Bill");
            Console.WriteLine("0. Back");

            switch (Console.ReadLine())
            {
                case "1": BillingAppointment(); break;
                case "2": BillingInpatient(); break;
            }
        }
        //------------------------------------------------
        //BILLING APPOINTMENT
        //------------------------------------------------
        private void BillingAppointment()
        {
            Console.Clear();

            // Always reload patients from DB to get latest appointments
            Patients = SqliteDatabase.GetAllPatients();

            if (!Patients.Any())
            {
                Console.WriteLine(" No patients in the system.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" APPOINTMENT BILLING");
            Console.WriteLine("-------------------------");
            Console.WriteLine("Select a patient to view appointment bill:\n");

            for (int i = 0; i < Patients.Count; i++)
            {
                var p = Patients[i];
                Console.WriteLine($"{i + 1}. {p.Name} ({p.ID}) — Appointments: {p.Appointments.Count}");
            }

            Console.Write("\nSelect patient (Enter to cancel): ");
            int index = ReadSelection(Patients.Count);

            if (index == -1)
            {
                Console.WriteLine(" Cancelled.");
                Console.ReadKey();
                return;
            }

            var patient = Patients[index];

            if (patient == null)
            {
                Console.WriteLine(" Error: Patient record missing.");
                Console.ReadKey();
                return;
            }

            double total = patient.GetBill();

            Console.Clear();
            Console.WriteLine($" APPOINTMENT BILL FOR {patient.Name}");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"Number of Appointments: {patient.Appointments.Count}");
            Console.WriteLine($"Total Appointment Bill: €{total:F2}");
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        //------------------------------------------------------
        //BILLING INPATIENT
        //------------------------------------------------------
        private void BillingInpatient()
        {
            Console.Clear();

            // Reload latest inpatients
            Inpatients = SqliteDatabase.GetActiveInpatients();

            // SAFETY: If no inpatients exist
            if (!Inpatients.Any())
            {
                Console.WriteLine(" No inpatients found.");
                Console.WriteLine("Press Any Key to Return");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Select inpatient:");
            for (int i = 0; i < Inpatients.Count; i++)
                Console.WriteLine($"{i + 1}. {Inpatients[i].Name} — Room {Inpatients[i].RoomID}");

            Console.Write("\nSelect inpatient (Enter to cancel): ");
            int index = ReadSelection(Inpatients.Count);

            // SAFETY: user cancelled or invalid input
            if (index == -1)
            {
                Console.WriteLine(" Cancelled.");
                Console.WriteLine("Press Any Key to Return");
                Console.ReadKey();
                return;
            }

            var inp = Inpatients[index];

            // SAFETY: Inpatient record cannot be empty or broken
            if (inp == null)
            {
                Console.WriteLine(" Error: Inpatient record is missing.");
                Console.ReadKey();
                return;
            }

            if (string.IsNullOrWhiteSpace(inp.Name))
                inp.Name = "Unknown Patient";

            // Safety check: Daily rate must be valid
            if (inp.DailyRate <= 0)
            {
                Console.WriteLine(" Daily rate invalid or missing. Setting to €100 default.");
                inp.DailyRate = 100; // safe fallback
            }

            Console.Clear();
            Console.WriteLine($" BILLING SUMMARY FOR {inp.Name}");
            Console.WriteLine("--------------------------------");

            int days = inp.GetDaysStayed();
            double total = inp.GetBill();

            Console.WriteLine($"Days Stayed: {days}");
            Console.WriteLine($"Daily Rate: €{inp.DailyRate:F2}");
            Console.WriteLine($"Total Bill: €{total:F2}");

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }

        // ============================================================
        // DASHBOARD
        // ============================================================
        private void ShowDashboard()
        {
            Console.Clear();
            Rooms = SqliteDatabase.GetAllRooms();
            Inpatients = SqliteDatabase.GetActiveInpatients();

            Console.WriteLine(" HOSPITAL DASHBOARD\n");

            foreach (var room in Rooms)
            {
                var occ = Inpatients.Where(i => i.RoomID == room.RoomID).ToList();

                Console.WriteLine($"Room {room.RoomID} ({room.Type}) — {occ.Count}/{room.Capacity}");

                foreach (var i in occ)
                    Console.WriteLine($"   → {i.Name} (Since {i.AdmissionDate:yyyy-MM-dd})");

                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}

