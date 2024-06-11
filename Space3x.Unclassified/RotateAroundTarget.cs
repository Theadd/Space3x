using Space3x.Attributes.Types;
using UnityEngine;

namespace Space3x.Unclassified
{
    public class RotateAroundTarget : MonoBehaviour
    {
        [Inline]
        public GameObject Target;
        
        public int RotationSpeed = 20;

        private void Update()
        {
            // Spin the object around the target at 20 degrees/second.
            transform.RotateAround(Target.transform.position, Vector3.up, RotationSpeed * Time.deltaTime);
        }
    }
}
