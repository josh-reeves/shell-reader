# ShellReader

Shell reader is a programmable alternative for GNU Readline-like libraries similar to [tonerdo/readline](https://github.com/tonerdo/readline) and [rafntor/readline.ext](https://github.com/rafntor/readline.ext).

Like these other projects, the goal of ShellReader is to provide a replacement for Console.ReadLine that provides some of the terminal features found in unix shells. Unlike readline or readline.ext, however ShellReader aims to be a generalized, configurable solution that can be used to recreate those features, extend them or create entirely new ones.

This is primarily accomplished via the following design changes:
- The dictionary used to map key combinations to functions is now a publicly accessible "KeyMap" property of the ShellReader class, and the actions being used for values have been replaced with funcs that return a string and take a single "input" string as an argument.
    - To facilitate this change, the strings being used as keys have been replaced with ConsoleKeyInfo objects, and the ShellReader class has private methods that compare the modifiers for any keypress against the minimum modifiers required for any entry in the dictionary.
  
      In simpler terms, this means that if Ctrl+Shift+R is pressed, the class will check for the following, and return whichever entry, if any are defined, most closely matches the pressed keys:
      - Ctrl+Shift+R
      - Ctrl+R
      - Shift+R
      - R
        
      The order in which these entries are defined does not matter.

- In order to provide unix shell functionality without cluttering the default implementation of the ShellReaderClass, a separate ShellControls class is included in the library. This class includes method implementations for unix behaviors similar to those included in readline and readline.ext. These methods are named after their conventional key combinations. To make use of these behaviors, simply instantiate a ShellControls object and map any desired methods to the appropriate key combination. 

- Interfaces have been included for all of the classes in the ShellReader library. This makes it easy to create your own implemenation of any of the included classes.
    - As part of this, the IConsole interface from readline.ext was kept, although the requirement for an "AdvanceCursor" method was removed, and the requirement for a new "TextCursor" property was added. ShellReader does **NOT** include the KeyParser class or the copy of the Console Implementation provided by readline.ext.<br><ul>A basic implementation of this interface, "Terminal," with a TextCursor child class is provided with the library. In order to minimize additional complexity, if no terminal object is provided to a ShellReader via an optional argument at instatiation, an instance of the Terminal class is created and automatically assigned to the ShellReader's Terminal property. This property is publicly accessible, and can be updated after instantiation if desired.

      In order to minimize the implications of the hard dependency created by this approach, both the ShellReader and Terminal clhttps://github.com/josh-reeves/shell-reader/blob/main/README.mdasses are defined in the same ShellReader.cs file.</ul>
- Unlike readline and readline.ext, ShellReader makes very little use of static classes and methods. Instead, the ShellReader class is instantiated to create a reader object.

## Getting Started
ShellReader is still in early development, and there are still a few features that need to be added, including:
- Adding missing behavior definitions to ShellControls.
- Adding the ReadPassword method to the ShellReader class.
- Adding auto-complete.
- Additional bug testing and troubleshooting.

The plan is to eventually distribute the library via nuget.org. Until that is set up, the nuget file can be downloaded from the [releases](https://github.com/josh-reeves/shell-reader/releases) page.

Once the nuget file has been added to your project, you can use the ShellReader namespace to instantiate and configure a new ShellReader object:

```c#
using ShellReader;

...

// Create a ShellReader object:
ShellReader reader = new ShellReader();

// We're using the included controls, so create a control object too:
ShellControls controls = new ShellControls();

// Configure the ShellReader:
reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Enter, false, false, false), controls.Enter);
reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.Backspace, false, false, false), controls.Backspace);
reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false), controls.UpArrow);
reader.KeyMap.Add(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false), controls.DownArrow);

...

// Use the ShellReader:
string str = reader.Read("$ ");
 
```
