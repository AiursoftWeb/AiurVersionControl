using System;
using System.Linq;

namespace AiurStore
{
    public abstract class InOutDatabase<T>
    {
        public abstract IQueryable<T> Query();
        public abstract void Insert(T newObject);
    }

    public class FileInOutDatabase<T> : InOutDatabase<T>
    {
        public override void Insert(T newObject)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<T> Query()
        {
            throw new NotImplementedException();
        }
    }
}
