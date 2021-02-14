namespace AiurVersionControl.Models
{
    public interface IModification<T> where T: WorkSpace
    {
        void Apply(T workspace);
    }
}
