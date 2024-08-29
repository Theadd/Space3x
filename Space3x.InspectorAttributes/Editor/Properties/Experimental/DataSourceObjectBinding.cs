using System;
using System.Collections;
using System.Reflection;
using Space3x.Properties.Types;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    public class DataSourceObjectBinding : BindableDataSource<UnityEngine.Object>
    {
        public DataSourceObjectBinding(IPropertyNode property) : base(property) { }

        // public DataSourceObjectBinding(IPropertyNode property, int index) : base(property, index) { }
    }
}
