using System;
using System.Collections.Generic;
using Space3x.Attributes.Types;
using Unity.Properties;
using UnityEngine;

namespace Space3x.FuzzyLogic
{
    [CreateAssetMenu(fileName = "FollowTargetTogether", menuName = "Follow Target Together", order = 0)]
    public class FollowTargetTogetherSO : ScriptableObject
    {
        public TextAsset FuzzyLogicData = null;

        // [field: NonSerialized]
//        [ShowInInspector]
        public Transform Target { get; set; }

//        [ShowInInspector]
        public float SampleRate { get; set; } = 11f;

        [Header("Header TEXT!")]
        
        [NonSerialized]
//        [ShowInInspector]
        public float SampleRateV2 = 12f;

        [field: System.NonSerialized]
        [CreateProperty]
        public float SampleRateV3 { get; set; } = 13f;

        [NonSerialized]
//        [ShowInInspector]
        private float SampleRateV4 = 14f;
        
//        [ShowInInspector]
        public float SampleAlwaysShown = 14f;

        public string Description;

        public List<Transform> Followers { get; set; } = new List<Transform>();

        [field: NonSerialized]
        public FuzzyLogic Controller { get; set; } = null;
    }
}
