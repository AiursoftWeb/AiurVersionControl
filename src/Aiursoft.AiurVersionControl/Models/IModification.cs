namespace Aiursoft.AiurVersionControl.Models
{
    /// <summary>
    /// An operation which can be applied to workspace.
    /// </summary>
    /// <typeparam name="T">Target workspace type.</typeparam>
    public interface IModification<T> where T: WorkSpace
    {
        void Apply(T workspace);
    }
}
