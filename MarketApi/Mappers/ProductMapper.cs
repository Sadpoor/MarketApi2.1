using MarketApi.DTOs.Product;
using MarketApi.models;
using Riok.Mapperly.Abstractions;

namespace MarketApi.Mappers;
[Mapper]

public partial class ProductMapper
{
    public partial Product ToEntity(AddProductDto dto);
    public partial void UpdateEntity(UpdateProductDto dto, Product entity);
    public partial AddProductDto ToDto(Product product);

}

