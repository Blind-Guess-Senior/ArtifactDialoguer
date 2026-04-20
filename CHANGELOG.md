# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- 

### Fixed

- 

### Changed

- 

### Removed

-

## [0.2.1] - 2026-04-20

### Added

- Editor tools to clean all compiled dialogues.

### Fixed

- Move sample dialogue file into Tests folder.

## [0.2.0] - 2026-04-20

### Added

- Add save support.
- Add cross text support for option's display content.
- Now lexer will output all illegal tokens.

### Fixed

- Add extension for sample dialogue file so that it won't be compiled by "Compile All".
- Fix \[empty\] lexing in option context.
- Fix cross text at the end of the block gets swallowed.

### Changed

- Now all nodes got an id.
- Runtime once statements record now use node id rather than node reference itself.
- Runtime flag previous in runner now moved to state.

## [0.1.3] - 2026-04-18

### Fixed

- Block declaration and option now will not cause amibiguities in lexing process.

## [0.1.2] - 2026-04-18

### Fixed

- Tests will correctly use sample files in package folder rather than project folder.
- Return statements will now correctly return to the goto statement instead of the beginning of the block.
- Return statements that are not within a goto context now do nothing, just like the behavior when selecting options.

## [0.1.1] - 2026-04-16

### Fixed

- Tests will use sample files in package folder rather than project folder.

## [0.1.0] - 2026-04-15

### Added

- Unity package essential files.
- Lexer implementation.
- Ast definition for compiled dialogue.
- LL(1) parser implementation without complex value expression.
- Asset importer for specified .artidial file extension.
- Simple ScriptedObject storage for compiled dialogue.
- Editor tools for compile all dialogue.
- Dialogue runner VM implementation.

