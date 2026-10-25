# Population Simulation: Foxes & Bunnies 🦊🐰

A predator–prey simulation, built as a Windows Forms desktop app in C# (.NET 8). Bunnies eat grass, foxes hunt bunnies, and you can watch both populations rise and fall on a live chart.

## How the simulation works

- The board is an **n × n grid** (10×10, 20×20 or 30×30). Grass grows on its cells.
- Every tick, each animal moves one cell, including diagonally, and **loses energy**.
- **Bunnies** prefer cells with grass. Eating the grass gives them energy, and eaten grass grows back with a set probability.
- **Foxes** hunt. If a bunny is on a neighbouring cell, the fox jumps onto it and eats it.
- An animal **dies** when its energy runs out.
- When an animal's energy goes above its species' **reproduction threshold**, it has an offspring on a free neighbouring cell and splits its energy with it.
- The game ends when **either species dies out**.

You set all the rules when you start a new game: grass regrowth chance, reproduction thresholds, food energy, energy lost per step and tick interval.

## Features

- **Start, pause, resume**, or go forward **one step at a time**
- **Edit the board live.** Drag a bunny or a fox from the palette onto the board, or select an animal and remove it. The simulation pauses automatically while you edit.
- Animated animal icons with **energy bars**. Click an animal to follow its energy on a side card.
- A **population chart** of both species over time
- A status bar showing the step count and the number of bunnies and foxes
- **Save and load** games as `.sim` text files. Corrupt files give clear error messages with the line number.

## Architecture

The app has three layers, each depending only on the one below it:

| Layer | Project | Contents |
|---|---|---|
| **View** | `FoxBunnySimView` | Windows Forms: `MainForm`, board panel, animal palette, population chart, parameters dialog |
| **Model** | `FoxBunnySimCore` | `GameModel`, `Animal` → `Bunny` / `Fox`, the game board, timer abstraction, events (`GameAdvanced`, `GameOver`) |
| **Persistence** | `FoxBunnySimCore` | `IDataAccess` with async load and save, the `.sim` file reader and writer, and typed file exceptions |

- The view never holds live animal objects. It only gets read-only `AnimalStatus` snapshots and refers to animals by ID.
- The timer sits behind an `ITimer` interface, so the tests can use a `MockTimer` instead of real time.

The full documentation, with use case and class diagrams, is in [`Dokumentacio.pdf`](Dokumentacio.pdf). It's written in Hungarian.

## Running it

You need **Windows** and the [.NET 8 SDK](https://dotnet.microsoft.com/download). Windows Forms only runs on Windows.

```
dotnet run --project FoxBunnySimView
```

You can also open `FoxBunnySim.sln` in Visual Studio and press F5.

Sample save files are in `FoxBunnySimModel/Input/`.

### Tests

```
dotnet test
```

The unit tests use MSTest and Moq. They cover the model's game logic and use a mock timer.

## About

Made as a university assignment for the Event-driven Applications course at ELTE (2025).
