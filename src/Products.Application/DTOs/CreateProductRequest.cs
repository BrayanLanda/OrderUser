using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Products.Application.DTOs
{
    public record CreateProductRequest
    (
        string Name,
        string Description,
        decimal Price,
        int Stock
    );
}