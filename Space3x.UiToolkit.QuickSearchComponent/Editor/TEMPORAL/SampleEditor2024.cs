//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using Space3x.Attributes.Types;
//using Space3x.InspectorAttributes.Editor.Drawers;
//using Space3x.InspectorAttributes.Types;
//using Space3x.InspectorAttributes.Utilities;
//using Unity.VisualScripting;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.UIElements;
//using SerializableType = Space3x.InspectorAttributes.Types.SerializableType;
//
//namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
//{
//    [InitializeOnLoad]
//    public class SampleEditor2024 : EditorWindow
//    {
////        [SerializeField]
////        private VisualTreeAsset m_VisualTreeAsset = default;
//    
//        [SerializeField]
//        private StyleSheet m_StyleSheet;
//
//        private VisualElement m_Content;
//
//        static SampleEditor2024() => EditorApplication.update += Startup;
//    
//        #region EditorWindow stuff
//        static void Startup()
//        {
//            EditorApplication.update -= Startup;
//            ShowWindow();
//        }
//
//        private static void ShowWindow()
//        {
//            var window = GetWindow<SampleEditor2024>();
//            window.titleContent = new GUIContent("SampleEditor2024");
//            window.Show();
//        }
//    
//        [MenuItem("Tools/SampleEditor2024")]
//        public static void ShowExample()
//        {
//            SampleEditor2024 wnd = GetWindow<SampleEditor2024>();
//            wnd.titleContent = new GUIContent("SampleEditor2024");
//        }
//        #endregion
//    
//        public void CreateGUI()
//        {
//            // Each editor window contains a root VisualElement object
//            VisualElement root = rootVisualElement;
//
//            if (root != null && m_StyleSheet != null)
//            {
//                root.styleSheets.Add(m_StyleSheet);
//            }
//            ThirdTestMe();
//
//            // VisualElements objects can contain other VisualElement following a tree hierarchy.
//            VisualElement label = new Button(TestMeSecond) { text = "TestMe M" };
//            TextElement info = new TextElement()
//            {
//                style = { flexGrow = 1f },
//                text = @$"<b>TODO:</b>
//* UnityEditor.Search.QueryEngine<T>; @SEE: 
//  + https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Search.QueryEngine_1.html
//  + https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Search.QueryEngine.html
//* UnityEditor.Search.FuzzySearch.FuzzyMatch; @SEE:
//  + https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Search.FuzzySearch.FuzzyMatch.html
//* UnityEditor.Compilation.Assembly; @SEE:
//  + https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Compilation.Assembly.html
//
//*** SUPER!!!! https://docs.unity3d.com/6000.0/Documentation/ScriptReference/TypeCache.html"
//            };
//            root.Add(label);
//            root.Add(info);
//        
//            m_Content = new QuickSearchElement()
//            {
//                style = { flexGrow = 1f }
//            };
//            root.Add(m_Content);
//        
//            root.style.flexGrow = 1f;
//        
//        }
//
//        private void ThirdTestMe()
//        {
//            var primitiveKeys = QuickType.Primitives.Keys.ToList();
//            primitiveKeys = primitiveKeys.Skip(3).ToList();
//            foreach (var type in primitiveKeys)
//            {
//                Debug.Log($"[Type]: {type.Name} (Primitive: {type.IsPrimitive.ToString()})");
//            }
//        }
//
//        private async void TestMeSecond()
//        {
//
//            await XmlDocumentationGenerator.GenerateAll();
//            
////            var files = Directory.GetFiles(@"S:\Unity6000\Space3x 6000.0.0b13\", "*.csproj", SearchOption.TopDirectoryOnly);
////            foreach (var file in files)
////            {
////                Debug.Log($"File: {file}");
////                try
////                {
////                    var res = XmlDocumentationGenerator.Generate(file);
////                    Debug.Log($"res : {res}");
////                }
////                catch (Exception e)
////                {
////                    Debug.LogException(e);
////                }
////            }
//            
////            var docsPath = BoltCore.Paths.assemblyDocumentations;
////            var projectPath = @"S:\Unity6000\Space3x 6000.0.0b13\Assembly-CSharp.csproj";
////            Debug.Log($"docsPath: {docsPath}");
////            var res = XmlDocumentationGenerator.Generate(projectPath);
////            Debug.Log($"res : {res}");
//            
////            NamedType nTypeA = new NamedType(typeof(TypeSelectorDrawer));
////            NamedType nTypeB = new NamedType(typeof(BaseTypeDrawer<SerializableType, TypeSelectorAttribute>));
////            NamedType nTypeC = new NamedType(typeof(BaseTypeDrawer<,>));
////
////            Type mTypeA = typeof(string[]);
////
////            var attrs = Enum.GetNames(typeof(TypeAttributes));
////            var allTypeAttributes = attrs.Select(attrName =>
////                Enum.Parse<TypeAttributes>(attrName, false)).AsReadOnlyList();
////        
////
////            Debug.Log($" ----------------------- ATTRIBUTES ----------------------- ");
////            foreach (var attr in attrs)
////                Debug.Log($"{attr}");
////        
////            Debug.Log($" ----------------------- ISPLAY PARTS ----------------------- ");
////            ((Type)nTypeC).GetDisplayParts();
////            Debug.Log($" ----------------------- IERARCHY PARTS ----------------------- ");
////            ((Type)nTypeC).GetHierarchyParts();
//        }
//
//        private void TestMe()
//        {
//            // TypeSelectorDrawer : BaseTypeDrawer<SerializableType, TypeSelectorAttribute>
//            SerializableType sTypeA = new SerializableType(typeof(TypeSelectorDrawer));
//            SerializableType sTypeB = new SerializableType(typeof(BaseTypeDrawer<SerializableType, TypeSelectorAttribute>));
//            SerializableType sTypeC = new SerializableType(typeof(BaseTypeDrawer<,>));
//            
//            NamedType nTypeA = new NamedType(typeof(TypeSelectorDrawer));
//            NamedType nTypeB = new NamedType(typeof(BaseTypeDrawer<SerializableType, TypeSelectorAttribute>));
//            NamedType nTypeC = new NamedType(typeof(BaseTypeDrawer<,>));
//
//            Type mTypeA = typeof(string[]);
//            Type mTypeB = typeof(IList<>);
//            Type mTypeC = typeof(IBindable);
//            
//            
//            var typeNames = new List<string> {
//                sTypeA.TypeName, sTypeB.TypeName, sTypeC.TypeName, 
//                nTypeA.TypeName, nTypeB.TypeName, nTypeC.TypeName, 
//                mTypeA.FullTypeName(), mTypeB.FullTypeName(), mTypeC.FullTypeName()};
//
//            Debug.Log("-- FullTypeNames --");
//            foreach (var typeName in typeNames) Debug.Log(typeName);
//            // Debug.Log(string.Join("\n", typeNames));
//            Debug.Log("   ");
//
//            List<PropertyName> pNames = new List<PropertyName>();
//
//            foreach (var typeName in typeNames)
//            {
//                pNames.Add(new PropertyName(typeName));
//            }
//            Debug.Log("-- PropertyNames --");
//            foreach (var pName in pNames) Debug.Log(pName.ToString());
//            Debug.Log("   ");
//
////        var systemTypes = new List<Type> { (Type)sTypeA, (Type)sTypeB, (Type)sTypeC, (Type)nTypeA, (Type) nTypeB, (Type) nTypeC, mTypeA, mTypeB, mTypeC };
////            
////        Debug.Log("-- ObsoleteTypeNames --");
////        foreach (Type systemType in systemTypes)
////        {
////            Debug.Log(systemType.FullTypeNameAsReturnedByFieldsOfSerializedProperties());
////        }
////        Debug.Log("   ");
////            
////        Debug.Log("-- .ToString() --");
////        foreach (Type systemType in systemTypes)
////        {
////            Debug.Log(systemType.ToString());
////        }
////        Debug.Log("   ");
//        }
//    }
//}
