# Artifact Dialoguer

[中文](./README.md) | [English](./README-EN.md)

Artifact Dialoguer 是一个适用于 Unity 的高级对话系统框架，提供强大的流程控制、逻辑判定、运行时变量功能。

## 1. 导入包

使用 Unity Package Manager (UPM) 导入：

1. 打开 Unity 编辑器，点击菜单栏 `Window` -> `Package Manager`。
2. 点击左上角 `+` 按钮，选择 `Add package from git URL...`。
3. 填入 `https://github.com/Blind-Guess-Senior/ArtifactDialoguer.git` 并点选 `Add`。使用 `...ArtifactDialoguer.git#tag`
   以指定版本。例如 `...ArtifactDialoguer.git#v0.2.0`

更多信息参见官方文档 `https://docs.unity3d.com/6000.3/Documentation/Manual/upm-ui-giturl.html`.

## 2. 编写对话文件

对话脚本文件以 `.artidial` 为后缀。

对话脚本文件是基于自定义语义规则的纯文本文件，经过编译后存储于 `DialogueStorageObject`，由 `DialogueRunner` 运行。

一个典型的对话脚本文件形如:

```text
::VillageScope::

startBlock:
@ Player
你好
@ 村民A
你好
@ Player
再见
@ 村民A
再见

[unreach] ifCondDialogue1:
@ 村民B
什么！你居然拿到了圣剑？！
[ret]

[cycle] cycledDialogue:
@ 村民A
你找我有什么事吗
[if hasSword == true] ? 我拿到圣剑了 -> [goto ifcondDialogue1]
? 没什么事了 -> [null]
@ 村民A
没什么事的话，就来我这坐坐吧。
```

它的典型构成如下:

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

对话文件的所有具体语法如下:

- **Namespace (命名空间)**

  ```text
  ::VillageScope::
  // 这里应当有若干block
  // ::前后允许任意数量空格
  ```

  组织和划分不同的对话内容域，Block 和 Variable 在同一 Namespace 通用。一个 Namespace 声明将会作用于其之后的所有内容，直到遇到新的
  Namespace 声明或文件结束。Namespace 的名称内部不允许存在空格。

  每个语句都必须有所属的Namespace。

- **Block (语句块)**

  ```text
  startBlock:
  // 这里应当有若干语句
  ```

  对话流的基本运行单元，同一 Namespace 内名称唯一（`BlockName`）。Block 的名称不允许存在空格。`:` 必须紧跟名称。

  自然流(Natural Flow): 当一个 Block 执行完毕后，将会自然流向下一个 Block。Namespace 切换、文件结束都会截断自然流动。标记有
  `[unreach]` 的 Block不会进入自然流。

  Block 可以通过`Goto`命令执行跳转。

  每个语句都必须有所属的Block。

- **注释 (Comments)**

  ```text
  // 这是一行注释，不会在游戏中显示
  ```

  注释不会被视为有效语句。

- **Speaker (说话人)**

  ```text
  @Player
  这里是文本
  每当返回当前文本时，speaker会附带在返回结果中。
  '@' 后允许任意数量空格
  ```

  标记对话的当前发言角色。后续所有文本都归属于该 Speaker，直到遇到新的 Speaker 声明或进入了新的 Block。

  Speaker 声明后必须换行。`@` 后的整行文本都将被视为 Speaker 的名称。

- **文本 (Text & CrossText)**

  ```text
  这是单行文本。
  这会被识别为Block:
  
  {这是允许多次步进拆分的
  带有换行的
  CrossText跨行长文本。}
  
  {
  CrossText将会按照换行符分割为多个元素，每次递进吐出一行。
  注意 '{' 后和 '}' 前的换行符也会被计入。
  }
  
  特别地，[empty]也是一种文本
  ```

  对话文本。分为常规文本与跨行文本。

  常规文本的行首不能是保留符号（`@` `:` `[` `]` `{` `}` `(` `)` 算术符号等）。常规文本直到遇到换行才会中止。

  特别地，如果一行内没有空格且最后存在`:`，这将被视为 Block 定义。

  跨行文本包裹在 `{` `}` 内。其中的所有内容都将被视为文本。

  如果需要在单行文本中包含符号，使用单行的跨行文本是一个简便的方法。

  ```text
  {这个等效于一个单行常规文本，但可以有除了花括号外的各种符号 []():?@->}
  ```

- **空文本 (Empty)**

  ```text
  @[empty]
  这是一段旁白
  
  OR
  
  ? [empty] -> [goto slienceResponseDialogue1]
  ```

  空文本，可用于 Speaker 或选项组文本及常规文本。空文本将表现为一个空字符串。

- **Command (命令)**

  操纵对话流程、进行数据修改的操作节点:

    - `goto`: 跳转到目标 Block。

      ```text
      [if missionComplete1 == true] [goto missionCompleteBlock1]
      
      [if hasSword == true] ? 我拿到圣剑了 -> [goto ifcondDialogue1]
      ? 没什么事了 -> [null]
      ```

    - `ret`: 回到上一次 `goto` 调用处。如果当前 Block 不由 `goto` 进入，`ret` 无效果。

      ```text
      [ret]
      ```

    - `set`: 修改变量（如 `a = 1`），不存在的变量会被声明并赋值。变量用于 `set` 和 `if`。

      等号的左边必须是一个变量名，等号的右边是一个表达式（当前版本不支持复杂表达式，仅支持单个值）。
      表达式可以包含整形字面量、浮点数字面量、布尔值字面量、变量名和算术运算符(`+` `-` `*` `/` `%`)。

      变量具有类型，类型由等号右侧的表达式推断得出。不同类型的变量无法比较。重设变量时不考虑变量原类型。

      ```text
      [set hasA = true]
      [set valueB = 111]
      [set float1 = 1.1]
      [set float1 = 11] // float1现在是int类型
      [set valueC = valueB]
      ```

    - `null`: 空指令。一般用于选项处，表示无操作。

      ```text
      [if hasSword == true] ? 我拿到圣剑了 -> [goto ifcondDialogue1]
      ? 没什么事了 -> [null]
      ```

  Command 在运行中不会有返回值，Command 执行完毕后，将会立刻进入下一个语句。

  Command 如果不在行首，可能会被 Text 的解析视为文本的一部分。一般情况下，建议将 Command 语句放置于行首。

- **选项组 (Options)**

  ```text
  [Attribute] ? 显示文本 -> [Command]
  [Attribute] ? 显示文本 -> [Command]  
  ```

  ```text
  @村民A
  还有什么事吗？
  [if hasSword == true] ? 我拿到圣剑了 -> [goto ifcondDialogue1]
  ? 没什么事了 -> [null]
  
  @村民A
  那就再见吧。
  这段话会在选择 "没什么事了" 选项后出现
  ```

  分支选项，内部包含多个选项子节点。被选择后将会执行对应的 Command。

  其中 `[Attribute]` 非必须。`->` 后必须是一个 Command。显示文本不能出现 `-`。显示文本可以是 `[empty]`。`?` 和 `->`
  前后的换行不影响语义。

  如果需要在显示文本中出现 `-`，可以使用跨行文本。

  ```text
    ? {God-of-War} -> [goto choseGowBlock]
  ```

  连续的仅由空白字符或注释隔开的选项语句将会被视为一组。

  如果需要连续的多组选项，可以使用 `[null]` 分隔。

  ```text
  ? 我先说这一句话 -> [null]
  [null]
  ? 再说这一句话 -> [goto nextBlock]
  ```

- **Attribute (属性)**

  对 Block 或 Statement 生效的修改器，控制节点是否展示。

    - `if`: 可用于 Block Speaker Text Option Command，条件判断，条件满足时才运行该部分。关系运算符左右均为表达式（当前版本不支持复杂表达式，仅支持单个值）。
      对不同类型的值的比较总会被视为不满足条件。

      ```text
      [if hasA == true] 这句话只在 hasA 是 true 的时候才会显示
      ```

    - `once`: 可用于 Block Speaker Text Option Command，标记该语句/语句块仅呈现一次。

      ```text
      [once] oncedBlock:
      这个块内都只会出现一次
      [set hasA = false]
      
      anotherBlock:
      [once] @ villager
      酒不在我这
      我不会再说第二遍了
      
      @ another one
      [once] 这句话只会显示一次
      [once] [set hasA = true]
      [once] ? 这个选项只能选一次 -> [goto winBlock]
      ? 这个选项可以选很多次 -> [goto loseBlock]
      ```

    - `cycle`: 仅用于 Block，标识该 Block 是循环对话，该 Block 结束后，自然流（Natural Flow）将会流向自己。

      ```text
      [cycle] cycledDialogue:
      @ 村民A
      没什么事的话，就来我这坐坐吧。
      ```

    - `unreach`: 仅用于 Block。标识该 Block 不进入自然流（Natural Flow），只能通过 `goto` 命令抵达。建议所有仅由选项抵达的块都附加
      `unreach` 属性。

      ```text
      [unreach] ifCondDialogue1:
      @ 村民B
      什么！你居然拿到了圣剑？！
      ```

  Attribute 的顺序不影响解析，`if` Attribute一定最先解析。如果未满足 `if` 的条件，`once` 不会被触发。

  `cycle` 与 `unreach` 互斥。

## 3. 编译对话文件

在Editor上方的 `Artifact Dialoguer` 选框内点击 `Compile All`， 将编译项目文件夹内所有 `.artidial` 文件，输出 asset 到
`Resources/ArtiDialogue` 文件夹。

依据 Namespace 拆分文件，每个 Namespace 生成一个 asset 文件。

## 4. DialogueRunner

`DialogueRunner` 在游戏进行时控制对话的进展。

对于一个参与对话的游戏内物体，其应当挂载一个 `DialogueRunner` 脚本，并绑定编译后的对话文件，设定初始 Block。

该脚本提供如下方法:

- **`public void Init()`**: 初始化并重置 Runner 的当前状态，挂载指定的初始 Block。将会在 `Awake` 时自动执行一次。
- **`public IDialogueRuntimeResult Next()`**: 对话步进一步，并返回 `IDialogueRuntimeResult`。`IDialogueRuntimeResult`
  包含了当前语句的信息。
- **`public IDialogueRuntimeResult OptionChosen(int index \OR\ DialogueRuntimeResultOption option)`**: 对话步进至选项时（返回
  `DialogueRuntimeResultOptionsGot`），Runner 挂起，禁用 `Next()`，调用该方法并传入选定的选项后，执行选项操作并使对话步进。
- **`public DialogueState ExportSave(bool blockLevelOnly = false)`**: 导出 Runner 的当前状态，以供存档。如果
  `blockLevelOnly` 为 true，导出的状态不记录当前在 Block 中的位置。
- **`public void LoadSave(DialogueState state)`**: 加载存档。存档形式受存档时选项控制。

随后，一般地，需要一个脚本持有 `DialogueRunner` 的引用，并在恰当的时机调用上述提供的方法，根据方法的返回值填充UI内容、处理存档行为等。

## 5. DialogueRuntimeResult

对话步进过程中的语句信息。

所有 `DialogueRuntimeResult` 提供 `public Type ResultType` 属性，该属性指示 Result 的类型。

一般地，填充UI的代码需要枚举 Result 的可能值并执行对应的操作。

- **`DialogueRuntimeResultTextGot`**:

  读取到文本内容。

  ```text
    public string Speaker;
    public string Content;
  ```

  `Speaker` 指示当前文本的 Speaker; `Content` 为当前文本。

  特别地，跨行文本将多次返回 `DialogueRuntimeResultTextGot`，每次的 `Content` 将扩展一行。

- **`DialogueRuntimeResultOptionsGot`**:

  读取到选项组。选项组中仅包含属性检验通过的选项。如 `if` 判定不通过、已经选取过的 `[once]` 选项不会出现。

  ```text
    public List<DialogueRuntimeResultOption> Option;
  ```

  `Option` 包含所有应当展示的选项。

  ```text
    public struct DialogueRuntimeResultOption
    {
      public readonly int Index;
      public string Text;
    }
  ```

  `Index` 为该选项的序号; `Text` 为该选项的显示文本。

  当选择选项时，可以向 `OptionChosen` 传入 `Index` 或 `DialogueRuntimeResultOption` 本身。

- **`DialogueRuntimeResultBlockEnd`**:

  当前所处 Block 已自然执行完毕。再次调用 `Next()` 将尝试进入自然流（Natural Flow）的下一个 Block。如果没有下一个 Block，再次
  `Next()` 将返回 `DialogueRuntimeResultDialogueEnd`。

  `[ret]` 不会返回此结果。

- **`DialogueRuntimeResultDialogueEnd`**:

  对话正常结束。此后尝试步进将只会得到此结果。

- **`DialogueRuntimeResultError`**:

  对话进行异常。当前 Block 不存在时出现。

- **`DialogueRuntimeResultNextDenied`**: 正在等待分支选项选择，`Next()` 被禁用。

---

### TODOs:

- complex arithmetic expression
- re-add compile flagged feature support
- var use before definition check
- extcall support
- unchosen option support (by command like \[empty\])
- english readme.md
-
