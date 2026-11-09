using UnityEngine;

// A person in the city. Talking to them creates a connection that lasts for a while
// and gives a bonus in the final interview.
public class ContactNPC : Interactable
{
    static readonly string[] tips =
    {
        "Send me your best project. I can introduce you to a hiring manager.",
        "Explain your work clearly and people will remember you.",
        "I know a team that is hiring right now. Keep building projects!"
    };

    public override string Prompt => "Talk";

    public override void Interact(PlayerController2D player)
    {
        GameData.Instance.AddConnection();
        HUD.Say("New connection! " + tips[Random.Range(0, tips.Length)]);
        Destroy(gameObject);
    }
}
