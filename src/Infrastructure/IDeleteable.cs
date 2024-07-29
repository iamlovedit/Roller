namespace Roller.Infrastructure;

public interface IDeleteable
{
    bool IsDeleted { get; set; }
}