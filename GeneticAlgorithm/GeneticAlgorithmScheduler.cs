using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

class GeneticAlgorithmScheduler
{
	public static int PopulationSize = 100; // размер популяции
	public static int MaxGenerations = 100; // кол-во поколений
	public static double MutationRate = 0.1; // вер мутации
	public static double CrossoverRate = 0.8; // вер скрещивания

	public static DataTable GenerateSchedule()
	{
		List<List<DriverShift>> population = GenerateInitialPopulation();
		double lastBestFitness = 0;

		for (int generation = 0; generation < MaxGenerations; generation++)
		{
			var evaluatedPopulation = population
				.Select(schedule => (fitness: CalculateFitness(schedule), schedule))
				.OrderByDescending(e => e.fitness)
				.ToList();

			if (generation > 0 && evaluatedPopulation[0].fitness <= lastBestFitness)
			{
				MutationRate = Math.Min(MutationRate + 0.05, 0.3);
				Console.WriteLine($"Увеличение вероятности мутации до {MutationRate * 100:0.00}%");
			}
			lastBestFitness = evaluatedPopulation[0].fitness;

			if (generation % 20 == 0)
			{
				population = GenerateInitialPopulation();
				Console.WriteLine($"Пересчёт популяции в поколении {generation + 1}.");
			}

			var nextGeneration = Select(evaluatedPopulation);

			while (nextGeneration.Count < PopulationSize)
			{
				if (Random.Shared.NextDouble() < CrossoverRate)
				{
					var parents = SelectParents(nextGeneration);
					var offspring = Crossover(parents.Item1, parents.Item2);
					nextGeneration.Add(offspring);
				}
			}

			for (int i = 0; i < nextGeneration.Count; i++)
			{
				if (Random.Shared.NextDouble() < MutationRate)
				{
					nextGeneration[i] = Mutate(nextGeneration[i]);
				}
			}

			population = nextGeneration;

			Console.WriteLine($"Generation {generation + 1}: Best Fitness = {evaluatedPopulation[0].fitness}");
		}

		return ConvertScheduleToDataTable(population.First());
	}

	private static List<List<DriverShift>> GenerateInitialPopulation()
	{
		var population = new List<List<DriverShift>>();
		for (int i = 0; i < PopulationSize; i++)
		{
			population.Add(CreateRandomSchedule());
		}
		return population;
	}

	private static double CalculateFitness(List<DriverShift> schedule)
	{
		int peakCoverage = schedule.Count(shift => DriverShift.IsPeakHour(shift.StartTime));
		int validShifts = schedule.Count(shift => DriverShift.IsValidShift(shift));
		double invalidShiftPenalty = schedule.Count(shift => !DriverShift.IsValidShift(shift)) * 5;
		double overlapPenalty = HasOverlappingShifts(schedule) ? 10 : 0;

		return peakCoverage * 3 + validShifts * 2 - invalidShiftPenalty - overlapPenalty;
	}

	private static List<List<DriverShift>> Select(List<(double fitness, List<DriverShift> schedule)> evaluatedPopulation)
	{
		int numToSelect = PopulationSize / 2;
		return evaluatedPopulation.Take(numToSelect).Select(e => e.schedule).ToList();
	}

	private static (List<DriverShift>, List<DriverShift>) SelectParents(List<List<DriverShift>> population)
	{
		var parent1 = population[Random.Shared.Next(population.Count)];
		var parent2 = population[Random.Shared.Next(population.Count)];
		return (parent1, parent2);
	}

	private static List<DriverShift> Crossover(List<DriverShift> parent1, List<DriverShift> parent2)
	{
		int crossoverPoint = Random.Shared.Next(1, parent1.Count - 1);
		var offspring = new List<DriverShift>();

		offspring.AddRange(parent1.Take(crossoverPoint));
		offspring.AddRange(parent2.Skip(crossoverPoint));

		if (Random.Shared.NextDouble() < 0.3)
		{
			offspring.Add(DriverShift.CreateRandomShift());
		}

		return offspring.Where(DriverShift.IsValidShift).ToList();
	}

	private static List<DriverShift> Mutate(List<DriverShift> schedule)
	{
		int mutationIndex = Random.Shared.Next(schedule.Count);
		var shift = schedule[mutationIndex];

		shift.Action = DriverShift.GenerateAction();
		AdjustDurationBasedOnAction(shift);

		if (!DriverShift.IsValidShift(shift))
		{
			shift.Action = "Начало маршрута";
			AdjustDurationBasedOnAction(shift);
		}

		return schedule;
	}

	private static DataTable ConvertScheduleToDataTable(List<DriverShift> schedule)
	{
		var table = new DataTable();
		for (int i = 1; i <= 8; i++)
		{
			table.Columns.Add($"Водитель {i}");
		}

		int maxShifts = schedule.Count / 8;
		for (int row = 0; row < maxShifts; row++)
		{
			var dataRow = table.NewRow();
			for (int col = 0; col < 8; col++)
			{
				var index = row * 8 + col;
				if (index < schedule.Count)
				{
					var shift = schedule[index];
					if (DriverShift.IsValidShift(shift))
					{
						dataRow[col] = $"{shift.StartTime:HH:mm} - {shift.EndTime:HH:mm}: {shift.Action}";
					}
					else
					{
						dataRow[col] = "Некорректная смена";
					}
				}
				else
				{
					dataRow[col] = "Нет смены";
				}
			}
			table.Rows.Add(dataRow);
		}

		return table;
	}

	private static bool HasOverlappingShifts(List<DriverShift> shifts)
	{
		for (int i = 0; i < shifts.Count; i++)
		{
			for (int j = i + 1; j < shifts.Count; j++)
			{
				if (shifts[i].EndTime > shifts[j].StartTime && shifts[i].StartTime < shifts[j].EndTime)
					return true;
			}
		}
		return false;
	}

	private static List<DriverShift> CreateRandomSchedule()
	{
		var schedule = new List<DriverShift>();
		TimeSpan totalDayDuration = TimeSpan.FromHours(21);
		TimeSpan shiftDuration = TimeSpan.FromHours(totalDayDuration.TotalHours / 8);

		for (int driver = 0; driver < 8; driver++)
		{
			var driverStartTime = DateTime.Today.AddHours(6).Add(shiftDuration * driver);
			var driverEndTime = driverStartTime + shiftDuration;

			while (driverStartTime < driverEndTime)
			{
				var shift = new DriverShift
				{
					StartTime = driverStartTime,
					Action = DriverShift.GenerateAction()
				};
				AdjustDurationBasedOnAction(shift);

				if (DriverShift.IsValidShift(shift))
				{
					schedule.Add(shift);
				}

				driverStartTime = shift.EndTime;
			}
		}
		return schedule;
	}

	public static void AdjustDurationBasedOnAction(DriverShift shift)
	{
		switch (shift.Action)
		{
			case "Краткий перерыв":
				shift.EndTime = shift.StartTime.AddMinutes(15);
				break;
			case "Обеденный перерыв":
				shift.EndTime = shift.StartTime.AddMinutes(60);
				break;
			case "Конец смены":
				shift.EndTime = shift.StartTime.AddMinutes(30);
				break;
			case "Начало маршрута":
				shift.EndTime = shift.StartTime.AddHours(Random.Shared.Next(2, 4));
				break;
		}
	}
}
