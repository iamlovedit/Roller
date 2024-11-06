namespace Roller.Infrastructure.Domain;

public interface IDateAbility
{
    DateTime CreatedDate { get; set; }

    DateTime? UpdatedDate { get; set; }
}