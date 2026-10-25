namespace FoxBunnySimCore.Model;

public abstract class Animal{
    #region Properties

    //identifies the animal for the outside world, so nobody has to hold the live object
    private static int _nextId;
    public int Id { get; } = System.Threading.Interlocked.Increment(ref _nextId);
    public Position Pos { get; private set; }
    public double Energy { get; private set; }
    public abstract Species Species { get; }

    #endregion

    #region Construction
    public static Animal Create(Species species, Position pos, double energy) => species == Species.Fox ? new Fox(pos, energy) : new Bunny(pos, energy);
    protected Animal(Position pos, double energy){
       
        Pos = pos;
        Energy = energy;
    }
    #endregion

    #region Behaviour of the species

    //return the target cell if its free, cells with food is preferred
    protected abstract Cell ChooseTarget(IReadOnlyList<Cell> neighbours, Random random);
    protected abstract bool CanEat(Cell target);
    #endregion

    #region Game Cycle
    
    //handles a round of game: eating, reproduction, survival check, movement
    public (Animal? Offspring, Position? NewPos) Move(IReadOnlyList<Cell> neighbours, Random random,
        double reproductionThreshold, double energyLoss, double foodEnergy){
       
        Position oldPos = Pos;
        Cell target = ChooseTarget(neighbours, random);
        Position newPos = target.Pos;

        if (CanEat(target))
            Energy += foodEnergy;

        Energy -= energyLoss;
        if (Energy <= 0)
            return (null, null);

        Pos = newPos;
        Animal? child = Energy > reproductionThreshold ? Reproduce(neighbours, oldPos, random) : null;
        return (child, newPos);
    }

    //if there is space for it, creates a new animal
    private Animal? Reproduce(IReadOnlyList<Cell> neighbours, Position oldPos, Random random){
        
        List<Position> free = neighbours
            .Where(c => c.Occupant == Species.None && c.Pos != Pos)
            .Select(c => c.Pos)
            .ToList();
        if (oldPos != Pos)
            free.Add(oldPos);
        if (free.Count == 0)
            return null;

        Energy /= 2;
        return Create(Species, free[random.Next(free.Count)], Energy);
    }

    //picks a new cell for the offspring
    protected Cell PickRandom(List<Cell> pool, Random random) => pool.Count == 0 ? new Cell(Pos, false, Species.None) : pool[random.Next(pool.Count)];

    #endregion
}
