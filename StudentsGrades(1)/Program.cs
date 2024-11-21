using Microsoft.Data.SqlClient;
using System;

class Program
{
    static string connectionString = "Data Source=DESKTOP-9K56BQI\\SQLEXPRESS;Initial Catalog=studentsDB;Integrated Security=True;TrustServerCertificate=True";

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Меню:");
            Console.ResetColor();
            Console.WriteLine("1 - Показати всі дані");
            Console.WriteLine("2 - Показати студентів з оцінкою вище заданої");
            Console.WriteLine("3 - Показати унікальні предмети з мінімальними оцінками");
            Console.WriteLine("4 - Показати мінімальну та максимальну середню оцінку");
            Console.WriteLine("5 - Показати кількість студентів з оцінками по математиці");
            Console.WriteLine("6 - Показати статистику по групах");
            Console.WriteLine("exit - Вихід з програми");
            Console.Write("\nВиберіть опцію: ");
            string choice = Console.ReadLine()?.ToLower();

            if (choice == "exit")
            {
                break;
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    switch (choice)
                    {
                        case "1":
                            ShowAllData(connection);
                            break;
                        case "2":
                            Console.Write("\nВведіть мінімальну оцінку: ");
                            if (decimal.TryParse(Console.ReadLine(), out decimal minGrade))
                            {
                                ShowStudentsWithMinGradeGreaterThan(connection, minGrade);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Помилка: введено не число.");
                                Console.ResetColor();
                            }
                            break;
                        case "3":
                            ShowSubjectsWithMinGrades(connection);
                            break;
                        case "4":
                            ShowMinMaxAverageGrade(connection);
                            break;
                        case "5":
                            ShowMathGradesCount(connection);
                            break;
                        case "6":
                            ShowGroupStatistics(connection);
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                            Console.ResetColor();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка при підключенні до бази даних: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
            Console.ReadKey();
        }
    }

    static void ShowAllData(SqlConnection connection)
    {
        string query = "SELECT full_name, group_name, avg_grade, min_subject_name, max_subject_name FROM StudentsRating;";
        var command = new SqlCommand(query, connection);
        var reader = command.ExecuteReader();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n===== Всі дані =====");
        Console.ResetColor();
        while (reader.Read())
        {
            Console.WriteLine($"Студент: {reader["full_name"]}, Група: {reader["group_name"]}, Середня оцінка: {reader["avg_grade"]}, Мін. предмет: {reader["min_subject_name"]}, Макс. предмет: {reader["max_subject_name"]}");
            Console.WriteLine("-----------------------------");
        }
    }

    static void ShowStudentsWithMinGradeGreaterThan(SqlConnection connection, decimal minGrade)
    {
        string query = "SELECT full_name FROM StudentsRating WHERE avg_grade > @minGrade;";
        var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@minGrade", minGrade);
        var reader = command.ExecuteReader();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nСтуденти з оцінкою вище {minGrade}:");
        Console.ResetColor();
        while (reader.Read())
        {
            Console.WriteLine(reader["full_name"]);
            Console.WriteLine("-----------------------------");
        }
    }

    static void ShowSubjectsWithMinGrades(SqlConnection connection)
    {
        string query = "SELECT DISTINCT min_subject_name FROM StudentsRating;";
        var command = new SqlCommand(query, connection);
        var reader = command.ExecuteReader();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\nУнікальні предмети з мінімальною оцінкою:");
        Console.ResetColor();
        while (reader.Read())
        {
            Console.WriteLine(reader["min_subject_name"]);
            Console.WriteLine("-----------------------------");
        }
    }

    static void ShowMinMaxAverageGrade(SqlConnection connection)
    {
        string queryMin = "SELECT MIN(avg_grade) FROM StudentsRating;";
        string queryMax = "SELECT MAX(avg_grade) FROM StudentsRating;";

        var commandMin = new SqlCommand(queryMin, connection);
        var commandMax = new SqlCommand(queryMax, connection);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nМінімальна середня оцінка: {commandMin.ExecuteScalar()}");
        Console.WriteLine($"Максимальна середня оцінка: {commandMax.ExecuteScalar()}");
        Console.ResetColor();
        Console.WriteLine("-----------------------------");
    }

    static void ShowMathGradesCount(SqlConnection connection)
    {
        string queryMinMath = "SELECT COUNT(*) FROM StudentsRating WHERE min_subject_name = 'Математика';";
        string queryMaxMath = "SELECT COUNT(*) FROM StudentsRating WHERE max_subject_name = 'Математика';";

        var commandMinMath = new SqlCommand(queryMinMath, connection);
        var commandMaxMath = new SqlCommand(queryMaxMath, connection);

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"\nКількість студентів з мінімальною оцінкою по математиці: {commandMinMath.ExecuteScalar()}");
        Console.WriteLine($"Кількість студентів з максимальною оцінкою по математиці: {commandMaxMath.ExecuteScalar()}");
        Console.ResetColor();
        Console.WriteLine("-----------------------------");
    }

    static void ShowGroupStatistics(SqlConnection connection)
    {
        string query = "SELECT group_name, COUNT(*) AS student_count, AVG(avg_grade) AS group_avg_grade FROM StudentsRating GROUP BY group_name;";
        var command = new SqlCommand(query, connection);
        var reader = command.ExecuteReader();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nСтатистика по групах:");
        Console.ResetColor();
        while (reader.Read())
        {
            Console.WriteLine($"Група: {reader["group_name"]}, Кількість студентів: {reader["student_count"]}, Середня оцінка групи: {reader["group_avg_grade"]}");
            Console.WriteLine("-----------------------------");
        }
    }
}
