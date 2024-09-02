using System;

namespace Space3x.Properties.Types
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomRuntimeDrawer : Attribute
    {
        /// <summary>
        /// An array of <see cref="Type"/> values to be handled by the drawer.
        /// </summary>
        public Type[] Types { get; set; }
        
        /// <summary>
        /// UseForChildren has not been implemented yet for runtime drawers. In the meantime, you can specify them all.
        /// </summary>
        public bool UseForChildren { get; set; }
        
        public CustomRuntimeDrawer(params Type[] types) => Types = types;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="useForChildren">UseForChildren has not been implemented yet for runtime drawers. In the meantime, you can specify them all.</param>
        public CustomRuntimeDrawer(Type type, bool useForChildren = false)
        {
            Types = new[] { type };
            UseForChildren = useForChildren;
        }
    }
}
