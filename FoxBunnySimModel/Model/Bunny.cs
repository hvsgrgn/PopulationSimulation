namespace FoxBunnySimCore.Model;

internal class Bunny : Animal
{
    public Bunny(Position pos, double energy) : base(pos, energy) { }

    public override Species Species => Species.Bunny;

    protected override Cell ChooseTarget(IReadOnlyList<Cell> neighbours, Random random){
        
        List<Cell> free = neighbours.Where(c => c.Occupant == Species.None).ToList();
        List<Cell> grassy = free.Where(c => c.HasGrass).ToList();
        return PickRandom(grassy.Count > 0 ? grassy : free, random);
    }

    protected override bool CanEat(Cell target) => target.HasGrass;
}
