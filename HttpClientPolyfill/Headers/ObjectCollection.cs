using System;
using System.Collections.ObjectModel;

namespace System.Net.Http.Headers
{
  internal class ObjectCollection<T> : Collection<T> where T : class
  {
    public ObjectCollection() : this(ObjectCollection<T>.defaultValidator)
    {
    }

    public ObjectCollection(Action<T> validator)
    {
      this.validator = validator;
    }

    protected override void InsertItem(int index, T item)
    {
      if (this.validator != null)
      {
        this.validator(item);
      }
      base.InsertItem(index, item);
    }

    protected override void SetItem(int index, T item)
    {
      if (this.validator != null)
      {
        this.validator(item);
      }
      base.SetItem(index, item);
    }

    private static void CheckNotNull(T item)
    {
      if (item == null)
      {
        throw new ArgumentNullException("item");
      }
    }

    private static readonly Action<T> defaultValidator = new Action<T>(ObjectCollection<T>.CheckNotNull);

    private Action<T> validator;
  }
}
