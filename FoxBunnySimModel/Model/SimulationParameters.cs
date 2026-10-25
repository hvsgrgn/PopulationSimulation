using FoxBunnySimCore.Persistence;

namespace FoxBunnySimCore.Model;

public class SimulationParameters
{
    public int BoardSize { get; set; } = 20;
    public int BunnyCount { get; set; } = 80;
    public int FoxCount { get; set; } = 5;
    public SimulationRules Rules { get; set; } = new SimulationRules(
        Interval: 500, GrassGrowth: 0.1, BunnyReprod: 10, FoxReprod: 20, FoodEnergy: 3, EnergyLoss: 1);
}
