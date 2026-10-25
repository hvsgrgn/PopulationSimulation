using FoxBunnySimCore.Model;
using System.Drawing.Drawing2D;
using static FoxBunnySimView.ViewConfig;

namespace FoxBunnySimView;

public readonly record struct SliderSpec(int Min, int Max, double Step, string Format);

public static class ViewConfig
{
    #region Main window

    public const string WindowTitle = "Fox and bunny simulation";
    public const string FileFilter = "Simulation files (*.sim)|*.sim|All files (*.*)|*.*";
    public const string FileExtension = "sim";
    public const int MinWindowWidth = 900;
    public const int MinWindowHeight = 600;
    public const int ChartPanelWidth = 440;

    #endregion

    #region Colors

    public static readonly Color Background = Color.FromArgb(40, 34, 30);
    public static readonly Color Surface = Color.FromArgb(54, 46, 40);
    public static readonly Color Border = Color.FromArgb(82, 70, 60);
    public static readonly Color Hover = Color.FromArgb(68, 58, 50);
    public static readonly Color Pressed = Color.FromArgb(78, 66, 56);
    public static readonly Color Text = Color.FromArgb(234, 224, 208);
    public static readonly Color MutedText = Color.FromArgb(160, 145, 125);
    public static readonly Color Accent = Color.FromArgb(146, 172, 108);
    public static readonly Color AccentHover = Color.FromArgb(162, 188, 122);
    public static readonly Color AccentDark = Color.FromArgb(118, 142, 86);
    public static readonly Color AccentText = Color.FromArgb(30, 26, 22);
    public static readonly Color LightText = Color.FromArgb(250, 246, 238);
    public static readonly Color SecondaryButton = Color.FromArgb(76, 65, 56);
    public static readonly Color SecondaryButtonHover = Color.FromArgb(90, 77, 66);
    public static readonly Color SecondaryButtonPressed = Color.FromArgb(102, 88, 75);
    public static readonly Color BunnyColor = Color.FromArgb(120, 170, 225);
    public static readonly Color FoxColor = Color.FromArgb(236, 138, 86);

    #endregion

    #region Fonts

    public const string FontFamilyName = "Segoe UI";
    public const float BaseFontSize = 10f;
    public static readonly Font BaseFont = new Font(FontFamilyName, BaseFontSize);
    public static readonly Font BoldFont = new Font(FontFamilyName, BaseFontSize, FontStyle.Bold);
    public static readonly Font SmallFont = new Font(FontFamilyName, 9f);

    #endregion

    #region Buttons, cards and strips

    public const int ButtonMinWidth = 90;
    public const int ButtonMinHeight = 32;
    public static readonly Padding ButtonPadding = new Padding(10, 3, 10, 3);

    public const int CardCornerRadius = 12;
    public const float CardBorderWidth = 1f;
    public const float CardActiveBorderWidth = 2f;

    public static readonly Padding MenuPadding = new Padding(8, 4, 0, 4);
    public static readonly Color RunItemColor = AccentDark;
    public static readonly Padding StatusFirstMargin = new Padding(10, 3, 25, 2);
    public static readonly Padding StatusMiddleMargin = new Padding(0, 3, 25, 2);
    public static readonly Padding StatusLastMargin = new Padding(0, 3, 0, 2);

    #endregion

    #region Board

    public const int BoardMargin = 28;
    public const int BoardPlatePadding = 10;
    public const int BoardPlateCornerRadius = 22;
    public const double TileCornerFraction = 0.28;
    public static readonly Color BoardPlateColor = Color.FromArgb(48, 41, 36);
    public const int MinCellSize = 8;
    public const int TileGap = 2;
    public const int AnimationIntervalMs = 150;
    public const int FrameStaggerX = 7;
    public const int FrameStaggerY = 3;
    public static readonly Color DirtColor = Color.FromArgb(118, 96, 72);
    public static readonly Color GrassBaseColor = Color.FromArgb(86, 124, 62);
    public const double BunnyScale = 1.0;
    public const double FoxScale = 1.4;

    public const int EnergyBarMinHeight = 3;
    public const int EnergyBarHeightDivisor = 8;
    public const int EnergyBarInset = 1;
    public const double EnergyBarHighLimit = 0.6;
    public const double EnergyBarMidLimit = 0.3;
    public static readonly Color EnergyBarBackColor = Color.FromArgb(170, 60, 50, 40);
    public static readonly Color EnergyBarHighColor = Color.FromArgb(96, 170, 80);
    public static readonly Color EnergyBarMidColor = Color.FromArgb(226, 178, 48);
    public static readonly Color EnergyBarLowColor = Color.FromArgb(200, 70, 60);

    public const float CellHighlightWidth = 3f;
    public static readonly Color DropValidColor = AccentDark;
    public static readonly Color DropInvalidColor = Color.FromArgb(200, 70, 60);
    public static readonly Color AnimalHoverColor = Color.FromArgb(234, 224, 208);
    public static readonly Color AnimalSelectedColor = Color.FromArgb(110, 190, 240);

    #endregion

    #region Population chart

    public static readonly Color ChartBackground = Surface;
    public static readonly Color ChartGridColor = Border;
    public const float ChartLineWidth = 2.5f;
    public const int ChartGridLines = 4;
    public const int ChartPlotLeft = 50;
    public const int ChartPlotTop = 50;
    public const int ChartPlotRightPadding = 20;
    public const int ChartPlotBottomPadding = 40;
    public const int ChartMinPlotSize = 10;
    public const int ChartLegendLeft = 15;
    public const int ChartLegendTop = 12;
    public const int ChartLegendSwatch = 12;
    public const int ChartLegendSwatchOffsetY = 4;
    public const int ChartLegendFoxOffset = 100;
    public const int ChartLegendTextGap = 16;
    public const int ChartAxisLabelLeft = 5;
    public const int ChartStepLabelOffset = 70;
    public const int ChartStepLabelGap = 8;

    #endregion

    #region Slider

    public const int SliderHeight = 30;
    public const int SliderTrackHeight = 6;
    public const int SliderThumbDiameter = 18;
    public const int SliderKeyboardLargeStepDivisor = 10;
    public const float SliderThumbBorderWidth = 2f;
    public const float SliderFocusWidth = 2f;
    public const int SliderFocusInflate = 3;
    public static readonly Color SliderTrackColor = Border;
    public static readonly Color SliderFillColor = Accent;
    public static readonly Color SliderThumbColor = Surface;
    public static readonly Color SliderThumbHoverColor = Hover;
    public static readonly Color SliderThumbBorderColor = Accent;
    public static readonly Color SliderFocusColor = Color.FromArgb(120, Accent);

    #endregion

    #region Side panel and palette

    public const int SidePanelWidth = 290;
    public static readonly string[] SpeciesNames = { "Bunny", "Fox" };
    public const string PaletteTitle = "Add an animal";
    public const int PalettePadding = 16;
    public const int PaletteTitleHeight = 30;
    public const int PaletteCardHeight = 120;
    public const int PaletteCardGap = 14;
    public const int PaletteIconSize = 64;
    public const int PaletteLabelHeight = 34;
    public const int PaletteDisabledAlpha = 150;
    public static readonly Color PaletteCardColor = Surface;
    public static readonly Color PaletteCardHoverColor = Hover;
    public static readonly Color PaletteCardActiveColor = Pressed;
    public static readonly Color PaletteCardBorderColor = Border;
    public static readonly Color PaletteCardActiveBorderColor = Accent;

    #endregion

    #region Selected animals

    public const string PetPanelTitle = "Selected animals";
    public const string PetPanelHint = "Click an animal on the board to follow it here.";
    public const int PetPanelTitleHeight = 34;
    public const int PetPanelHintTop = 4;
    public static readonly Padding PetPanelTitlePadding = new Padding(16, 6, 0, 0);
    public static readonly Padding PetPanelPadding = new Padding(16, 0, 16, 12);
    public const int PetCardHeight = 196;
    public const int PetCardGap = 12;
    public const int PetCardPadding = 12;
    public const int PetCardMinWidth = 50;
    public const int PetNameHeight = 24;
    public const int PetIconTopGap = 4;
    public const int PetIconSize = 56;
    public const int PetBarTopGap = 8;
    public const int PetBarHeight = 12;
    public const int PetEnergyTextGap = 2;
    public const int PetEnergyTextHeight = 18;
    public const int PetButtonHeight = 38;
    public const int PetButtonGap = 10;
    public const string PetEnergyFormat = "0.#";
    public static readonly Color PetCardColor = Surface;
    public static readonly Color PetCardBorderColor = Border;
    public static readonly Color PetBarTrackColor = Border;
    public static readonly Color PetRemoveColor = Color.FromArgb(190, 84, 66);
    public static readonly Color PetRemoveHoverColor = Color.FromArgb(208, 104, 84);
    public static readonly Color PetRemovePressedColor = Color.FromArgb(160, 66, 52);

    public static readonly string[] PetNames =
    {
        "Biscuit", "Clover", "Pepper", "Maple", "Nutmeg", "Willow", "Bramble", "Juniper", "Pip", "Thistle",
        "Hazel", "Acorn", "Poppy", "Rusty", "Ember", "Sage", "Misty", "Fennel", "Basil", "Ziggy"
    };

    #endregion

    #region Parameters dialog

    public const int DialogClientWidth = 540;
    public const int DialogRowHeight = 44;
    public const int DialogLabelColumnWidth = 220;
    public const int DialogValueColumnWidth = 80;
    public const int DialogButtonBarHeight = 60;
    public const int DialogComboWidth = 140;
    public static readonly Padding DialogTablePadding = new Padding(16, 14, 16, 0);
    public static readonly Padding DialogButtonBarPadding = new Padding(16, 10, 16, 10);
    public static readonly Padding DialogButtonMargin = new Padding(6, 0, 0, 0);

    public static readonly int[] BoardSizes = GameBoard.AllowedSizes;
    public const double MaxBunnyFraction = 0.5;
    public const double MaxFoxFraction = 0.25;

    public static readonly SliderSpec GrassGrowthSlider = new SliderSpec(0, 100, 0.01, "P0");
    public static readonly SliderSpec BunnyReprodSlider = new SliderSpec(4, 40, 1, "0");
    public static readonly SliderSpec FoxReprodSlider = new SliderSpec(4, 60, 1, "0");
    public static readonly SliderSpec FoodEnergySlider = new SliderSpec(1, 20, 0.5, "0.0");
    public static readonly SliderSpec EnergyLossSlider = new SliderSpec(1, 6, 0.5, "0.0");
    public static readonly SliderSpec IntervalSlider = new SliderSpec(1, 20, 50, "0' ms'");

    #endregion
}

public static class UiStyle
{
    #region Buttons and titles

    public static void StyleButton(Button button, bool primary)
    {
        PrepareButton(button);
        button.AutoSize = true;
        button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button.MinimumSize = new Size(ButtonMinWidth, ButtonMinHeight);
        button.Padding = ButtonPadding;

        if (primary)
            SetButtonColors(button, Accent, AccentHover, AccentDark, AccentText);
        else
            SetButtonColors(button, SecondaryButton, SecondaryButtonHover, SecondaryButtonPressed, ViewConfig.Text);
    }

    public static void StyleCompactButton(Button button, Color back, Color hover, Color pressed, Color fore)
    {
        PrepareButton(button);
        button.AutoSize = false;
        button.Padding = Padding.Empty;
        SetButtonColors(button, back, hover, pressed, fore);
    }

    public static void StyleSecondaryCompactButton(Button button)
    {
        StyleCompactButton(button, SecondaryButton, SecondaryButtonHover, SecondaryButtonPressed, ViewConfig.Text);
    }

    public static void StyleSectionTitle(Label label)
    {
        label.Dock = DockStyle.Top;
        label.Height = PetPanelTitleHeight;
        label.Padding = PetPanelTitlePadding;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Font = BoldFont;
        label.ForeColor = MutedText;
        label.BackColor = Background;
    }

    private static void PrepareButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
        button.Font = BoldFont;
    }

    private static void SetButtonColors(Button button, Color back, Color hover, Color pressed, Color fore)
    {
        button.BackColor = back;
        button.ForeColor = fore;
        button.FlatAppearance.MouseOverBackColor = hover;
        button.FlatAppearance.MouseDownBackColor = pressed;
    }

    #endregion

    #region Dialogs

    public static void ApplyDialog(Control control)
    {
        if (control is Form form)
            form.Font = BaseFont;

        if (control is ComboBox comboBox)
        {
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.BackColor = Surface;
            comboBox.ForeColor = ViewConfig.Text;
        }
        else if (control is not Button && control is not StyledSlider)
        {
            control.BackColor = Background;
            control.ForeColor = ViewConfig.Text;
        }

        foreach (Control child in control.Controls)
            ApplyDialog(child);
    }

    #endregion

    #region Drawing helpers

    public static Color EnergyBarColor(double ratio)
    {
        if (ratio >= EnergyBarHighLimit)
            return EnergyBarHighColor;
        if (ratio >= EnergyBarMidLimit)
            return EnergyBarMidColor;
        return EnergyBarLowColor;
    }

    public static void DrawCard(Graphics graphics, Rectangle rect, Color fill, Color border, float borderWidth, int radius = CardCornerRadius)
    {
        using (GraphicsPath path = RoundedRectangle(rect, radius))
        using (SolidBrush fillBrush = new SolidBrush(fill))
        using (Pen borderPen = new Pen(border, borderWidth))
        {
            graphics.FillPath(fillBrush, path);
            graphics.DrawPath(borderPen, path);
        }
    }

    public static GraphicsPath RoundedRectangle(Rectangle rect, int radius)
    {
        int d = radius * 2;
        GraphicsPath path = new GraphicsPath();
        path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    #endregion

    #region Menu and status bar

    public static void StyleStrip(ToolStrip strip)
    {
        strip.Renderer = new ToolStripProfessionalRenderer(new ConfigColorTable());
        strip.BackColor = Surface;
        strip.ForeColor = ViewConfig.Text;
        strip.Font = BaseFont;
        foreach (ToolStripItem item in strip.Items)
            StyleItem(item);
    }

    private static void StyleItem(ToolStripItem item)
    {
        item.ForeColor = ViewConfig.Text;
        if (item is ToolStripMenuItem menuItem)
        {
            menuItem.DropDown.BackColor = Surface;
            foreach (ToolStripItem child in menuItem.DropDownItems)
                StyleItem(child);
        }
    }

    private class ConfigColorTable : ProfessionalColorTable
    {
        public ConfigColorTable()
        {
            UseSystemColors = false;
        }

        public override Color MenuStripGradientBegin => Surface;
        public override Color MenuStripGradientEnd => Surface;
        public override Color StatusStripGradientBegin => Surface;
        public override Color StatusStripGradientEnd => Surface;
        public override Color ToolStripDropDownBackground => Surface;
        public override Color ImageMarginGradientBegin => Surface;
        public override Color ImageMarginGradientMiddle => Surface;
        public override Color ImageMarginGradientEnd => Surface;
        public override Color MenuBorder => Border;
        public override Color MenuItemBorder => Border;
        public override Color MenuItemSelected => Hover;
        public override Color MenuItemSelectedGradientBegin => Hover;
        public override Color MenuItemSelectedGradientEnd => Hover;
        public override Color MenuItemPressedGradientBegin => ViewConfig.Pressed;
        public override Color MenuItemPressedGradientMiddle => ViewConfig.Pressed;
        public override Color MenuItemPressedGradientEnd => ViewConfig.Pressed;
        public override Color SeparatorDark => Border;
        public override Color SeparatorLight => Border;
        public override Color ToolStripBorder => Border;
    }

    #endregion
}

public class StyledSlider : Control
{
    #region Fields

    private int _minimum;
    private int _maximum = 100;
    private int _value;
    private bool _dragging;
    private bool _hover;

    #endregion

    #region Events

    public event EventHandler? ValueChanged;

    #endregion

    #region Construction

    public StyledSlider()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        TabStop = true;
        Cursor = Cursors.Hand;
        Height = SliderHeight;
        BackColor = Background;
    }

    #endregion

    #region Properties

    public int Minimum
    {
        get => _minimum;
        set
        {
            _minimum = value;
            Value = _value;
            Invalidate();
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            _maximum = value;
            Value = _value;
            Invalidate();
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            int clamped = Math.Clamp(value, _minimum, _maximum);
            if (clamped == _value)
                return;

            _value = clamped;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private int TrackLeft => SliderThumbDiameter / 2;
    private int TrackRight => Width - SliderThumbDiameter / 2;

    #endregion

    #region Painting

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        float centerY = Height / 2f;
        float thumbX = ThumbX();
        float diameter = SliderThumbDiameter;

        RectangleF track = new RectangleF(TrackLeft, centerY - SliderTrackHeight / 2f,
            Math.Max(1, TrackRight - TrackLeft), SliderTrackHeight);
        FillBar(e.Graphics, track, SliderTrackColor);
        FillBar(e.Graphics, new RectangleF(track.Left, track.Top, thumbX - track.Left, track.Height), SliderFillColor);

        RectangleF thumb = new RectangleF(thumbX - diameter / 2f, centerY - diameter / 2f, diameter, diameter);
        if (Focused)
        {
            using (Pen focusPen = new Pen(SliderFocusColor, SliderFocusWidth))
                e.Graphics.DrawEllipse(focusPen, RectangleF.Inflate(thumb, SliderFocusInflate, SliderFocusInflate));
        }

        Color thumbColor = _hover || _dragging ? SliderThumbHoverColor : SliderThumbColor;
        using (SolidBrush thumbBrush = new SolidBrush(thumbColor))
        using (Pen borderPen = new Pen(SliderThumbBorderColor, SliderThumbBorderWidth))
        {
            e.Graphics.FillEllipse(thumbBrush, thumb);
            e.Graphics.DrawEllipse(borderPen, thumb);
        }
    }

    private float ThumbX()
    {
        if (_maximum <= _minimum)
            return TrackLeft;
        return TrackLeft + (float)(_value - _minimum) / (_maximum - _minimum) * (TrackRight - TrackLeft);
    }

    private static void FillBar(Graphics graphics, RectangleF bar, Color color)
    {
        if (bar.Width < 1)
            return;

        float d = Math.Min(bar.Height, bar.Width);
        using (GraphicsPath path = new GraphicsPath())
        using (SolidBrush brush = new SolidBrush(color))
        {
            path.AddArc(bar.Left, bar.Top, d, d, 90, 180);
            path.AddArc(bar.Right - d, bar.Top, d, d, 270, 180);
            path.CloseFigure();
            graphics.FillPath(brush, path);
        }
    }

    #endregion

    #region Mouse

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left)
            return;

        Focus();
        _dragging = true;
        SetFromX(e.X);
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging)
            SetFromX(e.X);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _dragging = false;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        Invalidate();
    }

    private void SetFromX(int x)
    {
        float span = TrackRight - TrackLeft;
        if (span <= 0)
            return;

        float t = Math.Clamp((x - TrackLeft) / span, 0f, 1f);
        Value = _minimum + (int)Math.Round(t * (_maximum - _minimum));
    }

    #endregion

    #region Keyboard and focus

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override bool IsInputKey(Keys keyData)
    {
        switch (keyData & Keys.KeyCode)
        {
            case Keys.Left:
            case Keys.Right:
            case Keys.Up:
            case Keys.Down:
            case Keys.Home:
            case Keys.End:
            case Keys.PageUp:
            case Keys.PageDown:
                return true;
        }
        return base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        int large = Math.Max(1, (_maximum - _minimum) / SliderKeyboardLargeStepDivisor);

        switch (e.KeyCode)
        {
            case Keys.Left:
            case Keys.Down:
                Value = _value - 1;
                break;
            case Keys.Right:
            case Keys.Up:
                Value = _value + 1;
                break;
            case Keys.PageDown:
                Value = _value - large;
                break;
            case Keys.PageUp:
                Value = _value + large;
                break;
            case Keys.Home:
                Value = _minimum;
                break;
            case Keys.End:
                Value = _maximum;
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    #endregion
}
