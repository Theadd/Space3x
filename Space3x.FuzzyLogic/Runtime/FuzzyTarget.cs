using UnityEngine;

namespace Space3x.FuzzyLogic
{
    public class FuzzyTarget : MonoBehaviour
    {
        public FollowTargetTogetherSO FuzzyLogicData;
        
        private void OnEnable() => FuzzyLogicData.Target = transform;

        private void OnDisable() => FuzzyLogicData.Target = null;
    }
}
