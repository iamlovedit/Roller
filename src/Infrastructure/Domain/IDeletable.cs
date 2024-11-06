namespace Roller.Infrastructure.Domain;

public interface IDeletable
{
    bool IsDeleted { get; set; }
}