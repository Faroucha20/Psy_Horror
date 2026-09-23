using UnityEngine;

public class PassthroughStarter : MonoBehaviour
{
    private OVRPassthroughLayer passthroughLayer;

    void Awake()
    {
        passthroughLayer = GetComponent<OVRPassthroughLayer>();
        passthroughLayer.hidden = false;
    }
}