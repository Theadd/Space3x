using UnityEngine;

namespace Space3x.InspectorAttributes
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    internal static class DomainReloader
    {
#if !UNITY_EDITOR && RUNTIME_UITOOLKIT_DRAWERS
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnEnterPlayModeInRuntimeBuild()
        {
            // Called once in a runtime player build before anything else   
            Reload();
        }
#endif
        
#if UNITY_EDITOR

        // From UnityEditor.InitializeOnLoadAttribute annotation
        static DomainReloader()
        {
            /*
             * - Domain reloads (recompiles scripts):
             *      - Unity Editor first load.
             *      - When scripts are modified if Auto Refresh is enabled in Preferences > Asset Pipeline.
             *      - Enter Play Mode if Domain Reload is NOT disabled.
             */
            InitializeInEditor();
            Reload();
        }
        
        [UnityEditor.InitializeOnEnterPlayMode]
        static void OnEnterPlayModeInEditor(UnityEditor.EnterPlayModeOptions options)
        {
            if (options.HasFlag(UnityEditor.EnterPlayModeOptions.DisableDomainReload))
            {
                /*
                 * - Enter Play Mode if Domain Reload IS disabled.
                 */
                InitializeInEditor();
                Reload(false);
            }
        }

        private static void InitializeInEditor()
        {
            Debug.Log("[PM!] DomainReloader InitializeInEditor()");
            Debug.Log("[PM!] DomainReloader <color=#FF0000FF>UNREGISTER</color> OnPlayModeStateChanged Callback");
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Debug.Log("[PM!] DomainReloader <color=#00FF00FF>REGISTER</color> OnPlayModeStateChanged Callback");
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange mode)
        {
            if (mode is UnityEditor.PlayModeStateChange.ExitingPlayMode or UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                Debug.Log("[PM!] DomainReloader OnPlayModeStateChanged(): " + mode);
                Reload();
            }
        }
#endif

        private static void Reload(bool isDomainReload = true)
        {
            Debug.Log("[PM!] <b>DomainReloader Reload()</b>");

            PropertyAttributeController.ReloadAll();
            UngroupedMarkerDecorators.ReloadAll();
            if (isDomainReload)
                CachedDrawers.ReloadAll();
            AnnotatedRuntimeType.ReloadAll();
#if UNITY_EDITOR
            Debug.Log("[PM!] RESETTING EDITOR INSPECTORS");
            // if (UnityEditor.Selection.activeTransform != null)
            //     UnityEditor.EditorUtility.SetDirty(UnityEditor.Selection.activeTransform);
#endif
        }
    }
}
