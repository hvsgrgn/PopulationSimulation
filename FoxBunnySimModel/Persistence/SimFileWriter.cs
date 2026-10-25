namespace FoxBunnySimCore.Persistence;

public static class SimFileWriter
{
    public static async Task WriteAsync(TextWriter writer, GameState state)
    {
        RulesData rules = state.Rules;
        await writer.WriteLineAsync(Invariant(
            $"{state.BoardSize} {state.Animals.Count} {state.GameTime} {rules.Interval} {rules.GrassGrowth} {rules.BunnyReprod} {rules.FoxReprod} {rules.FoodEnergy} {rules.EnergyLoss}"));

        foreach (AnimalData animal in state.Animals)
            await writer.WriteLineAsync(Invariant($"{animal.Row} {animal.Column} {animal.Energy} {animal.Species}"));

        for (int x = 0; x < state.BoardSize; x++)
        {
            IEnumerable<char> row = Enumerable.Range(0, state.BoardSize)
                .Select(y => state.Grass[x, y] ? '1' : '0');
            await writer.WriteLineAsync(string.Join(' ', row));
        }

        await writer.WriteLineAsync(Invariant($"{state.History.Count}"));
        foreach (PopulationData population in state.History)
            await writer.WriteLineAsync(Invariant($"{population.Bunnies} {population.Foxes}"));
    }

    private static string Invariant(FormattableString text)
    {
        return FormattableString.Invariant(text);
    }
}
