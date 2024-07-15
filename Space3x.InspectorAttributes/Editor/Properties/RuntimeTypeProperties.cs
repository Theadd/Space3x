using System.Collections.Generic;

namespace Space3x.InspectorAttributes.Editor
{
    public class RuntimeTypeProperties
    {
        private PropertyAttributeController Controller { get; set; }

        public List<IProperty> Values { get; set; }
        
        public List<string> Keys { get; }

        public RuntimeTypeProperties(PropertyAttributeController controller)
        {
            Controller = controller;
            Values = new List<IProperty>();
            Keys = new List<string>();
            Bind();
        }

        public IProperty GetValue(string key)
        {
            var i = Keys.IndexOf(key);
            return i >= 0 ? Values[i] : null;
        }

        private void Bind()
        {
            for (var i = 0; i < Controller.AnnotatedType.Values.Count; i++)
            {
                var entry = Controller.AnnotatedType.Values[i];
                var isNodeTree = (entry.FieldType.IsClass || entry.FieldType.IsInterface) && entry.FieldType != typeof(string);
                if (entry.IsSerializable)
                {
                    if (isNodeTree)
                        Values.Add(new SerializedPropertyNodeTree()
                        {
                            Name = entry.Name,
                            ParentPath = Controller.ParentPath,
                            Flags = entry.Flags,
                            SerializedObject = Controller.SerializedObject
                        });
                    else
                        Values.Add(new SerializedPropertyNode()
                        {
                            Name = entry.Name,
                            ParentPath = Controller.ParentPath,
                            Flags = entry.Flags,
                            SerializedObject = Controller.SerializedObject
                        });
                    Keys.Add(entry.Name);
                }
                else
                {
                    if (entry.IsHidden)
                    {
                        // Values.Add(null);
                    }
                    else
                    {
                        if (isNodeTree)
                            Values.Add(new NonSerializedPropertyNodeTree()
                            {
                                Name = entry.Name,
                                ParentPath = Controller.ParentPath,
                                Flags = entry.Flags,
                                SerializedObject = Controller.SerializedObject
                            });
                        else
                            Values.Add(new NonSerializedPropertyNode()
                            {
                                Name = entry.Name,
                                ParentPath = Controller.ParentPath,
                                Flags = entry.Flags,
                                SerializedObject = Controller.SerializedObject
                            });
                        Keys.Add(entry.Name);
                    }
                }
            }
        }
    }
}
