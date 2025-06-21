public interface IProgressInteractable : IInteractable
{
    float progressSpeed { get; set; }
    float progress { get; set; }
    bool progressCompleted { get; set; }
    float GetProgress();
}