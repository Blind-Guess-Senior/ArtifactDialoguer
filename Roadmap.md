# Roadmap of Artifact Dialoguer

### v0.5

- [ ] Escape characters support.
- [ ] Create assets menu support for `.artidial` file. Although it will take no effect after v0.7.
- [ ] `DialogueReturned` result when `ret`.

### v0.6

- [ ] Inline variable usage. Like bash's variable replacement by $. Implement stright way first.

### v0.7

- [ ] Split to backend and frontend. Use OCaml to compile, and provide Unity frontend to run dialogue.

### v0.8 and later

#### Multi-language update

- [ ] Use text library to store text dialogues.
- [ ] Support multi-language through this. Inline variable evaluation should be considered. e.g. `float 5.41` should displayed as `5,41` in German and `5.41` in English.

#### Option update

- [ ] No-chosen option support. Which means it do not displayed but can be chosen by sending "unchosen".
- [ ] Displayable but only conditional (including `if` and `once`) chooseable option.
- [ ] Skip empty option group.
- [ ] Maybe `elseif` `else` support.

#### Once update

- [ ] `else` for `once`.

#### Compile update

- [ ] `extcall` support.
- [ ] Dataflow analytics.
- [ ] Complex arithmetic expression support.
- [ ] Pre-compile hook for expanding compile args support.

#### Variable update

- [ ] FP variable that stores a expression rather than a result. Maybe a default behaviour, and current evaluate way will be alternative by explicitly use `evaluate()`.
- [ ] Enum variable support. Maybe just implement by `set a_variable = EnumClassName.SpecifiedEnum` without a explicit declaretion.
- [ ] Block ref type variable support.
- [ ] Maybe enum value mapping in namespace. e.g. `Gender.M` -> "he" `Gender.F` -> "she".
- [ ] Switch-based variable replacement. e.g. if pie_count = 1 then replaced by "A pie" and otherwise replaced by "some pies", or even "{pie_count} pies".

#### Command update

- [ ] End block command `stop`. Which would immediately stop that block and set pointer to next block as if current block is ended by natural flow.
- [ ] Standard `extcall` library support. Including `visited` `random` and other arithmetic function

#### Group update

- [ ] Line group support. Which would randomly return one of statements in groups each time.
- [ ] Block group support. Which should looks like several normal blocks with same name. And they will be compiled to unique block with unique suffix, while the original block will be compiled to a line-group-only block with `goto`.

#### Others

- [ ] Save support.
- [ ] Considerable custom attribute support for runner result receiver. If so, some pre-defined custom attribute should be added. e.g. `lastline` (of a block). Also, text library should support query by special custom attribute (maybe start with some symbol) to allow easily copy dialogue texts.



