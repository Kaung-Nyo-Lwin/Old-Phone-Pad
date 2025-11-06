using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/decode", (DecodeRequest req) =>
{
    var decoded = OldPhonePadDecoder.Decode(req.Input ?? string.Empty);
    return Results.Json(new { decoded });
});

app.Run();

record DecodeRequest(string? Input);



/// <summary>
/// Translates an old-phone keypad input string into the corresponding text.
/// </summary>
/// <param name="input">The input string, composed of digits '2'-'9',
/// pauses (' '), backspaces ('*'), and a terminal ('#').</param>
/// <returns>The translated string.</returns>
/// <remarks>
/// This method implements a Finite State Machine (FSM) to parse the input.
/// A buffer of consecutive, identical key presses is "committed" (translated
/// and appended to the output) when one of three events occurs:
/// 1. A pause (' ') is encountered.
/// 2. A different digit key is pressed.
/// 3. A special character ('*' or '#') is pressed.
///
/// The backspace ('*') and send ('#') keys are "commit-then-act" triggers.
/// </remarks>

static class OldPhonePadDecoder
{

    /// <summary>
    /// Helper map to store the key-to-letter mappings.
    /// Using Dictionary<char, string> is clean, maintainable, and
    /// uses an efficient 'char' key. 
    /// </summary>
    private static readonly Dictionary<char, string> _keyMap = new Dictionary<char, string>
    {
        { '2', "ABC" },
        { '3', "DEF" },
        { '4', "GHI" },
        { '5', "JKL" },
        { '6', "MNO" },
        { '7', "PQRS" },
        { '8', "TUV" },
        { '9', "WXYZ" }
        // '0' and '1' are not mapped per the prompt 
    };

    /// <summary>
    /// Processes the current key-press buffer, translates it to a
    /// character, and appends it to the output.
    /// </summary>
    private static void ProcessBuffer(StringBuilder buffer, StringBuilder output)
    {
        if (buffer.Length == 0)
        {
            return;
        }

        char button = buffer[0];
        int presses = buffer.Length;

        if (_keyMap.TryGetValue(button, out string? letters))
        {
            // Use modulo arithmetic to handle wrap-around
            // (e.g., "2222" -> index 3 % 3 -> index 0 -> 'A')
            // This makes the solution robust and handles buttons of
            // different lengths (e.g., "7" -> PQRS) correctly.
            int index = (presses - 1) % letters.Length;
            output.Append(letters[index]);
        }
    }

    public static string Decode(string input)
    {
        // Handle null/empty edge cases
        if (string.IsNullOrEmpty(input))
        {
            return "";
        }

        // Use StringBuilder for O(N) performance, pre-sized to input length
        // to avoid reallocations. [28, 37]
        var output = new StringBuilder(input.Length);
        
        // This buffer IS the state of our FSM
        var currentBuffer = new StringBuilder();

        foreach (char c in input)
        {
            if (c == '#') // Send (Terminal Rule)
            {
                ProcessBuffer(currentBuffer, output); // 1. Commit
                break;                                // 2. Terminate
            }

            if (c == '*') // Backspace (Special Character Rule)
            {
                ProcessBuffer(currentBuffer, output); // 1. Commit
                currentBuffer.Clear();                // 2. Reset buffer
                if (output.Length > 0)
                {
                    output.Length--; // 3. Perform backspace on final output
                }
            }
            else if (c == ' ') // Pause (Pause Rule)
            {
                ProcessBuffer(currentBuffer, output); // 1. Commit
                currentBuffer.Clear();                // 2. Reset buffer
            }
            else if (char.IsDigit(c))
            {
                // Is this the same button as before, or a new one?
                if (currentBuffer.Length > 0 && currentBuffer[0]!= c)
                {
                    // Different Button Rule
                    ProcessBuffer(currentBuffer, output); // 1. Commit old button
                    currentBuffer.Clear();                // 2. Reset buffer
                    currentBuffer.Append(c);              // 3. Start new button
                }
                else
                {
                    // Same button (or buffer was empty)
                    currentBuffer.Append(c);
                }
            }
            // Ignore any other characters
        }

        // printf("Final output: %s\n", output.ToString());
        Console.WriteLine("Input: " + input + ", output: " + output.ToString());
        return output.ToString();
    }

}