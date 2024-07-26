using Space3x.Attributes.Types.DeveloperNotes;

namespace Space3x.Attributes.Types
{
    public interface IContract { }
    
    public interface ISealedExtension { }

    public interface ISealedExtension<T> : ISealedExtension
    {
        public Extension<T> GetExtension<TExtensionHandler>() => 
            (Extension<T>) GenericExtension.GetExtension<T>(typeof(T), typeof(TExtensionHandler));
    }
    
    [ViolatesYAGNI]
    public interface IExtensionContext
    {
        public IExtensionContext
            WithExtension<TExtension, TExtensionType, TValue>(
                out TValue outValue,
                TValue defaultValue = default(TValue)
            )
            where TExtension : GenericExtension 
            where TExtensionType : class;
        
        public IExtensionContext
            WithExtension<TExtension, TExtensionType, TTarget>(
                TTarget target
            )
            where TExtension : GenericExtension 
            where TExtensionType : class
            where TTarget : class => WithExtension<TExtension, TExtensionType, TTarget>(target, out _);
        
        public IExtensionContext
            WithExtension<TExtension, TExtensionType, TTarget>(
                TTarget target,
                out bool success
            )
            where TExtension : GenericExtension 
            where TExtensionType : class
            where TTarget : class;
    }
}
