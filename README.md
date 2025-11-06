# OldPhonePadWeb

A minimal ASP.NET Core web app that simulates an old phone keypad and decodes multi‑press digit sequences into text via a server endpoint.

## Features
- On‑screen keypad with 0–9, `*` (separator/backspace), and `#` (send).
- Displays the raw digits and the decoded message.
- Server endpoint `POST /decode` that implements a robust FSM decoder.
- Keyboard support: digits `0–9`, `*`, `#`, and Backspace.
- Docker and Docker Compose support for easy containerization.

## How It Works (Decoding Rules)
On the server, consecutive presses of the same digit map to a letter. The mapping uses wrap‑around within the key length:
- 2=ABC, 3=DEF, 4=GHI, 5=JKL, 6=MNO, 7=PQRS, 8=TUV, 9=WXYZ
- Space (`' '`) commits current buffer and separates letters (pause)
- `*` commits the current buffer, then backspaces one character from the output
- `#` commits the current buffer and terminates decoding

Examples
- `33#` → E
- `4433555 555666#` → HELLO
- `8 88777444666*664#` → TUSING

## Run Locally
Prereqs: .NET 9 SDK

- Restore and build
  - `dotnet restore`
  - `dotnet build`
- Run
  - `dotnet run`
- Open: `http://localhost:5000` (or the URL printed in the console)

## UI Usage
- Click digits to append to the sequence (Digits panel shows your input).
- `0` displays as a space in the Digits panel.
- `*` inserts a separator character in Digits (server interprets it as backspace when decoding).
- `#` sends the digits to the server (`POST /decode`) and shows the decoded text.

[UI.png](images/UI.png)

Keyboard shortcuts
- Digits `0–9` to enter numbers
- `*` to insert separator
- `#` to send and decode
- Backspace to insert separator (same as `*` in the UI)

## API
- `POST /decode`
  - Body: `{ "input": "4433555 555666#" }`
  - Response: `{ "decoded": "HELLO" }`

## Docker
- Build image: `docker build -t oldphonepadweb .`
- Run: `docker run --rm -p 8080:8080 oldphonepadweb`
- Open: `http://localhost:8080`

## Docker Compose
- Prod: `docker compose up --build`
- Dev (hot‑reload): `docker compose -f docker-compose.dev.yml up`

## Tests
- Solution file: `OldPhonePadWeb.sln`
- Test project: `tests/OldPhonePadWeb.Tests`
- Ensure NuGet is configured (a `NuGet.Config` pointing to nuget.org is included).
- Run: `dotnet test`

Test cases cover:
- Provided examples (E, B, HELLO, TUSING)
- Null/empty input, send only, spaces only
- Backspace behavior and wrap‑around logic

## Project Structure
```
OldPhonePadWeb/
├─ Program.cs                 # Minimal API + decoder
├─ wwwroot/index.html         # Keypad UI
├─ Dockerfile                 # Multi‑stage Docker build
├─ .dockerignore
├─ docker-compose.yml         # Production compose
├─ docker-compose.dev.yml     # Dev compose with dotnet watch
├─ NuGet.Config               # NuGet source (nuget.org)
└─ tests/
   └─ OldPhonePadWeb.Tests/   # NUnit tests
```

## Notes
- The UI always displays exactly what is sent to the server. The server controls the canonical decoding.
- If you change decoding behavior, adjust `Program.cs` and (optionally) align the UI hints.
