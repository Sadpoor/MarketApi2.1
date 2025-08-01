﻿using MarketApi.DTOs.DiscountCode;
using MarketApi.Models.DiscountCode;
using Riok.Mapperly.Abstractions;


namespace MarketApi.Mappers;
[Mapper]

public partial class DiscoutCodeMapper
{
    public partial DiscountCode ToEntity(AddDiscountCodeDto dto);
    public partial void UpdateEntity(AddDiscountCodeDto dto, DiscountCode entity);

}

