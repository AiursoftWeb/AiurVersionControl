using System.Collections.Generic;

namespace AiurVersionControl.LSEQ.LogootEngine
{
    public interface ILogootEngine
    {
        /** integrate remote or local changes to the system **/
        void Deliver(Patch patch);

        /** generate patch from local changes **/
        Patch GeneratePatch(List<Delta> deltas); // PASUR
    }
}