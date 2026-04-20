# Artifact Dialoguer

[中文](./README.md) | [English](./README-EN.md)

*English Version is translated by Gemini 3.1 Pro (Preview).*

Artifact Dialoguer is an advanced dialogue system framework for Unity, providing powerful flow control, logical
evaluation, and runtime variable features.

## 1. Importing the Package

Import using the Unity Package Manager (UPM):

1. Open the Unity Editor, click on `Window` -> `Package Manager` from the menu bar.
2. Click the `+` button in the top left corner, and select `Add package from git URL...`.
3. Enter `https://github.com/Blind-Guess-Senior/ArtifactDialoguer.git` and click `Add`. Use
   `...ArtifactDialoguer.git#tag` to specify a version (e.g., `...ArtifactDialoguer.git#v0.2.0`).

For more information, refer to the
official [Unity documentation](https://docs.unity3d.com/6000.3/Documentation/Manual/upm-ui-giturl.html).

## 2. Writing Dialogue Files

Dialogue script files use the `.artidial` extension.

These are plain text files based on custom semantic rules. After compilation, they are stored in a
`DialogueStorageObject` and executed by the `DialogueRunner`.

A typical dialogue script looks like this:

```text
::VillageScope::

startBlock:
@ Player
Hello!
@ VillagerA
Hello.
@ Player
Goodbye!
@ VillagerA
See you.

[unreach] ifCondDialogue1:
@ VillagerB
What! You actually got the Holy Sword?!
[ret]

[cycle] cycledDialogue:
@ VillagerA
Did you need something?
[if hasSword == true] ? I got the Holy Sword -> [goto ifcondDialogue1]
? Not really -> [null]
@ VillagerA
If you don't have anything going on, stay a while.
```

Its basic structure is:

```text
Namespace
    Block
        Statements
    Block
        Statements
Namespace
    Block
        Statements
```

Detailed syntax rules for dialogue files:

- **Namespace**

  ```text
  ::VillageScope::
  // Several blocks go here
  // Any amount of whitespace is allowed around ::
  ```

  Organizes and groups different conversational scopes. Blocks and Variables are shared within the same Namespace. A
  Namespace declaration affects everything that follows until a new Namespace declaration or the end of the file. Spaces
  are not allowed within the Namespace name itself.

  Every statement must belong to a Namespace.

- **Block**

  ```text
  startBlock:
  // Several statements go here
  ```

  The basic execution unit of the dialogue flow. Must be uniquely named (`BlockName`) within the same Namespace. Block
  names cannot contain spaces, and `:` must immediately follow the name.

  Natural Flow: When a Block is finished, it naturally flows into the next Block. Namespace shifts or the end of the
  file interrupts this flow. Blocks marked with `[unreach]` will not be entered via Natural Flow.

  You can jump to a Block using the `Goto` command.

  Every statement must belong to a Block.

- **Comments**

  ```text
  // This is a comment line; it will not be displayed in the game.
  ```

  Comments are ignored during compilation.

- **Speaker**

  ```text
  @Player
  Text goes here
  Any text returned will include the current speaker.
  Any amount of whitespace is allowed after '@'.
  ```

  Marks the current speaker. All subsequent text belongs to this Speaker until a new Speaker is declared or a new Block
  begins.

  You must break the line after the Speaker declaration. The entire remaining line after `@` is treated as the Speaker's
  name.

- **Text & CrossText**

  ```text
  This is a single-line text.
  This is recognized as a Block:
  
  {This is a
  multi-line
  CrossText with line breaks.}
  
  {
  A CrossText is split element by element based on line breaks, revealing one extra line per step.
  Note that line breaks directly after '{' and before '}' are also counted.
  }
  
  Specifically, [empty] is also a type of text.
  ```

  Dialogue text. Divides into regular text and CrossText.

  The beginning of a regular text line cannot be a reserved symbol (`@`, `:`, `[`, `]`, `{`, `}`, `(`, `)`, arithmetic
  operators, etc.). Regular text runs until a line break is encountered.

  Specifically, if a line has no spaces and ends with `:`, it will be parsed as a Block definition.

  CrossText is enclosed in `{` and `}`. Everything inside is treated as text.

  If you need to include reserved symbols in a single line of text, using a single-line CrossText is a great workaround.

  ```text
  {This acts like a single-line text but can contain symbols like []():?@-> without parsing issues.}
  ```

- **Empty Text**

  ```text
  @[empty]
  This acts as a narration.
  
  OR
  
  ? [empty] -> [goto slienceResponseDialogue1]
  ```

  An empty text node. Can be used for Speakers, option texts, or standard text logic. It manifests as an empty string
  `""` at runtime.

- **Command**

  Operations responsible for manipulating the dialogue flow and variables:

    - `goto`: Jumpt to the target Block.

      ```text
      [if missionComplete1 == true] [goto missionCompleteBlock1]
      
      [if hasSword == true] ? I got the Holy Sword -> [goto ifcondDialogue1]
      ? Not really -> [null]
      ```

    - `ret`: Returns to the calling `goto` statement. If the current block wasn't entered via `goto`, `ret` has no
      effect.

      ```text
      [ret]
      ```

    - `set`: Modifies a runtime variable (e.g., `a = 1`). Non-existent variables will be declared and assigned.
      Variables are utilized by `set` and `if`.

      The left side of the equals sign must be a variable name, and the right side an expression (currently only
      single-value literal expressions and simple math are supported).
      Expressions can contain integer/float/boolean literals, references to other variables, and arithmetic operators (
      `+` `-` `*` `/` `%`).

      Variables are strongly typed, inferred from the right-side expression. Values of different types cannot be
      compared directly.

      ```text
      [set hasA = true]
      [set valueB = 111]
      [set float1 = 1.1]
      [set float1 = 11] // float1 is now typed as an int
      [set valueC = valueB]
      ```

    - `null`: No operation. Often used in options to do nothing.

      ```text
      [if hasSword == true] ? I got the Holy Sword -> [goto ifcondDialogue1]
      ? Not really -> [null]
      ```

  Commands do not return values during runtime. After execution, the runner immediately proceeds to the next statement.
  It's recommended to place commands at the beginning of a line to avoid them being accidentally swallowed by text
  lines.

- **Options**

  ```text
  [Attribute] ? Display Text -> [Command]
  [Attribute] ? Display Text -> [Command]  
  ```

  ```text
  @VillagerA
  Anything else?
  [if hasSword == true] ? I got the Holy Sword -> [goto ifcondDialogue1]
  ? Not really -> [null]
  ```

  Branching options containing multiple sub-nodes. Upon selection, the corresponding Command is executed.

  `[Attribute]` is optional. A Command must be placed after `->`. The option's display text cannot contain `-`, but can
  be `[empty]`. Line breaks around `?` and `->` do not affect semantics.

  To use a `-` in your option text, wrap the display text in a CrossText bracket:

  ```text
    ? {God-of-War} -> [goto choseGowBlock]
  ```

  Continuous option statements (separated only by whitespace or comments) will be automatically grouped into a single
  selection prompt.

  To separate successive option groups, you can place a `[null]` command in between:

  ```text
  ? First line choice -> [null]
  [null]
  ? Second line choice -> [goto nextBlock]
  ```

- **Attribute**

  Modifiers for Blocks or Statements that evaluate display conditions.

    - `if`: Can be used on Blocks, Speakers, Text, Options, and Commands. Evaluates conditional expressions; the node
      runs only if the condition is met. Does not evaluate cross-type logic.

      ```text
      [if hasA == true] This sentence only appears if hasA is true.
      ```

    - `once`: Can be used on Blocks, Speakers, Text, Options, and Commands. Marks that this statement/block can only
      ever trigger/present once.

      ```text
      [once] oncedBlock:
      Every statement within this block only happens once.
      [set hasA = false]
      
      anotherBlock:
      [once] @ villager
      I don't have your drink.
      I'm not repeating myself.
      
      @ another one
      [once] This sentence only appears once.
      [once] [set hasA = true]
      [once] ? You can only pick this option once -> [goto winBlock]
      ? You can pick this repeatedly -> [goto loseBlock]
      ```

    - `cycle`: Used only for Blocks. Indicates that the natural flow of this block will loop back unto itself upon
      completion.

      ```text
      [cycle] cycledDialogue:
      @ VillagerA
      Come sit down whenever.
      ```

    - `unreach`: Used only for Blocks. Isolates this Block from Natural Flow, making it accessible only via the `goto`
      command. Highly recommended for blocks serving strictly as branching responses.

  Attribute parsing order does not matter, but `if` attributes are always evaluated first. If an `if` condition fails,
  `once` triggers won't be consumed.
  `cycle` and `unreach` are mutually exclusive.

## 3. Compiling Dialogue Files

Click `Compile All` under the `Artifact Dialoguer` top menu in the Unity Editor to compile all `.artidial` files within
the active project. Serialized `.asset` files are generated in `Resources/ArtiDialogue`.

Files are split according to Namespaces—one asset file is generated per Namespace.

## 4. DialogueRunner

`DialogueRunner` controls the progression of dialogues in real-time.

An in-game GameObject participating in dialogue logic should attach a `DialogueRunner` script, referencing a compiled
`DialogueStorageObject` asset and configuring a starting Block.

Key APIs:

- **`public void Init()`**: Initializes and resets Runner variables and assigns the entry Block. It executes
  automatically in `Awake`.
- **`public IDialogueRuntimeResult Next()`**: Advances the dialogue by one step, returning an `IDialogueRuntimeResult`
  detailing the current statement's data.
- **`public IDialogueRuntimeResult OptionChosen(int index \OR\ DialogueRuntimeResultOption option)`**: Called after
  `Next()` returns a `DialogueRuntimeResultOptionsGot` state. Resumes the Runner with the chosen option index, executes
  the selection constraint, and returns an updated result step.
- **`public DialogueState ExportSave(bool blockLevelOnly = false)`**: Exports the current local state of the Runner for
  saving purposes. If `blockLevelOnly` is true, the state ignores progress *inside* the block.
- **`public void LoadSave(DialogueState state)`**: Applies loaded configuration back into the Runner.

Typically, a custom UI Manager script will maintain a reference to `DialogueRunner`, calling these methods and utilizing
`switch/case` logic with the result variables to draw UI buttons, texts, and execute overarching save/load behaviors.

## 5. DialogueRuntimeResult

Contains context-specific data acquired during the dialogue progression.
You can query the exact result type with the `public Type ResultType` property.

- **`DialogueRuntimeResultTextGot`**:

  Standard text result.

  ```text
    public string Speaker;
    public string Content;
  ```

  `Speaker` is the character talking. `Content` is the spoken text.
  CrossText will broadcast `DialogueRuntimeResultTextGot` multiple times sequentially, with one extra line of `Content`
  per step.

- **`DialogueRuntimeResultOptionsGot`**:

  Received valid options. Any option failing its `if` condition or consumed `[once]` parameter will not be listed.

  ```text
    public List<DialogueRuntimeResultOption> Option;
  ```

  `Option` stores the available branches to select.

  ```text
    public struct DialogueRuntimeResultOption
    {
      public readonly int Index;
      public string Text;
    }
  ```

  When picking a response, pass the `Index` (or the element itself) to the `OptionChosen` API.

- **`DialogueRuntimeResultBlockEnd`**:

  The current Block has inherently exhausted its statements. Calling `Next()` again enters the next consecutive Block by
  Natural Flow. If no additional Blocks exist, `Next()` returns `DialogueRuntimeResultDialogueEnd`.

- **`DialogueRuntimeResultDialogueEnd`**:

  The conversation has concluded safely. Subsequent steps return the same signal.

- **`DialogueRuntimeResultError`**:

  Yielded upon failing to interpret or discover the current target block.

- **`DialogueRuntimeResultNextDenied`**:

  The system is currently halted on an active option prompt and expects an `OptionChosen` call. Calling `Next` directly
  returns this fallback warning.

---

### TODOs:

- Complex arithmetic expression
- Re-add compile flagged feature support
- Var use before definition check
- Extcall support
- Unchosen option support (by command like \[empty\])
- English readme.md (completed)

