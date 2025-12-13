using System;
using System.Collections.Generic;

namespace HospitalManager.Models
{
    public class Room
    {
        public string RoomID { get; set; } = string.Empty;   // e.g., R001
        public string Type { get; set; } = string.Empty;     // General, Private, ICU
        public int Capacity { get; set; }      // Max beds
        public double DailyRate { get; set; }  // Cost per day
        public List<string> Occupants { get; set; } // Patient IDs (not stored in DB, used in-memory if needed)

        public Room()
        {
            Occupants = new List<string>();
        }

        public bool HasSpace() => Occupants.Count < Capacity;

        public void AddPatient(string patientId)
        {
            if (!HasSpace())
                throw new Exception("Room is full.");

            if (!Occupants.Contains(patientId))
                Occupants.Add(patientId);
        }

        public void RemovePatient(string patientId)
        {
            Occupants.Remove(patientId);
        }

        public override string ToString()
        {
            return $"{RoomID} | {Type} | Beds: {Occupants.Count}/{Capacity} | Rate: €{DailyRate:F2}";
        }
    }
}

