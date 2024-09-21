using System;
using Space3x.Attributes.Types;
using UnityEngine;

[CreateAssetMenu(fileName = "SampleView", menuName = "UI Views/Sample View")]
public class SampleView : ScriptableObject
{
    [AllowExtendedAttributes]
    public bool sampleBool = true;
    [NonSerialized]
    [ShowInInspector]
    public AnnotatedColor sampleColor = (AnnotatedColor)new Color32(43, 61, 79, 255);
}
