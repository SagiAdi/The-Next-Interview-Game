using UnityEngine;

// A person in the city. Talking to them creates a connection that lasts for a while
// and gives a bonus in the interview.
public class ContactNPC : Interactable
{
    [SerializeField] private MessageChannel messages;

    [SerializeField]
    private string[] tips =
    {
        "Send me your best project. I can introduce you to a hiring manager.",
        "Explain your work clearly and people will remember you.",
        "I know a team that is hiring right now. Keep building projects!",
    };

    public override string Prompt => "Talk";

    public override void Interact()
    {
        GameSession.Instance.AddConnection();
        messages.Raise("New connection! " + tips[Random.Range(0, tips.Length)]);
        Destroy(gameObject);
    }
}
