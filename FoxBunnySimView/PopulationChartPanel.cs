using FoxBunnySimCore.Model;
using System.Drawing.Drawing2D;
using static FoxBunnySimView.ViewConfig;

namespace FoxBunnySimView;

public class PopulationChartPanel : Panel
{
    #region Fields

    private IReadOnlyList<Population>? _history;

    #endregion

    #region Construction

    public PopulationChartPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = ChartBackground;
    }

    #endregion

    #region Properties

    public IReadOnlyList<Population>? History
    {
        get => _history;
        set
        {
            _history = value;
            Invalidate();
        }
    }

    #endregion

    #region Painting

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        DrawLegend(e.Graphics);
        if (_history == null)
            return;

        Rectangle plot = new Rectangle(ChartPlotLeft, ChartPlotTop,
            ClientSize.Width - ChartPlotLeft - ChartPlotRightPadding,
            ClientSize.Height - ChartPlotTop - ChartPlotBottomPadding);
        if (plot.Width < ChartMinPlotSize || plot.Height < ChartMinPlotSize)
            return;

        int count = _history.Count;
        int max = 1;
        for (int i = 0; i < count; i++)
            max = Math.Max(max, Math.Max(_history[i].Bunnies, _history[i].Foxes));

        DrawGrid(e.Graphics, plot, max, count);
        if (count >= 2)
        {
            DrawSeries(e.Graphics, plot, max, count, BunnyColor, population => population.Bunnies);
            DrawSeries(e.Graphics, plot, max, count, FoxColor, population => population.Foxes);
        }
    }

    private void DrawLegend(Graphics graphics)
    {
        DrawLegendEntry(graphics, ChartLegendLeft, "Bunnies", BunnyColor);
        DrawLegendEntry(graphics, ChartLegendLeft + ChartLegendFoxOffset, "Foxes", FoxColor);
    }

    private void DrawLegendEntry(Graphics graphics, int left, string text, Color color)
    {
        using (SolidBrush swatchBrush = new SolidBrush(color))
        using (SolidBrush textBrush = new SolidBrush(ViewConfig.Text))
        {
            int swatch = ChartLegendSwatch;
            graphics.FillRectangle(swatchBrush, left, ChartLegendTop + ChartLegendSwatchOffsetY, swatch, swatch);
            graphics.DrawString(text, Font, textBrush, left + ChartLegendTextGap, ChartLegendTop);
        }
    }

    private void DrawGrid(Graphics graphics, Rectangle plot, int max, int count)
    {
        int lines = ChartGridLines;
        using (Pen gridPen = new Pen(ChartGridColor))
        using (SolidBrush mutedBrush = new SolidBrush(MutedText))
        {
            for (int i = 0; i <= lines; i++)
            {
                int y = plot.Top + plot.Height * i / lines;
                graphics.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                graphics.DrawString((max * (lines - i) / lines).ToString(), Font, mutedBrush, ChartAxisLabelLeft, y - Font.Height / 2);
            }
            graphics.DrawString("Step " + Math.Max(count - 1, 0), Font, mutedBrush,
                plot.Right - ChartStepLabelOffset, plot.Bottom + ChartStepLabelGap);
        }
    }

    private void DrawSeries(Graphics graphics, Rectangle plot, int max, int count, Color color, Func<Population, int> value)
    {
        PointF[] points = new PointF[count];
        for (int i = 0; i < count; i++)
        {
            points[i] = new PointF(plot.Left + (float)plot.Width * i / (count - 1),
                plot.Bottom - (float)plot.Height * value(_history![i]) / max);
        }

        using (Pen pen = new Pen(color, ChartLineWidth))
            graphics.DrawLines(pen, points);
    }

    #endregion
}
