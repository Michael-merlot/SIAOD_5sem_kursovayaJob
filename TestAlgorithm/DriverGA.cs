using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

class DriverGA
{
    public static int BusCycleTime = 60;
    public static int LunchBreak = 60;
    public static int ShortBreak = 15;
    public static TimeSpan StartTime = new TimeSpan(6, 0, 0);
    public static TimeSpan EndTime = new TimeSpan(3, 0, 0);
    public static int DriverWorkHours = 8;
    public static int PopulationSize = 20;
    public static int Generations = 100;
    public static Random rnd = new Random();

    public static List<(TimeSpan, TimeSpan)> PeakHours = new List<(TimeSpan, TimeSpan)>
    {
        (new TimeSpan(7, 0, 0), new TimeSpan(9, 0, 0)),
        (new TimeSpan(17, 0, 0), new TimeSpan(19, 0, 0))
    };

    public static List<(TimeSpan, string)> GenerateDriverSchedule(TimeSpan startTime, bool isShortBreak)
    {
        var currentTime = startTime;
        var schedule = new List<(TimeSpan, string)>();
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

    static void Main(string[] args)
    {
        var startTimes = new List<TimeSpan>
        {
            new TimeSpan(6, rnd.Next(1, 10), 0),
            new TimeSpan(7, rnd.Next(1, 10), 0),
            new TimeSpan(8, rnd.Next(1, 10), 0),
            new TimeSpan(11, rnd.Next(1, 10), 0),
            new TimeSpan(13, rnd.Next(1, 10), 0),
            new TimeSpan(15, rnd.Next(1, 10), 0),
            new TimeSpan(17, rnd.Next(1, 10), 0),
            new TimeSpan(18, rnd.Next(1, 10), 0)
        };

        for (int i = 0; i < startTimes.Count; i++)
        {
            bool isShortBreak = i >= 4;
            var schedule = GenerateDriverSchedule(startTimes[i], isShortBreak);
            Console.WriteLine($"Водитель {i + 1}:");
            foreach (var (time, action) in schedule)
            {
                Console.WriteLine($"{time:hh\\:mm} - {action}");
            }
            Console.WriteLine();
        }
    }
}
