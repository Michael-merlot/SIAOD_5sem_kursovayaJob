using System;
using System.Collections.Generic;
using System.Linq;

class DriverGA
{
    public static int BusCycleTime = 60;
    public static int LunchBreak = 60;
    public static int ShortBreak = 15;
    public static int DriverWorkHours = 8;
    public static int PopulationSize = 20;
    public static int Generations = 100;
    public static Random rnd = new Random();

    public static List<(TimeSpan, TimeSpan)> PeakHours = new List<(TimeSpan, TimeSpan)>
    {
        (new TimeSpan(7, 0, 0), new TimeSpan(9, 0, 0)),
        (new TimeSpan(17, 0, 0), new TimeSpan(19, 0, 0))
    };

    public static List<(int, int)> StartShiftRanges = new List<(int, int)>
    {
        (6, 8),  // Водитель 1
        (7, 9),  // Водитель 2
        (8, 10), // Водитель 3
        (11, 13),// Водитель 4
        (14, 15),// Водитель 5
        (15, 16),// Водитель 6
        (16, 18),// Водитель 7
        (18, 18) // Водитель 8 (строго)
    };

    public static List<TimeSpan> GenerateStartTimes()
    {
        var startTimes = new List<TimeSpan>();
        foreach (var (startHour, endHour) in StartShiftRanges)
        {
            int randomHour = rnd.Next(startHour, endHour + 1);
            int randomMinute = rnd.Next(0, 60);
            startTimes.Add(new TimeSpan(randomHour, randomMinute, 0));
        }
        return startTimes;
    }

    public static List<(TimeSpan, string)> GenerateDriverSchedule(TimeSpan startTime, bool isShortBreak)
    {
        var currentTime = startTime;
        var schedule = new List<(TimeSpan, string)>()
        {
            (currentTime, IsPeakHour(currentTime) ? "Начало смены - Начало маршрута - Час пик" : "Начало смены")
        };
        int totalWorkMinutes = 0;
        bool lunchBreakTaken = false;
        int shortBreakCounter = 0;

        while (totalWorkMinutes < DriverWorkHours * 60)
        {
            if (!isShortBreak && !lunchBreakTaken && totalWorkMinutes >= 4 * 60 && !IsPeakHour(currentTime))
            {
                schedule.Add((currentTime, "Обеденный перерыв (1 час)"));
                currentTime = currentTime.Add(TimeSpan.FromMinutes(LunchBreak));
                totalWorkMinutes += LunchBreak;
                lunchBreakTaken = true;
                continue;
            }
            else if (isShortBreak && shortBreakCounter < 2 && totalWorkMinutes >= (3 + shortBreakCounter * 3) * 60 && !IsPeakHour(currentTime))
            {
                schedule.Add((currentTime, "Краткий перерыв (15 минут)"));
                currentTime = currentTime.Add(TimeSpan.FromMinutes(ShortBreak));
                totalWorkMinutes += ShortBreak;
                shortBreakCounter++;
                continue;
            }

            string action = IsPeakHour(currentTime) ? "Начало маршрута (час пик)" : "Начало маршрута";
            schedule.Add((currentTime, action));
            currentTime = currentTime.Add(TimeSpan.FromMinutes(BusCycleTime));
            totalWorkMinutes += BusCycleTime;
        }
        schedule.Add((currentTime, "Окончание смены"));
        return schedule;
    }

    public static bool IsPeakHour(TimeSpan currentTime)
    {
        foreach (var (start, end) in PeakHours)
        {
            if (currentTime >= start && currentTime < end)
                return true;
        }
        return false;
    }

    public static int Fitness(List<List<(TimeSpan, string)>> schedules)
    {
        int score = 100; // Start with a base positive score
        foreach (var schedule in schedules)
        {
            foreach (var (time, action) in schedule)
            {
                if (action.Contains("Обеденный перерыв") && IsPeakHour(time))
                {
                    score -= 10;
                }
                else if (action.Contains("Начало маршрута") && IsPeakHour(time))
                {
                    score -= 5;
                }
                else
                {
                    score += 2; // Reward for non-peak hour actions
                }
            }
        }
        score += rnd.Next(-5, 6); // Add small randomness to simulate dynamic changes
        return Math.Max(score, 0); // Ensure score is always positive
    }

    static void Main(string[] args)
    {
        var population = new List<List<List<(TimeSpan, string)>>>(PopulationSize);

        for (int i = 0; i < PopulationSize; i++)
        {
            var startTimes = GenerateStartTimes();
            var schedules = new List<List<(TimeSpan, string)>>();

            for (int j = 0; j < startTimes.Count; j++)
            {
                bool isShortBreak = j >= 4;
                schedules.Add(GenerateDriverSchedule(startTimes[j], isShortBreak));
            }

            population.Add(schedules);
        }

        for (int generation = 0; generation < Generations; generation++)
        {
            population = population.OrderByDescending(Fitness).ToList();

            var newPopulation = new List<List<List<(TimeSpan, string)>>> { population[0], population[1] };

            for (int i = 2; i < PopulationSize; i++)
            {
                var parent1 = population[rnd.Next(population.Count)];
                var parent2 = population[rnd.Next(population.Count)];

                var child = new List<List<(TimeSpan, string)>>();
                for (int j = 0; j < parent1.Count; j++)
                {
                    child.Add(rnd.Next(2) == 0 ? parent1[j] : parent2[j]);
                }

                newPopulation.Add(child);
            }

            population = newPopulation;

            Console.WriteLine($"Генерация {generation + 1}: Лучший Фитнес = {Fitness(population[0])}");
        }

        var bestSchedules = population.First();
        Console.WriteLine("\nЛучшее расписание:");

        for (int i = 0; i < bestSchedules.Count; i++)
        {
            Console.WriteLine($"Водитель {i + 1}:");
            foreach (var (time, action) in bestSchedules[i])
            {
                Console.WriteLine($"{time:hh\\:mm} - {action}");
            }
            Console.WriteLine();
        }
    }
}
