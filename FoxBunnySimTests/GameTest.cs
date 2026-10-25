using FoxBunnySimCore.Model;
using FoxBunnySimCore.Persistence;

namespace FoxBunnySimTest;

[TestClass]
public class GameModelTest{
   
    //species numbers as they are stored in the file
    private const int BunnyNumber = 0;
    private const int FoxNumber = 1;
    private const int Size = 10;

    private readonly List<string> _files = new List<string>();
    private GameModel? _model;

    [TestCleanup]
    public void Cleanup(){

        _model?.Dispose();
        foreach (string file in _files)
            File.Delete(file);
    }

    #region Helpers

    //no grass growth and thresholds too high to reproduce unless a test says otherwise,
    //the long interval makes sure the real timer never ticks during a test
    private static RulesData Rules(double grassGrowth = 0, double bunnyReprod = 100, double foodEnergy = 3) =>
        new RulesData(60000, grassGrowth, bunnyReprod, 100, foodEnergy, 1);

    private static bool[,] Grass(bool value){

        bool[,] grass = new bool[Size, Size];
        for (int x = 0; x < Size; x++){
            for (int y = 0; y < Size; y++)
                grass[x, y] = value;
        }
        return grass;
    }

    //the model loads from files, so the test game is written into a temporary file with the original writer
    private async Task<string> WriteFile(GameState state){

        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sim");
        _files.Add(path);
        using (StreamWriter writer = new StreamWriter(path))
            await SimFileWriter.WriteAsync(writer, state);
        return path;
    }

    private async Task<GameModel> LoadModel(List<AnimalData> animals, RulesData rules, bool[,]? grass = null, int gameTime = 0){

        //the file needs at least one history entry
        List<PopulationData> history = new List<PopulationData> { new PopulationData(animals.Count(animal => animal.Species == BunnyNumber), animals.Count(animal => animal.Species == FoxNumber)) };
        string path = await WriteFile(new GameState(Size, grass ?? Grass(false), animals, history, gameTime, rules));
        _model = await GameModel.LoadAsync(path);
        return _model;
    }

    //a bunny and a fox in opposite corners, so nothing can happen between them in a single step
    private static List<AnimalData> FarApart(double bunnyEnergy = 5, double foxEnergy = 5) =>
        new List<AnimalData>{
            new AnimalData(0, 0, bunnyEnergy, BunnyNumber),
            new AnimalData(9, 9, foxEnergy, FoxNumber)
        };

    private static AnimalStatus StatusOf(GameModel model, Species species) =>
        model.GetAnimalStatuses().Single(status => status.Species == species);

    private static Population CurrentPopulation(GameModel model) => model.History[model.History.Count - 1];

    #endregion

    [TestMethod]
    //new game: the requested animals are on the board, none of them shares a cell
    public void GameModelNewGameTest(){

        _model = GameModel.CreateNew(new SimulationParameters { BoardSize = 10, BunnyCount = 12, FoxCount = 3 });

        Assert.AreEqual(10, _model.GameBoardSize);
        Assert.AreEqual(0, _model.GameTime);
        Assert.AreEqual(new Population(12, 3), CurrentPopulation(_model));
        Assert.AreEqual(15, _model.GetAnimalStatuses().Select(animal => animal.Pos).Distinct().Count());
    }

    [TestMethod]
    //the numbers of the file (0 bunny, 1 fox) have to become the right species
    public async Task GameModelLoadTest(){

        bool[,] grass = Grass(false);
        grass[1, 2] = true;
        GameModel model = await LoadModel(FarApart(), Rules(), grass, 12);

        Assert.AreEqual(12, model.GameTime);
        Assert.AreEqual(Size, model.GameBoardSize);
        Assert.AreEqual(new Population(1, 1), CurrentPopulation(model));
        Assert.AreEqual(new Position(0, 0), StatusOf(model, Species.Bunny).Pos);
        Assert.AreEqual(new Position(9, 9), StatusOf(model, Species.Fox).Pos);
        Assert.IsTrue(model.IsGrassAt(new Position(1, 2)));
        Assert.IsFalse(model.IsGrassAt(new Position(2, 1)), "Row and column must not be swapped");
    }

    [TestMethod]
    //the view must only ever see the model's own exception
    public async Task GameModelLoadErrorTest(){

        //species 7 does not exist in the file format
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sim");
        _files.Add(path);
        File.WriteAllText(path, "10 1 0 500 0.1 10 20 3 1\n0 0 5 7\n");

        GameFileException? caught = null;
        try{
            await GameModel.LoadAsync(path);
        }
        catch (GameFileException ex){
            caught = ex;
        }

        Assert.IsNotNull(caught, "A GameFileException should have been thrown");
        Assert.IsTrue(caught!.InnerException is GameFileValueException, "The reason of the persistence layer is kept");
    }

    [TestMethod]
    //a bunny has to be saved as 0 and a fox as 1
    public async Task GameModelSaveTest(){

        GameModel model = await LoadModel(FarApart(4, 6), Rules(), null, 3);
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sim");
        _files.Add(path);

        await model.SaveAsync(path);

        //reading the file back with the original reader shows what was written
        GameState saved = await new SimFileReader(new StringReader(File.ReadAllText(path))).ReadAsync();
        Assert.AreEqual(3, saved.GameTime);
        Assert.AreEqual(2, saved.Animals.Count);
        Assert.IsTrue(saved.Animals.Contains(new AnimalData(0, 0, 4, BunnyNumber)));
        Assert.IsTrue(saved.Animals.Contains(new AnimalData(9, 9, 6, FoxNumber)));
    }

    [TestMethod]
    //one step: time, history, event, and what the animals pay for moving
    public async Task GameModelStepTest(){

        GameModel model = await LoadModel(FarApart(5, 8), Rules());
        int advanced = 0;
        model.GameAdvanced += (sender, e) => advanced++;

        model.Step(null, EventArgs.Empty);

        Assert.AreEqual(1, model.GameTime);
        Assert.AreEqual(2, model.History.Count);
        Assert.AreEqual(1, advanced, "GameAdvanced should be raised once per step");
        Assert.AreEqual(4, StatusOf(model, Species.Bunny).Energy, "5 - 1 energy loss, there was no food");
        Assert.AreEqual(7, StatusOf(model, Species.Fox).Energy);
    }

    [TestMethod]
    //eating: bunnies eat grass, foxes eat bunnies
    public async Task GameModelEatingTest(){

        //test 1: the bunny steps onto grass and eats it
        List<AnimalData> grassy = new List<AnimalData>{
            new AnimalData(1, 1, 5, BunnyNumber),
            new AnimalData(9, 9, 5, FoxNumber)
        };

        GameModel model = await LoadModel(grassy, Rules(), Grass(true));
        model.Step(null, EventArgs.Empty);

        AnimalStatus bunny = StatusOf(model, Species.Bunny);
        Assert.AreEqual(7, bunny.Energy, "5 + 3 food - 1 loss");
        Assert.IsFalse(model.IsGrassAt(bunny.Pos), "The eaten grass is gone");

        //test 2: the fox is first in the list, so it moves before the bunnies can run away
        List<AnimalData> hunt = new List<AnimalData>{
            new AnimalData(0, 0, 5, FoxNumber),
            new AnimalData(0, 1, 5, BunnyNumber),
            new AnimalData(9, 9, 5, BunnyNumber)
        };

        model = await LoadModel(hunt, Rules());
        model.Step(null, EventArgs.Empty);

        Assert.AreEqual(new Population(1, 1), CurrentPopulation(model));
        Assert.AreEqual(new Position(0, 1), StatusOf(model, Species.Fox).Pos, "The fox moved onto the bunny");
        Assert.AreEqual(7, StatusOf(model, Species.Fox).Energy);
    }

    [TestMethod]
    //above the threshold the energy is split between the parent and the child
    public async Task GameModelReproductionTest(){

        GameModel model = await LoadModel(FarApart(10, 5), Rules(0, 5));

        model.Step(null, EventArgs.Empty);

        List<AnimalStatus> bunnies = model.GetAnimalStatuses().Where(status => status.Species == Species.Bunny).ToList();
        Assert.AreEqual(2, bunnies.Count);
        Assert.AreEqual(4.5, bunnies[0].Energy);
        Assert.AreEqual(4.5, bunnies[1].Energy);
        Assert.AreNotEqual(bunnies[0].Pos, bunnies[1].Pos);
    }

    [TestMethod]
    public async Task GameModelGameOverTest(){

        //the bunny has 1 energy, it starves in the first step
        GameModel model = await LoadModel(FarApart(1, 5), Rules());
        int gameOver = 0;
        model.GameOver += (sender, e) => gameOver++;

        model.Step(null, EventArgs.Empty);

        Assert.AreEqual(new Population(0, 1), CurrentPopulation(model));
        Assert.IsTrue(model.IsGameOver);
        Assert.AreEqual(1, gameOver);

        model.Step(null, EventArgs.Empty);
        Assert.AreEqual(1, gameOver, "GameOver should only be raised once");
    }

    [TestMethod]
    public async Task GameModelEditAnimalsTest(){

        GameModel model = await LoadModel(FarApart(), Rules());

        //test 1: free cell yes, occupied or outside cell no
        Assert.IsTrue(model.PlaceAnimal(new Position(3, 3), Species.Fox));
        Assert.AreEqual(Species.Fox, model.FindAnimal(new Position(3, 3))!.Value.Species);
        Assert.IsFalse(model.PlaceAnimal(new Position(0, 0), Species.Fox), "The cell is taken by the bunny");
        Assert.IsFalse(model.PlaceAnimal(new Position(Size, 0), Species.Bunny), "Outside the board");

        //test 2: no editing while the simulation runs
        int bunnyId = StatusOf(model, Species.Bunny).Id;
        model.ResumeGame();
        Assert.IsFalse(model.PlaceAnimal(new Position(4, 4), Species.Bunny));
        Assert.IsFalse(model.RemoveAnimal(bunnyId));
        model.PauseGame();

        //test 3: removing
        Assert.IsTrue(model.RemoveAnimal(bunnyId));
        Assert.IsNull(model.GetStatus(bunnyId), "A removed animal has no status anymore");
        Assert.IsFalse(model.RemoveAnimal(bunnyId), "It is already gone");
    }
}
