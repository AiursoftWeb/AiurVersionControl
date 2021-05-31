using AiurVersionControl.CRUD;
using AiurVersionControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurVersionControl.LSEQ.Modifications
{
    class DocCommit : IModification<CollectionWorkSpace<string>>
    {
        public void Apply(CollectionWorkSpace<string> workspace)
        {
            throw new NotImplementedException();
        }
    }
}
