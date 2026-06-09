# T3Sharp - Ternary Computer Simulator Suite

A comprehensive .NET-based suite of ternary processor simulators implementing a balanced ternary instruction set architecture (ISA) with multiple microarchitectures.

## Overview

T3Sharp implements a **balanced ternary computing system** using the mathematical system {-, 0, +} where each digit (trit) represents -1, 0, and +1 respectively. This project provides multiple simulator implementations to explore different processor designs and optimizations.

### Key Features

- **Multiple Processor Implementations**: In-order, out-of-order, and pipelined processors
- **Word Sizes**: Support for 9, 18, 27, and 36 trit word sizes
- **Multiple Output Formats**: Ternary, 9-ary, 27-ary, and binary representations
- **Interactive CLI**: Full-featured command-line interface for debugging and testing
- **Program Loading**: Support for text, ninary (0n), tryx (0y), and binary file formats
- **Debugging Tools**: Breakpoints, trace buffer, disassembly, memory inspection

## Architecture

### Core Components

```
T3Sharp/
├── src/
│   ├── TritTypes/              # Core balanced ternary types
│   │   ├── Word9.cs           # 9-trit word type
│   │   ├── Word18.cs          # 18-trit word type
│   │   ├── Word27.cs          # 27-trit word type
│   │   ├── Word36.cs          # 36-trit word type
│   │   ├── BalancedTernary.cs # Balanced ternary operations
│   │   └── TritEncoding.cs    # Format encoding/decoding
│   ├── T3Simulator.Common/    # Shared simulator infrastructure
│   │   ├── ProcessorBase.cs   # Abstract processor base class
│   │   ├── T3Disassembler.cs  # Instruction disassembler
│   │   └── T3Config.cs        # Processor configuration
│   ├── T3Simulator.InOrder/   # In-order processor implementation
│   ├── T3Simulator.OutOfOrder/# Out-of-order processor implementation
│   └── T3Simulator.CLI/       # Command-line interface
└── tests/                      # Unit and integration tests
```

### Word Types

| Type | Trits | Range | Internal Storage |
|------|-------|-------|------------------|
| Word9  | 9     | ±13,122 | short           |
| Word18 | 18    | ±193,710,244 | long         |
| Word27 | 27    | ±2.87 × 10^12 | BigInteger   |
| Word36 | 36    | ±4.22 × 10^16 | BigInteger   |

### Output Formats

- **Ternary**: String of '-', '0', '+' characters (e.g., "+-0+0")
- **9-ary**: W, X, Y, Z, 0, 1, 2, 3, 4 (pairs of trits)
- **27-ary**: N, O, P, Q, R, S, T, U, V, W, X, Y, Z, 0-9, A, B, C, D (triples of trits)
- **Binary**: Standard binary representation

## Quick Start

### Building

```bash
dotnet build src/T3Sharp.sln
```

### Running the CLI

```bash
dotnet run --project src/T3Simulator.CLI/T3Simulator.CLI.csproj
```

### CLI Commands

```
T3> help                    # Show available commands
T3> load program.tsc        # Load program from file
T3> dump registers trinary  # Show registers in ternary format
T3> dump registers nonary   # Show registers in 9-ary format
T3> dump registers 27ary    # Show registers in 27-ary format
T3> step 10                 # Execute 10 instructions
T3> run 100                 # Run up to 100 instructions
T3> breakpoint 0x00000010   # Set breakpoint
T3> trace on                # Enable trace buffer
T3> disassemble             # Show disassembled program
T3> stack                   # Show stack contents
T3> memory 0x00000000 16    # Show 16 memory locations
```

### Input Formats

Programs can be loaded in multiple formats:

1. **Text format**: Simple sequence of '+', '-', '0' characters
2. **Ninary format**: Prefixed with "0n" (e.g., "0nWZX01234")
3. **Tryx format**: Prefixed with "0y" (e.g., "0yNOPQRST")
4. **Binary format**: .bin files with raw binary data

## Example Programs

### Hello World (Text Format)

```
+-0+0-0+0-0+0-0+0-0+0-0+0-0+0
0-0+0-0+0-0+0-0+0-0+0-0+0-0+0
```

### Simple Addition (Ninary Format)

```
0nWZX01234WXYZ01234WXYZ
```

## Processor Configurations

### T3-9 (9-trit word)
- Smallest word size
- Limited address space (128 words)
- Fast execution
- Good for learning ternary arithmetic

### T3-18 (18-trit word)
- Balanced word size
- 16 K-word address space
- Suitable for practical programs
- Default configuration

### T3-27 (27-trit word)
- Large address space
- 128 M-word capacity
- For complex applications
- Uses BigInteger internally

### T3-36 (36-trit word)
- Maximum capacity
- 32 G-word address space
- For large-scale simulations
- Uses BigInteger internally

## Microarchitectures

### In-Order Processor
- Simple, educational design
- One instruction per cycle
- No speculation or out-of-order execution
- Good for understanding ISA semantics

### Out-of-Order Processor
- Advanced pipeline design
- Instruction reordering
- Branch prediction
- Higher performance

### Pipelined Processor
- 5-stage pipeline
- forwarding units
- hazard detection
- Balanced performance

## Development

### Adding a New Processor

1. Create new processor class inheriting from `ProcessorBase<T>`
2. Implement abstract methods:
   - `Step()` - Execute one instruction
   - `LoadProgram()` - Load program into memory
   - `GetState()` - Return current processor state
3. Add configuration to `T3Config.cs`
4. Update CLI to use new processor type

### Adding New Output Format

1. Implement encoding in `TritEncoding.cs`
2. Add format constant in `Program.cs`
3. Update `FormatValue()` method
4. Update help text for new format

## Testing

```bash
# Run all tests
dotnet test tests/

# Run specific test project
dotnet test tests/T3Simulator.Tests/T3Simulator.Tests.csproj

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Balanced ternary concept by Thomas Fowler (1840)
- T3 processor design inspired by Setun computer (Moscow State University, 1958)
- Built with .NET 8.0 SDK

## References

- [Balanced Ternary on Wikipedia](https://en.wikipedia.org/wiki/Balanced_ternary)
- [Setun Computer](https://en.wikipedia.org/wiki/Setun)
- [The Art of Computer Programming, Vol 2](https://www-cs-faculty.stanford.edu/~knuth/taocp.html)

---

**Happy Ternary Computing!** 🌟