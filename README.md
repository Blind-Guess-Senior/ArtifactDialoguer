# Artifact Dialoguer

*由Gemini 3.1 Pro(Preview)生成并人工简单修正，未来将会替换*

Artifact Dialoguer 是一个适用于 Unity 的高级对话系统框架，支持自定义对话语法、变量逻辑判定以及流程控制。本指南将帮助你了解如何导入、编写和运行这些对话。

## 1. 导入包

你可以通过 Unity Package Manager (UPM) 导入此 GitHub 仓库：

1. 打开 Unity 编辑器，点击菜单栏 `Window` -> `Package Manager`。
2. 点击左上角的 `+` 按钮，选择 `Add package from git URL...`。
3. 填入 `https://github.com/Blind-Guess-Senior/ArtifactDialoguer.git` 并点选 `Add`。

如有疑问，参见官方文档 `https://docs.unity3d.com/6000.3/Documentation/Manual/upm-ui-giturl.html`.

## 2. 剧本文件命名

所有的对话脚本文件必须使用 `.artidial` 作为文件后缀名。
它是基于自定义语义规则的纯文本文件，在 Unity 内会被解析和编译，最终以一个存有 AST 结构或运行时数据的
`DialogueStorageObject` 作为载体供 `DialogueRunner` 执行。

## 3. 语法详解

一个典型的对话脚本文件形如：

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

它的典型构成如下：
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

以下是对话文件中各个基本结构和概念的作用：

- **Namespace (命名空间)**

  用于组织和划分不同的对话内容域，Block和Variable在同一Namespace通用。一个Namespace作用于其之后的所有内容，直到遇到新的Namespace定义或文件结束。

  每个语句都必须有所属的Namespace。

  ```text
  ::VillageScope::
  // 这里应当有若干block
  // ::前后允许任意数量空格
  ```

- **Block (语句块)**

  对话流的基本运行单元，具有Namespace内唯一的名称标示（`BlockName`）。每个 Block 执行完毕后会隐式流向下一个定义的 Block (
  Natural Flow)，Block 结束会发送特殊的结果。

  Block 可以通过`Goto`命令执行跳转。

  每个语句都必须有所属的Block。

  ```text
  startBlock:
  // 这里应当有若干语句
  ```

- **Speaker (说话人)**

  用来标记某一句对话的当前发言角色。在此标记生效后，后续的文本都将归属于该 Speaker，直到遇到新的 Speaker 声明或进入了新的
  Block。

  ```text
  @Player
  这里是文本
  每次返回当前文本时，speaker会附带在返回结果中。
  '@' 后允许任意数量空格
  ```

- **注释 (Comments)**

  注释不会被视为有效语句。

  ```text
  // 这是一行注释，不会在游戏中显示
  ```

- **常规文本 (Text & CrossText)**

  最普通的角色对白或旁白本身，分为单行展示或允许内部存在换行的文本。解析后将其和讲话人绑定打包发送到前端渲染逻辑。

  ```text
  这是单行文本。
  {这是允许多次步进拆分的
  带有换行的
  CrossText跨行长文本。}
  {
  CrossText将会按照换行符分割为多个元素，每次递进吐出一行。
  注意 '{' 后和 '}' 前的换行符也会被计入。
  }
  特别地，[empty]也是一种文本
  ```

- *空文本 (Empty)*

  空文本，可用于 Speaker 或选项组文本及常规文本。

  ```text
  @[empty]
  这是一段旁白
  
  OR
  
  ? [empty] -> [goto slienceResponseDialogue1]
  ```

- **Attribute (属性标记)**
  针对特定的 Block、文本或者选项生效的前置/后置修改器，用于控制节点本身的展示和生命周期。比如：
    - `if`: 代表条件判断，若条件不满足则完全跳过该部分。
      ```text
      [if hasA == true] 这句话只在 hasA 是 true 的时候才会显示
      ```
    - `once`: 标记某段话或某个选项对于当前runner“仅呈现一次”。
      ```text
      [once] 这句话只会显示一次
      ```
    - `cycle`: 仅用于 Block，标识该 Block 是循环对话，它的 Natural Flow Next 将会是自身。
      ```text
      [cycle] cycledDialogue:
      @ 村民A
      没什么事的话，就来我这坐坐吧。
      ```
    - `unreach`: 仅用于 Block，标识该 Block 不会在正常流程中抵达，只能通过 `goto` 命令抵达。
      ```text
      [unreach] ifCondDialogue1:
      @ 村民B
      什么！你居然拿到了圣剑？！
      ```

  Attribute 的顺序不影响解析，`if` Attribute一定最先解析。如果未满足 `if` 的条件，`once` 不会被触发。

- **Command (命令)**

  直接操纵对话流程以及进行数据修改的操作节点：

    - `goto`: 指定跳转到另一个目标 Block。
      ```text
      [if hasSword == true] ? 我拿到圣剑了 -> [goto ifcondDialogue1]
      ? 没什么事了 -> [null]
      ```
    - `ret`: 从当前被 `goto` 进的调用栈中弹出，返回至上层继续执行。如果当前 Block 不由 `goto` 进入，`ret` 无效果。
      ```text
      [ret]
      ```
    - `set`: 用于在对话运行时的环境中修改或者声明变量（如 `a = 1`）。
      ```text
      [set hasA = true]
      [set valueB = 111]
      ```
    - `null`: 空指令。
      ```text
      [if hasSword == true] ? 我拿到圣剑了 -> [goto ifcondDialogue1]
      ? 没什么事了 -> [null]
      ```

- **选项组 (Options)**
  游戏内抛给玩家的分支，内部包含多个选项子节点。选项被选择后也会执行它挂载的对应 Command。

  ```text
  @村民A
  还有什么事吗？
  [if hasSword == true] ? 我拿到圣剑了 -> [goto ifcondDialogue1]
  ? 没什么事了 -> [null]
  
  @村民A
  那就再见吧。
  这段话会在选择 "没什么事了" 选项后出现
  ```

  注意： `->` 后必须是一个 Command。

## 4. 编译对话文件

在Editor上方的 `Artifact Dialoguer` 选框内点击 `Compile All` 将会自动编译项目文件夹内所有 `.artidial` 文件，输出 asset 到
`Resources/ArtiDialogue` 文件夹。

依据 Namespace 拆分文件。

## 5. DialogueRunner

`DialogueRunner` 在游戏进行时控制对话的进展。

- **`Init()`**: 初始化并重置 Runner 的当前状态（调用栈，内置变量，状态字典归零等），挂载指定的起步 Block（`startBlock`）。
- **`Next()`**: 通知对话向后步进一句，并把最后得到的内容封装在 `IDialogueRuntimeResult` 里返回。如果当前项是命令，会执行命令并步进直到有能返回的结果。
- **`OptionChosen(int index)`**: 当 `Next()` 让对话进入需要玩家输入选项的情境（即返回了 `DialogueRuntimeResultOptionsGot`
  结果）时系统会挂起，调用此方法并传入选择的选项以推动对话继续。

## 6. DialogueRuntimeResult 详解

通过每次调用 `Next()` 你拿到的Result代表了系统当前的状态切片：

- **`DialogueRuntimeResultTextGot`**: 系统获取到了一句常规文本与说话人数据。此时你应该提取里面的文本并在你的 UI
  文字框里打印（Typewriter打字机效果等）。
- **`DialogueRuntimeResultOptionsGot`**: 碰到分支选项点。它会带出校验完可用范围后的实际选项列表，此时你应当使用此数据渲染选项按钮供玩家选择。
- **`DialogueRuntimeResultBlockEnd`**: 表示当前所处的 Block 已经执行完它所有的
  Statement。一般这代表一段情节节点的结束，如果不加干涉接下来会滑落至下一个可连在身后的节点。
- **`DialogueRuntimeResultDialogueEnd`**: 无节点可通，剧情线或脚本流程在此彻底结束。
- **`DialogueRuntimeResultNextDenied`**: 正在等待分支选项选择，`Next()` 被禁用。
- **`DialogueRuntimeResultError`**: 对话进行出现异常。

---

### TODOs:

- complex arithmetic expression
- re-add compile flagged feature support
- var use before definition check
- extcall support
- save support
- unchosen option support (by command like \[empty\])
- global once statement
- value evaluate rules - readme.md
- english readme.md
-

tips:
vars with different type will be treat as non-equal, and compare with them always return false.
