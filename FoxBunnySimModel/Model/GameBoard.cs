using FoxBunnySimCore.Persistence;

namespace FoxBunnySimCore.Model;

public readonly record struct Position(int X, int Y){
    public Position Offset(int dx, int dy) => new Position(X + dx, Y + dy);
}

public enum Species{
    None = 0,
    Bunny = 1,
    Fox = 2
}

public readonly record struct Cell(Position Pos, bool HasGrass, Species Occupant);

public class GameBoard{
    
    public static readonly int[] AllowedSizes = GameState.AllowedBoardSizes;

    private readonly Cell[,] _map;
    public int Size { get; }

    public GameBoard(int size){
        
        Size = size;
        _map = new Cell[size, size];
        for (int x = 0; x < size; x++){
            for (int y = 0; y < size; y++)
                _map[x, y] = new Cell(new Position(x, y), false, Species.None);
        }
    }

    public Cell this[Position pos] => _map[pos.X, pos.Y];

    public bool Contains(Position pos) => 
        pos.X >= 0 && pos.Y >= 0 && pos.X < Size && pos.Y < Size;

    public void SetGrass(Position pos, bool hasGrass) =>
        _map[pos.X, pos.Y] = _map[pos.X, pos.Y] with { HasGrass = hasGrass };

    public void SetOccupant(Position pos, Species occupant) =>
        _map[pos.X, pos.Y] = _map[pos.X, pos.Y] with { Occupant = occupant };
}
