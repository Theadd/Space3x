using System.Collections.Generic;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [ExcludeFromDocs]
    public abstract class FieldFactoryBase
    {
        /// <summary>
        /// Static global flag to enable by default all read-only properties whose property field elements are created
        /// by a type derived from this class.
        /// </summary>
        public static bool EnableAllReadOnly = false;

        /// <summary>
        /// Whether to enable read-only property field elements created by this instance by default.
        /// </summary>
        public bool EnableReadOnly { get; set; } = false;
        
        /// <summary>
        /// Whether a read-only property created by this instance should be enabled by default.
        /// </summary>
        protected bool IsReadOnlyEnabled => EnableAllReadOnly || EnableReadOnly;
        
        public PropertyAttributeController Controller { get; protected set; }
        
        public VisualElement Container { get; set; }
        
        protected List<BindablePropertyField> BindableFields = new List<BindablePropertyField>();
        
        public void Clear()
        {
            RemoveAllBindableFields();
            Container?.Clear();
            Container?.WithClasses(false, UssConstants.UssFactoryPopulated);
        }
        
        protected void RemoveAllBindableFields()
        {
            for (var index = BindableFields.Count - 1; index >= 0; index--)
            {
                var bindableField = BindableFields[index];
                bindableField.ProperlyRemoveFromHierarchy();
            }

            BindableFields.Clear();
        }
    }
}
