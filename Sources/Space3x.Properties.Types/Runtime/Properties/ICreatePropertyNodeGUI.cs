using UnityEngine.UIElements;

namespace Space3x.Properties.Types
{
    public interface ICreatePropertyNodeGUI
    {
        VisualElement CreatePropertyNodeGUI(IPropertyNode property);
    }
}
