# Sergio
A robust static file server, written in F# using Kestrel

## Getting Started

Assuming binary is available in your PATH and named `sergio.exe`:

```
USAGE: sergio [--help] [--listener <string> <int>] [--log-level <int>] [--gzip <bool>] <path>

ROOT:

    <path>                specify a working directory

OPTIONS:

    --listener <string> <int>
                          specify a listener (ex: --listener localhost 5001)
    --log-level <int>     set the log level (default = LogLevel.Error)
    --gzip <bool>         enable gzip compress (default = False)
    --help                display this list of options.
```