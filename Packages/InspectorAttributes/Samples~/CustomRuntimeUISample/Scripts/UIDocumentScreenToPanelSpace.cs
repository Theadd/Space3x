using UnityEngine;
using UnityEngine.UIElements;

public class UIDocumentScreenToPanelSpace : MonoBehaviour
{
    private UIDocument m_Document;

    private void OnEnable()
    {
        m_Document = GetComponent<UIDocument>();
        m_Document.panelSettings.SetScreenToPanelSpaceFunction((screenPosition =>
        {
            var invalidPosition = new Vector2(float.NaN, float.NaN);
            var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(cameraRay, out hit, 100f, LayerMask.GetMask("UI")))
            {
                return invalidPosition;
            }

            var pixelUV = hit.textureCoord;
            pixelUV.y = 1 - pixelUV.y;
            pixelUV.x *= m_Document.panelSettings.targetTexture.width;
            pixelUV.y *= m_Document.panelSettings.targetTexture.height;

            return pixelUV;
        }));
    }
}
