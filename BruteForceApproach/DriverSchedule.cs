using System;
using System.Collections.Generic;

class DriverSchedule
{
	public static int BusCycleTime = 60;
	public static int ShiftChangeTime = 15;
	public static int LunchBreak = 60;
	public static int ShortBreak = 15;
	public static TimeSpan StartTime = new TimeSpan(6, 0, 0);
	public static TimeSpan EndTime = new TimeSpan(3, 0, 0);
	public static int DriverWorkHours = 8;

	public static List<(TimeSpan, TimeSpan)> PeakHours = new List<(TimeSpan, TimeSpan)>
	{
		(new TimeSpan(7, 0, 0), new TimeSpan(9, 0, 0)),
		(new TimeSpan(17, 0, 0), new TimeSpan(19, 0, 0))
	};

	public static bool IsPeakHour(TimeSpan currentTime)
	{
		foreach (var (start, end) in PeakHours)
		{
			if (currentTime >= start && currentTime < end)
				return true;
		}
		return false;
	}

	public static List<Dictionary<string, string>> CreateDriverSchedule(int driverId, DateTime startTime, bool isShortBreaks)
	{
		var schedule = new List<Dictionary<string, string>>();
		var currentTime = startTime;
		int totalWorkedHours = 0;

		while (totalWorkedHours < DriverWorkHours)
		{
			schedule.Add(new Dictionary<string, string>
			{
				{ "Время", currentTime.ToString("HH:mm") },
				{ "Действие", "Начало маршрута" }
			});
			currentTime = currentTime.AddMinutes(BusCycleTime);
			totalWorkedHours++;

			if (!isShortBreaks && totalWorkedHours == 4)
			{
				schedule.Add(new Dictionary<string, string>
				{
					{ "Время", currentTime.ToString("HH:mm") },
					{ "Действие", "Обеденный перерыв (1 час)" }
				});
				currentTime = currentTime.AddMinutes(LunchBreak);
			}
			else if (isShortBreaks && totalWorkedHours % 2 == 0)
			{
				schedule.Add(new Dictionary<string, string>
				{
					{ "Время", currentTime.ToString("HH:mm") },
					{ "Действие", "Краткий перерыв (15 минут)" }
				});
				currentTime = currentTime.AddMinutes(ShortBreak);
			}
		}

		schedule.Add(new Dictionary<string, string>
		{
			{ "Время", currentTime.ToString("HH:mm") },
			{ "Действие", "Окончание смены" }
		});

		return schedule;
	}
}
