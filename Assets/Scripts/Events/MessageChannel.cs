using UnityEngine;

// Short messages for the player ("Project completed!"). The game raises them, the HUD shows them.
[CreateAssetMenu(menuName = "Next Interview/Events/Message Channel")]
public class MessageChannel : EventChannel<string>
{
}
