namespace FoxBunnySimCore.Model;

public interface ITimer
{
    bool Enabled { get; set; }
    double Interval { get; set; }

    event EventHandler? Elapsed;

    void Start();
}
