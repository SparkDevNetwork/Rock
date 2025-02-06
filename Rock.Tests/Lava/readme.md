## Tests for Rock Lava Implementation for the Fluid Templating Framework
These tests are designed for the implementation of Rock Lava built on the Fluid Templating Framework.
![More about Lava](https://community.rockrms.com/Lava)

Fluid is an open-source .NET template engine that implements the Liquid template language.
![More about Fluid](https://github.com/sebastienros/fluid)

## Design Notes

### Unit Tests: Lava Templates as Input

The basic structure of each unit test follows this pattern:
Provide a Lava template string as input, and compare the output produced by the rendering engine to the expected output.

This approach is preferred to testing the filter functions by calling them directly because:
* it allows the tests to be designed in a way that is portable between framework implementations.
* it ensures that the filters are tested in the way they will be used in production.
