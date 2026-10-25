using FoxBunnySimCore.Model;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using static FoxBunnySimCore.Model.Species;
using static FoxBunnySimView.ViewConfig;
using static FoxBunnySimView.UiStyle;

namespace FoxBunnySimView;

public class CellEventArgs : EventArgs
{
    public Position Pos { get; }

    public CellEventArgs(Position pos)
    {
        Pos = pos;
    }
}

public class BoardPanel : Panel
{
    #region Fields

    private readonly AnimatedIcon _grassIcon = AnimatedIcon.FromBitmap(Properties.Resources.GrassIcon);
    private readonly AnimatedIcon _bunnyIcon = AnimatedIcon.FromBitmap(Properties.Resources.BunnyIcon);
    private readonly AnimatedIcon _foxIcon = AnimatedIcon.FromBitmap(Properties.Resources.FoxIcon);
    private readonly SolidBrush _dirtBrush = new SolidBrush(DirtColor);
    private readonly SolidBrush _grassBrush = new SolidBrush(GrassBaseColor);
    private readonly SolidBrush _barBackBrush = new SolidBrush(EnergyBarBackColor);
    private readonly System.Windows.Forms.Timer _animationTimer = new System.Windows.Forms.Timer();

    private GameModel? _model;
    private Bitmap? _dirtTile;
    private Bitmap[] _grassTiles = Array.Empty<Bitmap>();
    private int _tileSize;
    private int _frame;
    private Position? _hover;
    private Species? _ghostSpecies;
    private Point _ghostPoint;

    #endregion

    #region Events

    public event EventHandler<CellEventArgs>? AnimalClicked;

    #endregion

    #region Construction

    public BoardPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Background;

        _animationTimer.Interval = AnimationIntervalMs;
        _animationTimer.Tick += OnAnimationTick;
        if (_grassIcon.FrameCount > 1 || _bunnyIcon.FrameCount > 1 || _foxIcon.FrameCount > 1)
            _animationTimer.Start();
    }

    #endregion

    #region Properties

    public GameModel? Model
    {
        get => _model;
        set
        {
            _model = value;
            _hover = null;
            Invalidate();
        }
    }

    public IEnumerable<int>? SelectedAnimalIds { get; set; }

    #endregion

    #region Public methods

    public bool TryGetCell(Point clientPoint, out Position pos)
    {
        pos = default;
        if (_model == null)
            return false;

        int size = _model.GameBoardSize;
        int cell = CellSize(size);
        if (cell < MinCellSize)
            return false;

        Point offset = BoardOffset(cell, size);
        int px = clientPoint.X - offset.X;
        int py = clientPoint.Y - offset.Y;
        if (px < 0 || py < 0 || px / cell >= size || py / cell >= size)
            return false;

        pos = new Position(py / cell, px / cell);
        return true;
    }

    public void SetGhost(Species species, Point clientPoint)
    {
        _ghostSpecies = species;
        _ghostPoint = clientPoint;
        Invalidate();
    }

    public void ClearGhost()
    {
        _ghostSpecies = null;
        Invalidate();
    }

    #endregion

    #region Layout

    private int CellSize(int boardSize)
    {
        return (Math.Min(ClientSize.Width, ClientSize.Height) - 2 * BoardMargin) / boardSize;
    }

    private Point BoardOffset(int cell, int boardSize)
    {
        return new Point((ClientSize.Width - cell * boardSize) / 2, (ClientSize.Height - cell * boardSize) / 2);
    }

    private static int EnergyBarHeight(int cell)
    {
        return Math.Max(EnergyBarMinHeight, cell / EnergyBarHeightDivisor);
    }

    private static int FreeIconSpace(int cell)
    {
        return Math.Max(1, cell - EnergyBarHeight(cell) - 2 * EnergyBarInset);
    }

    private static int IconSize(int cell, Species species)
    {
        double scale = species == Fox ? FoxScale : BunnyScale;
        return Math.Max(1, (int)Math.Round(FreeIconSpace(cell) * scale));
    }

    private bool AnimalAt(Position pos)
    {
        return _model != null && _model.HasAnimalAt(pos);
    }

    #endregion

    #region Mouse and animation

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && TryGetCell(e.Location, out Position pos) && AnimalAt(pos))
            AnimalClicked?.Invoke(this, new CellEventArgs(pos));
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        Position? hover = TryGetCell(e.Location, out Position pos) && AnimalAt(pos) ? pos : null;
        Cursor = hover.HasValue ? Cursors.Hand : Cursors.Default;
        if (hover != _hover)
        {
            _hover = hover;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        Cursor = Cursors.Default;
        if (_hover.HasValue)
        {
            _hover = null;
            Invalidate();
        }
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        _frame++;
        if (_model != null)
            Invalidate();
    }

    #endregion

    #region Painting

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_model == null)
            return;

        int size = _model.GameBoardSize;
        int cell = CellSize(size);
        if (cell < MinCellSize)
            return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Point offset = BoardOffset(cell, size);
        e.Graphics.TranslateTransform(offset.X, offset.Y);

        DrawPlate(e.Graphics, size, cell);
        DrawTiles(e.Graphics, size, cell);
        List<AnimalStatus> animals = _model!.GetAnimalStatuses();
        DrawAnimals(e.Graphics, animals, cell);
        DrawOutlines(e.Graphics, animals, cell);

        e.Graphics.ResetTransform();
        if (_ghostSpecies.HasValue)
            DrawGhost(e.Graphics, cell);
    }

    private static void DrawPlate(Graphics graphics, int size, int cell)
    {
        int pad = BoardPlatePadding;
        int side = size * cell - TileGap + 2 * pad;
        DrawCard(graphics, new Rectangle(-pad, -pad, side, side), BoardPlateColor,
            Border, CardBorderWidth, BoardPlateCornerRadius);
    }

    private void DrawTiles(Graphics graphics, int size, int cell)
    {
        int tileSize = cell - TileGap;
        EnsureTiles(tileSize);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Bitmap tile = _dirtTile!;
                if (_model!.IsGrassAt(new Position(x, y)))
                {
                    int frame = (_frame + x * FrameStaggerX + y * FrameStaggerY) % _grassTiles.Length;
                    tile = _grassTiles[frame];
                }
                graphics.DrawImageUnscaled(tile, y * cell, x * cell);
            }
        }
    }

    private void EnsureTiles(int tileSize)
    {
        if (_dirtTile != null && _tileSize == tileSize)
            return;

        DisposeTiles();
        _tileSize = tileSize;
        _dirtTile = CreateTile(tileSize, _dirtBrush, null);

        _grassTiles = new Bitmap[_grassIcon.FrameCount];
        for (int i = 0; i < _grassTiles.Length; i++)
            _grassTiles[i] = CreateTile(tileSize, _grassBrush, _grassIcon.GetFrame(i, tileSize, tileSize));
    }

    private static Bitmap CreateTile(int tileSize, Brush fill, Bitmap? icon)
    {
        Bitmap tile = new Bitmap(tileSize, tileSize, PixelFormat.Format32bppPArgb);
        using (Graphics graphics = Graphics.FromImage(tile))
        using (GraphicsPath path = RoundedRectangle(new Rectangle(0, 0, tileSize, tileSize), TileRadius(tileSize)))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillPath(fill, path);
            if (icon != null)
            {
                graphics.SetClip(path);
                graphics.DrawImageUnscaled(icon, 0, 0);
            }
        }
        return tile;
    }

    private static int TileRadius(int tileSize)
    {
        return Math.Max(1, (int)(tileSize * TileCornerFraction));
    }

    private void DisposeTiles()
    {
        _dirtTile?.Dispose();
        _dirtTile = null;
        foreach (Bitmap tile in _grassTiles)
            tile.Dispose();
        _grassTiles = Array.Empty<Bitmap>();
    }

    private void DrawAnimals(Graphics graphics, List<AnimalStatus> animals, int cell)
    {
        foreach (AnimalStatus animal in animals)
            DrawAnimalIcon(graphics, animal, cell);
        foreach (AnimalStatus animal in animals)
            DrawEnergyBar(graphics, animal, cell);
    }

    private void DrawOutlines(Graphics graphics, List<AnimalStatus> animals, int cell)
    {
        if (SelectedAnimalIds != null)
        {
            foreach (AnimalStatus selected in animals.Where(animal => SelectedAnimalIds.Contains(animal.Id)))
                DrawCellOutline(graphics, selected.Pos, cell, AnimalSelectedColor);
        }

        if (_hover.HasValue && AnimalAt(_hover.Value))
            DrawCellOutline(graphics, _hover.Value, cell, AnimalHoverColor);

        if (_ghostSpecies.HasValue && TryGetCell(_ghostPoint, out Position ghostCell))
        {
            Color color = AnimalAt(ghostCell) ? DropInvalidColor : DropValidColor;
            DrawCellOutline(graphics, ghostCell, cell, color);
        }
    }

    private void DrawCellOutline(Graphics graphics, Position pos, int cell, Color color)
    {
        int inset = (int)Math.Ceiling(CellHighlightWidth / 2);
        int side = cell - TileGap - 2 * inset;
        Rectangle rect = new Rectangle(pos.Y * cell + inset, pos.X * cell + inset, side, side);
        using (GraphicsPath path = RoundedRectangle(rect, TileRadius(side)))
        using (Pen pen = new Pen(color, CellHighlightWidth))
            graphics.DrawPath(pen, path);
    }

    private void DrawAnimalIcon(Graphics graphics, AnimalStatus animal, int cell)
    {
        int iconSize = IconSize(cell, animal.Species);
        int centerX = animal.Pos.Y * cell + (cell - TileGap) / 2;
        int centerY = animal.Pos.X * cell + EnergyBarHeight(cell) + EnergyBarInset + FreeIconSpace(cell) / 2;

        AnimatedIcon icon = animal.Species == Fox ? _foxIcon : _bunnyIcon;
        Bitmap frame = icon.GetFrame(_frame + animal.Pos.X * FrameStaggerY + animal.Pos.Y, iconSize, iconSize);
        graphics.DrawImageUnscaledAndClipped(frame, new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize));
    }

    private void DrawEnergyBar(Graphics graphics, AnimalStatus animal, int cell)
    {
        int inset = EnergyBarInset;
        int left = animal.Pos.Y * cell + inset;
        int top = animal.Pos.X * cell + inset;
        int barWidth = cell - TileGap - 2 * inset;
        double ratio = animal.EnergyRatio;

        graphics.FillRectangle(_barBackBrush, left, top, barWidth, EnergyBarHeight(cell));
        using (SolidBrush barBrush = new SolidBrush(EnergyBarColor(ratio)))
            graphics.FillRectangle(barBrush, left, top, (int)(barWidth * ratio), EnergyBarHeight(cell));
    }

    private void DrawGhost(Graphics graphics, int cell)
    {
        int iconSize = IconSize(cell, _ghostSpecies!.Value);
        AnimatedIcon icon = _ghostSpecies == Fox ? _foxIcon : _bunnyIcon;
        graphics.DrawImageUnscaledAndClipped(icon.GetFrame(_frame, iconSize, iconSize),
            new Rectangle(_ghostPoint.X - iconSize / 2, _ghostPoint.Y - iconSize / 2, iconSize, iconSize));
    }

    #endregion

    #region Disposal

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer.Dispose();
            _grassIcon.Dispose();
            _bunnyIcon.Dispose();
            _foxIcon.Dispose();
            DisposeTiles();
            _dirtBrush.Dispose();
            _grassBrush.Dispose();
            _barBackBrush.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}
