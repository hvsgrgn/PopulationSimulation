using FoxBunnySimCore.Model;
using System.Drawing.Drawing2D;
using static FoxBunnySimCore.Model.Species;
using static FoxBunnySimView.ViewConfig;
using static FoxBunnySimView.UiStyle;

namespace FoxBunnySimView;

public class PaletteDragEventArgs : EventArgs
{
    public Species Species { get; }
    public Point ScreenPoint { get; }
    public bool Canceled { get; }

    public PaletteDragEventArgs(Species species, Point screenPoint, bool canceled)
    {
        Species = species;
        ScreenPoint = screenPoint;
        Canceled = canceled;
    }
}

public class AnimalPalette : Control
{
    #region Fields

    private readonly AnimatedIcon[] _icons =
    {
        AnimatedIcon.FromBitmap(Properties.Resources.BunnyIcon),
        AnimatedIcon.FromBitmap(Properties.Resources.FoxIcon)
    };

    //card index -> species, so the cards do not depend on the enum numbers
    private static readonly Species[] CardSpecies = { Bunny, Fox };

    private int _hover = -1;
    private int _dragSpecies = -1;

    #endregion

    #region Events

    public event EventHandler<PaletteDragEventArgs>? DragStarted;
    public event EventHandler<PaletteDragEventArgs>? DragMoved;
    public event EventHandler<PaletteDragEventArgs>? DragEnded;

    #endregion

    #region Construction

    public static int RequiredHeight =>
        2 * PalettePadding + PaletteTitleHeight
        + CardCount * PaletteCardHeight + (CardCount - 1) * PaletteCardGap;

    private static int CardCount => SpeciesNames.Length;

    public AnimalPalette()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw, true);
        BackColor = Background;
        ForeColor = ViewConfig.Text;
    }

    #endregion

    #region Layout

    private Rectangle CardRect(int index)
    {
        int top = PalettePadding + PaletteTitleHeight + index * (PaletteCardHeight + PaletteCardGap);
        return new Rectangle(PalettePadding, top, Width - 2 * PalettePadding, PaletteCardHeight);
    }

    private int CardAt(Point point)
    {
        for (int i = 0; i < CardCount; i++)
        {
            if (CardRect(i).Contains(point))
                return i;
        }
        return -1;
    }

    #endregion

    #region Painting

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using (SolidBrush mutedBrush = new SolidBrush(MutedText))
            e.Graphics.DrawString(PaletteTitle, BoldFont, mutedBrush, PalettePadding, PalettePadding);

        for (int i = 0; i < CardCount; i++)
            DrawCard(e.Graphics, i);

        if (!Enabled)
        {
            using (SolidBrush fade = new SolidBrush(Color.FromArgb(PaletteDisabledAlpha, Background)))
                e.Graphics.FillRectangle(fade, ClientRectangle);
        }
    }

    private void DrawCard(Graphics graphics, int index)
    {
        Rectangle rect = CardRect(index);
        bool active = index == _dragSpecies;
        Color fill = active ? PaletteCardActiveColor
            : index == _hover && Enabled ? PaletteCardHoverColor
            : PaletteCardColor;
        Color border = active ? PaletteCardActiveBorderColor : PaletteCardBorderColor;
        UiStyle.DrawCard(graphics, rect, fill, border, active ? CardActiveBorderWidth : CardBorderWidth);

        int iconSize = PaletteIconSize;
        int iconAreaHeight = rect.Height - PaletteLabelHeight;
        graphics.DrawImageUnscaled(_icons[index].GetFrame(0, iconSize, iconSize),
            rect.Left + (rect.Width - iconSize) / 2, rect.Top + (iconAreaHeight - iconSize) / 2);

        Rectangle labelRect = new Rectangle(rect.Left, rect.Bottom - PaletteLabelHeight, rect.Width, PaletteLabelHeight);
        using (SolidBrush textBrush = new SolidBrush(ViewConfig.Text))
        using (StringFormat centered = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            graphics.DrawString(SpeciesNames[index], BoldFont, textBrush, labelRect, centered);
    }

    #endregion

    #region Mouse

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        int card = CardAt(e.Location);
        if (e.Button != MouseButtons.Left || card < 0)
            return;

        _dragSpecies = card;
        Capture = true;
        Cursor = Cursors.Hand;
        Invalidate();
        DragStarted?.Invoke(this, new PaletteDragEventArgs(CardSpecies[card], MousePosition, false));
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_dragSpecies >= 0)
        {
            DragMoved?.Invoke(this, new PaletteDragEventArgs(CardSpecies[_dragSpecies], MousePosition, false));
            return;
        }

        int card = CardAt(e.Location);
        if (card != _hover)
        {
            _hover = card;
            Cursor = card >= 0 ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
            EndDrag(false);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_dragSpecies < 0 && _hover != -1)
        {
            _hover = -1;
            Cursor = Cursors.Default;
            Invalidate();
        }
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (_dragSpecies >= 0 && !Capture)
            EndDrag(true);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    private void EndDrag(bool canceled)
    {
        int dragged = _dragSpecies;
        if (dragged < 0)
            return;

        _dragSpecies = -1;
        Capture = false;
        Cursor = Cursors.Default;
        Invalidate();
        DragEnded?.Invoke(this, new PaletteDragEventArgs(CardSpecies[dragged], MousePosition, canceled));
    }

    #endregion

    #region Disposal

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (AnimatedIcon icon in _icons)
                icon.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}

public class PetCard : Control
{
    #region Fields

    private readonly AnimatedIcon _icon;
    private readonly Button _unselectButton = new Button();
    private readonly Button _removeButton = new Button();
    private AnimalStatus _status;

    #endregion

    #region Events

    public event EventHandler? UnselectClicked;
    public event EventHandler? RemoveClicked;

    #endregion

    #region Properties

    public int AnimalId { get; }
    public string PetName { get; }

    #endregion

    #region Construction

    public PetCard(int animalId, string petName, AnimatedIcon icon)
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw, true);
        AnimalId = animalId;
        PetName = petName;
        _icon = icon;
        BackColor = Background;
        ForeColor = ViewConfig.Text;

        _unselectButton.Text = "Unselect";
        StyleSecondaryCompactButton(_unselectButton);
        _unselectButton.Click += (sender, e) => UnselectClicked?.Invoke(this, EventArgs.Empty);

        _removeButton.Text = "Remove";
        StyleCompactButton(_removeButton, PetRemoveColor, PetRemoveHoverColor,
            PetRemovePressedColor, LightText);
        _removeButton.Click += (sender, e) => RemoveClicked?.Invoke(this, EventArgs.Empty);

        Controls.Add(_unselectButton);
        Controls.Add(_removeButton);
        Height = PetCardHeight + PetCardGap;
    }

    #endregion

    #region Public methods

    public void SetStatus(AnimalStatus status)
    {
        _status = status;
        Invalidate();
    }

    #endregion

    #region Layout

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        int padding = PetCardPadding;
        int width = Math.Max(1, (Width - 2 * padding - PetButtonGap) / 2);
        int top = PetCardHeight - padding - PetButtonHeight;
        _unselectButton.SetBounds(padding, top, width, PetButtonHeight);
        _removeButton.SetBounds(padding + width + PetButtonGap, top, width, PetButtonHeight);
    }

    #endregion

    #region Painting

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        int padding = PetCardPadding;
        UiStyle.DrawCard(e.Graphics, new Rectangle(0, 0, Width - 1, PetCardHeight - 1),
            PetCardColor, PetCardBorderColor, CardBorderWidth);

        int iconSize = PetIconSize;
        int iconTop = padding + PetNameHeight + PetIconTopGap;
        Rectangle nameRect = new Rectangle(padding, padding, Width - 2 * padding, PetNameHeight);
        Rectangle track = new Rectangle(padding, iconTop + iconSize + PetBarTopGap, Width - 2 * padding, PetBarHeight);
        Rectangle textRect = new Rectangle(padding, track.Bottom + PetEnergyTextGap, Width - 2 * padding, PetEnergyTextHeight);

        e.Graphics.DrawImageUnscaled(_icon.GetFrame(0, iconSize, iconSize), (Width - iconSize) / 2, iconTop);
        DrawEnergyBar(e.Graphics, track);

        using (SolidBrush textBrush = new SolidBrush(ViewConfig.Text))
        using (SolidBrush mutedBrush = new SolidBrush(MutedText))
        using (StringFormat centered = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
        {
            e.Graphics.DrawString(PetName, BoldFont, textBrush, nameRect, centered);
            e.Graphics.DrawString("Energy " + _status.Energy.ToString(PetEnergyFormat) + " / " + _status.Threshold.ToString(PetEnergyFormat),
                SmallFont, mutedBrush, textRect, centered);
        }
    }

    private void DrawEnergyBar(Graphics graphics, Rectangle track)
    {
        double ratio = _status.EnergyRatio;
        using (SolidBrush trackBrush = new SolidBrush(PetBarTrackColor))
            graphics.FillRectangle(trackBrush, track);
        using (SolidBrush barBrush = new SolidBrush(EnergyBarColor(ratio)))
            graphics.FillRectangle(barBrush, track.Left, track.Top, (int)(track.Width * ratio), track.Height);
    }

    #endregion
}

public class PetEventArgs : EventArgs
{
    public int AnimalId { get; }

    public PetEventArgs(int animalId)
    {
        AnimalId = animalId;
    }
}

public class PetPanel : FlowLayoutPanel
{
    #region Fields

    private readonly List<PetCard> _cards = new List<PetCard>();
    private readonly AnimatedIcon _bunnyIcon = AnimatedIcon.FromBitmap(Properties.Resources.BunnyIcon);
    private readonly AnimatedIcon _foxIcon = AnimatedIcon.FromBitmap(Properties.Resources.FoxIcon);
    private readonly Random _random = new Random();
    private GameModel? _model;

    #endregion

    #region Events

    public event EventHandler? SelectionChanged;
    public event EventHandler<PetEventArgs>? RemoveRequested;

    #endregion

    #region Construction

    public PetPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        FlowDirection = FlowDirection.TopDown;
        WrapContents = false;
        AutoScroll = true;
        BackColor = Background;
        ForeColor = ViewConfig.Text;
        Padding = PetPanelPadding;
    }

    #endregion

    #region Properties

    public IEnumerable<int> AnimalIds => _cards.Select(card => card.AnimalId);

    public GameModel? Model
    {
        get => _model;
        set
        {
            _model = value;
            for (int i = _cards.Count - 1; i >= 0; i--)
                RemoveCard(i);
            NotifySelectionChanged();
        }
    }

    private int CardWidth => Math.Max(PetCardMinWidth, ClientSize.Width - Padding.Horizontal);

    #endregion

    #region Following animals

    public void Follow(AnimalStatus animal)
    {
        if (_model == null || _cards.Any(existing => existing.AnimalId == animal.Id))
            return;

        PetCard card = new PetCard(animal.Id, PickName(), animal.Species == Fox ? _foxIcon : _bunnyIcon);
        card.Margin = Padding.Empty;
        card.Width = CardWidth;
        card.UnselectClicked += OnCardUnselect;
        card.RemoveClicked += OnCardRemove;
        UpdateCard(card);

        _cards.Add(card);
        Controls.Add(card);
        ScrollControlIntoView(card);
        NotifySelectionChanged();
    }

    public void RefreshPets()
    {
        if (_model == null || _cards.Count == 0)
            return;

        bool removed = false;
        for (int i = _cards.Count - 1; i >= 0; i--){
            if (!UpdateCard(_cards[i])){
                RemoveCard(i);
                removed = true;
            }
        }

        if (removed)
            NotifySelectionChanged();
    }

    //asks the model for the animal's current data, returns false if it is gone
    private bool UpdateCard(PetCard card)
    {
        AnimalStatus? status = _model!.GetStatus(card.AnimalId);
        if (status == null)
            return false;

        card.SetStatus(status.Value);
        return true;
    }

    private void RemoveCard(int index)
    {
        PetCard card = _cards[index];
        _cards.RemoveAt(index);
        Controls.Remove(card);

        if (IsHandleCreated)
            BeginInvoke(new Action(card.Dispose));
        else
            card.Dispose();
    }

    private void NotifySelectionChanged()
    {
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    private string PickName()
    {
        List<string> free = PetNames.Where(name => _cards.All(card => card.PetName != name)).ToList();
        IList<string> pool = free.Count > 0 ? free : PetNames;
        return pool[_random.Next(pool.Count)];
    }

    #endregion

    #region Event handlers

    private void OnCardUnselect(object? sender, EventArgs e)
    {
        int index = _cards.IndexOf((PetCard)sender!);
        if (index < 0)
            return;

        RemoveCard(index);
        NotifySelectionChanged();
    }

    private void OnCardRemove(object? sender, EventArgs e)
    {
        RemoveRequested?.Invoke(this, new PetEventArgs(((PetCard)sender!).AnimalId));
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        foreach (PetCard card in _cards)
            card.Width = CardWidth;
    }

    #endregion

    #region Painting and disposal

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_model == null || _cards.Count > 0)
            return;

        Rectangle rect = new Rectangle(Padding.Left, PetPanelHintTop,
            ClientSize.Width - Padding.Horizontal, ClientSize.Height - PetPanelHintTop);
        using (SolidBrush mutedBrush = new SolidBrush(MutedText))
        using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center })
            e.Graphics.DrawString(PetPanelHint, SmallFont, mutedBrush, rect, format);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _bunnyIcon.Dispose();
            _foxIcon.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}
