using KanbanApp.Domain.Entities;

namespace KanbanApp.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(ApplicationUser user);
}