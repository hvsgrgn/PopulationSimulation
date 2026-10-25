namespace FoxBunnySimCore.Model;

internal class Fox : Animal
{
    public Fox(Position pos, double energy) : base(pos, energy) { }

    public override Species Species => Species.Fox;

    protected override Cell ChooseTarget(IReadOnlyList<Cell> neighbours, Random random){
        
        List<Cell> bunnies = neighbours.Where(c => c.Occupant == Species.Bunny).ToList();
        List<Cell> free = neighbours.Where(c => c.Occupant == Species.None).ToList();
        return PickRandom(bunnies.Count > 0 ? bunnies : free, random);
    }

    protected override bool CanEat(Cell target) => target.Occupant == Species.Bunny;
}
