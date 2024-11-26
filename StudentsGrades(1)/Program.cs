using Microsoft.Data.SqlClient;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    static string connectionString;

    static async Task Main(string[] args)
    {
        // Отримуємо рядок підключення з конфігураційного файлу
        connectionString = ConfigurationManager.ConnectionStrings["StudentsDb"]?.ConnectionString;

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Не вдалося знайти рядок підключення в конфігураційному файлі.");
            Console.ResetColor();
            return;
        }

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
            Console.WriteLine("7 - Оновити оцінку студента");
            Console.WriteLine("8 - Видалити студента");
            Console.WriteLine("9 - Змінити СКБД");
            Console.WriteLine("exit - Вихід з програми");
            Console.Write("\nВиберіть опцію: ");
            string choice = Console.ReadLine()?.ToLower();

            if (choice == "exit")
            {
                break;
            }

            try
            {
                // Зміна СКБД
                if (choice == "9")
                {
                    await ChangeDatabaseConnectionStringAsync();
                    continue;
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    switch (choice)
                    {
                        case "1":
                            await ShowAllDataAsync(connection);
                            break;
                        case "2":
                            Console.Write("\nВведіть мінімальну оцінку: ");
                            if (decimal.TryParse(Console.ReadLine(), out decimal minGrade))
                            {
                                await ShowStudentsWithMinGradeGreaterThanAsync(connection, minGrade);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Помилка: введено не число.");
                                Console.ResetColor();
                            }
                            break;
                        case "3":
                            await ShowSubjectsWithMinGradesAsync(connection);
                            break;
                        case "4":
                            await ShowMinMaxAverageGradeAsync(connection);
                            break;
                        case "5":
                            await ShowMathGradesCountAsync(connection);
                            break;
                        case "6":
                            await ShowGroupStatisticsAsync(connection);
                            break;
                        case "7":
                            await UpdateStudentGradeAsync(connection);  // Оновлення оцінки
                            break;
                        case "8":
                            await DeleteStudentAsync(connection); // Видалення студента
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

    static async Task ShowAllDataAsync(SqlConnection connection)
    {
        string query = "SELECT full_name, group_name, avg_grade, min_subject_name, max_subject_name FROM StudentsRating;";
        var command = new SqlCommand(query, connection);

        Stopwatch stopwatch = Stopwatch.StartNew(); // Запуск лічильника часу
        var reader = await command.ExecuteReaderAsync();
        stopwatch.Stop();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n===== Всі дані =====");
        Console.ResetColor();
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"Студент: {reader["full_name"]}, Група: {reader["group_name"]}, Середня оцінка: {reader["avg_grade"]}, Мін. предмет: {reader["min_subject_name"]}, Макс. предмет: {reader["max_subject_name"]}");
            Console.WriteLine("-----------------------------");
        }
        Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
    }

    static async Task ShowStudentsWithMinGradeGreaterThanAsync(SqlConnection connection, decimal minGrade)
    {
        string query = "SELECT full_name FROM StudentsRating WHERE avg_grade > @minGrade;";
        var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@minGrade", minGrade);

        Stopwatch stopwatch = Stopwatch.StartNew();
        var reader = await command.ExecuteReaderAsync();
        stopwatch.Stop();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nСтуденти з оцінкою вище {minGrade}:");
        Console.ResetColor();
        while (await reader.ReadAsync())
        {
            Console.WriteLine(reader["full_name"]);
            Console.WriteLine("-----------------------------");
        }
        Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
    }

    static async Task ShowSubjectsWithMinGradesAsync(SqlConnection connection)
    {
        string query = "SELECT DISTINCT min_subject_name FROM StudentsRating;";
        var command = new SqlCommand(query, connection);

        Stopwatch stopwatch = Stopwatch.StartNew();
        var reader = await command.ExecuteReaderAsync();
        stopwatch.Stop();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\nУнікальні предмети з мінімальною оцінкою:");
        Console.ResetColor();
        while (await reader.ReadAsync())
        {
            Console.WriteLine(reader["min_subject_name"]);
            Console.WriteLine("-----------------------------");
        }
        Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
    }

    static async Task ShowMinMaxAverageGradeAsync(SqlConnection connection)
    {
        string queryMin = "SELECT MIN(avg_grade) FROM StudentsRating;";
        string queryMax = "SELECT MAX(avg_grade) FROM StudentsRating;";

        var commandMin = new SqlCommand(queryMin, connection);
        var commandMax = new SqlCommand(queryMax, connection);

        Stopwatch stopwatch = Stopwatch.StartNew();
        var minGrade = await commandMin.ExecuteScalarAsync();
        var maxGrade = await commandMax.ExecuteScalarAsync();
        stopwatch.Stop();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nМінімальна середня оцінка: {minGrade}");
        Console.WriteLine($"Максимальна середня оцінка: {maxGrade}");
        Console.ResetColor();
        Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
    }

    static async Task ShowMathGradesCountAsync(SqlConnection connection)
    {
        string queryMinMath = "SELECT COUNT(*) FROM StudentsRating WHERE min_subject_name = 'Математика';";
        string queryMaxMath = "SELECT COUNT(*) FROM StudentsRating WHERE max_subject_name = 'Математика';";

        var commandMinMath = new SqlCommand(queryMinMath, connection);
        var commandMaxMath = new SqlCommand(queryMaxMath, connection);

        Stopwatch stopwatch = Stopwatch.StartNew();
        var minCount = await commandMinMath.ExecuteScalarAsync();
        var maxCount = await commandMaxMath.ExecuteScalarAsync();
        stopwatch.Stop();

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"\nКількість студентів з мінімальною оцінкою по математиці: {minCount}");
        Console.WriteLine($"Кількість студентів з максимальною оцінкою по математиці: {maxCount}");
        Console.ResetColor();
        Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
    }

    static async Task ShowGroupStatisticsAsync(SqlConnection connection)
    {
        string query = "SELECT group_name, COUNT(*) AS student_count, AVG(avg_grade) AS avg_group_grade FROM StudentsRating GROUP BY group_name;";
        var command = new SqlCommand(query, connection);

        Stopwatch stopwatch = Stopwatch.StartNew();
        var reader = await command.ExecuteReaderAsync();
        stopwatch.Stop();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nСтатистика по групах:");
        Console.ResetColor();
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"Група: {reader["group_name"]}, Кількість студентів: {reader["student_count"]}, Середня оцінка групи: {reader["avg_group_grade"]}");
            Console.WriteLine("-----------------------------");
        }
        Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
    }

    static async Task UpdateStudentGradeAsync(SqlConnection connection)
    {
        Console.Write("\nВведіть ім'я студента для оновлення: ");
        string studentName = Console.ReadLine();

        Console.Write("Введіть нову середню оцінку: ");
        if (decimal.TryParse(Console.ReadLine(), out decimal newGrade))
        {
            string query = "UPDATE StudentsRating SET avg_grade = @newGrade WHERE full_name = @studentName;";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@newGrade", newGrade);
            command.Parameters.AddWithValue("@studentName", studentName);

            Stopwatch stopwatch = Stopwatch.StartNew();
            int rowsAffected = await command.ExecuteNonQueryAsync();
            stopwatch.Stop();

            if (rowsAffected > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Оцінка студента {studentName} успішно оновлена.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Студента з таким ім'ям не знайдено.");
            }
            Console.ResetColor();
            Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Помилка: введена неправильна оцінка.");
            Console.ResetColor();
        }
    }

    static async Task DeleteStudentAsync(SqlConnection connection)
    {
        Console.Write("\nВведіть ім'я студента для видалення: ");
        string studentName = Console.ReadLine();

        string query = "DELETE FROM StudentsRating WHERE full_name = @studentName;";
        var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@studentName", studentName);

        Stopwatch stopwatch = Stopwatch.StartNew();
        int rowsAffected = await command.ExecuteNonQueryAsync();
        stopwatch.Stop();

        if (rowsAffected > 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Студент {studentName} успішно видалений.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Студента з таким ім'ям не знайдено.");
        }
        Console.ResetColor();
        Console.WriteLine($"Час виконання запиту: {stopwatch.Elapsed.TotalSeconds} секунд.");
    }

    static async Task ChangeDatabaseConnectionStringAsync()
    {
        Console.WriteLine("Оберіть тип СКБД:");
        Console.WriteLine("1 - SQL Server");

        string choice = Console.ReadLine();

        if (choice == "1")
        {
            // Заміна на SQL Server
            connectionString = ConfigurationManager.ConnectionStrings["SqlServerDb"]?.ConnectionString;
            Console.WriteLine("СКБД змінено на SQL Server.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Некоректний вибір.");
            Console.ResetColor();
        }
    }
}
