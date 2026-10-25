using FoxBunnySimCore.Model;
using static FoxBunnySimView.ViewConfig;
using static FoxBunnySimView.UiStyle;

namespace FoxBunnySimView;

public class ParametersForm : Form
{
    private class SliderRow
    {
        private readonly Label _valueLabel;
        private readonly double _step;
        private readonly string _format;

        public StyledSlider Slider { get; }
        public double Value => Math.Round(Slider.Value * _step, 4);

        public SliderRow(StyledSlider slider, Label valueLabel, SliderSpec spec)
        {
            Slider = slider;
            _valueLabel = valueLabel;
            _step = spec.Step;
            _format = spec.Format;
            Slider.ValueChanged += (sender, e) => UpdateLabel();
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            _valueLabel.Text = Value.ToString(_format);
        }
    }

    #region Fields

    private readonly SimulationParameters _initial;
    private readonly TableLayoutPanel _table = new TableLayoutPanel();
    private readonly ComboBox? _sizeBox;
    private readonly SliderRow? _bunnyCount;
    private readonly SliderRow? _foxCount;
    private readonly SliderRow _grassGrowth;
    private readonly SliderRow _bunnyReprod;
    private readonly SliderRow _foxReprod;
    private readonly SliderRow _foodEnergy;
    private readonly SliderRow _energyLoss;
    private readonly SliderRow _interval;

    #endregion

    #region Properties

    public SimulationParameters Result { get; private set; }

    #endregion

    #region Construction

    public ParametersForm(SimulationParameters initial, bool isNewGame)
    {
        _initial = initial;
        Result = initial;

        Text = isNewGame ? "New simulation" : "Parameters";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        _table.Dock = DockStyle.Top;
        _table.Padding = DialogTablePadding;
        _table.ColumnCount = 3;
        _table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DialogLabelColumnWidth));
        _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DialogValueColumnWidth));

        if (isNewGame)
        {
            _sizeBox = AddSizeRow(initial.BoardSize);
            int cells = initial.BoardSize * initial.BoardSize;
            _bunnyCount = AddSlider("Starting bunnies", CountSpec(cells, MaxBunnyFraction), initial.BunnyCount);
            _foxCount = AddSlider("Starting foxes", CountSpec(cells, MaxFoxFraction), initial.FoxCount);
            _sizeBox.SelectedIndexChanged += OnSizeChanged;
        }

        SimulationRules rules = initial.Rules;
        _grassGrowth = AddSlider("Grass regrowth chance", GrassGrowthSlider, rules.GrassGrowth);
        _bunnyReprod = AddSlider("Bunny reproduction threshold", BunnyReprodSlider, rules.BunnyReprod);
        _foxReprod = AddSlider("Fox reproduction threshold", FoxReprodSlider, rules.FoxReprod);
        _foodEnergy = AddSlider("Energy gained from food", FoodEnergySlider, rules.FoodEnergy);
        _energyLoss = AddSlider("Energy lost per step", EnergyLossSlider, rules.EnergyLoss);
        _interval = AddSlider("Step interval (higher = slower)", IntervalSlider, rules.Interval);

        _table.Height = _table.RowCount * DialogRowHeight + DialogTablePadding.Vertical;
        FlowLayoutPanel buttons = CreateButtonBar();

        Controls.Add(_table);
        Controls.Add(buttons);
        ClientSize = new Size(DialogClientWidth, _table.Height + buttons.Height);
        ApplyDialog(this);
    }

    private FlowLayoutPanel CreateButtonBar()
    {
        Button cancelButton = CreateButton("Cancel", false, DialogResult.Cancel);
        Button okButton = CreateButton("OK", true, DialogResult.OK);
        okButton.Click += OnOkClicked;
        AcceptButton = okButton;
        CancelButton = cancelButton;

        FlowLayoutPanel buttons = new FlowLayoutPanel();
        buttons.Dock = DockStyle.Bottom;
        buttons.Height = DialogButtonBarHeight;
        buttons.FlowDirection = FlowDirection.RightToLeft;
        buttons.Padding = DialogButtonBarPadding;
        buttons.Controls.Add(okButton);
        buttons.Controls.Add(cancelButton);
        return buttons;
    }

    private static Button CreateButton(string text, bool primary, DialogResult result)
    {
        Button button = new Button();
        button.Text = text;
        button.DialogResult = result;
        button.Margin = DialogButtonMargin;
        StyleButton(button, primary);
        return button;
    }

    #endregion

    #region Rows

    private static SliderSpec CountSpec(int cells, double fraction)
    {
        return new SliderSpec(1, Math.Max(1, (int)(cells * fraction)), 1, "0");
    }

    private static Label CreateLabel(string text, ContentAlignment alignment)
    {
        Label label = new Label();
        label.Text = text;
        label.Dock = DockStyle.Fill;
        label.TextAlign = alignment;
        return label;
    }

    private int AddRow()
    {
        _table.RowCount++;
        _table.RowStyles.Add(new RowStyle(SizeType.Absolute, DialogRowHeight));
        return _table.RowCount - 1;
    }

    private ComboBox AddSizeRow(int boardSize)
    {
        ComboBox sizeBox = new ComboBox();
        sizeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        foreach (int size in BoardSizes)
            sizeBox.Items.Add(size + " x " + size);
        sizeBox.SelectedIndex = Math.Max(0, Array.IndexOf(BoardSizes, boardSize));
        sizeBox.Anchor = AnchorStyles.Left;
        sizeBox.Width = DialogComboWidth;

        int index = AddRow();
        _table.Controls.Add(CreateLabel("Board size", ContentAlignment.MiddleLeft), 0, index);
        _table.Controls.Add(sizeBox, 1, index);
        _table.SetColumnSpan(sizeBox, 2);
        return sizeBox;
    }

    private SliderRow AddSlider(string text, SliderSpec spec, double value)
    {
        Label valueLabel = CreateLabel("", ContentAlignment.MiddleRight);

        StyledSlider slider = new StyledSlider();
        slider.Minimum = spec.Min;
        slider.Maximum = spec.Max;
        slider.Value = (int)Math.Round(value / spec.Step);
        slider.Dock = DockStyle.Fill;

        int index = AddRow();
        _table.Controls.Add(CreateLabel(text, ContentAlignment.MiddleLeft), 0, index);
        _table.Controls.Add(slider, 1, index);
        _table.Controls.Add(valueLabel, 2, index);
        return new SliderRow(slider, valueLabel, spec);
    }

    #endregion

    #region Event handlers

    private int SelectedBoardSize()
    {
        return _sizeBox == null || _sizeBox.SelectedIndex < 0 ? _initial.BoardSize : BoardSizes[_sizeBox.SelectedIndex];
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        int cells = SelectedBoardSize() * SelectedBoardSize();
        _bunnyCount!.Slider.Maximum = CountSpec(cells, MaxBunnyFraction).Max;
        _foxCount!.Slider.Maximum = CountSpec(cells, MaxFoxFraction).Max;
    }

    private void OnOkClicked(object? sender, EventArgs e)
    {
        SimulationRules rules = new SimulationRules(
            Interval: _interval.Value,
            GrassGrowth: _grassGrowth.Value,
            BunnyReprod: _bunnyReprod.Value,
            FoxReprod: _foxReprod.Value,
            FoodEnergy: _foodEnergy.Value,
            EnergyLoss: _energyLoss.Value);

        Result = new SimulationParameters
        {
            BoardSize = SelectedBoardSize(),
            BunnyCount = _bunnyCount == null ? _initial.BunnyCount : (int)_bunnyCount.Value,
            FoxCount = _foxCount == null ? _initial.FoxCount : (int)_foxCount.Value,
            Rules = rules
        };
    }

    #endregion
}
