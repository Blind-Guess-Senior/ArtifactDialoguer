using UnityEditor;
using UnityEditor.Build;

// ReSharper disable CheckNamespace

namespace BlindGuessSenior.ArtifactDialoguer.Editor.SettingToggler
{
    /// <summary>
    /// Editor tools for simplify compile flag management.
    /// </summary>
    public static class CompileFlagInterface
    {
        #region Fields

        /// <summary>
        /// Parent menu path for all item of Artifact Dialoguer.
        /// </summary>
        private const string MenuPath = "Artifact Dialoguer/";

        #endregion

        #region Options

        #region Frontend Options

        #region Defualt Namespace Option

        /// <summary>
        /// Menu path of frontend's default namespace option.
        /// </summary>
        private const string OptionsFrontendDefaultNamespacePath = "Options/Frontend/Default Unified Namespace";

        /// <summary>
        /// Compile flags of frontend's default namespace option.
        /// <br/>
        /// It will set namespace to "__Artifact_Dialoguer_Default_Namespace__" for all files with undefined namespaces.
        /// <br/>
        /// When disabled, every file got its own path-based unique namespace.
        /// </summary>
        private const string DefineSymbolOptionsFrontendDefaultNamespace =
            "__ARTIFACT_DIALOGUER__OPTION__DEFAULT_NAMESPACE";

        /// <summary>
        /// Toggle frontend's default namespace option by editing scripting define symbols.
        /// </summary>
        [MenuItem(MenuPath + OptionsFrontendDefaultNamespacePath, false, -900)]
        private static void ToggleOptionsFrontendDefaultNamespace()
        {
            var namedBuildTarget = GetNamedBuildTarget();

            var defines = GetScriptingDefineSymbols(namedBuildTarget);

            if (defines.Contains(DefineSymbolOptionsFrontendDefaultNamespace))
            {
                // If enabled, then disable.
                defines = defines.Replace(DefineSymbolOptionsFrontendDefaultNamespace, "").Replace(";;", ";").Trim(';');
            }
            else
            {
                // If disabled, then enable.
                if (!string.IsNullOrEmpty(defines))
                {
                    defines += ";";
                }

                defines += DefineSymbolOptionsFrontendDefaultNamespace;
            }

            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
        }

        /// <summary>
        /// Display a check mark next to the menu item to indicate whether it is currently enabled.
        /// </summary>
        /// <returns>Always true.</returns>
        [MenuItem(MenuPath + OptionsFrontendDefaultNamespacePath, true)]
        private static bool ValidateToggleOptionsFrontendDefaultNamespace()
        {
            var namedBuildTarget = GetNamedBuildTarget();

            var defines = GetScriptingDefineSymbols(namedBuildTarget);

            Menu.SetChecked(MenuPath + OptionsFrontendDefaultNamespacePath,
                defines.Contains(DefineSymbolOptionsFrontendDefaultNamespace));

            return true;
        }

        #endregion

        #region Explicit Namespace & Block Option

        /// <summary>
        /// Menu path of frontend's explicit namespace & block option.
        /// </summary>
        private const string OptionsFrontendExplicitNamespaceAndBlockPath =
            "Options/Frontend/Explicit Namespace & Block";

        /// <summary>
        /// Compile flags of frontend's explicit namespace & block option.
        /// <br/>
        /// It will treat behaviors in the file that do not explicitly specify a namespace and block as errors.
        /// <br/>
        /// When disabled, compiler will add default namespace & block for undefined part in file.
        /// </summary>
        private const string DefineSymbolOptionsFrontendExplicitNamespaceAndBlock =
            "__ARTIFACT_DIALOGUER__OPTION__EXPLICIT_NAMESPACEANDBLOCK";

        /// <summary>
        /// Toggle frontend's default namespace option by editing scripting define symbols.
        /// </summary>
        [MenuItem(MenuPath + OptionsFrontendExplicitNamespaceAndBlockPath, false, -900)]
        private static void ToggleOptionsFrontendExplicitNamespaceAndBlock()
        {
            var namedBuildTarget = GetNamedBuildTarget();

            var defines = GetScriptingDefineSymbols(namedBuildTarget);

            if (defines.Contains(DefineSymbolOptionsFrontendExplicitNamespaceAndBlock))
            {
                // If enabled, then disable.
                defines = defines.Replace(DefineSymbolOptionsFrontendExplicitNamespaceAndBlock, "").Replace(";;", ";")
                    .Trim(';');
            }
            else
            {
                // If disabled, then enable.
                if (!string.IsNullOrEmpty(defines))
                {
                    defines += ";";
                }

                defines += DefineSymbolOptionsFrontendExplicitNamespaceAndBlock;
            }

            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
        }

        /// <summary>
        /// Display a check mark next to the menu item to indicate whether it is currently enabled.
        /// </summary>
        /// <returns>Always true.</returns>
        [MenuItem(MenuPath + OptionsFrontendExplicitNamespaceAndBlockPath, true)]
        private static bool ValidateToggleOptionsFrontendExplicitNamespaceAndBlock()
        {
            var namedBuildTarget = GetNamedBuildTarget();

            var defines = GetScriptingDefineSymbols(namedBuildTarget);

            Menu.SetChecked(MenuPath + OptionsFrontendExplicitNamespaceAndBlockPath,
                defines.Contains(DefineSymbolOptionsFrontendExplicitNamespaceAndBlock));

            return true;
        }

        #endregion

        #endregion

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get current named build target.
        /// </summary>
        /// <returns>The named build target of current platform.</returns>
        private static NamedBuildTarget GetNamedBuildTarget()
        {
            // Get right named build target.
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

            return namedBuildTarget;
        }

        /// <summary>
        /// Get project scripting define symbols of given named build target.
        /// </summary>
        /// <param name="namedBuildTarget">The named build target want to get scripting define symbols.</param>
        /// <returns>The define symbols of given named build target.</returns>
        private static string GetScriptingDefineSymbols(NamedBuildTarget namedBuildTarget)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);

            return defines;
        }

        /// <summary>
        /// Used to find this file.
        /// </summary>
        public static void CompileFlagInterfaceNavigator()
        {
        }

        #endregion
    }
}