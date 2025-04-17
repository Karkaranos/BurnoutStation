using UnityEngine;
using FMODUnity;

public class FMODEventsManager : MonoBehaviour
{

    [field: Header("ServerMusic")]
    [field: SerializeField] public EventReference BGM { get; private set; }

    [field:Header("ServerSFX")]
    [field:SerializeField] public EventReference[] TimerWarnings { get; private set; }
    [field:SerializeField] public EventReference PoliceSirens { get; private set; }
    [field: SerializeField] public EventReference[] EndingLines { get; private set; }
    [field: SerializeField] public EventReference[] ApprovalLines { get; private set; }
    [field: SerializeField] public EventReference[] CensorshipLines { get; private set; }
    [field: SerializeField] public EventReference GraffitiDisplay { get; private set; }
    [field: SerializeField] public EventReference GameStart { get; private set; }



    [field: Header("ClientSFX")]
    [field: SerializeField] public EventReference Spraypaint { get; private set; }
    [field: SerializeField] public EventReference SwitchCans { get; private set; }
    [field: SerializeField] public EventReference ButtonClick { get; private set; }

    public static FMODEventsManager instance { get; private set; }


    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("There is more than one FMODEventsManager in the scene");
            Destroy(instance);
        }
        instance = this;
    }
}
