namespace FoxBunnySimCore.Model;

public readonly record struct AnimalStatus(int Id, Position Pos, Species Species, double Energy, double Threshold){
    public double EnergyRatio => Threshold > 0 ? Math.Clamp(Energy / Threshold, 0, 1) : 0;
}

public readonly record struct Population(int Bunnies, int Foxes);

public readonly record struct SimulationRules(double Interval, double GrassGrowth, double BunnyReprod, double FoxReprod, double FoodEnergy, double EnergyLoss){
    public double ReproductionThreshold(Species species) => species == Species.Fox ? FoxReprod : BunnyReprod;
}
