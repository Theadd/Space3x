using Space3x.UiToolkit.Types;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    [UxmlElement]
    [HideInInspector]
    public partial class FileDialogField : TextField
    {
        public Button OpenFileButton { get; }
        
        /// <summary>
        /// The dialog window title.
        /// </summary>
        [UxmlAttribute, CreateProperty]
        public string DialogTitle { get; set; }
        
        /// <summary>
        /// The default value.
        /// </summary>
        [UxmlAttribute, CreateProperty]
        public string DefaultValue { get; set; }

        /// <summary>
        /// The file extensions to filter in the open file dialog without preceding period character. Separate multiple
        /// extensions with comma.
        /// </summary>
        [UxmlAttribute, CreateProperty]
        public string FileExtension { get; set; } = "exe";
        
        /// <summary>
        /// Whether the shown dialog is a save file dialog or an open file dialog.
        /// </summary>
        [UxmlAttribute, CreateProperty]
        public bool IsSaveFileDialog { get; set; } = false;
        
        /// <summary>
        /// The placeholder text to display in the "Save As" text field when the <see cref="IsSaveFileDialog"/> is true.
        /// This is the name of the file to be saved.
        /// </summary>
        [UxmlAttribute, CreateProperty]
        public string DefaultName { get; set; } = "";
        
        /// <summary>
        /// Whether the path is relative to the project folder or an absolute path instead.
        /// </summary>
        [UxmlAttribute, CreateProperty]
        public bool RelativeToProject { get; set; } = true;
        
        public FileDialogField()
        {
            OpenFileButton = new Button(OnButtonClick) { text = "..." };
            Add(OpenFileButton);
        }

        private void OnButtonClick()
        {
            var initialPath = "";
            var newValue = "";
            if (string.IsNullOrEmpty(value))
            {
                if (IsSaveFileDialog)
                    initialPath = string.IsNullOrEmpty(DefaultValue) 
                        ? Paths.project
                        : Paths.AbsolutePath(DefaultValue);
                else
                    initialPath = string.IsNullOrEmpty(DefaultValue) ? "" : Paths.RelativePath(DefaultValue);
            }
            else
            {
                initialPath = IsSaveFileDialog ? Paths.AbsolutePath(value) : Paths.RelativePath(value);
            }

            if (!IsSaveFileDialog)
                newValue = EditorUtility.OpenFilePanel(DialogTitle ?? "Open", initialPath, FileExtension);
            else
                newValue = EditorUtility.SaveFilePanel(DialogTitle ?? "Save", initialPath, DefaultName, FileExtension);
            
            if (!string.IsNullOrEmpty(newValue))
                value = RelativeToProject ? FileUtil.GetProjectRelativePath(newValue) : newValue;
        }
    }
}
