using FoxBunnySimCore.Persistence;

namespace FoxBunnySimCore.Model;

public class GameModel : IDisposable
{
    #region Fields

    private const double InitialGrassChance = 0.6;

    private static readonly IDataAccess DataAccess = new FileDataAccess();

    private readonly GameBoard _board;
    private readonly TimerAggregation _timer = new TimerAggregation();
    private readonly List<Population> _history;
    private readonly Random _random = new Random();
    private readonly object _stepLock = new object();
    private List<Animal> _animals;
    private SimulationRules _rules;
    private int _gameTime;
    private bool _gameOverRaised;

    #endregion

    #region Events

    public event EventHandler? GameAdvanced;
    public event EventHandler? GameOver;

    #endregion

    #region Properties

    public int GameBoardSize => _board.Size;
    public int GameTime => _gameTime;
    public bool IsTimerRunning => _timer.Enabled;
    public bool IsGameOver => _gameOverRaised;
    public IReadOnlyList<Population> History => _history;

    #endregion

    #region Construction
    private GameModel(GameBoard board, List<Animal> animals, SimulationRules rules, int gameTime, List<Population> history){
       
        _board = board;
        _animals = animals;
        _rules = rules;
        _gameTime = gameTime;
        _history = history;

        foreach (Animal animal in _animals)
            _board.SetOccupant(animal.Pos, animal.Species);

        _timer.Interval = _rules.Interval;
        _timer.Elapsed += Step;

        if (_history.Count == 0)
            _history.Add(CountPopulation());
        else
            UpdateCurrentPopulation();
    }

    public static GameModel CreateNew(SimulationParameters parameters){
        
        Random random = new Random();
        int size = parameters.BoardSize;
        SimulationRules rules = parameters.Rules;

        GameBoard board = new GameBoard(size);
        Position[] cells = new Position[size * size];
        for (int x = 0; x < size; x++){
            for (int y = 0; y < size; y++){
                Position pos = new Position(x, y);
                board.SetGrass(pos, random.NextDouble() < InitialGrassChance);
                cells[x * size + y] = pos;
            }
        }
        //shuffling, so that the specimens arent spawned next to each other filling up the top rows
        random.Shuffle(cells);

        //make sure not too many specimens were handed
        int bunnyCount = Math.Min(parameters.BunnyCount, cells.Length);
        int foxCount = Math.Min(parameters.FoxCount, cells.Length - bunnyCount);
        List<Animal> animals = new List<Animal>();
        animals.AddRange(cells.Take(bunnyCount).Select(pos => Animal.Create(Species.Bunny, pos, rules.ReproductionThreshold(Species.Bunny) / 2)));
        animals.AddRange(cells.Skip(bunnyCount).Take(foxCount).Select(pos => Animal.Create(Species.Fox, pos, rules.ReproductionThreshold(Species.Fox) / 2)));

        return new GameModel(board, animals, rules, 0, new List<Population>());
    }

    public static async Task<GameModel> LoadAsync(string path){
        
        try{
            return FromState(await DataAccess.LoadAsync(path));
        }
        catch (SimFileException ex){
            throw new GameFileException(ex.Message, ex);
        }
    }

    //maps the plain data read from the file onto the model types
    private static GameModel FromState(GameState state){
        
        GameBoard board = new GameBoard(state.BoardSize);
        for (int x = 0; x < state.BoardSize; x++){
            for (int y = 0; y < state.BoardSize; y++)
                board.SetGrass(new Position(x, y), state.Grass[x, y]);
        }

        List<Animal> animals = state.Animals.Select(data => Animal.Create(SpeciesOf(data.Species), new Position(data.Row, data.Column), data.Energy)).ToList();
        SimulationRules rules = new SimulationRules(state.Rules.Interval, state.Rules.GrassGrowth, state.Rules.BunnyReprod, state.Rules.FoxReprod, state.Rules.FoodEnergy, state.Rules.EnergyLoss);
        List<Population> history = state.History.Select(population => new Population(population.Bunnies, population.Foxes)).ToList();

        return new GameModel(board, animals, rules, state.GameTime, history);
    }

    #endregion

    #region Game control

    public void ResumeGame(){
        
        CheckGameOver();
        if (!_gameOverRaised)
            _timer.Start();
    }

    public void PauseGame() => _timer.Stop();

    public void Dispose() => _timer.Dispose();

    #endregion

    #region Simulation step

    public void Step(object? sender, EventArgs e){
        
        //to forego race condition of a user-modified animal set or timeskip-button pressed and ticking
        lock (_stepLock){
            _gameTime++;
            GrowGrass();
            MoveAnimals();
            _history.Add(CountPopulation());
            GameAdvanced?.Invoke(this, EventArgs.Empty);
            CheckGameOver();
        }
    }

    private void GrowGrass(){
        
        for (int x = 0; x < GameBoardSize; x++){
            for (int y = 0; y < GameBoardSize; y++){
                Position pos = new Position(x, y);
                if (!_board[pos].HasGrass && _random.NextDouble() < _rules.GrassGrowth)
                    _board.SetGrass(pos, true);
            }
        }
    }

    private void MoveAnimals(){
        
        //creating a copy so we can iterate and modify at the same time
        List<Animal> next = new List<Animal>(_animals);

        foreach (Animal animal in _animals)
        {
            //if its already dead, skip it
            if (!next.Contains(animal))
                continue;

            Position oldPos = animal.Pos;
            double threshold = _rules.ReproductionThreshold(animal.Species);
            (Animal? offspring, Position? newPos) = animal.Move(GetNeighbours(oldPos), _random, threshold, _rules.EnergyLoss, _rules.FoodEnergy);

            _board.SetOccupant(oldPos, Species.None);

            //if it dies after stepping, remove it
            if (newPos == null){
                next.Remove(animal);
                continue;
            }

            //taking it out of the nullable wrapper
            Position pos = newPos.Value;
            Animal? eaten = next.Find(other => other != animal && other.Pos == pos);
            if (eaten != null)
                next.Remove(eaten);

            _board.SetOccupant(pos, animal.Species);
            if (animal.Species == Species.Bunny)
                _board.SetGrass(pos, false);

            if (offspring != null){
                _board.SetOccupant(offspring.Pos, offspring.Species);
                next.Add(offspring);
            }
        }

        _animals = next;
    }

    private List<Cell> GetNeighbours(Position pos){
        
        List<Cell> cells = new List<Cell>();
        for (int dx = -1; dx <= 1; dx++){
            for (int dy = -1; dy <= 1; dy++){
                Position neighbour = pos.Offset(dx, dy);
                if ((dx != 0 || dy != 0) && _board.Contains(neighbour))
                    cells.Add(_board[neighbour]);
            }
        }
        return cells;
    }

    private void CheckGameOver(){
        
        if (_gameOverRaised)
            return;

        Population population = CountPopulation();
        if (population.Bunnies == 0 || population.Foxes == 0){
            _gameOverRaised = true;
            _timer.Stop();
            GameOver?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region Editing animals

    public bool PlaceAnimal(Position pos, Species species){
        lock (_stepLock){
            if (IsTimerRunning || _gameOverRaised || !_board.Contains(pos) || _board[pos].Occupant != Species.None)
                return false;

            Animal animal = Animal.Create(species, pos, _rules.ReproductionThreshold(species));
            _animals.Add(animal);
            _board.SetOccupant(pos, animal.Species);
            UpdateCurrentPopulation();
            return true;
        }
    }

    public bool RemoveAnimal(int id){
        lock (_stepLock){
            Animal? animal = _animals.Find(candidate => candidate.Id == id);
            if (animal == null || IsTimerRunning || _gameOverRaised)
                return false;

            _animals.Remove(animal);

            _board.SetOccupant(animal.Pos, Species.None);
            UpdateCurrentPopulation();
            CheckGameOver();
            return true;
        }
    }

    public AnimalStatus? FindAnimal(Position pos){
        lock (_stepLock){
            Animal? animal = _animals.Find(candidate => candidate.Pos == pos);
            return animal == null ? null : CreateStatus(animal);
        }
    }

    #endregion

    #region Display queries

    public bool IsGrassAt(Position pos) => _board[pos].HasGrass;

    public bool HasAnimalAt(Position pos) => _board[pos].Occupant != Species.None;

    public List<AnimalStatus> GetAnimalStatuses(){
        lock (_stepLock){
            return _animals.Select(CreateStatus).ToList();
        }
    }

    //null if the animal is not alive anymore
    public AnimalStatus? GetStatus(int id){
        lock (_stepLock){
            Animal? animal = _animals.Find(candidate => candidate.Id == id);
            return animal == null ? null : CreateStatus(animal);
        }
    }

    #endregion

    #region Parameters and state

    public SimulationParameters GetParameters(){
        
        Population current = _history[_history.Count - 1];
        return new SimulationParameters{
            BoardSize = GameBoardSize,
            BunnyCount = current.Bunnies,
            FoxCount = current.Foxes,
            Rules = _rules
        };
    }

    public void ApplyParameters(SimulationParameters parameters){
        
        _rules = parameters.Rules;
        _timer.Interval = _rules.Interval;
    }

    public async Task SaveAsync(string path){
        
        GameState state;
        lock (_stepLock){
            state = CreateState();
        }

        try{
            await DataAccess.SaveAsync(path, state);
        }
        catch (SimFileException ex){
            throw new GameFileException(ex.Message, ex);
        }
    }

    //maps the model onto the plain data the persistence layer writes
    private GameState CreateState(){
        
        bool[,] grass = new bool[GameBoardSize, GameBoardSize];
        for (int x = 0; x < GameBoardSize; x++){
            for (int y = 0; y < GameBoardSize; y++)
                grass[x, y] = _board[new Position(x, y)].HasGrass;
        }

        List<AnimalData> animals = _animals.Select(animal => new AnimalData(animal.Pos.X, animal.Pos.Y, animal.Energy, NumberOf(animal.Species))).ToList();
        RulesData rules = new RulesData(_rules.Interval, _rules.GrassGrowth, _rules.BunnyReprod, _rules.FoxReprod, _rules.FoodEnergy, _rules.EnergyLoss);
        List<PopulationData> history = _history.Select(population => new PopulationData(population.Bunnies, population.Foxes)).ToList();
        return new GameState(GameBoardSize, grass, animals, history, _gameTime, rules);
    }

    #endregion

    #region Helpers

    //the file stores 0 = bunny, 1 = fox, whatever numbers the Species enum has
    private static Species SpeciesOf(int number) => number switch{
        0 => Species.Bunny,
        1 => Species.Fox,
        _ => throw new ArgumentOutOfRangeException(nameof(number))
    };

    private static int NumberOf(Species species) => species == Species.Fox ? 1 : 0;

    private AnimalStatus CreateStatus(Animal animal) =>
         new AnimalStatus(animal.Id, animal.Pos, animal.Species, animal.Energy, _rules.ReproductionThreshold(animal.Species));

    private Population CountPopulation() =>
       new Population(_animals.Count(animal => animal.Species == Species.Bunny), _animals.Count(animal => animal.Species == Species.Fox));

    private void UpdateCurrentPopulation() => _history[_history.Count - 1] = CountPopulation();

    #endregion
}


