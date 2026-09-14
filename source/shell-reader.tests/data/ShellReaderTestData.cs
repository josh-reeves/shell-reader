namespace ShellReader.Tests.Data;

public class ShellReaderTestData : TheoryData<ConsoleKeyInfo[], string>
{

    #region Constructors
    public ShellReaderTestData()
    {
        Add(      
        [
            new('t', ConsoleKey.T, false, false, false), 
            new('e', ConsoleKey.E, false, false, false),
            new('s', ConsoleKey.S, false, false, false),
            new('t', ConsoleKey.T, false, false, false),
            new('\n', ConsoleKey.Enter, false, false, false)
        
        ], "test");
        Add(            
        [
            new('T', ConsoleKey.T, true, false, false), 
            new('h', ConsoleKey.H, false, false, false),
            new('e', ConsoleKey.E, false, false, false),
            new(' ', ConsoleKey.Spacebar, false, false, false),
            new('q', ConsoleKey.Q, false, false, false), 
            new('u', ConsoleKey.U, false, false, false), 
            new('i', ConsoleKey.I, false, false, false), 
            new('c', ConsoleKey.C, false, false, false), 
            new('k', ConsoleKey.K, false, false, false),
            new(' ', ConsoleKey.Spacebar, false, false, false),
            new('b', ConsoleKey.B, false, false, false),
            new('r', ConsoleKey.R, false, false, false),
            new('o', ConsoleKey.O, false, false, false),
            new('w', ConsoleKey.W, false, false, false),
            new('n', ConsoleKey.N, false, false, false),
            new(' ', ConsoleKey.Spacebar, false, false, false),
            new('f', ConsoleKey.F, false, false, false),
            new('o', ConsoleKey.O, false, false, false),
            new('x', ConsoleKey.X, false, false, false),
            new(' ', ConsoleKey.Spacebar, false, false, false),
            new('j', ConsoleKey.J, false, false, false),
            new('u', ConsoleKey.U, false, false, false),
            new('m', ConsoleKey.M, false, false, false),
            new('p', ConsoleKey.P, false, false, false),
            new('s', ConsoleKey.S, false, false, false),
            new(' ', ConsoleKey.Spacebar, false, false, false),
            new('o', ConsoleKey.O, false, false, false),
            new('v', ConsoleKey.V, false, false, false),
            new('e', ConsoleKey.E, false, false, false),
            new('r', ConsoleKey.R, false, false, false),
            new(' ', ConsoleKey.Spacebar, false, false, false),
            new('t', ConsoleKey.T, false, false, false),
            new('h', ConsoleKey.H, false, false, false),
            new('e', ConsoleKey.E, false, false, false),
            new(' ', ConsoleKey.Spacebar, false, false, false),
            new('l', ConsoleKey.L, false, false, false),
            new('a', ConsoleKey.A, false, false, false),
            new('z', ConsoleKey.Z, false, false, false),
            new('y', ConsoleKey.Y, false, false, false),
            new(' ', ConsoleKey.Spacebar, false, false, false),
            new('d', ConsoleKey.D, false, false, false),
            new('o', ConsoleKey.O, false, false, false),
            new('g', ConsoleKey.G, false, false, false),
            new('.', ConsoleKey.Spacebar, false, false, false),
            new('\n', ConsoleKey.Enter, false, false, false)

        ], "The quick brown fox jumps over the lazy dog.");
        Add(
        [
                new('A', ConsoleKey.A, true, false, false), 
                new('1', ConsoleKey.D1, false, false, false),
                new('B', ConsoleKey.B, true, false, false),
                new('2', ConsoleKey.D2, true, false, false), 
                new('C', ConsoleKey.C, true, false, false),
                new('3', ConsoleKey.D3, false, false, false),
                new('\n', ConsoleKey.Enter, false, false, false)

        ], "A1B2C3");

    }
    
    #endregion

}