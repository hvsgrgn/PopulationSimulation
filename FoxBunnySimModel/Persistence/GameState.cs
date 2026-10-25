namespace FoxBunnySimCore.Persistence;

//species is stored as the number used in the file: 0 = bunny, 1 = fox
public readonly record struct AnimalData(int Row, int Column, double Energy, int Species);

public readonly record struct PopulationData(int Bunnies, int Foxes);

public readonly record struct RulesData(double Interval, double GrassGrowth, double BunnyReprod, double FoxReprod, double FoodEnergy, double EnergyLoss);

public readonly record struct GameState(int BoardSize, bool[,] Grass, List<AnimalData> Animals, List<PopulationData> History, int GameTime, RulesData Rules)
{
    public static readonly int[] AllowedBoardSizes = { 10, 20, 30 };
}