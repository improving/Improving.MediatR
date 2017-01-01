namespace Improving.MediatR
{
    public abstract class Key
    {
        protected Key()
        {
        }

        protected Key(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public int? OrderBy { get; set; }

        public static Key<TId> For<TId>(TId id, string displayName = "", int? orderBy = null)
        {
            return new Key<TId>(id, displayName)
            {
                OrderBy = orderBy
            };
        }
    }

    public class Key<TId> : Key
    {
        public Key()
        {

        }

        public Key(TId id)
        {
            Id = id;
        }

        public Key(TId id, string name) : this(id)
        {
            Name = name;
        }

        public TId Id { get; set; }

        public override string ToString()
        {
            return $"{Id} <{Name}>";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var otherKey = obj as Key<TId>;
            return ((object)otherKey) != null
                   && Equals(Id, otherKey.Id)
                   && OrderBy == otherKey.OrderBy
                   && Equals(Name, otherKey.Name);
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Key<TId> key1, Key<TId> key2)
        {
            return Equals(key1, key2);
        }

        public static bool operator !=(Key<TId> key1, Key<TId> key2)
        {
            return !(key1 == key2);
        }
    }
}
