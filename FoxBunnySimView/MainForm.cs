using FoxBunnySimCore.Model;
using static FoxBunnySimView.ViewConfig;
using static FoxBunnySimView.UiStyle;

namespace FoxBunnySimView;

public class MainForm : Form
{
    #region Fields

    private const string HintText = "Drag a bunny or a fox onto the board to add one   |   Click an animal to follow it in the side panel   |   The simulation pauses while you drag or remove";

    private readonly BoardPanel _boardPanel;
    private readonly AnimalPalette _palette;
    private readonly PetPanel _petPanel;
    private readonly PopulationChartPanel _chartPanel;

    private readonly ToolStripMenuItem _newGameItem;
    private readonly ToolStripMenuItem _parametersItem;
    private readonly ToolStripMenuItem _runItem;
    private readonly ToolStripMenuItem _stepItem;
    private readonly ToolStripMenuItem _loadItem;
    private readonly ToolStripMenuItem _saveItem;

    private readonly ToolStripStatusLabel _stepLabel;
    private readonly ToolStripStatusLabel _bunnyLabel;
    private readonly ToolStripStatusLabel _foxLabel;

    private GameModel? _model;
    private SimulationParameters _newGameDefaults = new SimulationParameters();
    private bool _gameOver;
    private bool _busy;
    private GameModel? _interactionModel;
    private bool _resumeAfterInteraction;

    #endregion

    #region Construction

    public MainForm()
    {
        Text = WindowTitle;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(MinWindowWidth, MinWindowHeight);
        Font = BaseFont;
        BackColor = Background;

        _newGameItem = CreateMenuItem("New game...", OnNewGameClicked);
        _parametersItem = CreateMenuItem("Change parameters...", OnParametersClicked);
        _runItem = CreateMenuItem("Start", OnRunClicked);
        _stepItem = CreateMenuItem("Next step", OnStepClicked);
        _loadItem = CreateMenuItem("Load...", OnLoadClicked);
        _saveItem = CreateMenuItem("Save...", OnSaveClicked);

        _stepLabel = new ToolStripStatusLabel { Margin = StatusFirstMargin };
        _bunnyLabel = new ToolStripStatusLabel { Margin = StatusMiddleMargin };
        _foxLabel = new ToolStripStatusLabel { Margin = StatusLastMargin };

        _boardPanel = new BoardPanel { Dock = DockStyle.Fill };
        _boardPanel.AnimalClicked += OnAnimalClicked;

        _palette = new AnimalPalette { Dock = DockStyle.Top, Height = AnimalPalette.RequiredHeight };
        _palette.DragStarted += OnPaletteDragStarted;
        _palette.DragMoved += OnPaletteDragMoved;
        _palette.DragEnded += OnPaletteDragEnded;

        _petPanel = new PetPanel { Dock = DockStyle.Fill };
        _petPanel.SelectionChanged += OnPetSelectionChanged;
        _petPanel.RemoveRequested += OnPetRemoveRequested;
        _boardPanel.SelectedAnimalIds = _petPanel.AnimalIds;

        _chartPanel = new PopulationChartPanel { Dock = DockStyle.Right, Width = ChartPanelWidth };

        Controls.Add(_boardPanel);
        Controls.Add(CreateSidePanel());
        Controls.Add(_chartPanel);
        Controls.Add(CreateStatusStrip());
        Controls.Add(CreateMenuStrip());

        FormClosing += OnFormClosing;
        Shown += OnShown;
        UpdateStatus();
        UpdateControls();
    }

    private static ToolStripMenuItem CreateMenuItem(string text, EventHandler onClick)
    {
        ToolStripMenuItem item = new ToolStripMenuItem(text);
        item.Click += onClick;
        return item;
    }

    private MenuStrip CreateMenuStrip()
    {
        ToolStripMenuItem exitItem = CreateMenuItem("Exit", OnExitClicked);
        ToolStripMenuItem gameMenu = new ToolStripMenuItem("Game", null, _newGameItem, _parametersItem, new ToolStripSeparator(), exitItem);
        ToolStripMenuItem fileMenu = new ToolStripMenuItem("File", null, _loadItem, _saveItem);

        MenuStrip menuStrip = new MenuStrip { Padding = MenuPadding };
        menuStrip.Items.AddRange(new ToolStripItem[] { gameMenu, _runItem, _stepItem, fileMenu });
        StyleStrip(menuStrip);
        _runItem.ForeColor = RunItemColor;
        _runItem.Font = BoldFont;
        MainMenuStrip = menuStrip;
        return menuStrip;
    }

    private StatusStrip CreateStatusStrip()
    {
        ToolStripStatusLabel hintLabel = new ToolStripStatusLabel(HintText) { Spring = true, TextAlign = ContentAlignment.MiddleRight };

        StatusStrip statusStrip = new StatusStrip { SizingGrip = false };
        statusStrip.Items.AddRange(new ToolStripItem[] { _stepLabel, _bunnyLabel, _foxLabel, hintLabel });
        StyleStrip(statusStrip);
        _bunnyLabel.ForeColor = BunnyColor;
        _foxLabel.ForeColor = FoxColor;
        hintLabel.ForeColor = MutedText;
        return statusStrip;
    }

    private Panel CreateSidePanel()
    {
        Label petsTitle = new Label { Text = PetPanelTitle };
        StyleSectionTitle(petsTitle);

        Panel sidePanel = new Panel { Dock = DockStyle.Left, Width = SidePanelWidth, BackColor = Background };
        sidePanel.Controls.Add(_petPanel);
        sidePanel.Controls.Add(petsTitle);
        sidePanel.Controls.Add(_palette);
        return sidePanel;
    }

    #endregion

    #region State

    private void SetModel(GameModel model)
    {
        DisposeModel();

        _model = model;
        _model.GameAdvanced += OnGameAdvanced;
        _model.GameOver += OnGameOver;

        _gameOver = false;
        _petPanel.Model = _model;
        _boardPanel.Model = _model;
        _chartPanel.History = _model.History;
        UpdateStatus();
        UpdateControls();
    }

    private void DisposeModel()
    {
        if (_model == null)
            return;

        _model.GameAdvanced -= OnGameAdvanced;
        _model.GameOver -= OnGameOver;
        _model.Dispose();
        _model = null;
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        UpdateControls();
    }

    private void UpdateControls()
    {
        bool running = _model != null && _model.IsTimerRunning;
        bool canEdit = _model != null && !_gameOver && !_busy;

        _runItem.Text = running ? "Pause" : _model != null && _model.GameTime > 0 ? "Resume" : "Start";
        _runItem.Enabled = canEdit;
        _stepItem.Enabled = canEdit && !running;
        _parametersItem.Enabled = canEdit;
        _saveItem.Enabled = canEdit;
        _palette.Enabled = canEdit;
        _petPanel.Enabled = canEdit;
        _newGameItem.Enabled = !_busy;
        _loadItem.Enabled = !_busy;
    }

    private void UpdateStatus()
    {
        if (_model == null)
        {
            _stepLabel.Text = "No simulation loaded";
            _bunnyLabel.Text = "";
            _foxLabel.Text = "";
            return;
        }

        Population current = _model.History[_model.History.Count - 1];
        _stepLabel.Text = "Step " + _model.GameTime;
        _bunnyLabel.Text = "Bunnies " + current.Bunnies;
        _foxLabel.Text = "Foxes " + current.Foxes;
    }

    private void RefreshView()
    {
        _petPanel.RefreshPets();
        _boardPanel.Invalidate();
        _chartPanel.Invalidate();
        UpdateStatus();
    }

    #endregion

    #region Pausing for dialogs and interactions

    private bool PauseForDialog()
    {
        if (_model == null || !_model.IsTimerRunning)
            return false;

        _model.PauseGame();
        UpdateControls();
        return true;
    }

    private void ResumeAfterDialog(bool wasRunning, GameModel? modelBefore)
    {
        if (wasRunning && _model != null && _model == modelBefore && !_gameOver && !_model.IsGameOver)
            _model.ResumeGame();
        UpdateControls();
    }

    private bool BeginInteraction()
    {
        if (_model == null || _gameOver || _busy || _interactionModel != null)
            return false;

        _interactionModel = _model;
        _resumeAfterInteraction = PauseForDialog();
        return true;
    }

    private void EndInteraction()
    {
        if (_interactionModel == null)
            return;

        GameModel model = _interactionModel;
        _interactionModel = null;
        ResumeAfterDialog(_resumeAfterInteraction, model);
    }

    #endregion

    #region Game menu

    private void OnShown(object? sender, EventArgs e)
    {
        PromptNewGame();
    }

    private void OnNewGameClicked(object? sender, EventArgs e)
    {
        GameModel? before = _model;
        bool wasRunning = PauseForDialog();
        PromptNewGame();
        ResumeAfterDialog(wasRunning, before);
    }

    private void PromptNewGame()
    {
        using (ParametersForm dialog = new ParametersForm(_newGameDefaults, true))
        {
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            _newGameDefaults = dialog.Result;
            SetModel(GameModel.CreateNew(dialog.Result));
        }
    }

    private void OnParametersClicked(object? sender, EventArgs e)
    {
        if (_model == null)
            return;

        GameModel? before = _model;
        bool wasRunning = PauseForDialog();
        using (ParametersForm dialog = new ParametersForm(_model.GetParameters(), false))
        {
            if (dialog.ShowDialog(this) == DialogResult.OK)
                _model.ApplyParameters(dialog.Result);
        }
        ResumeAfterDialog(wasRunning, before);
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        Close();
    }

    #endregion

    #region Running the simulation

    private void OnRunClicked(object? sender, EventArgs e)
    {
        if (_model == null)
            return;

        if (_model.IsTimerRunning)
            _model.PauseGame();
        else
            _model.ResumeGame();
        UpdateControls();
    }

    private void OnStepClicked(object? sender, EventArgs e)
    {
        if (_model != null && !_model.IsTimerRunning)
            _model.Step(this, EventArgs.Empty);
    }

    private void OnGameAdvanced(object? sender, EventArgs e)
    {
        RunOnUiThread(RefreshView);
    }

    private void OnGameOver(object? sender, EventArgs e)
    {
        RunOnUiThread(ShowGameOver);
    }

    private void RunOnUiThread(Action action)
    {
        if (!IsDisposed && IsHandleCreated)
            BeginInvoke(action);
    }

    private void ShowGameOver()
    {
        if (_model == null)
            return;

        _gameOver = true;
        RefreshView();
        UpdateControls();

        Population last = _model.History[_model.History.Count - 1];
        string who = last.Bunnies == 0 && last.Foxes == 0 ? "Both species" : last.Bunnies == 0 ? "The bunnies" : "The foxes";
        MessageBox.Show(this, who + " died out. Both species survived " + _model.GameTime + " steps.",
            "Game over", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    #endregion

    #region Files

    private async void OnLoadClicked(object? sender, EventArgs e)
    {
        GameModel? before = _model;
        bool wasRunning = PauseForDialog();
        try
        {
            string? path = AskForPath(new OpenFileDialog());
            if (path == null)
                return;

            SetBusy(true);
            try
            {
                SetModel(await GameModel.LoadAsync(path));
            }
            catch (GameFileException ex)
            {
                MessageBox.Show(this, ex.Message, "Loading failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }
        finally
        {
            ResumeAfterDialog(wasRunning, before);
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_model == null)
            return;

        GameModel? before = _model;
        bool wasRunning = PauseForDialog();
        try
        {
            string? path = AskForPath(new SaveFileDialog());
            if (path == null)
                return;

            SetBusy(true);
            try
            {
                await _model.SaveAsync(path);
            }
            catch (GameFileException ex)
            {
                MessageBox.Show(this, ex.Message, "Saving failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }
        finally
        {
            ResumeAfterDialog(wasRunning, before);
        }
    }

    private string? AskForPath(FileDialog dialog)
    {
        using (dialog)
        {
            dialog.Filter = FileFilter;
            dialog.DefaultExt = FileExtension;
            return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
        }
    }

    #endregion

    #region Adding, following and removing animals

    private void OnPaletteDragStarted(object? sender, PaletteDragEventArgs e)
    {
        if (BeginInteraction())
            ShowGhost(e);
    }

    private void OnPaletteDragMoved(object? sender, PaletteDragEventArgs e)
    {
        if (_interactionModel != null)
            ShowGhost(e);
    }

    private void OnPaletteDragEnded(object? sender, PaletteDragEventArgs e)
    {
        if (_interactionModel == null)
            return;

        _boardPanel.ClearGhost();
        if (!e.Canceled && _model != null
            && _boardPanel.TryGetCell(_boardPanel.PointToClient(e.ScreenPoint), out Position pos)
            && _model.PlaceAnimal(pos, e.Species))
        {
            RefreshView();
        }
        EndInteraction();
    }

    private void ShowGhost(PaletteDragEventArgs e)
    {
        _boardPanel.SetGhost(e.Species, _boardPanel.PointToClient(e.ScreenPoint));
    }

    private void OnAnimalClicked(object? sender, CellEventArgs e)
    {
        if (_model == null || _gameOver || _busy)
            return;

        AnimalStatus? animal = _model.FindAnimal(e.Pos);
        if (animal != null)
            _petPanel.Follow(animal.Value);
    }

    private void OnPetSelectionChanged(object? sender, EventArgs e)
    {
        _boardPanel.Invalidate();
    }

    private void OnPetRemoveRequested(object? sender, PetEventArgs e)
    {
        if (!BeginInteraction())
            return;

        if (_model != null && _model.RemoveAnimal(e.AnimalId))
            RefreshView();
        EndInteraction();
    }

    #endregion

    #region Closing

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        DisposeModel();
    }

    #endregion
}
