using System.Collections.Generic;
using System.Linq;
using BlindGuessSenior.ArtifactDialoguer.Backend;
using BlindGuessSenior.ArtifactDialoguer.Frontend;
using NUnit.Framework;
using UnityEngine;

namespace BlindGuessSenior.ArtifactDialoguer.Tests
{
    public class RuntimeDialogueFeatureTests
    {
        private readonly List<Object> _createdObjects = new();

        [SetUp]
        public void SetUp()
        {
            DialogueState.PerNamespaceStates.Clear();
            _createdObjects.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _createdObjects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _createdObjects.Clear();
            DialogueState.PerNamespaceStates.Clear();
        }

        [Test]
        public void OnceAndPOnceUseNamespaceAndRunnerScopes()
        {
            var statementStorage = CompileStorage(
                @"::OncePOnceStatementRuntimeTest::

[cycle] start:
@ narrator
[once] namespace-once
[ponce] per-runner
[once] [ponce] both
always
");

            var runnerA = CreateRunner(statementStorage, "start");
            Assert.That(DrainTextsUntilBoundary(runnerA), Is.EqualTo(new[]
            {
                "namespace-once",
                "per-runner",
                "both",
                "always"
            }));
            Assert.That(DrainTextsUntilBoundary(runnerA), Is.EqualTo(new[] { "always" }));

            var runnerB = CreateRunner(statementStorage, "start");
            Assert.That(DrainTextsUntilBoundary(runnerB), Is.EqualTo(new[] { "per-runner", "always" }));
            Assert.That(DrainTextsUntilBoundary(runnerB), Is.EqualTo(new[] { "always" }));

            var optionStorage = CompileStorage(
                @"::OncePOnceOptionRuntimeTest::

[cycle] start:
@ narrator
choose
            [once] ? namespace option-> [null]
            [ponce] ? runner option-> [null]
            ? normal option-> [null]
");

            var optionRunnerA = CreateRunner(optionStorage, "start");
            AssertNextText(optionRunnerA, "choose");
            AssertNextOptions(optionRunnerA, "namespace option", "runner option", "normal option");
            AssertBlockEnd(optionRunnerA.OptionChosen(0));

            AssertNextText(optionRunnerA, "choose");
            AssertNextOptions(optionRunnerA, "runner option", "normal option");
            AssertBlockEnd(optionRunnerA.OptionChosen(0));

            AssertNextText(optionRunnerA, "choose");
            AssertNextOptions(optionRunnerA, "normal option");

            var optionRunnerB = CreateRunner(optionStorage, "start");
            AssertNextText(optionRunnerB, "choose");
            AssertNextOptions(optionRunnerB, "runner option", "normal option");
        }

        [Test]
        public void NamespaceVariablesAreSharedAcrossRunners()
        {
            var storage = CompileStorage(
                @"::VariableRuntimeTest::

setter:
@ narrator
[set shared = 1]
[set flag = true]
[if shared == 1] 
setter can read int
[if flag == true] 
setter can read bool

[unreach] reader:
@ narrator
[if shared == 1] 
reader can read int
[if flag == true] 
reader can read bool
[set shared = 2]
[if shared == 2] 
reader can update int

[unreach] verifier:
@ narrator
[if shared == 2] 
verifier sees updated int
");

            Assert.That(storage.ScopeVars, Does.Contain("shared"));
            Assert.That(storage.ScopeVars, Does.Contain("flag"));

            var setter = CreateRunner(storage, "setter");
            Assert.That(DrainTextsUntilEnd(setter), Is.EqualTo(new[]
            {
                "setter can read int",
                "setter can read bool"
            }));

            var reader = CreateRunner(storage, "reader");
            Assert.That(DrainTextsUntilEnd(reader), Is.EqualTo(new[]
            {
                "reader can read int",
                "reader can read bool",
                "reader can update int"
            }));

            var verifier = CreateRunner(storage, "verifier");
            Assert.That(DrainTextsUntilEnd(verifier), Is.EqualTo(new[] { "verifier sees updated int" }));
        }

        private DialogueStorageObject CompileStorage(string source)
        {
            var tokens = Lexer.Tokenize(source);
            var story = Parser.Parse(new[] { tokens }).Single();
            var storage = ScriptableObject.CreateInstance<DialogueStorageObject>();
            storage.Namespace = story.Namespace;
            storage.ScopeVars = story.ScopeVars.ToList();
            storage.Blocks = story.Blocks;
            _createdObjects.Add(storage);
            return storage;
        }

        private DialogueRunner CreateRunner(DialogueStorageObject storage, string startBlock)
        {
            var gameObject = new GameObject($"DialogueRunnerTest_{startBlock}");
            gameObject.SetActive(false);
            _createdObjects.Add(gameObject);

            var runner = gameObject.AddComponent<DialogueRunner>();
            runner.dialogueStorageObject = storage;
            runner.startBlock = startBlock;
            runner.Init();
            return runner;
        }

        private static List<string> DrainTextsUntilBoundary(DialogueRunner runner)
        {
            var texts = new List<string>();
            for (var i = 0; i < 32; i++)
            {
                var result = runner.Next();
                switch (result)
                {
                    case DialogueRuntimeResultTextGot textGot:
                        texts.Add(textGot.Content);
                        break;
                    case DialogueRuntimeResultBlockEnd:
                        return texts;
                    case DialogueRuntimeResultDialogueEnd:
                        Assert.Fail("Dialogue ended before reaching a block boundary.");
                        return texts;
                    default:
                        Assert.Fail($"Unexpected runtime result: {result.GetType().Name}");
                        return texts;
                }
            }

            Assert.Fail("Dialogue did not reach a block boundary within the step limit.");
            return texts;
        }

        private static List<string> DrainTextsUntilEnd(DialogueRunner runner)
        {
            var texts = new List<string>();
            for (var i = 0; i < 32; i++)
            {
                var result = runner.Next();
                switch (result)
                {
                    case DialogueRuntimeResultTextGot textGot:
                        texts.Add(textGot.Content);
                        break;
                    case DialogueRuntimeResultBlockEnd:
                        break;
                    case DialogueRuntimeResultDialogueEnd:
                        return texts;
                    default:
                        Assert.Fail($"Unexpected runtime result: {result.GetType().Name}");
                        return texts;
                }
            }

            Assert.Fail("Dialogue did not end within the step limit.");
            return texts;
        }

        private static void AssertNextText(DialogueRunner runner, string expected)
        {
            var result = runner.Next();
            Assert.That(result, Is.TypeOf<DialogueRuntimeResultTextGot>());
            Assert.That(((DialogueRuntimeResultTextGot)result).Content, Is.EqualTo(expected));
        }

        private static void AssertNextOptions(DialogueRunner runner, params string[] expected)
        {
            var result = runner.Next();
            Assert.That(result, Is.TypeOf<DialogueRuntimeResultOptionsGot>());
            var options = ((DialogueRuntimeResultOptionsGot)result).Option.Select(option => option.Text);
            Assert.That(options, Is.EqualTo(expected));
        }

        private static void AssertBlockEnd(IDialogueRuntimeResult result)
        {
            Assert.That(result, Is.TypeOf<DialogueRuntimeResultBlockEnd>());
        }
    }
}
