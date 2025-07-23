using MarketApi.models;
using MarketApi.DTOs.Product;
using Riok.Mapperly.Abstractions;
using MarketApi.DTOs.User;

namespace MarketApi.Mappers;
[Mapper]

    public partial class ProductMapper
    {
    public partial Product ToEntity(AddProductDto dto);
    public partial void UpdateEntity(UpdateProductDto dto, Product entity);
    public partial AddProductDto ToDto(Product product);

}

