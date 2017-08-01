# Elements of Computing (Nand2Tetris)

These files are a distillation of the projects I completed for the [Nand2Tetris](http://nand2tetris.org/) course.  In the interests of complying with the authors' wishes that solutions to the course not be made publically accessible, these will be available only briefly for portfolio purposes, and I have posted them in a completed form that is unlikely to be particularly useful to someone attempting to legitimately complete the projects one at a time.

The course involves 12 projects arranged into 2 major themes.  The first half begins with a hardware simulator and a pre-defined NAND logic gate.  From this NAND gate, I constructed increasingly complex logic gates and assemblies, culminating in a simple van Neumann-architecture computer involving a ram bank, read-only instruction memory, an arithmetic logic unit, and two registers.

The second half begins writing instructions for this "Hack" computer platform in ascending levels of abstraction.  I began by implementing a simple assembly language, then used that to build a stack-based virtual machine emulator.  The next two projects involved writing a compiler to compile a simple high-level programming language, Jack, into virtual machine instructions. Finally, I wrote a set of base libraries (or operating system) for the virtual machine using the Jack language.  The final resulting toolchain can be used to convert high-level Jack code into Hack assembly, which can then be run on the architecture implemented in the first half.

### Project directories, in order of completion:

 * Assembler - processes a text file of "Hack" assembly instructions and translates it into a series of binary instructions for the Hack computer platform designed in the course.  For course purposes, this is a text file listing the binary words, not an actual binary file.
 * VM Emulator - accepts a text file of instructions for a stack-based virtual machine (or a directory containing multiple such files) and outputs a single Hack assembly language file.
 * Compiler - tokenizes a program written in the high-level "Jack" language and then compiles it into virtual machine bytecode instructions.  Accepts a directory of .jack files and outputs one virtual machine file for each class (.jack file) in the program.
 * VM OS - a set of basic libraries used for screen access, memory management, keyboard input, string and array processing, etc. for programs running on the virtual machine.  The components are written in the Jack language and the compiler is used to produce the VM code.
