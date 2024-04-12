namespace Roller.Infrastructure;

public interface IDeletable
{
    bool IsDeleted { get; set; }
}