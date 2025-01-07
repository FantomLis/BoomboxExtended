namespace FantomLis.BoomboxExtended.Containers;

public record DelayedModalContainer(string Title, string Description, float Time, ModalOption[] Options)
{
    public string Title = Title;
    public string Description = Description;
    public float Time = Time;
    public ModalOption[] Options = Options;

    public bool isCancelled { private set; get; }
    public bool isShowed { private set; get; }

    public void Cancel() => isCancelled = true;

    public void ShowModal()
    {
        isShowed = true;
        if (isCancelled) return;
        Modal.Show(Title, Description, Options);
    }
}