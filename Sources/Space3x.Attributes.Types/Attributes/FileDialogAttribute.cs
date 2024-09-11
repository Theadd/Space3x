using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Displays a TextField with a customizable file picker dialog on the annotated string property in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FileDialogAttribute : PropertyAttribute
    {
        /// <summary>
        /// Whether the shown dialog is a save file dialog or an open file dialog.
        /// </summary>
        public bool IsSave { get; set; } = false;

        /// <summary>
        /// The file extensions to filter in the open file dialog without preceding period character. Separate multiple
        /// extensions with comma. Defaults to "exe".
        /// </summary>
        public string FileExtension { get; set; } = "exe";
        
        /// <summary>
        /// The default value.
        /// </summary>
        public string DefaultValue { get; set; }
        
        /// <summary>
        /// The dialog window title.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// The label text in the inspector.
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// The placeholder text to display in the "Save As" text field when IsSaveFileDialog is true.
        /// This is the name of the file to be saved.
        /// </summary>
        public string DefaultName { get; set; } = "";
        
        /// <summary>
        /// Whether the path is relative to the project folder or an absolute path instead.
        /// </summary>
        public bool RelativeToProject { get; set; } = true;
    }
}
