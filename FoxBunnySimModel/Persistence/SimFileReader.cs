using System.Globalization;

namespace FoxBunnySimCore.Persistence;

/*
Simulation file (.sim), plain text, values separated by spaces:
n m t interval grassGrowth bunnyReprod foxReprod foodEnergy energyLoss
m lines of: x y energy species      (species: bunny = 0, fox = 1)
n lines of n numbers                (0: no grass, 1: grass)
h                                   (number of history entries)
h lines of: bunnies foxes
*/
public class SimFileReader{
   
    private static readonly char[] Separators = { ' ', '\t' };

    private readonly TextReader _reader;
    private int _lineNumber;

    public SimFileReader(TextReader reader){
        _reader = reader;
    }

    #region Reading the game

    public async Task<GameState> ReadAsync(){
        
        string[] header = await NextFieldsAsync("the header line", 9);
        int size = ParseInt(header[0], "board size");
        int animalCount = ParseInt(header[1], "animal count");
        int gameTime = ParseInt(header[2], "step count");
        RulesData rules = ReadRules(header);

        if (!GameState.AllowedBoardSizes.Contains(size))
            throw Invalid("board size", $"{size} is not one of {string.Join(", ", GameState.AllowedBoardSizes)}");
        if (animalCount < 0 || animalCount > size * size)
            throw Invalid("animal count", $"{animalCount} does not fit on a {size} x {size} board");
        if (gameTime < 0)
            throw Invalid("step count", "it can not be negative");

        List<AnimalData> animals = await ReadAnimalsAsync(size, animalCount);
        bool[,] grass = await ReadGrassAsync(size);
        List<PopulationData> history = await ReadHistoryAsync();

        return new GameState(size, grass, animals, history, gameTime, rules);
    }

    private RulesData ReadRules(string[] header){
        
        RulesData rules = new RulesData(
            ParseDouble(header[3], "step interval"),
            ParseDouble(header[4], "grass regrowth chance"),
            ParseDouble(header[5], "bunny reproduction threshold"),
            ParseDouble(header[6], "fox reproduction threshold"),
            ParseDouble(header[7], "food energy"),
            ParseDouble(header[8], "energy loss"));

        if (rules.Interval <= 0)
            throw Invalid("step interval", "it must be positive");
        if (rules.GrassGrowth < 0 || rules.GrassGrowth > 1)
            throw Invalid("grass regrowth chance", "it must be between 0 and 1");
        if (rules.BunnyReprod <= 0 || rules.FoxReprod <= 0)
            throw Invalid("reproduction threshold", "it must be positive");
        if (rules.FoodEnergy < 0 || rules.EnergyLoss < 0)
            throw Invalid("energy value", "it can not be negative");
        return rules;
    }

    private async Task<List<AnimalData>> ReadAnimalsAsync(int size, int count){
        
        List<AnimalData> animals = new List<AnimalData>();
        HashSet<(int, int)> taken = new HashSet<(int, int)>();

        for (int i = 0; i < count; i++){
            
            string[] fields = await NextFieldsAsync($"animal {i + 1} of {count} (x y energy species)", 4);
            int row = ParseInt(fields[0], "animal row");
            int column = ParseInt(fields[1], "animal column");
            double energy = ParseDouble(fields[2], "animal energy");
            int species = ParseInt(fields[3], "animal species");

            if (row < 0 || row >= size || column < 0 || column >= size)
                throw Invalid("animal position", $"({row}, {column}) is outside the board");
            if (energy <= 0)
                throw Invalid("animal energy", "it must be positive");
            if (species != 0 && species != 1)
                throw Invalid("animal species", $"{species} is neither 0 (bunny) nor 1 (fox)");
            if (!taken.Add((row, column)))
                throw Invalid("animal position", $"({row}, {column}) is already taken by another animal");

            animals.Add(new AnimalData(row, column, energy, species));
        }
        return animals;
    }

    private async Task<bool[,]> ReadGrassAsync(int size){
        
        bool[,] grass = new bool[size, size];
        for (int x = 0; x < size; x++){
            string[] tiles = await NextFieldsAsync($"grass row {x + 1} of {size}", size);
            for (int y = 0; y < size; y++){
                if (tiles[y] != "0" && tiles[y] != "1")
                    throw Invalid("grass tile", $"'{tiles[y]}' is neither 0 (no grass) nor 1 (grass)");
                grass[x, y] = tiles[y] == "1";
            }
        }
        return grass;
    }

    private async Task<List<PopulationData>> ReadHistoryAsync(){
        
        string[] countField = await NextFieldsAsync("the history length", 1);
        int count = ParseInt(countField[0], "history length");
        if (count < 1)
            throw Invalid("history length", "there must be at least one entry");

        List<PopulationData> history = new List<PopulationData>();
        for (int i = 0; i < count; i++){
            string[] fields = await NextFieldsAsync($"history entry {i + 1} of {count} (bunnies foxes)", 2);
            int bunnies = ParseInt(fields[0], "bunny count");
            int foxes = ParseInt(fields[1], "fox count");
            if (bunnies < 0 || foxes < 0)
                throw Invalid("history entry", "the population can not be negative");
            history.Add(new PopulationData(bunnies, foxes));
        }
        return history;
    }

    #endregion

    #region Reading lines and values

    private async Task<string[]> NextFieldsAsync(string expected, int count){
        
        string? line = await _reader.ReadLineAsync();
        _lineNumber++;
        if (line == null)
            throw new GameFileEndedEarlyException(_lineNumber, expected);

        string[] fields = line.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != count)
            throw new GameFileSyntaxException(_lineNumber, $"{expected} ({count} values separated by spaces)", line);
        return fields;
    }

    private int ParseInt(string text, string field){
        
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            throw new GameFileSyntaxException(_lineNumber, $"a whole number for the {field}", text);
        return value;
    }

    private double ParseDouble(string text, string field){
       
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) || !double.IsFinite(value))
            throw new GameFileSyntaxException(_lineNumber, $"a number for the {field}", text);
        return value;
    }

    private GameFileValueException Invalid(string field, string reason)
    {
        return new GameFileValueException(_lineNumber, field, reason);
    }

    #endregion
}
