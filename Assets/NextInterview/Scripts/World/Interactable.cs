using UnityEngine;

// Base class for things the player can use. The player stands close and presses E
// (or the Interact button). See PlayerController2D.FindNearest.
[RequireComponent(typeof(Collider2D))]
public abstract class Interactable : MonoBehaviour
{
    // Text shown in the on-screen hint when the player is close enough.
    public virtual string Prompt => "Interact";

    public abstract void Interact(PlayerController2D player);
}
