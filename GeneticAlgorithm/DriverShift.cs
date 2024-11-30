using System;

class DriverShift
{
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public string Action { get; set; }

	public static (TimeSpan, TimeSpan)[] PeakHours =
	{
		(new TimeSpan(7, 0, 0), new TimeSpan(9, 0, 0)),
		(new TimeSpan(17, 0, 0), new TimeSpan(19, 0, 0))
	};

	public static bool IsPeakHour(DateTime time)
	{
		foreach (var (start, end) in PeakHours)
		{
			if (time.TimeOfDay >= start && time.TimeOfDay < end)
				return true;
		}
		return false;
	}

	public static string GenerateAction()
	{
		string[] actions = { "Начало маршрута", "Краткий перерыв", "Обеденный перерыв", "Конец смены" };
		return actions[Random.Shared.Next(actions.Length)];
	}

	public static DriverShift CreateRandomShift()
	{
		var startTime = DateTime.Today.AddHours(Random.Shared.Next(6, 27));
		var action = GenerateAction();

		var shift = new DriverShift
		{
			StartTime = startTime,
			Action = action
		};

		GeneticAlgorithmScheduler.AdjustDurationBasedOnAction(shift);
		return shift;
	}

	public static bool IsValidShift(DriverShift shift)
	{
		if (shift.EndTime <= shift.StartTime || shift.StartTime.Hour < 6 && shift.EndTime.Hour > 3)
			return false;

		return shift.Action switch
		{
			"Краткий перерыв" => (shift.EndTime - shift.StartTime).TotalMinutes == 15,
			"Обеденный перерыв" => (shift.EndTime - shift.StartTime).TotalMinutes == 60,
			"Конец смены" => (shift.StartTime.Hour >= 16 && (shift.EndTime - shift.StartTime).TotalMinutes <= 30),
			"Начало маршрута" => (shift.EndTime - shift.StartTime).TotalHours >= 2,
			_ => false
		};
	}
}
