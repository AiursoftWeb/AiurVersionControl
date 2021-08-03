using AiurVersionControl.CRUD;
using AiurVersionControl.LSEQ.LogootEngine;
using AiurVersionControl.Models;
using NetDiff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurVersionControl.LSEQ.Modifications
{
    class DocCommit : IModification<CollectionWorkSpace<string>>
    {

        public DocCommit() { }

        public DocCommit(DiffResult<Chunk<string>>[] sourceDiff)
        {
            
        }

        public void Apply(CollectionWorkSpace<string> workspace)
        {
            throw new NotImplementedException();
        }
    }
}
