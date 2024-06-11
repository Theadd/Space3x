using System;
using Space3x.Attributes.Types;
using UnityEngine;

namespace Space3x.FuzzyLogic
{
    public class FollowTogether : MonoBehaviour
    {
        [Inline]
        public FollowTargetTogetherSO FuzzyLogicData;
        
        public float SpeedMultiplier = 1f;
        
        public float DistanceToTargetMultiplier = 1f;
        
        public float DistanceToFollowersMultiplier = 1f;
        
        private void OnEnable() => FuzzyLogicData?.Followers.Add(transform);

        private void OnDisable() => FuzzyLogicData?.Followers.Remove(transform);

        private void Start()
        {
            if (FuzzyLogicData?.Controller == null)
                FuzzyLogicData.Controller = FuzzyLogic.Deserialize(FuzzyLogicData.FuzzyLogicData.bytes, null);
        }

        private void Update()
        {
            FuzzyLogic controller = FuzzyLogicData.Controller;
            if (controller == null)
                return;

            controller.evaluate = true;

            controller.GetFuzzificationByName("distance").value = Vector3.Distance(FuzzyLogicData.Target.position, transform.position);
            float speed = controller.Output() * controller.defuzzification.maxValue;
            var nextPosition = Vector3.MoveTowards(
                transform.position, 
                FuzzyLogicData.Target.position, 
                speed * Time.deltaTime * SpeedMultiplier);

            foreach (var follower in FuzzyLogicData.Followers)
            {
                if (follower.Equals(transform)) 
                    break;
                
                controller.GetFuzzificationByName("distance").value = Vector3.Distance(follower.position, transform.position);
                speed = controller.Output() * controller.defuzzification.maxValue;
                nextPosition = Vector3.MoveTowards(
                    nextPosition, 
                    follower.position, 
                    speed * Time.deltaTime * SpeedMultiplier);
            }

            transform.position = nextPosition;
//            fuzzyLogic.evaluate = true;
//            fuzzyLogic.GetFuzzificationByName("distance").value = Vector3.Distance(target.position, source.position);
//
//            float speed = fuzzyLogic.Output() * fuzzyLogic.defuzzification.maxValue;
//            source.position = Vector3.MoveTowards(source.position, target.position, speed * Time.deltaTime);
        }
    }
}
