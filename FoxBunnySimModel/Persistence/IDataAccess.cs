namespace FoxBunnySimCore.Persistence;

public interface IDataAccess{
    Task<GameState> LoadAsync(string path);
    Task SaveAsync(string path, GameState state);
}
