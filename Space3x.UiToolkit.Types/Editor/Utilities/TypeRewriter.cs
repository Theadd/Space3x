using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Space3x.UiToolkit.Types
{
    public class TypeRewriter
    {
        public interface IStyles
        {
            StyleTag Primary { get; }
            StyleTag Secondary { get; }
            StyleTag Highlight { get; }
        }
        
        public class Styles : IStyles
        {
            public StyleTag Primary { get; set; }
            public StyleTag Secondary { get; set; }
            public StyleTag Highlight { get; set; }

            public Styles() { }
            
            public Styles(IStyles other)
            {
                Primary = other.Primary;
                Secondary = other.Secondary;
                Highlight = other.Highlight;
            }

            public Styles(in StyleTag primary, in StyleTag secondary, in StyleTag highlight)
            {
                Primary = primary;
                Secondary = secondary;
                Highlight = highlight;
            }
        }

        public static readonly Styles DefaultStyle = new Styles
        {
            Primary = StyleTag.Primary,
            Secondary = StyleTag.Secondary,
            Highlight = StyleTag.Highlight
        };

        private IStyles m_Style = DefaultStyle;
        public IStyles DeclarationStyle { get; set; } = new Styles(in StyleTag.Primary, in StyleTag.Highlight, in StyleTag.NoStyle); // e.g.: _public class_ *MyClass*<*TValue*>
        public IStyles MainTypeStyle { get; set; } = new Styles(in StyleTag.Highlight, in StyleTag.Alternative, in StyleTag.NoStyle); // e.g.: _public class_ *MyClass*<*TValue*>
        public IStyles BaseTypeStyle { get; set; } = new Styles(in StyleTag.Secondary, StyleTag.Alternative, StyleTag.Highlight);
        public IStyles InterfaceStyle { get; set; } = new Styles(StyleTag.Alternative, StyleTag.Alternative, StyleTag.Highlight);
        public IStyles InheritedInterfaceStyle { get; set; } = new Styles(StyleTag.Grey, StyleTag.Grey, StyleTag.Grey);
        public IStyles TypeDescriptionStyle { get; set; } = new Styles(StyleTag.NoStyle, StyleTag.Grey, StyleTag.Light);
        public static IStyles NoStyle { get; } = new Styles(StyleTag.NoStyle, StyleTag.NoStyle, StyleTag.NoStyle);

        private string Styling(IStyles style, string value = "")
        {
            m_Style = style;
            return value;
        }

        public Type Target { get; set; }
        
        private static readonly string[] NoStrings = Array.Empty<string>();
        
        public TypeRewriter SetTarget(Type type)
        {
            Target = type;
            return this;
        }

        public string GetFormattedType() =>
            DeclarationStyle.Primary.Wrap($"{Visibility}{Declaration}") + Stylized(Target, MainTypeStyle) +
            Prefix(" : ",
                NJoin(", ", new[]
                {
                    Stylized(Target.BaseType, BaseTypeStyle),
                    NJoin(", ", new[]
                    {
                        Styling(InterfaceStyle) + NJoin(", ", Interfaces.Select(Stylized)),
                        Styling(InheritedInterfaceStyle) + NJoin(", ", InheritedInterfaces.Select(Stylized))
                    })
                })
            );

        private static string[] GenericArguments(Type type, IStyles style) => (type?.IsGenericType ?? false) 
            ? type.GetGenericArguments().Select(x => x.IsGenericType 
                ? AsDisplayName(x, new Styles(style) { Primary = style.Secondary }) 
                : style.Secondary.Wrap(x.Name)).ToArray() 
            : NoStrings;

        private string Stylized(Type type) => AsDisplayName(type, m_Style);
        
        private string Stylized(Type type, IStyles style) => AsDisplayName(type, style);
        
        public static string AsDisplayName(Type type, IStyles style) =>
            !IsType(type) 
                ? string.Empty
                : Prefix("", 
                    style.Primary.Wrap(type.SimpleName()),
                    Prefix(style.Highlight.Wrap("<"), 
                        NJoin(", ", GenericArguments(type, style)), 
                        style.Highlight.Wrap(">")));

        public Type[] Interfaces => !IsType(Target.BaseType)
            ? Target.GetInterfaces() 
            : Target.GetInterfaces().Except(Target.BaseType!.GetInterfaces()).ToArray();

        public Type[] InheritedInterfaces => !IsType(Target.BaseType)
            ? Type.EmptyTypes
            : Target.BaseType!.GetInterfaces();
        
        public static string Prefix(string prefix, string value, string postfix = null) => string.IsNullOrEmpty(value) ? string.Empty : $"{prefix}{value}{postfix ?? string.Empty}";

        public static string NJoin(string separator, IEnumerable<string> values) => string.Join(separator, values.Where(s => !string.IsNullOrEmpty(s)));
        
        private static bool IsType(Type type) => type != null && type != typeof(object);
        
        public static string Description(Type type) => 
            type != null && Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) is DescriptionAttribute descAttribute 
                ? descAttribute.Description 
                : (type?.TryGetSummary(out var summary) ?? false) ? summary : null;
        
        public string Visibility =>
            (Target.Attributes & TypeAttributes.VisibilityMask) switch 
            {
                TypeAttributes.Public => "public ",
                TypeAttributes.NestedPublic => "public ",
                TypeAttributes.NestedPrivate => "private ",
                TypeAttributes.NestedFamily => "protected ",
                TypeAttributes.NestedAssembly => "internal ",
                TypeAttributes.NestedFamORAssem => "protected internal ",
                _ => "internal "
            };
        
        public string Declaration =>
            (Target.Attributes & (TypeAttributes.Abstract | TypeAttributes.Sealed)) switch
            {
                TypeAttributes.Abstract => "abstract ",
                TypeAttributes.Sealed => "sealed ",
                TypeAttributes.Abstract | TypeAttributes.Sealed => "static ",
                _ => ""
            } 
            +
            (Target.Attributes & TypeAttributes.ClassSemanticsMask) switch 
            {
                TypeAttributes.Interface => "interface ",
                _ => Target.IsValueType ? "struct " : "class "
            };
    }
}
