﻿using System;
using Space3x.Attributes.Types;
using UnityEngine;
using UnityEditor;

namespace Space3x.InspectorAttributes.Editor
{
    public interface IAttributeExtensionContext
    {
        IProperty Property { get; }
    }

    public interface IAttributeExtensionContext<TAttribute> : IExtensionContext, IAttributeExtensionContext
        where TAttribute : Attribute
    {
        TAttribute Target { get; }

        IExtensionContext 
            IExtensionContext.WithExtension<TExtension, TExtensionType, TValue>(
                out TValue outValue, 
                TValue defaultValue)
        {
            if (Target is ISealedExtension<TExtensionType> extension)
            {
                extension
                    .GetExtension<TExtension>()
                    .TryApply<TValue, TExtensionType>(this, Target as TExtensionType, out outValue, defaultValue);
            }
            else
            {
                Debug.Log("Extension not found");
                outValue = defaultValue;
            }

            return this;
        }

        IExtensionContext 
            IExtensionContext.WithExtension<TExtension, TExtensionType, TTarget>(TTarget target, out bool success)
        {
            if (Target is ISealedExtension<TExtensionType> extension)
                success = extension
                    .GetExtension<TExtension>()
                    .TryApply<TTarget, TExtensionType>(this, Target as TExtensionType, target);
            else
                success = false;

            return this;
        }
    }
}