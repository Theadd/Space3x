#if UNITY_EDITOR
using Space3x.Attributes.Types;
using UnityEditor;
using UnityEngine;

namespace Space3x.Unclassified
{
//    [Icon("Assets/Scripts/Core/Utilities/Editor/Resources/RotateDirectionRestrain")]
    [ExecuteInEditMode]
    public class AlignTransformToSceneViewCamera : MonoBehaviour
    {
        [Button(nameof(Unlink))]
        public Transform targetTransform;
        public bool reverseAlignTargets = false;
        public bool restoreTargetTransformWhenDone = true;
        public bool restoreUsingLocalSpaceCoords = false;

        private Vector3 _initialPos;
        private Quaternion _initialRotation;
        private int _lastTargetId = 0;
        private bool _isTargetEmpty = true;
        private Transform _t;

        private void OnValidate() => Validate();

        private void OnDisable() => Unlink();

        public void Unlink()
        {
            targetTransform = null;
            Validate();
        }

        private void Validate()
        {
            _isTargetEmpty = (targetTransform == null);
            if ((_isTargetEmpty ? 0 : targetTransform.GetInstanceID()) != _lastTargetId)
            {
                // Existing Transform changed, restore initial values if enabled
                if (restoreTargetTransformWhenDone && _lastTargetId != 0)
                    RestoreTransformToInitialValues(_t);

                if (!_isTargetEmpty)
                    StoreInitialTransformValues(targetTransform);

                _lastTargetId = _isTargetEmpty ? 0 : targetTransform.GetInstanceID();
                _t = targetTransform;
            }
        }

        private void RestoreTransformToInitialValues(Transform t)
        {
            if (restoreUsingLocalSpaceCoords)
            {
                t.localPosition = _initialPos;
                t.localRotation = _initialRotation;
            }
            else
            {
                t.position = _initialPos;
                t.rotation = _initialRotation;
            }
        }
        
        private void StoreInitialTransformValues(Transform t)
        {
            _initialPos = restoreUsingLocalSpaceCoords ? t.localPosition : t.position;
            _initialRotation = restoreUsingLocalSpaceCoords ? t.localRotation : t.rotation;
        }

        private void LateUpdate()
        {
            if (_isTargetEmpty) return;
            var t = SceneView.lastActiveSceneView.camera.transform;
            
            if (reverseAlignTargets)
            {
                t.position = targetTransform.position;
                t.rotation = targetTransform.rotation;
            }
            else
            {
                targetTransform.position = t.position;
                targetTransform.rotation = t.rotation;
            }
        }
    }
}
#endif
