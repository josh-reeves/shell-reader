namespace ShellReader.Example;

class Program
{
    static void Main(string[] args)
    {
        string input = string.Empty,
               exitStr = "exit",
               passwordStr = "pass",
               prompt = "$ ",
               mask = string.Empty,
               passMsg = $"Enter a sequence of any length to use as a mask. Current mask: {mask}",
               welcomeMsg = "ShellReader Example. Type 'pass' to enter password mode. Type 'exit' to quit:";

        ShellReader reader = new(prompt: prompt, mask: mask, separators: [" "]);
        ShellControls controls = new(reader);

        controls.RegisterAllWithDefaults();

        Console.TreatControlCAsInput = true;

        while(!string.Equals(input, exitStr))
        {
            reader.Terminal.WriteLine(welcomeMsg);
            input = reader.Read() ?? string.Empty;

            if (string.Equals(input, passwordStr))
            {
                reader.Terminal.WriteLine(passMsg);
                mask = reader.Read(prompt: "") ?? string.Empty;

                input = reader.ReadPassword(mask: mask) ?? string.Empty;
    
            }

            reader.Terminal.WriteLine($"Input Received: {input}");

        }

    }

}
