public interface ICardDropZone
{
    public void OnEnter(Card card);
    public void OnExit(Card card);
    public void OnDrop(Card card);
}