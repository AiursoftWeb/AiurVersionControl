using NetDiff;
using System.Collections.Generic;

namespace AiurVersionControl.LSEQ.LogootEngine
{
    public interface ILogootEngine
    {
        /** integrate remote or local changes to the system **/
        void Deliver(MyPatch patch);

        /** generate patch from local changes **/
        MyPatch GeneratePatch(DiffResult<Chunk<string>>[] deltas); // PASUR
    }
}