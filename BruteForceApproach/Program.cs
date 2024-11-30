using System;
using System.Data;

class Program
{
	static void Main(string[] args)
	{
		var scheduleTable = Scheduler.GenerateFullSchedule();

		Console.WriteLine("Расписание водителей:");
		PrintTable(scheduleTable);
	}

	static void PrintTable(DataTable table)
	{
		foreach (DataColumn column in table.Columns)
		{
			Console.Write($"{column.ColumnName}\t");
		}
		Console.WriteLine();

		foreach (DataRow row in table.Rows)
		{
			foreach (var item in row.ItemArray)
			{
				Console.Write($"{item}\t");
			}
			Console.WriteLine();
		}
	}
}
