using System;

namespace Space3x.Attributes.Types.DeveloperNotes
{
    /// <summary>
    /// Used to annotate a Code Smell in code.
    /// </summary>
    public abstract class CodeSmellAttribute : Attribute {
        public string Comment { get; set; } = "";
        
        public CodeSmellAttribute() { }
        
        public CodeSmellAttribute(string comment) => Comment = comment;
    }

    /// <summary>
    /// Used to annotate a Principle Violations in code.
    /// </summary>
    public abstract class PrincipleViolationAttribute : Attribute
    {
        public string Comment { get; set; } = "";
        
        public PrincipleViolationAttribute() { }
        
        public PrincipleViolationAttribute(string comment) => Comment = comment;
    }

    /// <summary>
    /// Principle Violation: "You Ain't Gonna Need It" Principle.  
    ///  
    /// - “Always implement things when you actually need them, never when you just foresee that you may need them”.
    ///  
    /// Don't implement an abstraction which complicates the feature that you are implementing in the present,
    /// because there's a good chance you'll never actually leverage your abstraction in the future.
    /// </summary>
    public class ViolatesYAGNI : PrincipleViolationAttribute { }
    
    
    /// <summary>
    /// Principle Violation: "Single Responsibility Principle".
    /// 
    /// Any single object in object-oriented programing (OOP) should be made for one specific function.
    /// </summary>
    public class ViolatesSingleResponsibility : PrincipleViolationAttribute { }

    /// <summary>
    /// Principle Violation: "Open/Closed Principle"
    ///
    /// Code should be open for extension but closed for modification. In simpler terms, you should be able
    /// to add new functionality to existing code without altering its source.
    /// </summary>
    public class ViolatesOpenClosed : PrincipleViolationAttribute { }
    
    /// <summary>
    /// Code Smell: "Speculative Generality".
    /// 
    /// Sometimes code is created “just in case” to support anticipated future features that never get implemented.
    /// As a result, code becomes hard to understand and support.
    /// </summary>
    public class SpeculativeGenerality : CodeSmellAttribute { }
}
