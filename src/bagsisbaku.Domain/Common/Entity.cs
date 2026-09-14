namespace bagsisbaku.Domain.Common;

public abstract class Entity
{
    protected Entity()
    {
    }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Entity Id boş ola bilməz.");
        }

        Id = id;
    }

    public Guid Id { get; private set; }
}
