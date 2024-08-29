using System;
using System.Collections;
using System.Reflection;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public class DataSourceBinding : BindableDataSource<object>
    {
        public DataSourceBinding(IPropertyNode property) : base(property) { }

        // public DataSourceBinding(IPropertyNode property, int index) : base(property, index) { }
    }
}
