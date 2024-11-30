using System;
using System.Collections.Generic;
using System.Data;

class Scheduler
{
	public static DataTable GenerateFullSchedule()
	{
		var table = new DataTable();

		var startTimes = new List<DateTime>
		{
			new DateTime(2024, 10, 13, 6, 0, 0),
			new DateTime(2024, 10, 13, 6, 0, 0),
			new DateTime(2024, 10, 13, 7, 0, 0),
			new DateTime(2024, 10, 13, 13, 0, 0),
			new DateTime(2024, 10, 13, 15, 0, 0),
			new DateTime(2024, 10, 13, 15, 0, 0),
			new DateTime(2024, 10, 13, 17, 0, 0),
			new DateTime(2024, 10, 13, 19, 0, 0)
		};

		for (int i = 0; i < startTimes.Count; i++)
		{
			table.Columns.Add($"Водитель {i + 1}");
		}

		int maxRows = 0;
		var schedules = new List<List<string>>();

		for (int i = 0; i < startTimes.Count; i++)
		{
			bool isShortBreaks = i % 2 != 0;
			var driverSchedule = DriverSchedule.CreateDriverSchedule(i, startTimes[i], isShortBreaks);

			var driverActions = new List<string>();
			foreach (var action in driverSchedule)
			{
				driverActions.Add($"{action["Время"]}: {action["Действие"]}");
			}

			schedules.Add(driverActions);
			maxRows = Math.Max(maxRows, driverActions.Count);
		}

		for (int row = 0; row < maxRows; row++)
		{
			var dataRow = table.NewRow();
			for (int col = 0; col < schedules.Count; col++)
			{
				dataRow[col] = row < schedules[col].Count ? schedules[col][row] : "";
			}
			table.Rows.Add(dataRow);
		}

		return table;
	}
}
