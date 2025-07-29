using MarketApi.DTOs.User;
using MarketApi.Models.Users;
using Riok.Mapperly.Abstractions;

namespace MarketApi.Mappers;
[Mapper]
public partial class UserMapper
{
    public partial User ToEntity(AddUserDto dto);
    public partial void UpdateEntity(UpdateUserDto dto, User entity);
    public partial AddUserDto ToDto(User user);
}

