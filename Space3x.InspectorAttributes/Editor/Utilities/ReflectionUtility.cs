/* Author: v0lt13 - https://github.com/v0lt13
 * Source: https://github.com/v0lt13/EditorAttributes
 * License: The Unlicense - https://github.com/v0lt13/EditorAttributes/blob/main/LICENSE
 */
using System;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections;
using Space3x.InspectorAttributes.Editor.Extensions;

namespace Space3x.InspectorAttributes.Editor.Utilities
{
	public static class ReflectionUtility
    {
		public const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		public static FieldInfo FindField(string fieldName, SerializedProperty property, ref object targetObject)
		{
			var fieldInfo = FindField(fieldName, targetObject);

			// If the field null we try to see if its inside a serialized object
			if (fieldInfo == null)
			{
				var serializedObjectType = GetSerializedObjectFieldType(property, out object properTarget);

				if (serializedObjectType != null)
				{
					fieldInfo = serializedObjectType.GetField(fieldName, BINDING_FLAGS);
					targetObject = properTarget;
				}
			}

			return fieldInfo;
		}

		internal static FieldInfo FindField(string fieldName, object targetObject) => FindMember(fieldName, targetObject.GetType(), BINDING_FLAGS, MemberTypes.Field) as FieldInfo;

		public static PropertyInfo FindProperty(string propertyName, SerializedProperty property, ref object targetObject)
		{
			var propertyInfo = FindProperty(propertyName, targetObject);

			// If the property null we try to see if its inside a serialized object
			if (propertyInfo == null)
			{
				var serializedObjectType = GetSerializedObjectFieldType(property, out object properTarget);

				if (serializedObjectType != null)
				{
					propertyInfo = serializedObjectType.GetProperty(propertyName, BINDING_FLAGS);
					targetObject = properTarget;
				}
			}

			return propertyInfo;
		}

		internal static PropertyInfo FindProperty(string propertyName, object targetObject) => FindMember(propertyName, targetObject.GetType(), BINDING_FLAGS, MemberTypes.Property) as PropertyInfo;
		
		public static MethodInfo FindFunction(string functionName, SerializedProperty property, ref object targetObject)
		{
			MethodInfo methodInfo = FindFunction(functionName, targetObject);
			
			// If the method is null we try to see if it's inside a serialized object
			if (methodInfo == null)
			{
				var serializedObjectType = GetSerializedObjectFieldType(property, out object properTarget);

				try
				{
					methodInfo = serializedObjectType.GetMethod(functionName, BINDING_FLAGS);
				}
				catch (AmbiguousMatchException)
				{
					var functions = serializedObjectType.GetMethods();

					foreach (var function in functions)
					{
						if (function.Name == functionName)
						{
							methodInfo = function;
							targetObject = properTarget;
							break;
						}
					}
				}
			}

			return methodInfo;
		}

		internal static MethodInfo FindFunction(string functionName, object targetObject)
		{
			try
			{
				return FindMember(functionName, targetObject.GetType(), BINDING_FLAGS, MemberTypes.Method) as MethodInfo;
			}
			catch (AmbiguousMatchException)
			{
				var functions = targetObject.GetType().GetMethods();

				foreach (var function in functions)
				{
					if (function.Name == functionName) return function;
				}

				return null;
			}
		}

		public static MemberInfo FindMember(string memberName, Type targetType, BindingFlags bindingFlags, MemberTypes memberType)
		{
			switch (memberType)
			{
				case MemberTypes.Field:

					FieldInfo fieldInfo = null;

					while (targetType != null && !TryGetField(memberName, targetType, bindingFlags, out fieldInfo)) 
						targetType = targetType.BaseType;

					return fieldInfo;

				case MemberTypes.Property:

					PropertyInfo propertyInfo = null;

					while (targetType != null && !TryGetProperty(memberName, targetType, bindingFlags, out propertyInfo)) 
						targetType = targetType.BaseType;

					return propertyInfo;

				case MemberTypes.Method:

					MethodInfo methodInfo = null;

					while (targetType != null && !TryGetMethod(memberName, targetType, bindingFlags, out methodInfo)) 
						targetType = targetType.BaseType;

					return methodInfo;
			}

			return null;
		}

		public static bool TryGetField(string name, Type type, BindingFlags bindingFlags, out FieldInfo fieldInfo)
		{
			fieldInfo = type.GetField(name, bindingFlags);

			return fieldInfo != null;
		}

		public static bool TryGetProperty(string name, Type type, BindingFlags bindingFlags, out PropertyInfo propertyInfo)
		{
			propertyInfo = type.GetProperty(name, bindingFlags);

			return propertyInfo != null;
		}

		public static bool TryGetMethod(string name, Type type, BindingFlags bindingFlags, out MethodInfo methodInfo)
		{
			methodInfo = type.GetMethod(name, bindingFlags);

			return methodInfo != null;
		}

		public static bool IsPropertyCollection(SerializedProperty property)
		{
			var arrayField = FindField(property.propertyPath.Split(".")[0], property);
			var memberInfoType = GetMemberInfoType(arrayField);

			return memberInfoType.IsArray || memberInfoType.GetInterfaces().Contains(typeof(IList));
		}

		public static MemberInfo GetValidMemberInfo(string memberName, SerializedProperty serializedProperty, out object targetObj)
		{
			// var initialTarget = (object)serializedProperty.serializedObject.targetObject;
			// var targetObject = (object)serializedProperty.serializedObject.targetObject;
			var initialTarget = (object)serializedProperty.GetDeclaringObject();
			var targetObject = (object)serializedProperty.GetDeclaringObject();

			MemberInfo memberInfo = FindField(memberName, serializedProperty, ref targetObject);
			
			if (memberInfo == null)
			{
				targetObject = initialTarget;
				memberInfo = FindProperty(memberName, serializedProperty, ref targetObject);
				
				if (memberInfo == null)
				{
					targetObject = initialTarget;
					memberInfo = FindFunction(memberName, serializedProperty, ref targetObject);
				}
			}
			targetObj = targetObject;
			return memberInfo;
		}

		internal static MemberInfo GetValidMemberInfo(string memberName, object targetObject) // Internal function used for the button drawer
		{
			MemberInfo memberInfo;

			memberInfo = FindField(memberName, targetObject);

			memberInfo ??= FindProperty(memberName, targetObject);
			memberInfo ??= FindFunction(memberName, targetObject);

			return memberInfo;
		}

		public static Type GetSerializedObjectFieldType(SerializedProperty property, out object serializedObject)
		{
			var targetObject = property.serializedObject.targetObject;
			var pathComponents = property.propertyPath.Split('.'); // Split the property path to get individual components
			var targetObjectType = targetObject.GetType();

			var serializedObjectField = FindMember(pathComponents[0], targetObjectType, BINDING_FLAGS, MemberTypes.Field) as FieldInfo;

			serializedObject = serializedObjectField.GetValue(targetObject);

			return serializedObject?.GetType();
		}

		public static Type GetMemberInfoType(MemberInfo memberInfo) 
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				return fieldInfo.FieldType;
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				return propertyInfo.PropertyType;
			}
			else if (memberInfo is MethodInfo methodInfo)
			{
				return methodInfo.ReturnType;
			}

			return null;
		}

		public static object GetMemberInfoValue(MemberInfo memberInfo, SerializedProperty property, object[] methodParameters = null)
		{
			var targetObject = property.serializedObject.targetObject;

			try
			{
				switch (memberInfo)
				{
					case FieldInfo fieldInfo:
						return fieldInfo.GetValue(targetObject);
					case PropertyInfo propertyInfo:
						return propertyInfo.GetValue(targetObject);
					case MethodInfo methodInfo:
						return methodInfo.Invoke(targetObject, methodParameters);
				}
			}
			catch (ArgumentException) // If this expection is thrown it means that the member we try to get the value from is inside a different target
			{
				GetSerializedObjectFieldType(property, out object serializedObjectTarget);

				switch (memberInfo)
				{
					case FieldInfo fieldInfo:
						return fieldInfo.GetValue(serializedObjectTarget);
					case PropertyInfo propertyInfo:
						return propertyInfo.GetValue(serializedObjectTarget);
					case MethodInfo methodInfo:
						return methodInfo.Invoke(serializedObjectTarget, methodParameters);
				}
			}

			return null;
		}

		public static object GetMemberInfoValue(MemberInfo memberInfo, object targetObject, object[] methodParameters = null)
			=>
				memberInfo switch
				{
					FieldInfo fieldInfo => fieldInfo.GetValue(targetObject),
					PropertyInfo propertyInfo => propertyInfo.GetValue(targetObject),
					MethodInfo methodInfo => methodInfo.Invoke(targetObject, methodParameters),
					_ => null
				};
		
		public static Invokable<TIn, TOut> CreateInvokable<TIn, TOut>(string memberName, SerializedProperty property)
		{
			var mInfo = GetValidMemberInfo(memberName, property, out object targetObject);
			return mInfo == null 
				? null 
				: new Invokable<TIn, TOut>() 
				{
					CallableMember = mInfo,
					TargetObject = targetObject 
				};
		}
		
		/* ------------------------ */
		
		public static Invokable<TIn, TOut> CreateInvokable<TIn, TOut>(string memberName, IPropertyNode property)
		{
			var mInfo = GetValidMemberInfo(memberName, property, out object targetObject);
			return mInfo == null 
				? null 
				: new Invokable<TIn, TOut>() 
				{
					CallableMember = mInfo,
					TargetObject = targetObject 
				};
		}
		
		public static MemberInfo GetValidMemberInfo(string memberName, IPropertyNode property, out object targetObj)
		{
			var initialTarget = (object)property.GetDeclaringObject();
			var targetObject = (object)property.GetDeclaringObject();

			MemberInfo memberInfo = FindField(memberName, property, ref targetObject);
			
			if (memberInfo == null)
			{
				targetObject = initialTarget;
				memberInfo = FindProperty(memberName, property, ref targetObject);
				
				if (memberInfo == null)
				{
					targetObject = initialTarget;
					memberInfo = FindFunction(memberName, property, ref targetObject);
				}
			}
			targetObj = targetObject;
			return memberInfo;
		}
		
		public static FieldInfo FindField(string fieldName, IPropertyNode property, ref object targetObject)
		{
			var fieldInfo = FindField(fieldName, targetObject);

			// If the field null we try to see if its inside a serialized object
			if (fieldInfo == null)
			{
				var serializedObjectType = GetSerializedObjectFieldType(property, out object properTarget);

				if (serializedObjectType != null)
				{
					fieldInfo = serializedObjectType.GetField(fieldName, BINDING_FLAGS);
					targetObject = properTarget;
				}
			}

			return fieldInfo;
		}
		
		public static Type GetSerializedObjectFieldType(IPropertyNode property, out object serializedObject)
		{
			var targetObject = property.GetSerializedObject().targetObject;
			var pathComponents = property.PropertyPath.Split('.'); // Split the property path to get individual components
			var targetObjectType = targetObject.GetType();

			var serializedObjectField = FindMember(pathComponents[0], targetObjectType, BINDING_FLAGS, MemberTypes.Field) as FieldInfo;

			serializedObject = serializedObjectField.GetValue(targetObject);

			return serializedObject?.GetType();
		}
		
		public static PropertyInfo FindProperty(string propertyName, IPropertyNode property, ref object targetObject)
		{
			var propertyInfo = FindProperty(propertyName, targetObject);

			// If the property null we try to see if its inside a serialized object
			if (propertyInfo == null)
			{
				var serializedObjectType = GetSerializedObjectFieldType(property, out object properTarget);

				if (serializedObjectType != null)
				{
					propertyInfo = serializedObjectType.GetProperty(propertyName, BINDING_FLAGS);
					targetObject = properTarget;
				}
			}

			return propertyInfo;
		}
		
		public static MethodInfo FindFunction(string functionName, IPropertyNode property, ref object targetObject)
		{
			MethodInfo methodInfo = FindFunction(functionName, targetObject);
			
			// If the method is null we try to see if it's inside a serialized object
			if (methodInfo == null)
			{
				var serializedObjectType = GetSerializedObjectFieldType(property, out object properTarget);

				try
				{
					methodInfo = serializedObjectType.GetMethod(functionName, BINDING_FLAGS);
				}
				catch (AmbiguousMatchException)
				{
					var functions = serializedObjectType.GetMethods();

					foreach (var function in functions)
					{
						if (function.Name == functionName)
						{
							methodInfo = function;
							targetObject = properTarget;
							break;
						}
					}
				}
			}

			return methodInfo;
		}
    }
}
