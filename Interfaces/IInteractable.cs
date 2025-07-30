public interface IInteractable
{
    void InteractWith();
    void SetFocus(bool focus = false);

    string GetPrompt();
    string GetName();
    string GetDescription();
}
