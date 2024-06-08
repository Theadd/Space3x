using System.Collections.Generic;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    public class RuntimeTypeProperties
    {
        private PropertyAttributeController Controller { get; set; }

        public List<IProperty> Values { get; set; }

        public RuntimeTypeProperties(PropertyAttributeController controller)
        {
            Controller = controller;
            Values = new List<IProperty>();
            Bind();
        }

        private void Bind()
        {
            for (var i = 0; i < Controller.AnnotatedType.Values.Count; i++)
            {
                var entry = Controller.AnnotatedType.Values[i];
                if (entry.IsSerializable)
                {
                    Values.Add(new SerializedPropertyNode()
                    {
                        Name = entry.Name,
                        ParentPath = Controller.ParentPath,
                        Flags = entry.Flags
                    });
                }
                else
                {
                    if (entry.IsHidden)
                    {
                        // Values.Add(null);
                    }
                    else
                    {
                        Values.Add(new NonSerializedPropertyNode()
                        {
                            Name = entry.Name,
                            ParentPath = Controller.ParentPath,
                            Flags = entry.Flags
                        });
                    }
                }
            }
        }
    }
}