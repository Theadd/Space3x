using System.Collections.Generic;
using Space3x.Properties.Types;
using Space3x.Properties.Types.Editor;

namespace Space3x.InspectorAttributes.Editor
{
    public class RuntimeTypeProperties
    {
        private PropertyAttributeController Controller { get; set; }

        public List<IPropertyNode> Values { get; set; }
        
        public List<string> Keys { get; }

        public RuntimeTypeProperties(PropertyAttributeController controller)
        {
            Controller = controller;
            Values = new List<IPropertyNode>();
            Keys = new List<string>();
            Bind();
        }

        public IPropertyNode GetValue(string key)
        {
            var i = Keys.IndexOf(key ?? string.Empty);
            return i >= 0 ? Values[i] : null;
        }
        
        public IPropertyNode GetNextValue(string currentKey)
        {
            var i = Keys.IndexOf(currentKey ?? string.Empty);
            return i >= 0 && i < Keys.Count - 1 ? Values[i + 1] : null;
        }

        private void AddPropertyTreeRoot()
        {
            if (Controller.SerializedObject != null)
            {
                Values.Add(new SerializedPropertyNodeTree()
                {
                    Name = string.Empty,
                    ParentPath = Controller.ParentPath,
                    Flags = VTypeFlags.Serializable,
                    Controller = Controller,
                });
            }
            else
            {
                Values.Add(new NonSerializedPropertyNodeTree()
                {
                    Name = string.Empty,
                    ParentPath = Controller.ParentPath,
                    Flags = VTypeFlags.None,
                    Controller = Controller,
                });
            }
            Keys.Add(string.Empty);
        }
        
        private void Bind()
        {
            AddPropertyTreeRoot();
            for (var i = 0; i < Controller.AnnotatedType.Values.Count; i++)
            {
                var entry = Controller.AnnotatedType.Values[i];
                var isNodeTree = (entry.FieldType.IsClass || entry.FieldType.IsInterface) && entry.FieldType != typeof(string);

                if (entry.RuntimeMethod != null)
                {
                    Values.Add(new InvokablePropertyNode()
                    {
                        Name = entry.Name,
                        ParentPath = Controller.ParentPath,
                        Flags = entry.Node.Flags,
                        Controller = Controller,
                    });
                    Keys.Add(entry.Name);
                    continue;
                }
                
                if (entry.Node.IsSerializable)
                {
                    if (isNodeTree)
                        Values.Add(new SerializedPropertyNodeTree()
                        {
                            Name = entry.Name,
                            ParentPath = Controller.ParentPath,
                            Flags = entry.Node.Flags,
                            Controller = Controller,
                        });
                    else
                        Values.Add(new SerializedPropertyNode()
                        {
                            Name = entry.Name,
                            ParentPath = Controller.ParentPath,
                            Flags = entry.Node.Flags,
                            Controller = Controller,
                        });
                    Keys.Add(entry.Name);
                }
                else
                {
                    if (!entry.Node.IsHidden || entry.Node.IncludeInInspector)
                    {
                        if (isNodeTree)
                            Values.Add(new NonSerializedPropertyNodeTree()
                            {
                                Name = entry.Name,
                                ParentPath = Controller.ParentPath,
                                Flags = entry.Node.Flags,
                                Controller = Controller,
                            });
                        else
                            Values.Add(new NonSerializedPropertyNode()
                            {
                                Name = entry.Name,
                                ParentPath = Controller.ParentPath,
                                Flags = entry.Node.Flags,
                                Controller = Controller,
                            });
                        Keys.Add(entry.Name);
                    }
                }
            }
        }
    }
}
