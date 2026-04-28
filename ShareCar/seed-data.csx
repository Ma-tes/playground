#r "nuget: Microsoft.Data.Sqlite, 8.0.11"

using Microsoft.Data.Sqlite;
using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var dbPath = @"c:\Source\ShareCar\src\ShareCar.BackendService.App\ShareCar.db";
var connection = new SqliteConnection($"Data Source={dbPath}");
connection.Open();

// Clear existing data (vehicles first due to FK)
Execute(connection, "DELETE FROM Vehicles;");
Execute(connection, "DELETE FROM ParkingLots;");

// Reset autoincrement
Execute(connection, "DELETE FROM sqlite_sequence WHERE name IN ('ParkingLots', 'Vehicles');");

// Insert parking lots — real Ostrava locations with GPS coordinates
var parkingLots = new[]
{
    (Name: "Forum Nová Karolina", Lat: 49.8356, Lon: 18.2878, Cap: 24),
    (Name: "OC Futurum Ostrava", Lat: 49.8289, Lon: 18.2563, Cap: 18),
    (Name: "Hlavní nádraží Ostrava", Lat: 49.8358, Lon: 18.2715, Cap: 12),
    (Name: "Outlet Arena Moravia", Lat: 49.7972, Lon: 18.2344, Cap: 20),
    (Name: "Parking Stodolní", Lat: 49.8393, Lon: 18.2870, Cap: 10),
    (Name: "VŠB-TU Ostrava Poruba", Lat: 49.8271, Lon: 18.1604, Cap: 30),
    (Name: "Městský stadion Vítkovice", Lat: 49.8178, Lon: 18.2752, Cap: 16),
};

foreach (var p in parkingLots)
{
    Execute(connection,
        $"INSERT INTO ParkingLots (Name, Latitude, Longitude, TotalCapacity) VALUES ('{Esc(p.Name)}', {p.Lat}, {p.Lon}, {p.Cap});");
}

// Read back IDs
var lotIds = new Dictionary<string, int>();
using (var cmd = connection.CreateCommand())
{
    cmd.CommandText = "SELECT Id, Name FROM ParkingLots;";
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        lotIds[reader.GetString(1)] = reader.GetInt32(0);
    }
}

// Insert vehicles — real car models with Czech-style plates, Status 0 = Available
var vehicles = new (string Model, string Plate, string LotName, int Odometer)[]
{
    // Forum Nová Karolina
    ("Škoda Octavia 2023", "2T4 1234", "Forum Nová Karolina", 15230),
    ("Škoda Fabia 2022", "3T2 5678", "Forum Nová Karolina", 28400),
    ("Volkswagen Golf 2023", "4T1 9012", "Forum Nová Karolina", 8750),
    ("Hyundai i30 2024", "5T3 3456", "Forum Nová Karolina", 3200),
    ("Tesla Model 3 2024", "6T0 7890", "Forum Nová Karolina", 12100),

    // OC Futurum
    ("Toyota Yaris 2023", "1T8 2345", "OC Futurum Ostrava", 19800),
    ("Škoda Superb 2022", "2T9 6789", "OC Futurum Ostrava", 42300),
    ("Renault Clio 2023", "3T0 1234", "OC Futurum Ostrava", 11500),

    // Hlavní nádraží
    ("Volkswagen Passat 2023", "4T2 5678", "Hlavní nádraží Ostrava", 35600),
    ("Škoda Enyaq 2024", "5T5 9012", "Hlavní nádraží Ostrava", 6400),
    ("Ford Focus 2022", "6T7 3456", "Hlavní nádraží Ostrava", 52100),
    ("Kia Ceed 2023", "7T1 7890", "Hlavní nádraží Ostrava", 17800),

    // Outlet Arena Moravia
    ("Škoda Kamiq 2024", "8T3 2345", "Outlet Arena Moravia", 4500),
    ("Hyundai Tucson 2023", "9T4 6789", "Outlet Arena Moravia", 22700),
    ("Peugeot e-208 2023", "1T5 0123", "Outlet Arena Moravia", 9300),
    ("Dacia Duster 2024", "2T6 4567", "Outlet Arena Moravia", 7100),
    ("Volkswagen ID.4 2024", "3T8 8901", "Outlet Arena Moravia", 5800),
    ("Citroën C3 2022", "4T9 2345", "Outlet Arena Moravia", 38200),

    // Parking Stodolní
    ("BMW 320d 2023", "5T0 6789", "Parking Stodolní", 18900),
    ("Mercedes-Benz A 200 2024", "6T1 0123", "Parking Stodolní", 7600),

    // VŠB-TU Ostrava Poruba
    ("Škoda Scala 2023", "7T2 4567", "VŠB-TU Ostrava Poruba", 21300),
    ("Toyota Corolla 2023", "8T3 8901", "VŠB-TU Ostrava Poruba", 14700),
    ("Volkswagen T-Roc 2024", "9T4 2345", "VŠB-TU Ostrava Poruba", 6200),
    ("Renault Zoe 2023", "1T6 6789", "VŠB-TU Ostrava Poruba", 32400),
    ("Škoda Octavia Combi 2022", "2T7 0123", "VŠB-TU Ostrava Poruba", 48700),
    ("Hyundai Kona Electric 2024", "3T8 4567", "VŠB-TU Ostrava Poruba", 3800),
    ("Seat Leon 2023", "4T9 8901", "VŠB-TU Ostrava Poruba", 16500),
    ("Fiat 500e 2024", "5T0 2345", "VŠB-TU Ostrava Poruba", 5100),

    // Městský stadion Vítkovice
    ("Škoda Karoq 2023", "6T1 6789", "Městský stadion Vítkovice", 27800),
    ("Opel Astra 2023", "7T2 0123", "Městský stadion Vítkovice", 19200),
    ("Mazda CX-30 2024", "8T3 4567", "Městský stadion Vítkovice", 8900),
    ("Nissan Leaf 2023", "9T4 8901", "Městský stadion Vítkovice", 25600),
};

foreach (var v in vehicles)
{
    var lotId = lotIds[v.LotName];
    Execute(connection,
        $"INSERT INTO Vehicles (Model, PlateNumber, CurrentParkingLotId, Status, Odometer) VALUES ('{Esc(v.Model)}', '{Esc(v.Plate)}', {lotId}, 0, {v.Odometer});");
}

// Verify
using (var cmd = connection.CreateCommand())
{
    cmd.CommandText = "SELECT COUNT(*) FROM ParkingLots;";
    Console.WriteLine($"Parking lots inserted: {cmd.ExecuteScalar()}");
}
using (var cmd = connection.CreateCommand())
{
    cmd.CommandText = "SELECT COUNT(*) FROM Vehicles;";
    Console.WriteLine($"Vehicles inserted: {cmd.ExecuteScalar()}");
}

// Summary per lot
using (var cmd = connection.CreateCommand())
{
    cmd.CommandText = "SELECT p.Name, COUNT(v.Id) FROM ParkingLots p LEFT JOIN Vehicles v ON v.CurrentParkingLotId = p.Id GROUP BY p.Id ORDER BY p.Id;";
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        Console.WriteLine($"  {reader.GetString(0)}: {reader.GetInt32(1)} vehicles");
    }
}

connection.Close();
Console.WriteLine("Done.");

static void Execute(SqliteConnection conn, string sql)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    cmd.ExecuteNonQuery();
}

static string Esc(string s) => s.Replace("'", "''");
