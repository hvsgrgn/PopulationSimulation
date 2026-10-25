using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace FoxBunnySimView;

public class AnimatedIcon : IDisposable
{
    #region Fields

    private readonly List<Bitmap> _sourceFrames = new List<Bitmap>();
    private List<Bitmap> _scaledFrames = new List<Bitmap>();
    private int _scaledWidth;
    private int _scaledHeight;

    #endregion

    #region Construction

    public AnimatedIcon(Bitmap source)
    {
        FrameDimension dimension = new FrameDimension(source.FrameDimensionsList[0]);
        int count = source.GetFrameCount(dimension);
        for (int i = 0; i < count; i++)
        {
            source.SelectActiveFrame(dimension, i);
            _sourceFrames.Add(new Bitmap(source));
        }
    }

    public static AnimatedIcon FromBitmap(Bitmap source)
    {
        using (source)
            return new AnimatedIcon(source);
    }

    #endregion

    #region Frames

    public int FrameCount => _sourceFrames.Count;

    public Bitmap GetFrame(int index, int width, int height)
    {
        if (width != _scaledWidth || height != _scaledHeight)
            Rescale(width, height);

        int frame = index % _scaledFrames.Count;
        return _scaledFrames[frame < 0 ? frame + _scaledFrames.Count : frame];
    }

    private void Rescale(int width, int height)
    {
        DisposeFrames(_scaledFrames);
        _scaledFrames = new List<Bitmap>();

        foreach (Bitmap frame in _sourceFrames)
        {
            Bitmap scaled = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            using (Graphics graphics = Graphics.FromImage(scaled))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(frame, 0, 0, width, height);
            }
            _scaledFrames.Add(scaled);
        }

        _scaledWidth = width;
        _scaledHeight = height;
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        DisposeFrames(_sourceFrames);
        DisposeFrames(_scaledFrames);
    }

    private static void DisposeFrames(List<Bitmap> frames)
    {
        foreach (Bitmap frame in frames)
            frame.Dispose();
    }

    #endregion
}
