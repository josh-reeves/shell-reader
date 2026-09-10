using SharpHook.Data;
using SharpHook.Simulation;

namespace ShellReader.Tests;

public class ShellReaderTests
{
    EventSimulator simulator;

    public ShellReaderTests()
    {
        simulator = EventSimulator.Create("ShellReaderTests");
        
    }

}
