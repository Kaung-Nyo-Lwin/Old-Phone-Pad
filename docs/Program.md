# Program.cs – Detailed Documentation

This document explains the application logic implemented in `Program.cs`, including the hosting setup, API surface, decoder class, method contracts, algorithmic flow, edge cases, and extensibility.

## Overview
The app is an ASP.NET Core minimal API that exposes a single endpoint for decoding multi‑press old‑phone keypad input into text. Static web assets under `wwwroot/` provide a keypad UI which posts digit sequences to the server for decoding.

## Hosting Pipeline
- `WebApplication.CreateBuilder(args)` and `builder.Build()` create the app.
- `app.UseDefaultFiles()` serves `wwwroot/index.html` by default.
- `app.UseStaticFiles()` serves static assets from `wwwroot`.

These middlewares make the UI immediately accessible at the root URL while the API is available under `/decode`.

## API
- Method: POST
- Route: `/decode`
- Request Body: JSON
  - `{ "input": string }`
    - `input`: a sequence composed of digits `'2'..'9'`, spaces `' '`, backspace `'*'`, and terminal `'#'`. Other characters are ignored.
- Response Body: JSON
  - `{ "decoded": string }`
    - `decoded`: the decoded text.

### Example
Request
```http
POST /decode
Content-Type: application/json

{ "input": "4433555 555666#" }
```
Response
```json
{ "decoded": "HELLO" }
```

## Decoder Class: `OldPhonePadDecoder`
A static, stateless class encapsulating keypad decoding logic.

### Key Mapping
```csharp
private static readonly Dictionary<char, string> _keyMap = new()
{
    { '2', "ABC" }, { '3', "DEF" }, { '4', "GHI" }, { '5', "JKL" },
    { '6', "MNO" }, { '7', "PQRS" }, { '8', "TUV" }, { '9', "WXYZ" }
    // '0' and '1' are intentionally not mapped
};
```
- Consecutive presses of the same digit cycle letters within its group using wrap‑around.
- `0` and `1` are not mapped; the UI uses `0` as a visual space and `*` as backspace, but server mapping intentionally omits them per the current design.

### Helper: `ProcessBuffer(StringBuilder buffer, StringBuilder output)`
- Purpose: Commit the current run of the same digit to the output.
- Inputs:
  - `buffer`: holds a sequence of identical digits representing a single character press‑run.
  - `output`: the accumulating decoded message.
- Behavior:
  - If `buffer` is empty, do nothing.
  - Let `button = buffer[0]`, `presses = buffer.Length`.
  - If `_keyMap` contains `button`, compute `index = (presses - 1) % letters.Length` and append `letters[index]` to `output`.
- Side Effects: None beyond appending to `output`. `buffer` is not mutated here.

### Method: `string Decode(string input)`
- Arguments:
  - `input`: may be `null`, empty, or any string. Expected symbols: digits `'2'..'9'`, space `' '`, backspace `'*'`, terminal `'#'`. Other characters are ignored.
- Returns:
  - The decoded string per keypad rules.
- Algorithm (Finite State Machine with a mutable buffer):
  1. If `input` is `null` or empty, return `""`.
  2. Initialize `output` and an empty `currentBuffer`.
  3. Iterate each character `c` in `input`:
     - `'#'` (Send/Terminal): `ProcessBuffer(currentBuffer, output)` then `break` (stop processing).
     - `'*'` (Backspace): `ProcessBuffer(currentBuffer, output)`, clear `currentBuffer`, and if `output.Length > 0`, remove one character (backspace on finalized output).
     - `' '` (Pause): `ProcessBuffer(currentBuffer, output)`, clear `currentBuffer` (letter boundary).
     - Digit `'0'..'9'`:
       - If `currentBuffer` is non‑empty and `currentBuffer[0] != c`, then `ProcessBuffer` (commit), clear `currentBuffer`, and start a new run with `c`.
       - Else append `c` to `currentBuffer` (continuation of the current run).
     - Any other character: ignored.
  4. After the loop, call `ProcessBuffer(currentBuffer, output)` to commit any pending run unless the loop terminated early after `'#'` (in which case `currentBuffer` will already be empty if immediately cleared before breaking).
- Complexity:
  - Time: O(N) for input length N.
  - Space: O(N) for output. `currentBuffer` is bounded by the longest run of identical digits.
- Thread Safety: The decoder is stateless and uses only local variables, safe for concurrent use.

### Behavioral Rules (Commit‑Then‑Act)
- A buffer (run of same digit) is “committed” into a letter when:
  1) A different digit is pressed, 2) a pause `' '` is encountered, or 3) a special key `'*'` or `'#'` is processed.
- `'*'` (Backspace): commit pending letter, then delete one previously committed character if present.
- `'#'` (Send): commit pending letter and terminate.

## Edge Cases
- `null`, `""` → `""`.
- `"#"` → `""`.
- Only spaces → `""`.
- Only backspaces → `""`.
- Wrap‑around: e.g., `"2222#"` → `A` (4 presses on `ABC` → index `(4-1)%3=0`).
- Mixed sequences with `*` and spaces commit appropriately per rules.

## Examples
- `"33#"` → `E`
- `"227*#"` → `B`
- `"4433555 555666#"` → `HELLO`
- `"8 88777444666*664#"` → `TUSING`

## Tests
Unit tests live under `tests/OldPhonePadWeb.Tests`. They validate examples, edge cases, backspace behavior, and wrap‑around logic using NUnit.

## Extensibility
- Map `0` or `1`: add entries to `_keyMap` and update UI hints.
- Alternate behaviors (e.g., long‑press timing instead of explicit spaces) could be added by changing input semantics.
- For DI and deeper testing, move the decoder to its own class/library and register with the service container.

## Notes on UI
The UI sends raw digits (including `*`/`#`/spaces) to the API. The server is the source of truth for decoding. UI labeling clarifies that `*` is backspace and `#` sends/terminates.

