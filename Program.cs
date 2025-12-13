using HospitalManager.DataAccess;
using HospitalManager.BusinessLogic;

class Program
{
    static void Main()
    {
        // Initialize SQLite DB and tables
        SqliteDatabase.InitializeDatabase();   

        var hospital = new HospitalService();
        hospital.Start();
    }
}

