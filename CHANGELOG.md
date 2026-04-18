# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- 

### Changed

- 

### Removed

- 

## [0.1.2] - 2026-04-18

### Fixed

- Tests will correctly use sample files in package folder rather than project folder
- Return statements will now correctly return to the goto statement instead of the beginning of the block.
- Return statements that are not within a goto context now do nothing, just like the behavior when selecting options.

## [0.1.1] - 2026-04-16

### Fixed

- Tests will use sample files in package folder rather than project folder

## [0.1.0] - 2026-04-15

### Added

- Unity package essential files
- Lexer implementation
- Ast definition for compiled dialogue
- LL(1) parser implementation without complex value expression
- Asset importer for specified .artidial file extension
- Simple ScriptedObject storage for compiled dialogue
- Editor tools for compile all dialogue
- Dialogue runner VM implementation

