namespace FoxBunnySimCore.Persistence;

public class FileDataAccess : IDataAccess{
    public async Task<GameState> LoadAsync(string path){
        
        try{
            using (StreamReader reader = new StreamReader(path)){
                return await new SimFileReader(reader).ReadAsync();
            }
        }
        catch (Exception ex) when (IsAccessError(ex)){
            throw new GameFileAccessException(path, "read", ex);
        }
    }

    public async Task SaveAsync(string path, GameState state){
        
        try{
            using (StreamWriter writer = new StreamWriter(path))
                await SimFileWriter.WriteAsync(writer, state);
        }
        catch (Exception ex) when (IsAccessError(ex)){
            throw new GameFileAccessException(path, "write", ex);
        }
    }

    private static bool IsAccessError(Exception ex) =>
        ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException;

}
